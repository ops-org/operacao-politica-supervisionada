'use strict';

app.controller('PrincipalController', ['$scope', '$api',
    function ($scope, $api) {

    	document.title = "OPS :: Operação Politica Supervisionada";
    	$scope.disqusConfig = {
    		disqus_identifier: 'Pagina Inicial',
    	};

    	$api.get('Indicadores/ParlamentarResumoGastos').success(function (response) {
    		$scope.CampeoesGastos = response;
    	});

    	//$api.get('Indicadores/ResumoAuditoria').success(function (response) {
    	//	$scope.ResumoAuditoria = JSON.parse(response);
    	//});
    }]);