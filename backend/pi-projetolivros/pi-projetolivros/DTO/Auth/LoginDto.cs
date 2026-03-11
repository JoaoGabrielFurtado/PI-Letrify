using System.ComponentModel.DataAnnotations;

namespace pi_projetolivros.DTO.Auth;
public class LoginDto
{
    [Required(ErrorMessage = "O campo Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "O formato do Email é inválido.")]
    public string Email { get; set; }
    public string Senha { get; set; }
}
