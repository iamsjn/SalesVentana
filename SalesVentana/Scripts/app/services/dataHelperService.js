(function (app) {
    'use strict'

    app.factory('dataHelperService', dataHelperService);

    function dataHelperService() {
        return {
            getHeader: function (data, exception) {
                var temp = [];
                var headerData = [];

                $.each(data, function (k, v) {
                    temp = (k, v);
                })

                for (var header in temp) {
                    if (exception.indexOf(header) === -1)
                        headerData.push(header);
                }

                return headerData;
            },
            getGrandTotal: function (index, data, header) {
                var total = 0;
                var grandTotal = '';

                $.each(data, function (k, v) {
                    if (index != 0 && (isNaN(v[header]) || v[header] == null)) {
                        total = 0;
                        return false;
                    }
                    else if (!isNaN(v[header]) && v[header] != null) {
                        total += parseFloat(v[header]);
                    }
                    else if (index == 0 && isNaN(v[header])) {
                        grandTotal = 'Grand-Total';
                        return false;
                    }
                })

                if (total > 0 && index > 0)
                    grandTotal = total;
                else if (index > 0)
                    grandTotal = '';

                return grandTotal;
            },
            getCommaSeparatedData: function (data, header, exception) {
                if (!isNaN(data) && data != null && data != '' && exception.indexOf(header) === -1)
                    return parseFloat(data).toLocaleString();
                else if (isNaN(data) || data != null)
                    return data;
                else if (data == null)
                    return '';
                else
                    return 0;
            },
            getFormattedData: function (data, format) {
                if (!isNaN(data) && data != null && data != '')
                    return format == 'Million' ? data / 1000000 : data;
                else
                    return data;
            }
        }
    }
})(angular.module('common.core'));