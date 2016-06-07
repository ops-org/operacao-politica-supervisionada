'use strict';

app.controller('DeputadoController', ['$scope', "$odataresource", '$routeParams',
    function ($scope, $odataresource, $routeParams) {

    	var $oDataQuery = $odataresource("/odata/camara_lancamento", {}, {}, {
    		//odatakey: 'id',
    		isodatav4: true
    	}).odata();

    	$oDataQuery.get($routeParams.id, function (data) {
    		$scope.fornecedor = data;
    	}, function (error) {
    		console.log(error);
    	});
    }]);