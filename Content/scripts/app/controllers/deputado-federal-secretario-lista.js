'use strict';

app.controller('DeputadoFederalSecretariosListaController', ["$scope", "$tabela", "$api",
    function ($scope, $tabela, $api) {

    	document.title = "OPS :: Secretários parlamentares";

    	$scope.BuscaGrid = function () {
    		var filtro = {};

    		$scope.tableParams = $tabela.databind('Deputado/Secretarios', filtro);
    	}

    	$scope.BuscaGrid();
    }]);