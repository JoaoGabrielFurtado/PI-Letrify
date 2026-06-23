using pi_projetolivros.DTO.RetornoQdrant;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Qdrant;
using pi_projetolivros.DTO.ConectarApiLivros;
using pi_projetolivros.DTO.RetornoQdrant;

namespace pi_projetolivros.Servicos;
public class QdrantServices
{
    private readonly Qdrant.Client.QdrantClient _clienteQdrant;  
    private readonly string _nomeColecao = "usuarios_matches";
    private readonly string _livroColecao = "livros_semanticos";

    public QdrantServices(IConfiguration configuration)
    {
        string url = configuration["Qdrant:Url"] ?? throw new Exception("URL do Qdrant vazia!");
        string apiKey = configuration["Qdrant:ApiKey"] ?? throw new Exception("API Key do Qdrant vazia!");
        _clienteQdrant = new Qdrant.Client.QdrantClient(host: url, https: true, apiKey: apiKey);
    }

    public async Task InicializarColecaoAsync()
    {
        var colecoes = await _clienteQdrant.ListCollectionsAsync();

        if (!colecoes.Contains(_nomeColecao))
        {
            await _clienteQdrant.CreateCollectionAsync(
                collectionName: _nomeColecao,
                vectorsConfig: new VectorParams { Size = 3072, Distance = Distance.Cosine }
            );
        }
    }

    public async Task SalvarVetorUsuarioAsync(int usuarioId, float[] vetorCaracteristicas)
    {
        var ponto = new PointStruct
        {
            Id = (ulong)usuarioId,
            Vectors = vetorCaracteristicas,
            Payload ={
                ["usuarioId"] = usuarioId
            }
        };

        await _clienteQdrant.UpsertAsync(_nomeColecao, new[] { ponto });
    }

    public async Task<List<RetornoQdrantDto>> BuscarUsuariosParecidosAsync(int usuarioIdQueEstaBuscando, float[] vetorDoUsuario, int limite = 5)
    {
        var filtro = new Qdrant.Client.Grpc.Filter();
        filtro.MustNot.Add(new Qdrant.Client.Grpc.Condition
        {
            HasId = new Qdrant.Client.Grpc.HasIdCondition
            {
                HasId = { (ulong)usuarioIdQueEstaBuscando }
            }
        });

        var pontosRetornados = await _clienteQdrant.SearchAsync(
            collectionName: _nomeColecao,
            vector: vetorDoUsuario,
            filter: filtro,
            limit: (ulong)limite
        );

        var listaDeIds = new List<RetornoQdrantDto>();

        foreach (var ponto in pontosRetornados)
        {
            int idDoUsuarioMatch = (int)ponto.Id.Num;
            float score = ponto.Score;
            var proximo = new RetornoQdrantDto
            {
                Id = idDoUsuarioMatch,
                Score = score
            };
            listaDeIds.Add(proximo);
        }

        return listaDeIds;
    }

    public async Task CriarColecaoLivrosAsync()
    {
        var colecaoExiste = await _clienteQdrant.CollectionExistsAsync(_livroColecao);
        if (colecaoExiste) return;

        await _clienteQdrant.CreateCollectionAsync(_livroColecao, new VectorParams
        {
            Size = 3072,
            Distance = Distance.Cosine
        });
    }

    public async Task SalvarVetorLivroAsync(OpenLibraryBuscaSemanticaDoc livro, float[] embedding)
    {
        var openLibraryKey = livro.Key ?? Guid.NewGuid().ToString();

        var ponto = new PointStruct
        {
            Id = new PointId { Uuid = Guid.NewGuid().ToString() },
            Vectors = new Vectors
            {
                Vector = new Vector { Data = { embedding } }
            },
            Payload =
        {
            ["titulo"]           = livro.Title ?? "Sem título",
            ["autor"]            = string.Join(", ", livro.AuthorName ?? []),
            ["open_library_key"] = openLibraryKey,
            ["assuntos"]         = string.Join(", ", livro.Subject?.Take(5) ?? []),
            ["isbn"]             = livro.Isbn?.FirstOrDefault() ?? ""
        }
        };

        await _clienteQdrant.UpsertAsync(_livroColecao, new[] { ponto });
    }

    public async Task<List<LivroSemanticoResultadoDto>> BuscarLivrosPorSensacaoAsync(float[] vetorBusca, int limite = 10)
    {
        var pontosRetornados = await _clienteQdrant.SearchAsync(
            collectionName: _livroColecao,
            vector: vetorBusca,
            limit: (ulong)limite,
            scoreThreshold: 0.5f
        );

        var resultado = new List<LivroSemanticoResultadoDto>();

        foreach (var ponto in pontosRetornados)
        {
            var key = ponto.Payload["open_library_key"].StringValue;
            var isbn = ponto.Payload.ContainsKey("isbn") ? ponto.Payload["isbn"].StringValue : "";
            var olid = key.Replace("/works/", "");

            var capaUrl = !string.IsNullOrEmpty(isbn)
                ? $"https://covers.openlibrary.org/b/isbn/{isbn}-M.jpg"
                : $"https://covers.openlibrary.org/b/olid/{olid}-M.jpg";

            resultado.Add(new LivroSemanticoResultadoDto
            {
                Titulo = ponto.Payload["titulo"].StringValue,
                Autor = ponto.Payload["autor"].StringValue,
                OpenLibraryKey = key,
                Assuntos = ponto.Payload["assuntos"].StringValue,
                Score = Math.Round(ponto.Score * 100, 1),
                CapaUrl = capaUrl
            });
        }

        return resultado;
    }
}

