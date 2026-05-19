using pi_projetolivros.Models.Banco;

namespace pi_projetolivros.Models.Chat;

public class MensagemChat
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; }
    public string Conteudo { get; set; }
    public int? MensagemPaiId { get; set; }
    public virtual MensagemChat MensagemPai { get; set; }
    public virtual ICollection<MensagemChat> Respostas { get; set; }
    public DateTime DataPostagem { get; set; } = DateTime.UtcNow;
    public virtual ICollection<CurtidaChat> Curtidas { get; set; } = new List<CurtidaChat>();

    public int? GrupoId { get; set; }
    public virtual Grupo Grupo { get; set; }
}
