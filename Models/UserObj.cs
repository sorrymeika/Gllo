using System;
using System.Collections.Generic;
using System.Text;

namespace Gllo.Models
{
    [Serializable]
    public class UserList : List<UserObj>
    {
    }

    [Serializable]
    public enum UserRoles
    {
        Customer,
        Vip,
        Manager,
        Administrator,
        System,
    }

    [Serializable]
    public class UserObj
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string SafeCode { get; set; }
        public string Password { get; set; }
        public UserRoles UserRole { get; set; }
        public string NickName { get; set; }
        public bool Gender { get; set; }
        public string Email { get; set; }
        public DateTime BirthDay { get; set; }
        public DateTime RegDate { get; set; }
        public DateTime LastLoginTime { get; set; }
        public int GroupID { get; set; }
        public int Points { get; set; }
    }

    [Serializable]
    public class AddressObj
    {
        public int AddressID { get; set; }
        public int UserID { get; set; }
        public string Address { get; set; }
        public string Zip { get; set; }
        public string Receiver { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public int CityID { get; set; }
        public int RegionID { get; set; }
    }

}
