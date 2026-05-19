namespace pi_projetolivros.Models.Banco
{
    public class SolicitacaoGrupo
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int GrupoId { get; set; }
        public string Status { get; set; } = "Pendente"; // "Pendente" | "Aceita" | "Recusada"
        public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;

        public Usuario Usuario { get; set; } = null!;
        public Grupo Grupo { get; set; } = null!;
    }
}
