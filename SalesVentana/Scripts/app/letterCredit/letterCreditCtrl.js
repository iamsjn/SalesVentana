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

    app.controller('letterCreditCtrl', salesCtrl);

    salesCtrl.$inject = ['$scope', '$timeout', 'apiService', 'notificationService', 'excelExportService'];

    function salesCtrl($scope, $timeout, apiService, notificationService, excelExportService) {
        //variable declaration
        $scope.inputStatuses = [];
        $scope.outputStatuses = [];
        $scope.inputBanks = [];
        $scope.outputBanks = [];
        $scope.inputSuppliers = [];
        $scope.outputSuppliers = [];
        $scope.inputTerms = [];
        $scope.outputTerms = [];
        $scope.issueDate = '';
        $scope.lcHeader = [];
        $scope.lcData = [];
        $scope.currentPage = 0;
        $scope.totalItems = 0;
        $scope.entryLimit = 0;
        $scope.noOfPages = 0;
        $scope.search = {};
        //drop down filling

        //apiServices
        $scope.loadInitData = function () {

            if ($scope.userData.isUserLoggedIn) {

                loadStatus();
                loadBank();
                loadSupplier();
                loadTerms();
                loadLCSummary();
            }
        }

        function loadStatus() {
            apiService.get('api/lettercredit/status/#', null,
                statusDataLoadCompleted,
                dataLoadFailed);
        }

        function statusDataLoadCompleted(result) {
            $scope.inputStatuses = result.data.statuses;
        }

        function loadBank() {
            apiService.get('api/lettercredit/bank/#', null,
                bankDataLoadCompleted,
                dataLoadFailed);
        }

        function bankDataLoadCompleted(result) {
            $scope.inputBanks = result.data.banks;
        }

        function loadSupplier() {
            apiService.get('api/lettercredit/supplier/#', null,
                supplierDataLoadCompleted,
                dataLoadFailed);
        }

        function supplierDataLoadCompleted(result) {
            $scope.inputSuppliers = result.data.suppliers;
        }

        function loadTerms() {
            apiService.get('api/lettercredit/term/#', null,
                termDataLoadCompleted,
                dataLoadFailed);
        }

        function termDataLoadCompleted(result) {
            $scope.inputTerms = result.data.terms;
        }

        function loadLCSummary() {
            apiService.get('api/lettercredit/lc-summary/#', null,
                lcSummaryDataLoadCompleted,
                dataLoadFailed);
        }

        function lcSummaryDataLoadCompleted(result) {

            $scope.lcData = result.data.table;

            //pagination
            $scope.currentPage = 1;
            $scope.totalItems = $scope.lcData.length;
            $scope.entryLimit = 25;
            $scope.noOfPages = Math.ceil($scope.totalItems / $scope.entryLimit);

            getDataHeader();
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

        function getDataHeader() {

            var test = [];
            $scope.lcHeader = [];

            $.each($scope.lcData, function (k, v) {
                test = (k, v);
            })

            for (var header in test)
                $scope.lcHeader.push(header);
        }

        //common data load fail function
        function dataLoadFailed(response) {
            notificationService.displayError(response.data);
        }

        //Excel Export
        $scope.exportToExcel = function (tableId) {
            var exportHref = excelExportService.tableToExcel(tableId, 'LC Data');
            $timeout(function () { location.href = exportHref; }, 100);
        }

        //function calling

        //library initialization
        $('#issueDate').daterangepicker();
    }

})(angular.module('salesVentana'));