namespace pi_projetolivros.DTO.Usuario;

using pi_projetolivros.Models.Banco;

public class Seguidor
{
    public int Id { get; set; }
    public int SeguidorId { get; set; }

    public virtual Usuario SeguidorUsuario { get; set; }

    public int SeguidoId { get; set; }
    public virtual Usuario SeguidoUsuario { get; set; }

    public DateTime DataSeguimento { get; set; } = DateTime.Now;
}
