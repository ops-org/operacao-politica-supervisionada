<template>
  <div>
    <div class="container vld-parent" ref="Detalhes">
        <h3 class="page-title">Deputado(a) Federal</h3>

        <div class="row form-group">
            <div class="col-xs-12 col-md-4 col-lg-2 text-center">
                <img class="img-thumbnail img-responsive img-deputado" v-bind:src="API + '/deputado/imagem/' + id + '_120x160'" v-bind:alt="deputado_federal.nome_parlamentar" width="130" height="170" />
            </div>
            <div class="col-xs-12 col-md-8 col-lg-10 text-left">
                <h4 style="margin-top: 0;">{{deputado_federal.nome_parlamentar}} <small>({{deputado_federal.sigla_partido}} / {{deputado_federal.sigla_estado}})</small></h4>
                <div class="row">
                    <div class="col-sm-6">
                        <p class="mb-0"><strong>Nome civil:</strong> {{deputado_federal.nome_civil}}</p>
                        <p class="mb-0"><strong>Partido:</strong> {{deputado_federal.nome_partido}}</p>
                        <p class="mb-0"><strong>Estado:</strong> {{deputado_federal.nome_estado}}</p>
                        <p class="mb-0"><strong>Condição:</strong> {{deputado_federal.condicao}} ({{deputado_federal.situacao}})</p>
                        <p class="mb-0" v-if="deputado_federal.profissao"><strong>Profissão:</strong> {{deputado_federal.profissao}}</p>
                        <p class="mb-0" v-if="deputado_federal.nome_municipio_nascimento"><strong>Naturalidade:</strong> {{deputado_federal.nome_municipio_nascimento}} - {{deputado_federal.sigla_estado_nascimento}}</p>
                        <p class="mb-0"><strong>Nascimento:</strong> {{deputado_federal.nascimento}}</p>
                        <p class="mb-0" v-if="deputado_federal.falecimento"><strong>Falecimento:</strong> {{deputado_federal.falecimento}}</p>
                    </div>
                    <div class="col-sm-6">
                        <p class="mb-0" title="Secretários Parlamentares">
                            <strong>Pessoal do Gabinete:</strong>
                            <a v-bind:href="'/deputado-federal/' + id + '/secretario'" title="Clique para ver a lista de secretários">{{deputado_federal.quantidade_secretarios}} Secretário(s)</a>
                        </p>
                        <p class="mb-0">
                            <strong>Custo Mensal do Gabinete:</strong>
                            <a v-bind:href="'/deputado-federal/' + id + '/secretario'" title="Clique para ver a lista de secretários">R$ {{deputado_federal.custo_secretarios}}</a>
                        </p>
                        <p class="mb-0">
                            <strong>Gasto Acumulado CEAP:</strong>
                            <a v-bind:href="'/deputado-federal?IdParlamentar=' + id + '&Periodo=0&Agrupamento=6'" title="Clique para ver os gastos com cota parlamentar em detalhes">R$ {{deputado_federal.valor_total_ceap}}</a>
                        </p>
                        <p class="mb-0">
                            <strong>Visualizar:</strong>
                            <a v-bind:href="'https://www.camara.leg.br/deputados/' + id + '/biografia'" target="_blank" onclick="return trackOutboundLink(this, true);">Biografia</a>
                            <span v-if="deputado_federal.situacao!='Fim de Mandato'">&nbsp;-
                                <a v-bind:href="'http://www.camara.leg.br/Internet/Deputado/dep_Detalhe.asp?id=' + id" target="_blank" onclick="return trackOutboundLink(this, true);">Página Oficial</a>
                            </span>
                        </p>
                        <p class="mb-0"><strong>Telefone:</strong> {{deputado_federal.telefone}}</p>
                        <p class="mb-0"><strong>Email:</strong> {{deputado_federal.email}}</p>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="container-fluid">
        <div class="card mb-3">
            <div class="card-header bg-light">
                Comparativo de gastos mensais com a cota parlamentar
            </div>
            <div class="card-body chart-deputado-gastos-por-mes vld-parent" ref="DeputadoGastosPorMes">
                <highcharts :options="chartDeputadoGastosPorMesOptions" ref="chartDeputadoGastosPorMes"></highcharts>
            </div>
        </div>

        <div class="row form-group">
            <div class="col-xs-12 col-sm-6">
                <div class="card mb-3">
                    <div class="card-header bg-light">
                        <a class="float-right" v-bind:href="'/deputado-federal?IdParlamentar=' + id + '&Periodo=0&Agrupamento=6'">Ver lista completa</a>
                        Maiores Notas/Recibos
                    </div>
                    <div class="card-body maiores-notas vld-parent" ref="MaioresNotas">
                        <div class="table-responsive" v-if="MaioresNotas.length > 0">
                            <table class="table table-striped table-hover table-sm" style="margin: 0;">
                                <thead>
                                    <tr>
                                        <th style="width:80%">Fornecedor</th>
                                        <th style="width:20%">Valor</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="row in MaioresNotas" :key="row.id_cf_despesa">
                                        <td><a v-bind:href="'/fornecedor/' + row.id_fornecedor">{{row.nome_fornecedor}}</a></td>
                                        <td><a v-bind:href="'/deputado-federal/documento/' + row.id_cf_despesa">{{row.valor_liquido}}</a></td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-xs-12 col-sm-6">
                <div class="card mb-3">
                    <div class="card-header bg-light">
                        <a class="float-right" v-bind:href="'/deputado-federal?IdParlamentar=' + id + '&Periodo=0&Agrupamento=3'">Ver lista completa</a>
                        Maiores fornecedores
                    </div>
                    <div class="card-body maiores-fornecedores vld-parent" ref="MaioresFornecedores">
                        <div class="table-responsive" v-if="MaioresFornecedores.length > 0">
                            <table class="table table-striped table-hover table-sm" style="margin: 0;">
                                <thead>
                                    <tr>
                                        <th style="width:80%">Fornecedor</th>
                                        <th style="width:20%">Valor</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="row in MaioresFornecedores" :key="row.id_fornecedor">
                                        <td><a v-bind:href="'/fornecedor/' + row.id_fornecedor">{{row.nome_fornecedor}}</a></td>
                                        <td><a v-bind:href="'/deputado-federal?IdParlamentar=' + id + '&Fornecedor=' + row.id_fornecedor + '&Periodo=0&Agrupamento=6'">{{row.valor_total}}</a></td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="card mb-3" id="deputados-presenca">
            <div class="card-header bg-light">
                Frequência nas Sessões Ordinárias
            </div>
            <div class="card-body chart-deputado-presenca vld-parent" ref="DeputadoPresenca">
                <div class="row form-group">
                    <div class="col-xs-12 col-sm-4">
                        <highcharts :options="chartDeputadoPresencaPercentualOptions" ref="chartDeputadoPresencaPercentual"></highcharts>
                    </div>
                    <div class="col-xs-12 col-sm-8">
                        <highcharts :options="chartDeputadoPresencaAnualOptions" ref="chartDeputadoPresencaAnual"></highcharts>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
