namespace pi_projetolivros.Models.Chat
{
    public class CurtidaChat
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int MensagemId { get; set; }
        public DateTime DataCurtida { get; set; } = DateTime.UtcNow;

        public Models.Banco.Usuario Usuario { get; set; } = null!;
        public MensagemChat Mensagem { get; set; } = null!;
    }
}
