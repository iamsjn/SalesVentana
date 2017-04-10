(function () {
    'use strict';

    angular.module('salesVentana', ['common.core', 'common.ui'])
        .config(config).run(run);

    config.$inject = ['$routeProvider'];
    function config($routeProvider) {
        $routeProvider
            .when("/", {
                templateUrl: "scripts/app/account/login.html",
                controller: "loginCtrl"
            })
            .when("/home", {
                templateUrl: "scripts/app/home/index.html",
                controller: "indexCtrl",
                resolve: { isAuthenticated: isAuthenticated }
            })
            .when("/register", {
                templateUrl: "scripts/app/account/register.html",
                controller: "registerCtrl"
            })
            .when("/customers", {
                templateUrl: "scripts/app/customers/customers.html",
                controller: "customersCtrl"
            })
            .when("/customers/register", {
                templateUrl: "scripts/app/customers/register.html",
                controller: "customersRegCtrl",
                resolve: { isAuthenticated: isAuthenticated }
            })
            .when("/sales", {
                templateUrl: "scripts/app/sales/sales.html",
                controller: "salesCtrl",
                resolve: { isAuthenticated: isAuthenticated }
            })
            .when("/movies", {
                templateUrl: "scripts/app/movies/movies.html",
                controller: "moviesCtrl"
            })
            .when("/movies/add", {
                templateUrl: "scripts/app/movies/add.html",
                controller: "movieAddCtrl",
                resolve: { isAuthenticated: isAuthenticated }
            })
            .when("/movies/:id", {
                templateUrl: "scripts/app/movies/details.html",
                controller: "movieDetailsCtrl",
                resolve: { isAuthenticated: isAuthenticated }
            })
            .when("/movies/edit/:id", {
                templateUrl: "scripts/app/movies/edit.html",
                controller: "movieEditCtrl"
            })
            .when("/rental", {
                templateUrl: "scripts/app/rental/rental.html",
                controller: "rentStatsCtrl"
            }).otherwise({ redirectTo: "/" });
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
            $location.path('/login');
        }
    }

})();