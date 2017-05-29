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

    app.controller('letterCreditCtrl', letterCreditCtrl);

    letterCreditCtrl.$inject = ['$scope', '$timeout', 'apiService', 'notificationService', 'excelExportService', 'dataSharingService', 'pathNameService', 'dataHelperService', 'Popeye'];

    function letterCreditCtrl($scope, $timeout, apiService, notificationService, excelExportService, dataSharingService, pathNameService, dataHelperService, Popeye) {
        //variable declaration
        var vm = this;
        vm.inputLCNos = [];
        vm.outputLCNos = [];
        vm.inputStatuses = [];
        vm.outputStatuses = [];
        vm.inputBanks = [];
        vm.outputBanks = [];
        vm.inputSuppliers = [];
        vm.outputSuppliers = [];
        vm.inputTerms = [];
        vm.outputTerms = [];
        vm.issueDate = moment().subtract(30, 'days').format('MM/DD/YYYY') + ' - ' + moment().format('MM/DD/YYYY');
        vm.lcHeader = [];
        vm.lcData = [];
        vm.valueFormatter = 'Million';
        vm.currentPage = 0;
        vm.totalItems = 0;
        vm.entryLimit = 0;
        vm.noOfPages = 0;
        vm.search = {};
        //drop down filling

        //apiServices
        vm.initData = function () {

            if ($scope.userData.isUserLoggedIn) {
                loadInitData();
                vm.filterLCSummary();
            }
        }

        function loadLCNo() {
            apiService.get('api/lettercredit/lcnos/#', null,
                lcNoDataLoadCompleted,
                dataLoadFailed);
        }

        function lcNoDataLoadCompleted(result) {
            vm.inputLCNos = result.data.lcNos;
        }

        function loadStatus() {
            apiService.get('api/lettercredit/status/#', null,
                statusDataLoadCompleted,
                dataLoadFailed);
        }

        function statusDataLoadCompleted(result) {
            vm.inputStatuses = result.data.statuses;
        }

        function loadBank() {
            apiService.get('api/lettercredit/bank/#', null,
                bankDataLoadCompleted,
                dataLoadFailed);
        }

        function bankDataLoadCompleted(result) {
            vm.inputBanks = result.data.banks;
        }

        function loadSupplier() {
            apiService.get('api/lettercredit/supplier/#', null,
                supplierDataLoadCompleted,
                dataLoadFailed);
        }

        function supplierDataLoadCompleted(result) {
            vm.inputSuppliers = result.data.suppliers;
        }

        function loadTerms() {
            apiService.get('api/lettercredit/term/#', null,
                termDataLoadCompleted,
                dataLoadFailed);
        }

        function termDataLoadCompleted(result) {
            vm.inputTerms = result.data.terms;
        }

        function loadInitData() {
            apiService.get('api/lettercredit/initial-data/#', null,
                loadInitDataLoadCompleted,
                dataLoadFailed);
        }

        function loadInitDataLoadCompleted(result) {
            vm.inputLCNos = result.data.lcNos;
            vm.inputStatuses = result.data.statuses;
            vm.inputBanks = result.data.banks;
            vm.inputSuppliers = result.data.suppliers;
            vm.inputTerms = result.data.terms;
        }

        vm.filterLCSummary = function () {

            var searchCriteria = [];
            var lcIds = '', statusIds = '', supplierIds = '', bankIds = '', termIds = '';
            var issueDateArr = vm.issueDate.split('-');
            var issueFromDate = issueDateArr.length > 0 ? issueDateArr[0] : '';
            var issueToDate = issueDateArr.length > 1 ? issueDateArr[1] : '';

            $.each(vm.outputLCNos, function () {
                lcIds += this.lcId + ',';
            })

            $.each(vm.outputStatuses, function () {
                statusIds += this.statusId + ',';
            })

            $.each(vm.outputSuppliers, function () {
                supplierIds += this.supplierId + ',';
            })

            $.each(vm.outputBanks, function () {
                bankIds += this.bankId + ',';
            })

            $.each(vm.outputTerms, function () {
                termIds += this.termId + ',';
            })

            searchCriteria = {
                lcIds: lcIds, statusIds: statusIds, supplierIds: supplierIds, bankIds: bankIds, termIds: termIds, issueFromDate: issueFromDate, issueToDate: issueToDate
            };

            vm.lcPromise = apiService.post('api/lettercredit/lc-summary/#', searchCriteria,
                lcSummaryDataLoadCompleted,
                dataLoadFailed);
        }

        function lcSummaryDataLoadCompleted(result) {

            vm.valueFormatter = 'Million';
            vm.lcData = result.data.table;

            //pagination
            vm.currentPage = 1;
            vm.totalItems = vm.lcData.length;
            vm.entryLimit = 15;
            vm.noOfPages = Math.ceil(vm.totalItems / vm.entryLimit);

            vm.lcHeader = dataHelperService.getHeader(vm.lcData, ['lcid']);

            getGrandTotal();
        }

        vm.getCommaSeparatedData = function (data, header, exception) {
            if (header == 'LC Value' || header == 'LC Value(TK)')
                data = dataHelperService.getFormattedData(data, vm.valueFormatter);
            return dataHelperService.getCommaSeparatedData(data, header, exception);
        }

        function getGrandTotal() {

            var total = '';
            vm.totalData = [];

            for (var i = 0; i < vm.lcHeader.length; i++) {
                total = dataHelperService.getGrandTotal(i, vm.lcData, vm.lcHeader[i]);
                if (!isNaN(total)) {
                    if (vm.lcHeader[i] == 'LC Value' || vm.lcHeader[i] == 'LC Value(TK)')
                        total = dataHelperService.getFormattedData(total, vm.valueFormatter);
                    vm.totalData.push(total.toLocaleString());
                }
                else {
                    vm.totalData.push(total);
                }
            }
            vm.lcTotal = JSON.parse(JSON.stringify(vm.totalData));
        }

        vm.changeValueFormat = function () {
            getGrandTotal();
            vm.lcData = vm.lcData;
        }

        vm.detailLC = function (event) {

            dataSharingService.setData([$(event.target).attr('data-id')]);
            $timeout(function () {
                var modal = Popeye.openModal({
                    templateUrl: pathNameService.pathName + "Scripts/app/letterCredit/lcDetail.html",
                    controller: "lcDetailCtrl as lcDetailCtrl"
                });
            }, 1000)
        };

        //common data load fail function
        function dataLoadFailed(response) {
            notificationService.displayError(response.data);
        }

        //Excel Export
        vm.exportToExcel = function (tableId) {
            var exportHref = excelExportService.tableToExcel(tableId, 'LC Data');
            $timeout(function () { location.href = exportHref; }, 100);
        }

        //library initialization
        $('#issueDate').daterangepicker();
    }

})(angular.module('salesVentana'));