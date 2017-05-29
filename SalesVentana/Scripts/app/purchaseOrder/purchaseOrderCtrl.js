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

    app.controller('purchaseOrderCtrl', purchaseOrderCtrl);

    purchaseOrderCtrl.$inject = ['$scope', '$timeout', 'apiService', 'notificationService', 'excelExportService', 'dataSharingService', 'pathNameService', 'Popeye', 'dataHelperService'];

    function purchaseOrderCtrl($scope, $timeout, apiService, notificationService, excelExportService, dataSharingService, pathNameService, Popeye, dataHelperService) {
        //variable declaration
        var vm = this;
        vm.bpHeader = [];
        vm.bpData = [];
        vm.itemHeader = [];
        vm.itemData = [];
        vm.bpValueFormatter = 'Million';
        vm.itemValueFormatter = 'Million';
        vm.bpCurrentPage = 0;
        vm.bpTotalItems = 0;
        vm.bpEntryLimit = 0;
        vm.bpNoOfPages = 0;
        vm.itemCurrentPage = 0;
        vm.itemTotalItems = 0;
        vm.itemEntryLimit = 0;
        vm.itemNoOfPages = 0;

        //apiServices
        vm.initData = function () {

            if ($scope.userData.isUserLoggedIn) {
                getPurchaseOrderDetail();
            }
        }

        function getPurchaseOrderDetail() {

            vm.poPromise = apiService.get('api/po/po-detail/#', null,
                purchaseOrderDataLoadCompleted,
                dataLoadFailed);
        }

        function purchaseOrderDataLoadCompleted(result) {

            vm.bpValueFormatter = 'Million';
            vm.itemValueFormatter = 'Million';
            vm.bpData = result.data.bp;
            vm.itemData = result.data.item;
            //pagination
            vm.bpCurrentPage = 1;
            vm.bpTotalItems = vm.bpData.length;
            vm.bpEntryLimit = 15;
            vm.bpNoOfPages = Math.ceil(vm.bpTotalItems / vm.bpEntryLimit);

            vm.itemCurrentPage = 1;
            vm.itemTotalItems = vm.itemData.length;
            vm.itemEntryLimit = 15;
            vm.itemNoOfPages = Math.ceil(vm.itemTotalItems / vm.itemEntryLimit);

            vm.bpHeader = dataHelperService.getHeader(vm.bpData, ['BPID']);
            getBPGrandTotal()
            vm.bpTotal = JSON.parse(JSON.stringify(vm.bpTotalData));

            vm.itemHeader = dataHelperService.getHeader(vm.itemData, ['SKUID']);
            getItemGrandTotal()
            vm.itemTotal = JSON.parse(JSON.stringify(vm.itemTotalData));
        }

        vm.getBPCommaSeparatedData = function (data, header, exception) {
            if (header != 'POQty')
                data = dataHelperService.getFormattedData(data, vm.bpValueFormatter);
            return dataHelperService.getCommaSeparatedData(data, header, exception);
        }

        vm.getItemCommaSeparatedData = function (data, header, exception) {
            if (header != 'POQty')
                data = dataHelperService.getFormattedData(data, vm.itemValueFormatter);
            return dataHelperService.getCommaSeparatedData(data, header, exception);
        }


        function getBPGrandTotal() {

            var total = '';
            vm.bpTotalData = [];

            for (var i = 0; i < vm.bpHeader.length; i++) {
                total = dataHelperService.getGrandTotal(i, vm.bpData, vm.bpHeader[i]);
                if (!isNaN(total)) {
                    if (vm.bpHeader[i] != 'POQty')
                        total = dataHelperService.getFormattedData(total, vm.bpValueFormatter);
                    vm.bpTotalData.push(total.toLocaleString());
                }
                else {
                    vm.bpTotalData.push(total);
                }
            }
        }


        function getItemGrandTotal() {

            var total = '';
            vm.itemTotalData = [];

            for (var i = 0; i < vm.itemHeader.length; i++) {
                total = dataHelperService.getGrandTotal(i, vm.itemData, vm.itemHeader[i]);
                if (!isNaN(total)) {
                    if (vm.itemHeader[i] != 'POQty')
                        total = dataHelperService.getFormattedData(total, vm.itemValueFormatter);
                    vm.itemTotalData.push(total.toLocaleString());
                }
                else {
                    vm.itemTotalData.push(total);
                }
            }
        }

        vm.changeValueFormat = function (x) {
            if (x == 'bp') {
                getBPGrandTotal()
                vm.bpTotal = JSON.parse(JSON.stringify(vm.bpTotalData));
                vm.bpData = vm.bpData;
            }
            else if (x == 'item') {
                getItemGrandTotal()
                vm.itemTotal = JSON.parse(JSON.stringify(vm.itemTotalData));
                vm.itemData = vm.itemData;
            }
        }

        //common data load fail function
        function dataLoadFailed(response) {
            notificationService.displayError(response.data);
        }

        //Excel Export
        vm.exportToExcel = function (tableId) {
            var exportHref = excelExportService.tableToExcel(tableId, 'Purchase Order Detail');
            $timeout(function () { location.href = exportHref; }, 100);
        }
    }

})(angular.module('salesVentana'));