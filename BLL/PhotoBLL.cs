using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;

namespace Gllo.BLL
{
    public class PhotoBLL
    {
        private Gllo.DAL.PhotoDAL dal = new DAL.PhotoDAL();

        #region IsPhotoExists
        public bool IsPhotoExists(string photoName)
        {
            return dal.IsPhotoExists(photoName);
        }
        #endregion

        #region AddPhoto
        public void AddPhoto(PhotoObj photoObj)
        {
            dal.AddPhoto(photoObj);
        }
        #endregion

        #region ModifyPhoto
        public void ModifyPhoto(PhotoObj photoObj)
        {
            dal.ModifyPhoto(photoObj);
        }
        #endregion

        #region DeletePhoto
        public void DeletePhoto(int photoID)
        {
            dal.DeletePhoto(photoID);
        }
        #endregion

        #region GetPhotoByID
        public PhotoObj GetPhotoByID(int photoID)
        {
            return dal.GetPhotoByID(photoID);
        }
        #endregion

        #region GetPhotos
        public JsonArray GetPhotos(IList<int> categoryIDs, string keywords, int page, int pageSize, out int total)
        {
            return dal.GetPhotos(categoryIDs, keywords, page, pageSize, out total);
        }

        public JsonArray GetPhotos(IList<int> categoryIDs)
        {
            return dal.GetPhotos(categoryIDs);
        }
        #endregion

        #region AddPhotoPoint
        public void AddPhotoPoint(PhotoPointObj photoPointObj)
        {
            dal.AddPhotoPoint(photoPointObj);
        }
        #endregion

        #region ModifyPhotoPoint
        public void ModifyPhotoPoint(PhotoPointObj photoPointObj)
        {
            dal.ModifyPhotoPoint(photoPointObj);
        }
        #endregion

        #region DeletePhotoPoint
        public void DeletePhotoPoint(int pointID)
        {
            dal.DeletePhotoPoint(pointID);
        }
        #endregion

        #region GetPhotoPoints
        public JsonArray GetPhotoPoints(int photoID)
        {
            return dal.GetPhotoPoints(photoID);
        }
        #endregion

        #region GetPhotoPointByID
        public PhotoPointObj GetPhotoPointByID(int pointID)
        {
            return dal.GetPhotoPointByID(pointID);
        }
        #endregion
    }
}
