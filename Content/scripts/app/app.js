var varsao = 1;

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
				placeholder: "Selecione",
				selectOnClose: false
				//escapeMarkup: function (markup) { return markup; }, // let our custom formatter work
				////minimumInputLength: 1,
				//templateResult: formatRepo, // omitted for brevity, see the source of this page
				//templateSelection: formatRepoSelection // omitted for brevity, see the source of this page
			});
		}

		if (defaultValues) {
			$.ajax({
				dataType: "json",
				url: apiUrl,
				data: { 'qs': defaultValues },
				success: function (data) {
					var $lstOption = [];
					for (var i = 0; i < data.results.length; i++) {
						var item = data.results[i];
						$lstOption.push($('<option selected></option>').val(item.id).text(item.text));
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



var app = angular.module('app', ['ngRoute', 'ui.bootstrap', 'ngTable', 'ngResource', 'ngSanitize', 'ngCookies']);

app.config(['$provide', '$routeProvider', '$httpProvider', '$interpolateProvider', '$compileProvider',
	function ($provide, $routeProvider, $httpProvider, $interpolateProvider, $compileProvider) {

		$compileProvider.debugInfoEnabled(false);

		//================================================
		// Ignore Template Request errors if a page that was requested was not found or unauthorized.  The GET operation could still show up in the browser debugger, but it shouldn't show a $compile:tpload error.
		//================================================
		$provide.decorator('$templateRequest', ['$delegate', function ($delegate) {
			var mySilentProvider = function (tpl, ignoreRequestError) {
				return $delegate(tpl, true);
			}
			return mySilentProvider;
		}]);

		//================================================
		// Add an interceptor for AJAX errors
		//================================================
		$httpProvider.interceptors.push(['$q', '$location', function ($q, $location) {
			return {
				'responseError': function (response) {
					if (response.status === 401)
						$location.url('/login/entrar');
					return $q.reject(response);
				}
			};
		}]);


		//$httpProvider.interceptors.push('$httpRequestInterceptor');

		$routeProvider
			.when("/inicio", { templateUrl: "app/inicio" })
			.when("/sobre", { templateUrl: "app/sobre" })
			.when("/deputado-federal/documento/:id", { templateUrl: "app/auditoria/deputado-federal-documento" })
			.when("/deputado-federal/secretario/:id", { templateUrl: "app/auditoria/deputado-federal-secretario-detalhes" })
			.when("/deputado-federal/secretario", { templateUrl: "app/auditoria/deputado-federal-secretario-lista" })
			.when("/deputado-federal/:id", { templateUrl: "app/auditoria/deputado-federal-detalhes" })
			.when("/deputado-federal", { templateUrl: "app/auditoria/deputado-federal-lista", reloadOnSearch: false })

			.when("/senador/:id", { templateUrl: "app/auditoria/senador-detalhes" })
			.when("/senador", { templateUrl: "app/auditoria/senador-lista", reloadOnSearch: false })

			.when("/fornecedor/:id", { templateUrl: "app/auditoria/fornecedor" })

			.when("/fiscalize", { templateUrl: "app/fiscalize/nota-fiscal-lista" })
			.when("/fiscalize/:id", { templateUrl: "app/fiscalize/nota-fiscal-detalhes" })

			.otherwise({ redirectTo: '/inicio' });
	}]);

app.run(['$http', '$cookies', '$cookieStore', function ($http, $cookies, $cookieStore) {
	//If a token exists in the cookie, load it after the app is loaded, so that the application can maintain the authenticated state.
	$http.defaults.headers.common.Authorization = 'Bearer ' + $cookieStore.get('_Token');
	$http.defaults.headers.common.RefreshToken = $cookieStore.get('_RefreshToken');
}]);

//GLOBAL FUNCTIONS - pretty much a root/global controller.
//Get username on each page
//Get updated token on page change.
//Logout available on each page.
app.run(['$rootScope', '$http', '$cookies', '$cookieStore', function ($rootScope, $http, $cookies, $cookieStore) {

	$rootScope.logout = function () {

		$http.post('/api/Account/Logout')
            .success(function (data, status, headers, config) {
            	$http.defaults.headers.common.Authorization = null;
            	$http.defaults.headers.common.RefreshToken = null;
            	$cookieStore.remove('_Token');
            	$cookieStore.remove('_RefreshToken');
            	$rootScope.username = '';
            	$rootScope.loggedIn = false;
            	window.location = '#/signin';
            });

	}

	$rootScope.$on('$locationChangeSuccess', function (event) {
		if ($http.defaults.headers.common.RefreshToken != null) {
			var params = "grant_type=refresh_token&refresh_token=" + $http.defaults.headers.common.RefreshToken;
			$http({
				url: '/Token',
				method: "POST",
				headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
				data: params
			})
            .success(function (data, status, headers, config) {
            	$http.defaults.headers.common.Authorization = "Bearer " + data.access_token;
            	$http.defaults.headers.common.RefreshToken = data.refresh_token;

            	$cookieStore.put('_Token', data.access_token);
            	$cookieStore.put('_RefreshToken', data.refresh_token);

            	$http.get('/api/WS_Account/GetCurrentUserName')
                    .success(function (data, status, headers, config) {
                    	if (data != "null") {
                    		$rootScope.username = data.replace(/["']{1}/gi, "");//Remove any quotes from the username before pushing it out.
                    		$rootScope.loggedIn = true;
                    	}
                    	else
                    		$rootScope.loggedIn = false;
                    });


            })
            .error(function (data, status, headers, config) {
            	$rootScope.loggedIn = false;
            });
		}
	});
}]);

//app.factory('$httpRequestInterceptor', ['$q', '$location', function ($q, $location) {
//	return {
//		'responseError': function (rejection) {
//			// do something on error
//			if (rejection.status === 404) { //pagina não Encontrada
//				$location.path('/error');
//				//} else if (rejection.status === 401) { //Sem Permissão
//				//    if (rejection.config.method == "GET") {
//				//        $location.path('/401');
//				//    } else {
//				//        //$window.history.back();
//				//    }
//			} else if (rejection.status === 403) {//Não Autenticado
//				window.location.href = "Login";
//			}
//			return $q.reject(rejection);
//		}
//	};
//}]);

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

app.factory('$box', ['$rootScope', '$modal', 'toastr', function ($rootScope, $modal, toastr) {

	//https://github.com/Foxandxss/angular-toastr
	var _toastr = function (title, message, type) {
		return toastr[type](message, title);
	};

	//http://t4t5.github.io/sweetalert/
	var _swal = function (title, message, type) {
		window.swal(title, message, type);
	}

	return {
		error: function (title, message) {
			_swal(title, message, 'error');
		},
		info: function (title, message) {
			_swal(title, message, 'info');
		},
		success: function (title, message) {
			_toastr(title, message, 'success');
		},
		warning: function (title, message) {
			_toastr(title, message, 'warning');
		},
		confirm: function (title, text, callback) {
			$rootScope.$evalAsync(function () {
				swal({
					title: title,
					text: text,
					type: "warning",
					showCancelButton: true,
					confirmButtonColor: "#DD6B55",
					confirmButtonText: "Sim",
					cancelButtonText: "Não",
					closeOnConfirm: false
				}, function () {
					try {
						callback();
					} catch (e) {
						alert(e.Message);
					}
					swal.close();
				});
			});
		},
		warningCallBack: function (title, text, callback) {
			$rootScope.$evalAsync(function () {
				swal({
					title: title,
					text: text,
					type: "warning",
					showCancelButton: false,
					confirmButtonColor: "#DD6B55",
					confirmButtonText: "Ok",
					closeOnConfirm: false
				}, function () {
					try {
						callback();
					} catch (e) {
						alert(e.Message);
					}
					swal.close();
				});
			});
		},
		modal: function (controller, templateUrl, $modalParameters, closeCallback, cancelCallback, size) {

			var modalInstance = $modal.open({
				templateUrl: './partials/' + templateUrl,
				controller: controller,
				size: size || 'lg',
				backdrop: false,
				resolve: {
					$modalParameters: function () {
						return $modalParameters;
					}
				}
			});

			modalInstance.result.then(closeCallback, cancelCallback);

			//if (!window.ga) { return; }
			//var base = $('base').attr('href').replace(/\/$/, "");

			//window.ga('send', 'pageview', {
			//    page: base + '/' + templateUrl
			//});
		}
	};
}]);

app.factory('$tabela', ["$rootScope", "$resource", "NgTableParams", "$location",
	function ($rootScope, $resource, NgTableParams, $location) {
		var page_load = true;
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

						return _$resource.query({
							//filter: params.filter(),
							sorting: sorting,
							count: params.count(),
							page: params.page()
						})
						.$promise
						.then(function (data) {
							if (!page_load) {
								//salvar a pesquisa atual na URL, para histórico e compartlhamento.
								filter['sorting'] = sorting;
								//filter['count'] = params.count();
								filter['page'] = params.page();

								$location.search(OPS.objectToQueryString(filter));
							}

							if (data.valor_total) {
								setTimeout(LoadPopoverAuditoria, 100);

								if (!page_load) {
									// Esperar carregar o grid e levar para o grid, util quando paginando e filtrando.
									setTimeout(function () {
										$('body').animate({ scrollTop: $('#table-auditoria').offset().top }, 600);
									}, 50);
								}

								$rootScope.ValorTotal = data.valor_total;
								page_load = false;
							}

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
