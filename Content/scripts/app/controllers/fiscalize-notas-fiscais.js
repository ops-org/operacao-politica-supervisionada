'use strict';

app.controller('FiscalizeNotasFiscaisController', ["$scope", "$api",
    function ($scope, $api) {
    	$loading.show();

    	$api.get('Fiscalize').then(function (response) {
    		$loading.hide();
    		$scope.notas = JSON.parse(response.data);
    	});
    }]);
