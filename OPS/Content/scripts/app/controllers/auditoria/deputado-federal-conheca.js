'use strict';

app.controller('DeputadoFederalConhecaController', ["$scope", "$tabela", "$api", "$queryString",
    function ($scope, $tabela, $api, $queryString) {

        document.title = "OPS :: Deputado Federal";

        $scope.disqusConfig = {
            disqus_identifier: 'deputado-federal-conheca',
            disqus_url: base_url + '/deputado-federal/conheca'
        };

        var init = function () {
            document.title = "OPS :: Deputado Federal";
            $scope.filtro = {};

            var qs = $queryString.search();

            OPS.select("#lstUF", "./Api/Estado", qs.Uf);
            $scope.filtro.Uf = qs.Uf;

            OPS.select("#lstPartido", "./Api/Partido", qs.Partido);
            $scope.filtro.Partido = qs.Partido;

            $scope.Pesquisar(true);

            $('#lstPerido').val('9').selectpicker({
                width: '100%',
                actionsBox: true,
                liveSearch: true,
                liveSearchNormalize: true
            });

            $scope.filtro.Periodo = '9';
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
            $scope.deputado_federal = {};

            $api.post('Deputado/Lista', $scope.filtro).success(function (response) {
                $scope.deputado_federal = response;
            });
        }

        $scope.LimparFiltros = function () {
            $("#lstUF, #lstPartido").selectpicker('deselectAll');
            $("#lstPerido").val('9').selectpicker('refresh');
        }

        init();
    }]);