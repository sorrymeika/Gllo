using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gllo.Models;
using System.Data.SqlClient;
using System.Data;

namespace Gllo.DAL
{
    public class FreightModel
    {
        private SqlHelper dbc;

        public void AddModel(FreightModelInfo modelInfo)
        {
            using (dbc = new SqlHelper())
            {
                SqlParameter modelIDParam = dbc.AddOutputParameter("@ModelID");
                dbc.AddStringParameter("@ModelName", 200, modelInfo.ModelName);
                dbc.AddIntParameter("@ExpressID", modelInfo.ExpressID);
                dbc.AddDecimalParameter("@Freight", modelInfo.Freight);
                dbc.AddDecimalParameter("@Freight1", modelInfo.Freight1);

                dbc.BeginTran();

                dbc.ExecuteNonQuery("insert into FreightModel (ModelName,ExpressID,Freight,Freight1) values (@ModelName,@ExpressID,@Freight,@Freight1) select @ModelID=@@IDENTITY", System.Data.CommandType.Text);
                modelInfo.ModelID = (int)modelIDParam.Value;

                bool result = modelInfo.AreaFreightList.All<AreaFreightInfo>(info =>
                {
                    dbc.ClearParameters();
                    dbc.AddIntParameter("@ModelID", modelInfo.ModelID);
                    dbc.AddStringParameter("@AreaID", 400, info.AreaID);
                    dbc.AddTinyIntParameter("@AreaType", info.AreaType);
                    dbc.AddDecimalParameter("@Freight", info.Freight);
                    dbc.AddDecimalParameter("@Freight1", info.Freight1);

                    var ef = dbc.ExecuteNonQuery("insert into AreaFreight (ModelID,AreaID,AreaType,Freight,Freight1) values (@ModelID,@AreaID,@AreaType,@Freight,@Freight1)", System.Data.CommandType.Text);
                    return ef == 1;
                });

                if (!result)
                    dbc.RollbackTran();
                else
                    dbc.CommitTran();
            }
        }

        public void ModifyModel(FreightModelInfo modelInfo)
        {
            using (dbc = new SqlHelper())
            {
                dbc.BeginTran();

                dbc.AddIntParameter("@ModelID", modelInfo.ModelID);
                dbc.AddIntParameter("@ExpressID", modelInfo.ExpressID);
                dbc.AddStringParameter("@ModelName", 200, modelInfo.ModelName);
                dbc.AddDecimalParameter("@Freight", modelInfo.Freight);
                dbc.AddDecimalParameter("@Freight1", modelInfo.Freight1);

                dbc.ExecuteNonQuery("update FreightModel set ModelName=@ModelName,ExpressID=@ExpressID,Freight=@Freight,Freight1=@Freight1 where ModelID=@ModelID");
                dbc.ExecuteNonQuery("delete from AreaFreight where ModelID=@ModelID");

                bool result = modelInfo.AreaFreightList.All<AreaFreightInfo>(info =>
                {
                    dbc.ClearParameters();
                    dbc.AddIntParameter("@ModelID", modelInfo.ModelID);
                    dbc.AddStringParameter("@AreaID", 400, info.AreaID);
                    dbc.AddTinyIntParameter("@AreaType", info.AreaType);
                    dbc.AddDecimalParameter("@Freight", info.Freight);
                    dbc.AddDecimalParameter("@Freight1", info.Freight1);

                    var ef = dbc.ExecuteNonQuery("insert into AreaFreight (ModelID,AreaID,AreaType,Freight,Freight1) values (@ModelID,@AreaID,@AreaType,@Freight,@Freight1)", System.Data.CommandType.Text);
                    return ef == 1;
                });

                if (!result)
                    dbc.RollbackTran();
                else
                    dbc.CommitTran();
            }
        }

