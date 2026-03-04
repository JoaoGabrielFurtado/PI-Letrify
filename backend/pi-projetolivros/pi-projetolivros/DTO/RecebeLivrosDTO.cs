using System.Text.Json.Serialization;

namespace pi_projetolivros.DTO;

public class RecebeLivrosDTO
{
    [JsonPropertyName("title")]
    public string Titulo { get; set; }

    [JsonPropertyName("publish_date")]
    public string DataPublicacao { get; set; }

    [JsonPropertyName("number_of_pages")]
    public int? NumeroPaginas { get; set; } 

    [JsonPropertyName("authors")]
    public List<OpenLibraryAutorDto> Autores { get; set; }

    [JsonPropertyName("publishers")]
    public List<OpenLibraryEditoraDto> Editora { get; set; }

    [JsonPropertyName("subjects")]
    public List<OpenLibraryTemasDto> Temas { get; set; }
}

// ler autores em lista [ ] 
public class OpenLibraryAutorDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

// ler editoras
public class OpenLibraryEditoraDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

// ler temas
public class OpenLibraryTemasDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

