using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.Models.Banco;
using pi_projetolivros.Serviços;
using pi_projetolivros_banco;
using System.Reflection.Metadata;
using System.Security.Claims;

namespace pi_projetolivros.Controllers.Usuario;

[ApiController]
[Route("api/[controller]")]
public class MatchController : ControllerBase
{
    private readonly Banco _contexto;
    private readonly GeminiServices _geminiService;
    private readonly QdrantServices _qdrantService; 

    public MatchController(Banco contexto, GeminiServices geminiService, QdrantServices qdrantService)
    {
        _contexto = contexto;
        _geminiService = geminiService;
        _qdrantService = qdrantService;
    }

    [HttpPost("")]
    [Authorize]
    public async Task<IActionResult> AtualizaDadosMatch()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var livrosDoUsuario = await _contexto.SituacaoLivros
        .Include(l => l.Livro)
        .Where(u => u.UsuarioId == usuarioId && u.Livro != null)
        .Select(l => l.Livro)
        .ToListAsync();

        var rankTemas = livrosDoUsuario
            .Where(l => !string.IsNullOrEmpty(l.Temas))
            .SelectMany(l => l.Temas.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()))
            .GroupBy(palavra => palavra)
            .OrderByDescending(grupo => grupo.Count())
            .Select(grupo => grupo.Key)
            .Take(3)
            .ToList();

        var rankAutores = livrosDoUsuario
            .Where(l => !string.IsNullOrEmpty(l.Autor))
            .SelectMany(l => l.Autor.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(t => t.Trim())
            .GroupBy(palavra => palavra)
            .OrderByDescending(grupo => grupo.Count())
            .Select(grupo => grupo.Key)
            .Take(3)
            .ToList();

        var temasLimpos = rankTemas.Where(t => t != "Sem Temas");
        var autoresLimpos = rankAutores.Where(a => a != "Autor Desconhecido");

        string enviarIA = $"Temas Favoritos: {string.Join(", ", temasLimpos)}. Autores Favoritos: {string.Join(", ", autoresLimpos)}.";

        var vetorDeCaracteristicas = await _geminiService.ObterEmbeddingAsync(enviarIA);

        await _qdrantService.InicializarColecaoAsync();

        await _qdrantService.SalvarVetorUsuarioAsync(usuarioId, vetorDeCaracteristicas);

        var matches = await _qdrantService.BuscarUsuariosParecidosAsync(usuarioId, vetorDeCaracteristicas, limite: 5);

        if (!matches.Any())
        {
            return Ok(new { mensagem = "Nenhum match encontrado ainda.", usuariosParecidos = new List<object>() });
        }

        var idsDosMatches = matches.Select(m => m.Id).ToList();

        var usuariosDoBanco = await _contexto.Usuarios
            .Where(u => idsDosMatches.Contains(u.Id))
            .Select(u => new
            {
                u.Id,
                u.Nome,  
                u.Cidade,
                u.FotoPerfil
            })
            .ToListAsync();

        var resultadoFinal = matches.Select(matchQdrant => new
        {
            Usuario = usuariosDoBanco.FirstOrDefault(u => u.Id == matchQdrant.Id),
            ScoreMatch = matchQdrant.Score
        })
        .Where(x => x.Usuario != null) 
        .ToList();

        return Ok(new
        {
            mensagem = "Matchmaking concluído com sucesso!",
            usuariosParecidos = resultadoFinal
        });
    }
}
