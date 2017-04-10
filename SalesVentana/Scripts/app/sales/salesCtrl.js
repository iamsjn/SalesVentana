(function (app) {
    'use strict';

    app.controller('salesCtrl', salesCtrl);

    salesCtrl.$inject = ['$scope', 'apiService', 'notificationService'];

    function salesCtrl($scope, apiService, notificationService) {
        //variable declaration
        var currentYear = new Date().getFullYear();
        var monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        $scope.inputSalesYear = [];
        $scope.outputSalesYear = [];
        $scope.inputSalesQuarter = [];
        $scope.outputSalesQuarter = [];
        $scope.inputSalesMonth = [];
        $scope.outputSalesMonth = [];
        $scope.inputBrands = [];
        $scope.outputBrands = [];
        $scope.inputProductCategories = [];
        $scope.outputProductCategories = [];
        $scope.inputProducts = [];
        $scope.outputProducts = [];
        $scope.currentYear = currentYear;
        $scope.currentMonth = monthNames[new Date().getMonth()];

        //drop down filling
        for (var i = parseInt(currentYear) ; i >= 2000; i--) {
            $scope.inputSalesYear.push({ year: i });
        }

        for (var i = 1 ; i <= 4; i++) {
            $scope.inputSalesQuarter.push({ quarter: 'Q' + i });
        }

        for (var i = 0 ; i < 12; i++) {
            $scope.inputSalesMonth.push({ month: '' + monthNames[i] });
        }

        //fusion chart config
        $scope.yearlysalesattrs = {
            "chart": {
                caption: "Yearly Sales Share",
                bgcolor: "FFFFFF",
                showvalues: "1",
                showpercentvalues: "1",
                showborder: "0",
                showplotborder: "0",
                showlegend: "1",
                legendborder: "0",
                legendposition: "bottom",
                enablesmartlabels: "1",
                use3dlighting: "0",
                showshadow: "0",
                legendbgcolor: "#CCCCCC",
                legendbgalpha: "20",
                legendborderalpha: "0",
                legendshadow: "0",
                legendnumcolumns: "3",
                subCaption: "",
                xAxisName: "Month",
                yAxisName: "Revenues (In USD)",
                theme: "fint"
            }
        };

        $scope.yearlysalesshareattrs = {
            "chart": {
                caption: "Yearly Sales Share of Individual Channel",
                bgcolor: "FFFFFF",
                showvalues: "1",
                showpercentvalues: "1",
                showborder: "0",
                showplotborder: "0",
                showlegend: "1",
                legendborder: "0",
                legendposition: "bottom",
                enablesmartlabels: "1",
                use3dlighting: "0",
                showshadow: "0",
                legendbgcolor: "#CCCCCC",
                legendbgalpha: "20",
                legendborderalpha: "0",
                legendshadow: "0",
                legendnumcolumns: "3",
                subCaption: "",
                xAxisName: "Month",
                yAxisName: "Revenues (In USD)",
                theme: "fint"
            }
        };

        $scope.yearlysalesdataset = { "category": [[{ "label": "AC" }]], "data": [[{ "label": "AC", "value": 1987354192 }]] };
        $scope.yearlysalessharedataset = { "category": [[{ "label": "AC" }]], "data": [[{ "label": "AC", "value": 1987354192 }]] };

        //apiServices
        function loadBrand() {
            apiService.get('/api/sales/brand/#', null,
                        brandDataLoadCompleted,
                        dataLoadFailed);
        }

        function brandDataLoadCompleted(result) {
            $scope.inputBrands = result.data.brands;
        }

        function loadProductCategory() {
            apiService.get('/api/sales/product-category/#', null,
                        productCategoryDataLoadCompleted,
                        dataLoadFailed);
        }

        function productCategoryDataLoadCompleted(result) {
            $scope.inputProductCategories = result.data.productCategories;
        }

        function loadProduct() {
            apiService.get('/api/sales/product/#', null,
                        productDataLoadCompleted,
                        dataLoadFailed);
        }

        function productDataLoadCompleted(result) {
            $scope.inputProducts = result.data.products;
        }

        $scope.brandClick = function () {
            apiService.post('/api/sales/product-category/#', $scope.outputBrands,
                        productCategoryDataLoadCompleted,
                        dataLoadFailed);
        }

        $scope.productCategoryClick = function () {
            apiService.post('/api/sales/product/#', $scope.outputProductCategories,
                        productDataLoadCompleted,
                        dataLoadFailed);
        }

        $scope.filterSales = function () {
            apiService.post('/api/sales/yearly-sales/' + parseInt($scope.outputSalesYear[0]["year"]), null,
                        yearlySalesDataLoadCompleted,
                        dataLoadFailed);
        }

        function yearlySalesDataLoadCompleted(result) {
            var yearlysalesCategory = [];
            var yearlysalesData = [];

            $.each(result.data.table, function (k, v) {
                yearlysalesCategory.push({ label: v["Brand"] });
                yearlysalesData.push({ label: v["Brand"], value: v["Share"] });
            });

            $scope.yearlysalesdataset.category = yearlysalesCategory;
            $scope.yearlysalesdataset.data = yearlysalesData;
        }

        //common data load fail function
        function dataLoadFailed(response) {
            notificationService.displayError(response.data);
        }

        //function calling
        loadBrand();
        loadProductCategory();
        loadProduct();

        //library initialization
        $(".select2").select2();
        $('input[type="checkbox"].flat-red, input[type="radio"].flat-red').iCheck({
            checkboxClass: 'icheckbox_flat-purple',
            radioClass: 'iradio_flat-purple'
        });
    }

})(angular.module('salesVentana'));