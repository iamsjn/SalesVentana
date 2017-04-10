(function(app) {
    'use strict';

    app.directive('breadCrumb', breadCrumb);

    function breadCrumb() {
        return {
            restrict: 'E',
            replace: true,
            templateUrl: '/scripts/app/layout/breadCrumb.html'
        }
    }

})(angular.module('common.ui'));