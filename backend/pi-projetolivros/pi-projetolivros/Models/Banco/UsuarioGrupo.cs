namespace pi_projetolivros.Models.Banco
{
    public class UsuarioGrupo
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int GrupoId { get; set; }
        public string Role { get; set; } = "Membro"; // "Lider" | "Admin" | "Membro"
        public DateTime DataEntrada { get; set; } = DateTime.UtcNow;

        public Usuario Usuario { get; set; } = null!;
        public Grupo Grupo { get; set; } = null!;
    }
}
