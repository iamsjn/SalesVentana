(function (app) {
    'use strict';

    app.filter('StartFrom', function () {
        return function (input, start) {
            if (input) {
                start = +start;
                return input.slice(start);
            }
            return [];
        };
    });

    app.controller('receivableSalesDetailCtrl', receivableSalesDetailCtrl);

    receivableSalesDetailCtrl.$inject = ['$timeout', 'apiService', 'notificationService', 'excelExportService', 'dataSharingService', 'dataHelperService'];

    function receivableSalesDetailCtrl($timeout, apiService, notificationService, excelExportService, dataSharingService, dataHelperService) {
        //variable declaration
        var vm = this;
        vm.valueFormatter = 'Million';
        var header = [];
        vm.data = [];

        //apiServices
        vm.loadInitData = function () {
            loadRSDeatil();
        }

        function loadRSDeatil() {
            vm.detailPromise = apiService.get('api/receivable-sales/rs-detail/' + parseInt(dataSharingService.getData()[0]), null,
                rsDetailDataLoadCompleted,
                dataLoadFailed);
        }

        function rsDetailDataLoadCompleted(result) {

            vm.valueFormatter = 'Million';
            vm.header = [];
            vm.data = result.data.table;

            //pagination
            vm.currentPage = 1;
            vm.totalItems = vm.data.length;
            vm.entryLimit = 15;
            vm.noOfPages = Math.ceil(vm.totalItems / vm.entryLimit);

            vm.header = dataHelperService.getHeader(vm.data, ['']);

            getGrandTotal();
        }

        function getGrandTotal() {

            var total = '';
            vm.totalData = [];

            for (var i = 0; i < vm.header.length; i++) {
                total = dataHelperService.getGrandTotal(i, vm.data, vm.header[i]);
                if (!isNaN(total)) {
                    if (vm.header[i] != 'Work Order Number' && vm.header[i] != 'No. of Bills')
                        total = dataHelperService.getFormattedData(total, vm.valueFormatter);
                    vm.totalData.push(total.toLocaleString());
                }
                else {
                    vm.totalData.push(total);
                }
            }
            vm.total = JSON.parse(JSON.stringify(vm.totalData));
        }

        vm.getCommaSeparatedData = function (data, header, exception) {
            if (header != 'Work Order Number' && header != 'No. of Bills')
                data = dataHelperService.getFormattedData(data, vm.valueFormatter);
            return dataHelperService.getCommaSeparatedData(data, header, exception);
        }

        vm.changeValueFormat = function () {
            getGrandTotal();
            vm.data = vm.data;
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