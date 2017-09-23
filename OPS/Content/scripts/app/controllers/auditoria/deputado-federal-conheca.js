'use strict';

app.controller('DeputadoFederalConhecaController', ["$scope", "$tabela", "$api", "$queryString",
    function ($scope, $tabela, $api, $queryString) {

        document.title = "OPS :: Deputado Federal";

        $scope.disqusConfig = {
            disqus_identifier: 'deputado-federal-conheca',
            disqus_url: base_url + '/deputado-federal/conheca'
        };

        $api.get('Deputado/Lista').success(function (response) {
            $scope.deputado_federal = response;
        });
    }]);