        public JsonArray GetModels()
        {
            using (dbc = new SqlHelper())
            {
                var result = dbc.GetJsonArray("select ExpressID,ExpressName,ExpressUrl from Express where DeletedFlag=0", CommandType.Text);
                if (result != null)
                {
                    result.ForEach(obj =>
                    {
                        dbc.ClearParameters();
                        dbc.AddIntParameter("@ExpressID", (int)obj["ExpressID"]);
                        var children = dbc.GetJsonArray("select ModelID,ModelName,ExpressID,Freight,Freight1 from FreightModel where ExpressID=@ExpressID", CommandType.Text);
                        if (children != null)
                        {
                            obj.Add("FreightModels", children);
                        }
                    });
                }
                return result;
            }
        }

        public JsonArray GetModels(int page, int pageSize, out int total, int expressID, string keywords)
        {
            using (dbc = new SqlHelper())
            {
                string where = "Express.DeletedFlag=0";
                if (expressID != 0)
                {
                    dbc.AddIntParameter("@ExpressID", expressID);
                    where += " and FreightModel.ExpressID=@ExpressID";
                }
                if (!string.IsNullOrEmpty(keywords))
                {
                    dbc.AddStringParameter("@Keywords", 50, keywords);
                    where += " and ModelName like '%'+@Keywords+'%'";
                }
                var result = dbc.GetJsonArray("ModelID",
                    "ModelID,ModelName,FreightModel.ExpressID,Express.ExpressName,Freight,Freight1",
                    "FreightModel inner join Express on FreightModel.ExpressID=Express.ExpressID",
                    where,
                    page,
                    pageSize,
                    out total,
                    "ModelID",
                    false);
                if (result != null)
                {
                    result.ForEach(obj =>
                    {
                        dbc.ClearParameters();
                        dbc.AddIntParameter("@ModelID", (int)obj["ModelID"]);
                        var children = dbc.GetJsonArray("select AreaID,AreaType,Freight,Freight1 from AreaFreight where ModelID=@ModelID", CommandType.Text);
                        if (children != null)
                        {
                            obj.Add("children", children);
                        }
                    });
                }

                return result;
            }
        }

        public FreightModelInfo GetModel(int modelID)
        {
            using (dbc = new SqlHelper())
            {
                dbc.AddIntParameter("@ModelID", modelID);

                FreightModelInfo result = null;

                dbc.Read("select ModelName,ExpressID,Freight,Freight1 from FreightModel where ModelID=@ModelID",
                    CommandType.Text,
                    dr =>
                    {
                        if (dr.HasRows && dr.Read())
                        {
                            result = new FreightModelInfo()
                            {
                                ModelID = modelID,
                                ModelName = dr[0] == DBNull.Value ? null : (string)dr[0],
                                ExpressID = dr[1] == DBNull.Value ? 0 : (int)dr[1],
                                Freight = dr[2] == DBNull.Value ? 0 : (decimal)dr[2],
                                Freight1 = dr[3] == DBNull.Value ? 0 : (decimal)dr[3],
                                AreaFreightList = new List<AreaFreightInfo>(),
                            };
                        }
                    });
                if (result == null)
                    return null;

                dbc.Read("select AreaID,AreaType,Freight,Freight1 from AreaFreight where ModelID=@ModelID",
                    CommandType.Text,
                    dr =>
                    {
                        while (dr.Read())
                        {
                            result.AreaFreightList.Add(new AreaFreightInfo()
                            {
                                ModelID = modelID,
                                AreaID = dr[0] == DBNull.Value ? null : (string)dr[0],
                                AreaType = dr[1] == DBNull.Value ? (byte)0 : (byte)dr[1],
                                Freight = dr[2] == DBNull.Value ? 0 : (decimal)dr[2],
                                Freight1 = dr[3] == DBNull.Value ? 0 : (decimal)dr[3],
                            });
                        }
                    });

                return result;
            }
        }

        public void DeleteFreightModel(int id)
        {
            using (dbc = new SqlHelper())
            {
                dbc.AddIntParameter("@ModelID", id);

                dbc.BeginTran();

                dbc.ExecuteNonQuery("delete from FreightModel where ModelID=@ModelID");
                dbc.ExecuteNonQuery("delete from AreaFreight where ModelID=@ModelID");
                dbc.ExecuteNonQuery("delete from ProductFreight where ModelID=@ModelID", CommandType.Text);

                dbc.CommitTran();
            }
        }
    }
}
