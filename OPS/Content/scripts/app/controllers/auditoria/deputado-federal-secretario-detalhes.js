'use strict';

app.controller('DeputadoFederalSecretariosDetalhesController', ["$scope", "$routeParams", "$api",
    function ($scope, $routeParams, $api) {

		document.title = "OPS :: Secretários parlamentares";
    	$scope.disqusConfig = {
    		disqus_identifier: 'deputado-federal-secretario-' + $routeParams.id.toString(),
    		disqus_url: base_url + '/deputado-federal/' + $routeParams.id.toString() + '/secretario'
    	};

		$api.get('Deputado/' + $routeParams.id.toString()).success(function (response) {
			$scope.deputado_federal = response;
			
			document.title = "OPS :: Secretários parlamentares do(a) dep(a). " + response.nome_parlamentar;
		});

    	$api.get('Deputado/' + $routeParams.id.toString() + '/Secretarios').success(function (response) {
    		$scope.deputado_federal_secretarios = response;
    	});

    }]);