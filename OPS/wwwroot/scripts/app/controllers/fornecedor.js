'use strict';

app.controller('FornecedorController', ["$scope", "$api", "$routeParams", "authService",
    function ($scope, $api, $routeParams, authService) {
        document.title = "OPS :: Beneficiario";

        function init() {
            $scope.disqusConfig = {
                disqus_identifier: 'fornecedor-' + $routeParams.id.toString(),
                disqus_url: base_url + '/fornecedor/' + $routeParams.id.toString()
            };

            $scope.BuscarCaptchaReceita = function () {
                $("#captcha_img").fadeOut(1000, function () {
                    $(this).attr('src', "");
                    $scope.BuscarCaptcha();
                });
            };

            setTimeout(function () {
                $("#img-input").keydown(function (e) {
                    if (e.keyCode === 13) {
                        e.preventDefault();

                        if ($("#img-input").val()) {
                            $scope.ObterDados();
                        } else {
                            alert('Digite o texto da imagem!');
                            $("#img-input").focus();
                        }

                        return false;
                    }
                });
            }, 1000);

            $scope.PesquisarNoMaps = function (f) {
                window.open("https://www.google.com/maps/place/" + (f.logradouro + ',' + f.numero + ',' + f.cep.replace(".", "") + ',' + f.estado + ',Brasil').split(' ').join('+'));
            };

            $scope.PesquisarNoGoogle = function (f) {
                window.open("https://www.google.com.br/search?q=" + f.nome + ',' + f.cidade + ',' + f.estado);
            };

            $scope.ExpandirContrairInformacoesAdicional = function (e) {
                if ($('#collapseDadosEmpresaAdicional').is(':visible')) {
                    $(e.target).text('Ver mais');
                    $('#collapseDadosEmpresaAdicional').hide();
                } else {
                    $(e.target).text('Ver menos');
                    $('#collapseDadosEmpresaAdicional').show();
                }
            };

            CarregaGraficoRecebimentosPorMesDeputados();
            CarregaGraficoRecebimentosPorMesSenadores();

            $api.get('Fornecedor/' + $routeParams.id + '/DeputadoFederalMaioresGastos').success(function (response) {
                $scope.DeputadoFederalMaioresGastos = response
            });

            $api.get('Fornecedor/' + $routeParams.id + '/SenadoresMaioresGastos').success(function (response) {
                $scope.SenadoresMaioresGastos = response
            });
        };

        var CarregaGraficoRecebimentosPorMesDeputados = function () {
            $api.get('Fornecedor/' + $routeParams.id.toString() + '/RecebimentosMensaisPorAnoDeputados').success(function (response) {
                if (!response) return;

                var chart = new Highcharts.Chart({
                    chart: {
                        renderTo: 'fornecedor-recebimentos-por-mes-deputados',
                        type: 'bar'
                    },

                    title: {
                        text: null
                    },

                    xAxis: {
                        categories: ['Jan', 'Fev', 'Mar', 'Abr', 'Maio', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
                    },

                    yAxis: [{
                        tickAmount: 10,
                        title: {
                            text: "Valor (em reais)"
                        },
                        labels: {
                            align: 'left',
                            x: 3,
                            y: 16,
                            format: '{value:.,0f}'
                        },
                        showFirstLabel: false
                    }],

                    tooltip: {
                        shared: true,
                        crosshairs: true,
                        pointFormat: '<span style=color:{point.color}">\u25CF</span> {series.name}: <b class="legend">{point.y:.,2f}</b><br/>'
                    },

                    plotOptions: {
                        series: {
                            stacking: 'normal'
                        }
                    },

                    legend: {
                        layout: 'vertical',
                        align: 'right',
                        verticalAlign: 'top',
                        floating: true,
                        borderWidth: 1,
                        backgroundColor: ((Highcharts.theme && Highcharts.theme.legendBackgroundColor) || '#FFFFFF'),
                        shadow: true
                    },

                    series: response
                });
            });
        }

        var CarregaGraficoRecebimentosPorMesSenadores = function () {
            $api.get('Fornecedor/' + $routeParams.id.toString() + '/RecebimentosMensaisPorAnoSenadores').success(function (response) {
                if (!response) return;

                var chart = new Highcharts.Chart({
                    chart: {
                        renderTo: 'fornecedor-recebimentos-por-mes-senadores',
                        type: 'bar'
                    },

                    title: {
                        text: null
                    },

                    xAxis: {
                        categories: ['Jan', 'Fev', 'Mar', 'Abr', 'Maio', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
                    },

                    yAxis: [{
                        tickAmount: 10,
                        title: {
                            text: "Valor (em reais)"
                        },
                        labels: {
                            align: 'left',
                            x: 3,
                            y: 16,
                            format: '{value:.,0f}'
                        },
                        showFirstLabel: false
                    }],

                    tooltip: {
                        shared: true,
                        crosshairs: true,
                        pointFormat: '<span style=color:{point.color}">\u25CF</span> {series.name}: <b class="legend">{point.y:.,2f}</b><br/>'
                    },

                    plotOptions: {
                        series: {
                            stacking: 'normal'
                        }
                    },

                    legend: {
                        layout: 'vertical',
                        align: 'right',
                        verticalAlign: 'top',
                        floating: true,
                        borderWidth: 1,
                        backgroundColor: ((Highcharts.theme && Highcharts.theme.legendBackgroundColor) || '#FFFFFF'),
                        shadow: true
                    },

                    series: response
                });
            });
        }

        $scope.ReconsultarDadosReceita = function () {
            $scope.BuscarCaptcha();

            $("#fsConsultaReceita").show();
        };

        var $loader = $('<img class="loader-facebook" src="./images/ajax-loader-facebook.gif"/> <em>Buscando ...</em>');

        $scope.BuscarCaptcha = function () {
            var strUrl = 'Api/Fornecedor/Captcha/' + $scope.fornecedor.cnpj_cpf;
            $.ajax({
                type: 'GET',
                url: strUrl,
                contentType: "application/json; charset=utf-8",
                beforeSend: function () {
                    $loader.insertAfter($("#captcha_img"));
                },
                success: function (data) {
                    $("#captcha_img").removeClass("hidden").attr('src', data).fadeIn(1000);
                    $('#img-input').val('').focus();
                },
                complete: function () {
                    $loader.remove();
                    $("#img-input").focus();
                },
                error: function (err) {
                    alert("erro na tentativa de obter o captcha");
                }
            });
        };

        $scope.ConsultarCNPJ = function () {
            if ($("#img-input").val()) {
                $scope.ObterDados();
            } else {
                alert('Digite o texto da imagem!');
                $("#img-input").focus();
            }
        }

        $scope.ObterDados = function () {
            $api.post('Fornecedor/ConsultarDadosCnpj', { "cnpj": $scope.fornecedor.cnpj_cpf, "captcha": $("#img-input").val() }).success(function (response) {
                AtualizarTipoFornecedor(response);

                $("#buscar-modal").modal("hide");

                $("#fsConsultaReceita, #dvInfoDataConsultaCNPJ").hide();
                $("#fsDadosReceita, #fsQuadroSocietario, #dvBotoesAcao").show();
            }).error(function (data) {
                //$loading.hide();
                if (data.responseJSON && data.responseJSON.ExceptionMessage) {
                    $("#msgErro-span").text(data.responseJSON.ExceptionMessage).closest("p").removeClass("hidden");
                    $scope.BuscarCaptcha();
                } else {
                    $("#msgErro-span").text("Erro de comunicação.").closest("p").removeClass("hidden");
                }

                setTimeout(function () {
                    $("#msgErro-span").closest("p").addClass("hidden");
                }, 5000);
            });
        };

        $api.get('Fornecedor', $routeParams.id).success(AtualizarTipoFornecedor);

        function AtualizarTipoFornecedor(response) {
            $scope.fornecedor = response.fornecedor;
            $scope.quadro_societario = response.quadro_societario;

            document.title = "OPS :: Beneficiario - " + response.fornecedor.nome;

            if (response.fornecedor.cnpj_cpf.length === 14) {
                $scope.fornecedor.genero = 'pj';

                if (authService.authentication && authService.authentication.isAuth) {

                    setTimeout(function () {
                        $('#btnReconsultarDadosReceita').show();

                        if (!response.fornecedor.data_de_abertura) {
                            $scope.ReconsultarDadosReceita();
                        }
                    }, 100);
                }
            } else if (response.fornecedor.cnpj_cpf.length === 11) {
                $scope.fornecedor.genero = 'pf';
            } else {
                $scope.fornecedor.genero = 'nd'; //fornecedor interno / sem cnpj
            }
        }

        init();
    }]);