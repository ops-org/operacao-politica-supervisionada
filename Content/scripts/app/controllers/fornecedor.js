'use strict';

app.controller('FornecedorDetalhesController', ["$scope", "$api", "$routeParams",
	function ($scope, $api, $routeParams) {

		//TODO: Corrigir rota
		$api.get('fornecedor/?value=' + $routeParams.id).success(function (response) {
			$scope.fornecedor = response;
		});

		//TODO: Corrigir rota
		$api.get('fornecedor/QuadroSocietario/?value=' + $routeParams.id).success(function (response) {
			$scope.fornecedorQuadroSocietario = response;
		});

		loadAuditoriaFornecedor();
	}]);