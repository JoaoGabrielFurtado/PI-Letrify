using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pi_projetolivros.DTO.ConectarApiLivros;
using pi_projetolivros.Models;
using pi_projetolivros.Models.Banco;
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

    [HttpGet("livrostema")]
    public async Task<ActionResult<List<LivroClasse>>> ExplorarLivros(
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
    public async Task<ActionResult<List<LivroClasse>>> BuscarPorTitulo(
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
    #region Region métodos
    private List<LivroClasse> ConverterDocsParaLivros(List<OpenLibraryPesquisaDoc> docs, string temaPadrao = "")
    {
        var listaDeLivros = new List<LivroClasse>();
        foreach (var doc in docs)
        {
            string primeiroIsbn = doc.Isbn?.FirstOrDefault() ?? "Sem ISBN";
            var meuLivro = new LivroClasse
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
    #endregion

}