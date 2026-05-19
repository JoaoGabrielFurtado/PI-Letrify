namespace pi_projetolivros.Models.Banco
{
    public class MensagemGrupo
    {
        public int Id { get; set; }
        public int GrupoId { get; set; }
        public int UsuarioId { get; set; }
        public string Conteudo { get; set; } = string.Empty;
        public DateTime DataPostagem { get; set; } = DateTime.UtcNow;

        public Grupo Grupo { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
    }
}
