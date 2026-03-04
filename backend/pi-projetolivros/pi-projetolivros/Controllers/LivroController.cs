using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pi_projetolivros.DTO;
using pi_projetolivros.Models;
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

    //public LivroController(Banco contexto)
    //{
    //    _contexto = contexto;
    //}

    [HttpGet("livroespecifico/{isbn}")]
    public async Task<ActionResult<Livro>> RetornaLivroComIsbn(string isbn)
    {

        //exemplo: GET /api/livro/livroespecifico/{isbn}

        string url = $"https://openlibrary.org/api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return BadRequest("Erro ao se comunicar com a Open Library.");

        string jsonString = await response.Content.ReadAsStringAsync();

        var resultadoOpenLibrary = JsonSerializer.Deserialize<Dictionary<string, RecebeLivrosDTO>>(jsonString);

        if (resultadoOpenLibrary == null || !resultadoOpenLibrary.Any())
            return NotFound("Livro não encontrado.");

        var livroExtraido = resultadoOpenLibrary.Values.First();

        var meuLivro = new Livro
        {
            Isbn = isbn,
            Titulo = livroExtraido.Titulo,
            DataPublicacao = livroExtraido.DataPublicacao,
            Paginas = livroExtraido.NumeroPaginas ?? 0,
            AutorPrincipal = livroExtraido.Autores?.FirstOrDefault()?.Name,
            Editora = livroExtraido.Editora?.FirstOrDefault()?.Name,
            Temas = livroExtraido.Temas?.Select(s => s.Name).ToList() ?? new List<string>()
        };

        return Ok(meuLivro);
    }

    [HttpGet("livrostema")]
    public async Task<ActionResult<List<Livro>>> ExplorarLivros(
        [FromQuery] string tema = "fiction",
        [FromQuery] int pagina = 1,
        [FromQuery] int quantidade = 20)
    {

        // exemplo: GET /api/livro/livrostema?tema=fantasy&quantidade=10

        string temaTratado = Uri.EscapeDataString(tema.Trim().ToLower());
        string url = $"https://openlibrary.org/search.json?subject={temaTratado}&page={pagina}&limit={quantidade}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return BadRequest("Erro ao buscar livros na Open Library.");

        string jsonString = await response.Content.ReadAsStringAsync();

        var resultadoBusca = JsonSerializer.Deserialize<RecebeTodosOsLivrosDTO>(jsonString);

        if (resultadoBusca == null || resultadoBusca.Docs == null || !resultadoBusca.Docs.Any())
            return NotFound("Nenhum livro encontrado para este tema.");

        var listaDeLivros = ConverterDocsParaLivros(resultadoBusca.Docs, tema);
        return Ok(listaDeLivros);
    }

    [HttpGet("livrostitulo")]
    public async Task<ActionResult<List<Livro>>> BuscarPorTitulo(
        [FromQuery] string titulo,
        [FromQuery] int pagina = 1,
        [FromQuery] int quantidade = 20)
    {

        //exemplo pesquisa: GET /api/livro/livrostitulo?titulo=principe&quantidade=20


        if (string.IsNullOrWhiteSpace(titulo))
            return BadRequest("O título para pesquisa não pode estar vazio.");

        string tituloTratado = Uri.EscapeDataString(titulo.Trim().ToLower());

        string url = $"https://openlibrary.org/search.json?title={tituloTratado}&page={pagina}&limit={quantidade}";

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return BadRequest("Erro ao buscar livros na Open Library.");

        string jsonString = await response.Content.ReadAsStringAsync();

        var resultadoBusca = JsonSerializer.Deserialize<RecebeTodosOsLivrosDTO>(jsonString);

        if (resultadoBusca == null || resultadoBusca.Docs == null || !resultadoBusca.Docs.Any())
            return NotFound("Nenhum livro encontrado com este título.");

        var listaDeLivros = ConverterDocsParaLivros(resultadoBusca.Docs);
        return Ok(listaDeLivros);
    }



    // Métodos

    private List<Livro> ConverterDocsParaLivros(List<OpenLibraryPesquisaDoc> docs, string temaPadrao = "")
    {
        var listaDeLivros = new List<Livro>();
        foreach (var doc in docs)
        {
            string primeiroIsbn = doc.Isbn?.FirstOrDefault() ?? "Sem ISBN";
            var meuLivro = new Livro
            {
                Isbn = primeiroIsbn,
                Titulo = doc.Titulo,
                AutorPrincipal = doc.NomeAutor?.FirstOrDefault() ?? "Autor Desconhecido",
                DataPublicacao = doc.PrimeiroAnoPublicacao?.ToString() ?? "Desconhecida",
                Editora = doc.Editora?.FirstOrDefault() ?? "Editora Desconhecida",
                Paginas = 0,
                Temas = string.IsNullOrEmpty(temaPadrao) ? new List<string>() : new List<string> { temaPadrao }
            };
            listaDeLivros.Add(meuLivro);
        }
        return listaDeLivros;
    }
}