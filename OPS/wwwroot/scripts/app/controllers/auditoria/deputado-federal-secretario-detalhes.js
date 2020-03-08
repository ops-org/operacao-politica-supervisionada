'use strict';

app.controller('DeputadoFederalSecretariosDetalhesController', ["$scope", "$tabela", "$api", "$routeParams", "$queryString",
    function ($scope, $tabela, $api, $routeParams, $queryString) {

		document.title = "OPS :: Secretários parlamentares";
    	$scope.disqusConfig = {
    		disqus_identifier: 'deputado-federal-secretario-' + $routeParams.id.toString(),
    		disqus_url: base_url + '/deputado-federal/' + $routeParams.id.toString() + '/secretario'
    	};

		$api.get('Deputado/' + $routeParams.id.toString()).success(function (response) {
			$scope.deputado_federal = response;
			
            document.title = "OPS :: " + response.nome_parlamentar + " :: Secretários parlamentares";
		});

        //var qs = $queryString.search();
        //if (qs.page) {
        //    $tabela.params.page = parseInt(qs.page);
        //}
        //if (qs.sorting) {
        //    var lstSorting = qs.sorting.split(' ');
        //    $tabela.params.sorting = {};
        //    $tabela.params.sorting[lstSorting[0]] = lstSorting[1];
        //}

        $scope.BuscaGrid = function () {
            var filtroAtivo = { Ativo: true };
            $scope.tableParamsAtivo = $tabela.databind('Deputado/' + $routeParams.id.toString() + '/Secretarios', filtroAtivo, false);

            var filtroInativo = { Ativo: false };
            $scope.tableParamsInativo = $tabela.databind('Deputado/' + $routeParams.id.toString() + '/Secretarios', filtroInativo, false);
        };

        $scope.BuscaGrid();
    }]);