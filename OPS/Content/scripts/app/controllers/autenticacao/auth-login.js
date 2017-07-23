'use strict';
app.controller('LoginController', ['$scope', '$location', 'authService', 'ngAuthSettings', 'localStorageService',
   function ($scope, $location, authService, ngAuthSettings, localStorageService) {

   	$scope.loginData = {
   		userName: "",
   		password: "",
   		useRefreshTokens: true
   	};

   	$scope.message = "";

   	$scope.login = function () {
   		authService.login($scope.loginData).then(function (response) {
   			var returnUrl = localStorageService.get('returnUrl');
   			if (returnUrl) {
   				localStorageService.remove('returnUrl');
   				$location.path(returnUrl);
   			} else {
   				$location.path('/');
   			}
   		},
		  function (err) {
		  	$scope.message = err.error_description;
		  });
   	};

   	$scope.authExternalProvider = function (provider) {

   		var redirectUri = ngAuthSettings.apiServiceBaseUri + 'authcomplete.html';

   		var externalProviderUrl = ngAuthSettings.apiServiceBaseUri + "api/Account/ExternalLogin?provider=" + provider
																	+ "&response_type=token&client_id=" + ngAuthSettings.clientId
																	+ "&redirect_uri=" + redirectUri;
   		window.$windowScope = $scope;

   		var oauthWindow = window.open(externalProviderUrl, "Authenticate Account", "location=0,status=0,width=600,height=750");
   	};

   	$scope.authCompletedCB = function (fragment) {
   		$scope.$apply(function () {
   			if (fragment.haslocalaccount == 'False') {
   				authService.logOut();

   				authService.externalAuthData = {
   					provider: fragment.provider,
   					userName: fragment.external_user_name,
   					email: fragment.external_email,
   					externalAccessToken: fragment.external_access_token
   				};

   				authService.registerExternal(authService.externalAuthData).then(function (response) {
   					var returnUrl = localStorageService.get('returnUrl');
   					if (returnUrl) {
   						localStorageService.remove('returnUrl');
   						$location.path(returnUrl);
   					} else {
   						$location.path('/');
   					}
   				}, function (response) {
   					var errors = [];
   					for (var key in response.ModelState) {
   						errors.push(response.ModelState[key]);
   					}
   					$scope.message = "Falha ao registrar usuário devido a: " + errors.join(' ');
   				});
   			}
   			else {
   				//Obtain access token and redirect to orders
   				var externalData = { provider: fragment.provider, externalAccessToken: fragment.external_access_token };
   				authService.obtainAccessToken(externalData).then(function (response) {
   					var returnUrl = localStorageService.get('returnUrl');
   					if (returnUrl) {
   						localStorageService.remove('returnUrl');
   						$location.path(returnUrl);
   					} else {
   						$location.path('/');
   					}
   				},
			 function (err) {
			 	$scope.message = err.error_description;
			 });
   			}

   		});
   	}
   }]);
