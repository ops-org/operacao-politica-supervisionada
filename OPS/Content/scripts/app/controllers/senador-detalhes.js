'use strict';

app.controller('SenadorVisualizarController', ["$scope", "$api", "$routeParams", "NgTableParams",
    function ($scope, $api, $routeParams, NgTableParams) {
    	document.title = "OPS :: Senador";

    	$scope.disqusConfig = {
    		disqus_identifier: 'senador-' + $routeParams.id.toString(),
    		disqus_url: base_url + '/senador/' + $routeParams.id.toString()
    	};

    	$api.get('Senador/' + $routeParams.id.toString()).success(function (response) {
    		$scope.senador = response;

			document.title = "OPS :: Senador - " + response.nome_parlamentar;
    	});

    	$api.get('Senador/' + $routeParams.id.toString() + '/GastosMensaisPorAno').success(function (response) {
    		Highcharts.setOptions({
    			lang: {
    				decimalPoint: ',',
    				thousandsSep: ' '
    			}
    		});

    		$('#senadores-gastos-por-mes').highcharts({
    			chart: {
    				type: 'column'
    			},

    			title: {
    				text: null //'Gasto mensal com a cota parlamentar'
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

    	$api.get('Senador/' + $routeParams.id.toString() + '/MaioresNotas').success(function (response) {
    		$scope.MaioresNotas = response
    	});

    	$api.get('Senador/' + $routeParams.id.toString() + '/MaioresFornecedores').success(function (response) {
    		$scope.MaioresFornecedores = response
    	});
    }]);