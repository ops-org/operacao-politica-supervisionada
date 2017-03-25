'use strict';

app.controller('SetPasswordController',
[
    '$scope', '$routeParams', '$location', '$api',
    function ($scope, $routeParams, $location, $api) {
        document.title = "OPS :: Definir nova senha";

        $scope.SetPassword = function () {
            $scope.reset.UserId = $location.search().id;
            $scope.reset.Token = $location.search().token;

            $api.post('Account/SetPassword', $scope.reset)
                .success(function (data) {
                    alert('Sua senha foi redeninida com sucesso.');
                    $location.path('/login');
                });
        }
    }
]);