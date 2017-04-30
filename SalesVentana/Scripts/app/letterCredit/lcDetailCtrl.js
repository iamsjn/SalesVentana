(function (app) {
    'use strict';

    app.filter('itemStartFrom', function () {
        return function (input, start) {
            if (input) {
                start = +start;
                return input.slice(start);
            }
            return [];
        };
    });

    app.filter('activityStartFrom', function () {
        return function (input, start) {
            if (input) {
                start = +start;
                return input.slice(start);
            }
            return [];
        };
    });

    app.controller('lcDetailCtrl', lcDetailCtrl);

    lcDetailCtrl.$inject = ['$scope', '$timeout', 'apiService', 'notificationService', 'excelExportService', 'dataSharingService'];

    function lcDetailCtrl($scope, $timeout, apiService, notificationService, excelExportService, dataSharingService) {
        //variable declaration
        var headerItem = [];
        $scope.itemData = [];
        $scope.expenditureData = [];
        $scope.activityData = [];

        //apiServices
        $scope.loadInitData = function () {
            loadLCItems();
            loadLCActivities();
        }

        function loadLCItems() {
            apiService.get('api/lettercredit/lc-items/' + parseInt(dataSharingService.getData()), null,
                lcItemDataLoadCompleted,
                dataLoadFailed);
        }

        function lcItemDataLoadCompleted(result) {
            $scope.itemHeader = [];
            $scope.itemData = result.data.table;

            //pagination
            $scope.itemCurrentPage = 1;
            $scope.itemTotalItems = $scope.itemData.length;
            $scope.itemEntryLimit = 15;
            $scope.itemNoOfPages = Math.ceil($scope.itemTotalItems / $scope.itemEntryLimit);

            getDataHeader($scope.itemData);
            $scope.itemHeader = headerItem;
        }

        function loadLCActivities() {
            apiService.get('api/lettercredit/lc-activities/' + parseInt(dataSharingService.getData()), null,
                lcActivityDataLoadCompleted,
                dataLoadFailed);
        }

        function lcActivityDataLoadCompleted(result) {
            $scope.activityHeader = [];
            $scope.activityData = result.data.table;

            //pagination
            $scope.activityCurrentPage = 1;
            $scope.activityTotalItems = $scope.activityData.length;
            $scope.activityEntryLimit = 15;
            $scope.activityNoOfPages = Math.ceil($scope.activityTotalItems / $scope.activityEntryLimit);

            getDataHeader($scope.activityData);
            $scope.activityHeader = headerItem;
        }

        $scope.getCommaSeparatedValue = function (data) {

            if (!isNaN(data) && data != null && data != '')
                return parseFloat(data).toLocaleString();
            else if (isNaN(data) || data != null)
                return data;
            else if (data == null)
                return '';
            else
                return 0;
        }

        function getDataHeader(dataItem) {
            headerItem = [];
            var test = [];

            $.each(dataItem, function (k, v) {
                test = (k, v);
            })

            for (var header in test)
                if (header != 'lcid')
                    headerItem.push(header);
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
    }

})(angular.module('salesVentana'));