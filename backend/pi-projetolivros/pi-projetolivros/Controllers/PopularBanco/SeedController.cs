using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using pi_projetolivros.Models;
using pi_projetolivros.Models.Banco;
using pi_projetolivros.Servicos;
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
    private readonly CloudinaryService _cloudinaryService;

    public SeedController(Banco contexto, GeminiServices geminiService, QdrantServices qdrantService, IConfiguration configuration, CloudinaryService cloudinaryService)
    {
        _contexto = contexto;
        _geminiService = geminiService;
        _qdrantService = qdrantService;
        _configuration = configuration;
        _cloudinaryService = cloudinaryService;
    }

    private static readonly List<string> FotosSeed = new()
{
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421166/fotos-perfil/xzvzgrcfmhze9538c2fq.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421168/fotos-perfil/qc9o0dmwfd22jfnjvjdq.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421169/fotos-perfil/d3lfjfurlyq21ecdny4m.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421170/fotos-perfil/yahztkeeorseyk9u4xsm.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421171/fotos-perfil/pnidmr0gy8wijyakquuo.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421172/fotos-perfil/hqlqjzqihvtfuoie0pu9.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421173/fotos-perfil/hcp2pjw9xdf8mpvn5udv.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421174/fotos-perfil/gnalrzuk0cgkkukknii2.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421175/fotos-perfil/pznlbvru8jqwujegs9x4.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421176/fotos-perfil/al9je7rgirmgasn2rwb9.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421177/fotos-perfil/nowyqavzva1vpnosh2nr.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421178/fotos-perfil/g1cr3nf9o7squ8wxtemb.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421178/fotos-perfil/hss9wpht0mfbbdhhfhjo.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421179/fotos-perfil/a4qhkbplo9gznuta4kqd.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421180/fotos-perfil/mtngu6pwlnmv6uqd2ptz.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421181/fotos-perfil/bmij1cjtbbrpcugvfg99.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421182/fotos-perfil/pvts1karmgd61uwakbrl.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421182/fotos-perfil/nws2zmqwupurnlubxwxd.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421183/fotos-perfil/e2k3zadhqczd0ovdoyg4.jpg",
    "https://res.cloudinary.com/dfycbkm67/image/upload/v1779421184/fotos-perfil/zjjmcevxxqpuqc2y74cq.jpg"
};

    public class TriboData
    {
        public string Prompt { get; set; }
        public List<Livro> Livros { get; set; }
        public float[] VetorCache { get; set; }
    }

    [HttpPost("upload-fotos-seed")]
    public async Task<IActionResult> UploadFotosSeed([FromForm] List<IFormFile> fotos)
    {
        var chaveEnviada = Request.Headers["X-Chave-Mestra"].FirstOrDefault();
        if (chaveEnviada != _configuration["SegurancaDaApi:ChaveMestraSeed"])
            return Unauthorized(new { erro = "Acesso negado." });

        var urls = new List<string>();
        foreach (var foto in fotos)
        {
            var url = await _cloudinaryService.UploadFotoPerfilAsync(foto);
            urls.Add(url);
        }

        return Ok(new { mensagem = $"{urls.Count} fotos enviadas.", urls });
    }

    [HttpPost("gerar/{quantidade}")]
    public async Task<IActionResult> GerarUsuarios(int quantidade)
    {
        var chaveEnviada = Request.Headers["X-Chave-Mestra"].FirstOrDefault();
        if (chaveEnviada != _configuration["SegurancaDaApi:ChaveMestraSeed"])
            return Unauthorized(new { erro = "Acesso negado. Você não possui permissão de Arquiteto para rodar a esteira de dados." });

        var tribos = new List<TriboData>
        {
            new TriboData
            {
                Prompt = "Temas Favoritos: Ficção Científica, Espaço, Distopia. Autores Favoritos: Isaac Asimov, Frank Herbert, Arthur C. Clarke.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "Duna", Autor = "Frank Herbert", Temas = "Ficção Científica, Espaço, Política, Distopia", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Fundação", Autor = "Isaac Asimov", Temas = "Ficção Científica, Espaço, Política", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "2001: Uma Odisseia no Espaço", Autor = "Arthur C. Clarke", Temas = "Ficção Científica, Espaço, Filosofia", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Neuromancer", Autor = "William Gibson", Temas = "Ficção Científica, Cyberpunk, Tecnologia", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Guia do Mochileiro das Galáxias", Autor = "Douglas Adams", Temas = "Ficção Científica, Humor, Espaço", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                }
            },
            new TriboData
            {
                Prompt = "Temas Favoritos: Romance, Época, Drama. Autores Favoritos: Jane Austen, Emily Brontë, Gustave Flaubert.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "Orgulho e Preconceito", Autor = "Jane Austen", Temas = "Romance, Época, Drama, Clássico", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Morro dos Ventos Uivantes", Autor = "Emily Brontë", Temas = "Romance, Drama, Tragédia", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Madame Bovary", Autor = "Gustave Flaubert", Temas = "Romance, Drama, Realismo", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Anna Karenina", Autor = "Liev Tolstói", Temas = "Romance, Drama, Sociedade", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Razão e Sensibilidade", Autor = "Jane Austen", Temas = "Romance, Época, Clássico", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                }
            },
            new TriboData
            {
                Prompt = "Temas Favoritos: Terror, Suspense, Sobrenatural, Psicológico. Autores Favoritos: Stephen King, Edgar Allan Poe, Shirley Jackson.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "O Iluminado", Autor = "Stephen King", Temas = "Terror, Suspense", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Corvo", Autor = "Edgar Allan Poe", Temas = "Terror, Poesia", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "A Maldição de Hill House", Autor = "Shirley Jackson", Temas = "Terror, Sobrenatural, Psicológico", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "It — A Coisa", Autor = "Stephen King", Temas = "Terror, Suspense, Sobrenatural", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Drácula", Autor = "Bram Stoker", Temas = "Terror, Sobrenatural, Vampiros", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                }
            },
            new TriboData
            {
                Prompt = "Temas Favoritos: Fantasia, Magia, Aventura, Épico. Autores Favoritos: J.R.R. Tolkien, J.K. Rowling, George R.R. Martin.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "O Senhor dos Anéis", Autor = "J.R.R. Tolkien", Temas = "Fantasia, Aventura, Épico", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Harry Potter e a Pedra Filosofal", Autor = "J.K. Rowling", Temas = "Fantasia, Magia, Aventura", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "A Guerra dos Tronos", Autor = "George R.R. Martin", Temas = "Fantasia, Política, Épico", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Nome do Vento", Autor = "Patrick Rothfuss", Temas = "Fantasia, Magia, Aventura", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Hobbit", Autor = "J.R.R. Tolkien", Temas = "Fantasia, Aventura, Clássico", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                }
            },
            new TriboData
            {
                Prompt = "Temas Favoritos: Negócios, Finanças, Desenvolvimento Pessoal, Produtividade. Autores Favoritos: Robert Kiyosaki, Napoleon Hill, Dale Carnegie.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "Pai Rico, Pai Pobre", Autor = "Robert Kiyosaki", Temas = "Negócios, Finanças", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Pense e Enriqueça", Autor = "Napoleon Hill", Temas = "Desenvolvimento Pessoal, Finanças", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Como Fazer Amigos e Influenciar Pessoas", Autor = "Dale Carnegie", Temas = "Desenvolvimento Pessoal, Comunicação", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Poder do Hábito", Autor = "Charles Duhigg", Temas = "Produtividade, Psicologia, Desenvolvimento Pessoal", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "A Startup Enxuta", Autor = "Eric Ries", Temas = "Negócios, Empreendedorismo, Inovação", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                }
            },
            new TriboData
            {
                Prompt = "Temas Favoritos: Filosofia, Estoicismo, Existencialismo. Autores Favoritos: Marco Aurélio, Albert Camus, Friedrich Nietzsche.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "Meditações", Autor = "Marco Aurélio", Temas = "Filosofia, Estoicismo", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Mito de Sísifo", Autor = "Albert Camus", Temas = "Filosofia, Existencialismo", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Assim Falou Zaratustra", Autor = "Friedrich Nietzsche", Temas = "Filosofia, Existencialismo", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "A República", Autor = "Platão", Temas = "Filosofia, Política, Ética", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Ser e o Nada", Autor = "Jean-Paul Sartre", Temas = "Filosofia, Existencialismo", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                }
            },
            new TriboData
            {
                Prompt = "Temas Favoritos: Literatura Brasileira, Realismo, Regionalismo. Autores Favoritos: Machado de Assis, Clarice Lispector, Guimarães Rosa.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "Dom Casmurro", Autor = "Machado de Assis", Temas = "Literatura Brasileira, Realismo, Romance", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "A Hora da Estrela", Autor = "Clarice Lispector", Temas = "Literatura Brasileira, Existencialismo, Drama", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Grande Sertão: Veredas", Autor = "Guimarães Rosa", Temas = "Literatura Brasileira, Regionalismo, Aventura", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Memórias Póstumas de Brás Cubas", Autor = "Machado de Assis", Temas = "Literatura Brasileira, Realismo, Sátira", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Vidas Secas", Autor = "Graciliano Ramos", Temas = "Literatura Brasileira, Regionalismo, Drama", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                }
            },
            new TriboData
            {
                Prompt = "Temas Favoritos: Distopia, Política, Sociedade, Futuro. Autores Favoritos: George Orwell, Aldous Huxley, Margaret Atwood.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "1984", Autor = "George Orwell", Temas = "Distopia, Política, Sociedade", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Admirável Mundo Novo", Autor = "Aldous Huxley", Temas = "Distopia, Ficção Científica, Sociedade", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Conto da Aia", Autor = "Margaret Atwood", Temas = "Distopia, Feminismo, Política", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Fahrenheit 451", Autor = "Ray Bradbury", Temas = "Distopia, Censura, Sociedade", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "A Revolução dos Bichos", Autor = "George Orwell", Temas = "Distopia, Política, Sátira", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                }
            },
            new TriboData
            {
                Prompt = "Temas Favoritos: Mistério, Crime, Thriller, Investigação. Autores Favoritos: Agatha Christie, Arthur Conan Doyle, Gillian Flynn.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "E Não Sobrou Nenhum", Autor = "Agatha Christie", Temas = "Mistério, Crime, Suspense", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Cão dos Baskervilles", Autor = "Arthur Conan Doyle", Temas = "Mistério, Investigação, Aventura", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Garota Exemplar", Autor = "Gillian Flynn", Temas = "Thriller, Suspense, Psicológico", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Silêncio dos Inocentes", Autor = "Thomas Harris", Temas = "Thriller, Crime, Psicológico", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Os Homens que Não Amavam as Mulheres", Autor = "Stieg Larsson", Temas = "Thriller, Crime, Investigação", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                }
            },
            new TriboData
            {
                Prompt = "Temas Favoritos: Psicologia, Comportamento Humano, Ciência. Autores Favoritos: Daniel Kahneman, Viktor Frankl, Yuval Noah Harari.",
                Livros = new List<Livro>
                {
                    new Livro { Titulo = "Rápido e Devagar", Autor = "Daniel Kahneman", Temas = "Psicologia, Comportamento, Ciência", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Em Busca de Sentido", Autor = "Viktor Frankl", Temas = "Psicologia, Filosofia, Autobiografia", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Sapiens", Autor = "Yuval Noah Harari", Temas = "História, Ciência, Antropologia", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "O Homem em Busca de Sentido", Autor = "Viktor Frankl", Temas = "Psicologia, Filosofia", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                    new Livro { Titulo = "Homo Deus", Autor = "Yuval Noah Harari", Temas = "Futuro, Tecnologia, Filosofia", Isbn = $"Sem ISBN - {Guid.NewGuid().ToString()[..8]}" },
                }
            },
        };

        foreach (var tribo in tribos)
        {
            await _contexto.Livros.AddRangeAsync(tribo.Livros);
            tribo.VetorCache = await _geminiService.ObterEmbeddingAsync(tribo.Prompt);
        }
        await _contexto.SaveChangesAsync();

        await _qdrantService.InicializarColecaoAsync();

        var random = new Random();

        var fakerUsuarios = new Faker<Models.Banco.Usuario>("pt_BR")
            .RuleFor(u => u.Nome, f => f.Name.FullName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Nome).ToLower())
            .RuleFor(u => u.Cidade, f => f.Address.City())
            .RuleFor(u => u.Senha, f => "123456")
            .RuleFor(u => u.FotoPerfil, f => FotosSeed.Any()
                ? FotosSeed[random.Next(FotosSeed.Count)]
                : null);

        int usuariosCriados = 0;

        for (int i = 0; i < quantidade; i++)
        {
            var novoUsuario = fakerUsuarios.Generate();
            await _contexto.Usuarios.AddAsync(novoUsuario);
            await _contexto.SaveChangesAsync();

            var indicesPrincipais = Enumerable.Range(0, tribos.Count)
                .OrderBy(_ => random.Next())
                .Take(random.Next(1, 3))
                .ToList();

            var livrosDoUsuario = indicesPrincipais
                .SelectMany(idx => tribos[idx].Livros)
                .DistinctBy(l => l.Titulo)
                .ToList();

            foreach (var livro in livrosDoUsuario)
            {
                var statusOpcoes = new[] { "Lido", "Lido", "Lendo", "Quero Ler" }; 
                var situacao = new SituacaoLivro
                {
                    UsuarioId = novoUsuario.Id,
                    LivroId = livro.Id,
                    Status = statusOpcoes[random.Next(statusOpcoes.Length)]
                };
                await _contexto.SituacaoLivros.AddAsync(situacao);
            }
            await _contexto.SaveChangesAsync();


            var vetorFinal = indicesPrincipais
                .Select(idx => tribos[idx].VetorCache)
                .Aggregate((a, b) => a.Zip(b, (x, y) => x + y).ToArray())
                .Select(v => v / indicesPrincipais.Count)
                .ToArray();

            await _qdrantService.SalvarVetorUsuarioAsync(novoUsuario.Id, vetorFinal);

            usuariosCriados++;
        }

        return Ok(new
        {
            mensagem = $"Sucesso! {usuariosCriados} usuários criados com {tribos.Count} tribos literárias, fotos de perfil e indexados no Qdrant.",
            aviso = $"Foram consumidas apenas {tribos.Count} requisições do Gemini para popular o banco inteiro!"
        });
    }
}