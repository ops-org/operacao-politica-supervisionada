'use strict';

app.controller('DenunciaDetalhesController', ["$scope", "$routeParams", "$api",
    function ($scope, $routeParams, $api) {

        document.title = "OPS :: Detalhes da denúncia";
        $scope.novo_comentario = { texto: null };

        var ConsultarDetalhes = function () {
            $api.get('Denuncia/' + $routeParams.id.toString()).success(function (response) {
                $scope.denuncia = response;
                $scope.novo_comentario.situacao = response.situacao;

                document.title = "OPS :: Detalhes da denúncia " + response.codigo;
            });
        }

        $scope.EnviarComentario = function () {
            $scope.novo_comentario.id_denuncia = $routeParams.id;
            if (!$scope.novo_comentario.texto) {
                alert('Informe o comentário!');
                return;
            }

            $api.post('Denuncia/AdicionarComentario', $scope.novo_comentario).success(function (response) {
                $scope.novo_comentario.texto = null;
                alert('Comentário adicioando com sucesso!');

                ConsultarDetalhes();
            });
        };

        ConsultarDetalhes();
    }]);