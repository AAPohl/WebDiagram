using Microsoft.AspNetCore.SignalR;

public class DiagramHub : Hub
{
	public async Task SendImage(byte[] imageData)
	{
		// Broadcast an alle verbundenen Clients
		await Clients.All.SendAsync("ReceiveImage", Convert.ToBase64String(imageData));
	}
}

