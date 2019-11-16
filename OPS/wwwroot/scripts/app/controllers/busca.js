'use strict';

app.controller('BuscaController', ["$rootScope", "$scope", "$api", "$routeParams", "$location",
    function ($rootScope, $scope, $api, $routeParams, $location) {
        document.title = "OPS :: Busca";

        function init() {
            $scope.filtro = {};
            $scope.filtro.q = $routeParams.q;

            $scope.Pesquisar(true);
        };

        $scope.Pesquisar = function (page_load) {
            $rootScope.countRequest++;
            if (page_load !== true) {
                $location.search($scope.filtro);
            }

            $scope.deputado_federal = {};
            $scope.senador = {};
            $scope.fornecedor = {};

            $api.get('Indicadores/Busca?value=' + $scope.filtro.q).success(function (response) {
                $scope.deputado_federal = response.deputado_federal;
                $scope.senador = response.senador;
                $scope.fornecedor = response.fornecedor;

                $rootScope.countRequest--;
            });
        };

        init();
    }]);