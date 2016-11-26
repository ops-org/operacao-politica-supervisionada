'use strict';

app.controller('FornecedorController', ["$scope", "$api", "$routeParams",
	function ($scope, $api, $routeParams) {
	    document.title = "OPS :: Beneficiario";

	    function init() {
	        $scope.disqusConfig = {
	            disqus_identifier: 'fornecedor-' + $routeParams.id.toString(),
	            disqus_url: base_url + '/fornecedor/' + $routeParams.id.toString()
	        };

	        setTimeout(function () {
	            $("#buscar-captcha-btn").on("click", function (e) {
	                e.preventDefault();

	                $("#captcha_img").fadeOut(1000, function () {
	                    $(this).attr('src', "");
	                    $scope.BuscarCaptcha();
	                });

	            });

	            $("#img-input").keydown(function (e) {
	                if (e.keyCode == 13) {
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

	            //$("#buscarDados-btn").on("click", function (e) {
	            //    e.preventDefault();

	            //    if ($("#img-input").val()) {
	            //        $scope.ObterDados();
	            //    } else {
	            //        alert('Digite o texto da imagem!');
	            //        $("#img-input").focus();
	            //    }
	            //});

	            //$('#ButtonDenunciar').click(function (e) {
	            //	e.preventDefault();
	            //	window.parent.TabDenuncia($("#lblCNPJ").text(), $("#lblRazaoSocial").text());
	            //});

	            //$('#ButtonListarDoacoes').click(function (e) {
	            //	e.preventDefault();
	            //	window.parent.TabDoacoes($("#lblCNPJ").text(), $("#lblRazaoSocial").text());
	            //});

	            //$('#ButtonListarDeputados').click(function (e) {
	            //	e.preventDefault();
	            //	window.parent.addTabDocumentos($("#lblCNPJ").text(), $("#lblRazaoSocial").text(), 0);
	            //});

	            //$('#ButtonListarDocumentos').click(function (e) {
	            //	e.preventDefault();
	            //	window.parent.addTabDocumentos($("#lblCNPJ").text(), $("#lblRazaoSocial").text(), 1);
	            //});

	           
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

	        Highcharts.setOptions({
	            lang: {
	                decimalPoint: ',',
	                thousandsSep: ' '
	            }
	        });

	        CarregaGraficoRecebimentosPorMesDeputados();
	        CarregaGraficoRecebimentosPorMesSenadores();

	        $api.get('Fornecedor/DeputadoFederalMaioresGastos', $routeParams.id).success(function (response) {
	            $scope.DeputadoFederalMaioresGastos = response
	        });

	        $api.get('Fornecedor/SenadoresMaioresGastos', $routeParams.id).success(function (response) {
	            $scope.SenadoresMaioresGastos = response
	        });
	    };

	    var CarregaGraficoRecebimentosPorMesDeputados = function () {
	        $api.get('Fornecedor/RecebimentosMensaisPorAnoDeputados', $routeParams.id).success(function (response) {
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
	                    categories: ['Jan', 'Feb', 'Mar', 'Abr', 'Maio', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
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
	        $api.get('Fornecedor/RecebimentosMensaisPorAnoSenadores', $routeParams.id).success(function (response) {
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
	                    categories: ['Jan', 'Feb', 'Mar', 'Abr', 'Maio', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
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

	    var $loader = $('<img class="loader-facebook" src="./Content/images/ajax-loader-facebook.gif"/> <em>Buscando ...</em>');

	    $scope.BuscarCaptcha = function () {
	        var strUrl = 'Api/Fornecedor/Captcha?value=' + $scope.fornecedor.cnpj_cpf;
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

	        if (response.fornecedor.cnpj_cpf.length == 14) {
	            $scope.fornecedor.tipo = 'pj';

	            if (!response.fornecedor.data_de_abertura) {
	                setTimeout(function () {
	                    $scope.ReconsultarDadosReceita();
	                }, 100);
	            }

	            //$api.get('Fornecedor/QuadroSocietario', $routeParams.id).success(function (response) {
	            //	$scope.fornecedorQuadroSocietario = response;
	            //});
	        } else if (response.fornecedor.cnpj_cpf.length == 11) {
	            $scope.fornecedor.tipo = 'pf';
	        } else {
	            $scope.fornecedor.tipo = 'nd'; //fornecedor interno / sem cnpj
	        }
	    }

	    init();
	}]);