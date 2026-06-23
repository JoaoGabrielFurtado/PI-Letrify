using pi_projetolivros.DTO.ConectarApiLivros;
using Qdrant.Client.Grpc;

namespace pi_projetolivros.Servicos;

public class LivroIngestaoService
{
    private readonly HttpClient _http;
    private readonly GeminiServices _geminiService;
    private readonly QdrantServices _qdrantService;

    public LivroIngestaoService(HttpClient http, GeminiServices geminiService, QdrantServices qdrantService)
    {
        _http = http;
        _geminiService = geminiService;
        _qdrantService = qdrantService;
    }

    public async Task IndexarLivrosAsync(IEnumerable<string> titulos)
    {
        foreach (var titulo in titulos)
        {
            var url = $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(titulo)}&limit=1&fields=key,title,author_name,subject,isbn";
            var response = await _http.GetFromJsonAsync<OpenLibraryBuscaSemanticaDTO>(url);
            var livro = response?.Docs?.FirstOrDefault();
            if (livro == null) continue;

            var textoSemantico = MontarTextoSemantico(livro);
            var embedding = await _geminiService.ObterEmbeddingAsync(textoSemantico);

            await _qdrantService.SalvarVetorLivroAsync(livro, embedding);
        }
    }

    private string MontarTextoSemantico(OpenLibraryBuscaSemanticaDoc livro)
    {
        var texto = "Livro literário.";

        if (livro.AuthorName?.Any() == true)
            texto += $" Escrito por {string.Join(", ", livro.AuthorName)}.";

        if (livro.Subject?.Any() == true)
            texto += $" Temas e gêneros: {string.Join(", ", livro.Subject.Take(8))}.";

        return texto;
    }
}