var varsao = 1;
var base_url = 'http://ops.net.br';

var $EditError = function (ex) {
	var msg = ex.ExceptionMessage || ex.Message || ex.d || ex;
	if (msg)
		alert('Erro:' + msg);
};

var OPS = function ($) {
	var select = function (selector, apiUrl, defaultValues) {
		var $select = $(selector);

		var bindSelect2 = function () {
			$select.select2({
				ajax: {
					url: apiUrl,
					dataType: 'json',
					delay: 250,
					data: function (params) {
						return {
							q: params.term, // search term
							page: params.page
						};
					},
					processResults: function (data, params) {
						// parse the results into the format expected by Select2
						// since we are using custom formatting functions we do not need to
						// alter the remote JSON data, except to indicate that infinite
						// scrolling can be used
						params.page = params.page || 1;

						return {
							results: data.results,
							pagination: {
								more: (params.page * 30) < data.total_count
							}
						};
					},
					cache: true
				},
				allowClear: true,
				placeholder: "Todos"
			});
		}

		if (defaultValues) {
			$.ajax({
				dataType: "json",
				url: apiUrl,
				data: { 'qs': defaultValues },
				success: function (data) {
					var lstSelected = defaultValues.split(',');
					var $lstOption = [];
					for (var i = 0; i < data.results.length; i++) {
						var item = data.results[i];
						$lstOption.push($('<option></option>').val(item.id).text(item.text).attr('selected', lstSelected.indexOf(item.id.toString()) !== -1));
					}
					$select.append($lstOption);

					bindSelect2();
				}
			});
		} else {
			bindSelect2();
		}
	}

	var objectToQueryString = function (obj, s) {
		s = s || [];
		var r20 = /%20/g
		var lstkeys = Object.keys(obj);

		for (var i = 0; i < lstkeys.length; i++) {
			if (obj[lstkeys[i]]) {
				s.push(lstkeys[i] + '=' + obj[lstkeys[i]]);
			}
		}

		return s.join("&").replace(r20, "+");
	}

	return {
		select: select,
		objectToQueryString: objectToQueryString
	};
}(jQuery);

