'use strict';

app.controller('SolicitacaoRestituicaoController', ['$scope', '$api', '$timeout',
    function ($scope, $api, $timeout) {
        document.title = "OPS :: E-mailzaço";

        $scope.submit = function (form) {
            // If form is invalid, return and let AngularJS show validation errors.
            if (form.$invalid) {
                return;
            }

            // Trigger validation flag.
            $scope.submitted = true;

            // Default values for the request.
            var params = {
                'nome': $scope.nome,
                'email': $scope.email,
                'cidade': $scope.cidade,
                'estado': $scope.estado
            };

            $api.post('Contato/SolicitarRestituicao', params).then(function (data, status, headers, config) {
                $scope.nome = null;
                $scope.email = null;
                $scope.cidade = null;
                $scope.estado = null;
                $scope.messages = 'Sua mensagem foi enviada com sucesso!';
                $scope.submitted = false;

                // Hide status messages after three seconds.
                $timeout(function () {
                    $scope.messages = null;
                }, 10000);
            }, function (data, status, headers, config) {
                $scope.submitted = false;

                if (ga.q) {
                    ga('send', 'event', 'Email', 'envio', 'Beto Rosado', {
                        'transport': 'beacon'
                    });
                }
            });
        };
    }]);