(function(app) {
    'use strict';

    app.directive('sideBar', sideBar);

    function sideBar() {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: window.location.pathname + '/scripts/app/layout/sideBar.html'
        }
    }

})(angular.module('common.ui'));