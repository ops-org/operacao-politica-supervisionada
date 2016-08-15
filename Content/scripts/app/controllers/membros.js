'use strict';

app.controller('MembrosController', ["$window", "$scope", "$routeParams", "$tabela", "$api",
    function ($window, $scope, $routeParams, $tabela, $api) {

    	$scope.SelecionarEstado = function (self) {
    		$scope.BuscaGrid(self.value);
    	}

    	$scope.BuscaGrid = function (uf) {
    		$scope.tableParams = $tabela.databind('Usuario/Pesquisa', { uf: uf }, 10);
    	}

    	if ($routeParams.id) {
    		$scope.BuscaGrid($routeParams.id);
    	}
    }]);