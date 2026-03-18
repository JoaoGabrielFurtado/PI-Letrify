using pi_projetolivros.DTO.RetornoQdrant;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace pi_projetolivros.Serviços;
public class QdrantServices
{
    private readonly QdrantClient _clienteQdrant;  
    private readonly string _nomeColecao = "usuarios_matches";

    public QdrantServices(IConfiguration configuration)
    {
        string url = configuration["Qdrant:Url"] ?? throw new Exception("URL do Qdrant vazia!");
        string apiKey = configuration["Qdrant:ApiKey"] ?? throw new Exception("API Key do Qdrant vazia!");
        _clienteQdrant = new QdrantClient(host: url, https: true, apiKey: apiKey);
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
}
