namespace pi_projetolivros.Models.Banco;

public class MensagemDireta
{
    public int Id { get; set; }
    public int ConversaId { get; set; }
    public int RemetenteId { get; set; }
    public string Conteudo { get; set; } = string.Empty;
    public bool Lida { get; set; } = false;
    public DateTime DataEnvio { get; set; } = DateTime.UtcNow;

    public Conversa Conversa { get; set; } = null!;
    public Usuario Remetente { get; set; } = null!;
}
