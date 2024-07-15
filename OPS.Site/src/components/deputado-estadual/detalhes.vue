<template>
  <div>
    <div class="container vld-parent" ref="Detalhes">
        <h3 class="page-title">[BETA] Deputado(a) Estadual</h3>

        <div class="row form-group">
            <!-- <div class="col-xs-12 col-md-4 col-lg-2 text-center">
            </div> -->
            <div class="col-xs-12 col-md-8 col-lg-10 text-left">
                <h4 style="margin-top: 0;">{{deputado_estadual.nome_parlamentar}} <small>({{deputado_estadual.sigla_partido}} / {{deputado_estadual.sigla_estado}})</small></h4>
                <div class="row">
                    <div class="col-sm-6">
                        <p class="mb-0"><strong>Nome:</strong> {{deputado_estadual.nome_civil}}</p>
                        <p class="mb-0"><strong>Partido:</strong> {{deputado_estadual.nome_partido}}</p>
                        <p class="mb-0"><strong>Estado:</strong> {{deputado_estadual.nome_estado}}</p>
                        <p class="mb-0" v-if="deputado_estadual.profissao"><strong>Profissão:</strong> {{deputado_estadual.profissao}}</p>
                        <p class="mb-0" v-if="deputado_estadual.naturalidade"><strong>Naturalidade:</strong> {{deputado_estadual.naturalidade}}</p>
                        <p class="mb-0" v-if="deputado_estadual.nascimento"><strong>Nascimento:</strong> {{deputado_estadual.nascimento}}</p>
                        <p class="mb-0"><strong>Telefone:</strong> {{deputado_estadual.telefone}}</p>
                        <p class="mb-0"><strong>Email:</strong> {{deputado_estadual.email}}</p>
                    </div>
                    <div class="col-sm-6" v-if="id>100">
                        <!-- <p class="mb-0" title="Secretários Parlamentares">
                            <strong>Pessoal do Gabinete: </strong>
                        </p>
                        <p class="mb-0">
                            <strong>Folha de Pagamento (Acumulado): </strong>
                        </p> -->
                        <p class="mb-0">
                            <strong>Cota Parlamentar (Acumulado): </strong>
                        </p>
                        <p class="mb-0" v-if="deputado_estadual.perfil || deputado_estadual.site">
                            <strong>Visualizar: </strong>
                            <span v-if="deputado_estadual.perfil">
                                <a
                                  v-bind:href="deputado_estadual.perfil"
                                  target="_blank">Página Oficial</a>
                            </span>
                            <span v-if="deputado_estadual.site">&nbsp;-
                                <a
                                  v-bind:href="deputado_estadual.site"
                                  target="_blank">Site</a>
                            </span>
                        </p>
                    </div>
                </div>
            </div>
        </div>

        <div class="card mb-3" v-if="MaioresFornecedores.length > 0">
            <div class="card-header bg-light">
                Gastos anuais com a cota parlamentar
            </div>
            <div class="card-body chart-deputado-gastos-por-mes vld-parent" ref="DeputadoGastosPorAno">
                <highcharts :options="chartDeputadoGastosPorAnoOptions" ref="chartDeputadoGastosPorAno"></highcharts>
            </div>
        </div>

        <div class="alert alert-warning" role="alert" v-if="deputado_estadual.sigla_estado=='SC'">
          Algumas notas podem não ter um fornecedor identificado e com isso não serão apresentadas abaixo!<br>
        </div>

        <div class="row form-group" v-if="MaioresFornecedores.length > 0">
            <div class="col-xs-12 col-sm-6">
                <div class="card mb-3">
                    <div class="card-header bg-light">
                        Maiores fornecedores
                    </div>
                    <div class="card-body maiores-fornecedores vld-parent" ref="MaioresFornecedores">
                        <div class="table-responsive">
                            <table class="table table-striped table-hover table-sm" style="margin: 0;" aria-label="">
                                <thead>
                                    <tr>
                                        <th style="width:80%">Fornecedor</th>
                                        <th style="width:20%" class="text-right">Valor</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="row in MaioresFornecedores" :key="row.id_fornecedor">
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
             <div class="col-xs-12 col-sm-6" >
                <div class="card mb-3">
                    <div class="card-header bg-light">
                        Maiores Notas/Recibos
                    </div>
                    <div class="card-body maiores-notas vld-parent" ref="MaioresNotas">
                        <div class="table-responsive">
                            <table class="table table-striped table-hover table-sm" style="margin: 0;" aria-label="">
                                <thead>
                                    <tr>
                                        <th style="width:80%">Fornecedor</th>
                                        <th style="width:20%" class="text-right">Valor</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="row in MaioresNotas" :key="row.id_cf_despesa">
                                        <td class="text-right">{{row.valor_liquido}}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>

            <!-- <div class="col-sm-12" v-if="id>100">
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
            </div> -->
        </div>
    </div>
