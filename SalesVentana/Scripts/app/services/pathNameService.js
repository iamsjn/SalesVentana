(function (app) {

    'use strict'

    app.factory('pathNameService', pathNameService);

    function pathNameService() {

        var service = {
            pathName: window.location.pathname
        };

        return service;
    }

})(angular.module('common.core'))