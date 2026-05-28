using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.DTO.Chat;
using pi_projetolivros.Hubs;
using pi_projetolivros.Models.Chat;
using pi_projetolivros.Servicos;
using pi_projetolivros_banco;
using System.Security.Claims;

namespace pi_projetolivros.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly Banco _contexto;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly NotificacaoService _notificacaoService;

    public ChatController(Banco contexto, IHubContext<ChatHub> hubContext, NotificacaoService notificacaoService)
    {
        _contexto = contexto;
        _hubContext = hubContext;
        _notificacaoService = notificacaoService; 
    }

    [HttpPost("enviar")]
    [EnableRateLimiting("ChatAntiSpam")]
    public async Task<IActionResult> EnviarChat([FromBody] EnviarMensagemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Conteudo))
            return BadRequest(new { erro = "O conteúdo da mensagem não pode ser vazio." });

        if (dto.Conteudo.Length > 150)
            return BadRequest(new { erro = "Limite do Post: 150 caracteres." });

        var usuarioIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdText, out int usuarioId))
            return Unauthorized(new { erro = "Token inválido." });

        if (dto.GrupoId.HasValue)
        {
            var grupoExiste = await _contexto.Grupos.AnyAsync(g => g.Id == dto.GrupoId.Value);
            if (!grupoExiste)
                return NotFound(new { erro = "Grupo não encontrado." });

            var ehMembro = await _contexto.UsuarioGrupos
                .AnyAsync(ug => ug.GrupoId == dto.GrupoId.Value && ug.UsuarioId == usuarioId);

            if (!ehMembro)
                return Forbid();
        }

        int? donoDaMensagemPaiId = null;

        if (dto.MensagemPaiId.HasValue)
        {
            var mensagemPai = await _contexto.MensagensChat
                .Where(m => m.Id == dto.MensagemPaiId.Value)
                .Select(m => new { m.Id, m.UsuarioId })
                .FirstOrDefaultAsync();

            if (mensagemPai == null)
                return NotFound(new { erro = "O post que você está tentando responder não existe mais." });

            if (mensagemPai.UsuarioId != usuarioId)
                donoDaMensagemPaiId = mensagemPai.UsuarioId;
        }

        var mensagemMolde = new MensagemChat
        {
            UsuarioId = usuarioId,
            Conteudo = dto.Conteudo,
            MensagemPaiId = dto.MensagemPaiId,
            DataPostagem = DateTime.UtcNow,
            GrupoId = dto.GrupoId 
        };

        _contexto.MensagensChat.Add(mensagemMolde);
        await _contexto.SaveChangesAsync();

        var usuarioQueEnviou = await _contexto.Usuarios.FindAsync(usuarioId);

        var mensagemEmTempoReal = new
        {
            Id = mensagemMolde.Id,
            Conteudo = mensagemMolde.Conteudo,
            DataPostagem = mensagemMolde.DataPostagem,
            Usuario = new
            {
                Id = usuarioQueEnviou.Id,
                Nome = usuarioQueEnviou.Nome,
                FotoPerfil = usuarioQueEnviou.FotoPerfil
            },
            MensagemPaiId = dto.MensagemPaiId,

            Grupo = dto.GrupoId.HasValue ? new
            {
                Id = mensagemMolde.GrupoId,
                Nome = await _contexto.Grupos
                    .Where(g => g.Id == dto.GrupoId.Value)
                    .Select(g => g.Nome)
                    .FirstOrDefaultAsync()
            } : null
        };

        await _hubContext.Clients.All.SendAsync("ReceberNovaMensagem", mensagemEmTempoReal);

        if (donoDaMensagemPaiId.HasValue)
        {
            await _notificacaoService.EnviarAsync(
                usuarioDestinoId: donoDaMensagemPaiId.Value,
                tipo: "Mencao",
                conteudo: $"{usuarioQueEnviou.Nome} respondeu ao seu comentário."
            );
        }

        return Ok(new
        {
            message = "Mensagem enviada com sucesso!",
            id = mensagemMolde.Id,
            dataPostagem = mensagemMolde.DataPostagem
        });
    }

    [HttpGet("listar")]
    [AllowAnonymous]
    public async Task<IActionResult> ListarChatGlobal([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 50)
    {
        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1 || tamanhoPagina > 100) tamanhoPagina = 50;

        var usuarioIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(usuarioIdText, out int usuarioLogadoId);

        int quantidadePular = (pagina - 1) * tamanhoPagina;

        var mensagensGlobais = await _contexto.MensagensChat
            .AsNoTracking()
            .Where(m => m.MensagemPaiId == null)
            .OrderByDescending(m => m.DataPostagem)
            .Skip(quantidadePular)
            .Take(tamanhoPagina)
            .Select(m => new
            {
                m.Id,
                m.Conteudo,
                m.DataPostagem,
                Usuario = new
                {
                    m.Usuario.Id,
                    m.Usuario.Nome,
                    m.Usuario.FotoPerfil,
                    m.Usuario.Premium
                },
                TotalCurtidas = m.Curtidas.Count,
                EuCurti = usuarioLogadoId != 0 && m.Curtidas.Any(c => c.UsuarioId == usuarioLogadoId),
                Grupo = m.GrupoId != null ? new
                {
                    m.Grupo.Id,
                    m.Grupo.Nome,
                    m.Grupo.FotoCapa,
                    m.Grupo.Status
                } : null,
                Respostas = m.Respostas
                    .OrderBy(r => r.DataPostagem)
                    .Select(r => new
                    {
                        r.Id,
                        r.Conteudo,
                        r.DataPostagem,
                        Usuario = new
                        {
                            r.Usuario.Id,
                            r.Usuario.Nome,
                            r.Usuario.FotoPerfil
                        },
                        TotalCurtidas = r.Curtidas.Count,
                        EuCurti = usuarioLogadoId != 0 && r.Curtidas.Any(c => c.UsuarioId == usuarioLogadoId)
                    }).ToList()
            })
            .ToListAsync();

        return Ok(mensagensGlobais);
    }

    [HttpDelete("deletar/{id}")]
    public async Task<IActionResult> DeletarMensagem([FromRoute] int id)
    {
        var usuarioIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdText, out int usuarioId))
            return Unauthorized(new { erro = "Token inválido." });

        var mensagem = await _contexto.MensagensChat
            .Include(m => m.Respostas)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mensagem == null)
            return NotFound(new { erro = "Mensagem não encontrada." });

        if (mensagem.UsuarioId != usuarioId)
            return BadRequest(new { erro = "Você não tem permissão para apagar esta mensagem." });

        if (mensagem.Respostas.Any())
            _contexto.MensagensChat.RemoveRange(mensagem.Respostas);

        _contexto.MensagensChat.Remove(mensagem);
        await _contexto.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("MensagemDeletada", id);

        return Ok(new { message = "Mensagem apagada com sucesso!" });
    }

    [HttpPost("curtir/{mensagemId}")]
    public async Task<IActionResult> CurtirOuDescurtir([FromRoute] int mensagemId)
    {
        var usuarioIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdText, out int usuarioId))
            return Unauthorized(new { erro = "Token inválido." });

        var mensagemExiste = await _contexto.MensagensChat.AnyAsync(m => m.Id == mensagemId);
        if (!mensagemExiste)
            return NotFound(new { erro = "Mensagem não encontrada." });

        var curtidaExistente = await _contexto.CurtidasChat
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.MensagemId == mensagemId);

        if (curtidaExistente != null)
        {
            // Já curtiu → descurte
            _contexto.CurtidasChat.Remove(curtidaExistente);
            await _contexto.SaveChangesAsync();

            var totalAposRemover = await _contexto.CurtidasChat.CountAsync(c => c.MensagemId == mensagemId);

            await _hubContext.Clients.All.SendAsync("AtualizarCurtidas", new
            {
                MensagemId = mensagemId,
                Total = totalAposRemover,
                Curtiu = false
            });

            return Ok(new { mensagem = "Curtida removida.", curtiu = false, total = totalAposRemover });
        }

        // Não curtiu ainda → curte
        _contexto.CurtidasChat.Add(new CurtidaChat
        {
            UsuarioId = usuarioId,
            MensagemId = mensagemId
        });
        await _contexto.SaveChangesAsync();

        var totalAposAdicionar = await _contexto.CurtidasChat.CountAsync(c => c.MensagemId == mensagemId);

        await _hubContext.Clients.All.SendAsync("AtualizarCurtidas", new
        {
            MensagemId = mensagemId,
            Total = totalAposAdicionar,
            Curtiu = true
        });

        return Ok(new { mensagem = "Amei!", curtiu = true, total = totalAposAdicionar });
    }
}