using System.Web;
using System.Web.Optimization;

namespace OPS
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/vendor")
				.Include("~/Content/scripts/jquery/jquery-{version}.js")
				.Include("~/Content/scripts/ng/angular.js")
				.Include("~/Content/scripts/ng/angular-route.js")
				.Include("~/Content/scripts/ng/angular-cookies.js")
				.Include("~/Content/scripts/ng/angular-sanitize.js")
				.Include("~/Content/scripts/ng/angular-resource.js")
				.Include("~/Content/scripts/bootstrap/bootstrap.js")
				.Include("~/Content/scripts/misc/ui-bootstrap.js")
				.Include("~/Content/scripts/jquery/select2.js")
				.Include("~/Content/scripts/ng/ng-table.js"));

			bundles.Add(new ScriptBundle("~/bundles/app")
				.Include("~/Content/scripts/app/app.js")
				.IncludeDirectory("~/Content/scripts/app/controllers/", "*.js", true));

			//bundles.Add(new ScriptBundle("~/bundles/fiscalize")
			//	.Include("~/Content/scripts/app/app.js")
			//	.IncludeDirectory("~/Content/scripts/app/controllers/", "*.js", true));

			bundles.Add(new StyleBundle("~/bundles/styles")
				.Include("~/Content/styles/bootstrap.css")
				.Include("~/Content/styles/ui-bootstrap-csp.css")
				.Include("~/Content/styles/ng-table.css")
				.Include("~/Content/styles/landing-page.css")
				.Include("~/Content/styles/select2.css")
				.Include("~/Content/styles/Site.css"));

			// Set EnableOptimizations to false for debugging. For more information,
			// visit http://go.microsoft.com/fwlink/?LinkId=301862
			BundleTable.EnableOptimizations = false;
		}
	}
}
