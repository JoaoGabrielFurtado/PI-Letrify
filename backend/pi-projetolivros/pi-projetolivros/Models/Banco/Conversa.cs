namespace pi_projetolivros.Models.Banco;

public class Conversa
{
    public int Id { get; set; }
    public int Usuario1Id { get; set; }
    public int Usuario2Id { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? UltimaMensagemEm { get; set; }

    public Usuario Usuario1 { get; set; } = null!;
    public Usuario Usuario2 { get; set; } = null!;
    public ICollection<MensagemDireta> Mensagens { get; set; } = new List<MensagemDireta>();
}
