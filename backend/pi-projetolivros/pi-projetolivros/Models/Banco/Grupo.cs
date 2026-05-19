namespace pi_projetolivros.Models.Banco;

public class Grupo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Status { get; set; } = "Aberto"; // "Aberto" | "Fechado"
    public string? FotoCapa { get; set; }
    public int LiderId { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public Usuario Lider { get; set; } = null!;
    public ICollection<UsuarioGrupo> Membros { get; set; } = new List<UsuarioGrupo>();
}
