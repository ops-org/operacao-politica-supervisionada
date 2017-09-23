'use strict';

app.controller('DeputadoFederalFrequenciaListaController', ["$scope", "$tabela", "$api", "$queryString",
    function ($scope, $tabela, $api, $queryString) {

        document.title = "OPS :: Assiduidade nas sessões da Câmara Federal";

        var qs = $queryString.search();
        if (qs.page) {
            $tabela.params.page = parseInt(qs.page);
        }
        if (qs.sorting) {
            var lstSorting = qs.sorting.split(' ');
            $tabela.params.sorting = {};
            $tabela.params.sorting[lstSorting[0]] = lstSorting[1];
        }

        $scope.BuscaGrid = function () {
            var filtro = {};

            $scope.tableParams = $tabela.databind('Deputado/Frequencia', filtro);
        }

        $scope.BuscaGrid();
    }]);