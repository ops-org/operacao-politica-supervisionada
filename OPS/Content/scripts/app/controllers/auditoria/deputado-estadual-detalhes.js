'use strict';

app.controller('DeputadoEstadualVisualizarController', ["$scope", "$api", "$routeParams", "NgTableParams",
    function ($scope, $api, $routeParams, NgTableParams) {
    	document.title = "OPS :: Deputado Estadual";

    	$scope.disqusConfig = {
    		disqus_identifier: 'deputado-' + $routeParams.id.toString(),
    		disqus_url: base_url + '/deputado/' + $routeParams.id.toString()
    	};

    	$api.get('DeputadoEstadual/' + $routeParams.id.toString()).success(function (response) {
    		$scope.deputado = response;

			document.title = "OPS :: Deputado Estadual - " + response.nome_parlamentar;
    	});

    	$api.get('DeputadoEstadual/' + $routeParams.id.toString() + '/GastosMensaisPorAno').success(function (response) {
    		$('#deputado-gastos-por-mes').highcharts({
    			chart: {
    				type: 'column'
    			},

    			title: {
    				text: null //'Gasto mensal com a cota parlamentar'
    			},

    			xAxis: {
    				categories: ['Jan', 'Fev', 'Mar', 'Abr', 'Maio', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
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

    	$api.get('DeputadoEstadual/' + $routeParams.id.toString() + '/MaioresNotas').success(function (response) {
            $scope.MaioresNotas = response;
        });

    	$api.get('DeputadoEstadual/' + $routeParams.id.toString() + '/MaioresFornecedores').success(function (response) {
            $scope.MaioresFornecedores = response;
        });
    }]);