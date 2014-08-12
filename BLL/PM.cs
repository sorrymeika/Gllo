using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gllo.Models;

namespace Gllo.BLL
{
    public class PM
    {
        private readonly DAL.PM dal = new DAL.PM();

        public void AddPM(PMInfo pmInfo)
        {
            dal.AddPM(pmInfo);
        }

        public void ModifyPM(PMInfo pmInfo)
        {
            dal.ModifyPM(pmInfo);
        }

        public void DeletePM(int pmID)
        {
            dal.DeletePM(pmID);
        }

        public PMInfo GetPM(int pmID)
        {
            return dal.GetPM(pmID);
        }

        public JsonArray GetPM(int page, int pageSize, out int total,
            int? userID = null,
            string userName = null,
            DateTime? dtFrom = null,
            DateTime? dtTo = null)
        {
            return dal.GetPM(page, pageSize, out total, userID, userName, dtFrom, dtTo);
        }

        public int CountNotRead(int userID)
        {
            return dal.CountNotRead(userID);
        }

        public void ReadPM(int pmID, int userID)
        {
            dal.ReadPM(pmID, userID);
        }
    }
}
