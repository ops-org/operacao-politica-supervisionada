'use strict';

app.controller('VerifyEmailController',
[
    '$scope', '$routeParams', '$location', '$api',
    function ($scope, $routeParams, $location, $api) {
        document.title = "OPS :: Confirmar e-mail";

        $api.post('Account/VerifyEmail', $routeParams.token)
                .success(function (data) {
                    alert('Sua senha foi confirmado com sucesso.');
                    $location.path('/login');
                });
    }
]);