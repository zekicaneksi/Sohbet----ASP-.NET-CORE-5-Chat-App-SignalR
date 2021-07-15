using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Sohbet.Hubs
{
    public class User
    {
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
    }

    [Authorize]
    public class ChatHub : Hub
    {
        static List<User> SignalRUsers = new List<User>();
        public async Task SendMessage(string message)
        {
            string nick = (from c in Context.User.Claims
                    where c.Type == "Nick"
                    select c.Value).FirstOrDefault();
            await Clients.All.SendAsync("ReceiveMessage", nick , message);
        }
        public override async Task OnConnectedAsync()
        {
            User user = new User();
            user.UserName = (from c in Context.User.Claims
                             where c.Type == "Nick"
                             select c.Value).FirstOrDefault();
            user.ConnectionId = Context.ConnectionId;

            //send shut connection message if the user is already connected on another tab
            User user_check = (from c in SignalRUsers where c.UserName == user.UserName select c).FirstOrDefault();
            if (user_check != default)
            {
                await Clients.Client(user_check.ConnectionId).SendAsync("ShutConnection");
            }
            
            SignalRUsers.Add(user);

            string message="";
            foreach (User nick in SignalRUsers)
            {
                message += "&" + nick.UserName;
            }
            await Clients.Client(user.ConnectionId).SendAsync("ActiveUserList", message);
            await Clients.All.SendAsync("NewUserConnected", user.UserName);

        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var holdUserName = (from c in Context.User.Claims
                             where c.Type == "Nick"
                             select c.Value).FirstOrDefault();
            SignalRUsers.Remove((from c in SignalRUsers where c.UserName==holdUserName select c).FirstOrDefault());
            await Clients.All.SendAsync("UserDisconnected", holdUserName);
            await base.OnDisconnectedAsync(exception);
        }

    }
}
