using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using pi_projetolivros.Models;
using pi_projetolivros.Models.Banco;
using pi_projetolivros.Serviços;
using pi_projetolivros_banco;

namespace pi_projetolivros.Controllers.PopularBanco;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly Banco _contexto;
    private readonly GeminiServices _geminiService;
    private readonly QdrantServices _qdrantService;
    private readonly IConfiguration _configuration;

    public SeedController(Banco contexto, GeminiServices geminiService, QdrantServices qdrantService, IConfiguration configuration)
    {
        _contexto = contexto;
        _geminiService = geminiService;
        _qdrantService = qdrantService;
        _configuration = configuration;
    }

    public class TriboData
    {
        public string Prompt { get; set; }
        public List<Livro> Livros { get; set; }
        public float[] VetorCache { get; set; }
    }

    [HttpPost("gerar/{quantidade}")]
    public async Task<IActionResult> GerarUsuarios(int quantidade)
    {
        var chaveEnviada = Request.Headers["X-Chave-Mestra"].FirstOrDefault();

        var chaveCorreta = _configuration["SegurancaDaApi:ChaveMestraSeed"];

        if (chaveEnviada != chaveCorreta)
        {
            return Unauthorized(new
            {
                erro = "Acesso negado. Você não possui permissão de Arquiteto para rodar a esteira de dados."
            });
        }


        var tribos = new List<TriboData>
        {
            new TriboData
            {
                Prompt = "Temas Favoritos: Ficção Científica, Espaço, Distopia. Autores Favoritos: Isaac Asimov, Frank Herbert.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "Duna", Autor = "Frank Herbert", Temas = "Ficção Científica, Espaço, Política, Distopia", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Fundação", Autor = "Isaac Asimov", Temas = "Ficção Científica, Espaço, Política", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" }
                }
            },
            new TriboData
            {
                Prompt = "Temas Favoritos: Romance, Época, Drama. Autores Favoritos: Jane Austen, Emily Brontë.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "Orgulho e Preconceito", Autor = "Jane Austen", Temas = "Romance, Época, Drama, Clássico", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Morro dos Ventos Uivantes", Autor = "Emily Brontë", Temas = "Romance, Drama, Tragédia", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" }
                }
            },
            new TriboData
            {
                Prompt = "Temas Favoritos: Terror, Suspense, Sobrenatural. Autores Favoritos: Stephen King, Edgar Allan Poe.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "O Iluminado", Autor = "Stephen King", Temas = "Terror, Suspense", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Corvo", Autor = "Edgar Allan Poe", Temas = "Terror, Poesia", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" }
                }
            },
            new TriboData
            {
                Prompt = "Temas Favoritos: Fantasia, Magia, Aventura. Autores Favoritos: J.R.R. Tolkien, J.K. Rowling.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "O Senhor dos Anéis", Autor = "J.R.R. Tolkien", Temas = "Fantasia, Aventura", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Harry Potter e a Pedra Filosofal", Autor = "J.K. Rowling", Temas = "Fantasia, Magia", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" }
                }
            },
            new TriboData
            {
                Prompt = "Temas Favoritos: Negócios, Finanças, Desenvolvimento Pessoal. Autores Favoritos: Robert Kiyosaki, Napoleon Hill.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "Pai Rico, Pai Pobre", Autor = "Robert Kiyosaki", Temas = "Negócios, Finanças", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Pense e Enriqueça", Autor = "Napoleon Hill", Temas = "Desenvolvimento Pessoal", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" }
                }
            }
        };

        foreach (var tribo in tribos)
        {
            await _contexto.Livros.AddRangeAsync(tribo.Livros);
            tribo.VetorCache = await _geminiService.ObterEmbeddingAsync(tribo.Prompt);
        }
        await _contexto.SaveChangesAsync(); 

        await _qdrantService.InicializarColecaoAsync();

        var fakerUsuarios = new Faker<Models.Banco.Usuario>("pt_BR")
            .RuleFor(u => u.Nome, f => f.Name.FullName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Nome).ToLower())
            .RuleFor(u => u.Senha, f => "123456"); 

        var random = new Random();
        int usuariosCriados = 0;

        for (int i = 0; i < quantidade; i++)
        {
            var novoUsuario = fakerUsuarios.Generate();
            await _contexto.Usuarios.AddAsync(novoUsuario);
            await _contexto.SaveChangesAsync();

            int indexSorteado = random.Next(tribos.Count);
            var triboSorteada = tribos[indexSorteado];

            foreach (var livro in triboSorteada.Livros)
            {
                var situacao = new SituacaoLivro
                {
                    UsuarioId = novoUsuario.Id,
                    LivroId = livro.Id,
                    Status = "Lido"
                };
                await _contexto.SituacaoLivros.AddAsync(situacao);
            }
            await _contexto.SaveChangesAsync();

            await _qdrantService.SalvarVetorUsuarioAsync(novoUsuario.Id, triboSorteada.VetorCache);

            usuariosCriados++;
        }

        return Ok(new
        {
            mensagem = $"Sucesso absoluto! {usuariosCriados} usuários foram criados, com livros vinculados na Azure, e indexados no Qdrant.",
            aviso = "Foram consumidas APENAS 5 requisições do Gemini para popular o banco inteiro!"
        });
    }
}