var app;
(function (angular, $) {
	'use strict';

	app = angular.module('app', ['ngRoute', 'ngTable', 'ngResource', 'ngSanitize', 'ngCookies', 'angularUtils.directives.dirDisqus']);

	app.config(['$provide', '$routeProvider', '$httpProvider', '$interpolateProvider', '$compileProvider', '$locationProvider',
		function ($provide, $routeProvider, $httpProvider, $interpolateProvider, $compileProvider, $locationProvider) {

			$compileProvider.debugInfoEnabled(false);

			$routeProvider
				.when("/", { templateUrl: "app/inicio" })
				.when("/sobre", { templateUrl: "app/sobre" })
				.when("/deputado-federal/documento/:id", { templateUrl: "app/auditoria/deputado-federal-documento" })
				.when("/deputado-federal/:id/secretario", { templateUrl: "app/auditoria/deputado-federal-secretario-detalhes" })
				.when("/deputado-federal/secretario", { templateUrl: "app/auditoria/deputado-federal-secretario-lista" })
				.when("/deputado-federal/:id", { templateUrl: "app/auditoria/deputado-federal-detalhes" })
				.when("/deputado-federal", { templateUrl: "app/auditoria/deputado-federal-lista" })

				.when("/senador/:id", { templateUrl: "app/auditoria/senador-detalhes" })
				.when("/senador", { templateUrl: "app/auditoria/senador-lista" })

				.when("/fornecedor/:id", { templateUrl: "app/auditoria/fornecedor" })

				.when("/fiscalize", { templateUrl: "app/fiscalize/nota-fiscal-lista" })
				.when("/fiscalize/:id", { templateUrl: "app/fiscalize/nota-fiscal-detalhes" })

				.otherwise({ redirectTo: '/' });

			$locationProvider.html5Mode(true);
		}]);

	app.run(['$rootScope', '$http', '$location', '$templateCache', '$route',
		function ($rootScope, $http, $location, $templateCache, $route) {
			$http.get('app/inicio', { cache: $templateCache });

			// https://www.consolelog.io/angularjs-change-path-without-reloading
			var original = $location.path;
			$location.path = function (path, reload) {
				if (reload === false) {
					var lastRoute = $route.current;
					var un = $rootScope.$on('$locationChangeSuccess', function () {
						$route.current = lastRoute;
						un();
					});
				}
				return original.apply($location, [path]);
			};

			$rootScope.$on('$locationChangeSuccess', function (event) {
				setTimeout(function () {
					ga('send', 'pageview', { 'page': $location.path() });
				}, 1000);
			});
		}]);

	app.factory('$api', ['$http', '$rootScope', '$cacheFactory', '$q', function ($http, $rootScope, $cacheFactory, $q) {
		//URL's que estão salvas em cache
		var urlCache = [];

		//http://stackoverflow.com/questions/28669537/angularjs-abort-cancel-running-http-calls
		var promiseCanceller = $q.defer();

		if (!$rootScope.countRequest)
			$rootScope.countRequest = 0;

		var _$http = function (method, url, params, data, cache) {
			$rootScope.countRequest++;
			var urlCompleta = ("./api/" + url).toLowerCase();

			return $http({
				method: method,
				url: urlCompleta,
				params: params, //querystring
				data: data,
				cache: cache === true
			}).success(function () {
				$rootScope.countRequest--;

				if (cache && urlCache.indexOf(urlCompleta) == -1) {
					urlCache.push(urlCompleta);

					//console.log('Add Cache: ' + urlCompleta);
				}
			}).error(function (data, status, headers, config) {
				$rootScope.countRequest--;

				var $httpDefaultCache = $cacheFactory.get('$http');
				$httpDefaultCache.remove(urlCompleta);

				$EditError(data);
			});
		}

		return {
			getByFilter: function (controller, action, filtrosPesquisa, cache) {
				return _$http('POST', controller + "/" + action, null, filtrosPesquisa, cache);
			},
			get: function (controller, id, cache) {
				return _$http('GET', controller + (id || id === 0 ? "/" + id : ''), null, null, cache);
			},
			post: function (controller, model) {
				return _$http('POST', controller, null, model, false);
			},
			delete: function (controller, id) {
				return _$http('DELETE', controller + "/" + id, null, null, false);
			},
			put: function (controller, id, model) {
				return _$http('PUT', controller + "/" + id, null, model, false);
			},
			autocomplete: function (controller, term) {
				promiseCanceller.resolve('request cancelled');
				promiseCanceller = $q.defer();

				return $http.get('./api/' + controller + '/AutoComplete?value=' + term,
						  {
						  	timeout: promiseCanceller.promise
						  }).then(function (response) {
						  	return response.data;
						  });
			},
			data: function (action) {
				return $http.post('./api/Data/' + action).then(function (response) {
					return response.data;
				});
			},
			upload: function (url, obj) {
				$rootScope.countRequest++;
				return $http.post(url, obj, {
					transformRequest: angular.identity,
					headers: { 'Content-Type': undefined }
				}).success(function () {
					$rootScope.countRequest--;
				}).error(function () {
					$rootScope.countRequest--;
				});
			},
			clearCache: function (controller) {
				var $httpDefaultCache = $cacheFactory.get('$http');
				//console.log('Cache: ' + controller);

				if (controller) {
					for (var i = urlCache.length - 1; i >= 0; i--) {
						try {
							if (urlCache[i].split('/')[3] == controller) {
								//console.log('Remove Cache: ' + urlCache[i]);

								$httpDefaultCache.remove(urlCache[i]);
								urlCache.splice(i, 1);
							}
						} catch (e) {
							console.log((e.message || e) + ' Cache' + urlCache[i])
						}
					}
				} else {
					//console.log('Remove Cache: ALL');

					$httpDefaultCache.removeAll();
					urlCache = [];
				}
			}
		};
	}]);

	app.factory('$tabela', ["$rootScope", "$resource", "NgTableParams", "$location", "$route", "$timeout",
		function ($rootScope, $resource, NgTableParams, $location, $route, $timeout) {
			//Controle da pagina atual
			var location_path;
			var params = { page: 1, count: 100 };

			return {
				params: params,
				databind: function (url, filter) {
					var _$resource = $resource('./api/' + url, filter || {}, {
						query: {
							method: "GET"
						}
					});

					return new NgTableParams(params, {
						counts: false,
						filterDelay: 300,
						getData: function (params) {
							if (!$rootScope.countRequest)
								$rootScope.countRequest = 0;

							$rootScope.countRequest++;

							var paramsSorting = params.sorting();
							var sorting = '';
							if (Object.keys(paramsSorting).length > 0) {
								var key = Object.keys(paramsSorting)[0];
								sorting = key + ' ' + paramsSorting[key];
							}


							var ngTableFilter = {};
							if (Object.keys(params.filter()).length > 0) {
								angular.copy(params.filter(), ngTableFilter); //DeepCopy
							}

							ngTableFilter['sorting'] = sorting;
							ngTableFilter['count'] = params.count();
							ngTableFilter['page'] = params.page();

							return _$resource.query(ngTableFilter)
							.$promise
							.then(function (data) {

								if (location_path == window.location.pathname) { //page_load??
									//salvar a pesquisa atual na URL, para histórico e compartlhamento.
									filter['sorting'] = sorting || undefined;
									//filter['count'] = params.count();
									if (params.page() > 1) {
										filter['page'] = params.page();
									}

									$location.path($location.path(), false).search(filter);

									// Esperar carregar o grid e levar para o grid, util quando paginando e filtrando.
									setTimeout(function () {
										$('body').animate({ scrollTop: $('.scroll-mark').offset().top }, 600);
									}, 50);
								}

								if (data.valor_total) {
									setTimeout(LoadPopoverAuditoria, 100);
									$rootScope.ValorTotal = data.valor_total;
								}

								location_path = window.location.pathname;
								$rootScope.countRequest--;
								params.total(data.total_count);
								return data.results;
							})
							.catch(function (response) {
								$rootScope.countRequest--;
								alert(response.data.ExceptionMessage);
							});
						}
					});
				}
			};
		}]);

	app.factory("$queryString", ["$window", "$location", function ($window, $location) {
		function search() {

			var left = $window.location.search
				.split(/[&||?]/)
				.filter(function (x) { return x.indexOf("=") > -1; })
				.map(function (x) { return x.split(/=/); })
				.map(function (x) {
					x[1] = x[1].replace(/\+/g, " ");
					return x;
				})
				.reduce(function (acc, current) {
					acc[current[0]] = current[1];
					return acc;
				}, {});

			var right = $location.search() || {};

			var leftAndRight = Object.keys(right)
				.reduce(function (acc, current) {
					acc[current] = right[current];
					return acc;
				}, left);

			return leftAndRight;
		}

		return {
			search: search
		};

	}]);

}(window.angular, window.jQuery));


