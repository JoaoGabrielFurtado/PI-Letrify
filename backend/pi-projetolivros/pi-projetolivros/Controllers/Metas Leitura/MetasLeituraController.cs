using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.Models.Banco;
using pi_projetolivros.Servicos;
using pi_projetolivros_banco;
using System.Security.Claims;

namespace pi_projetolivros.Controllers.Metas_Leitura;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MetasLeituraController : ControllerBase
{
    private readonly Banco _contexto;
    private readonly NotificacaoService _notificacaoService;

    public MetasLeituraController(Banco contexto, NotificacaoService notificacaoService)
    {
        _contexto = contexto;
        _notificacaoService = notificacaoService;
    }

    // POST /api/metasleitura — cria uma nova meta
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarMetaDto dto)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var tiposPermitidos = new[] { "Paginas", "Minutos", "Livros" };
        var periodicidadesPermitidas = new[] { "Diaria", "Semanal", "Mensal" };

        if (!tiposPermitidos.Contains(dto.Tipo))
            return BadRequest(new { erro = "Tipo inválido. Use 'Paginas', 'Minutos' ou 'Livros'." });

        if (!periodicidadesPermitidas.Contains(dto.Periodicidade))
            return BadRequest(new { erro = "Periodicidade inválida." });

        if (dto.ValorAlvo <= 0)
            return BadRequest(new { erro = "O valor alvo deve ser maior que zero." });

        // Desativa metas antigas do mesmo tipo para não acumular metas conflitantes
        var metasAntigas = await _contexto.MetasLeitura
            .Where(m => m.UsuarioId == usuarioId && m.Tipo == dto.Tipo && m.Ativa)
            .ToListAsync();

        foreach (var antiga in metasAntigas)
            antiga.Ativa = false;

        var meta = new MetaLeitura
        {
            UsuarioId = usuarioId.Value,
            Tipo = dto.Tipo,
            ValorAlvo = dto.ValorAlvo,
            Periodicidade = dto.Periodicidade
        };

        _contexto.MetasLeitura.Add(meta);
        await _contexto.SaveChangesAsync();

        // Garante que o usuário tenha um registro de streak
        var streakExiste = await _contexto.StreaksLeitura.AnyAsync(s => s.UsuarioId == usuarioId);
        if (!streakExiste)
        {
            _contexto.StreaksLeitura.Add(new StreakLeitura { UsuarioId = usuarioId.Value });
            await _contexto.SaveChangesAsync();
        }

        return Ok(new { mensagem = "Meta criada com sucesso!", metaId = meta.Id });
    }

    // GET /api/metasleitura — lista metas ativas do usuário
    [HttpGet]
    public async Task<IActionResult> ListarMinhasMetas()
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var metas = await _contexto.MetasLeitura
            .AsNoTracking()
            .Where(m => m.UsuarioId == usuarioId && m.Ativa)
            .Select(m => new
            {
                m.Id,
                m.Tipo,
                m.ValorAlvo,
                m.Periodicidade,
                m.DataCriacao,
                // Indica se já fez check-in hoje
                CheckInHoje = m.CheckIns.Any(c => c.Data == DateOnly.FromDateTime(DateTime.UtcNow))
            })
            .ToListAsync();

        return Ok(metas);
    }

    // POST /api/metasleitura/{metaId}/checkin — marca o progresso do dia
    [HttpPost("{metaId}/checkin")]
    public async Task<IActionResult> FazerCheckIn([FromRoute] int metaId, [FromBody] CheckInDto dto)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var meta = await _contexto.MetasLeitura
            .FirstOrDefaultAsync(m => m.Id == metaId && m.UsuarioId == usuarioId && m.Ativa);

        if (meta == null)
            return NotFound(new { erro = "Meta não encontrada." });

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        var checkInExistente = await _contexto.CheckInsLeitura
            .FirstOrDefaultAsync(c => c.MetaId == metaId && c.Data == hoje);

        if (checkInExistente != null)
            return BadRequest(new { erro = "Você já fez check-in hoje para esta meta." });

        var cumpriu = dto.ValorRegistrado >= meta.ValorAlvo;

        var checkIn = new CheckInLeitura
        {
            MetaId = metaId,
            UsuarioId = usuarioId.Value,
            Data = hoje,
            ValorRegistrado = dto.ValorRegistrado,
            Cumprida = cumpriu
        };

        _contexto.CheckInsLeitura.Add(checkIn);
        await _contexto.SaveChangesAsync();

        // ── Atualiza o streak ──────────────────────────────────────────────
        var streak = await _contexto.StreaksLeitura
            .FirstOrDefaultAsync(s => s.UsuarioId == usuarioId);

        if (streak == null)
        {
            streak = new StreakLeitura { UsuarioId = usuarioId.Value };
            _contexto.StreaksLeitura.Add(streak);
        }

        if (cumpriu)
        {
            var ontem = hoje.AddDays(-1);

            if (streak.UltimoCheckIn == ontem || streak.UltimoCheckIn == null)
            {
                // Manteve a sequência ou é o primeiro check-in
                streak.StreakAtual++;
            }
            else if (streak.UltimoCheckIn != hoje)
            {
                // Quebrou a sequência — reinicia
                streak.StreakAtual = 1;
            }

            streak.UltimoCheckIn = hoje;
            streak.MaiorStreak = Math.Max(streak.MaiorStreak, streak.StreakAtual);

            await _contexto.SaveChangesAsync();

            // Notifica marcos de streak (7, 30, 100 dias)
            var marcos = new[] { 7, 30, 100, 365 };
            if (marcos.Contains(streak.StreakAtual))
            {
                await _notificacaoService.EnviarAsync(
                    usuarioDestinoId: usuarioId.Value,
                    tipo: "Mencao",
                    conteudo: $"🔥 Incrível! Você atingiu {streak.StreakAtual} dias seguidos de leitura!"
                );
            }
        }
        else
        {
            await _contexto.SaveChangesAsync();
        }

        return Ok(new
        {
            mensagem = cumpriu ? "Meta do dia cumprida! 🎉" : "Check-in registrado, mas a meta não foi atingida hoje.",
            cumprida = cumpriu,
            streakAtual = streak.StreakAtual,
            maiorStreak = streak.MaiorStreak
        });
    }

    // GET /api/metasleitura/streak — retorna o streak atual do usuário
    [HttpGet("streak")]
    public async Task<IActionResult> ObterStreak()
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var streak = await _contexto.StreaksLeitura
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UsuarioId == usuarioId);

        if (streak == null)
            return Ok(new { streakAtual = 0, maiorStreak = 0, ultimoCheckIn = (DateOnly?)null });

        return Ok(new
        {
            streak.StreakAtual,
            streak.MaiorStreak,
            streak.UltimoCheckIn,
            streak.CongelamentosDisponiveis
        });
    }

    // GET /api/metasleitura/{metaId}/historico — calendário de check-ins (últimos 30 dias)
    [HttpGet("{metaId}/historico")]
    public async Task<IActionResult> Historico([FromRoute] int metaId)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var metaExiste = await _contexto.MetasLeitura
            .AnyAsync(m => m.Id == metaId && m.UsuarioId == usuarioId);

        if (!metaExiste)
            return NotFound(new { erro = "Meta não encontrada." });

        var limite = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30);

        var historico = await _contexto.CheckInsLeitura
            .AsNoTracking()
            .Where(c => c.MetaId == metaId && c.Data >= limite)
            .OrderBy(c => c.Data)
            .Select(c => new { c.Data, c.ValorRegistrado, c.Cumprida })
            .ToListAsync();

        return Ok(historico);
    }

    // DELETE /api/metasleitura/{metaId} — desativa a meta
    [HttpDelete("{metaId}")]
    public async Task<IActionResult> Desativar([FromRoute] int metaId)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var meta = await _contexto.MetasLeitura
            .FirstOrDefaultAsync(m => m.Id == metaId && m.UsuarioId == usuarioId);

        if (meta == null)
            return NotFound(new { erro = "Meta não encontrada." });

        meta.Ativa = false;
        await _contexto.SaveChangesAsync();

        return Ok(new { mensagem = "Meta desativada com sucesso." });
    }

    private int? ObterUsuarioId()
    {
        var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(valor, out int id) ? id : null;
    }
}