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

    app.controller('lcDetailCtrl', lcDetailCtrl);

    lcDetailCtrl.$inject = ['$timeout', 'apiService', 'notificationService', 'excelExportService', 'dataSharingService', 'dataHelperService'];

    function lcDetailCtrl($timeout, apiService, notificationService, excelExportService, dataSharingService, dataHelperService) {
        //variable declaration
        var vm = this;
        var headerItem = [];
        vm.itemData = [];
        vm.expenditureData = [];
        vm.activityData = [];

        //apiServices
        vm.loadInitData = function () {
            loadLCDeatil();
        }

        function loadLCDeatil() {
            vm.detailPromise = apiService.get('api/lettercredit/lc-detail/' + parseInt(dataSharingService.getData()[0]), null,
                lcDetailDataLoadCompleted,
                dataLoadFailed);
        }

        function lcDetailDataLoadCompleted(result) {
            vm.itemHeader = [];
            vm.itemData = result.data.lcItem;
            vm.expenditureHeader = [];
            vm.expenditureData = result.data.expenditure;
            vm.activityHeader = [];
            vm.activityData = result.data.activity;

            //pagination
            //lcitem
            vm.itemCurrentPage = 1;
            vm.itemTotalItems = vm.itemData.length;
            vm.itemEntryLimit = 15;
            vm.itemNoOfPages = Math.ceil(vm.itemTotalItems / vm.itemEntryLimit);

            //expenditure
            vm.expenditureCurrentPage = 1;
            vm.expenditureTotalItems = vm.expenditureData.length;
            vm.expenditureEntryLimit = 15;
            vm.expenditureNoOfPages = Math.ceil(vm.expenditureTotalItems / vm.expenditureEntryLimit);

            //activity
            vm.activityCurrentPage = 1;
            vm.activityTotalItems = vm.activityData.length;
            vm.activityEntryLimit = 15;
            vm.activityNoOfPages = Math.ceil(vm.activityTotalItems / vm.activityEntryLimit);

            vm.itemHeader = dataHelperService.getHeader(vm.itemData, ['lcid', 'SKUID']);
            vm.expenditureHeader = dataHelperService.getHeader(vm.expenditureData, ['LCID']);
            vm.activityHeader = dataHelperService.getHeader(vm.activityData, ['LCID']);
        }

        //function getGrandTotal() {

        //    var total = '';
        //    vm.totalData = [];

        //    for (var i = 0; i < vm.rsHeader.length; i++) {
        //        total = dataHelperService.getGrandTotal(i, vm.rsData, vm.rsHeader[i]);
        //        if (!isNaN(total)) {
        //            if (vm.rsHeader[i] != 'No. Of W/O')
        //                total = dataHelperService.getFormattedData(total, vm.valueFormatter);
        //            vm.totalData.push(total.toLocaleString());
        //        }
        //        else {
        //            vm.totalData.push(total);
        //        }
        //    }
        //    vm.rsTotal = JSON.parse(JSON.stringify(vm.totalData));
        //}

        vm.getCommaSeparatedData = function (data, header, exception) {
            data = dataHelperService.getFormattedData(data, vm.valueFormatter);
            return dataHelperService.getCommaSeparatedData(data, header, exception);
        }

        //common data load fail function
        function dataLoadFailed(response) {
            notificationService.displayError(response.data);
        }

        //Excel Export
        vm.exportToExcel = function (tableId) {
            var exportHref = excelExportService.tableToExcel(tableId, 'LC Data');
            $timeout(function () { location.href = exportHref; }, 100);
        }
    }

})(angular.module('salesVentana'));