(function (app) {
    'use strict';

    app.filter('startFrom', function () {
        return function (input, start) {
            if (input) {
                start = +start;
                return input.slice(start);
            }
            return [];
        };
    });

    app.controller('salesCtrl', salesCtrl);

    salesCtrl.$inject = ['$scope', 'apiService', 'notificationService'];

    function salesCtrl($scope, apiService, notificationService) {
        //variable declaration
        var currentYear = new Date().getFullYear();
        var monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        var chartValueType = '';
        var pathName = window.location.pathname;
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
        $scope.salesData = [];
        $scope.salesHeader = [];
        $scope.brandType = true;
        $scope.categoryType = false;
        $scope.productType = false;
        $scope.regionType = false;
        $scope.showroomType = false;
        $scope.showChart = false;
        $scope.salesType = 'Share';
        $scope.chartFilter = 'Brand';
        $scope.currentYear = currentYear;
        $scope.yearItemIsDisabled = false;
        $scope.quarterItemIsDisabled = true;
        $scope.monthItemIsDisabled = true;
        $scope.currentPage = 0;
        $scope.totalItems = 0;
        $scope.entryLimit = 0; // items per page
        $scope.noOfPages = 0;
        $scope.search = {};
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
        $scope.saleschartattrs = {
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
                xAxisName: $scope.chartFilter,
                yAxisName: "Share",
                theme: "fint"
            }
        };

        $scope.saleschartdataset = { "category": [[{ "label": "AC" }]], "data": [[{ "label": "AC", "value": 1987354192 }]] };

        //apiServices
        $scope.loadInitData = function () {
            $scope.outputSalesYear = [{ year: parseInt(currentYear), ticked: true }];
            $scope.filterSales();
        }

        function loadBrand() {
            apiService.get(pathName + '/api/sales/brand/#', null,
                        brandDataLoadCompleted,
                        dataLoadFailed);
        }

        function brandDataLoadCompleted(result) {
            $scope.inputBrands = result.data.brands;
        }

        function loadProductCategory() {
            apiService.get(pathName + '/api/sales/product-category/#', null,
                        productCategoryDataLoadCompleted,
                        dataLoadFailed);
        }

        function productCategoryDataLoadCompleted(result) {
            $scope.inputProductCategories = result.data.productCategories;
        }

        function loadProduct() {
            apiService.get(pathName + '/api/sales/product/#', null,
                        productDataLoadCompleted,
                        dataLoadFailed);
        }

        function productDataLoadCompleted(result) {
            $scope.inputProducts = result.data.products;
        }

        function loadRegion() {
            apiService.get(pathName + '/api/sales/region/#', null,
                        regionDataLoadCompleted,
                        dataLoadFailed);
        }

        function regionDataLoadCompleted(result) {
            $scope.inputRegion = result.data.regions;
        }

        function loadChannel() {
            apiService.get(pathName + '/api/sales/channel/#', null,
                        channelDataLoadCompleted,
                        dataLoadFailed);
        }

        function channelDataLoadCompleted(result) {
            $scope.inputChannel = result.data.channels;
        }

        function loadShowroom() {
            apiService.get(pathName + '/api/sales/showroom/#', null,
                        showroomDataLoadCompleted,
                        dataLoadFailed);
        }

        function showroomDataLoadCompleted(result) {
            $scope.inputShowroom = result.data.showrooms;
        }

        $scope.brandClick = function () {
            apiService.post(pathName + '/api/sales/product-category/#', $scope.outputBrands,
                        productCategoryDataLoadCompleted,
                        dataLoadFailed);
        }

        $scope.productCategoryClick = function () {
            apiService.post(pathName + '/api/sales/product/#', $scope.outputProductCategories,
                        productDataLoadCompleted,
                        dataLoadFailed);
        }

        $scope.filterSales = function () {
            var searchCriteria = [];
            var reportType = [];
            var brandIds = '', categoryIds = '', productIds = '', regionIds = '', channelIds = '', showroomIds = '';
            var salesQuarter = '';

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

            $.each($scope.outputSalesQuarter, function () {
                salesQuarter += this.quarter + ',';
            })

            this.reportType = {};

            searchCriteria = {
                brandIds: brandIds, categoryIds: categoryIds, productIds: productIds, regionIds: regionIds, channelIds: channelIds, showroomIds: showroomIds,
                brandType: $scope.brandType, categoryType: $scope.categoryType, productType: $scope.productType, regionType: $scope.regionType,
                showroomType: $scope.showroomType, salesQuarter: salesQuarter
            };

            switch ($scope.salesType) {
                case 'Share':
                    apiService.post(pathName + '/api/sales/yearly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
                        yearlySalesDataLoadCompleted,
                        dataLoadFailed);
                    break;
                case 'TotalSales(Mil)':
                    apiService.post(pathName + '/api/sales/quarterly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
                        yearlySalesDataLoadCompleted,
                        dataLoadFailed);
                    break;
                case 'TotalSalesWEE(Mil)':
                    apiService.post(pathName + '/api/sales/yearly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
                        yearlySalesDataLoadCompleted,
                        dataLoadFailed);
                    break;
                default:
                    apiService.post(pathName + '/api/sales/yearly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
                        yearlySalesDataLoadCompleted,
                        dataLoadFailed);
                    break;
            }
        }

        function yearlySalesDataLoadCompleted(result) {
            var salesChartCategory = [];
            var salesChartData = [];
            var test = [];
            var total = 0;
            $scope.salesHeader = [];
            $scope.salesData = [];
            $scope.salesDataTotal = [];
            $scope.showChart = true;
            $scope.salesData = result.data.table;
            //pagination
            $scope.currentPage = 1;
            $scope.totalItems = $scope.salesData.length;
            $scope.entryLimit = 15;
            $scope.noOfPages = Math.ceil($scope.totalItems / $scope.entryLimit);

            $.each(result.data.table, function (k, v) {
                salesChartCategory.push({ label: v[$scope.chartFilter] });
                salesChartData.push({ label: v[$scope.chartFilter], value: v[$scope.salesType] });
            });

            $scope.saleschartdataset.category = salesChartCategory;
            $scope.saleschartdataset.data = salesChartData;

            $.each($scope.salesData, function (k, v) {
                test = (k, v);
            })

            for (var header in test) {
                $scope.salesHeader.push(header);
            }

            for (var i = 0; i < $scope.salesHeader.length; i++) {
                total = 0;
                $.each($scope.salesData, function (k, v) {
                    if (!isNaN(v[$scope.salesHeader[i]])) {
                        total += v[$scope.salesHeader[i]];
                    }
                    else if (i == 0 && isNaN(v[$scope.salesHeader[i]])) {
                        $scope.salesDataTotal.push('Total');
                        return false;
                    }
                })
                if (total > 0)
                    $scope.salesDataTotal.push(total.toLocaleString());
                else if (total <= 0 && i > 0)
                    $scope.salesDataTotal.push('');
            }   
        }

        $scope.changeSalesType = function () {
            $scope.outputSalesYear = [];
            $scope.outputSalesQuarter = [];
            $scope.outputSalesMonth = [];

            if ($scope.salesType == 'Share') {
                $scope.yearItemIsDisabled = false;
                $scope.quarterItemIsDisabled = true;
                $scope.monthItemIsDisabled = true;
            }
            else if ($scope.salesType == 'TotalSales(Mil)') {
                $scope.yearItemIsDisabled = false;
                $scope.quarterItemIsDisabled = false;
                $scope.monthItemIsDisabled = true;
            }
            else if ($scope.salesType == 'TotalSalesWE(Mil)') {
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
        if ($scope.userData.isUserLoggedIn) {
            loadBrand();
            loadProductCategory();
            loadProduct();
            loadRegion();
            loadChannel();
            loadShowroom();
        }

        //library initialization

    }

})(angular.module('salesVentana'));