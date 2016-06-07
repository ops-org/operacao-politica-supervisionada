'use strict';

app.controller('DeputadoListaController', ['$scope', "$tabela", "$api",
    function ($scope, $tabela, $api) {
    	$scope.visao = 'parlamentar.html';
    	$scope.filtro = {};

    	$scope.Pesquisar = function () {
    		$scope.filtro.deputado = $("#lstParlamentar").val();
    		$scope.filtro.despesa = $("#lstDespesa").val();
    		$scope.filtro.estado = $("#lstUF").val();
    		$scope.filtro.partido = $("#lstPartido").val();
    		$scope.filtro.fornecedor = $("#lstFornecedor").val();
    		$scope.filtro.periodo = $("#lstPerido").val();
    		$scope.filtro.agrupamento = $("#lstAgrupamento").val();

    		$scope.BuscaGrid();
    	};

    	$scope.BuscaGrid = function () {
    		$scope.tableParams = $tabela.databind('Deputado/LancamentosPorParlamentar', $scope.filtro);
    	}

    	$scope.Limpar = function () {
    		$scope.filtro = {};
    		$scope.Pesquisar();
    	}

    	//$scope.columns = [
        //        { title: 'Nome', field: 'NomeParlamentar' },
        //        { title: 'UF', field: 'SgUF' },
        //        { title: 'Partido', field: 'SgPartido' },
        //        { title: 'Valor Total', field: 'VlrTotal' }
    	//];

    	//<tr ng-repeat="row in $data">
		//	<td data-title="'Nome'" style="width:30%" sortable="'NomeParlamentar'"><a href="#/deputado/{{row.IdeCadastro}}">{{row.NomeParlamentar}}</a></td>
		//	<td data-title="'UF'" style="width:30%" sortable="'SgUF'">{{row.SgUF}}</td>
		//	<td data-title="'Partido'" style="width:30%" sortable="'SgPartido'">{{row.SgPartido}}</td>
		//	<td data-title="'Valor Total'" style="width:30%" sortable="'VlrTotal'">{{row.VlrTotal}}</td>
		//</tr>

    	//$scope.CaregarListas();
    	$scope.Pesquisar();

    	OPS.select("#lstParlamentar", "./api/deputado/pesquisa");
    	OPS.select("#lstDespesa", "./api/despesa/pesquisa");
    	OPS.select("#lstUF", "./api/estado/pesquisa");
    	OPS.select("#lstPartido", "./api/partido/pesquisa");
    	OPS.select("#lstFornecedor", "./api/fornecedor/pesquisa");
    }]);
