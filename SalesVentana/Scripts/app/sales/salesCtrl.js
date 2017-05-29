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

    salesCtrl.$inject = ['$scope', '$timeout', 'apiService', 'notificationService', 'excelExportService', 'dataHelperService'];

    function salesCtrl($scope, $timeout, apiService, notificationService, excelExportService, dataHelperService) {
        //variable declaration
        var vm = this;
        var currentYear = new Date().getFullYear();
        var monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        var chartValueType = '';
        var globalIndex = 0;
        vm.inputSalesYear = [];
        vm.outputSalesYear = [];
        vm.inputSalesQuarter = [];
        vm.outputSalesQuarter = [];
        vm.inputSalesMonth = [];
        vm.outputSalesMonth = [];
        vm.inputBrands = [];
        vm.outputBrands = [];
        vm.inputProductCategories = [];
        vm.outputProductCategories = [];
        vm.inputProducts = [];
        vm.outputProducts = [];
        vm.inputRegion = [];
        vm.outputRegion = [];
        vm.inputChannel = [];
        vm.outputChannel = [];
        vm.inputShowroom = [];
        vm.outputShowroom = [];
        vm.firstReportFilter = 'Brand';
        vm.secondReportFilter = 'Brand';
        vm.yAxisName = 'Share';
        vm.salesType = 'yearly';
        vm.valueFormatter = 'Million';
        vm.cashValueFormatter = 'Million';
        vm.valValueFormatter = 'Million';
        vm.isSearchbtnDisabled = false;
        vm.showAreaChart = false;
        vm.showPieChart = false;
        vm.showColumnChart = false;
        vm.showStackedChart = false;
        vm.isPieChart = true;
        vm.isColumnChart = true;
        vm.isStackedChart = false;
        vm.isPieCompatible = false;
        vm.isColumnCompatible = false;
        vm.isStackedCompatible = true;
        vm.currentYear = currentYear;
        vm.yearItemIsDisabled = false;
        vm.quarterItemIsDisabled = true;
        vm.monthItemIsDisabled = true;
        vm.isChannelDisabled = true;
        vm.currentPage = 0;
        vm.totalItems = 0;
        vm.entryLimit = 0;
        vm.noOfPages = 0;
        vm.search = {};
        vm.currentMonth = monthNames[new Date().getMonth()];

        //drop down filling
        vm.inputSalesYear.push({ year: parseInt(currentYear), ticked: true });
        for (var i = parseInt(currentYear - 1); i >= 2000; i--) {
            vm.inputSalesYear.push({ year: i, ticked: false });
        }

        for (var i = 1; i <= 4; i++) {
            vm.inputSalesQuarter.push({ quarter: 'Q' + i });
        }

        for (var i = 0; i < 12; i++) {
            vm.inputSalesMonth.push({ month: '' + monthNames[i] });
        }

        //apiServices
        vm.initData = function () {

            if ($scope.userData.isUserLoggedIn) {
                vm.outputSalesYear = [{ year: parseInt(currentYear), ticked: true }];
                initChart();
                loadInitialData();
                vm.filterSales();
            }
        }

        function initChart() {

            //fusion chart config
            vm.piechartattrs = {
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
                    xAxisName: vm.firstReportFilter,
                    yAxisName: vm.yAxisName,
                    theme: "fint"
                }
            };

            vm.columnchartattrs = {
                "chart": {
                    caption: "Yearly Sales",
                    bgcolor: "FFFFFF",
                    showvalues: "0",
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
                    xAxisName: vm.firstReportFilter,
                    yAxisName: vm.yAxisName,
                    theme: "fint"
                }
            };

            vm.areachartattrs = {
                "chart": {
                    caption: "Yearly Sales",
                    subCaption: "",
                    showborder: "0",
                    showvalues: "0",
                    showpercentvalues: "1",
                    enablesmartlabels: "1",
                    xAxisName: vm.firstReportFilter,
                    yAxisName: vm.yAxisName,
                    usePlotGradientColor: "1",
                    plotGradientColor: "#605ca8",
                    theme: "fint"
                }
            };

            vm.mschartdatasource =
                {
                    "chart": {
                        caption: "Yearly Sales",
                        bgcolor: "FFFFFF",
                        showvalues: "0",
                        showpercentvalues: "0",
                        showborder: "0",
                        showplotborder: "0",
                        showlegend: "1",
                        legendborder: "1",
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
                        legendnumcolumns: "8",
                        legendIconBgColor: "#FFFFFF",
                        legendIconBorderColor: "#4cc78e",
                        drawCustomLegendIcon: "1",
                        subCaption: "",
                        xAxisName: vm.firstReportFilter,
                        yAxisName: vm.yAxisName,
                        numberPrefix: "TK ",
                        theme: "fint"
                    },
                    "categories": [{
                        "category": [{ "label": "Q1" }]
                    }],
                    "dataset": [{
                        "seriesname": "Food Products",
                        "data": [{ "value": 11000 }]
                    }]
                }

            vm.sschartdataset = { "category": [[{ "label": "AC" }]], "data": [[{ "label": "AC", "value": 1987354192 }]] };

            //rendering stacked chart at container
            var stackedChart = FusionCharts("stackedChart");
            if (stackedChart !== undefined) {
                stackedChart.dispose();
            }
            else {
                var chart = new FusionCharts({ renderAt: "stacked_chart_container", dataSource: vm.mschartdatasource, type: "mscolumn3d", id: 'stackedChart', width: "100%", height: "700" });
                chart.render();
            }
        }

        function loadBrand() {
            apiService.get('api/sales/brand/#', null,
                brandDataLoadCompleted,
                dataLoadFailed);
        }

        function brandDataLoadCompleted(result) {
            vm.inputBrands = result.data.brands;
        }

        function loadProductCategory() {
            apiService.get('api/sales/product-category/#', null,
                productCategoryDataLoadCompleted,
                dataLoadFailed);
        }

        function productCategoryDataLoadCompleted(result) {
            vm.inputProductCategories = result.data.productCategories;
        }

        function loadProduct() {
            apiService.get('api/sales/product/#', null,
                productDataLoadCompleted,
                dataLoadFailed);
        }

        function productDataLoadCompleted(result) {
            vm.inputProducts = result.data.products;
        }

        function loadRegion() {
            apiService.get('api/sales/region/#', null,
                regionDataLoadCompleted,
                dataLoadFailed);
        }

        function regionDataLoadCompleted(result) {
            vm.inputRegion = result.data.regions;
        }

        function loadChannel() {
            apiService.get('api/sales/channel/#', null,
                channelDataLoadCompleted,
                dataLoadFailed);
        }

        function channelDataLoadCompleted(result) {
            vm.inputChannel = result.data.channels;
        }

        function loadShowroom() {
            apiService.get('api/sales/showroom/#', null,
                showroomDataLoadCompleted,
                dataLoadFailed);
        }

        function showroomDataLoadCompleted(result) {
            vm.inputShowroom = result.data.showrooms;
        }

        vm.brandClick = function () {
            apiService.post('api/sales/product-category/#', vm.outputBrands,
                productCategoryDataLoadCompleted,
                dataLoadFailed);
        }

        vm.productCategoryClick = function () {
            apiService.post('api/sales/product/#', vm.outputProductCategories,
                productDataLoadCompleted,
                dataLoadFailed);
        }

        function loadInitialData() {
            apiService.get('api/sales/initial-data/#', null,
                initialDataLoadCompleted,
                dataLoadFailed);
        }

        function initialDataLoadCompleted(result) {
            vm.inputBrands = result.data.brands;
            vm.inputProductCategories = result.data.productCategories;
            vm.inputProducts = result.data.products;
            vm.inputRegion = result.data.regions;
            vm.inputChannel = result.data.channels;
            vm.inputShowroom = result.data.showrooms;
        }

        vm.filterSales = function () {

            vm.isSearchbtnDisabled = true;
            var searchCriteria = [];
            var firstReportFilter = vm.firstReportFilter;
            var secondReportFilter = vm.secondReportFilter;
            var brandIds = '', categoryIds = '', productIds = '', regionIds = '', channelIds = '', showroomIds = '';
            var salesQuarter = '', salesMonth = '';

            $.each(vm.outputBrands, function () {
                brandIds += this.brandId + ',';
            })

            $.each(vm.outputProductCategories, function () {
                categoryIds += this.categoryId + ',';
            })

            $.each(vm.outputProducts, function () {
                productIds += this.productId + ',';
            })

            $.each(vm.outputRegion, function () {
                regionIds += this.regionId + ',';
            })

            $.each(vm.outputChannel, function () {
                channelIds += this.channelId + ',';
            })

            $.each(vm.outputShowroom, function () {
                showroomIds += this.showroomId + ',';
            })

            //taking only one filter if both are same
            if (firstReportFilter == secondReportFilter)
                secondReportFilter = '';

            switch (vm.salesType) {
                case 'yearly':
                    searchCriteria = {
                        brandIds: brandIds, categoryIds: categoryIds, productIds: productIds, regionIds: regionIds, channelIds: channelIds, showroomIds: showroomIds,
                        firstReportFilter: firstReportFilter, secondReportFilter: secondReportFilter
                    };

                    vm.salesPromise = apiService.post('api/sales/yearly-sales/' + parseInt(vm.outputSalesYear[0]['year']), searchCriteria,
                        salesDataLoadCompleted,
                        dataLoadFailed);
                    break;
                case 'quarterly':

                    $.each(vm.outputSalesQuarter, function () {
                        salesQuarter += this.quarter + ',';
                    })

                    searchCriteria = {
                        brandIds: brandIds, categoryIds: categoryIds, productIds: productIds, regionIds: regionIds, channelIds: channelIds, showroomIds: showroomIds,
                        salesQuarter: salesQuarter, firstReportFilter: firstReportFilter, secondReportFilter: secondReportFilter
                    };

                    vm.salesPromise = apiService.post('api/sales/quarterly-sales/' + parseInt(vm.outputSalesYear[0]['year']), searchCriteria,
                        salesDataLoadCompleted,
                        dataLoadFailed);
                    break;
                case 'monthly':

                    $.each(vm.outputSalesMonth, function () {
                        salesMonth += this.month + ',';
                    })

                    searchCriteria = {
                        brandIds: brandIds, categoryIds: categoryIds, productIds: productIds, regionIds: regionIds, channelIds: channelIds, showroomIds: showroomIds,
                        salesMonth: salesMonth, firstReportFilter: firstReportFilter, secondReportFilter: secondReportFilter
                    };

                    vm.salesPromise = apiService.post('api/sales/monthly-sales/' + parseInt(vm.outputSalesYear[0]['year']), searchCriteria,
                        salesDataLoadCompleted,
                        dataLoadFailed);
                    break;
                default:

                    searchCriteria = {
                        brandIds: brandIds, categoryIds: categoryIds, productIds: productIds, regionIds: regionIds, channelIds: channelIds, showroomIds: showroomIds,
                        firstReportFilter: firstReportFilter, secondReportFilter: secondReportFilter
                    };

                    vm.salesPromise = apiService.post('api/sales/yearly-sales/' + parseInt(vm.outputSalesYear[0]['year']), searchCriteria,
                        salesDataLoadCompleted,
                        dataLoadFailed);
                    break;
            }
        }

        function salesDataLoadCompleted(result) {

            var combinedData = [];
            globalIndex = 0;
            vm.isSearchbtnDisabled = false;
            vm.showAreaChart = true;
            vm.showPieChart = vm.isPieChart;
            vm.showColumnChart = vm.isColumnChart;
            vm.showStackedChart = vm.isStackedChart;
            vm.yAxisName = vm.salesType != 'yearly' ? 'TotalSales(In Mil)' : 'Share';

            //setting sales val data
            vm.valueFormatter = 'Million';
            vm.salesValHeader = [];
            vm.salesValData = [];
            vm.salesValTotal = [];
            vm.salesTempData = [];
            vm.salesTempData = Object.create(result.data.val);
            vm.salesValData = JSON.parse(JSON.stringify(result.data.val));
            combinedData = JSON.parse(JSON.stringify(result.data.val));

            //pagination
            vm.valCurrentPage = 1;
            vm.valTotalItems = vm.salesValData.length;
            vm.valEntryLimit = 15;
            vm.valNoOfPages = Math.ceil(vm.valTotalItems / vm.valEntryLimit);

            //rendering ms chart
            if (!vm.isStackedCompatible)
                fillMSChartData(result.data.val);

            vm.headerData = dataHelperService.getHeader(vm.salesValData, ['Share']);
            vm.salesValHeader = JSON.parse(JSON.stringify(vm.headerData));

            vm.totalData = dataHelperService.getSumOfData(vm.salesValData, vm.salesValHeader);
            vm.salesValTotal = JSON.parse(JSON.stringify(vm.totalData));

            //adding sub total row and combining columns
            if (vm.firstReportFilter != vm.secondReportFilter) {
                $.each(combinedData, function (k, v) {
                    v['combinedClmn'] = v[vm.firstReportFilter] + '-' + v[vm.secondReportFilter];
                });
                $.each(vm.salesTempData, function (k, v) {
                    pushNewSubTotalObject(vm.salesTempData.indexOf(v), vm.salesValData);
                });
            }

            //rendering ss chart
            if (vm.isStackedCompatible)
                fillSSChartData(combinedData);

            //settig sales qty data
            vm.valueFormatter = 'Thousand';
            globalIndex = 0;
            combinedData = [];
            vm.salesQtyHeader = [];
            vm.salesQtyData = [];
            vm.salesQtyTotal = [];
            vm.salesTempData = [];
            vm.totalData = [];
            vm.salesTempData = Object.create(result.data.qty);
            vm.salesQtyData = JSON.parse(JSON.stringify(result.data.qty));
            combinedData = JSON.parse(JSON.stringify(result.data.qty));

            //pagination
            vm.qtyCurrentPage = 1;
            vm.qtyTotalItems = vm.salesQtyData.length;
            vm.qtyEntryLimit = 15;
            vm.qtyNoOfPages = Math.ceil(vm.qtyTotalItems / vm.qtyEntryLimit);

            vm.headerData = dataHelperService.getHeader(vm.salesQtyData, ['Share']);
            vm.salesQtyHeader = JSON.parse(JSON.stringify(vm.headerData));

            vm.totalData = dataHelperService.getSumOfData(vm.salesQtyData, vm.salesQtyHeader);
            vm.salesQtyTotal = JSON.parse(JSON.stringify(vm.totalData));

            //adding sub total row and combining columns
            if (vm.firstReportFilter != vm.secondReportFilter) {
                $.each(combinedData, function (k, v) {
                    v['combinedClmn'] = v[vm.firstReportFilter] + '-' + v[vm.secondReportFilter];
                });
                $.each(vm.salesTempData, function (k, v) {
                    pushNewSubTotalObject(vm.salesTempData.indexOf(v), vm.salesQtyData);
                });
            }

            //setting sales cash data
            vm.valueFormatter = 'Million';
            vm.salesCashData = [];
            vm.salesCashHeader = [];
            vm.salesCashTotal = [];
            vm.totalData = [];

            if (vm.salesType == 'monthly') {
                vm.salesCashData = JSON.parse(JSON.stringify(result.data.cash));
                vm.headerData = dataHelperService.getHeader(vm.salesCashData, ['Share']);
                vm.salesCashHeader = JSON.parse(JSON.stringify(vm.headerData));

                vm.totalData = dataHelperService.getSumOfData(vm.salesCashData, vm.salesCashHeader);
                vm.salesCashTotal = JSON.parse(JSON.stringify(vm.totalData));
            }
        }

        vm.getCommaSeparatedData = function (data, header, exception) {
            return dataHelperService.getCommaSeparatedData(data, header, exception);
        }

        vm.changeSalesType = function () {
            $.each(vm.inputSalesQuarter, function (k, v) {
                v.ticked = false;
            })
            $.each(vm.inputSalesMonth, function (k, v) {
                v.ticked = false;
            })
            vm.outputSalesQuarter = [];
            vm.outputSalesMonth = [];

            switch (vm.salesType) {
                case 'yearly':
                    vm.quarterItemIsDisabled = true;
                    vm.monthItemIsDisabled = true;
                    vm.isChannelDisabled = true;
                    break;
                case 'quarterly':
                    vm.quarterItemIsDisabled = false;
                    vm.monthItemIsDisabled = true;
                    vm.isChannelDisabled = true;
                    break;
                case 'monthly':
                    vm.quarterItemIsDisabled = true;
                    vm.monthItemIsDisabled = false;
                    vm.isChannelDisabled = false;
                    break;
                default:
                    break;
            }
        }

        vm.changeSecondReportFilter = function () {
            switch (vm.firstReportFilter) {
                case 'Brand':
                    vm.secondReportFilter = 'Brand';
                    break;
                case 'ProductCategory':
                    vm.secondReportFilter = 'ProductCategory';
                    break;
                case 'Product':
                    vm.secondReportFilter = 'Product';
                    break;
                case 'Region':
                    vm.secondReportFilter = 'Region';
                    break;
                case 'Showroom':
                    vm.secondReportFilter = 'Showroom';
                    break;
                case 'Channel':
                    vm.secondReportFilter = 'Channel';
                    break;
                case 'Employee':
                    vm.secondReportFilter = 'Employee';
                    break;
                default:
                    vm.secondReportFilter = 'Brand';
                    break;
            }
        }

        vm.isStackChartCompatible = function () {
            var isEqual = vm.firstReportFilter != vm.secondReportFilter;
            vm.isStackedCompatible = !isEqual;
            vm.isColumnCompatible = isEqual;
            vm.isPieCompatible = isEqual;
            vm.isStackedChart = isEqual;
            vm.isPieChart = !isEqual;
            vm.isColumnChart = !isEqual;
        }

        vm.changeValueFormat = function (x) {
            if (x == 'val') {
                vm.valueFormatter = vm.valValueFormatter;
                vm.headerData = vm.salesValHeader;
                vm.totalData = dataHelperService.getSumOfData(vm.salesValData, vm.salesValHeader, vm.valueFormatter);
                vm.salesValTotal = JSON.parse(JSON.stringify(vm.totalData));
                vm.salesValData = vm.salesValData;
            }
            else if (x == 'cash') {
                vm.valueFormatter = vm.cashValueFormatter;
                vm.headerData = vm.salesCashHeader;
                vm.totalData = dataHelperService.getSumOfData(vm.salesCashData, vm.salesCashHeader, vm.valueFormatter);
                vm.salesCashTotal = JSON.parse(JSON.stringify(vm.totalData));
                vm.salesCashData = vm.salesCashData;
            }
        }

        function pushNewSubTotalObject(index, data) {

            var currentObj = {};
            var nextObj = {};
            var totalObj = {};
            currentObj = vm.salesTempData[index];
            nextObj = vm.salesTempData[index + 1];

            if (nextObj == undefined || (currentObj[vm.firstReportFilter] != nextObj[vm.firstReportFilter])) {

                totalObj = Object.create(currentObj);

                for (var k in totalObj) {
                    if (!isNaN(totalObj[k]) && totalObj[k] != null)
                        totalObj[k] = 0;
                    else if (isNaN(totalObj[k]) && totalObj[k] != null)
                        totalObj[k] = '';
                }

                $.each(vm.salesTempData, function (key, val) {
                    if (val[vm.headerData[0]] == currentObj[vm.firstReportFilter]) {
                        for (var k in val) {
                            if (!isNaN(val[k]) && val[k] != null) {
                                totalObj[k] += parseFloat(val[k]);
                            }
                            else if (k == vm.firstReportFilter && isNaN(val[k])) {
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
                data.splice(index + 1, 0, totalObj);
                globalIndex++;
            }
        }

        function fillSSChartData(combinedData) {
            var chartCategory = [];
            var chartData = [];
            var value = 0;

            $.each(combinedData, function (k, v) {

                if (vm.salesType != 'yearly')
                    value = parseFloat(v['TotalSales(TK)']);
                else
                    value = parseFloat(v['Share']);

                if (vm.firstReportFilter != vm.secondReportFilter) {
                    chartCategory.push({ label: v['combinedClmn'] });
                    chartData.push({ label: v['combinedClmn'], value: value });
                }
                else {
                    chartCategory.push({ label: v[vm.secondReportFilter] });
                    chartData.push({ label: v[vm.secondReportFilter], value: value });
                }
            });

            //chart attributes
            vm.piechartattrs.chart.showpercentvalues = vm.salesType != 'yearly' ? '0' : '1';
            vm.piechartattrs.chart.numberPrefix = vm.salesType != 'yearly' ? 'TK ' : '';
            vm.piechartattrs.chart.xAxisName = vm.firstReportFilter;
            vm.piechartattrs.chart.yAxisName = vm.yAxisName;
            vm.columnchartattrs.chart.showpercentvalues = vm.salesType != 'yearly' ? '0' : '1';
            vm.columnchartattrs.chart.numberPrefix = vm.salesType != 'yearly' ? 'TK ' : '';
            vm.columnchartattrs.chart.xAxisName = vm.firstReportFilter;
            vm.columnchartattrs.chart.yAxisName = vm.yAxisName;
            vm.areachartattrs.chart.showpercentvalues = vm.salesType != 'yearly' ? '0' : '1';
            vm.areachartattrs.chart.numberPrefix = vm.salesType != 'yearly' ? 'TK ' : '';
            vm.areachartattrs.chart.xAxisName = vm.firstReportFilter;
            vm.areachartattrs.chart.yAxisName = vm.yAxisName;

            //chart data
            vm.sschartdataset.category = chartCategory;
            vm.sschartdataset.data = chartData;

        }

        function fillMSChartData(data) {
            var chart = {};
            var value = 0.0;
            var color = '';
            var chartCategory = [];
            var chartLabel = [];
            var chartData = [];
            var uniqueCategory = []
            var stackedChart = FusionCharts("stackedChart");

            //disposing previously rendered chart
            if (stackedChart !== undefined)
                stackedChart.dispose();

            $.each(data, function (k, v) {
                if ($.inArray(v[vm.firstReportFilter], uniqueCategory) == -1)
                    uniqueCategory.push(v[vm.firstReportFilter]);
                if ($.inArray(v[vm.secondReportFilter], chartCategory) == -1)
                    chartCategory.push(v[vm.secondReportFilter]);
            });

            for (var k in uniqueCategory)
                chartLabel.push({ "label": uniqueCategory[k] });

            vm.mschartdatasource.dataset = [];

            $.each(chartCategory, function (i, j) {
                chartData = [];
                color = '#' + Math.random().toString(16).slice(2, 8).toUpperCase();
                $.each(uniqueCategory, function (m, n) {
                    value = 0.0;
                    $.each(data, function (k, v) {
                        if (v[vm.secondReportFilter] == j && v[vm.firstReportFilter] == n) {

                            if (vm.salesType != 'yearly')
                                value = parseFloat(v['TotalSales(TK)']);
                            else
                                value = parseFloat(v['Share']);
                            return false;
                        }
                    });
                    if (value > 0)
                        chartData.push({ "value": value, "color": color });
                    else
                        chartData.push({ "value": "", "color": color });
                });
                vm.mschartdatasource.dataset.push({ "seriesname": j, "data": chartData });
            });

            //chart attributes
            vm.mschartdatasource.chart.showpercentvalues = vm.salesType != 'yearly' ? '0' : '1';
            vm.mschartdatasource.chart.numberPrefix = vm.salesType != 'yearly' ? 'TK ' : '';
            vm.mschartdatasource.chart.xAxisName = vm.firstReportFilter;
            vm.mschartdatasource.chart.yAxisName = vm.yAxisName;

            //chart data
            vm.mschartdatasource.categories = [];
            var category = [];
            category = chartLabel;
            vm.mschartdatasource.categories.push({ category });

            //rendering chart
            chart = new FusionCharts({ renderAt: "stacked_chart_container", dataSource: vm.mschartdatasource, type: "mscolumn3d", id: 'stackedChart', width: "100%", height: "700" });
            chart.render();
        }

        //Excel Export
        vm.exportToExcel = function (tableId) {
            var exportHref = excelExportService.tableToExcel(tableId, 'Sales Data');
            $timeout(function () { location.href = exportHref; }, 100);
        }

        //common data load fail function
        function dataLoadFailed(response) {
            vm.isSearchbtnDisabled = false;
            notificationService.displayError(response.data);
        }
    }

})(angular.module('salesVentana'));