/* AuditoriaPF.aspx */
function loadAuditoriaPF() {
	$("#dialog-message").dialog({
		modal: true,
		autoOpen: false,
		height: 500,
		width: 800,
		buttons: {
			Ok: function () {
				$(this).dialog("close");
			}
		}
	});
};

function ExibeOqueProcurar() {
	$("#dialog-message").dialog("open");
}

function Denunciar() {
	window.parent.TabDenuncia($('#LabelCNPJ').text(), $('#LabelRazaoSocial').text());
}

function Doacoes() {
	window.parent.TabDoacoes($('#LabelCNPJ').text(), $('#LabelRazaoSocial').text());
}

function LoadPopoverAuditoria() {
	$('.popover-link').each(function () {
		$(this).popover({
			html: true,
			trigger: 'manual',
			content: function () {
				return $('#popover_content_wrapper').html();
			}
		}).click(function (e) {
			e.preventDefault();
			$('.popover').popover('hide');

			$(this).popover('toggle');
			$('.popover .btn').click(function (e) {
				var agrupamento = parseInt($(this).data('valor'));
				if (agrupamento !== 0) {

					var popover_content_id = $(this).closest('.popover').prop('id');
					var $buttonDetalhar = $('a.popover-link[aria-describedby="' + popover_content_id + '"]');

					var valor = $buttonDetalhar.data('valor');
					var descricao = $buttonDetalhar.data('descricao');

					var $select;
					switch (parseInt($('#lstAgrupamento').val())) {
						case 1:
							$select = $('#lstParlamentar');
							break;
						case 2:
							$select = $('#lstDespesa');
							break;
						case 3:
							$select = $('#lstFornecedor');
							break;
						case 4:
							$select = $('#lstPartido');
							break;
						case 5:
							$select = $('#lstUF');
							break;
						case 6:
							$select = $('#txtDocumento');
							break;
					}
					var $selectItens = $('#lstDespesa option[value="' + valor + '"]');
					if ($selectItens.length > 0) {
						$selectItens.attr('selected', 'selected');
					} else {
						var $option = $('<option selected></option>').val(valor).text(descricao);
						$select.append($option);
					}
					$select.trigger('change');

					$('#lstAgrupamento').val(agrupamento);

					angular.element('#ButtonPesquisar').trigger('click'); //call ngClick
				}

				$(this).parents('.popover').popover('hide');
			});
		});
	});
}

