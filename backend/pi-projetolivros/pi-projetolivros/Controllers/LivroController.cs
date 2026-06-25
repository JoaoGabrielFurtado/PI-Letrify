using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pi_projetolivros.DTO.ConectarApiLivros;
using pi_projetolivros.Models;
using pi_projetolivros.Models.Banco;
using pi_projetolivros.Servicos;
using pi_projetolivros_banco;
using System;
using System.Text.Json;

namespace pi_projetolivros.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LivroController : ControllerBase
{
    //private readonly Banco _contexto;
    private static readonly HttpClient _httpClient = new HttpClient();
    private readonly LivroIngestaoService _livroIngestaoService;
    private readonly QdrantServices _qdrantServices;
    private readonly GeminiServices _geminiServices;

    public LivroController(LivroIngestaoService livroIngestaoService, QdrantServices qdrantServices, GeminiServices geminiServices)
    {
        _livroIngestaoService = livroIngestaoService;
        _qdrantServices = qdrantServices;
        _geminiServices = geminiServices;
    }

    [HttpGet("buscar")]
    public async Task<ActionResult<List<LivroClasse>>> Buscar(
        [FromQuery] string? q = null,
        [FromQuery] string? titulo = null,
        [FromQuery] string? autor = null,
        [FromQuery] string? tema = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int quantidade = 20)
    {
        var parametros = new List<string>();

        if (!string.IsNullOrWhiteSpace(q))
            parametros.Add($"q={Uri.EscapeDataString(q.Trim())}");

        if (!string.IsNullOrWhiteSpace(titulo))
            parametros.Add($"title={Uri.EscapeDataString(titulo.Trim())}");

        if (!string.IsNullOrWhiteSpace(autor))
            parametros.Add($"author={Uri.EscapeDataString(autor.Trim())}");

        if (!string.IsNullOrWhiteSpace(tema))
            parametros.Add($"subject={Uri.EscapeDataString(tema.Trim().ToLower())}");

        if (!parametros.Any())
            return BadRequest("Informe ao menos um critério de busca: q, titulo, autor ou tema.");

        string campos = "title,author_name,first_publish_year,isbn,publisher,subject,cover_edition_key";
        string url = $"https://openlibrary.org/search.json?{string.Join("&", parametros)}&page={pagina}&limit={quantidade}&fields={campos}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return BadRequest("Erro ao buscar livros na Open Library.");

        string jsonString = await response.Content.ReadAsStringAsync();
        var resultadoBusca = JsonSerializer.Deserialize<RecebeTodosOsLivrosDTO>(jsonString);

        if (resultadoBusca?.Docs == null || !resultadoBusca.Docs.Any())
            return NotFound("Nenhum livro encontrado para os critérios informados.");

        var listaDeLivros = ConverterDocsParaLivros(resultadoBusca.Docs, tema ?? "");
        return Ok(listaDeLivros);
    }

