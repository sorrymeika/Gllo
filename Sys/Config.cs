using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Gllo.Sys
{
    public class Config
    {
        public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["Gllo"].ConnectionString;

        public static string Server = ConfigurationManager.AppSettings["Server"];

        public static readonly string MediaPath = ConfigurationManager.AppSettings["MediaPath"].TrimEnd('\\');

        public static string MediaServer = ConfigurationManager.AppSettings["MediaServer"].TrimEnd('/');

        public static string AlipaySellerEmail = ConfigurationManager.AppSettings["AlipaySellerEmail"];
        public static string AlipayPartner = ConfigurationManager.AppSettings["AlipayPartner"];
        public static string AlipayKey = ConfigurationManager.AppSettings["AlipayKey"];
        public static string AlipayReturnUrl = ConfigurationManager.AppSettings["AlipayReturnUrl"];
        public static string AlipayNotifyUrl = ConfigurationManager.AppSettings["AlipayNotifyUrl"];

    }
}