var _urq = _urq || [];
function loadSiteMaster() {
	setTimeout(function () {
		_urq.push(['setGACode', 'UA-38537890-5']);
		_urq.push(['setPerformInitialShorctutAnimation', false]);
		_urq.push(['initSite', '9cf4c59a-d438-48b0-aa5e-e16f549b9c8c']);

		(function () {
			var ur = document.createElement('script'); ur.type = 'text/javascript'; ur.async = true;
			ur.src = ('https:' == document.location.protocol ? 'https://cdn.userreport.com/userreport.js' : 'http://cdn.userreport.com/userreport.js');
			var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ur, s);
		})();

		var interval = setInterval(function () {
			if ($('#crowd-shortcut').length == 1) {
				clearInterval(interval);

				$('#crowd-shortcut').parent().css('top', '54px');
			}
		}, 100);

		$('#btnReportarErro').click(function () {
			if ($('#crowd-shortcut').length > 0) {
				_urq.push(['Feedback_Open', 'submit/bug']);
			} else {
				window.open('https://feedback.userreport.com/9cf4c59a-d438-48b0-aa5e-e16f549b9c8c/#submit/bug')
			}
		});

		/* Facebook */
		(function (d, s, id) {
			var js, fjs = d.getElementsByTagName(s)[0];
			if (d.getElementById(id)) return;
			js = d.createElement(s); js.id = id;
			js.src = "//connect.facebook.net/en_US/sdk.js#xfbml=1&version=v2.5&appId=1033624573364106";
			fjs.parentNode.insertBefore(js, fjs);
		}(document, 'script', 'facebook-jssdk'));
	}, 1000);
}

// https://github.com/felipefdl/cidades-estados-brasil-json/blob/master/Estados.json
var lstEstadosBrasileiros = [
	{ "$id": "1", "id": "AC", "text": "Acre" },
	{ "$id": "2", "id": "AL", "text": "Alagoas" },
	{ "$id": "3", "id": "AM", "text": "Amazonas" },
	{ "$id": "4", "id": "AP", "text": "Amapá" },
	{ "$id": "5", "id": "BA", "text": "Bahia" },
	{ "$id": "6", "id": "CE", "text": "Ceará" },
	{ "$id": "7", "id": "DF", "text": "Distrito Federal" },
	{ "$id": "8", "id": "ES", "text": "Espírito Santo" },
	{ "$id": "9", "id": "GO", "text": "Goiás" },
	{ "$id": "10", "id": "MA", "text": "Maranhão" },
	{ "$id": "11", "id": "MG", "text": "Minas Gerais" },
	{ "$id": "12", "id": "MS", "text": "Mato Grosso do Sul" },
	{ "$id": "13", "id": "MT", "text": "Mato Grosso" },
	{ "$id": "14", "id": "PA", "text": "Pará" },
	{ "$id": "15", "id": "PB", "text": "Paraíba" },
	{ "$id": "16", "id": "PE", "text": "Pernambuco" },
	{ "$id": "17", "id": "PI", "text": "Piauí" },
	{ "$id": "18", "id": "PR", "text": "Paraná" },
	{ "$id": "19", "id": "RJ", "text": "Rio de Janeiro" },
	{ "$id": "20", "id": "RN", "text": "Rio Grande do Norte" },
	{ "$id": "21", "id": "RO", "text": "Rondônia" },
	{ "$id": "22", "id": "RR", "text": "Roraima" },
	{ "$id": "23", "id": "RS", "text": "Rio Grande do Sul" },
	{ "$id": "24", "id": "SC", "text": "Santa Catarina" },
	{ "$id": "25", "id": "SE", "text": "Sergipe" },
	{ "$id": "26", "id": "SP", "text": "São Paulo" },
	{ "$id": "27", "id": "TO", "text": "Tocantins" }
];

