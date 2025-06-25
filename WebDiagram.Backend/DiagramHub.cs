using Microsoft.AspNetCore.SignalR;

public class DiagramHub : Hub
{
    public async Task JoinInstanceGroup(string instanceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, instanceId);
    }
}