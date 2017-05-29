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

    app.controller('projectCtrl', projectCtrl);

    projectCtrl.$inject = ['$scope', '$timeout', 'apiService', 'notificationService', 'excelExportService', 'dataSharingService', 'pathNameService', 'Popeye', 'dataHelperService'];

    function projectCtrl($scope, $timeout, apiService, notificationService, excelExportService, dataSharingService, pathNameService, Popeye, dataHelperService) {
        //variable declaration
        var vm = this;
        vm.inputProjects = [];
        vm.outputProjects = [];
        vm.header = [];
        vm.data = [];
        vm.projectDate = moment().subtract(30, 'days').format('MM/DD/YYYY') + ' - ' + moment().format('MM/DD/YYYY');
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
                vm.filterProjectSummary();
            }
        }

        function loadInitData() {
            apiService.get('api/project/getProject/#', null,
                loadInitDataLoadCompleted,
                dataLoadFailed);
        }

        function loadInitDataLoadCompleted(result) {
            vm.inputProjects = result.data.projects;
        }

        vm.filterProjectSummary = function () {

            var searchCriteria = [];
            var projectIds = '';
            var projectDateArr = vm.projectDate.split('-');
            var projectFromDate = projectDateArr.length > 0 ? projectDateArr[0] : '';
            var projectToDate = projectDateArr.length > 1 ? projectDateArr[1] : '';

            $.each(vm.outputProjects, function () {
                projectIds += this.projectId + ',';
            })

            searchCriteria = {
                projectIds: projectIds, projectFromDate: projectFromDate, projectToDate: projectToDate
            };

            vm.projectPromise = apiService.post('api/project/project-summary/#', searchCriteria,
                projectSummaryDataLoadCompleted,
                dataLoadFailed);
        }

        function projectSummaryDataLoadCompleted(result) {

            vm.valueFormatter = 'Million';
            vm.data = result.data.table;
            //pagination
            vm.currentPage = 1;
            vm.totalItems = vm.data.length;
            vm.entryLimit = 15;
            vm.noOfPages = Math.ceil(vm.totalItems / vm.entryLimit);

            vm.header = ['ProjectName', 'CreationDate', 'Description',
                'TotalBudget', 'PRQuantity', 'POAmount', 'MRRAmount', 'BillAmount', 'OtherAmount', 'BudgetVariance'];

            getGrandTotal();
        }

        function getGrandTotal() {

            var total = '';
            vm.totalData = [];

            for (var i = 0; i < vm.header.length; i++) {
                total = dataHelperService.getGrandTotal(i, vm.data, vm.header[i]);
                if (!isNaN(total)) {
                    if (vm.header[i] != 'PRQuantity')
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
            if (header != 'PRQuantity')
                data = dataHelperService.getFormattedData(data, vm.valueFormatter);
            return dataHelperService.getCommaSeparatedData(data, header, exception);
        }

        vm.changeValueFormat = function () {
            getGrandTotal();
            vm.data = vm.data;
        }

        vm.isbreakdownColumn = function (data, header) {
            return (data && (header == 'PRQuantity' || header == 'POAmount' || header == 'MRRAmount' || header == 'BillAmount' || header == 'OtherAmount')) ? true : false;
        }

        vm.detailProject = function (event) {

            var breakdown = $(event.target).attr('data-breakdown');
            if (breakdown == 'PRQuantity' || breakdown == 'POAmount' || breakdown == 'MRRAmount' || breakdown == 'BillAmount' || breakdown == 'OtherAmount') {
                dataSharingService.setData([$(event.target).attr('data-id'), breakdown]);
                $timeout(function () {
                    var modal = Popeye.openModal({
                        templateUrl: pathNameService.pathName + "Scripts/app/project/projectDetail.html",
                        controller: "projectDetailCtrl as projectDetailCtrl"
                    });
                }, 500)
            }
        }

        //common data load fail function
        function dataLoadFailed(response) {
            notificationService.displayError(response.data);
        }

        //Excel Export
        vm.exportToExcel = function (tableId) {
            var exportHref = excelExportService.tableToExcel(tableId, 'Projects');
            $timeout(function () { location.href = exportHref; }, 100);
        }

        //library initialization
        $('#projectDate').daterangepicker();
    }

})(angular.module('salesVentana'));