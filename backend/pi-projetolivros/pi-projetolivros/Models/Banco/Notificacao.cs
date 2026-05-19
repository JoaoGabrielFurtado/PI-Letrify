namespace pi_projetolivros.Models.Banco;

public class Notificacao
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }

    public string Tipo { get; set; } = string.Empty;    // "Match" | "Seguidor" | "Mencao"
    public string Conteudo { get; set; } = string.Empty;
    public bool Lida { get; set; } = false;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
}
