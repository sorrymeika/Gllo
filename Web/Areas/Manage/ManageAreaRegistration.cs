using System.Web.Mvc;

namespace Gllo.Web.Manage
{
    public class ManageAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Manage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Manage_enter",
                "Manage/",
                new { controller = "Home", action = "Login" }
            );

            context.MapRoute(
                "Manage_error",
                "Manage/Error/{id}.html",
                new { controller = "Home", action = "Error", id = @"\d+" }
            );

            context.MapRoute(
                "Manage_html",
                "Manage/{controller}/{action}.html",
                new { controller = "Home", action = "Index" }
            );

            context.MapRoute(
                "Manage_default",
                "Manage/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
