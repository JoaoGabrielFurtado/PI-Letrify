using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace pi_projetolivros.Hubs;

[Authorize]
public class DMHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var usuarioId = Context.UserIdentifier;
        if (usuarioId != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"dm-usuario-{usuarioId}");

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