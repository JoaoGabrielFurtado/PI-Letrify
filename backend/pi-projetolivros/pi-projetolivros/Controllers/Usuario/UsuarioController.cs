using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.DTO;
using pi_projetolivros.DTO.SituacaoLivros;
using pi_projetolivros.DTO.Usuario;
using pi_projetolivros.Models;
using pi_projetolivros.Models.Banco;
using pi_projetolivros.Servicos;
using pi_projetolivros_banco;
using System.Security.Claims;

namespace pi_projetolivros.Controllers.Usuario;

[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly Banco _contexto;
    private readonly CloudinaryService _storageService;

    public UsuarioController(Banco contexto, CloudinaryService storageService)
    {
        _contexto = contexto;
        _storageService = storageService;
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

    [HttpGet("usuariosPorNome")]
    public async Task<IActionResult> RetornaUsuariosPorNome(
        [FromQuery] string nome,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return BadRequest(new { erro = "O nome para pesquisa não pode estar vazio." });

        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1 || tamanhoPagina > 50) tamanhoPagina = 10;

        var nomeTratado = nome.Trim();

        var query = _contexto.Usuarios
            .AsNoTracking()
            .Where(u => u.Nome.Contains(nomeTratado)); 

        var totalEncontrados = await query.CountAsync();

        var usuarios = await query
            .OrderBy(u => u.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Cidade,
                u.FotoPerfil
            })
            .ToListAsync();

        var temMais = (pagina * tamanhoPagina) < totalEncontrados;

        return Ok(new
        {
            resultados = usuarios,
            paginaAtual = pagina,
            totalEncontrados,
            temMais        
        });
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
            if (dto.Foto != null && dto.Foto.Length > 0)
            {
                await _storageService.DeletarAsync(usuario.FotoPerfil);
                usuario.FotoPerfil = await _storageService.UploadFotoPerfilAsync(dto.Foto);
            }
        }

        _contexto.Usuarios.Update(usuario);
        await _contexto.SaveChangesAsync();

        return Ok(new { message = "Perfil atualizado com sucesso!" });
    }

    [HttpPut("tornar-premium")]
    [Authorize]
    public async Task<IActionResult> TornarPremium()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario == null)
            return NotFound("Usuário não encontrado no banco de dados.");

        if (usuario.Premium)
            return BadRequest(new { erro = "Você já é premium!" });

        usuario.Premium = true;
        await _contexto.SaveChangesAsync();

        return Ok(new { message = "Agora você é premium!" });
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

        else if (string.IsNullOrWhiteSpace(livroLocal.Temas) && !string.IsNullOrWhiteSpace(dto.Temas))
        {
            livroLocal.Temas = dto.Temas;
            _contexto.Livros.Update(livroLocal);
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

            await _contexto.SaveChangesAsync();
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


    [HttpGet("informacoes/{id?}")]
    [Authorize]
    public async Task<IActionResult> InformacoesUsuario(int? id)
    {
        int targetUserId;
        if (id.HasValue && id.Value > 0)
        {
            targetUserId = id.Value; 
        }
        else
        {
        var usuarioIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(usuarioIdText, out targetUserId))
                return Unauthorized(new { mensagem = "Token inválido." }); 
        }

        var usuario = await _contexto.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == targetUserId);

        if (usuario == null)
            return NotFound(new { mensagem = "Usuário não encontrado." });

        bool p = await _contexto.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == targetUserId)
            .Select(u => u.Premium)
            .FirstOrDefaultAsync();

        string pm = p ? "1" : "0";

        var livrosDoUsuario = await _contexto.SituacaoLivros
            .AsNoTracking()
        .Include(l => l.Livro)
            .Where(u => u.UsuarioId == targetUserId && u.Livro != null)
        .Select(l => l.Livro)
        .ToListAsync();

        var livroFavorito = await _contexto.Favoritos
            .AsNoTracking()
            .Include(f => f.Livro)
            .FirstOrDefaultAsync(l => l.UsuarioId == targetUserId);

        var temasContagem = livrosDoUsuario
            .Where(l => !string.IsNullOrEmpty(l.Temas)) 
            .SelectMany(l => l.Temas.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()))  
            .Where(tema => tema != "Sem Temas") 
            .GroupBy(palavra => palavra) 
            .Select(grupo => new
            {
                Tema = grupo.Key,
                Quantidade = grupo.Count()
            })
            .OrderByDescending(item => item.Quantidade)
            .Take(10) 
            .ToList();

        var autoresContagem = livrosDoUsuario
        .Where(l => !string.IsNullOrEmpty(l.Autor))
        .SelectMany(l => l.Autor.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(t => t.Trim())
        .GroupBy(palavra => palavra)
        .OrderByDescending(grupo => grupo.Count())
        .Select(grupo => new {
            Autor = grupo.Key,
            Quantidade = grupo.Count()
        })
        .Take(3)
        .ToList();

        var situacoesContadas = await _contexto.SituacaoLivros
            .AsNoTracking()
            .Where(u => u.UsuarioId == targetUserId && u.Livro != null)
            .GroupBy(u => u.Status) 
            .Select(g => new
            {
                Situacao = g.Key,
                Quantidade = g.Count() 
            })
            .OrderByDescending(x => x.Quantidade)
            .ToListAsync();

        var totalSeguidores = await _contexto.Seguidores
            .AsNoTracking()
            .CountAsync(s => s.SeguidoId == targetUserId);

        var totalSeguindo = await _contexto.Seguidores
            .AsNoTracking()
            .CountAsync(s => s.SeguidorId == targetUserId);

        var totalLivros = livrosDoUsuario.Count;

        return Ok(new
        {
            perfil = new
            {
                nome = usuario.Nome,
                foto = usuario.FotoPerfil,
                seguidores = totalSeguidores,
                seguindo = totalSeguindo,
                premium = pm
            },
            estatisticas = new
            {
                totalDeLivros = totalLivros,
                situacoes = situacoesContadas,
                topTemas = temasContagem,
                topAutores = autoresContagem
            },
            favorito = livroFavorito != null ? new
            {
                id = livroFavorito.Livro.Id,
                titulo = livroFavorito.Livro.Titulo,
                autor = livroFavorito.Livro.Autor,
            } : null
        });

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
            await _storageService.DeletarAsync(usuario.FotoPerfil);

            _contexto.Usuarios.Remove(usuario);
            await _contexto.SaveChangesAsync();
        }

        _contexto.Usuarios.Remove(usuario);
        await _contexto.SaveChangesAsync();

        return Ok(new { message = "Conta do usuário e seus dados foram removidos com sucesso." });
    }

}