using Microsoft.AspNetCore.SignalR;

namespace aws_agent.Services;

public class UploadHub : Hub
{
    public async Task JoinUploadSession(string uploadId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, uploadId);
    }

    public async Task LeaveUploadSession(string uploadId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, uploadId);
    }
}
