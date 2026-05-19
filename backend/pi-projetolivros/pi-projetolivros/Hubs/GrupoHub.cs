using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace pi_projetolivros.Hubs;

[Authorize]
public class GrupoHub : Hub
{
    public async Task EntrarNoGrupo(string grupoId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"grupo-{grupoId}");
    }

    public async Task SairDoGrupo(string grupoId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"grupo-{grupoId}");
    }
}