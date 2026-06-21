using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.DTO.DM;
using pi_projetolivros.Hubs;
using pi_projetolivros.Models.Banco;
using pi_projetolivros_banco;
using System.Security.Claims;

namespace pi_projetolivros.Controllers.DM;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MensagensDiretasController : ControllerBase
{
    private readonly Banco _contexto;
    private readonly IHubContext<DMHub> _hubContext;

    public MensagensDiretasController(Banco contexto, IHubContext<DMHub> hubContext)
    {
        _contexto = contexto;
        _hubContext = hubContext;
    }

    // GET /api/mensagensdiretas — lista as conversas do usuário 
    [HttpGet]
    public async Task<IActionResult> ListarConversas()
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var conversas = await _contexto.Conversas
            .AsNoTracking()
            .Where(c => c.Usuario1Id == usuarioId || c.Usuario2Id == usuarioId)
            .OrderByDescending(c => c.UltimaMensagemEm ?? c.DataCriacao)
            .Select(c => new
            {
                c.Id,
                OutroUsuario = c.Usuario1Id == usuarioId
                    ? new { c.Usuario2.Id, c.Usuario2.Nome, c.Usuario2.FotoPerfil }
                    : new { c.Usuario1.Id, c.Usuario1.Nome, c.Usuario1.FotoPerfil },
                UltimaMensagem = c.Mensagens
                    .OrderByDescending(m => m.DataEnvio)
                    .Select(m => new { m.Conteudo, m.DataEnvio, m.RemetenteId })
                    .FirstOrDefault(),
                NaoLidas = c.Mensagens.Count(m => !m.Lida && m.RemetenteId != usuarioId)
            })
            .ToListAsync();

        return Ok(conversas);
    }

    // GET /api/mensagensdiretas/{conversaId} — histórico da conversa
    [HttpGet("{conversaId}")]
    public async Task<IActionResult> HistoricoConversa(
        [FromRoute] int conversaId,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 50)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var conversa = await _contexto.Conversas.FindAsync(conversaId);
        if (conversa == null) return NotFound(new { erro = "Conversa não encontrada." });

        if (conversa.Usuario1Id != usuarioId && conversa.Usuario2Id != usuarioId)
            return Forbid();

        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1 || tamanhoPagina > 100) tamanhoPagina = 50;

        var mensagens = await _contexto.MensagensDiretas
            .AsNoTracking()
            .Where(m => m.ConversaId == conversaId)
            .OrderByDescending(m => m.DataEnvio)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(m => new
            {
                m.Id,
                m.Conteudo,
                m.RemetenteId,
                m.Lida,
                m.DataEnvio
            })
            .ToListAsync();

        // Marca como lidas as mensagens recebidas (não enviadas por mim)
        await _contexto.MensagensDiretas
            .Where(m => m.ConversaId == conversaId && m.RemetenteId != usuarioId && !m.Lida)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.Lida, true));

        return Ok(mensagens);
    }

    // POST /api/mensagensdiretas/enviar — envia mensagem (cria conversa se não existir)
    [HttpPost("enviar")]
    public async Task<IActionResult> Enviar([FromBody] EnviarMensagemDiretaDto dto)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        if (usuarioId == dto.DestinatarioId)
            return BadRequest(new { erro = "Você não pode enviar mensagem para si mesmo." });

        if (string.IsNullOrWhiteSpace(dto.Conteudo) || dto.Conteudo.Length > 1000)
            return BadRequest(new { erro = "Conteúdo inválido. Máximo 1000 caracteres." });

        // REGRA PRINCIPAL: só pode enviar DM se os dois se seguem mutuamente
        var euSigoEle = await _contexto.Seguidores
            .AnyAsync(s => s.SeguidorId == usuarioId && s.SeguidoId == dto.DestinatarioId);

        var eleMeSegue = await _contexto.Seguidores
            .AnyAsync(s => s.SeguidorId == dto.DestinatarioId && s.SeguidoId == usuarioId);

        if (!euSigoEle || !eleMeSegue)
            return BadRequest(new { erro = "Vocês precisam se seguir mutuamente para trocar mensagens." });

        // Normaliza o par para sempre ter o menor Id em Usuario1Id
        var menorId = Math.Min(usuarioId.Value, dto.DestinatarioId);
        var maiorId = Math.Max(usuarioId.Value, dto.DestinatarioId);

        var conversa = await _contexto.Conversas
            .FirstOrDefaultAsync(c => c.Usuario1Id == menorId && c.Usuario2Id == maiorId);

        if (conversa == null)
        {
            conversa = new Conversa { Usuario1Id = menorId, Usuario2Id = maiorId };
            _contexto.Conversas.Add(conversa);
            await _contexto.SaveChangesAsync();
        }

        var mensagem = new MensagemDireta
        {
            ConversaId = conversa.Id,
            RemetenteId = usuarioId.Value,
            Conteudo = dto.Conteudo
        };

        _contexto.MensagensDiretas.Add(mensagem);
        conversa.UltimaMensagemEm = DateTime.UtcNow;
        await _contexto.SaveChangesAsync();

        var remetente = await _contexto.Usuarios.FindAsync(usuarioId.Value);

        var payload = new
        {
            mensagem.Id,
            mensagem.ConversaId,
            mensagem.Conteudo,
            mensagem.DataEnvio,
            Remetente = new { remetente.Id, remetente.Nome, remetente.FotoPerfil }
        };

        // Envia em tempo real só para o destinatário — não precisa de F5
        await _hubContext.Clients
            .Group($"dm-usuario-{dto.DestinatarioId}")
            .SendAsync("ReceberMensagemDireta", payload);

        return Ok(new { mensagem = "Mensagem enviada!", conversaId = conversa.Id, mensagemId = mensagem.Id });
    }

    private int? ObterUsuarioId()
    {
        var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(valor, out int id) ? id : null;
    }
}