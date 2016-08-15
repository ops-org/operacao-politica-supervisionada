using System.Web;
using System.Web.Optimization;

namespace OPS
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			// Vai apenas unificar os arquivos em 1
			bundles.Add(new Bundle("~/Content/vendor")
				.Include("~/Content/scripts/jquery/jquery-2.2.3.min.js")
				.Include("~/Content/scripts/ng/angular.min.js")
				.Include("~/Content/scripts/ng/angular-route.min.js")
				.Include("~/Content/scripts/ng/angular-cookies.min.js")
				.Include("~/Content/scripts/ng/angular-sanitize.min.js")
				.Include("~/Content/scripts/ng/angular-resource.min.js")
				.Include("~/Content/scripts/bootstrap/bootstrap.min.js")
				.Include("~/Content/scripts/misc/ui-bootstrap.min.js")
				.Include("~/Content/scripts/jquery/select2.min.js"));

			// Vai unificar e minificar
			bundles.Add(new ScriptBundle("~/Content/app")
				.Include("~/Content/scripts/ng/ng-table.js")
				.Include("~/Content/scripts/app/app.js")
				.Include("~/Content/scripts/app/main.js")
				.IncludeDirectory("~/Content/scripts/app/controllers/", "*.js", true));

			//bundles.Add(new ScriptBundle("~/bundles/fiscalize")
			//	.Include("~/Content/scripts/app/app.js")
			//	.IncludeDirectory("~/Content/scripts/app/controllers/", "*.js", true));

			bundles.Add(new StyleBundle("~/Content/style/css")
				.Include("~/Content/styles/bootstrap.css")
				.Include("~/Content/styles/ui-bootstrap-csp.css")
				.Include("~/Content/styles/ng-table.css")
				.Include("~/Content/styles/landing-page.css")
				.Include("~/Content/styles/select2.css")
				.Include("~/Content/styles/Site.css"));

			// Set EnableOptimizations to false for debugging. For more information,
			// visit http://go.microsoft.com/fwlink/?LinkId=301862
#if DEBUG
			BundleTable.EnableOptimizations = false;
#endif
		}
	}
}
