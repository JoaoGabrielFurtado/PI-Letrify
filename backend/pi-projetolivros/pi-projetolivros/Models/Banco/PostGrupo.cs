using pi_projetolivros.Models.Chat;

namespace pi_projetolivros.Models.Banco
{
    public class PostGrupo
    {
        public int Id { get; set; }
        public int GrupoId { get; set; }
        public int UsuarioId { get; set; }
        public string Conteudo { get; set; } = string.Empty;
        public int? PostPaiId { get; set; }
        public DateTime DataPostagem { get; set; } = DateTime.UtcNow;

        public Grupo Grupo { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public PostGrupo? PostPai { get; set; }
        public ICollection<PostGrupo> Respostas { get; set; } = new List<PostGrupo>();

        public virtual ICollection<CurtidaChat> Curtidas { get; set; } = new List<CurtidaChat>();
    }
}
