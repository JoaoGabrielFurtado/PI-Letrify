namespace pi_projetolivros.DTO.Chat;

public class EnviarMensagemDto
{
    public string Conteudo { get; set; }
    public int? MensagemPaiId { get; set; }

    public int? GrupoId { get; set; }
}
