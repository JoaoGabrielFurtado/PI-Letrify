using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.DTO;
using pi_projetolivros.DTO.SituacaoLivros;
using pi_projetolivros.DTO.Usuario;
using pi_projetolivros.Models;
using pi_projetolivros.Models.Banco;
using pi_projetolivros_banco;
using System.Security.Claims;

namespace pi_projetolivros.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly Banco _contexto;
    public UsuarioController(Banco contexto)
    {
        _contexto = contexto;
    }

    // Ver o perfil de qualquer usuário
    [HttpGet("{id}")]
    public async Task<IActionResult> RetornaUsuarioComId(int id)
    {
        var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null)
            return NotFound("Usuário não encontrado.");

        var perfilPublico = new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Idade,
            usuario.Cidade,
            usuario.Descricao,
            usuario.FotoPerfil 
        };

        return Ok(perfilPublico);
    }


    // Usuario editar o proprio perfil
    [HttpPut("editar")]
    [Authorize] 
    public async Task<IActionResult> EditarUsuario([FromForm] EditarPerfilDto dto)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario == null)
            return NotFound("Usuário não encontrado no banco de dados.");

        if (dto.Idade.HasValue) usuario.Idade = dto.Idade.Value;
        if (!string.IsNullOrEmpty(dto.Cidade)) usuario.Cidade = dto.Cidade;
        if (!string.IsNullOrEmpty(dto.Descricao)) usuario.Descricao = dto.Descricao;

        if (dto.Foto != null && dto.Foto.Length > 0)
        {
            var extensao = Path.GetExtension(dto.Foto.FileName);

            var nomeArquivoUnico = Guid.NewGuid().ToString() + extensao;

            var caminhoFisico = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fotos", nomeArquivoUnico);

            using (var stream = new FileStream(caminhoFisico, FileMode.Create))
            {
                await dto.Foto.CopyToAsync(stream);
            }

            usuario.FotoPerfil = "/fotos/" + nomeArquivoUnico;
        }

        _contexto.Usuarios.Update(usuario);
        await _contexto.SaveChangesAsync();

        return Ok(new { message = "Perfil atualizado com sucesso!" });
    }

    [HttpPost("meus-livros")]
    [Authorize]
    public async Task<IActionResult> AdicionarLivroColecao([FromBody] AdicionarLivroColecaoDto dto)
    {
        var usuarioIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdText, out int usuarioId))
            return Unauthorized("Token inválido.");

        var statusPermitidos = new List<string> { "Lendo", "Lido", "Quero Ler" };
        if (!statusPermitidos.Contains(dto.Status))
            return BadRequest("Status inválido. Use apenas: 'Lendo', 'Lido' ou 'Quero Ler'.");

        // Busca por Titulo+Autor OU por ISBN (evita violação da constraint unique no isbn)
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
                Isbn = string.IsNullOrWhiteSpace(dto.Isbn) ? "Sem ISBN" : dto.Isbn
            };

            await _contexto.Livros.AddAsync(livroLocal);
            await _contexto.SaveChangesAsync();
        }

        var situacaoExistente = await _contexto.SituacaoLivros
            .FirstOrDefaultAsync(s => s.UsuarioId == usuarioId && s.LivroId == livroLocal.Id);

        if (situacaoExistente != null)
        {
            situacaoExistente.Status = dto.Status;
            situacaoExistente.DataAtualizacao = DateTime.Now;
            _contexto.SituacaoLivros.Update(situacaoExistente);
        }
        else
        {
            var novaSituacao = new SituacaoLivro
            {
                UsuarioId = usuarioId,
                LivroId = livroLocal.Id, 
                Status = dto.Status,
                DataAtualizacao = DateTime.Now
            };
            await _contexto.SituacaoLivros.AddAsync(novaSituacao);
        }

        await _contexto.SaveChangesAsync();

        return Ok(new { message = $"Livro '{livroLocal.Titulo}' movido para '{dto.Status}' com sucesso!" });
    }


    [HttpGet("{id}/livros")]
    public async Task<IActionResult> RetornaEstanteDoUsuario(int id)
    {
        var usuarioExiste = await _contexto.Usuarios.AnyAsync(u => u.Id == id);
        if (!usuarioExiste)
            return NotFound("Usuário não encontrado.");

        var situacoes = await _contexto.SituacaoLivros
            .Include(s => s.Livro)
            .Where(s => s.UsuarioId == id)
            .ToListAsync();

        var estante = new
        {
            Lendo = situacoes
                .Where(s => s.Status == "Lendo")
                .Select(s => s.Livro),  

            Lido = situacoes
                .Where(s => s.Status == "Lido")
                .Select(s => s.Livro),

            QueroLer = situacoes
                .Where(s => s.Status == "Quero Ler")
                .Select(s => s.Livro)
        };

        return Ok(estante);
    }


    [HttpDelete("meus-livros/{livroId}")]
    [Authorize] 
    public async Task<IActionResult> RemoverLivroColecao(int livroId)
    {
        var usuarioIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdText, out int usuarioId))
            return Unauthorized("Token inválido.");

        var situacaoExistente = await _contexto.SituacaoLivros
            .FirstOrDefaultAsync(s => s.UsuarioId == usuarioId && s.LivroId == livroId);

        if (situacaoExistente == null)
            return NotFound("Este livro não foi encontrado na sua lista.");

        _contexto.SituacaoLivros.Remove(situacaoExistente);
        await _contexto.SaveChangesAsync();

        return Ok(new { message = "Livro removido da sua lista com sucesso!" });
    }

    [HttpDelete("deletar")]
    [Authorize]
    public async Task<IActionResult> DeletarUsuario()
    {
        var usuarioIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(usuarioIdText, out int usuarioId))
            return Unauthorized("Token inválido.");

        var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario == null)
            return NotFound("Usuário não encontrado ou já foi deletado.");

        if (!string.IsNullOrEmpty(usuario.FotoPerfil))
        {
            var nomeArquivo = usuario.FotoPerfil.Replace("/fotos/", "");

            var caminhoFisico = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fotos", nomeArquivo);

            if (System.IO.File.Exists(caminhoFisico))
            {
                System.IO.File.Delete(caminhoFisico);
            }
        }

        _contexto.Usuarios.Remove(usuario);
        await _contexto.SaveChangesAsync();

        return Ok(new { message = "Conta do usuário e seus dados foram removidos com sucesso." });
    }

}