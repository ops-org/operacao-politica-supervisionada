'use strict';

app.controller('PrincipalController', ['$scope', '$api',
    function ($scope, $api) {

        document.title = "OPS :: Operação Politica Supervisionada";
        $scope.disqusConfig = {
            disqus_identifier: 'Pagina Inicial'
        };

        $scope.search = function (event) {
            event.stopPropagation();
            window.location.href = window.location.origin + '/#!/busca?q=' + $scope.q;
        };

        $api.get('Indicadores/ParlamentarResumoGastos').success(function (response) {
            $scope.CampeoesGastos = response;
        });

        $api.get('Deputado/CamaraResumoMensal').success(function (response) {
            var chart = Highcharts.chart('camara-resumo-gastos', {
                chart: {
                    type: 'column'
                },

                title: {
                    text: null //'Gasto mensal com a cota parlamentar'
                },

                xAxis: {
                    categories: ['Jan', 'Fev', 'Mar', 'Abr', 'Maio', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez']
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

            for (var i = 0; i <= chart.series.length - 5; i++) {
                chart.series[i].hide();
            }

            $('#camara-legislatura input[type=radio]').on('change', function () {
                switch ($(this).val()) {
                    case '56':
                        chart.series[0].hide(); //2009
                        chart.series[1].hide(); //2010
                        chart.series[2].hide(); //2011
                        chart.series[3].hide(); //2012
                        chart.series[4].hide(); //2013
                        chart.series[5].hide(); //2014
                        chart.series[6].hide(); //2015
                        chart.series[7].hide(); //2016
                        chart.series[8].hide(); //2017
                        chart.series[9].hide(); //2018
                        chart.series[10].show(); //2019
                        break;
                    case '55':
                        chart.series[0].hide(); //2009
                        chart.series[1].hide(); //2010
                        chart.series[2].hide(); //2011
                        chart.series[3].hide(); //2012
                        chart.series[4].hide(); //2013
                        chart.series[5].hide(); //2014
                        chart.series[6].show(); //2015
                        chart.series[7].show(); //2016
                        chart.series[8].show(); //2017
                        chart.series[9].show(); //2018
                        chart.series[10].hide(); //2019
                        break;
                    case '54':
                        chart.series[0].hide(); //2009
                        chart.series[1].hide(); //2010
                        chart.series[2].show(); //2011
                        chart.series[3].show(); //2012
                        chart.series[4].show(); //2013
                        chart.series[5].show(); //2014
                        chart.series[6].hide(); //2015
                        chart.series[7].hide(); //2016
                        chart.series[8].hide(); //2017
                        chart.series[9].hide(); //2018
                        chart.series[10].hide(); //2019
                        break;
                    case '53':
                        chart.series[0].show(); //2009
                        chart.series[1].show(); //2010
                        chart.series[2].hide(); //2011
                        chart.series[3].hide(); //2012
                        chart.series[4].hide(); //2013
                        chart.series[5].hide(); //2014
                        chart.series[6].hide(); //2015
                        chart.series[7].hide(); //2016
                        chart.series[8].hide(); //2017
                        chart.series[9].hide(); //2018
                        chart.series[10].hide(); //2019
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
            var chart = Highcharts.chart('senado-resumo-gastos', {
                chart: {
                    type: 'column'
                },

                title: {
                    text: null //'Gasto mensal com a cota parlamentar'
                },

                xAxis: {
                    categories: ['Jan', 'Fev', 'Mar', 'Abr', 'Maio', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez']
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

            for (var i = 0; i < chart.series.length - 4; i++) {
                chart.series[i].hide();
            }

            $('#senado-legislatura input[type=radio]').on('change', function () {
                switch ($(this).val()) {
                    case '55':
                        chart.series[0].hide(); //2008
                        chart.series[1].hide(); //2009
                        chart.series[2].hide(); //2010
                        chart.series[3].hide(); //2011
                        chart.series[4].hide(); //2012
                        chart.series[5].hide(); //2013
                        chart.series[6].hide(); //2014
                        chart.series[7].hide(); //2015
                        chart.series[8].hide(); //2016
                        chart.series[9].hide(); //2017
                        chart.series[10].hide(); //2018
                        chart.series[11].show(); //2019
                        break;
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
                        chart.series[10].show(); //2018
                        chart.series[11].hide(); //2019
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
                        chart.series[10].hide(); //2018
                        chart.series[11].hide(); //2019
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
                        chart.series[10].hide(); //2018
                        chart.series[11].hide(); //2019
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
            $('#camara-resumo-gastos-anual').highcharts({
                chart: {
                    type: 'bar'
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
                        //rotation: -90,
                        color: '#FFFFFF',
                        align: 'right',
                        format: '{point.y:,.2f}', // one decimal
                        y: -1, // -1 pixels down from the top
                        style: {
                            fontSize: '13px',
                            fontFamily: 'Verdana, sans-serif'
                        }
                    }
                }]
            });
        });

        $api.get('Senador/SenadoResumoAnual').success(function (response) {
            $('#senado-resumo-gastos-anual').highcharts({
                chart: {
                    type: 'bar'
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
                        //rotation: -90,
                        color: '#FFFFFF',
                        align: 'right',
                        format: '{point.y:,.2f}', // one decimal
                        y: -1, // -1 pixels down from the top
                        style: {
                            fontSize: '13px',
                            fontFamily: 'Verdana, sans-serif'
                        }
                    }
                }]
            });
        });
    }]);