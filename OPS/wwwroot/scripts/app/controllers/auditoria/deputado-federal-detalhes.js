'use strict';

app.controller('DeputadoVisualizarController', ["$scope", "$api", "$routeParams", "NgTableParams",
	function ($scope, $api, $routeParams, NgTableParams) {
		document.title = "OPS :: Deputado Federal";

		$scope.disqusConfig = {
			disqus_identifier: 'deputado-federal-' + $routeParams.id.toString(),
			disqus_url: base_url + '/deputado-federal/' + $routeParams.id.toString()
		};

		$api.get('Deputado/' + $routeParams.id.toString()).success(function (response) {
			$scope.deputado_federal = response;

			document.title = "OPS :: Deputado Federal - " + response.nome_parlamentar;
		});

        $api.get('Deputado/' + $routeParams.id.toString() + '/GastosMensaisPorAno').success(function (response) {
            if (response.length > 0) {
                $('#deputados-gastos-por-mes').highcharts({
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
                            format: '{value:.,2f}',
                            overflow: 'justify'
                        },
                        showFirstLabel: false
                    }],

                    tooltip: {
                        pointFormat: '<span style="color:{series.color}">\u25CF</span> {series.name}: <b>{point.y}</b><br/>',
                        shared: true,
                        crosshairs: true
                    },

                    series: response
                });
            } else {
                $('#deputados-gastos-por-mes').html("O parlamentar não fez uso da cota parlamentar até o momento!");
            }
		});

        $api.get('Deputado/' + $routeParams.id.toString() + '/ResumoPresenca').success(function (response) {
            if (response.frequencia_anual.categories.length > 0) {
                $('#deputados-presenca-total-percentual').highcharts({
                    chart: {
                        type: 'pie'
                    },

                    title: {
                        text: null
                    },
                    subtitle: {
                        text: 'Resumo Geral'
                    },

                    tooltip: {
                        pointFormat: '<span style="color:{series.color}">\u25CF</span> {series.name}: <b>{point.percentage:.2f}%</b><br/>'
                    },

                    plotOptions: {
                        pie: {
                            cursor: 'pointer',
                            size: '90%',
                            innerSize: '20%',
                            dataLabels: {
                                enabled: true,
                                format: '{point.percentage:.2f}%'
                            },
                            showInLegend: true,
                            data: response.frequencia_total_percentual
                        }
                    },

                    colors: ['#7cb5ec', '#e4d354', '#f15c80'],

                    series: [{
                        type: 'pie',
                        name: 'Frequência',

                        dataLabels: {
                            distance: -40,
                            format: '{point.percentage:.2f}%'
                        }
                    }]
                });

                $('#deputados-presenca-anual').highcharts({
                    chart: {
                        type: 'column'
                    },

                    title: {
                        text: null
                    },
                    subtitle: {
                        text: 'Resumo anual'
                    },

                    xAxis: {
                        categories: response.frequencia_anual.categories,
                    },

                    yAxis: [{ // left y axis
                        min: 0,
                        title: {
                            text: "Frequência"
                        },
                        labels: {
                            align: 'left',
                            x: 3,
                            y: 16,
                            format: '{value:.,0f}'
                        },
                        showFirstLabel: false,
                        stackLabels: {
                            enabled: true,
                            style: {
                                fontWeight: 'bold',
                                color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                            }
                        }
                    }],

                    tooltip: {
                        //pointFormat: '<span style="color:{series.color}">\u25CF</span> {series.name}: <b>{point.y}</b> ({point.percentage:.0f}%)<br/>',
                        pointFormat: '<span style="color:{series.color}">\u25CF</span> {series.name}: <b>{point.y}</b><br/>',
                        shared: true,
                        crosshairs: true
                    },

                    plotOptions: {
                        column: {
                            stacking: 'normal',
                            dataLabels: {
                                enabled: true
                            }
                        }
                    },

                    colors: ['#7cb5ec', '#e4d354', '#f15c80'],

                    series: response.frequencia_anual.series
                });
            } else {
                $('#deputados-presenca .card-body').html('Ainda não há presenças registradas para esse parlamentar.');
            }
		});

		$api.get('Deputado/' + $routeParams.id.toString() + '/MaioresNotas').success(function (response) {
			$scope.MaioresNotas = response;
		});

		$api.get('Deputado/' + $routeParams.id.toString() + '/MaioresFornecedores').success(function (response) {
			$scope.MaioresFornecedores = response;
		});
	}]);