'use strict';

app.controller('DeputadoDocumentoVisualizarController', ["$scope", "$api", "$routeParams",
    function ($scope, $api, $routeParams) {
    	document.title = "OPS :: Deputado Federal - NF:" + $routeParams.id;

    	$scope.disqusConfig = {
    		disqus_identifier: 'deputado-federal-' + $routeParams.id.toString(),
    		disqus_url: base_url + '/deputado-federal/' + $routeParams.id.toString()
    	};

    	$api.get('Deputado/Documento', $routeParams.id).success(function (response) {
    		$scope.documento = response;

    		var url_camara = 'http://www.camara.gov.br/cota-parlamentar/';
    		var doc = $scope.documento;

    		$scope.documento.url_documento = url_camara + 'documentos/publ/' + doc.id_cf_deputado + '/' + doc.ano + '/' + doc.id_documento + '.pdf'
    		$scope.documento.url_demais_documentos_mes = url_camara + 'sumarizado?nuDeputadoId=' + doc.id_cf_deputado + '&dataInicio=' + doc.mes + '/' + doc.ano + '&dataFim=' + doc.mes + '/' + doc.ano + '&despesa=&nomeHospede=&nomePassageiro=&nomeFornecedor=&cnpjFornecedor=&numDocumento=&sguf=&filtroNivel1=1&filtroNivel2=2&filtroNivel3=3'
    		$scope.documento.url_detalhes_documento = url_camara + 'documento?nuDeputadoId=' + doc.id_cf_deputado + '&numMes=' + doc.mes + '&numAno=' + doc.ano + '&despesa=' + doc.id_cf_despesa_tipo + '&cnpjFornecedor=' + doc.cnpj_cpf + '&idDocumento=' + doc.numero_documento

    		$scope.documento.url_beneficiario = './#/fornecedor/' + doc.id_fornecedor;
    	});
    }]);