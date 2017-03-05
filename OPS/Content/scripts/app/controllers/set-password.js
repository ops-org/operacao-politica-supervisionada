'use strict';

app.controller('SetPasswordController',
[
    '$scope', '$routeParams', '$location', '$api',
    function ($scope, $routeParams, $location, $api) {
        document.title = "OPS :: Definir nova senha";

        $scope.SetPassword = function () {
            $scope.reset.UserId = $routeParams.user_id;
            $scope.reset.Token = $routeParams.token;

            $api.post('Account/SetPassword', $scope.reset)
                .success(function (data) {
                    alert('Sua senha foi redeninida com sucesso.');
                    $location.path('/login');
                });
        }
    }
]);