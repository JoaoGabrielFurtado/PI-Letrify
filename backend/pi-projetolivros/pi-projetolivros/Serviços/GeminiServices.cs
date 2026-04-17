using System.Text;
using System.Text.Json;

namespace pi_projetolivros.Serviços;

public class GeminiServices
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiServices(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("API Key do Gemini não encontrada!");
    }

    public async Task<float[]> ObterEmbeddingAsync(string textoGostosLiterarios)
    {
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key={_apiKey}";

        var corpoRequisicao = new
        {
            model = "models/gemini-embedding-001",
            content = new
            {
                parts = new[] { new { text = textoGostosLiterarios } }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(corpoRequisicao), Encoding.UTF8, "application/json");

        var resposta = await _httpClient.PostAsync(url, jsonContent);

        if (!resposta.IsSuccessStatusCode)
        {
            if (resposta.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw new Exception("O nosso radar de Inteligência Artificial está superaquecido no momento! Por favor, aguarde 1 minutinho e tente novamente.");
            }

            var erro = await resposta.Content.ReadAsStringAsync();
            throw new Exception($"Erro na API do Gemini: {erro}");
        }

        var jsonResposta = await resposta.Content.ReadAsStringAsync();

        var opcoesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(jsonResposta, opcoesJson);

        return embeddingResponse?.embedding?.values ?? new float[0];
    }
}

public class EmbeddingResponse
{
    public Embedding embedding { get; set; }
}

public class Embedding
{
    public float[] values { get; set; }
}
