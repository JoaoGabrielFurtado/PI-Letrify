using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.DTO.SituacaoLivros;
using pi_projetolivros.Models.Banco;
using pi_projetolivros_banco;
using System.Security.Claims;

namespace pi_projetolivros.Controllers.Usuario;

[ApiController]
[Route("api/[controller]")]
public class FavoritosController : ControllerBase
{
    private readonly Banco _contexto;

    public FavoritosController(Banco contexto)
    {
        _contexto = contexto;
    }

    [HttpPost("add")]
    [Authorize]
    public async Task<IActionResult> AdicionarLivroFavoritos([FromBody] AdicionarLivroColecaoDto dto)
    {
        if (dto == null)
            return BadRequest(new { erro = "É preciso enviar os dados do livro para adicionar aos favoritos." });

        var usuarioIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdText, out int usuarioId))
            return Unauthorized(new { erro = "Token inválido ou não encontrado." });

        string nomeLivro = dto.Titulo;

        var livroLocal = await _contexto.Livros
                .FirstOrDefaultAsync(l =>
                    (l.Titulo == dto.Titulo && l.Autor == dto.Autor) ||
                    (!string.IsNullOrWhiteSpace(dto.Isbn) && dto.Isbn != "Sem ISBN" && l.Isbn == dto.Isbn)
                );

        if (livroLocal == null)
        {
            livroLocal = new Livro
            {
                Titulo = dto.Titulo,
                Autor = string.IsNullOrWhiteSpace(dto.Autor) ? "Autor Desconhecido" : dto.Autor,
                Isbn = string.IsNullOrWhiteSpace(dto.Isbn) ? $"Sem ISBN - {Guid.NewGuid().ToString().Substring(0, 8)}" : dto.Isbn,
                Temas = string.IsNullOrWhiteSpace(dto.Temas) ? "Sem Temas" : dto.Temas
            };

            await _contexto.Livros.AddAsync(livroLocal);

            await _contexto.SaveChangesAsync();
        }

        var favoritoExistente = await _contexto.Favoritos
                    .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId);

        if (favoritoExistente != null)
        {
            if (favoritoExistente.LivroId == livroLocal.Id)
                return BadRequest(new { erro = "Este livro já é o seu favorito atual." });

            favoritoExistente.LivroId = livroLocal.Id;
            favoritoExistente.DataFavoritado = DateTime.Now;

            _contexto.Favoritos.Update(favoritoExistente);
            await _contexto.SaveChangesAsync();

            return Ok(new { message = "Seu livro favorito foi atualizado com sucesso!" });
        }

        var novoFavorito = new Favorito
        {
            UsuarioId = usuarioId,
            LivroId = livroLocal.Id,
            DataFavoritado = DateTime.Now
        };

        await _contexto.Favoritos.AddAsync(novoFavorito);
        await _contexto.SaveChangesAsync();

        return Ok(new { message = $"Livro \"{nomeLivro}\" salvo como favorito com sucesso!" }); 

    }

    [HttpDelete("excluir")]
    [Authorize]
    public async Task<IActionResult> RemoverFavorito()
    {
        var usuarioIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdText, out int usuarioId))
            return Unauthorized(new { erro = "Token inválido ou não encontrado." });

        var livroFavorito = await _contexto.Favoritos.FirstOrDefaultAsync(l => l.UsuarioId == usuarioId);

        if (livroFavorito == null)
            return NotFound(new { erro = "Você não possui nenhum livro favoritado." });

        _contexto.Favoritos.Remove(livroFavorito);
        await _contexto.SaveChangesAsync();

        return Ok(new { message = "Livro favorito removido com sucesso!" });
    }


}
