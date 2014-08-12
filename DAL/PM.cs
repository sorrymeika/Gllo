using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gllo.Models;
using System.Data;

namespace Gllo.DAL
{
    public class PM
    {
        private SqlHelper dbc;

        public void AddPM(PMInfo pmInfo)
        {
            using (dbc = new SqlHelper())
            {
                dbc.AddIntParameter("@UserID", pmInfo.UserID);
                dbc.AddIntParameter("@UserIDFrom", pmInfo.UserIDFrom);
                dbc.AddDateTimeParameter("@PMTime", pmInfo.PMTime);
                dbc.AddStringParameter("@Title", 400, pmInfo.Title);
                dbc.AddTextParameter("@Content", pmInfo.Content);

                dbc.ExecuteNonQuery("insert into PM (UserID,UserIDFrom,PMTime,Title,Content) values (@UserID,@UserIDFrom,@PMTime,@Title,@Content)", CommandType.Text);
            }
        }

        public void ModifyPM(PMInfo pmInfo)
        {
            using (dbc = new SqlHelper())
            {
                dbc.AddIntParameter("@PMID", pmInfo.PMID);
                dbc.AddIntParameter("@UserIDFrom", pmInfo.UserIDFrom);
                dbc.AddDateTimeParameter("@PMTime", pmInfo.PMTime);
                dbc.AddStringParameter("@Title", 400, pmInfo.Title);
                dbc.AddTextParameter("@Content", pmInfo.Content);

                dbc.ExecuteNonQuery("update PM set UserIDFrom=@UserIDFrom,PMTime=@PMTime,Title=@Title,Content=@Content where PMID=@PMID", CommandType.Text);
            }
        }

        public void DeletePM(int pmID)
        {
            using (dbc = new SqlHelper())
            {
                dbc.AddIntParameter("@PMID", pmID);
                dbc.ExecuteNonQuery("delete from PM where PMID=@PMID", CommandType.Text);
            }
        }

        public PMInfo GetPM(int pmID)
        {
            using (dbc = new SqlHelper())
            {
                dbc.AddIntParameter("@PMID", pmID);
                PMInfo pmInfo = null;
                dbc.Read("select PMID,UserID,UserIDFrom,PMTime,Title,Content from PM where PMID=@PMID",
                    CommandType.Text,
                    dr =>
                    {
                        if (dr.HasRows && dr.Read())
                        {
                            pmInfo = new PMInfo()
                            {
                                PMID = (int)dr[0],
                                UserID = (int)dr[1],
                                UserIDFrom = dr[2] == DBNull.Value ? 0 : (int)dr[2],
                                PMTime = dr[3] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[3],
                                Title = dr[4] == DBNull.Value ? null : (string)dr[4],
                                Content = dr[5] == DBNull.Value ? null : (string)dr[5],
                            };
                        }
                    });

                return pmInfo;
            }
        }

        public int CountNotRead(int userID)
        {
            using (dbc = new SqlHelper())
            {
                dbc.AddIntParameter("@UserID", userID);
                return dbc.GetIntValue("select count(1) from (select PMID,@UserID as UserID from PM where UserID=@UserID or PM.UserID=0) a left join PMRead b on a.PMID=b.PMID and a.UserID=b.UserID where b.PMID is null", CommandType.Text);
            }
        }

        public void ReadPM(int pmID, int userID)
        {
            using (dbc = new SqlHelper())
            {
                dbc.AddIntParameter("@UserID", userID);
                dbc.AddIntParameter("@PMID", pmID);
                dbc.ExecuteNonQuery("if not exists (select 1 from PMRead where UserID=@UserID and PMID=@PMID) insert into PMRead (UserID,PMID) values (@UserID,@PMID)", CommandType.Text);
            }
        }

        public JsonArray GetPM(int page, int pageSize, out int total,
            int? userID = null,
            string userName = null,
            DateTime? dtFrom = null,
            DateTime? dtTo = null)
        {
            using (dbc = new SqlHelper())
            {
                StringBuilder where = new StringBuilder("1=1");
                if (userID != null)
                {
                    dbc.AddIntParameter("@UserID", userID.Value);
                    where.Append(" and PM.UserID=@UserID");
                }
                else if (!string.IsNullOrEmpty(userName))
                {
                    dbc.AddStringParameter("@UserName", 50, userName);
                }

                if (dtFrom != null && dtFrom.Value != DateTime.MinValue && dtTo != null && dtTo.Value != DateTime.MinValue)
                {
                    dbc.AddDateTimeParameter("@PMTimeFrom", dtFrom.Value);
                    dbc.AddDateTimeParameter("@PMTimeTo", dtTo.Value);
                    where.Append(" and PMTime between @PMTimeFrom and @PMTimeTo");
                }
                where.Append(" or PM.UserID=0");

                return dbc.GetJsonArray("PMID",
                    "PMID,PM.UserID,UserIDFrom,PMTime,PM.Title,PM.Content,Users.UserName",
                    "PM left join Users on Users.UserID=PM.UserID",
                    where.ToString(),
                    page,
                    pageSize,
                    out total,
                    "PMTime",
                    false);
            }
        }
    }
}
