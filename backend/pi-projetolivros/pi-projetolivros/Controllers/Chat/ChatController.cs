using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.DTO.Chat;
using pi_projetolivros.Hubs;
using pi_projetolivros.Models.Chat;
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
    public ChatController(Banco contexto, IHubContext<ChatHub> hubContext)
    {
        _contexto = contexto;
        _hubContext = hubContext;
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

        if (dto.MensagemPaiId.HasValue)
        {
            var paiExiste = await _contexto.MensagensChat.AnyAsync(m => m.Id == dto.MensagemPaiId.Value);
            if (!paiExiste)
                return NotFound(new { erro = "O post que você está tentando responder não existe mais." });
        }

        var mensagemMolde = new MensagemChat
        {
            UsuarioId = usuarioId,
            Conteudo = dto.Conteudo,
            MensagemPaiId = dto.MensagemPaiId, 
            DataPostagem = DateTime.UtcNow
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
            MensagemPaiId = dto.MensagemPaiId 
        };

        await _hubContext.Clients.All.SendAsync("ReceberNovaMensagem", mensagemEmTempoReal);

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

        int quantidadePular = (pagina - 1) * tamanhoPagina;

        var mensagensGlobais = await _contexto.MensagensChat
            .AsNoTracking()
            .Where(m => m.MensagemPaiId == null) 
            .OrderByDescending(m => m.DataPostagem) 
            .Skip(quantidadePular)
            .Take(tamanhoPagina)
            .Select(m => new
            {
                Id = m.Id,
                Conteudo = m.Conteudo,
                DataPostagem = m.DataPostagem,
                Usuario = new
                {
                    Id = m.Usuario.Id,
                    Nome = m.Usuario.Nome,
                    FotoPerfil = m.Usuario.FotoPerfil
                },
                Respostas = m.Respostas
                    .OrderBy(r => r.DataPostagem) 
                    .Select(r => new
                    {
                        Id = r.Id,
                        Conteudo = r.Conteudo,
                        DataPostagem = r.DataPostagem,
                        Usuario = new
                        {
                            Id = r.Usuario.Id,
                            Nome = r.Usuario.Nome,
                            FotoPerfil = r.Usuario.FotoPerfil
                        }
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
        {
            _contexto.MensagensChat.RemoveRange(mensagem.Respostas);
        }

        _contexto.MensagensChat.Remove(mensagem);

        await _contexto.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("MensagemDeletada", id);

        return Ok(new { message = "Mensagem apagada com sucesso!" });
    }
}
