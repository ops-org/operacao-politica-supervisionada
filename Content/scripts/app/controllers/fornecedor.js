'use strict';

app.controller('FornecedorController', ['$scope', "$odataresource", '$routeParams',
    function ($scope, $odataresource, $routeParams) {

    	var $oDataQuery = $odataresource("/odata/fornecedor", {}, {}, {
    		//odatakey: 'id',
    		isodatav4: true
    	}).odata();

    	$oDataQuery.get($routeParams.id, function (data) {
    		$scope.fornecedor = data;
    	}, function (error) {
    		console.log(error);
    	});
    }]);