using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;

namespace Gllo.BLL
{
    public class MediaBLL
    {
        private readonly Gllo.DAL.MediaDAL dal = new DAL.MediaDAL();

        public int SetMedia(MediaObj mediaObj)
        {
            return dal.SetMedia(mediaObj);
        }

        public MediaObj GetMedia(int mediaID)
        {
            return dal.GetMedia(mediaID);
        }

    }
}
