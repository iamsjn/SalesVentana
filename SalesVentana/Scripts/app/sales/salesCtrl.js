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

    salesCtrl.$inject = ['$scope', '$timeout', 'apiService', 'notificationService', 'excelExportService'];

    function salesCtrl($scope, $timeout, apiService, notificationService, excelExportService) {
        //variable declaration
        var currentYear = new Date().getFullYear();
        var monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        var chartValueType = '';
        var pathName = window.location.pathname;
        var globalIndex = 0;
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
        $scope.firstReportFilter = 'Brand';
        $scope.secondReportFilter = 'Brand';
        $scope.yAxisName = 'Share';
        $scope.salesType = 'yearly';
        $scope.showChart = false;
        $scope.showPieChart = false;
        $scope.showColumnChart = false;
        $scope.currentYear = currentYear;
        $scope.yearItemIsDisabled = false;
        $scope.quarterItemIsDisabled = true;
        $scope.monthItemIsDisabled = true;
        $scope.currentPage = 0;
        $scope.totalItems = 0;
        $scope.entryLimit = 0;
        $scope.noOfPages = 0;
        $scope.search = {};
        $scope.currentMonth = monthNames[new Date().getMonth()];

        //drop down filling
        $scope.inputSalesYear.push({ year: parseInt(currentYear), ticked: true });
        for (var i = parseInt(currentYear - 1); i >= 2000; i--) {
            $scope.inputSalesYear.push({ year: i, ticked: false });
        }

        for (var i = 1; i <= 4; i++) {
            $scope.inputSalesQuarter.push({ quarter: 'Q' + i });
        }

        for (var i = 0; i < 12; i++) {
            $scope.inputSalesMonth.push({ month: '' + monthNames[i] });
        }

        //fusion chart config
        $scope.salespiechartattrs = {
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
                xAxisName: $scope.firstReportFilter,
                yAxisName: $scope.yAxisName,
                theme: "fint"
            }
        };

        $scope.salescolumnchartattrs = {
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
                labelDisplay: "rotate",
                slantLabels: "1",
                use3dlighting: "0",
                showshadow: "0",
                legendbgcolor: "#CCCCCC",
                legendbgalpha: "20",
                legendborderalpha: "0",
                legendshadow: "0",
                legendnumcolumns: "3",
                subCaption: "",
                xAxisName: $scope.firstReportFilter,
                yAxisName: $scope.yAxisName,
                theme: "fint"
            }
        };

        $scope.salesareachartattrs = {
            "chart": {
                caption: "Yearly Sales",
                subCaption: "",
                showborder: "0",
                showvalues: "1",
                showpercentvalues: "1",
                enablesmartlabels: "1",
                xAxisName: $scope.firstReportFilter,
                yAxisName: $scope.yAxisName,
                usePlotGradientColor: "1",
                plotGradientColor: "#605ca8",
                theme: "fint"
            }
        };

        $scope.saleschartdataset = { "category": [[{ "label": "AC" }]], "data": [[{ "label": "AC", "value": 1987354192 }]] };

        //apiServices
        $scope.loadInitData = function () {
            if ($scope.userData.isUserLoggedIn) {

                loadBrand();
                loadProductCategory();
                loadProduct();
                loadRegion();
                loadChannel();
                loadShowroom();

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
            var firstReportFilter = $scope.firstReportFilter;
            var secondReportFilter = $scope.secondReportFilter;
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

            //taking only one filter if both are same
            if (firstReportFilter == secondReportFilter)
                secondReportFilter = '';

            switch ($scope.salesType) {
                case 'yearly':

                    searchCriteria = {
                        brandIds: brandIds, categoryIds: categoryIds, productIds: productIds, regionIds: regionIds, channelIds: channelIds, showroomIds: showroomIds,
                        firstReportFilter: firstReportFilter, secondReportFilter: secondReportFilter
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
                        salesQuarter: salesQuarter, firstReportFilter: firstReportFilter, secondReportFilter: secondReportFilter
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
                        salesMonth: salesMonth, firstReportFilter: firstReportFilter, secondReportFilter: secondReportFilter
                    };

                    apiService.post('api/sales/monthly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
                        yearlySalesDataLoadCompleted,
                        dataLoadFailed);
                    break;
                default:

                    searchCriteria = {
                        brandIds: brandIds, categoryIds: categoryIds, productIds: productIds, regionIds: regionIds, channelIds: channelIds, showroomIds: showroomIds,
                        firstReportFilter: firstReportFilter, secondReportFilter: secondReportFilter
                    };

                    apiService.post('api/sales/yearly-sales/' + parseInt($scope.outputSalesYear[0]['year']), searchCriteria,
                        yearlySalesDataLoadCompleted,
                        dataLoadFailed);
                    break;
            }
        }

        function yearlySalesDataLoadCompleted(result) {
            globalIndex = 0;
            $scope.salesData = [];
            $scope.salesTempData = [];
            $scope.salesDataSubTotal = [];
            $scope.showChart = true;
            $scope.showPieChart = true;
            $scope.showColumnChart = true;
            $scope.salesTempData = Object.create(result.data.table);
            $scope.salesData = JSON.parse(JSON.stringify(result.data.table));
            //pagination
            $scope.currentPage = 1;
            $scope.totalItems = $scope.salesData.length;
            $scope.entryLimit = 25;
            $scope.noOfPages = Math.ceil($scope.totalItems / $scope.entryLimit);

            fillChartData(result);
            getDataHeader();
            getSumOfData();

            //adding sub-total
            if ($scope.firstReportFilter != $scope.secondReportFilter) {
                $.each($scope.salesTempData, function (k, v) {
                    pushNewSubTotalObject($scope.salesTempData.indexOf(v));
                });
            }
        }

        $scope.getCommaSeparatedValue = function (data) {

            if (!isNaN(data) && data != null)
                return parseFloat(data).toLocaleString();
            else if (isNaN(data) || data != null)
                return data;
            else if (data == null)
                return '';
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

        $scope.changeSecondReportFilter = function () {
            switch ($scope.firstReportFilter) {
                case 'Brand':
                    $scope.secondReportFilter = 'Brand';
                    break;
                case 'ProductCategory':
                    $scope.secondReportFilter = 'ProductCategory';
                    break;
                case 'Product':
                    $scope.secondReportFilter = 'Product';
                    break;
                case 'Region':
                    $scope.secondReportFilter = 'Region';
                    break;
                case 'Showroom':
                    $scope.secondReportFilter = 'Showroom';
                    break;
                case 'Employee':
                    $scope.secondReportFilter = 'Employee';
                    break;
                default:
                    $scope.secondReportFilter = 'Brand';
                    break;
            }
        }

        function pushNewSubTotalObject(index) {

            var currentObj = {};
            var nextObj = {};
            var totalObj = {};
            currentObj = $scope.salesTempData[index];
            nextObj = $scope.salesTempData[index + 1];

            if (nextObj == undefined || (currentObj[$scope.firstReportFilter] != nextObj[$scope.firstReportFilter])) {

                totalObj = Object.create(currentObj);

                for (var k in totalObj) {
                    if (!isNaN(totalObj[k]) && totalObj[k] != null)
                        totalObj[k] = 0;
                    else if (isNaN(totalObj[k]) && totalObj[k] != null)
                        totalObj[k] = '';
                }

                $.each($scope.salesTempData, function (key, val) {
                    if (val[$scope.salesHeader[0]] == currentObj[$scope.firstReportFilter]) {
                        for (var k in val) {
                            if (!isNaN(val[k]) && val[k] != null) {
                                totalObj[k] += parseFloat(val[k]);
                            }
                            else if (k == $scope.firstReportFilter && isNaN(val[k])) {
                                totalObj[k] = 'Sub-Total';
                            }
                            else if (isNaN(val[k]) && val[k] != null) {
                                totalObj[k] = null;
                            }
                        }
                    }
                })

                index = globalIndex + index;
                totalObj['background'] = '#ffc56f';
                totalObj['fontweight'] = 'bold';
                $scope.salesData.splice(index + 1, 0, totalObj);
                globalIndex++;
            }
        }

        function fillChartData(result) {

            var salesChartCategory = [];
            var salesChartData = [];
            var value = 0;

            $.each(result.data.table, function (k, v) {
                if ($scope.salesType != 'yearly') {
                    $scope.yAxisName = 'TotalSales(In Mil)';
                    value = (parseFloat(v['TotalSales(TK)']) / 1000000);
                }
                else {
                    $scope.yAxisName = 'Share';
                    value = parseFloat(v['Share']);
                }

                salesChartCategory.push({ label: v[$scope.firstReportFilter] });
                salesChartData.push({ label: v[$scope.firstReportFilter], value: value });
            });

            $scope.saleschartdataset.category = salesChartCategory;
            $scope.saleschartdataset.data = salesChartData;
        }

        function getDataHeader() {

            var test = [];
            $scope.salesHeader = [];

            $.each($scope.salesData, function (k, v) {
                test = (k, v);
            })

            for (var header in test) {
                if (header != 'Share')
                    $scope.salesHeader.push(header);
            }
        }

        function getSumOfData() {

            var total = 0;
            $scope.salesDataTotal = [];

            for (var i = 0; i < $scope.salesHeader.length; i++) {
                total = 0;
                $.each($scope.salesData, function (k, v) {
                    if (!isNaN(v[$scope.salesHeader[i]]) && v[$scope.salesHeader[i]] != null) {
                        total += parseFloat(v[$scope.salesHeader[i]]);
                    }
                    else if (i == 0 && isNaN(v[$scope.salesHeader[i]])) {
                        $scope.salesDataTotal.push('Grand-Total');
                        return false;
                    }
                })

                if (total > 0 && i > 0)
                    $scope.salesDataTotal.push(total.toLocaleString());
                else if (i > 0)
                    $scope.salesDataTotal.push('');
            }
        }

        //Excel Export
        $scope.exportToExcel = function (tableId) {
            var exportHref = excelExportService.tableToExcel(tableId, 'Sales Data');
            $timeout(function () { location.href = exportHref; }, 100);
        }

        //common data load fail function
        function dataLoadFailed(response) {
            notificationService.displayError(response.data);
        }

        //library initialization
    }

})(angular.module('salesVentana'));