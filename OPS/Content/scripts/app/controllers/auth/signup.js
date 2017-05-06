'use strict';
app.controller('SignupController', ['$scope', '$location', '$timeout', 'authService', function ($scope, $location, $timeout, authService) {

    $scope.savedSuccessfully = false;
    $scope.message = "";

    $scope.registration = {
        userName: "",
        password: "",
        confirmPassword: ""
    };

    $scope.signUp = function () {

        authService.saveRegistration($scope.registration).then(function (response) {

            $scope.savedSuccessfully = true;
				$scope.message = "O usuário foi registrado com sucesso, você será rediciado para a página de login em 2 segundos.";
            startTimer();

        },
         function (response) {
             var errors = [];
             for (var key in response.data.ModelState) {
                 var model = response.data.ModelState[key];
                 if (!Array.isArray(model)) {
                     errors.push("<li>" + model + "</li>");
                 } else {
                     for (var subKey in model) {
                         errors.push("<li>" + model[subKey] + "</li>");
                     }
                 }
             }
             $scope.message = "Falha ao registrar usuário devido a: <ul style='padding-left: 25px;'>" + errors.join(' ') + "</ul>";
         });
    };

    var startTimer = function () {
        var timer = $timeout(function () {
            $timeout.cancel(timer);
            $location.path('/login');
        }, 2000);
    }

}]);