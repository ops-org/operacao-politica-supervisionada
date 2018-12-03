'use strict';

app.controller('DeputadoDocumentoVisualizarController', ["$scope", "$api", "$routeParams",
    function ($scope, $api, $routeParams) {
        document.title = "OPS :: Deputado Federal - NF:" + $routeParams.id;

        //$scope.disqusConfig = {
        //	disqus_identifier: 'deputado-federal-' + $routeParams.id.toString(),
        //	disqus_url: base_url + '/deputado-federal/' + $routeParams.id.toString()
        //};

        $api.get('Deputado/Documento', $routeParams.id).success(function (doc) {
            $api.get('Deputado/DocumentosDoMesmoDia', $routeParams.id).success(function (response) {
                $scope.DocumentosDoMesmoDia = response;
            });

            $api.get('Deputado/DocumentosDaSubcotaMes', $routeParams.id).success(function (response) {
                $scope.DocumentosDaSubcotaMes = response;
            });

            $scope.documento = doc;

            var url_camara = 'http://www.camara.gov.br/cota-parlamentar/';

            if (doc.link === 2) { //NF-e
                $scope.documento.url_documento = url_camara + 'nota-fiscal-eletronica?ideDocumentoFiscal=' + doc.id_documento;
            } else /*if (doc.link == 2)*/ {
                $scope.documento.url_documento = url_camara + 'documentos/publ/' + doc.id_deputado + '/' + doc.ano + '/' + doc.id_documento + '.pdf';
            }

            if (doc.link === 0) {
                $scope.documento.url_documento_nfe = url_camara + 'nota-fiscal-eletronica?ideDocumentoFiscal=' + doc.id_documento;
            }

            $scope.documento.url_demais_documentos_mes = url_camara + 'sumarizado?nuDeputadoId=' + doc.id_deputado + '&dataInicio=' + doc.mes + '/' + doc.ano + '&dataFim=' + doc.mes + '/' + doc.ano + '&despesa=' + doc.id_cf_despesa_tipo + '&nomeHospede=&nomePassageiro=&nomeFornecedor=&cnpjFornecedor=&numDocumento=&sguf='
            $scope.documento.url_detalhes_documento = url_camara + 'documento?nuDeputadoId=' + doc.id_deputado + '&numMes=' + doc.mes + '&numAno=' + doc.ano + '&despesa=' + doc.id_cf_despesa_tipo + '&cnpjFornecedor=' + doc.cnpj_cpf + '&idDocumento=' + doc.numero_documento

            $scope.documento.url_beneficiario = './#!/fornecedor/' + doc.id_fornecedor;
            $scope.documento.url_documentos_Deputado_beneficiario = './#!/deputado-federal?IdParlamentar=' + doc.id_cf_deputado + '&Fornecedor=' + doc.id_fornecedor + '&Periodo=0&Agrupamento=6';
        });

        
    }]);