using System.Web;
using System.Web.Optimization;

namespace SalesVentana
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
             "~/Scripts/Vendors/modernizr.js"));

            bundles.Add(new ScriptBundle("~/bundles/vendors").Include(
            "~/Scripts/Vendors/jquery.min.js",
            "~/Scripts/Vendors/bootstrap.js",
            "~/Scripts/Vendors/toastr.js",
            "~/Scripts/Vendors/jquery.raty.js",
            "~/Scripts/Vendors/respond.src.js",
            "~/Scripts/Vendors/angular.js",
            "~/Scripts/Vendors/angular-route.js",
            "~/Scripts/Vendors/angular-cookies.js",
            "~/Scripts/Vendors/angular-validator.js",
            "~/Scripts/Vendors/angular-base64.js",
            "~/Scripts/Vendors/angular-file-upload.js",
            "~/Scripts/Vendors/angucomplete-alt.min.js",
            "~/Scripts/Vendors/angular-animate.js",
            "~/Scripts/Vendors/ui-bootstrap-tpls-0.10.0.js",
            "~/Scripts/Vendors/underscore.js",
            "~/Scripts/Vendors/raphael.js",
            "~/Scripts/Vendors/fusioncharts.js",
            "~/Scripts/Vendors/fusioncharts.charts.js",
            "~/Scripts/Vendors/fusioncharts.theme.fint.js",
            "~/Scripts/Vendors/angular-fusioncharts.min.js",
            "~/Scripts/Vendors/jquery.fancybox.js",
            "~/Scripts/Vendors/jquery.fancybox-media.js",
            "~/Scripts/Vendors/isteven-multi-select.js",
            "~/Scripts/Vendors/jquery-ui.min.js",
            "~/Scripts/Vendors/moment.min.js",
            "~/Scripts/Vendors/daterangepicker.js",
            "~/Scripts/Vendors/bootstrap-datepicker.js",
            "~/Scripts/Vendors/jquery.slimscroll.min.js",
            "~/Scripts/Vendors/fastclick.min.js",
            "~/Scripts/Vendors/loading-bar.js",
            "~/Scripts/Vendors/popeye.min.js",
            "~/Scripts/Vendors/angular-busy.min.js",
            "~/Scripts/site.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
             "~/Scripts/app/modules/common.core.js",
            "~/Scripts/app/modules/common.ui.js",
             "~/Scripts/app/app.js",
             "~/Scripts/app/services/apiService.js",
             "~/Scripts/app/services/notificationService.js",
             "~/Scripts/app/services/membershipService.js",
             "~/Scripts/app/services/fileUploadService.js",
             "~/Scripts/app/services/pathNameService.js",
             "~/Scripts/app/services/excelExportService.js",
             "~/Scripts/app/services/dataSharingService.js",
             "~/Scripts/app/services/dataHelperService.js",
             "~/Scripts/app/layout/topBar.directive.js",
             "~/Scripts/app/layout/sideBar.directive.js",
             "~/Scripts/app/layout/breadCrumb.directive.js",
             "~/Scripts/app/layout/customPager.directive.js",
             "~/Scripts/app/account/loginCtrl.js",
             "~/Scripts/app/account/registerCtrl.js",
             "~/Scripts/app/home/rootCtrl.js",
             "~/Scripts/app/home/indexCtrl.js",
             "~/Scripts/app/sales/salesCtrl.js",
             "~/Scripts/app/letterCredit/letterCreditCtrl.js",
             "~/Scripts/app/letterCredit/lcDetailCtrl.js",
             "~/Scripts/app/receivableSales/receivableSalesCtrl.js",
             "~/Scripts/app/receivableSales/receivableSalesDetailCtrl.js",
             "~/Scripts/app/project/projectCtrl.js",
             "~/Scripts/app/project/projectDetailCtrl.js",
             "~/Scripts/app/purchaseOrder/purchaseOrderCtrl.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
            "~/content/css/bootstrap.css",
            "~/content/css/font-awesome.css",
            "~/content/css/site.css",
            "~/content/css/skin-purple.css",
            "~/content/css/datepicker3.css",
            "~/content/css/isteven-multi-select.css",
            "~/content/css/daterangepicker.css",
            "~/content/css/toastr.css",
            "~/content/css/jquery.fancybox.css",
            "~/content/css/loading-bar.css",
            "~/content/css/popeye.min.css",
            "~/content/css/angular-busy.min.css"));

            BundleTable.EnableOptimizations = false;

        }
    }
}