</template>

<script>
import { Chart } from 'highcharts-vue';

const axios = require('axios');

export default {
  name: 'DeputadoFederalDetalhes',
  components: {
    highcharts: Chart,
  },
  props: {
    id: Number,
  },
  data() {
    return {
      API: '',
      deputado_federal: {},
      MaioresNotas: {},
      MaioresFornecedores: {},

      chartDeputadoGastosPorMesOptions: {
        chart: {
          type: 'column',
        },

        title: {
          text: null, // 'Gasto mensal com a cota parlamentar'
        },

        xAxis: {
          categories: ['Jan', 'Fev', 'Mar', 'Abr', 'Maio', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
        },

        yAxis: [{ // left y axis
          title: {
            text: 'Valor (em reais)',
          },
          labels: {
            format: '{value:.,2f}',
            overflow: 'justify',
          },
          showFirstLabel: false,
        }],

        tooltip: {
          pointFormat: '<span style="color:{series.color}">\u25CF</span> {series.name}: <b>{point.y}</b><br/>',
          shared: true,
          crosshairs: true,
        },

        series: [],
      },

      chartDeputadoPresencaPercentualOptions: { },

      chartDeputadoPresencaAnualOptions: {
        chart: {
          type: 'column',
        },

        title: {
          text: null,
        },
        subtitle: {
          text: 'Resumo anual',
        },

        xAxis: {
          categories: [],
        },

        yAxis: [{ // left y axis
          min: 0,
          title: {
            text: 'Frequência',
          },
          labels: {
            align: 'left',
            x: 3,
            y: 16,
            format: '{value:.,0f}',
          },
          showFirstLabel: false,
          stackLabels: {
            enabled: true,
            style: {
              fontWeight: 'bold',
              color: (Chart.theme && Chart.theme.textColor) || 'gray',
            },
          },
        }],

        tooltip: {
          pointFormat: '<span style="color:{series.color}">\u25CF</span> {series.name}: <b>{point.y}</b><br/>',
          shared: true,
          crosshairs: true,
        },

        plotOptions: {
          column: {
            stacking: 'normal',
            dataLabels: {
              enabled: true,
            },
          },
        },

        colors: ['#7cb5ec', '#e4d354', '#f15c80'],

        series: [],
      },
    };
  },
  mounted() {
    window.document.title = 'OPS :: Deputado Federal';
    this.API = process.env.API;

    const loaderDetalhes = this.$loading.show({
      fullPage: false,
      container: this.$refs.Detalhes,
    });

    const loaderDeputadoGastosPorMes = this.$loading.show({
      fullPage: false,
      container: this.$refs.DeputadoGastosPorMes,
    });

    const loaderDeputadoPresenca = this.$loading.show({
      fullPage: false,
      container: this.$refs.DeputadoPresenca,
    });

    const loaderMaioresNotas = this.$loading.show({
      fullPage: false,
      container: this.$refs.MaioresNotas,
    });

    const loaderMaioresFornecedores = this.$loading.show({
      fullPage: false,
      container: this.$refs.MaioresFornecedores,
    });

    axios
      .get(`${process.env.API}/deputado/${this.id}`)
      .then((response) => {
        this.deputado_federal = response.data;

        window.document.title = `OPS :: Deputado Federal - ${response.data.nome_parlamentar}`;
        loaderDetalhes.hide();
      });

    axios
      .get(`${process.env.API}/deputado/${this.id}/GastosMensaisPorAno`)
      .then((response) => {
        this.chartDeputadoGastosPorMesOptions.series = response.data;

        loaderDeputadoGastosPorMes.hide();
      });

    axios
      .get(`${process.env.API}/deputado/${this.id}/ResumoPresenca`)
      .then((response) => {
        // TODO: pq não dá pra deixar isso no data
        this.chartDeputadoPresencaPercentualOptions = {
          chart: {
            type: 'pie',
          },

          title: {
            text: null,
          },
          subtitle: {
            text: 'Resumo Geral',
          },

          tooltip: {
            pointFormat: '<span style="color:{series.color}">\u25CF</span> {series.name}: <b>{point.percentage:.2f}teste%</b><br/>',
          },

          plotOptions: {
            pie: {
              cursor: 'pointer',
              size: '90%',
              innerSize: '20%',
              dataLabels: {
                enabled: true,
                format: '{point.percentage:.2f}%',
              },
              showInLegend: true,
              data: response.data.frequencia_total_percentual,
            },
          },

          colors: ['#7cb5ec', '#e4d354', '#f15c80'],

          series: [{
            type: 'pie',
            name: 'Frequência',

            dataLabels: {
              distance: -40,
              format: '{point.percentage:.2f}%',
            },
          }],
        };

        this.chartDeputadoPresencaAnualOptions.xAxis.categories = response.data.frequencia_anual.categories;
        this.chartDeputadoPresencaAnualOptions.series = response.data.frequencia_anual.series;

        loaderDeputadoPresenca.hide();
      });

    axios
      .get(`${process.env.API}/deputado/${this.id}/MaioresNotas`)
      .then((response) => {
        this.MaioresNotas = response.data;

        loaderMaioresNotas.hide();
      });

    axios
      .get(`${process.env.API}/deputado/${this.id}/MaioresFornecedores`)
      .then((response) => {
        this.MaioresFornecedores = response.data;

        loaderMaioresFornecedores.hide();
      });
  },
};
</script>


<style scoped>
  .chart-deputado-presenca, .chart-deputado-gastos-por-mes{
    min-height: 440px;;
  }
  .maiores-notas, .maiores-fornecedores{
    min-height: 100px;;
  }
</style>
