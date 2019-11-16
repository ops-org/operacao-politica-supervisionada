'use strict';

app.controller('DeputadoFederalFrequenciaDetalhesController', ["$scope", "$tabela", "$api", "$queryString", "$routeParams",
    function ($scope, $tabela, $api, $queryString, $routeParams) {

        document.title = "OPS :: Assiduidade em sessão da Câmara Federal";

        $scope.BuscaGrid = function () {
            var filtro = {};

            $tabela.params.count = 1000;
            $scope.tableParams = $tabela.databind('Deputado/Frequencia/' + $routeParams.id.toString(), filtro);
        }

        $scope.BuscaGrid();
    }]);