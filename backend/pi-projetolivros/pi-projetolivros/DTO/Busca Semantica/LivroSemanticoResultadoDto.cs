namespace pi_projetolivros.DTO.RetornoQdrant;

public class LivroSemanticoResultadoDto
{
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string OpenLibraryKey { get; set; } = string.Empty;
    public string Assuntos { get; set; } = string.Empty;
    public double Score { get; set; }
    public string CapaUrl { get; set; } = string.Empty;
}