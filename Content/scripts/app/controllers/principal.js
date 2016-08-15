'use strict';

app.controller('PrincipalController', ['$scope', '$api',
    function ($scope, $api) {
    	$api.get('Indicadores/ParlamentarResumoGastos').success(function (response) {
    		var data = JSON.parse(response);
    		$scope.MaioresGastos = data.filter(function (obj) { return obj.tipoCartao == 'MAIOR' });
    		$scope.MenosresGastos = data.filter(function (obj) { return obj.tipoCartao == 'MENOR' });
    	});

    	$api.get('Indicadores/ResumoAuditoria').success(function (response) {
    		$scope.ResumoAuditoria = JSON.parse(response);
    	});
    }]);