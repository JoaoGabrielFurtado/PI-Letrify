using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using pi_projetolivros.Hubs;
using pi_projetolivros.Models.Banco;
using pi_projetolivros.Servicos;
using pi_projetolivros_banco;
using System.Security.Claims;

namespace pi_projetolivros.Controllers.Grupos;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GruposController : ControllerBase
{
    private readonly Banco _contexto;
    private readonly IHubContext<GrupoHub> _hubContext;
    private readonly NotificacaoService _notificacaoService;
    private readonly CloudinaryService _cloudinaryService; // CLOUDINARY

    public GruposController(Banco contexto, IHubContext<GrupoHub> hubContext, NotificacaoService notificacaoService, CloudinaryService cloudinaryService)
    {
        _contexto = contexto;
        _hubContext = hubContext;
        _notificacaoService = notificacaoService;
        _cloudinaryService = cloudinaryService; // CLOUDINARY
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Listar()
    {
        var grupos = await _contexto.Grupos
            .AsNoTracking()
            .Select(g => new
            {
                g.Id,
                g.Nome,
                g.Descricao,
                g.Status,
                g.FotoCapa,
                g.DataCriacao,
                Lider = new { g.Lider.Id, g.Lider.Nome, g.Lider.FotoPerfil },
                TotalMembros = g.Membros.Count
            })
            .ToListAsync();

        return Ok(grupos);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Detalhe([FromRoute] int id)
    {
        var grupo = await _contexto.Grupos
            .AsNoTracking()
            .Where(g => g.Id == id)
            .Select(g => new
            {
                g.Id,
                g.Nome,
                g.Descricao,
                g.Status,
                g.FotoCapa,
                g.DataCriacao,
                Lider = new { g.Lider.Id, g.Lider.Nome, g.Lider.FotoPerfil },
                Membros = g.Membros.Select(m => new
                {
                    m.Usuario.Id,
                    m.Usuario.Nome,
                    m.Usuario.FotoPerfil,
                    m.Role,
                    m.DataEntrada
                })
            })
            .FirstOrDefaultAsync();

        if (grupo == null)
            return NotFound(new { erro = "Grupo não encontrado." });

        return Ok(grupo);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromForm] CriarGrupoDto dto)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var grupo = new Grupo
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Status = dto.Status ?? "Aberto",
            LiderId = usuarioId.Value
        };

        // CLOUDINARY: substitui o Path.Combine + FileStream
        if (dto.Foto != null && dto.Foto.Length > 0)
            grupo.FotoCapa = await _cloudinaryService.UploadFotoGrupoAsync(dto.Foto);

        _contexto.Grupos.Add(grupo);
        await _contexto.SaveChangesAsync();

        _contexto.UsuarioGrupos.Add(new UsuarioGrupo
        {
            UsuarioId = usuarioId.Value,
            GrupoId = grupo.Id,
            Role = "Lider"
        });
        await _contexto.SaveChangesAsync();

