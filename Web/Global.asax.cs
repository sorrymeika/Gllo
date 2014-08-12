using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gllo.Web
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Entrance",
                "",
                new { controller = "Home", action = "Entrance" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Index",
                "Index.html",
                new { controller = "Home", action = "Index" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Logout",
                "Logout",
                new { controller = "Home", action = "Logout" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Cart",
                "Cart.html",
                new { controller = "Order", action = "Cart" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Order",
                "Order.html",
                new { controller = "Order", action = "Order" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "OrderInfo",
                "Order/{id}.html",
                new { controller = "Order", action = "OrderInfo", id = @"\d+" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Survey",
                "Survey/{id}.html",
                new { controller = "Order", action = "Survey", id = @"\d+" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "ReturnExchange",
                "ReturnExchange/{id}.html",
                new { controller = "Order", action = "ReturnExchange", id = @"\d+" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "UserExchange",
                "UserExchange/{id}.html",
                new { controller = "User", action = "UserExchange", id = @"\d+" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "ProductIndex",
                "Product.html",
                new { controller = "Product", action = "Index" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Show",
                "Show/{id}.html",
                new { controller = "Package", action = "Index", id = @"^\d+(_\d+){0,1}$" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "ShowDefault",
                "Show.html",
                new { controller = "Package", action = "Index", id = 1 },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Package",
                "Package/{id}.html",
                new { controller = "Package", action = "Package", id = @"^\d+$" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "PotoT",
                "Photo/{id}/PotoT.html",
                new { controller = "Package", action = "PotoT", id = @"^\d+$" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "PotoTSwf",
                "Photo/{id}/PotoT.swf",
                new { controller = "Package", action = "PotoTSwf", id = @"^\d+$" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "PotoT1XML",
                "Photo/{id}/1.xml",
                new { controller = "Package", action = "PotoT1XML", id = @"^\d+$" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "PotoT2XML",
                "Photo/{id}/2.xml",
                new { controller = "Package", action = "PotoT2XML", id = @"^\d+$" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "PotoT3XML",
                "Photo/{id}/3.xml",
                new { controller = "Package", action = "PotoT3XML", id = @"^\d+$" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "PotoT4XML",
                "Photo/{id}/4.xml",
                new { controller = "Package", action = "PotoT4XML", id = @"^\d+$" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Search",
                "Search/{id}",
                new { controller = "Product", action = "Search", id = UrlParameter.Optional },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Sale",
                "Sale.html",
                new { controller = "Product", action = "OnSale" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Product",
                "Product/{id}.html",
                new { controller = "Product", action = "Product", id = @"\d+" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "ProductList",
                "List/{id}.html",
                new { controller = "Product", action = "Cates", id = @"^\d+(_\d+){0,1}$" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Account",
                "Account.html",
                new { controller = "User", action = "Account" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Register",
                "Register.html",
                new { controller = "User", action = "Register" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Login",
                "Login.html",
                new { controller = "User", action = "Login" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "HelpCenter",
                "help/{id}.html",
                new { controller = "News", action = "HelpCenter", id = @"\d+" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "News",
                "news/{id}.html",
                new { controller = "News", action = "News", id = @"\d+" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "PMList",
                "PM/List.html",
                new { controller = "User", action = "PMList" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "PM",
                "PM/{id}.html",
                new { controller = "User", action = "PM", id = @"\d+" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "CheckCode",
                "CheckCode/{id}.jpg",
                new { controller = "Home", action = "CheckCode", id = @"\d+" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Error",
                "Error/{id}.html",
                new { controller = "Home", action = "Error", id = @"\d+" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "DefaultHtml",
                "{controller}/{action}.html",
                new { controller = "Home", action = "Index" },
                new string[] { "Gllo.Web.Controllers" }
            );

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new string[] { "Gllo.Web.Controllers" }
            );


        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}