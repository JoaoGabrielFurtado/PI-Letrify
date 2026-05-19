using Microsoft.AspNetCore.SignalR;
using pi_projetolivros.Hubs;
using pi_projetolivros.Models.Banco;
using pi_projetolivros_banco;

namespace pi_projetolivros.Servicos;

public class NotificacaoService
{
    private readonly Banco _contexto;
    private readonly IHubContext<NotificacaoHub> _hubContext;

    public NotificacaoService(Banco contexto, IHubContext<NotificacaoHub> hubContext)
    {
        _contexto = contexto;
        _hubContext = hubContext;
    }

    // Chame este método em qualquer lugar que precise disparar uma notificação
    public async Task EnviarAsync(int usuarioDestinoId, string tipo, string conteudo)
    {
        var notificacao = new Notificacao
        {
            UsuarioId = usuarioDestinoId,
            Tipo = tipo,
            Conteudo = conteudo,
            DataCriacao = DateTime.UtcNow
        };

        _contexto.Notificacoes.Add(notificacao);
        await _contexto.SaveChangesAsync();

        // Dispara em tempo real só para o usuário destino
        var payload = new
        {
            notificacao.Id,
            notificacao.Tipo,
            notificacao.Conteudo,
            notificacao.DataCriacao
        };

        await _hubContext.Clients
            .Group($"usuario-{usuarioDestinoId}")
            .SendAsync("ReceberNotificacao", payload);
    }
}