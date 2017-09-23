'use strict';

app.controller('PrincipalController', ['$scope', '$api',
    function ($scope, $api) {

        document.title = "OPS :: Operação Politica Supervisionada";
        $scope.disqusConfig = {
            disqus_identifier: 'Pagina Inicial'
        };

        $api.get('Indicadores/ParlamentarResumoGastos').success(function (response) {
            $scope.CampeoesGastos = response;
        });

        $api.get('Deputado/CamaraResumoMensal').success(function (response) {
            Highcharts.setOptions({
                lang: {
                    decimalPoint: ',',
                    thousandsSep: ' '
                }
            });

            var chart = Highcharts.chart('camara-resumo-gastos', {
                chart: {
                    type: 'column'
                },

                title: {
                    text: null //'Gasto mensal com a cota parlamentar'
                },

                xAxis: {
                    categories: ['Jan', 'Feb', 'Mar', 'Abr', 'Maio', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez']
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

                dataLabels: {
                    enabled: true,
                    rotation: -90,
                    color: '#FFFFFF',
                    align: 'right',
                    format: '{point.y:.2f}', // one decimal
                    y: 10, // 10 pixels down from the top
                    style: {
                        fontSize: '13px',
                        fontFamily: 'Verdana, sans-serif'
                    }
                },

                series: response
            });

            //Esconder Intervalo de 2009 a 2014
            for (var i = 0; i < 5; i++) {
                chart.series[i].hide();
            }

            $('#camara-legislatura input[type=radio]').on('change', function () {
                switch ($(this).val()) {
                    case '54':
                        chart.series[0].hide(); //2009
                        chart.series[1].hide(); //2010
                        chart.series[2].hide(); //2011
                        chart.series[3].hide(); //2012
                        chart.series[4].hide(); //2013
                        chart.series[5].hide(); //2014
                        chart.series[6].show(); //2015
                        chart.series[7].show(); //2016
                        chart.series[8].show(); //2017
                        break;
                    case '53':
                        chart.series[0].hide(); //2009
                        chart.series[1].hide(); //2010
                        chart.series[2].show(); //2011
                        chart.series[3].show(); //2012
                        chart.series[4].show(); //2013
                        chart.series[5].show(); //2014
                        chart.series[6].hide(); //2015
                        chart.series[7].hide(); //2016
                        chart.series[8].hide(); //2017
                        break;
                    case '52':
                        chart.series[0].show(); //2009
                        chart.series[1].show(); //2010
                        chart.series[2].hide(); //2011
                        chart.series[3].hide(); //2012
                        chart.series[4].hide(); //2013
                        chart.series[5].hide(); //2014
                        chart.series[6].hide(); //2015
                        chart.series[7].hide(); //2016
                        chart.series[8].hide(); //2017
                        break;
                    default:
                        for (var i = 0; i < chart.series.length; i++) {
                            chart.series[i].show();
                        }
                        break;
                }
            });
        });

        $api.get('Senador/SenadoResumoMensal').success(function (response) {
            Highcharts.setOptions({
                lang: {
                    decimalPoint: ',',
                    thousandsSep: ' '
                }
            });

            var chart = Highcharts.chart('senado-resumo-gastos', {
                chart: {
                    type: 'column'
                },

                title: {
                    text: null //'Gasto mensal com a cota parlamentar'
                },

                xAxis: {
                    categories: ['Jan', 'Feb', 'Mar', 'Abr', 'Maio', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez']
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

            //Esconder Intervalo de 2008 a 2014
            for (var i = 0; i < 6; i++) {
                chart.series[i].hide();
            }

            $('#senado-legislatura input[type=radio]').on('change', function () {
                switch ($(this).val()) {
                    case '54':
                        chart.series[0].hide(); //2008
                        chart.series[1].hide(); //2009
                        chart.series[2].hide(); //2010
                        chart.series[3].hide(); //2011
                        chart.series[4].hide(); //2012
                        chart.series[5].hide(); //2013
                        chart.series[6].hide(); //2014
                        chart.series[7].show(); //2015
                        chart.series[8].show(); //2016
                        chart.series[9].show(); //2017
                        break;
                    case '53':
                        chart.series[0].hide(); //2008
                        chart.series[1].hide(); //2009
                        chart.series[2].hide(); //2010
                        chart.series[3].show(); //2011
                        chart.series[4].show(); //2012
                        chart.series[5].show(); //2013
                        chart.series[6].show(); //2014
                        chart.series[7].hide(); //2015
                        chart.series[8].hide(); //2016
                        chart.series[9].hide(); //2017
                        break;
                    case '52':
                        chart.series[0].show(); //2008
                        chart.series[1].show(); //2009
                        chart.series[2].show(); //2010
                        chart.series[3].hide(); //2011
                        chart.series[4].hide(); //2012
                        chart.series[5].hide(); //2013
                        chart.series[6].hide(); //2014
                        chart.series[7].hide(); //2015
                        chart.series[8].hide(); //2016
                        chart.series[9].hide(); //2017
                        break;
                    default:
                        for (var i = 0; i < chart.series.length; i++) {
                            chart.series[i].show();
                        }
                        break;
                }
            });
        });

        $api.get('Deputado/CamaraResumoAnual').success(function (response) {
            Highcharts.setOptions({
                global: {
                    useUTC: false
                },
                lang: {
                    decimalPoint: ',',
                    thousandsSep: ' '
                }
            });

            $('#camara-resumo-gastos-anual').highcharts({
                chart: {
                    type: 'column'
                },

                title: {
                    text: null
                },

                xAxis:
                {
                    categories: response.categories
                },

                yAxis: [{ // left y axis
                    title: {
                        text: "Valor (em reais)"
                    },
                    //labels: {
                    //    align: 'left',
                    //    x: 3,
                    //    y: 16,
                    //    format: '{value:,.0f}'
                    //},
                    showFirstLabel: false
                }],

                legend: {
                    enabled: false
                },

                tooltip: {
                    shared: true,
                    crosshairs: true,
                    pointFormat: '<span style=color:{point.color}">\u25CF</span> {series.name}: <b class="legend">{point.y:,.2f}</b><br/>'
                },

                series: [{
                    name: 'Câmara',
                    data: response.series,
                    dataLabels: {
                        enabled: true,
                        rotation: -90,
                        color: '#FFFFFF',
                        align: 'right',
                        format: '{point.y:,.2f}', // one decimal
                        y: 10, // 10 pixels down from the top
                        style: {
                            fontSize: '13px',
                            fontFamily: 'Verdana, sans-serif'
                        }
                    }
                }]
            });
        });

        $api.get('Senador/SenadoResumoAnual').success(function (response) {
            Highcharts.setOptions({
                lang: {
                    decimalPoint: ',',
                    thousandsSep: ' '
                }
            });

            $('#senado-resumo-gastos-anual').highcharts({
                chart: {
                    type: 'column'
                },

                title: {
                    text: null
                },

                xAxis:
                {
                    categories: response.categories
                },

                yAxis: [{ // left y axis
                    title: {
                        text: "Valor (em reais)"
                    },
                    //labels: {
                    //    align: 'left',
                    //    x: 3,
                    //    y: 16,
                    //    format: '{value:,.0f}'
                    //},
                    showFirstLabel: false
                }],

                legend: {
                    enabled: false
                },

                tooltip: {
                    shared: true,
                    crosshairs: true,
                    pointFormat: '<span style=color:{point.color}">\u25CF</span> {series.name}: <b class="legend">{point.y:,.2f}</b><br/>'
                },

                series: [{
                    name: 'Senado',
                    data: response.series,
                    dataLabels: {
                        enabled: true,
                        rotation: -90,
                        color: '#FFFFFF',
                        align: 'right',
                        format: '{point.y:,.2f}', // one decimal
                        y: 10, // 10 pixels down from the top
                        style: {
                            fontSize: '13px',
                            fontFamily: 'Verdana, sans-serif'
                        }
                    }
                }]
            });
        });
    }]);