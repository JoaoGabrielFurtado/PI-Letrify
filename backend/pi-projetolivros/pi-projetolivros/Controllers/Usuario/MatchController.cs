using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.Models.Banco;
using pi_projetolivros.Servicos;
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
        try
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Busca banco
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
                .Take(5)
                .ToList();

            var rankAutores = livrosDoUsuario
                .Where(l => !string.IsNullOrEmpty(l.Autor))
                .SelectMany(l => l.Autor.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(t => t.Trim())
                .GroupBy(palavra => palavra)
                .OrderByDescending(grupo => grupo.Count())
                .Select(grupo => grupo.Key)
                .Take(5)
                .ToList();

            var ultimosLivrosLidos = await _contexto.SituacaoLivros
                .Include(sl => sl.Livro)
                .Where(sl => sl.UsuarioId == usuarioId && sl.Status == "Lido")
                .OrderByDescending(sl => sl.DataAtualizacao)
                .Select(sl => sl.Livro.Titulo)
                .Take(3)
                .ToListAsync();

            // Monta string 
            var temasLimpos = rankTemas.Where(t => t != "Sem Temas");
            var autoresLimpos = rankAutores.Where(a => a != "Autor Desconhecido");

            var perfilSemantic = "Perfil de leitor.";

            if (temasLimpos.Any())
                perfilSemantic += $" Gosta de temas como {string.Join(", ", temasLimpos)}.";

            if (autoresLimpos.Any())
                perfilSemantic += $" Admira autores como {string.Join(", ", autoresLimpos)}.";

            if (ultimosLivrosLidos.Any())
                perfilSemantic += $" Recentemente leu as obras {string.Join(", ", ultimosLivrosLidos)}.";

            string enviarIA = perfilSemantic.Trim();

            // IA e Vetor
            var vetorDeCaracteristicas = await _geminiService.ObterEmbeddingAsync(enviarIA);

            await _qdrantService.InicializarColecaoAsync(); 
            await _qdrantService.SalvarVetorUsuarioAsync(usuarioId, vetorDeCaracteristicas);

            var matches = await _qdrantService.BuscarUsuariosParecidosAsync(usuarioId, vetorDeCaracteristicas, limite: 20); 

            if (!matches.Any())
            {
                return Ok(new { mensagem = "Nenhum match encontrado ainda.", usuariosParecidos = new List<object>() });
            }

            var idsDosMatches = matches.Select(m => m.Id).ToList();

            var usuariosDoBanco = await _contexto.Usuarios
                .Where(u => idsDosMatches.Contains(u.Id) && u.Id != usuarioId)
                .Select(u => new
                {
                    u.Id,
                    u.Nome,
                    u.Cidade,
                    u.FotoPerfil
                })
                .ToListAsync();

            var resultadoFinal = matches
                .Where(m => m.Id != usuarioId) 
                .Take(18) 
                .Select(matchQdrant => new
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
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro ao processar o Match. O serviço de IA pode estar temporariamente indisponível.", erro = ex.Message });
        }
    }

    [HttpPost("migrar-todos-vetores")]
    public async Task<IActionResult> MigrarTodosVetores()
    {
        var usuariosIds = await _contexto.SituacaoLivros
            .Select(s => s.UsuarioId)
            .Distinct()
            .ToListAsync();

        int atualizados = 0;

        foreach (var id in usuariosIds)
        {
            if (id == null) continue;
            int targetId = id.Value;

            var livrosDoUsuario = await _contexto.SituacaoLivros
                .Include(l => l.Livro)
                .Where(u => u.UsuarioId == targetId && u.Livro != null)
                .Select(l => l.Livro)
                .ToListAsync();

            var rankTemas = livrosDoUsuario
                .Where(l => !string.IsNullOrEmpty(l.Temas))
                .SelectMany(l => l.Temas.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()))
                .Where(tema => tema != "Sem Temas")
                .GroupBy(palavra => palavra)
                .OrderByDescending(grupo => grupo.Count())
                .Select(grupo => grupo.Key)
                .Take(5).ToList();

            var rankAutores = livrosDoUsuario
                .Where(l => !string.IsNullOrEmpty(l.Autor))
                .SelectMany(l => l.Autor.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(t => t.Trim())
                .Where(a => a != "Autor Desconhecido")
                .GroupBy(palavra => palavra)
                .OrderByDescending(grupo => grupo.Count())
                .Select(grupo => grupo.Key)
                .Take(5).ToList();

            var ultimosLivrosLidos = await _contexto.SituacaoLivros
                .Include(sl => sl.Livro)
                .Where(sl => sl.UsuarioId == targetId && sl.Status == "Lido")
                .OrderByDescending(sl => sl.DataAtualizacao)
                .Select(sl => sl.Livro.Titulo)
                .Take(3).ToListAsync();

            var perfilSemantic = "Perfil de leitor.";
            if (rankTemas.Any()) perfilSemantic += $" Gosta de temas como {string.Join(", ", rankTemas)}.";
            if (rankAutores.Any()) perfilSemantic += $" Admira autores como {string.Join(", ", rankAutores)}.";
            if (ultimosLivrosLidos.Any()) perfilSemantic += $" Recentemente leu as obras {string.Join(", ", ultimosLivrosLidos)}.";

            string enviarIA = perfilSemantic.Trim();

            if (enviarIA == "Perfil de leitor.") continue;

            try
            {
                var vetorNovo = await _geminiService.ObterEmbeddingAsync(enviarIA);
                await _qdrantService.SalvarVetorUsuarioAsync(targetId, vetorNovo);
                atualizados++;

                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao migrar usuário {targetId}: {ex.Message}");
            }
        }

        return Ok(new { mensagem = $"Migração concluída! {atualizados} usuários tiveram seus vetores atualizados com a nova string." });
    }
}
