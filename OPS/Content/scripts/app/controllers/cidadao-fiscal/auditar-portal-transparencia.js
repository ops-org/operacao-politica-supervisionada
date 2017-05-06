'use strict';

app.controller('AuditarPortalTransparenciaController', ['$scope', '$api', '$routeParams', '$interval', 'localStorageService',
    function ($scope, $api, $routeParams, $interval, localStorageService) {

    	document.title = "OPS :: Auditar portal de transparência";
    	$api.get('Estado').success(function (data) {
    		$scope.estados = data;
    	});

    	$api.get('CidadaoFiscal', $routeParams.id).success(function (data) {
    		$scope.auditoria = data;
    	});

    	$scope.Salvar = function () {
    		$api.post('CidadaoFiscal', $scope.auditoria).success(function (data) {
    			alert('Salvo com sucesso!');
    		});
    	};

    	$scope.GerarDenuncia = function () {
		  
    	};

    	//var autoSalvar = function () {
    	//	//$interval(function () {
    	//	//	localStorageService.set('mcf-auditoria', $scope.auditoria);
    	//	//}, 5000);
    	//};

    	//var auditoria = localStorageService.get('mcf-auditoria');
    	//if (auditoria) {
    	//	$scope.auditoria = auditoria;

    	//	autoSalvar();
    	//} else {
    	//	$api.get('CidadaoFiscal', $routeParams.id).success(function (response) {
    	//		auditoria = {};
    	//		auditoria.grupo = response;

    	//		$scope.auditoria = auditoria;

    	//		autoSalvar();
    	//	});
    	//}
    }]);