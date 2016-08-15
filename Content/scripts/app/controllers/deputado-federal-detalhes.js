'use strict';

app.controller('DeputadoVisualizarController', ["$scope", "$api", "$routeParams",
    function ($scope, $api, $routeParams) {

    	$api.get('deputado', $routeParams.id).success(function (response) {
    		$scope.deputado_federal = response;
    	});
    }]);