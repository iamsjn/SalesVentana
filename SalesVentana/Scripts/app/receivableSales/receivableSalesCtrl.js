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

    app.controller('receivableSalesCtrl', receivableSalesCtrl);

    receivableSalesCtrl.$inject = ['$scope', '$timeout', 'apiService', 'notificationService', 'excelExportService', 'dataSharingService', 'pathNameService', 'Popeye', 'dataHelperService'];

    function receivableSalesCtrl($scope, $timeout, apiService, notificationService, excelExportService, dataSharingService, pathNameService, Popeye, dataHelperService) {
        //variable declaration
        var vm = this;
        vm.inputChannels = [];
        vm.outputChannels = [];
        vm.inputWorkorders = [];
        vm.outputWorkorders = [];
        vm.inputCustomers = [];
        vm.outputCustomers = [];
        vm.inputSalesPerson = [];
        vm.outputSalesPerson = [];
        vm.rsHeader = [];
        vm.rsData = [];
        vm.valueFormatter = 'Million';
        vm.currentPage = 0;
        vm.totalItems = 0;
        vm.entryLimit = 0;
        vm.noOfPages = 0;
        vm.search = {};

        //apiServices
        vm.initData = function () {

            if ($scope.userData.isUserLoggedIn) {
                loadInitData();
                vm.filterRSSummary();
            }
        }

        function loadInitData() {
            apiService.get('api/receivable-sales/initial-data/#', null,
                loadInitDataLoadCompleted,
                dataLoadFailed);
        }

        function loadInitDataLoadCompleted(result) {
            vm.inputChannels = result.data.channels;
            vm.inputWorkorders = result.data.workorders;
            vm.inputCustomers = result.data.customers;
            vm.inputSalesPerson = result.data.salesPersons;
        }

        vm.filterRSSummary = function () {

            var searchCriteria = [];
            var channelIds = '', workorderIds = '', customerIds = '', salesPersonIds = '';

            $.each(vm.outputChannels, function () {
                channelIds += this.channelId + ',';
            })

            $.each(vm.outputWorkorders, function () {
                workorderIds += this.workorderId + ',';
            })

            $.each(vm.outputCustomers, function () {
                customerIds += this.customerId + ',';
            })

            $.each(vm.outputSalesPerson, function () {
                salesPersonIds += this.salesPersonId + ',';
            })

            searchCriteria = {
                channelIds: channelIds, workorderIds: workorderIds, customerIds: customerIds, salesPersonIds: salesPersonIds
            };

            vm.rsPromise = apiService.post('api/receivable-sales/rs-summary/#', searchCriteria,
                rsSummaryDataLoadCompleted,
                dataLoadFailed);
        }

        function rsSummaryDataLoadCompleted(result) {

            vm.valueFormatter = 'Million';
            vm.rsData = result.data.table;
            //pagination
            vm.currentPage = 1;
            vm.totalItems = vm.rsData.length;
            vm.entryLimit = 15;
            vm.noOfPages = Math.ceil(vm.totalItems / vm.entryLimit);

            vm.rsHeader = dataHelperService.getHeader(vm.rsData, ['customerID']);

            getGrandTotal();
        }

        function getGrandTotal() {

            var total = '';
            vm.totalData = [];

            for (var i = 0; i < vm.rsHeader.length; i++) {
                total = dataHelperService.getGrandTotal(i, vm.rsData, vm.rsHeader[i]);
                if (!isNaN(total)) {
                    if (vm.rsHeader[i] != 'No. Of W/O')
                        total = dataHelperService.getFormattedData(total, vm.valueFormatter);
                    vm.totalData.push(total.toLocaleString());
                }
                else {
                    vm.totalData.push(total);
                }
            }
            vm.rsTotal = JSON.parse(JSON.stringify(vm.totalData));
        }

        vm.getCommaSeparatedData = function (data, header, exception) {
            if (header != 'No. Of W/O')
                data = dataHelperService.getFormattedData(data, vm.valueFormatter);
            return dataHelperService.getCommaSeparatedData(data, header, exception);
        }

        vm.changeValueFormat = function () {
            getGrandTotal();
            vm.rsData = vm.rsData;
        }

        vm.detailRS = function (event) {
            dataSharingService.setData([$(event.target).attr('data-id')]);
            $timeout(function () {
                var modal = Popeye.openModal({
                    templateUrl: pathNameService.pathName + "Scripts/app/receivableSales/receivableSalesDetail.html",
                    controller: "receivableSalesDetailCtrl as receivableSalesDetailCtrl"
                })
            }, 500)
        }

        //common data load fail function
        function dataLoadFailed(response) {
            notificationService.displayError(response.data);
        }

        //Excel Export
        vm.exportToExcel = function (tableId) {
            var exportHref = excelExportService.tableToExcel(tableId, 'Receivable Sales');
            $timeout(function () { location.href = exportHref; }, 100);
        }
    }

})(angular.module('salesVentana'));