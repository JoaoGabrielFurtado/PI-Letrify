using System.Text;
using System.Text.Json;

namespace pi_projetolivros.Servicos;

public class OpenAIServices
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAIServices(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new ArgumentNullException("API Key da OpenAI não encontrada!");

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GerarTextoAsync(string prompt)
    {
        var url = "https://api.openai.com/v1/chat/completions";

        var body = new
        {
            model = "gpt-4o-mini", // mais barato e rápido
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = 1000,
            temperature = 0.7
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            var texto = result
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return texto ?? GerarAnaliseLocal(prompt);
        }

        return GerarAnaliseLocal(prompt);
    }

    public async Task<float[]> ObterEmbeddingAsync(string texto)
    {
        var url = "https://api.openai.com/v1/embeddings";

        var body = new
        {
            model = "text-embedding-3-small",
            input = texto
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();
            throw new Exception($"Erro na API da OpenAI: {erro}");
        }

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var values = result
            .GetProperty("data")[0]
            .GetProperty("embedding")
            .EnumerateArray()
            .Select(v => v.GetSingle())
            .ToArray();

        return values;
    }

    private string GerarAnaliseLocal(string prompt)
    {
        var linhas = prompt.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        string nome = ExtrairValor(linhas, "Nome do leitor:");
        string temas = ExtrairValor(linhas, "Temas mais lidos:");
        string autores = ExtrairValor(linhas, "Autores preferidos:");
        string lidos = ExtrairValor(linhas, "Últimos livros lidos:");
        string lendo = ExtrairValor(linhas, "Lendo atualmente:");
        string favorito = ExtrairValor(linhas, "Livro favorito declarado:");

        var sb = new StringBuilder();

        sb.AppendLine($"Análise do Perfil Literário de {nome}");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(temas))
            sb.AppendLine($"Sua estante revela um leitor apaixonado por {temas}. Esse perfil é típico de alguém que busca nas páginas não apenas entretenimento, mas reflexão e crescimento pessoal. Você tem um olhar criterioso para a literatura e não se contenta com o superficial.");
        else
            sb.AppendLine($"Sua estante revela um leitor versátil e curioso, sempre aberto a novas experiências literárias. Você transita entre diferentes gêneros com naturalidade, o que demonstra uma mente aberta e ávida por conhecimento.");

        sb.AppendLine();

        if (!string.IsNullOrEmpty(autores))
            sb.AppendLine($"A sua admiração por autores como {autores} diz muito sobre você. Esses escritores têm em comum a capacidade de construir narrativas densas e personagens que ficam na memória — exatamente o tipo de leitura que você valoriza.");

        if (!string.IsNullOrEmpty(favorito))
            sb.AppendLine($"O fato de {favorito} ser seu livro favorito reforça esse perfil: você aprecia obras que deixam marcas profundas e provocam reflexões que vão além das páginas.");

        if (!string.IsNullOrEmpty(lendo))
            sb.AppendLine($"Atualmente com {lendo} em mãos, você demonstra que mantém o hábito de leitura ativo e constante — uma característica admirável nos dias de hoje.");

        sb.AppendLine();
        sb.AppendLine("Recomendações especiais para você:");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(temas) && temas.Contains("Terror", StringComparison.OrdinalIgnoreCase))
        {
            sb.AppendLine("1. A Queda da Casa de Usher — Edgar Allan Poe: Um clássico do terror psicológico que vai ao encontro do seu gosto pelo suspense e atmosfera sombria.");
            sb.AppendLine("2. O Iluminado — Stephen King: Considerado uma das obras mais perturbadoras já escritas, perfeito para quem aprecia terror de qualidade.");
        }
        else if (!string.IsNullOrEmpty(temas) && temas.Contains("Ficção", StringComparison.OrdinalIgnoreCase))
        {
            sb.AppendLine("1. Duna — Frank Herbert: Uma épica de ficção científica que vai além do gênero, explorando política, religião e ecologia de forma magistral.");
            sb.AppendLine("2. Fundação — Isaac Asimov: A obra que definiu a ficção científica moderna, essencial para quem aprecia o gênero.");
        }
        else
        {
            sb.AppendLine("1. O Nome da Rosa — Umberto Eco: Um thriller histórico intelectualmente estimulante, perfeito para leitores exigentes.");
            sb.AppendLine("2. Cem Anos de Solidão — Gabriel García Márquez: Uma das maiores obras da literatura mundial, que encanta todo tipo de leitor.");
        }

        sb.AppendLine("3. O Mestre e Margarida — Mikhail Bulgakov: Uma obra única que mistura sátira política, romance e elementos sobrenaturais de forma brilhante.");
        sb.AppendLine("4. A Insustentável Leveza do Ser — Milan Kundera: Uma reflexão profunda sobre amor, identidade e as escolhas que definem nossas vidas.");
        sb.AppendLine("5. Sidarta — Hermann Hesse: Uma jornada espiritual que ressoa com leitores reflexivos e em busca de sentido.");

        sb.AppendLine();
        sb.AppendLine("Continue explorando novos mundos através das páginas. Sua estante já conta uma história linda sobre quem você é.");

        return sb.ToString();
    }

    private string ExtrairValor(string[] linhas, string chave)
    {
        var linha = linhas.FirstOrDefault(l => l.Contains(chave));
        if (linha == null) return string.Empty;
        var partes = linha.Split(':', 2);
        return partes.Length > 1 ? partes[1].Trim() : string.Empty;
    }
}
