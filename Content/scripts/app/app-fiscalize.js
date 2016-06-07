var app = angular.module('ops.fiscalize', []);

app.directive('format', ['$locale', function ($locale) {
	return {
		require: '?ngModel',
		link: function ($scope, element, attribute, controller) {
			switch (attribute.format) {
				case "hora":
					$(element).mask("99:99", {
						completedBlur: function () {
							controller.$setViewValue(this.val());
						}
					}).validaHora().change(function () {
						if (this.value == '') {
							controller.$setViewValue('');
						}
					});
					break;
				case "data":
					$(element).mask("99/99/9999", {
						completedBlur: function () {
							controller.$setViewValue(this.val());
						}
					});
					break;
				case "cpf":
					$(element).mask("999.999.999-99", {
						completedBlur: function () {
							controller.$setViewValue(this.val());
						}
					});
					break;
				case "cnpj":
					$(element).mask("99.999.999/9999-99", {
						completedBlur: function () {
							controller.$setViewValue(this.val());
						}
					})
					break;
				case "cep":
					$(element).mask("99999-999", {
						completedBlur: function () {
							controller.$setViewValue(this.val());
						}
					});
					break;
				case "ddd":
					$(element).mask("99", {
						completedBlur: function () {
							controller.$setViewValue(this.val());
						}
					}).change(function () { if (this.value == '') { controller.$setViewValue(''); } });
					break;
				case "telefone":
					$(element).maskTelefoneCelular().bind("keyup", function () {
						controller.$setViewValue($(this).val().replace(/\_/g, ''));
					}).change(function () {
						if (this.value == '') {
							controller.$setViewValue('');
						}
					});
					break;
				case "money":
					//remove the default formatter from the input directive to prevent conflict
					controller.$formatters.shift();

					var decimalDelimiter = $locale.NUMBER_FORMATS.DECIMAL_SEP,
						 thousandsDelimiter = $locale.NUMBER_FORMATS.GROUP_SEP,
						 decimals = attribute.decimals || 2;

					if (!controller) {
						return;
					}

					if (angular.isDefined(attribute.uiHideGroupSep)) {
						thousandsDelimiter = '';
					}

					if (isNaN(decimals)) {
						decimals = 2;
					}
					var viewMask = numberViewMask(decimals, decimalDelimiter, thousandsDelimiter),
						 modelMask = numberModelMask(decimals);

					function parse(value) {
						if (!value) {
							return value;
						}

						var valueToFormat = clearDelimitersAndLeadingZeros(value) || '0';
						var formatedValue = viewMask.apply(valueToFormat);
						var actualNumber = parseFloat(modelMask.apply(valueToFormat));

						if (angular.isDefined(attribute.uiNegativeNumber)) {
							var isNegative = (value[0] === '-'),
								 needsToInvertSign = (value.slice(-1) === '-');

							//only apply the minus sign if it is negative or(exclusive) needs to be negative
							if (needsToInvertSign ^ isNegative) {
								actualNumber *= -1;
								formatedValue = '-' + formatedValue;
							}
						}

						if (controller.$viewValue !== formatedValue) {
							controller.$setViewValue(formatedValue);
							controller.$render();
						}

						return actualNumber;
					}

					controller.$formatters.push(function (value) {
						var prefix = '';
						if (angular.isDefined(attribute.uiNegativeNumber) && value < 0) {
							prefix = '-';
						}

						if (!value) {
							return value;
						}

						var valueToFormat = prepareNumberToFormatter(value, decimals);
						return prefix + viewMask.apply(valueToFormat);
					});

					controller.$parsers.push(parse);

					break;
			}
		}
	};
}]);

app.controller('PrincipalController', ['$scope', '$http', function ($scope, $http) {
	var init = function () {
		$loading.show();

		$http.get('http://cors.io/?u=http://104.131.229.175/fiscalize/pro/json_lista_fiscalizacao.html').then(function (response) {
			$loading.hide();
			$scope.notas = response.data;
			$scope.visao = 'lista';
		});
	}

	$scope.VisualizarNota = function ($event, nota) {
		$event.stopImmediatePropagation();
		$loading.show();

		$http.get('http://cors.io/?u=http://104.131.229.175/fiscalize/pro/json_nota_fiscal.php?notaFiscalId=' + nota.notaFiscalId).then(function (response) {
			$loading.hide();
			$scope.nota = response.data;

			$scope.visao = 'nota';
		});
	}

	$scope.VoltarLista = function ($event) {
		$scope.visao = 'lista';
	}

	init();
}]);