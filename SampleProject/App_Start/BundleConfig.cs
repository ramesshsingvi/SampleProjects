/*
' Copyright (c) 2017  Blueclover Consulting Ltd
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System.Web;
using System.Web.Optimization;
using TrustonTap.Web.App_Start;

namespace TrustonTap.Web
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.js",
                        "~/Scripts/globalize.js",
                        "~/Scripts/globalize/currency.js",
                        "~/Scripts/globalize/number.js",
                        "~/Scripts/globalize/date.js",
                        "~/Scripts/globalize/plural.js",
                        "~/Scripts/globalize/relative-time.js",
                        "~/Scripts/globalize/unit.js",
                        "~/Scripts/jquery.validate.globalize.js"));

            bundles.Add(new ScriptBundle("~/bundles/gridmvc").Include(
                        "~/Scripts/gridmvc.js"));

            var ckBundle = new ScriptBundle("~/bundles/ckeditor").Include(
                        "~/ckeditor/ckeditor.js",
                        "~/ckeditor/adapters/jquery.js");
            ckBundle.Orderer = new PassthruBundleOrderer();

            bundles.Add(ckBundle);

            bundles.Add(new ScriptBundle("~/bundles/timesheet").Include(
                        "~/Scripts/jquery.plugin.min.js",
                        "~/Scripts/jquery.timeentry.min.js",
                        "~/Scripts/timesheet.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
						"~/Scripts/modernizr-*"));

			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
					  "~/Scripts/bootstrap.js",
                      "~/Scripts/bootstrap-checkbox.js",
                      "~/Scripts/bootstrap-confirmation.js",
                      "~/Scripts/bootstrap-datepicker.js",
                      "~/Scripts/select2.min.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/custom").Include(
                      "~/Scripts/custom.js"));

            bundles.Add(new ScriptBundle("~/bundles/maxlength").Include(
                      "~/Scripts/MaxLength.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-checkbox.css",
                      "~/Content/fontawesome-all.css",
                      "~/Content/bootstrap-datepicker.css",
                      "~/Content/Gridmvc.css",
                      "~/Content/select2.css",
                      "~/Content/site.css"));
        }
	}
}
