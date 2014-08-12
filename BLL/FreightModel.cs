using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gllo.Models;

namespace Gllo.BLL
{
    public class FreightModel
    {
        private readonly Gllo.DAL.FreightModel dal = new DAL.FreightModel();

        public void AddModel(FreightModelInfo modelInfo)
        {
            dal.AddModel(modelInfo);
        }

        public void ModifyModel(FreightModelInfo modelInfo)
        {
            dal.ModifyModel(modelInfo);
        }

        public FreightModelInfo GetModel(int modelID)
        {
            return dal.GetModel(modelID);
        }

        public JsonArray GetModels(int page, int pageSize, out int total, int expressID, string keywords)
        {
            return dal.GetModels(page, pageSize, out  total, expressID, keywords);
        }

        public JsonArray GetModels()
        {
            return dal.GetModels();
        }

        public void DeleteFreightModel(int id)
        {
            dal.DeleteFreightModel(id);
        }
    }
}
