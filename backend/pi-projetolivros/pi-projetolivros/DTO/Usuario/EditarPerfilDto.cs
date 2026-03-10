namespace pi_projetolivros.DTO.Usuario;
public class EditarPerfilDto
{
    public int? Idade { get; set; }
    public string? Cidade { get; set; }
    public string? Descricao { get; set; }
    public IFormFile? Foto { get; set; }
}
