using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.Hubs;
using pi_projetolivros.Models.Banco;
using pi_projetolivros_banco;
using System.Security.Claims;

namespace pi_projetolivros.Controllers.Usuario;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificacoesController : ControllerBase
{
    private readonly Banco _contexto;
    private readonly IHubContext<NotificacaoHub> _hubContext;

    public NotificacoesController(Banco contexto, IHubContext<NotificacaoHub> hubContext)
    {
        _contexto = contexto;
        _hubContext = hubContext;
    }

    // ── GET /api/notificacoes ────────────────────────────────────────────────
    // Retorna as notificações do usuário logado (não lidas primeiro)
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool apenasNaoLidas = false)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var query = _contexto.Notificacoes
            .AsNoTracking()
            .Where(n => n.UsuarioId == usuarioId);

        if (apenasNaoLidas)
            query = query.Where(n => !n.Lida);

        var notificacoes = await query
            .OrderBy(n => n.Lida)                // não lidas primeiro
            .ThenByDescending(n => n.DataCriacao)
            .Select(n => new
            {
                n.Id,
                n.Tipo,
                n.Conteudo,
                n.Lida,
                n.DataCriacao
            })
            .Take(50)
            .ToListAsync();

        var totalNaoLidas = await _contexto.Notificacoes
            .CountAsync(n => n.UsuarioId == usuarioId && !n.Lida);

        return Ok(new { notificacoes, totalNaoLidas });
    }

    // ── PUT /api/notificacoes/{id}/lida ──────────────────────────────────────
    // Marca uma notificação específica como lida
    [HttpPut("{id}/lida")]
    public async Task<IActionResult> MarcarComoLida([FromRoute] int id)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var notificacao = await _contexto.Notificacoes
            .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == usuarioId);

        if (notificacao == null)
            return NotFound(new { erro = "Notificação não encontrada." });

        notificacao.Lida = true;
        await _contexto.SaveChangesAsync();

        return Ok(new { mensagem = "Notificação marcada como lida." });
    }

    // ── PUT /api/notificacoes/marcar-todas-lidas ─────────────────────────────
    [HttpPut("marcar-todas-lidas")]
    public async Task<IActionResult> MarcarTodasComoLidas()
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        await _contexto.Notificacoes
            .Where(n => n.UsuarioId == usuarioId && !n.Lida)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Lida, true));

        return Ok(new { mensagem = "Todas as notificações foram marcadas como lidas." });
    }

    // ── DELETE /api/notificacoes/{id} ────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar([FromRoute] int id)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var notificacao = await _contexto.Notificacoes
            .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == usuarioId);

        if (notificacao == null)
            return NotFound(new { erro = "Notificação não encontrada." });

        _contexto.Notificacoes.Remove(notificacao);
        await _contexto.SaveChangesAsync();

        return Ok(new { mensagem = "Notificação removida." });
    }

    // ── Método auxiliar ──────────────────────────────────────────────────────
    private int? ObterUsuarioId()
    {
        var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(valor, out int id) ? id : null;
    }
}