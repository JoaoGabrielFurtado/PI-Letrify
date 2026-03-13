namespace pi_projetolivros.DTO.SituacaoLivros;

public class AdicionarLivroColecaoDto
{
    public string Titulo { get; set; } = string.Empty; 
    public string? Autor { get; set; }
    public string? Isbn { get; set; }  


    // "Lendo", "Lido", "Quero Ler"
    public string Status { get; set; } = string.Empty;
}
