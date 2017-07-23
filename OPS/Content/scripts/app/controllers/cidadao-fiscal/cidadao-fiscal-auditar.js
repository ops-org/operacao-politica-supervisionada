'use strict';

app.controller('AuditarPortalTransparenciaController', ['$scope', '$api', '$routeParams',
	function ($scope, $api, $routeParams) {

		document.title = "OPS :: Auditar portal de transparência";
		$api.get('Estado').success(function (data) {
			$scope.estados = data;
		});

		$api.get('CidadaoFiscal', $routeParams.id).success(function (data) {
			$scope.auditoria = data;
		});

		$scope.Salvar = function (form) {
			if (form.$valid) {
				$api.post('CidadaoFiscal', $scope.auditoria).success(function (data) {
					if ($routeParams.id)
						alert('Salvo com sucesso!');
					else
						window.location.href = './#!/cidadao-fiscal/' + data;
				});
			} else {
				angular.element("[name='" + form.$name + "']").find('.ng-invalid:visible:first').focus();
			}
		};

		$scope.GerarDenuncia = function () {
			$api.post('CidadaoFiscal', $scope.auditoria).success(function (data) {
				window.location.href = './Api/CidadaoFiscal/GerarDenuncia/' + data;
			});
		};

		$scope.codigo = $routeParams.id;
	}]);