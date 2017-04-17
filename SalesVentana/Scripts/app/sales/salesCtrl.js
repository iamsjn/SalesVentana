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
        $scope.employeeType = false;
        $scope.showChart = false;
        $scope.salesType = 'Share';
        $scope.chartFilter = 'Brand';
        $scope.currentYear = currentYear;
        $scope.yearItemIsDisabled = false;
        $scope.quarterItemIsDisabled = true;
        $scope.monthItemIsDisabled = true;
        $scope.isBrandDisabled = false;
        $scope.isCategoryDisabled = true;
        $scope.isProductDisabled = true;
        $scope.isRegionDisabled = true;
        $scope.isShowroomDisabled = true;
        $scope.isEmployeeDisabled = true;
        $scope.chartFilterChecked = false;
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
            if ($scope.userData.isUserLoggedIn) {
                $scope.outputSalesYear = [{ year: parseInt(currentYear), ticked: true }];
                $scope.filterSales();
            }
        }

        function loadBrand() {
            apiService.get('api/sales/brand/#', null,
                        brandDataLoadCompleted,
                        dataLoadFailed);
        }

        function brandDataLoadCompleted(result) {
            $scope.inputBrands = result.data.brands;
        }

        function loadProductCategory() {
            apiService.get('api/sales/product-category/#', null,
                        productCategoryDataLoadCompleted,
                        dataLoadFailed);
        }

        function productCategoryDataLoadCompleted(result) {
            $scope.inputProductCategories = result.data.productCategories;
        }

        function loadProduct() {
            apiService.get('api/sales/product/#', null,
                        productDataLoadCompleted,
                        dataLoadFailed);
        }

        function productDataLoadCompleted(result) {
            $scope.inputProducts = result.data.products;
        }

        function loadRegion() {
            apiService.get('api/sales/region/#', null,
                        regionDataLoadCompleted,
                        dataLoadFailed);
        }

        function regionDataLoadCompleted(result) {
            $scope.inputRegion = result.data.regions;
        }

        function loadChannel() {
            apiService.get('api/sales/channel/#', null,
                        channelDataLoadCompleted,
                        dataLoadFailed);
        }

        function channelDataLoadCompleted(result) {
            $scope.inputChannel = result.data.channels;
        }

        function loadShowroom() {
            apiService.get('api/sales/showroom/#', null,
                        showroomDataLoadCompleted,
                        dataLoadFailed);
        }

        function showroomDataLoadCompleted(result) {
            $scope.inputShowroom = result.data.showrooms;
        }

        $scope.brandClick = function () {
            apiService.post('api/sales/product-category/#', $scope.outputBrands,
                        productCategoryDataLoadCompleted,
                        dataLoadFailed);
        }

        $scope.productCategoryClick = function () {
            apiService.post('api/sales/product/#', $scope.outputProductCategories,
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
                showroomType: $scope.showroomType, salesQuarter: salesQuarter, employeeType: $scope.employeeType
            };

            switch ($scope.salesType) {
                case 'Share':
                    apiService.post('api/sales/yearly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
                        yearlySalesDataLoadCompleted,
                        dataLoadFailed);
                    break;
                case 'TotalSales(Mil)':
                    apiService.post('api/sales/quarterly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
                        yearlySalesDataLoadCompleted,
                        dataLoadFailed);
                    break;
                case 'TotalSalesWEE(Mil)':
                    apiService.post('api/sales/yearly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
                        yearlySalesDataLoadCompleted,
                        dataLoadFailed);
                    break;
                default:
                    apiService.post('api/sales/yearly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
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
            $scope.salesCommaSprtdData = [];
            $scope.salesDataTotal = [];
            $scope.showChart = true;
            $scope.salesData = result.data.table;
            //pagination
            $scope.currentPage = 1;
            $scope.totalItems = $scope.salesData.length;
            $scope.entryLimit = 100;
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
                if (header != 'Share')
                    $scope.salesHeader.push(header);
            }

            for (var i = 0; i < $scope.salesHeader.length; i++) {
                total = 0;
                $.each($scope.salesData, function (k, v) {
                    if (!isNaN(v[$scope.salesHeader[i]]) && v[$scope.salesHeader[i]] != null) {
                        total += parseFloat(v[$scope.salesHeader[i]]);
                    }
                    else if (i == 0 && isNaN(v[$scope.salesHeader[i]])) {
                        $scope.salesDataTotal.push('Total');
                        return false;
                    }
                })
                if (total >= 0 && i > 0)
                    $scope.salesDataTotal.push(total.toLocaleString());
                else if (i > 0)
                    $scope.salesDataTotal.push(0);
            }
        }

        $scope.getCommaSeparatedValue = function (data) {
            if (!isNaN(data) && data != null)
                return parseFloat(data).toLocaleString();
            else
                return 0;
        }

        $scope.changeSalesType = function () {
            $.each($scope.inputSalesQuarter, function (k, v) {
                v.ticked = false;
            })
            $.each($scope.inputSalesMonth, function (k, v) {
                v.ticked = false;
            })
            $scope.outputSalesQuarter = [];
            $scope.outputSalesMonth = [];

            switch ($scope.salesType) {
                case 'Share':
                    $scope.quarterItemIsDisabled = true;
                    $scope.monthItemIsDisabled = true;
                    break;
                case 'TotalSales(Mil)':
                    $scope.quarterItemIsDisabled = false;
                    $scope.monthItemIsDisabled = true;
                    break;
                case 'TotalSalesWE(Mil)':
                    $scope.quarterItemIsDisabled = true;
                    $scope.monthItemIsDisabled = false;
                    break;
                default:
                    break;
            }
        }

        $scope.enableChartType = function () {
            if ($scope.brandType)
                $scope.isBrandDisabled = !$scope.brandType;
            else
                $scope.isBrandDisabled = !$scope.brandType;
            if ($scope.categoryType)
                $scope.isCategoryDisabled = !$scope.categoryType;
            else
                $scope.isCategoryDisabled = !$scope.categoryType;
            if ($scope.productType)
                $scope.isProductDisabled = !$scope.productType;
            else
                $scope.isProductDisabled = !$scope.productType;
            if ($scope.regionType)
                $scope.isRegionDisabled = !$scope.regionType;
            else
                $scope.isRegionDisabled = !$scope.regionType;
            if ($scope.showroomType)
                $scope.isShowroomDisabled = !$scope.showroomType;
            else
                $scope.isShowroomDisabled = !$scope.showroomType;
            if ($scope.employeeType)
                $scope.isEmployeeDisabled = !$scope.employeeType;
            else
                $scope.isEmployeeDisabled = !$scope.employeeType;
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