namespace pi_projetolivros.DTO.Chat;

public class PublicarResenhaDto
{
    public string Conteudo { get; set; } = string.Empty;
    public int LivroId { get; set; }
    public int NotaLivro { get; set; }
    public int? GrupoId { get; set; }
}
