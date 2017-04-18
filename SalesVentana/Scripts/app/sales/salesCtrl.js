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
        $scope.showPieChart = false;
        $scope.showColumnChart = false;
        $scope.salesType = 'yearly';
        $scope.chartFilter = 'Brand';
        $scope.currentYear = currentYear;
        $scope.yearItemIsDisabled = false;
        $scope.quarterItemIsDisabled = true;
        $scope.monthItemIsDisabled = true;
        $scope.showroomItemIsDisabled = true;
        $scope.isBrandDisabled = false;
        $scope.isCategoryDisabled = true;
        $scope.isProductDisabled = true;
        $scope.isRegionDisabled = true;
        $scope.isShowroomRadioDisabled = true;
        $scope.isEmployeeDisabled = true;
        $scope.chartFilterChecked = false;
        $scope.isShowroomCheckDisabled = true;
        $scope.currentPage = 0;
        $scope.totalItems = 0;
        $scope.entryLimit = 0;
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
                caption: "Yearly Sales",
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


        $scope.salesareachartattrs = {
            "chart": {
                caption: "Yearly Sales",
                subCaption: "",
                showborder: "0",
                xAxisName: $scope.chartFilter,
                yAxisName: "Share",
                theme: "fint",
                //Setting gradient fill to true
                usePlotGradientColor: "1",
                //Setting the gradient formation color
                plotGradientColor: "#605ca8"
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

        $scope.channelClick = function () {
            var retailFound = false;
            $.each($scope.outputChannel, function (k, v) {
                if (v['channelName'] == 'Retail') {
                    retailFound = true;
                    return false;
                }
            });

            if (retailFound) {
                $scope.showroomItemIsDisabled = false;
                $scope.isShowroomCheckDisabled = false;
            }
            else {
                $scope.outputShowroom = [];
                $scope.isShowroomCheckDisabled = true;
                $.each($scope.inputShowroom, function (k, v) {
                    v.ticked = false;
                });
            }
        }

        $scope.filterSales = function () {
            var searchCriteria = [];
            var reportType = [];
            var brandIds = '', categoryIds = '', productIds = '', regionIds = '', channelIds = '', showroomIds = '';
            var salesQuarter = '', salesMonth = '';

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

            switch ($scope.salesType) {
                case 'yearly':

                    searchCriteria = {
                        brandIds: brandIds, categoryIds: categoryIds, productIds: productIds, regionIds: regionIds, channelIds: channelIds, showroomIds: showroomIds,
                        brandType: $scope.brandType, categoryType: $scope.categoryType, productType: $scope.productType, regionType: $scope.regionType,
                        showroomType: $scope.showroomType, employeeType: $scope.employeeType
                    };

                    apiService.post('api/sales/yearly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
                        yearlySalesDataLoadCompleted,
                        dataLoadFailed);
                    break;
                case 'quarterly':

                    $.each($scope.outputSalesQuarter, function () {
                        salesQuarter += this.quarter + ',';
                    })

                    searchCriteria = {
                        brandIds: brandIds, categoryIds: categoryIds, productIds: productIds, regionIds: regionIds, channelIds: channelIds, showroomIds: showroomIds,
                        brandType: $scope.brandType, categoryType: $scope.categoryType, productType: $scope.productType, regionType: $scope.regionType,
                        showroomType: $scope.showroomType, salesQuarter: salesQuarter, employeeType: $scope.employeeType
                    };

                    apiService.post('api/sales/quarterly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
                        yearlySalesDataLoadCompleted,
                        dataLoadFailed);
                    break;
                case 'monthly':

                    $.each($scope.outputSalesMonth, function () {
                        salesMonth += this.month + ',';
                    })

                    searchCriteria = {
                        brandIds: brandIds, categoryIds: categoryIds, productIds: productIds, regionIds: regionIds, channelIds: channelIds, showroomIds: showroomIds,
                        brandType: $scope.brandType, categoryType: $scope.categoryType, productType: $scope.productType, regionType: $scope.regionType,
                        showroomType: $scope.showroomType, employeeType: $scope.employeeType, salesMonth: salesMonth
                    };

                    apiService.post('api/sales/monthly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
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
            var value = 0;
            $scope.salesHeader = [];
            $scope.salesData = [];
            $scope.salesCommaSprtdData = [];
            $scope.salesDataTotal = [];
            $scope.showChart = true;
            $scope.showPieChart = true;
            $scope.showColumnChart = true;
            $scope.salesData = result.data.table;
            //pagination
            $scope.currentPage = 1;
            $scope.totalItems = $scope.salesData.length;
            $scope.entryLimit = 100;
            $scope.noOfPages = Math.ceil($scope.totalItems / $scope.entryLimit);

            $.each(result.data.table, function (k, v) {
                if ($scope.salesType != 'yearly')
                    value = (parseFloat(v['TotalSales(TK)']) / 1000000);
                else
                    value = parseFloat(v['Share']);

                salesChartCategory.push({ label: v[$scope.chartFilter] });
                salesChartData.push({ label: v[$scope.chartFilter], value: value });
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

                if (total > 0 && i > 0)
                    $scope.salesDataTotal.push(total.toLocaleString());
                else if (i > 0)
                    $scope.salesDataTotal.push('');
            }
        }

        $scope.getCommaSeparatedValue = function (data) {
            if (!isNaN(data) && data != null)
                return parseFloat(data).toLocaleString();
            else if (isNaN(data) || data != null)
                return data;
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
                case 'yearly':
                    $scope.quarterItemIsDisabled = true;
                    $scope.monthItemIsDisabled = true;
                    break;
                case 'quarterly':
                    $scope.quarterItemIsDisabled = false;
                    $scope.monthItemIsDisabled = true;
                    break;
                case 'monthly':
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
                $scope.isShowroomRadioDisabled = !$scope.showroomType;
            else
                $scope.isShowroomRadioDisabled = !$scope.showroomType;
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