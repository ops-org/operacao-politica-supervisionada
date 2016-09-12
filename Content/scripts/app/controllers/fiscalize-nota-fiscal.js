'use strict';

app.controller('FiscalizeNotaFiscalController', ["$scope", "$api", "$routeParams",
    function ($scope, $api, $routeParams) {

    	document.title = "OPS :: Fiscalize - Nota Fiscal";
    	$scope.disqusConfig = {
    		disqus_identifier: 'fiscalize-' + $routeParams.id.toString(),
			disqus_url: base_url + '/fiscalize/' + $routeParams.id.toString()
		};

    	$api.get('Fiscalize', $routeParams.id).then(function (response) {
    		$scope.nota = response.data;
    	});
    
}]);