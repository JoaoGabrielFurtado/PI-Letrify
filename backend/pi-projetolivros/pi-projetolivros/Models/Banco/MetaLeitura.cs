namespace pi_projetolivros.Models.Banco;

public class MetaLeitura
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Tipo { get; set; } = string.Empty; // "Paginas" | "Minutos" | "Livros"
    public int ValorAlvo { get; set; }
    public string Periodicidade { get; set; } = "Diaria"; // "Diaria" | "Semanal" | "Mensal"
    public bool Ativa { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
    public ICollection<CheckInLeitura> CheckIns { get; set; } = new List<CheckInLeitura>();
}