// http://www.tse.jus.br/partidos/partidos-politicos/registrados-no-tse
var lstPartidosBrasileiros = [
	{ "$id": "1", "id": "PMDB", "text": "PARTIDO DO MOVIMENTO DEMOCRÁTICO BRASILEIRO" },
	{ "$id": "2", "id": "PTB", "text": "PARTIDO TRABALHISTA BRASILEIRO" },
	{ "$id": "3", "id": "PDT", "text": "PARTIDO DEMOCRÁTICO TRABALHISTA" },
	{ "$id": "4", "id": "PT", "text": "PARTIDO DOS TRABALHADORES" },
	{ "$id": "5", "id": "DEM", "text": "DEMOCRATAS" },
	{ "$id": "6", "id": "PCdoB", "text": "PARTIDO COMUNISTA DO BRASIL" },
	{ "$id": "7", "id": "PSB", "text": "PARTIDO SOCIALISTA BRASILEIRO" },
	{ "$id": "8", "id": "PSDB", "text": "PARTIDO DA SOCIAL DEMOCRACIA BRASILEIRA" },
	{ "$id": "9", "id": "PTC", "text": "PARTIDO TRABALHISTA CRISTÃO" },
	{ "$id": "10", "id": "PSC", "text": "PARTIDO SOCIAL CRISTÃO" },
	{ "$id": "11", "id": "PMN", "text": "PARTIDO DA MOBILIZAÇÃO NACIONAL" },
	{ "$id": "12", "id": "PRP", "text": "PARTIDO REPUBLICANO PROGRESSISTA" },
	{ "$id": "13", "id": "PPS", "text": "PARTIDO POPULAR SOCIALISTA" },
	{ "$id": "14", "id": "PV", "text": "PARTIDO VERDE" },
	{ "$id": "15", "id": "PTdoB", "text": "PARTIDO TRABALHISTA DO BRASIL" },
	{ "$id": "16", "id": "PP", "text": "PARTIDO PROGRESSISTA" },
	{ "$id": "17", "id": "PSTU", "text": "PARTIDO SOCIALISTA DOS TRABALHADORES UNIFICADO" },
	{ "$id": "18", "id": "PCB", "text": "PARTIDO COMUNISTA BRASILEIRO" },
	{ "$id": "19", "id": "PRTB", "text": "PARTIDO RENOVADOR TRABALHISTA BRASILEIRO" },
	{ "$id": "20", "id": "PHS", "text": "PARTIDO HUMANISTA DA SOLIDARIEDADE" },
	{ "$id": "21", "id": "PSDC", "text": "PARTIDO SOCIAL DEMOCRATA CRISTÃO" },
	{ "$id": "22", "id": "PCO", "text": "PARTIDO DA CAUSA OPERÁRIA" },
	{ "$id": "23", "id": "PTN", "text": "PARTIDO TRABALHISTA NACIONAL" },
	{ "$id": "24", "id": "PSL", "text": "PARTIDO SOCIAL LIBERAL" },
	{ "$id": "25", "id": "PRB", "text": "PARTIDO REPUBLICANO BRASILEIRO" },
	{ "$id": "26", "id": "PSOL", "text": "PARTIDO SOCIALISMO E LIBERDADE" },
	{ "$id": "27", "id": "PR", "text": "PARTIDO DA REPÚBLICA" },
	{ "$id": "28", "id": "PSD", "text": "PARTIDO SOCIAL DEMOCRÁTICO" },
	{ "$id": "29", "id": "PPL", "text": "PARTIDO PÁTRIA LIVRE" },
	{ "$id": "30", "id": "PEN", "text": "PARTIDO ECOLÓGICO NACIONAL" },
	{ "$id": "31", "id": "PROS", "text": "PARTIDO REPUBLICANO DA ORDEM SOCIAL" },
	{ "$id": "32", "id": "SD", "text": "SOLIDARIEDADE" },
	{ "$id": "33", "id": "NOVO", "text": "PARTIDO NOVO" },
	{ "$id": "34", "id": "REDE", "text": "REDE SUSTENTABILIDADE" },
	{ "$id": "35", "id": "PMB", "text": "PARTIDO DA MULHER BRASILEIRA" }
];

/* Google Analytics */
try {
	(function (i, s, o, g, r, a, m) {
		i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {
			(i[r].q = i[r].q || []).push(arguments)
		}, i[r].l = 1 * new Date(); a = s.createElement(o),
        m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
	})(window, document, 'script', '//www.google-analytics.com/analytics.js', 'ga');

	ga('create', 'UA-38537890-4', 'auto');
	//ga('send', 'pageview');
} catch (e) { }

/**
* Função que acompanha um clique em um link externo no Analytics.
* Essa função processa uma string de URL válida como um argumento e usa essa string de URL
* como o rótulo do evento. Ao definir o método de transporte como 'beacon', o hit é enviado
* usando 'navigator.sendBeacon' em um navegador compatível.
*/
var trackOutboundLink = function (url) {
	ga('send', 'event', 'outbound', 'click', url, {
		'transport': 'beacon',
		'hitCallback': function () { document.location = url; }
	});
}

//<a href="http://www.example.com" onclick="trackOutboundLink('http://www.example.com'); return false;">Confira example.com</a>

// https://developers.google.com/analytics/devguides/collection/analyticsjs/user-timings
// Feature detects Navigation Timing API support.
if (window.performance) {
	// Sends the hit, passing `performance.now()` as the timing value.
	ga('send', 'timing', 'JS Dependencies', 'load', performance.now());
}