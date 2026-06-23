using System.Text.Json.Serialization;

namespace pi_projetolivros.DTO.ConectarApiLivros;

public class OpenLibraryBuscaSemanticaDTO
{
    [JsonPropertyName("docs")]
    public List<OpenLibraryBuscaSemanticaDoc>? Docs { get; set; }
}

public class OpenLibraryBuscaSemanticaDoc
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("author_name")]
    public List<string>? AuthorName { get; set; }

    [JsonPropertyName("subject")]
    public List<string>? Subject { get; set; }

    [JsonPropertyName("isbn")]
    public List<string>? Isbn { get; set; }

    [JsonIgnore]
    public string? FirstSentence { get; set; }
}