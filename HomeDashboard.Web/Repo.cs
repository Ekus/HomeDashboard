using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeDashboard.Web
{
    public class Repo
    {
        public static T Add<T>(T entity) {

             using (var session = Global.Store.OpenSession())
            {
                session.Store(entity);
                session.SaveChanges();
            }
            return entity;
        }

        public static IQueryable<ChatMessage> GetChatMessages()
        {
            using (var session = Global.Store.OpenSession())
            {
                return session.Query<ChatMessage>();
            }        
        }

        public static ListOfItems GetListItems()
        {
            using (var session = Global.Store.OpenSession())
            {
                return session.Query<ListOfItems>().FirstOrDefault();
            }
        }


        public static IDocumentSession GetSession()
        {
            return Global.Store.OpenSession();
        } 

    }
}