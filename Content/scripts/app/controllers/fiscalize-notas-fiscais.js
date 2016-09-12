'use strict';

app.controller('FiscalizeNotasFiscaisController', ["$scope", "$api",
    function ($scope, $api) {
    	document.title = "OPS :: Fiscalize - Notas mais suspeitas";
    	
    	$api.get('Fiscalize').then(function (response) {
    		$scope.notas = response.data;
    	});
    }]);
