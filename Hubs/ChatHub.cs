using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    private readonly ChatDbContext _context;
    private static ConcurrentDictionary<string, string> ConnectedUsers = new();

    public ChatHub(ChatDbContext context)
    {
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.GetHttpContext().Request.Query["user"].ToString();
        if (!string.IsNullOrEmpty(user))
        {
            ConnectedUsers[Context.ConnectionId] = user;

            // Notify others who joined
            await Clients.All.SendAsync("UpdateUserCount", ConnectedUsers.Count);
            await Clients.Others.SendAsync("UserSeenMessage", user);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if (ConnectedUsers.TryRemove(Context.ConnectionId, out var user))
        {
            await Clients.All.SendAsync("ReceiveMessage", user, $"{user} left the chat");

            // Update user count
            await Clients.All.SendAsync("UpdateUserCount", ConnectedUsers.Count);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string user, string message)
    {
        var msg = new Message
        {
            User = user,
            Text = message,
            Room = "General"
        };

        _context.Messages.Add(msg);
        await _context.SaveChangesAsync();

        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task SendMessageToRoom(string user, string message, string room)
    {
        var msg = new Message
        {
            User = user,
            Text = message,
            Room = room
        };

        _context.Messages.Add(msg);
        await _context.SaveChangesAsync();

        await Clients.Group(room).SendAsync("ReceiveMessage", user, message);
    }

    public async Task JoinRoom(string room)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, room);
    }
}
