using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace pi_projetolivros.Hubs;

[Authorize]
public class DMHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Tenta pegar pelo padrão, se for nulo, busca direto das Claims igual na Controller
        var usuarioId = Context.UserIdentifier ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        Console.WriteLine($"[SignalR-DM] Usuário {usuarioId} conectou!");

        if (!string.IsNullOrEmpty(usuarioId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"dm-usuario-{usuarioId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var usuarioId = Context.UserIdentifier;
        if (usuarioId != null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"dm-usuario-{usuarioId}");

        await base.OnDisconnectedAsync(exception);
    }
}