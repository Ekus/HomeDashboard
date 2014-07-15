using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRChat
{
    public class ChatHub : Hub
    {
        public void Send(string name, string message)
        {
            Clients.All.broadcastMessage(name, message);

            // Call the broadcastMessage method to update clients.
            if (message.StartsWith("press "))
            {
                //Clients.Client("Arduino").pressButton(message);
                Clients.All.pressButton(message);
            }

            if (message.StartsWith("say ") && message.Length>4)
                Clients.All.speak(message.Substring(4));


            
        }

//        public void PressButton(...)




        public override Task OnConnected()
        {
            // Add your own code here.
            // For example: in a chat application, record the association between
            // the current connection ID and user name, and mark the user as online.
            // After the code in this method completes, the client is informed that
            // the connection is established; for example, in a JavaScript client,
            // the start().done callback is executed.
            Clients.All.Trace(String.Format("Client {0} connected", Context.User.Identity.Name));
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            // Add your own code here.
            // For example: in a chat application, mark the user as offline, 
            // delete the association between the current connection id and user name.
            Clients.All.Trace(String.Format("Client {0} disconnected", Context.User.Identity.Name));
            return base.OnDisconnected();
        }

        public override Task OnReconnected()
        {
            // Add your own code here.
            // For example: in a chat application, you might have marked the
            // user as offline after a period of inactivity; in that case 
            // mark the user as online again.
            Clients.All.Trace(String.Format("Client {0} reconnected", Context.User.Identity.Name));
            return base.OnReconnected();
        }


    }
}
