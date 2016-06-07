'use strict';

app.controller('FornecedorListaController', ['$scope', "$tabela",
    function ( $scope, $tabela) {

    	$scope.PesquisarGeral = function () {
    		$scope.filtro.ANI_Nome = '';
    		$scope.filtro.PRO_Nome = '';
    		$scope.filtro.RAC_Nome = '';
    		$scope.BuscaGrid();
    	};


    	$scope.Pesquisar = function () {
    		$scope.filtro.PesquisaGeral = '';
    		$scope.BuscaGrid();
    	};

    	$scope.BuscaGrid = function () {
    		$scope.tableParams = $tabela.getPagedData('fornecedor', $scope.filtro);
    	}

    	$scope.Limpar = function () {
    		$scope.filtro = {};
    		$scope.Pesquisar();
    	}

    	$scope.Limpar();
    }]);