    [HttpPost("busca/semantica")]
    public async Task<IActionResult> BuscarPorSemantica([FromBody] BuscaSemanticaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Texto))
            return BadRequest("Descreva o que você quer sentir lendo.");

        var embedding = await _geminiServices.ObterEmbeddingAsync(request.Texto);
        var resultados = await _qdrantServices.BuscarLivrosPorSensacaoAsync(embedding);

        if (!resultados.Any())
            return NotFound("Nenhum livro encontrado para essa sensação. Tente indexar mais livros.");

        var livros = resultados.Select(r => new LivroClasse
        {
            Isbn           = r.OpenLibraryKey ?? "Sem ISBN",
            Titulo         = r.Titulo,
            AutorPrincipal = r.Autor ?? "Autor Desconhecido",
            DataPublicacao = "Desconhecida",
            Editora        = "Desconhecida",
            Paginas        = 0,
            Temas          = r.Assuntos?.Split(',').Select(t => t.Trim()).ToList() ?? new List<string>(),
            CapaUrl        = r.CapaUrl
        }).ToList();

        return Ok(livros);
    }

    //    [HttpPost("admin/indexar-livros")]
    //    public async Task<IActionResult> IndexarLivros()
    //    {
    //        var livros = new[]
    //        {
    //    // Clássicos universais
    //    "1984 George Orwell", "Admiravel Mundo Novo Huxley", "Fahrenheit 451 Bradbury",
    //    "O Senhor dos Aneis Tolkien", "O Hobbit Tolkien", "Dom Quixote Cervantes",
    //    "Crime e Castigo Dostoievski", "O Idiota Dostoievski", "Os Irmaos Karamazov Dostoievski",
    //    "A Metamorfose Kafka", "O Processo Kafka", "O Castelo Kafka",
    //    "Moby Dick Melville", "Guerra e Paz Tolstoi", "Anna Karenina Tolstoi",
    //    "O Pequeno Principe Saint-Exupery", "Cem Anos de Solidao Garcia Marquez",
    //    "Amor nos Tempos do Colera Garcia Marquez", "O Mestre e Margarida Bulgakov",
    //    "O Apanhador no Campo de Centeio Salinger", "As Vinhas da Ira Steinbeck",
    //    "De Camundongos e Homens Steinbeck", "A Leste do Eden Steinbeck",
    //    "Ulisses James Joyce", "Retrato do Artista Quando Jovem Joyce",
    //    "Mrs Dalloway Virginia Woolf", "Ao Farol Virginia Woolf",
    //    "As Ondas Virginia Woolf", "O Som e a Furia Faulkner",
    //    "Enquanto Agonizo Faulkner", "Lolita Nabokov", "Pale Fire Nabokov",
    //    "O Mago John Fowles", "O Colecionador Fowles",

    //    // Ficção científica
    //    "Duna Frank Herbert", "Fundacao Isaac Asimov", "Eu Robot Asimov",
    //    "O Fim da Eternidade Asimov", "O Guia do Mochileiro das Galaxias Douglas Adams",
    //    "Neuromancer William Gibson", "Solaris Stanislaw Lem",
    //    "A Mao Esquerda da Escuridao Ursula Le Guin", "Os Despossuidos Le Guin",
    //    "Hyperion Dan Simmons", "A Guerra dos Mundos Wells",
    //    "A Maquina do Tempo Wells", "O Homem Invisivel Wells",
    //    "Flowers for Algernon Daniel Keyes", "Ubik Philip K Dick",
    //    "O Homem do Castelo Alto Philip K Dick", "Androides Sonham com Ovelhas Electricas Dick",
    //    "Cronicas Marcianas Ray Bradbury", "Slaughterhouse Five Vonnegut",
    //    "Cat Cradle Vonnegut", "Brave New World Huxley",
    //    "Never Let Me Go Kazuo Ishiguro", "The Road Cormac McCarthy",
    //    "Station Eleven Emily St John Mandel", "Oryx and Crake Margaret Atwood",
    //    "O Conto da Aia Margaret Atwood", "Os Testamentos Atwood",
    //    "Recursion Blake Crouch", "Dark Matter Blake Crouch",

    //    // Terror e suspense
    //    "O Iluminado Stephen King", "It Stephen King", "Carrie Stephen King",
    //    "O Cemiterio Stephen King", "A Torre Negra King",
    //    "Dracula Bram Stoker", "Frankenstein Mary Shelley",
    //    "O Retrato de Dorian Gray Oscar Wilde", "Rebecca Daphne du Maurier",
    //    "A Queda da Casa de Usher Poe", "O Corvo Poe",
    //    "O Medico e o Monstro Stevenson", "O Exorcista William Blatty",
    //    "A Assombração da Casa da Colina Shirley Jackson",
    //    "Nos Jackson", "O Chamado de Cthulhu Lovecraft",
    //    "Casa de Folhas Mark Z Danielewski", "Bird Box Josh Malerman",
    //    "Mexican Gothic Silvia Moreno-Garcia",

    //    // Romance e drama
    //    "Orgulho e Preconceito Jane Austen", "Razao e Sensibilidade Austen",
    //    "Emma Austen", "Persuasao Austen",
    //    "Jane Eyre Charlotte Bronte", "O Morro dos Ventos Uivantes Emily Bronte",
    //    "Em Busca do Tempo Perdido Proust", "O Grande Gatsby Fitzgerald",
    //    "O Sol Tambem se Levanta Hemingway", "Por Quem os Sinos Dobram Hemingway",
    //    "Madame Bovary Flaubert", "A Educacao Sentimental Flaubert",
    //    "Middlemarch George Eliot", "O Moinho no Rio Floss Eliot",
    //    "Tess dos dUrberville Thomas Hardy", "Jude o Obscuro Hardy",
    //    "Os Miseraveis Victor Hugo", "Notre-Dame de Paris Hugo",
    //    "O Conde de Monte Cristo Dumas", "Os Tres Mosqueteiros Dumas",
    //    "Germinal Zola", "Nana Zola", "A Besta Humana Zola",
    //    "Pai Goriot Balzac", "Eugenie Grandet Balzac",
    //    "Resurrection Tolstoi", "A Morte de Ivan Ilich Tolstoi",
    //    "O Jogador Dostoievski", "Noites Brancas Dostoievski",
    //    "Humilhados e Ofendidos Dostoievski",

    //    // Existencialismo e filosofia
    //    "O Estrangeiro Camus", "A Peste Camus", "O Mito de Sisifo Camus",
    //    "A Queda Camus", "A Nausea Sartre", "Ser e o Nada Sartre",
    //    "Assim Falou Zaratustra Nietzsche", "Sidarta Hermann Hesse",
    //    "O Lobo da Estepe Hesse", "Demian Hesse", "Narciso e Goldmund Hesse",
    //    "O Jogo das Contas de Vidro Hesse", "Steppenwolf Hesse",
    //    "A Montanha Magica Thomas Mann", "Buddenbrooks Mann",
    //    "Morte em Veneza Mann", "O Processo Kafka",
    //    "Nausea Sartre", "O Ser e o Nada Sartre",
    //    "Medo e Tremor Kierkegaard", "O Desespero Humano Kierkegaard",

    //    // Aventura e acao
    //    "Vinte Mil Leguas Submarinas Verne", "A Volta ao Mundo em 80 Dias Verne",
    //    "Viagem ao Centro da Terra Verne", "Da Terra a Lua Verne",
    //    "Treasure Island Stevenson", "Robinson Crusoe Defoe",
    //    "Ivanhoe Walter Scott", "As Minas do Rei Salomao Haggard",
    //    "Allan Quatermain Haggard", "O Livro da Selva Kipling",
    //    "Kim Kipling", "Capitao Courageous Kipling",
    //    "O Corsario Negro Salgari", "Sandokan Salgari",
    //    "Os Piratas da Malasia Salgari", "Os Nibelungos",
    //    "Ben-Hur Lewis Wallace", "Quo Vadis Sienkiewicz",

    //    // Literatura brasileira
    //    "Dom Casmurro Machado de Assis", "Memorias Postumas de Bras Cubas Machado",
    //    "Quincas Borba Machado", "O Alienista Machado",
    //    "O Cortico Azevedo", "Iracema Jose de Alencar",
    //    "O Guarani Alencar", "A Moreninha Macedo",
    //    "Grande Sertao Veredas Guimaraes Rosa", "Sagarana Rosa",
    //    "Primeiras Estorias Rosa", "Vidas Secas Graciliano Ramos",
    //    "Memorias do Carcere Graciliano", "Sao Bernardo Graciliano",
    //    "Angustia Graciliano", "Capitaes da Areia Jorge Amado",
    //    "Gabriela Cravo e Canela Amado", "Tieta do Agreste Amado",
    //    "Tereza Batista Amado", "A Hora da Estrela Clarice Lispector",
    //    "Perto do Coracao Selvagem Clarice", "A Paixao Segundo GH Clarice",
    //    "Lacos de Familia Clarice", "A Maca no Escuro Clarice",
    //    "Fogo Morto Jose Lins do Rego", "Menino de Engenho Rego",
    //    "Triste Fim de Policarpo Quaresma Lima Barreto",
    //    "O Triste Fim de Policarpo Quaresma Barreto",
    //    "Cronicas de uma Morte Anunciada Garcia Marquez",
    //    "Macunaima Mario de Andrade", "Serafim Ponte Grande Oswald de Andrade",

    //    // Mitologia e fantasia
    //    "O Nome do Vento Patrick Rothfuss", "O Medo do Homem Sabio Rothfuss",
    //    "As Cronicas de Narnia CS Lewis", "A Divina Comedia Dante",
    //    "Odisseia Homero", "Iliada Homero", "Eneida Virgilio",
    //    "Metamorfoses Ovidio", "A Republica Platao",
    //    "O Silmarillion Tolkien", "Contos Inacabados Tolkien",
    //    "Eragon Christopher Paolini", "Eldest Paolini",
    //    "As Cronicas de Gelo e Fogo Martin", "A Guerra dos Tronos Martin",
    //    "A Tormenta de Espadas Martin", "Festim dos Corvos Martin",
    //    "A Roda do Tempo Robert Jordan", "O Olho do Mundo Jordan",
    //    "Mistborn Brandon Sanderson", "O Heroi das Eras Sanderson",
    //    "Elantris Sanderson", "O Caminho dos Reis Sanderson",

    //    // Contemporaneos e bestsellers
    //    "O Alquimista Paulo Coelho", "Onze Minutos Paulo Coelho",
    //    "O Zahir Coelho", "Veronika Decide Morrer Coelho",
    //    "O Codigo Da Vinci Dan Brown", "Inferno Dan Brown", "Anjos e Demonios Brown",
    //    "Garota Exemplar Gillian Flynn", "Perdida Flynn", "Lugares Escuros Flynn",
    //    "A Menina que Roubava Livros Markus Zusak",
    //    "A Culpa e das Estrelas John Green", "Procurando Alaska Green",
    //    "Jogos Vorazes Suzanne Collins", "Em Chamas Collins", "A Revolta Collins",
    //    "Divergente Veronica Roth", "Insurgente Roth", "Convergente Roth",
    //    "Harry Potter Pedra Filosofal Rowling", "Harry Potter Camara Secreta Rowling",
    //    "Harry Potter Prisioneiro de Azkaban Rowling", "Harry Potter Calice de Fogo Rowling",
    //    "Crepusculo Stephenie Meyer", "Lua Nova Meyer",
    //    "O Labirinto Dashner", "A Cura Mortal Dashner",
    //    "Cidade dos Ossos Cassandra Clare", "Cidade das Cinzas Clare",
    //    "Encantadora de Corvos Leigh Bardugo", "Seis de Corvos Bardugo",

    //    // Autoconhecimento e cronica
    //    "O Diario de Anne Frank", "Noite Elie Wiesel",
    //    "Em Busca de Sentido Viktor Frankl", "O Poder do Agora Tolle",
    //    "Sapiens Yuval Harari", "Homo Deus Harari", "21 Licoes Harari",
    //    "Cronicas de Rubem Braga", "O Globo Reporter Braga",
    //    "A Bagagem do Viajante Braga", "Para Gostar de Ler",
    //    "Feliz Ano Velho Marcelo Rubens Paiva",
    //    "Maysa Uma Biografia Zuza Homem de Mello",

    //    // Policiais e mistério
    //    "Assassinato no Expresso do Oriente Agatha Christie",
    //    "O Assassinato de Roger Ackroyd Christie",
    //    "E Nao Sobrou Nenhum Christie", "Morte no Nilo Christie",
    //    "O Cao dos Baskervilles Conan Doyle",
    //    "Um Estudo em Vermelho Doyle", "O Signo dos Quatro Doyle",
    //    "O Nome da Rosa Umberto Eco", "O Pendulo de Foucault Eco",
    //    "O Perfume Patrick Suskind", "O Contrabaixo Suskind",
    //    "A Garota com o Dragao Tatuado Stieg Larsson",
    //    "A Menina que Sonhava com Fosforos Larsson",
    //    "A Rainha no Palacio de Correntes de Ar Larsson",
    //    "Millennium Larsson", "Big Little Lies Liane Moriarty",
    //    "Nine Perfect Strangers Moriarty"
    //};

    //        await _livroIngestaoService.IndexarLivrosAsync(livros);
    //        return Ok($"{livros.Length} livros indexados com sucesso.");
    //    }




    // Métodos
    #region Region métodos
    private List<LivroClasse> ConverterDocsParaLivros(List<OpenLibraryPesquisaDoc> docs, string temaPadrao = "")
    {
        var listaDeLivros = new List<LivroClasse>();
        foreach (var doc in docs)
        {
            string primeiroIsbn = doc.Isbn?.FirstOrDefault() ?? "";
            string olid = doc.CoverEditionKey ?? "";

            var temasDoLivro = doc.Temas ?? new List<string>();
            if (!string.IsNullOrEmpty(temaPadrao) && !temasDoLivro.Contains(temaPadrao))
                temasDoLivro.Add(temaPadrao);
            if (!temasDoLivro.Any())
                temasDoLivro.Add("Literatura Geral");

            var meuLivro = new LivroClasse
            {
                Isbn = string.IsNullOrEmpty(primeiroIsbn) ? "Sem ISBN" : primeiroIsbn,
                Titulo = doc.Titulo,
                AutorPrincipal = doc.NomeAutor?.FirstOrDefault() ?? "Autor Desconhecido",
                DataPublicacao = doc.PrimeiroAnoPublicacao?.ToString() ?? "Desconhecida",
                Editora = doc.Editora?.FirstOrDefault() ?? "Editora Desconhecida",
                Paginas = 0,
                Temas = temasDoLivro,
                CapaUrl = ObterMelhorCapaUrl(primeiroIsbn, olid, doc.Titulo) 
            };
            listaDeLivros.Add(meuLivro);
        }
        return listaDeLivros;
    }

    private string ObterMelhorCapaUrl(string? isbn, string? olid, string? titulo)
    {
        // Prioridade 1: ISBN (melhor cobertura)
        if (!string.IsNullOrEmpty(isbn))
            return $"https://covers.openlibrary.org/b/isbn/{isbn}-M.jpg";

        // Prioridade 2: OLID
        if (!string.IsNullOrEmpty(olid))
            return $"https://covers.openlibrary.org/b/olid/{olid}-M.jpg";

        // Prioridade 3: Título (fallback via Open Library)
        if (!string.IsNullOrEmpty(titulo))
            return $"https://covers.openlibrary.org/b/title/{Uri.EscapeDataString(titulo)}-M.jpg";

        return "";
    }

    public record BuscaSemanticaRequest(string Texto);
    #endregion

}