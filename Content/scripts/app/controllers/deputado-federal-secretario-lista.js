'use strict';

app.controller('DeputadoFederalSecretariosListaController', ["$scope", "$tabela", "$api",
    function ($scope, $tabela, $api) {

    	$scope.BuscaGrid = function () {
    		var filtro = {};

    		$scope.tableParams = $tabela.databind('Deputado/Secretarios', filtro);
    	}

    	$scope.BuscaGrid();
    }]);