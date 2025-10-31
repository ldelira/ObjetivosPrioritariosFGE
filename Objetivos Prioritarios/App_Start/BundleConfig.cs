using System.Web;
using System.Web.Optimization;

namespace Objetivos_Prioritarios
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Utilice la versión de desarrollo de Modernizr para desarrollar y obtener información sobre los formularios.  De esta manera estará
            // para la producción, use la herramienta de compilación disponible en https://modernizr.com para seleccionar solo las pruebas que necesite.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/bundles/css2")
               ////.Include("~/assets/plugins/bootstrap/css/bootstrap.min.css")
               .Include("~/assets/plugins/font-awesome/css/font-awesome.min.css")
               //.Include("~/assets/plugins/flexslider/flexslider.css")
               //.Include("~/assets/css/animate.css")
               //.Include("~/assets/plugins/masterslider/style/masterslider.css")
               //.Include("~/assets/plugins/masterslider/skins/default/style.css")
               //-----------------------------------------------------------------------------------------
               .Include("~/assets/css/home-v5.css")
               //-----------------------------------------------------------------------------------------
               //.Include("~/assets/css/common.css")
               //.Include("~/assets/css/jquery-ui.css")
               .Include("~/assets/css/bootstrap-switch.css")
               );
            bundles.Add(new ScriptBundle("~/bundles/JQueryjs")
              .Include("~/assets/js/jquery.min.js", "~/Scripts/jquery-3.5.1.js")
              );

            bundles.Add(new ScriptBundle("~/bundles/js")
              .Include("~/assets/js/jquery-ui.js")
              .Include("~/assets/js/jquery.min.js")
              .Include("~/assets/plugins/bootstrap/js/bootstrap.min.js")
              .Include("~/assets/plugins/flexslider/jquery.flexslider-min.js")
              .Include("~/assets/js/imagesloaded.pkgd.min.js")
              .Include("~/assets/js/masonry.pkgd.min.js")
              .Include("~/assets/js/wow.min.js")
              .Include("~/assets/js/jquery.counterup.min.js")
              .Include("~/assets/js/waypoints.min.js")
              .Include("~/assets/js/custom.js")
              .Include("~/assets/plugins/masterslider/masterslider.min.js")
              .Include("~/assets/js/Utils.js")
              .Include("~/assets/js/elementOverlay.js")
            .Include("~/assets/js/Utils.DataTable.js")
            .Include("~/assets/js/bootstrap-switch.min.js")
            );
        }
    }
}
