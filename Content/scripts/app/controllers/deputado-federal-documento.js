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

    		var urlCamara = 'http://www.camara.gov.br/cota-parlamentar/';
    		var doc = $scope.documento;

    		$scope.documento.UrlDocumento = urlCamara + 'documentos/publ/' + doc.IdDeputado + '/' + doc.Ano + '/' + doc.IdDocumento + '.pdf'
    		$scope.documento.UrlDemaisDocumentosMes = urlCamara + 'sumarizado?nuDeputadoId=' + doc.IdDeputado + '&dataInicio=' + doc.Mes + '/' + doc.Ano + '&dataFim=' + doc.Mes + '/' + doc.Ano + '&despesa=&nomeHospede=&nomePassageiro=&nomeFornecedor=&cnpjFornecedor=&numDocumento=&sguf=&filtroNivel1=1&filtroNivel2=2&filtroNivel3=3'
    		$scope.documento.UrlDetalhesDocumento = urlCamara + 'documento?nuDeputadoId=' + doc.IdDeputado + '&numMes=' + doc.Mes + '&numAno=' + doc.Ano + '&despesa=' + doc.NumSubCota + '&cnpjFornecedor=' + doc.CnpjCpf + '&idDocumento=' + doc.Numero

    		document.title = "OPS :: Deputado Federal - NF:" + response.Id;
    	});
    }]);