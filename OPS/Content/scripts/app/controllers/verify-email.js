'use strict';

app.controller('VerifyEmailController',
[
    '$scope', '$routeParams', '$location', '$api',
    function ($scope, $routeParams, $location, $api) {
        $scope.loading = true;
        document.title = "OPS :: Confirmar e-mail";

        var verify = {
            UserId: $location.search().id,
            Token: $location.search().token
        };

        $api.post('Account/VerifyEmail', verify)
                .success(function (data) {
                    $scope.loading = false;
                });
    }
]);