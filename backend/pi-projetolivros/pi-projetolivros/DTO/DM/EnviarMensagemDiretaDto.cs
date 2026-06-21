namespace pi_projetolivros.DTO.DM;

public class EnviarMensagemDiretaDto
{
    public int DestinatarioId { get; set; }
    public string Conteudo { get; set; } = string.Empty;
}
