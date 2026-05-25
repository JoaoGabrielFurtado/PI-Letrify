using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.Servicos;
using pi_projetolivros_banco;
using System.Security.Claims;
using System.Text;

namespace pi_projetolivros.Controllers.Usuario;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PremiumController : ControllerBase
{
    private readonly Banco _contexto;
    private readonly GeminiServices _geminiService;

    public PremiumController(Banco contexto, GeminiServices geminiService)
    {
        _contexto = contexto;
        _geminiService = geminiService;
    }

    // GET /api/premium/analise
    [HttpGet("analise")]
    public async Task<IActionResult> AnalisePerfil()
    {
        var usuarioIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdText, out int usuarioId))
            return Unauthorized(new { erro = "Token inválido." });

        var usuario = await _contexto.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario == null)
            return NotFound(new { erro = "Usuário não encontrado." });

        if (!usuario.Premium)
            return StatusCode(403, new { erro = "Esta funcionalidade é exclusiva para usuários Premium." });

        var situacoes = await _contexto.SituacaoLivros
            .AsNoTracking()
            .Include(s => s.Livro)
            .Where(s => s.UsuarioId == usuarioId && s.Livro != null)
            .ToListAsync();

        if (!situacoes.Any())
            return Ok(new { mensagem = "Adicione livros à sua estante para receber sua análise." });

        var lidos = situacoes.Where(s => s.Status == "Lido").ToList();
        var lendo = situacoes.Where(s => s.Status == "Lendo").ToList();
        var quereLer = situacoes.Where(s => s.Status == "Quero Ler").ToList();

        var topTemas = situacoes
            .Where(s => !string.IsNullOrEmpty(s.Livro.Temas))
            .SelectMany(s => s.Livro.Temas.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()))
            .Where(t => t != "Sem Temas")
            .GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new { Tema = g.Key, Quantidade = g.Count() })
            .ToList();

        var topAutores = situacoes
            .Where(s => !string.IsNullOrEmpty(s.Livro.Autor))
            .SelectMany(s => s.Livro.Autor.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()))
            .Where(a => a != "Autor Desconhecido")
            .GroupBy(a => a)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new { Autor = g.Key, Quantidade = g.Count() })
            .ToList();

        var ultimosLidos = lidos
            .OrderByDescending(s => s.DataAtualizacao)
            .Take(5)
            .Select(s => s.Livro.Titulo)
            .ToList();

        var livroFavorito = await _contexto.Favoritos
            .AsNoTracking()
            .Include(f => f.Livro)
            .Where(f => f.UsuarioId == usuarioId)
            .Select(f => f.Livro.Titulo)
            .FirstOrDefaultAsync();

        var prompt = new StringBuilder();
        prompt.AppendLine("Você é um consultor literário especializado. Analise o perfil de leitura abaixo e escreva um texto personalizado em português do Brasil com dois blocos:");
        prompt.AppendLine("1. **Análise do Perfil**: Um parágrafo descrevendo o estilo, gostos e padrões de leitura desta pessoa de forma envolvente e empática.");
        prompt.AppendLine("2. **Recomendações**: Uma lista de 5 livros que esta pessoa provavelmente vai adorar, com uma frase explicando o motivo de cada indicação.");
        prompt.AppendLine("Seja específico, use os dados fornecidos e escreva de forma calorosa, como se fosse uma conversa entre amigos apaixonados por livros.");
        prompt.AppendLine();
        prompt.AppendLine($"Nome do leitor: {usuario.Nome}");
        prompt.AppendLine($"Total de livros na estante: {situacoes.Count}");
        prompt.AppendLine($"  - Lidos: {lidos.Count}");
        prompt.AppendLine($"  - Lendo atualmente: {lendo.Count}");
        prompt.AppendLine($"  - Quer ler: {quereLer.Count}");
        prompt.AppendLine($"  - Atenção: Retorne o texto num formato amigável, com emojis interatividos e num formato que agrade o leitor");

        if (ultimosLidos.Any())
            prompt.AppendLine($"Últimos livros lidos: {string.Join(", ", ultimosLidos)}");

        if (lendo.Any())
            prompt.AppendLine($"Lendo atualmente: {string.Join(", ", lendo.Select(s => s.Livro.Titulo))}");

        if (!string.IsNullOrEmpty(livroFavorito))
            prompt.AppendLine($"Livro favorito declarado: {livroFavorito}");

        if (topTemas.Any())
            prompt.AppendLine($"Temas mais lidos: {string.Join(", ", topTemas.Select(t => $"{t.Tema} ({t.Quantidade}x)"))}");

        if (topAutores.Any())
            prompt.AppendLine($"Autores preferidos: {string.Join(", ", topAutores.Select(a => $"{a.Autor} ({a.Quantidade} livros)"))}");

        if (quereLer.Any())
            prompt.AppendLine($"Livros na lista de desejos: {string.Join(", ", quereLer.Take(5).Select(s => s.Livro.Titulo))}");

        var analise = await _geminiService.GerarTextoAsync(prompt.ToString());

        return Ok(new
        {
            estatisticas = new
            {
                totalLivros = situacoes.Count,
                lidos = lidos.Count,
                lendo = lendo.Count,
                quereLer = quereLer.Count,
                topTemas,
                topAutores,
                ultimosLidos
            },
            analise 
        });
    }
}