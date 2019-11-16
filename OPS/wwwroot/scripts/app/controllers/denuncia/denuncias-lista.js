'use strict';

app.controller('DenunciasListaController', ["$scope", "$tabela", "$api", "$queryString",
    function ($scope, $tabela, $api, $queryString) {

        document.title = "OPS :: Denúncias";

        $scope.filtro = {
            MensagensNaoLidas: true,
            AguardandoRevisao: true,
            PendenteInformacao: true,
            Duvidoso: true,
            Dossie: false,
            Repetido: false,
            NaoProcede: false
        };

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
            $scope.tableParams = $tabela.databind('Denuncia', $scope.filtro);
        }

        $scope.BuscaGrid();
    }]);