        return Ok(new { mensagem = "Grupo criado com sucesso!", grupoId = grupo.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Editar([FromRoute] int id, [FromForm] EditarGrupoDto dto)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var grupo = await _contexto.Grupos.FindAsync(id);
        if (grupo == null) return NotFound(new { erro = "Grupo não encontrado." });

        if (grupo.LiderId != usuarioId.Value)
            return Forbid();

        if (!string.IsNullOrWhiteSpace(dto.Nome)) grupo.Nome = dto.Nome;
        if (!string.IsNullOrWhiteSpace(dto.Descricao)) grupo.Descricao = dto.Descricao;
        if (!string.IsNullOrWhiteSpace(dto.Status)) grupo.Status = dto.Status;

        // CLOUDINARY: deleta a capa antiga antes de subir a nova
        if (dto.Foto != null && dto.Foto.Length > 0)
        {
            await _cloudinaryService.DeletarAsync(grupo.FotoCapa);
            grupo.FotoCapa = await _cloudinaryService.UploadFotoGrupoAsync(dto.Foto);
        }

        await _contexto.SaveChangesAsync();
        return Ok(new { mensagem = "Grupo atualizado com sucesso!" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar([FromRoute] int id)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var grupo = await _contexto.Grupos.FindAsync(id);
        if (grupo == null) return NotFound(new { erro = "Grupo não encontrado." });

        if (grupo.LiderId != usuarioId.Value)
            return Forbid();

        // CLOUDINARY: remove a foto de capa antes de deletar o grupo
        await _cloudinaryService.DeletarAsync(grupo.FotoCapa);

        _contexto.Grupos.Remove(grupo);
        await _contexto.SaveChangesAsync();

        return Ok(new { mensagem = "Grupo deletado com sucesso." });
    }

    [HttpPost("{id}/entrar")]
    public async Task<IActionResult> Entrar([FromRoute] int id)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var grupo = await _contexto.Grupos.FindAsync(id);
        if (grupo == null) return NotFound(new { erro = "Grupo não encontrado." });

        var jaMembro = await _contexto.UsuarioGrupos
            .AnyAsync(ug => ug.UsuarioId == usuarioId && ug.GrupoId == id);

        if (jaMembro)
            return BadRequest(new { erro = "Você já é membro deste grupo." });

        if (grupo.Status == "Fechado")
        {
            var solicitacaoExistente = await _contexto.SolicitacoesGrupo
                .AnyAsync(s => s.UsuarioId == usuarioId && s.GrupoId == id && s.Status == "Pendente");

            if (solicitacaoExistente)
                return BadRequest(new { erro = "Você já possui uma solicitação pendente para este grupo." });

            _contexto.SolicitacoesGrupo.Add(new SolicitacaoGrupo
            {
                UsuarioId = usuarioId.Value,
                GrupoId = id
            });
            await _contexto.SaveChangesAsync();

            var nomeRequerente = await _contexto.Usuarios
                .Where(u => u.Id == usuarioId)
                .Select(u => u.Nome)
                .FirstOrDefaultAsync();

            await _notificacaoService.EnviarAsync(
                usuarioDestinoId: grupo.LiderId,
                tipo: "Mencao",
                conteudo: $"{nomeRequerente} solicitou entrar no grupo \"{grupo.Nome}\"."
            );

            return Ok(new { mensagem = "Solicitação enviada! Aguarde aprovação do líder." });
        }

        _contexto.UsuarioGrupos.Add(new UsuarioGrupo
        {
            UsuarioId = usuarioId.Value,
            GrupoId = id,
            Role = "Membro"
        });
        await _contexto.SaveChangesAsync();

        return Ok(new { mensagem = "Você entrou no grupo com sucesso!" });
    }

    [HttpPost("{id}/sair")]
    public async Task<IActionResult> Sair([FromRoute] int id)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var membro = await _contexto.UsuarioGrupos
            .FirstOrDefaultAsync(ug => ug.UsuarioId == usuarioId && ug.GrupoId == id);

        if (membro == null)
            return NotFound(new { erro = "Você não é membro deste grupo." });

        if (membro.Role == "Lider")
            return BadRequest(new { erro = "O líder não pode sair do grupo. Transfira a liderança ou delete o grupo." });

        _contexto.UsuarioGrupos.Remove(membro);
        await _contexto.SaveChangesAsync();

        return Ok(new { mensagem = "Você saiu do grupo." });
    }

    [HttpGet("{id}/solicitacoes")]
    public async Task<IActionResult> ListarSolicitacoes([FromRoute] int id)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        if (!await IsAdminOuLider(usuarioId.Value, id))
            return Forbid();

        var solicitacoes = await _contexto.SolicitacoesGrupo
            .AsNoTracking()
            .Where(s => s.GrupoId == id && s.Status == "Pendente")
            .Select(s => new
            {
                s.Id,
                s.DataSolicitacao,
                Usuario = new { s.Usuario.Id, s.Usuario.Nome, s.Usuario.FotoPerfil }
            })
            .ToListAsync();

        return Ok(solicitacoes);
    }

    [HttpPut("{id}/solicitacoes/{solicitacaoId}")]
    public async Task<IActionResult> ResponderSolicitacao(
        [FromRoute] int id,
        [FromRoute] int solicitacaoId,
        [FromBody] ResponderSolicitacaoDto dto)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        if (!await IsAdminOuLider(usuarioId.Value, id))
            return Forbid();

        var solicitacao = await _contexto.SolicitacoesGrupo
            .FirstOrDefaultAsync(s => s.Id == solicitacaoId && s.GrupoId == id);

        if (solicitacao == null || solicitacao.Status != "Pendente")
            return NotFound(new { erro = "Solicitação não encontrada ou já respondida." });

        var acao = dto.Aceitar ? "Aceita" : "Recusada";
        solicitacao.Status = acao;

