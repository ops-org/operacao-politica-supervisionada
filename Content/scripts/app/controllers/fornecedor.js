'use strict';

app.controller('FornecedorDetalhesController', ["$scope", "$api", "$routeParams",
	function ($scope, $api, $routeParams) {
		document.title = "OPS :: Fornecedor";

		function init() {
			$scope.disqusConfig = {
				disqus_identifier: 'fornecedor-' + $routeParams.id.toString(),
				disqus_url: base_url + '/fornecedor/' + $routeParams.id.toString()
			};

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

			$("#buscarDados-btn").on("click", function (e) {
				e.preventDefault();

				if ($("#img-input").val()) {
					$scope.ObterDados();
				} else {
					alert('Digite o texto da imagem!');
					$("#img-input").focus();
				}
			});

			$('#ButtonMaps').click(function (e) {
				e.preventDefault();
				window.open("http://maps.google.com/?q=" +
					$scope.fornecedor.Logradouro + ',' +
					$scope.fornecedor.Numero + ',' +
					$scope.fornecedor.Cep.replace(".", "") + ',' +
					$scope.fornecedor.Uf + ',Brasil');
			});

			$('#ButtonPesquisa').click(function (e) {
				e.preventDefault();
				window.open("http://www.google.com.br/search?q=" +
					$scope.fornecedor.RazaoSocial + ',' +
					$scope.fornecedor.Cidade + ',' +
					$scope.fornecedor.Uf);
			});

			$('#ButtonAtualizar').click(function (e) {
				e.preventDefault();

				$scope.ReconsultarDadosReceita();
			});

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

			$('#btCollapseDadosEmpresaAdicional').click(function () {
				if ($('#collapseDadosEmpresaAdicional').is(':visible')) {
					$(this).text('Ver mais');
					$('#collapseDadosEmpresaAdicional').hide();
				} else {
					$(this).text('Ver menos');
					$('#collapseDadosEmpresaAdicional').show();
				}

			})

			CarregaGraficoRecebimentosPorMes();

			$api.get('Fornecedor/DeputadoFederalMaioresGastos/?value=' + $routeParams.id).success(function (response) {
				$scope.DeputadoFederalMaioresGastos = response
			});

			$api.get('Fornecedor/SenadoresMaioresGastos/?value=' + $routeParams.id).success(function (response) {
				$scope.SenadoresMaioresGastos = response
			});
		};

		var CarregaGraficoRecebimentosPorMes = function () {
			$api.get('Fornecedor/RecebimentosMensaisPorAno?value=' + $routeParams.id).success(function (response) {
				Highcharts.setOptions({
					lang: {
						decimalPoint: ',',
						thousandsSep: ' '
					}
				});

				$('#fornecedor-recebimentos-por-mes').highcharts({
					chart: {
						type: 'column'
					},

					title: {
						text: null //'Recebibentos por mês pela cota parlamentar'
					},

					xAxis: {
						categories: ['Jan', 'Feb', 'Mar', 'Abr', 'Maio', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
					},

					yAxis: [{ // left y axis
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
			var strUrl = 'Api/Fornecedor/Captcha?value=' + $scope.fornecedor.CnpjCpf;
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

		$scope.ObterDados = function () {
			$api.post('Fornecedor/ConsultarDadosCnpj', { "cnpj": $scope.fornecedor.CnpjCpf, "captcha": $("#img-input").val() }).success(function (data) {
				if (data.erro.length > 0) {
					$("#msgErro-span").text(data.erro).closest("p").removeClass("hidden");
					$("#captcha_img").fadeOut(1000, function () {
						$(this).attr('src', "");
						$scope.BuscarCaptcha();
						$("#img-input").focus();
					});
					setTimeout(function () {
						$("#msgErro-span").closest("p").addClass("hidden");
					}, 2000);
				} else {
					if (data.dados != null) {
					debugger
						$scope.fornecedor = data.dados;
						$scope.fornecedorQuadroSocietario = data.dados.lstFornecedorQuadroSocietario;

						$("#buscar-modal").modal("hide");

						$("#fsConsultaReceita, #dvInfoDataConsultaCNPJ").hide();
						$("#fsDadosReceita, #fsQuadroSocietario, #dvBotoesAcao").show();

					} else {
						$("#msgErro-span").text("erro de comunicação com a receita.").closest("p").removeClass("hidden");
						$("#captcha_img").fadeOut(1000, function () {
							$(this).attr('src', "");
							$scope.BuscarCaptcha();
							$("#img-input").focus();
						});
						setTimeout(function () {
							$("#msgErro-span").closest("p").addClass("hidden");
						}, 5000);
					}

				}
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

		//TODO: Corrigir rota
		$api.get('Fornecedor/?value=' + $routeParams.id).success(function (response) {
			$scope.fornecedor = response;

			document.title = "OPS :: Fornecedor - " + response.RazaoSocial;

			if (!response.DataAbertura) {
				$scope.ReconsultarDadosReceita();
			}
		});

		//TODO: Corrigir rota
		$api.get('Fornecedor/QuadroSocietario/?value=' + $routeParams.id).success(function (response) {
			$scope.fornecedorQuadroSocietario = response;
		});

		init();
	}]);