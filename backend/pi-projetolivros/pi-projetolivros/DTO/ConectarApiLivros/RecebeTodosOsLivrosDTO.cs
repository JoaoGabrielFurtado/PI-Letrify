using System.Text.Json.Serialization;

namespace pi_projetolivros.DTO.ConectarApiLivros;

public class RecebeTodosOsLivrosDTO
{
    [JsonPropertyName("numFound")]
    public int NumEncontrado { get; set; } 

    [JsonPropertyName("docs")]
    public List<OpenLibraryPesquisaDoc> Docs { get; set; } 
}

public class OpenLibraryPesquisaDoc
{
    [JsonPropertyName("title")]
    public string Titulo { get; set; }

    [JsonPropertyName("author_name")]
    public List<string> NomeAutor { get; set; }

    [JsonPropertyName("first_publish_year")]
    public int? PrimeiroAnoPublicacao { get; set; }

    [JsonPropertyName("isbn")]
    public List<string> Isbn { get; set; }

    [JsonPropertyName("publisher")]
    public List<string> Editora { get; set; }

    [JsonPropertyName("subject")]
    public List<string> Temas { get; set; }
}
