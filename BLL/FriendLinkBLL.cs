using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;

namespace Gllo.BLL
{
    public class FriendLinkBLL
    {
        private readonly Gllo.DAL.FriendLinkDAL dal = new DAL.FriendLinkDAL();

        public void AddFriendLink(FriendLinkObj friendLinkObj)
        {
            dal.AddFriendLink(friendLinkObj);
        }

        public void ModifyFriendLink(FriendLinkObj friendLinkObj)
        {
            dal.ModifyFriendLink(friendLinkObj);
        }

        public void DeleteFriendLink(int friendId)
        {
            dal.DeleteFriendLink(friendId);
        }

        public FriendLinkObj GetFriendLinkByID(int friendId)
        {
            return dal.GetFriendLinkByID(friendId);
        }

        public JsonArray GetFriendLinks()
        {
            return dal.GetFriendLinks();
        }
    }
}
