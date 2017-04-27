(function () {
    'use strict';
    angular.module('salesVentana', ['common.core', 'common.ui'])
        .config(config).run(run);

    config.$inject = ['$routeProvider'];
    function config($routeProvider) {
        var pathName = window.location.pathname;
        $routeProvider
            .when("/", {
                templateUrl: pathName + "scripts/app/account/login.html",
                controller: "loginCtrl"
            })
            .when("/home", {
                templateUrl: pathName + "scripts/app/home/index.html",
                controller: "indexCtrl",
                resolve: { isAuthenticated: isAuthenticated }
            })
            .when("/register", {
                templateUrl: pathName + "scripts/app/account/register.html",
                controller: "registerCtrl"
            })
            .when("/sales", {
                templateUrl: pathName + "scripts/app/sales/sales.html",
                controller: "salesCtrl",
                resolve: { isAuthenticated: isAuthenticated }
            })
            .when("/lc", {
                templateUrl: pathName + "scripts/app/letterCredit/letterCredit.html",
                controller: "letterCreditCtrl",
                resolve: { isAuthenticated: isAuthenticated }
            })
            .otherwise({ redirectTo: "/" });
    }

    run.$inject = ['$rootScope', '$location', '$cookieStore', '$http'];

    function run($rootScope, $location, $cookieStore, $http) {
        // handle page refreshes
        $rootScope.repository = $cookieStore.get('repository') || {};
        if ($rootScope.repository.loggedUser) {
            $http.defaults.headers.common['Authorization'] = $rootScope.repository.loggedUser.authdata;
        }

        $(document).ready(function () {
            $(".fancybox").fancybox({
                openEffect: 'none',
                closeEffect: 'none'
            });

            $('.fancybox-media').fancybox({
                openEffect: 'none',
                closeEffect: 'none',
                helpers: {
                    media: {}
                }
            });

            $('[data-toggle=offcanvas]').click(function () {
                $('.row-offcanvas').toggleClass('active');
            });
        });
    }

    isAuthenticated.$inject = ['membershipService', '$rootScope', '$location'];

    function isAuthenticated(membershipService, $rootScope, $location) {
        if (!membershipService.isUserLoggedIn()) {
            $rootScope.previousState = $location.path();
            $location.path('#/');
        }
    }

})();