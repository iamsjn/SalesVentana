(function (app) {

    'use strict'

    app.factory('dataSharingService', dataSharingService);

    function dataSharingService() {

        var data = '';

        return {
            getData: function () {
                return data;
            },
            setData: function (value) {
                data = value;
            }
        };
    }

})(angular.module('common.core'))