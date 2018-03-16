using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client;
using System.Collections.Generic;

namespace HomeDashboard.Web
{
    public class ListHub : Hub
    {
        public void BroadcastItem(ListItem item)
        {
            Clients.All.broadcastItem(item);
        }

        public List<ListItem> GetItems() {
            var result = Repo.GetListItems();
            return result.Items;
        }

        public void BeginEditing(Guid itemId, string user)
        {
             using (var session = Global.Store.OpenSession())
            {
                 var list = session.Query<ListOfItems>().FirstOrDefault();
                 var item = list.Items.SingleOrDefault(i=>i.Id==itemId);
                 item.IsBeingEditedBy.Add(user);
                 item.Timestamp = DateTime.Now;
                 try
                 {
                     session.SaveChanges();
                     BroadcastItem(item);
                 }
                 catch (Raven.Abstractions.Exceptions.ConcurrencyException rex)
                 {
                     //LOG
                 }
            }
        }

        public void EndEditing(Guid itemId, string user)
        {
             using (var session = Global.Store.OpenSession())
            {
                 var list = session.Query<ListOfItems>().FirstOrDefault();
                 var item = list.Items.SingleOrDefault(i=>i.Id==itemId);
                 item.IsBeingEditedBy.Remove(user);
                 item.Timestamp = DateTime.Now;
                 try
                 {
                     session.SaveChanges();
                     BroadcastItem(item);
                 }
                 catch (Raven.Abstractions.Exceptions.ConcurrencyException rex)
                 {
                     //LOG
                 }
            }
        }

        
        public void BeginCompletion(Guid itemId, string user)
        {
             using (var session = Global.Store.OpenSession())
            {
                 var list = session.Query<ListOfItems>().FirstOrDefault();
                 var item = list.Items.SingleOrDefault(i=>i.Id==itemId);
                 item.IsBeingCompletedBy.Add(user);
                 item.Timestamp = DateTime.Now;
                 try
                 {
                     session.SaveChanges();
                     BroadcastItem(item);
                 }
                 catch (Raven.Abstractions.Exceptions.ConcurrencyException rex)
                 {
                     //LOG
                 }
            }
        }

        public void EndCompletion(Guid itemId, string user)
        {
             using (var session = Global.Store.OpenSession())
            {
                 var list = session.Query<ListOfItems>().FirstOrDefault();
                 var item = list.Items.SingleOrDefault(i=>i.Id==itemId);
                 item.IsBeingCompletedBy.Remove(user);
                 item.Timestamp = DateTime.Now;
                 try
                 {
                     session.SaveChanges();
                     BroadcastItem(item);
                 }
                 catch (Raven.Abstractions.Exceptions.ConcurrencyException rex)
                 {
                     //LOG
                 }
            }
        }

        public void SetCompletion(Guid itemId, string user)
        {
             using (var session = Global.Store.OpenSession())
            {
                 var list = session.Query<ListOfItems>().FirstOrDefault();
                 var item = list.Items.SingleOrDefault(i=>i.Id==itemId);
                 item.CompletedBy= user;
                 item.Timestamp = DateTime.Now;
                 try
                 {
                     session.SaveChanges();
                     BroadcastItem(item);
                 }
                 catch (Raven.Abstractions.Exceptions.ConcurrencyException rex)
                 {
                     //LOG
                 }
            }
        }

        public void SetUrgent(Guid itemId, bool isUrgent)
        {
             using (var session = Global.Store.OpenSession())
            {
                 var list = session.Query<ListOfItems>().FirstOrDefault();
                 var item = list.Items.SingleOrDefault(i=>i.Id==itemId);
                 item.IsUrgent = isUrgent;
                 item.Timestamp = DateTime.Now;
                 try
                 {
                     session.SaveChanges();
                     BroadcastItem(item);
                 }
                 catch (Raven.Abstractions.Exceptions.ConcurrencyException rex)
                 {
                     //LOG
                 }
            }
        }

        public void UpdateText(Guid itemId, string text)
        {
            using (var session = Global.Store.OpenSession())
            {
                var list = session.Query<ListOfItems>().FirstOrDefault();
                var item = list.Items.SingleOrDefault(i => i.Id == itemId);
                item.Text = text;
                item.Timestamp = DateTime.Now;
                try
                {
                    session.SaveChanges();
                    BroadcastItem(item);
                }
                catch (Raven.Abstractions.Exceptions.ConcurrencyException rex)
                {
                    //LOG
                }

            }

        }

        public void AddItem(string text)
        {
            using (var session = Global.Store.OpenSession())
            {
                var list = session.Query<ListOfItems>().FirstOrDefault();
                var item = new ListItem() { Text = text };
                list.Items.Add(item);
                session.SaveChanges();
                BroadcastItem(item);
            }
        }
        
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

        public override Task OnDisconnected(bool stopCalled)
        {
            // Add your own code here.
            // For example: in a chat application, mark the user as offline, 
            // delete the association between the current connection id and user name.

            using (var session = Global.Store.OpenSession())
            {
                var list = session.Query<ListOfItems>().FirstOrDefault();
                foreach (var item in list.Items)
                {
                    item.IsBeingEditedBy.Remove(Context.User.Identity.Name);
                }
                session.SaveChanges();
            }

            Clients.All.Trace(String.Format("Client {0} disconnected", Context.User.Identity.Name));
            return base.OnDisconnected(stopCalled);
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
