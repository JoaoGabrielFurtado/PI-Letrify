using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.DTO.Usuario;
using pi_projetolivros.Servicos;
using pi_projetolivros_banco;
using System.Security.Claims;

namespace pi_projetolivros.Controllers.Usuario;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SeguidoresController : ControllerBase
{
    private readonly Banco _contexto;
    private readonly NotificacaoService _notificacaoService; 

    public SeguidoresController(Banco contexto, IConfiguration configuration, NotificacaoService notificacaoService)
    {
        _contexto = contexto;
        _notificacaoService = notificacaoService; 
    }

    [HttpPost("seguir/{idSeguido}")]
    public async Task<IActionResult> SeguirEPararDeSeguir([FromRoute] int idSeguido)
    {
        var idSeguidor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idSeguidor, out int SeguidorId))
            return Unauthorized(new { erro = "Token inválido ou não encontrado." });

        if (SeguidorId == idSeguido)
            return BadRequest(new { erro = "Você não pode seguir a si mesmo." });

        var usuarioSeguido = await _contexto.Usuarios.FindAsync(idSeguido);
        if (usuarioSeguido == null)
            return NotFound(new { erro = "Usuário a ser seguido não encontrado." });

        var consultaSeguidor = await _contexto.Seguidores
            .FirstOrDefaultAsync(s => s.SeguidorId == SeguidorId && s.SeguidoId == idSeguido);

        if (consultaSeguidor != null)
        {
            _contexto.Seguidores.Remove(consultaSeguidor);
            await _contexto.SaveChangesAsync();
            return Ok(new { message = "Você deixou de seguir esta pessoa!" });
        }
        else
        {
            _contexto.Seguidores.Add(new Seguidor
            {
                SeguidorId = SeguidorId,
                SeguidoId = idSeguido,
                DataSeguimento = DateTime.UtcNow
            });
            await _contexto.SaveChangesAsync();

            var nomeSeguidor = await _contexto.Usuarios
                .Where(u => u.Id == SeguidorId)
                .Select(u => u.Nome)
                .FirstOrDefaultAsync();

            await _notificacaoService.EnviarAsync(
                usuarioDestinoId: idSeguido,
                tipo: "Seguidor",
                conteudo: $"{nomeSeguidor} começou a seguir você."
            );

            return Ok(new { message = "Você começou a seguir esta pessoa!" });
        }
    }

    [HttpGet("seguidores/{id?}")]
    public async Task<IActionResult> RetornaSeguidores(int? id)
    {
        int alvo;

        if (id.HasValue && id.Value > 0)
        {
            alvo = id.Value;
        }
        else
        {
            var idLogado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idLogado, out alvo))
                return Unauthorized(new { erro = "Token inválido ou não encontrado." });
        }

        var osSeguidores = await _contexto.Seguidores
            .AsNoTracking()
            .Include(s => s.SeguidorUsuario)
            .Where(s => s.SeguidoId == alvo)
            .Select(s => new
            {
                Id = s.SeguidorUsuario.Id,
                Nome = s.SeguidorUsuario.Nome,
                FotoPerfil = s.SeguidorUsuario.FotoPerfil
            })
            .ToListAsync();

        return Ok(osSeguidores);
    }

    [HttpGet("seguindo/{id?}")]
    public async Task<IActionResult> RetornaSeguindo(int? id)
    {
        int alvo;

        if (id.HasValue && id.Value > 0)
        {
            alvo = id.Value;
        }
        else
        {
            var idLogado = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idLogado, out alvo))
                return Unauthorized(new { erro = "Token inválido ou não encontrado." });
        }

        var pessoasSeguidas = await _contexto.Seguidores
            .AsNoTracking()
            .Include(s => s.SeguidoUsuario)
            .Where(s => s.SeguidorId == alvo)
            .Select(s => new
            {
                Id = s.SeguidoUsuario.Id,
                Nome = s.SeguidoUsuario.Nome,
                FotoPerfil = s.SeguidoUsuario.FotoPerfil
            })
            .ToListAsync();

        return Ok(pessoasSeguidas);
    }
}