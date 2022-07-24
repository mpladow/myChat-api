using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatService.Hubs
{
    public class ChatHub : Hub
    {
        private readonly string _botUser;
        private readonly IDictionary<string, UserConnection> _connections;

        public ChatHub(IDictionary<string, UserConnection> connections)
        {
            _botUser = "MyChat Bot";
            _connections = connections;
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            // remove the connection from the dictionary
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                _connections.Remove(Context.ConnectionId);
                Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has left");
                SendConnectionedUsers(userConnection.Room);
            }
            // maybe save chat log to database
            return base.OnDisconnectedAsync(exception);
        }
        public async Task JoinRoom(UserConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);
            //// receives the name of the function that will be received in the backend
            // we are going to save
            _connections[Context.ConnectionId] = userConnection; 
    // will invoke a message on the frontend
            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser,
                $"{userConnection.User} has joined {userConnection.Room}");
            await SendConnectionedUsers(userConnection.Room);
        }
        public async Task SendMessage(string message)
        {
            // here we try to get the room the user is connected to , then we send a message to that room, based off the connection id.

            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.Group(userConnection.Room)
                    .SendAsync("ReceiveMessage", userConnection.User, message);
            }
        }
        // everytime someone enters the room
        public async Task UsersInRoom()
        {

        }

        public Task SendConnectionedUsers(string room)
        {
            var users = _connections.Values.Where(x => x.Room == room).Select(c => c.User);

            return Clients.Group(room).SendAsync("UsersInRoom", users);

        }
    }
}

