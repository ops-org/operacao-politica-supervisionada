'use strict';

app.controller('DeputadoFederalSecretariosDetalhesController', ["$scope", "$routeParams", "$api",
    function ($scope, $routeParams, $api) {

    	document.title = "OPS :: Secretários parlamentares";
    	$scope.disqusConfig = {
    		disqus_identifier: 'deputado-federal-secretario-' + $routeParams.id.toString(),
    		disqus_url: base_url + '/deputado-federal/' + $routeParams.id.toString() + '/secretario'
    	};

    	$api.get('Deputado/SecretariosPorDeputado', $routeParams.id).success(function (response) {
    		$scope.deputado_federal_secretarios = response;

    		$scope.IdCadastro = $routeParams.id;
    		$scope.NomeParlamentar = response.length > 0 ? response[0].NomeParlamentar : null;
    		document.title = "OPS :: Secretários parlamentares do dep. " + $scope.NomeParlamentar;
    	});

    }]);