</div>
</template>

<script>
import { Chart } from 'highcharts-vue';

const axios = require('axios');

export default {
  name: 'DeputadoEstadualDetalhes',
  components: {
    highcharts: Chart,
  },
  props: {
    id: Number,
  },
  data() {
    return {
      API: '',
      deputado_estadual: {},
      MaioresNotas: {},
      MaioresFornecedores: {},

      chartDeputadoGastosPorAnoOptions: {
        chart: {
          type: 'bar',
        },

         title: {
          text: null, // 'Gasto mensal com a cota parlamentar'
        },

        xAxis: {
          categories: [],
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

        legend: {
          enabled: false,
        },

        tooltip: {
          pointFormat: '<span style="color:{series.color}">\u25CF</span> {series.name}: <b>{point.y}</b><br/>',
          shared: true,
          crosshairs: true,
        },

        series: [{
          pointWidth: 20,
          name: 'Valor (em reais)',
          data: [],
          dataLabels: {
            enabled: true,
            // rotation: -90,
            color: '#000',
            align: 'right',
            format: '{point.y:,.2f}', // one decimal
            y: -1, // -1 pixels down from the top
            style: {
              fontSize: '13px',
              fontFamily: 'Verdana, sans-serif',
            },
          },
        }],
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
    window.document.title = 'OPS :: Deputado Estadual';
    this.API = process.env.VUE_APP_API;

    const loaderDetalhes = this.$loading.show({
      fullPage: false,
      container: this.$refs.Detalhes,
    });

    const loaderDeputadoGastosPorAno = this.$loading.show({
      fullPage: false,
      container: this.$refs.DeputadoGastosPorAno,
    });

    // const loaderDeputadoPresenca = this.$loading.show({
    //   fullPage: false,
    //   container: this.$refs.DeputadoPresenca,
    // });

    const loaderMaioresNotas = this.$loading.show({
      fullPage: false,
      container: this.$refs.MaioresNotas,
    });

    const loaderMaioresFornecedores = this.$loading.show({
      fullPage: false,
      container: this.$refs.MaioresFornecedores,
    });

    axios
      .get(`${process.env.VUE_APP_API}/deputadoestadual/${this.id}`)
      .then((response) => {
        this.deputado_estadual = response.data;

        window.document.title = `OPS :: Deputado Estadual - ${response.data.nome_parlamentar}`;
        loaderDetalhes.hide();
      });

    axios
      .get(`${process.env.VUE_APP_API}/deputadoestadual/${this.id}/GastosPorAno`)
      .then((response) => {
        this.chartDeputadoGastosPorAnoOptions.series[0].data = response.data.series;
        this.chartDeputadoGastosPorAnoOptions.xAxis.categories = response.data.categories;

        loaderDeputadoGastosPorAno.hide();
      });

    // axios
    //   .get(`${process.env.VUE_APP_API}/deputadoestadual/${this.id}/ResumoPresenca`)
    //   .then((response) => {
    //     // TODO: pq não dá pra deixar isso no data
    //     this.chartDeputadoPresencaPercentualOptions = {
    //       chart: {
    //         type: 'pie',
    //       },

    //       title: {
    //         text: null,
    //       },
    //       subtitle: {
    //         text: 'Resumo Geral',
    //       },

    //       tooltip: {
    //         pointFormat: '<span style="color:{series.color}">\u25CF</span> {series.name}: <b>{point.percentage:.2f}teste%</b><br/>',
    //       },

    //       plotOptions: {
    //         pie: {
    //           cursor: 'pointer',
    //           size: '90%',
    //           innerSize: '20%',
    //           dataLabels: {
    //             enabled: true,
    //             format: '{point.percentage:.2f}%',
    //           },
    //           showInLegend: true,
    //           data: response.data.frequencia_total_percentual,
    //         },
    //       },

    //       colors: ['#7cb5ec', '#e4d354', '#f15c80'],

    //       series: [{
    //         type: 'pie',
    //         name: 'Frequência',

    //         dataLabels: {
    //           distance: -40,
    //           format: '{point.percentage:.2f}%',
    //         },
    //       }],
    //     };

    //     this.chartDeputadoPresencaAnualOptions.xAxis.categories = response.data.frequencia_anual.categories;
    //     this.chartDeputadoPresencaAnualOptions.series = response.data.frequencia_anual.series;

    //     loaderDeputadoPresenca.hide();
    //   });

    axios
      .get(`${process.env.VUE_APP_API}/deputadoestadual/${this.id}/MaioresNotas`)
      .then((response) => {
        this.MaioresNotas = response.data;

        loaderMaioresNotas.hide();
      });

    axios
      .get(`${process.env.VUE_APP_API}/deputadoestadual/${this.id}/MaioresFornecedores`)
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
