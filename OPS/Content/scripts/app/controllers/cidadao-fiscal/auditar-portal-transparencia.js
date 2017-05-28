'use strict';

app.controller('AuditarPortalTransparenciaController', ['$scope', '$api', '$routeParams', '$interval', 'localStorageService',
	function ($scope, $api, $routeParams, $interval, localStorageService) {

		document.title = "OPS :: Auditar portal de transparência";
		$api.get('Estado').success(function (data) {
			$scope.estados = data;
		});

		$api.get('CidadaoFiscal', $routeParams.id).success(function (data) {
			$scope.auditoria = data;
		});

		$scope.Salvar = function (helpForm) {
			if (helpForm.$valid) {
				$api.post('CidadaoFiscal', $scope.auditoria).success(function (data) {
					alert('Salvo com sucesso!');

					//if (!$routeParams.id) {
						window.location.href = './#/cidadao-fiscal/' + data;
					//}
				});
			} else {
				angular.element("[name='" + helpForm.$name + "']").find('.ng-invalid:visible:first').focus();
			}
		};

		$scope.GerarDenuncia = function () {
			window.location.href = './Api/CidadaoFiscal/GerarDenuncia/' + $routeParams.id;
		};

		$scope.codigo = $routeParams.id;
	}]);