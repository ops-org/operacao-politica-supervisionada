'use strict';

app.controller('DeputadoDocumentoVisualizarController', ["$scope", "$api", "$routeParams",
    function ($scope, $api, $routeParams) {
    	document.title = "OPS :: Deputado Federal - NF";

    	$scope.disqusConfig = {
    		disqus_identifier: 'deputado-federal-' + $routeParams.id.toString(),
    		disqus_url: base_url + '/deputado-federal/' + $routeParams.id.toString()
    	};

    	$api.get('Deputado/Documento', $routeParams.id).success(function (response) {
    		$scope.documento = response;

    		document.title = "OPS :: Deputado Federal - NF:" + response.Id;
    	});
    }]);