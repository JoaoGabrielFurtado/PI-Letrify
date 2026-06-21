namespace pi_projetolivros.Models.Banco;

public class CheckInLeitura
{
    public int Id { get; set; }
    public int MetaId { get; set; }
    public int UsuarioId { get; set; }
    public DateOnly Data { get; set; }
    public int ValorRegistrado { get; set; }
    public bool Cumprida { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public MetaLeitura Meta { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
