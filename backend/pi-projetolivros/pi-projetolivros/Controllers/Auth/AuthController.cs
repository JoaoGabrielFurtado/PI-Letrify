using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.DTO.Auth;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using pi_projetolivros_banco;
using pi_projetolivros.Models.Banco;

namespace pi_projetolivros.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly Banco _contexto;
    private readonly IConfiguration _configuration;

    public AuthController(Banco contexto, IConfiguration configuration)
    {
        _contexto = contexto;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Registro([FromBody] RegisterDto registroDto)
    {
        if (registroDto is null)
            return BadRequest();

        var emailUser = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == registroDto.Email);

        if (emailUser is not null)
            return BadRequest("E-mail já cadastrado");

        var senhaCriptografada = BCrypt.Net.BCrypt.HashPassword(registroDto.Senha);

        var novoUsuario = new Usuario 
        { 
            Nome = registroDto.Nome, 
            Email = registroDto.Email, 
            Senha = senhaCriptografada,
            
        };

        await _contexto.Usuarios.AddAsync(novoUsuario);
        await _contexto.SaveChangesAsync();

        return Ok(new { message = "Usuário criado com sucesso!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (loginDto is null)
            return BadRequest("Requisição inválida.");

        var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(d => d.Email == loginDto.Email);

        if (usuario is null)
        {
            return Unauthorized("Email ou senha inválidos.");
        }

        var senhaCorreta = BCrypt.Net.BCrypt.Verify(loginDto.Senha, usuario.Senha);

        if (!senhaCorreta)
        {
            return Unauthorized("Email ou senha inválidos.");
        }

        var tokenString = GenerateJwtToken(usuario);
        return Ok(new { token = tokenString }); 
    }

    private string GenerateJwtToken(Usuario user)
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.GivenName, user.Nome)
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
