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
        $scope.inputRegion = [];
        $scope.outputRegion = [];
        $scope.inputChannel = [];
        $scope.outputChannel = [];
        $scope.inputShowroom = [];
        $scope.outputShowroom = [];
        $scope.yearlylSales = [];
        $scope.salesHeader = [];
        $scope.salesData = [];
        $scope.brandType = true;
        $scope.categoryType = false;
        $scope.productType = false;
        $scope.regionType = false;
        $scope.showroomType = false;
        $scope.showChart = false;
        $scope.salesType = 'yearly';
        $scope.chartFilter = 'Brand';
        $scope.currentYear = currentYear;
        $scope.yearItemIsDisabled = false;
        $scope.quarterItemIsDisabled = true;
        $scope.monthItemIsDisabled = true;
        $scope.currentMonth = monthNames[new Date().getMonth()];

        //drop down filling
        $scope.inputSalesYear.push({ year: parseInt(currentYear), ticked: true });
        for (var i = parseInt(currentYear - 1) ; i >= 2000; i--) {
            $scope.inputSalesYear.push({ year: i, ticked: false });
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
                showlegend: "0",
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

        //apiServices
        $scope.loadInitData = function () {
            $scope.outputSalesYear = [{ year: parseInt(currentYear), ticked: true }];
            $scope.filterSales();
        }

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

        function loadRegion() {
            apiService.get('/api/sales/region/#', null,
                        regionDataLoadCompleted,
                        dataLoadFailed);
        }

        function regionDataLoadCompleted(result) {
            $scope.inputRegion = result.data.regions;
        }

        function loadChannel() {
            apiService.get('/api/sales/channel/#', null,
                        channelDataLoadCompleted,
                        dataLoadFailed);
        }

        function channelDataLoadCompleted(result) {
            $scope.inputChannel = result.data.channels;
        }

        function loadShowroom() {
            apiService.get('/api/sales/showroom/#', null,
                        showroomDataLoadCompleted,
                        dataLoadFailed);
        }

        function showroomDataLoadCompleted(result) {
            $scope.inputShowroom = result.data.showrooms;
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
            var searchCriteria = [];
            var reportType = [];
            var brandIds = '', categoryIds = '', productIds = '', regionIds = '', channelIds = '', showroomIds = '';

            $.each($scope.outputBrands, function () {
                brandIds += this.brandId + ',';
            })

            $.each($scope.outputProductCategories, function () {
                categoryIds += this.categoryId + ',';
            })

            $.each($scope.outputProducts, function () {
                productIds += this.productId + ',';
            })

            $.each($scope.outputRegion, function () {
                regionIds += this.regionId + ',';
            })

            $.each($scope.outputChannel, function () {
                channelIds += this.channelId + ',';
            })

            $.each($scope.outputShowroom, function () {
                showroomIds += this.showroomId + ',';
            })

            this.reportType = {};

            searchCriteria = {
                brandIds: brandIds, categoryIds: categoryIds, productIds: productIds, regionIds: regionIds, channelIds: channelIds, showroomIds: showroomIds,
                brandType: $scope.brandType, categoryType: $scope.categoryType, productType: $scope.productType, regionType: $scope.regionType, showroomType: $scope.showroomType
            };

            apiService.post('/api/sales/yearly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
                        yearlySalesDataLoadCompleted,
                        dataLoadFailed);
        }

        function yearlySalesDataLoadCompleted(result) {
            var yearlysalesCategory = [];
            var yearlysalesData = [];
            var test = [];

            $scope.salesHeader = [];
            $scope.yearlylSales = [];
            $scope.showChart = true;
            $scope.yearlylSales = result.data.table;

            $.each(result.data.table, function (k, v) {
                yearlysalesCategory.push({ label: v[$scope.chartFilter] });
                yearlysalesData.push({ label: v[$scope.chartFilter], value: v["Share"] });
            });

            $scope.yearlysalesdataset.category = yearlysalesCategory;
            $scope.yearlysalesdataset.data = yearlysalesData;

            $.each($scope.yearlylSales, function (k, v) {
                test = (k, v);
            })

            for (var headerName in test) {
                $scope.salesHeader.push(headerName);
            }
        }

        $scope.changeSalesType = function () {
            $scope.outputSalesYear = [];
            $scope.outputSalesQuarter = [];
            $scope.outputSalesMonth = [];
            if ($scope.salesType == 'yearly') {
                $scope.yearItemIsDisabled = false;
                $scope.quarterItemIsDisabled = true;
                $scope.monthItemIsDisabled = true;
            }
            if ($scope.salesType == 'quarterly') {
                $scope.yearItemIsDisabled = false;
                $scope.quarterItemIsDisabled = false;
                $scope.monthItemIsDisabled = true;
            }
            if ($scope.salesType == 'monthly') {
                $scope.yearItemIsDisabled = false;
                $scope.quarterItemIsDisabled = true;
                $scope.monthItemIsDisabled = false;
            }
        }

        //common data load fail function
        function dataLoadFailed(response) {
            notificationService.displayError(response.data);
        }

        //function calling
        loadBrand();
        loadProductCategory();
        loadProduct();
        loadRegion();
        loadChannel();
        loadShowroom();
        //library initialization
    }

})(angular.module('salesVentana'));