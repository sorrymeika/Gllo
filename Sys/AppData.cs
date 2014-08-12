using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

using Gllo.Models;

namespace Gllo.Sys
{
    public class AppData
    {
        public static UserObj SessionUser
        {
            get
            {
                if (HttpContext.Current.Session["User"] == null)
                    return null;

                return (UserObj)HttpContext.Current.Session["User"];
            }
            set
            {
                HttpContext.Current.Session["User"] = value;
            }
        }

        public static int SessionUserID
        {
            get
            {
                return SessionUser == null ? 0 : SessionUser.UserID;
            }
        }

        public static IDictionary<string, byte[]> SessionPicViews
        {
            get
            {
                IDictionary<string, byte[]> pics;
                if (HttpContext.Current.Session["PicViews"] == null)
                {
                    pics = new Dictionary<string, byte[]>();
                    HttpContext.Current.Session["PicViews"] = pics;
                }
                else
                    pics = (IDictionary<string, byte[]>)HttpContext.Current.Session["PicViews"];

                return pics;
            }
            set
            {
                HttpContext.Current.Session["PicViews"] = value;
            }
        }

        public static IList<int[]> GetBuy(string v)
        {
            IList<int[]> products = new List<int[]>();
            string[] cart = v.Split(',');
            for (int i = 0; i < cart.Length; i++)
            {
                if (string.IsNullOrEmpty(cart[i]))
                {
                    continue;
                }
                string[] nameValue = cart[i].Split('|');
                int[] res = new int[nameValue.Length];
                for (int j = 0; j < nameValue.Length; j++)
                {
                    res[j] = int.Parse(nameValue[j]);
                }
                products.Add(res);
            }
            return products;
        }

        public static IList<int[]> CookieCart
        {
            get
            {
                HttpCookie cookie = HttpContext.Current.Request.Cookies["Cart"];
                if (cookie == null || string.IsNullOrEmpty(cookie.Value))
                    return null;

                return GetBuy(cookie.Value);
            }
            set
            {
                HttpCookie cookie = new HttpCookie("Cart");
                cookie.HttpOnly = true;
                if (value == null)
                {
                    cookie.Expires = DateTime.Now.AddDays(-30);
                    cookie.Value = "";
                }
                else
                {
                    StringBuilder cart = new StringBuilder();
                    int i = 0;
                    foreach (int[] item in value)
                    {
                        if (i != 0)
                            cart.Append(",");

                        for (int j = 0; j < item.Length; j++)
                        {
                            if (j != 0)
                                cart.Append("|");
                            cart.Append(item[j]);
                        }
                        i++;
                    }
                    cookie.Expires = DateTime.Now.AddDays(30);
                    cookie.Value = cart.ToString();
                }
                HttpContext.Current.Response.SetCookie(cookie);
            }
        }

        public static bool IsManagerLogin
        {
            get
            {
                UserObj user = SessionUser;
                return user != null && (user.UserRole == UserRoles.Manager || user.UserRole == UserRoles.System || user.UserRole == UserRoles.Administrator);
            }
        }


    }
}