        if (dto.Aceitar)
        {
            _contexto.UsuarioGrupos.Add(new UsuarioGrupo
            {
                UsuarioId = solicitacao.UsuarioId,
                GrupoId = id,
                Role = "Membro"
            });
        }

        await _contexto.SaveChangesAsync();

        var nomeGrupo = await _contexto.Grupos
            .Where(g => g.Id == id)
            .Select(g => g.Nome)
            .FirstOrDefaultAsync();

        var mensagemNotificacao = dto.Aceitar
            ? $"Sua solicitação para entrar no grupo \"{nomeGrupo}\" foi aceita!"
            : $"Sua solicitação para entrar no grupo \"{nomeGrupo}\" foi recusada.";

        await _notificacaoService.EnviarAsync(
            usuarioDestinoId: solicitacao.UsuarioId,
            tipo: "Mencao",
            conteudo: mensagemNotificacao
        );

        return Ok(new { mensagem = $"Solicitação {acao.ToLower()} com sucesso." });
    }

    [HttpPut("{id}/membros/{membroId}/role")]
    public async Task<IActionResult> AlterarRole(
        [FromRoute] int id,
        [FromRoute] int membroId,
        [FromBody] AlterarRoleDto dto)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var grupo = await _contexto.Grupos.FindAsync(id);
        if (grupo == null) return NotFound(new { erro = "Grupo não encontrado." });

        if (grupo.LiderId != usuarioId.Value)
            return Forbid();

        var membro = await _contexto.UsuarioGrupos
            .FirstOrDefaultAsync(ug => ug.UsuarioId == membroId && ug.GrupoId == id);

        if (membro == null)
            return NotFound(new { erro = "Membro não encontrado no grupo." });

        if (membro.Role == "Lider")
            return BadRequest(new { erro = "Não é possível alterar a role do líder." });

        var rolesPermitidas = new[] { "Admin", "Membro" };
        if (!rolesPermitidas.Contains(dto.Role))
            return BadRequest(new { erro = "Role inválida. Use 'Admin' ou 'Membro'." });

        membro.Role = dto.Role;
        await _contexto.SaveChangesAsync();

        return Ok(new { mensagem = $"Membro atualizado para {dto.Role} com sucesso." });
    }

    [HttpDelete("{id}/membros/{membroId}")]
    public async Task<IActionResult> RemoverMembro([FromRoute] int id, [FromRoute] int membroId)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        if (!await IsAdminOuLider(usuarioId.Value, id))
            return Forbid();

        var membro = await _contexto.UsuarioGrupos
            .FirstOrDefaultAsync(ug => ug.UsuarioId == membroId && ug.GrupoId == id);

        if (membro == null)
            return NotFound(new { erro = "Membro não encontrado no grupo." });

        if (membro.Role == "Lider")
            return BadRequest(new { erro = "Não é possível remover o líder." });

        _contexto.UsuarioGrupos.Remove(membro);
        await _contexto.SaveChangesAsync();

        return Ok(new { mensagem = "Membro removido do grupo." });
    }

    [HttpGet("{id}/posts")]
    public async Task<IActionResult> ListarPosts(
        [FromRoute] int id,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        if (!await IsMembro(usuarioId.Value, id))
            return Forbid();

        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1 || tamanhoPagina > 50) tamanhoPagina = 20;

        var posts = await _contexto.PostsGrupo
            .AsNoTracking()
            .Where(p => p.GrupoId == id && p.PostPaiId == null)
            .OrderByDescending(p => p.DataPostagem)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(p => new
            {
                p.Id,
                p.Conteudo,
                p.DataPostagem,
                Usuario = new { p.Usuario.Id, p.Usuario.Nome, p.Usuario.FotoPerfil },
                Respostas = p.Respostas
                    .OrderBy(r => r.DataPostagem)
                    .Select(r => new
                    {
                        r.Id,
                        r.Conteudo,
                        r.DataPostagem,
                        Usuario = new { r.Usuario.Id, r.Usuario.Nome, r.Usuario.FotoPerfil }
                    })
            })
            .ToListAsync();

        return Ok(posts);
    }

    [HttpPost("{id}/posts")]
    public async Task<IActionResult> CriarPost([FromRoute] int id, [FromBody] CriarPostGrupoDto dto)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        if (!await IsMembro(usuarioId.Value, id))
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.Conteudo) || dto.Conteudo.Length > 500)
            return BadRequest(new { erro = "Conteúdo inválido. Máximo 500 caracteres." });

        if (dto.PostPaiId.HasValue)
        {
            var paiExiste = await _contexto.PostsGrupo
                .AnyAsync(p => p.Id == dto.PostPaiId && p.GrupoId == id);

            if (!paiExiste)
                return NotFound(new { erro = "Post pai não encontrado." });
        }

        var post = new PostGrupo
        {
            GrupoId = id,
            UsuarioId = usuarioId.Value,
            Conteudo = dto.Conteudo,
            PostPaiId = dto.PostPaiId
        };

        _contexto.PostsGrupo.Add(post);
        await _contexto.SaveChangesAsync();

        return Ok(new { mensagem = "Post criado com sucesso!", postId = post.Id });
    }

    [HttpDelete("{id}/posts/{postId}")]
    public async Task<IActionResult> DeletarPost([FromRoute] int id, [FromRoute] int postId)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var post = await _contexto.PostsGrupo
            .Include(p => p.Respostas)
            .FirstOrDefaultAsync(p => p.Id == postId && p.GrupoId == id);

        if (post == null)
            return NotFound(new { erro = "Post não encontrado." });

        var ehDono = post.UsuarioId == usuarioId.Value;
        var ehAdminOuLider = await IsAdminOuLider(usuarioId.Value, id);

        if (!ehDono && !ehAdminOuLider)
            return Forbid();

        if (post.Respostas.Any())
            _contexto.PostsGrupo.RemoveRange(post.Respostas);

        _contexto.PostsGrupo.Remove(post);
        await _contexto.SaveChangesAsync();

        return Ok(new { mensagem = "Post deletado com sucesso." });
    }

    [HttpPost("{id}/chat")]
    public async Task<IActionResult> EnviarMensagem([FromRoute] int id, [FromBody] EnviarMensagemGrupoDto dto)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        if (!await IsMembro(usuarioId.Value, id))
            return Forbid();

        if (string.IsNullOrWhiteSpace(dto.Conteudo) || dto.Conteudo.Length > 300)
            return BadRequest(new { erro = "Conteúdo inválido. Máximo 300 caracteres." });

        var mensagem = new MensagemGrupo
        {
            GrupoId = id,
            UsuarioId = usuarioId.Value,
            Conteudo = dto.Conteudo
        };

        _contexto.MensagensGrupo.Add(mensagem);
        await _contexto.SaveChangesAsync();

        var remetente = await _contexto.Usuarios.FindAsync(usuarioId.Value);

        var payload = new
        {
            mensagem.Id,
            mensagem.Conteudo,
            mensagem.DataPostagem,
            Usuario = new { remetente.Id, remetente.Nome, remetente.FotoPerfil }
        };

        await _hubContext.Clients.Group($"grupo-{id}").SendAsync("ReceberMensagemGrupo", payload);

        return Ok(new { mensagem = "Mensagem enviada!", id = mensagem.Id });
    }

    [HttpGet("{id}/chat")]
    public async Task<IActionResult> HistoricoChat(
        [FromRoute] int id,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 50)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        if (!await IsMembro(usuarioId.Value, id))
            return Forbid();

        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1 || tamanhoPagina > 100) tamanhoPagina = 50;

        var mensagens = await _contexto.MensagensGrupo
            .AsNoTracking()
            .Where(m => m.GrupoId == id)
            .OrderByDescending(m => m.DataPostagem)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(m => new
            {
                m.Id,
                m.Conteudo,
                m.DataPostagem,
                Usuario = new { m.Usuario.Id, m.Usuario.Nome, m.Usuario.FotoPerfil }
            })
            .ToListAsync();

        return Ok(mensagens);
    }

    private int? ObterUsuarioId()
    {
        var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(valor, out int id) ? id : null;
    }

    private async Task<bool> IsMembro(int usuarioId, int grupoId) =>
        await _contexto.UsuarioGrupos
            .AnyAsync(ug => ug.UsuarioId == usuarioId && ug.GrupoId == grupoId);

    private async Task<bool> IsAdminOuLider(int usuarioId, int grupoId) =>
        await _contexto.UsuarioGrupos
            .AnyAsync(ug => ug.UsuarioId == usuarioId &&
                            ug.GrupoId == grupoId &&
                            (ug.Role == "Admin" || ug.Role == "Lider"));
}