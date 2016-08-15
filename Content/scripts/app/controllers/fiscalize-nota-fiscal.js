'use strict';

app.controller('FiscalizeNotaFiscalController', ["$scope", "$api", "$routeParams",
    function ($scope, $api, $routeParams) {
    	$loading.show();

    	$api.get('Fiscalize', $routeParams.id).then(function (response) {
    		$loading.hide();
    		$scope.nota = JSON.parse(response.data);
    	});
    
}]);