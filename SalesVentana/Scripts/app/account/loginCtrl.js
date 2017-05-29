(function (app) {
    'use strict';

    app.controller('loginCtrl', loginCtrl);

    loginCtrl.$inject = ['$scope', 'membershipService', 'notificationService', '$rootScope', '$location'];

    function loginCtrl($scope, membershipService, notificationService, $rootScope, $location) {

        if ($scope.userData.isUserLoggedIn)
            $location.path('/sales');

        $scope.login = login;
        $scope.user = {};
        $scope.isSignInbtnDisabled = false;

        function login() {
            $scope.isSignInbtnDisabled = true;
            membershipService.login($scope.user, loginCompleted)
        }

        function loginCompleted(result) {
            $scope.isSignInbtnDisabled = false;

            if (result.data.success) {
                membershipService.saveCredentials($scope.user);
                notificationService.displaySuccess('Hello ' + $scope.user.username);
                $scope.userData.displayUserInfo();
                if ($rootScope.previousState)
                    $location.path($rootScope.previousState);
                else
                    $location.path('/sales');
            }
            else {
                notificationService.displayError('Login failed. Try again.');
            }
        }
    }

})(angular.module('common.core'));