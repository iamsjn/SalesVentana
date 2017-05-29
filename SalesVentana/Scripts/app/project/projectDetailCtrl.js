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

    app.controller('projectDetailCtrl', projectDetailCtrl);

    projectDetailCtrl.$inject = ['$timeout', 'apiService', 'notificationService', 'excelExportService', 'dataSharingService', 'dataHelperService'];

    function projectDetailCtrl($timeout, apiService, notificationService, excelExportService, dataSharingService, dataHelperService) {
        //variable declaration
        var vm = this;
        var header = [];
        vm.data = [];
        vm.valueFormatter = 'Million';

        //apiServices
        vm.loadInitData = function () {
            loadRSDeatil();
        }

        function loadRSDeatil() {
            vm.detailPromise = apiService.get('api/project/project-detail/' + parseInt(dataSharingService.getData()[0]) + '/' + dataSharingService.getData()[1], null,
                projectDetailDataLoadCompleted,
                dataLoadFailed);
        }

        function projectDetailDataLoadCompleted(result) {

            vm.valueFormatter = 'Million';
            vm.header = [];
            vm.data = result.data.table;

            //pagination
            vm.currentPage = 1;
            vm.totalItems = vm.data.length;
            vm.entryLimit = 15;
            vm.noOfPages = Math.ceil(vm.totalItems / vm.entryLimit);

            vm.header = dataHelperService.getHeader(vm.data, ['ProjectID', 'SKUID', 'POID', 'SupplierID', 'MRRID', 'FinancialRequesitionItemID', 'FinancialRequisitionID']);

            getGrandTotal();
        }

        vm.getCommaSeparatedData = function (data, header, exception) {
            if (header != 'Rate' && header != 'OrderingQty'
                && header != 'PRQuantity' && header != 'MRRQuantity')
                data = dataHelperService.getFormattedData(data, vm.valueFormatter);
            return dataHelperService.getCommaSeparatedData(data, header, exception);
        }

        function getGrandTotal() {

            var total = '';
            vm.totalData = [];

            for (var i = 0; i < vm.header.length; i++) {
                total = dataHelperService.getGrandTotal(i, vm.data, vm.header[i]);
                if (!isNaN(total)) {
                    if (vm.header[i] != 'Rate' && vm.header[i] != 'OrderingQty'
                        && vm.header[i] != 'PRQuantity' && vm.header[i] != 'MRRQuantity')
                        total = dataHelperService.getFormattedData(total, vm.valueFormatter);
                    vm.totalData.push(total.toLocaleString());
                }
                else {
                    vm.totalData.push(total);
                }
            }
            vm.total = JSON.parse(JSON.stringify(vm.totalData));
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
            var exportHref = excelExportService.tableToExcel(tableId, 'Project Details');
            $timeout(function () { location.href = exportHref; }, 100);
        }
    }

})(angular.module('salesVentana'));