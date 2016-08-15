'use strict';

app.controller('DeputadoFederalSecretariosDetalhesController', ["$scope", "$routeParams", "$api",
    function ($scope, $routeParams, $api) {

    	$api.get('Deputado/SecretariosPorDeputado', $routeParams.id).success(function (response) {
    		$scope.deputado_federal_secretarios = response;

    		$scope.NomeParlamentar = response.length > 0 ? response[0].NomeParlamentar : null;
    	});

    }]);