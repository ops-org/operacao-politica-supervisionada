'use strict';

app.controller('ResetPasswordController',
[
    '$scope', '$location', '$api',
    function ($scope, $location, $api) {
        document.title = "OPS :: Redefinir senha";

        $scope.ResetPassword = function () {
            $api.get('Account/ResetPassword?value=' + $scope.reset.email, null)
                .success(function (data) {
                    alert('Um email foi enviado para a sua caixa de entrada. Clique no link para definir uma nova senha.');
                    $location.path('/');
                });
        }
    }
]);