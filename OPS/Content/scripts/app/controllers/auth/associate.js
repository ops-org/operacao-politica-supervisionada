'use strict';
app.controller('AssociateController', ['$scope', '$location', '$timeout', 'authService', 'localStorageService',
   function ($scope, $location, $timeout, authService, localStorageService) {

      $scope.savedSuccessfully = false;
      $scope.message = "";

      $scope.registerData = {
         userName: authService.externalAuthData.userName,
         provider: authService.externalAuthData.provider,
         externalAccessToken: authService.externalAuthData.externalAccessToken
      };

      $scope.registerExternal = function () {

         authService.registerExternal($scope.registerData).then(function (response) {

            $scope.savedSuccessfully = true;
            $scope.message = "O usuário foi registrado com sucesso, você será redigido para a página de pedidos em 2 segundos.";
            startTimer();

         },
           function (response) {
              var errors = [];
              for (var key in response.ModelState) {
                 errors.push(response.ModelState[key]);
              }
              $scope.message = "Falha ao registrar usuário devido a: " + errors.join(' ');
           });
      };

      var startTimer = function () {
         var timer = $timeout(function () {
            $timeout.cancel(timer);

            var returnUrl = localStorageService.get('returnUrl');
            if (returnUrl) {
               localStorageService.remove('returnUrl');
               $location.path(returnUrl);
            } else{
               $location.path('/');
            }
         }, 2000);
      }

   }]);