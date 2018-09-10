'use strict';

app.controller('DeputadoFederalResumoController', ["$rootScope", "$scope", "$api", "$queryString",
    function ($rootScope, $scope, $tabela, $api, $queryString) {

        var init = function () {
            document.title = "OPS :: Deputado Federal";
            $scope.filtro = {};

            var qs = $queryString.search();
            if (qs.page) {
                $tabela.params.page = parseInt(qs.page);
            }
            if (qs.sorting) {
                var lstSorting = qs.sorting.split(' ');
                $tabela.params.sorting = {};
                $tabela.params.sorting[lstSorting[0]] = lstSorting[1];
            }

            OPS.select("#lstUF", "./Api/Estado", qs.Uf);
            $scope.filtro.Uf = qs.Uf;

            OPS.select("#lstPartido", "./Api/Partido", qs.Partido);
            $scope.filtro.Partido = qs.Partido;

            $scope.Pesquisar(true);

            $('#lstPerido').selectpicker({
                width: '100%',
                actionsBox: true,
                liveSearch: true,
                liveSearchNormalize: true
            });

            $('#lblDeputadoFederalUltimaAtualizacao').text(window.DeputadoFederalUltimaAtualizacao);
        }

        $scope.Pesquisar = function (page_load) {
            if (!page_load) {
                $scope.filtro.Uf = ($("#lstUF").val() || []).join(',') || null;
                $scope.filtro.Partido = ($("#lstPartido").val() || []).join(',') || null;
                $scope.filtro.Periodo = $("#lstPerido").val();
            }
            $scope.BuscaGrid();
        };

        $scope.BuscaGrid = function () {
            $scope.tableParams = $tabela.databind('Deputado/Resumo', $scope.filtro);
        }

        $scope.LimparFiltros = function () {
            $("#lstUF, #lstPartido").selectpicker('deselectAll');
            $("#lstPerido").val('8').selectpicker('refresh');
        }

        init();
    }]);
