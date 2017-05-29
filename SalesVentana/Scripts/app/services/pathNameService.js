(function (app) {

    'use strict'

    app.factory('pathNameService', pathNameService);

    function pathNameService() {
        var service = {};

        if (window.location.pathname != '/')
            service = { pathName: window.location.pathname + '/' };
        else
            service = { pathName: window.location.pathname };

        return service;
    }

})(angular.module('common.core'))