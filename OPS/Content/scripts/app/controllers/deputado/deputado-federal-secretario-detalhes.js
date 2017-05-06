'use strict';

app.controller('DeputadoFederalSecretariosDetalhesController', ["$scope", "$routeParams", "$api",
    function ($scope, $routeParams, $api) {

    	document.title = "OPS :: Secretários parlamentares";
    	$scope.disqusConfig = {
    		disqus_identifier: 'deputado-federal-secretario-' + $routeParams.id.toString(),
    		disqus_url: base_url + '/deputado-federal/' + $routeParams.id.toString() + '/secretario'
    	};

    	$api.get('Deputado/' + $routeParams.id.toString() + '/Secretarios').success(function (response) {
    		$scope.deputado_federal_secretarios = response;

    		$scope.id_cf_deputado = $routeParams.id;
    		$scope.nome_parlamentar = response.length > 0 ? response[0].nome_parlamentar : null;
    		document.title = "OPS :: Secretários parlamentares do dep. " + $scope.nome_parlamentar;
    	});

    }]);