using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;
using System.Data;
using System.Data.SqlClient;

namespace Gllo.DAL
{
    public class MediaDAL
    {
        SqlHelper helper;

        public int SetMedia(MediaObj mediaObj)
        {
            using (helper = new SqlHelper())
            {
                string sql = "if exists (select 1 from Media where MediaID=@MediaID) update Media set Description=@Description,Src=@Src,Type=@Type,SavePath=@SavePath,Url=@Url,Content=@Content where MediaID=@MediaID else insert into Media (MediaID,Description,Src,Type,SavePath,Url,Content) values (@MediaID,@Description,@Src,@Type,@SavePath,@Url,@Content)";

                helper.AddIntParameter("@MediaID", mediaObj.MediaID);
                helper.AddStringParameter("@Description", 200, mediaObj.Description);
                helper.AddStringParameter("@Src", 200, mediaObj.Src);
                helper.AddIntParameter("@Type", mediaObj.Type);
                helper.AddStringParameter("@SavePath", 200, mediaObj.SavePath);
                helper.AddStringParameter("@Url", 200, mediaObj.Url);
                helper.AddStringParameter("@Content", 2000, mediaObj.Content);

                return helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }

        public MediaObj GetMedia(int mediaID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@MediaID", mediaID);
                string sql = "select Description,Src,Type,SavePath,Url,Content from Media where MediaID=@MediaID";
                using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        MediaObj mediaObj = new MediaObj();

                        mediaObj.MediaID = mediaID;
                        mediaObj.Description = dr[0] == DBNull.Value ? null : (string)dr[0];
                        mediaObj.Src = dr[1] == DBNull.Value ? null : (string)dr[1];
                        mediaObj.Type = dr[2] == DBNull.Value ? 0 : (int)dr[2];
                        mediaObj.SavePath = dr[3] == DBNull.Value ? null : (string)dr[3];
                        mediaObj.Url = dr[4] == DBNull.Value ? null : (string)dr[4];
                        mediaObj.Content = dr[5] == DBNull.Value ? null : (string)dr[5];

                        return mediaObj;
                    }
                    return null;
                }
            }
        }
    }
}
