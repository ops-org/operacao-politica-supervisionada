<template>
  <div>
    <div class="container">
      <h3 class="page-title">Senador(a) Federal</h3>

      <div class="box-senador form-group">
        <div class="row">
          <div class="col-xs-12 col-md-4 col-lg-2 text-center">
            <img
              class="img-thumbnail img-responsive img-senador"
              v-bind:src="
                '//static.ops.org.br/senador/' +
                senador.id_sf_senador +
                '_240x300.jpg'
              "
              v-bind:title="senador.nome_parlamentar"
              v-bind:alt="senador.nome_parlamentar"
              width="170"
              height="210"
            />
          </div>
          <div class="col-xs-12 col-md-8 col-lg-10 text-left">
            <h3 style="margin-top: 0">
              {{ senador.nome_parlamentar }}
              <small
                >({{ senador.sigla_partido }} /
                {{ senador.sigla_estado }})</small
              >
            </h3>

            <div class="row">
              <div class="col-sm-6">
                <p class="mb-0">
                  <strong>Nome civil:</strong> {{ senador.nome_civil }}
                </p>
                <p class="mb-0">
                  <strong>Partido:</strong> {{ senador.nome_partido }}
                </p>
                <p class="mb-0">
                  <strong>Estado:</strong> {{ senador.nome_estado }}
                </p>
                <p class="mb-0">
                  <strong>Condição:</strong> {{ senador.condicao }}
                </p>
                <p class="mb-0">
                  <strong>Naturalidade:</strong> {{ senador.naturalidade }}
                </p>
                <p class="mb-0">
                  <strong>Nascimento:</strong> {{ senador.nascimento }}
                </p>
              </div>
              <div class="col-sm-6">
                <p class="mb-0">
                  <strong>Gasto Acumulado com Pessoal: </strong>
                  <a
                    v-bind:href="
                      '/#/senado/remuneracao?ag=6&sn=' + senador.id_sf_senador
                    "
                    title="Clique para ver os gastos com pessoal em detalhes"
                    >R$ {{ senador.valor_total_remuneracao }}</a
                  >
                </p>
                <p class="mb-0">
                  <strong>Gasto Acumulado CEAPS: </strong>
                  <a
                    v-bind:href="
                      '/#/senador?IdParlamentar=' +
                      senador.id_sf_senador +
                      '&Periodo=0&Agrupamento=6'
                    "
                    title="Clique para ver os gastos com cota parlamentar em detalhes"
                    >R$ {{ senador.valor_total_ceaps }}</a
                  >
                </p>
                <p class="mb-0">
                  <a
                    v-bind:href="
                      'http://www25.senado.leg.br/web/senadores/senador/-/perfil/' +
                      senador.id_sf_senador
                    "
                    target="_blank"
                    >Ver página oficial no senado</a
                  >
                </p>
                <p class="mb-0"><strong>Email:</strong> {{ senador.email }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="row">
        <div class="col-xs-12 col-sm-6">
          <div class="card mb-3">
            <div class="card-header bg-light">
              Gastos anuais com a cota parlamentar (CEAPS)
            </div>
            <div class="card-body chart-senador-presenca">
              <highcharts
                :options="chartSenadorGastosPorAnoOptions"
                ref="chartSenadorGastosPorAno"
              ></highcharts>
            </div>
          </div>
        </div>
        <div class="col-xs-12 col-sm-6">
          <div class="card mb-3">
            <div class="card-header bg-light">
              Gastos anuais com verba de gabinete (Secretários Parlamentares)
            </div>
            <div
              class="card-body chart-senador-gastos-por-mes vld-parent"
              ref="SenadorGastosComPessoalPorAno"
            >
              <highcharts
                :options="chartSenadorGastosComPessoalPorAnoOptions"
                ref="chartSenadorGastosComPessoalPorAno"
              ></highcharts>
            </div>
          </div>
        </div>
      </div>

      <div class="row form-group">
        <div class="col-xs-12 col-sm-6">
          <div class="card mb-3">
            <div class="card-header bg-light">
              <a
                class="float-right"
                v-bind:href="
                  '/#/senador?IdParlamentar=' +
                  senador.id_sf_senador +
                  '&Periodo=0&Agrupamento=3'
                "
                >Ver lista completa</a
              >
              Maiores fornecedores
            </div>
            <div class="card-body maiores-fornecedores">
              <div class="table-responsive">
                <table
                  class="table table-striped table-hover table-sm"
                  style="margin: 0"
                  aria-label=""
                >
                  <thead>
                    <tr>
                      <th style="width: 80%">Fornecedor</th>
                      <th style="width: 20%" class="text-right">Valor</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr
                      v-for="row in MaioresFornecedores"
                      :key="row.id_fornecedor"
                    >
                      <td>
                        <a v-bind:href="'/#/fornecedor/' + row.id_fornecedor">{{
                          row.nome_fornecedor
                        }}</a>
                      </td>
                      <td class="text-right">
                        <a
                          v-bind:href="
                            '/#/senador?IdParlamentar=' +
                            senador.id_sf_senador +
                            '&Fornecedor=' +
                            row.id_fornecedor +
                            '&Periodo=0&Agrupamento=6'
                          "
                          >{{ row.valor_total }}</a
                        >
                      </td>
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
              <a
                class="float-right"
                v-bind:href="
                  '/#/senador?IdParlamentar=' +
                  senador.id_sf_senador +
                  '&Periodo=0&Agrupamento=6'
                "
                >Ver lista completa</a
              >

              Maiores Notas/Recibos
            </div>
            <div class="card-body maiores-notas">
              <div class="table-responsive">
                <table
                  class="table table-striped table-hover table-sm"
                  style="margin: 0"
                  aria-label=""
                >
                  <thead>
                    <tr>
                      <th style="width: 80%">Fornecedor</th>
                      <th style="width: 20%" class="text-right">Valor</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="row in MaioresNotas" :key="row.id_sf_despesa">
                      <td>
                        <a v-bind:href="'/#/fornecedor/' + row.id_fornecedor">{{
                          row.nome_fornecedor
                        }}</a>
                      </td>
                      <td class="text-right">{{ row.valor }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { Chart } from "highcharts-vue";

const axios = require("axios");

export default {
  name: "SenadorFederalDetalhes",
  components: {
    highcharts: Chart,
  },
  props: {
    id: Number,
  },
  data() {
    return {
      API: "",
      senador: {},
      MaioresNotas: {},
      MaioresFornecedores: {},

      chartSenadorGastosPorAnoOptions: {
        chart: {
          type: "bar",
        },

        title: {
          text: null, // 'Gasto mensal com a cota parlamentar'
        },

        xAxis: {
          categories: [],
        },

        yAxis: [
          {
            // left y axis
            title: {
              text: "Valor (em reais)",
            },
            labels: {
              format: "{value:.,2f}",
              overflow: "justify",
            },
            showFirstLabel: false,
          },
        ],

        legend: {
          enabled: false,
        },

        tooltip: {
          pointFormat:
            '<span style="color:{series.color}">\u25CF</span> {series.name}: <b>{point.y}</b><br/>',
          shared: true,
          crosshairs: true,
        },

        series: [
          {
            pointWidth: 20,
            name: "Valor (em reais)",
            data: [],
            dataLabels: {
              enabled: true,
              // rotation: -90,
              color: "#000",
              align: "right",
              format: "{point.y:,.2f}", // one decimal
              y: -1, // -1 pixels down from the top
              style: {
                fontSize: "13px",
                fontFamily: "Verdana, sans-serif",
              },
            },
          },
        ],
      },

      chartSenadorGastosComPessoalPorAnoOptions: {
        chart: {
          type: "bar",
        },

        title: {
          text: null, // 'Gasto mensal com a cota parlamentar'
        },

        xAxis: {
          categories: [],
        },

        yAxis: [
          {
            // left y axis
            title: {
              text: "Valor (em reais)",
            },
            labels: {
              format: "{value:.,2f}",
              overflow: "justify",
            },
            showFirstLabel: false,
          },
        ],

        legend: {
          enabled: false,
        },

        tooltip: {
          pointFormat:
            '<span style="color:{series.color}">\u25CF</span> {series.name}: <b>{point.y}</b><br/>',
          shared: true,
          crosshairs: true,
        },

        series: [
          {
            pointWidth: 20,
            name: "Valor (em reais)",
            data: [],
            dataLabels: {
              enabled: true,
              // rotation: -90,
              color: "#000",
              align: "right",
              format: "{point.y:,.2f}", // one decimal
              y: -1, // -1 pixels down from the top
              style: {
                fontSize: "13px",
                fontFamily: "Verdana, sans-serif",
              },
            },
          },
        ],
      },
    };
  },
  mounted() {
    window.document.title = "OPS  Senador";
    this.API = process.env.VUE_APP_API;
    // const chartSenadorGastosPorAno = this.$refs.chartSenadorGastosPorAno.chart;
    // const chartSenadorPresencaPercentual = this.$refs.chartSenadorPresencaPercentual.chart;
    // const chartSenadorPresencaAnual = this.$refs.chartSenadorPresencaAnual.chart;

    axios
      .get(`${process.env.VUE_APP_API}/senador/${this.id}`)
      .then((response) => {
        this.senador = response.data;

        window.document.title = `OPS :: Senador - ${response.data.nome_parlamentar}`;
      });

    axios
      .get(`${process.env.VUE_APP_API}/senador/${this.id}/GastosPorAno`)
      .then((response) => {
        this.chartSenadorGastosPorAnoOptions.series[0].data =
          response.data.series;
        this.chartSenadorGastosPorAnoOptions.xAxis.categories =
          response.data.categories;
      });

    axios
      .get(`${process.env.VUE_APP_API}/senador/${this.id}/MaioresNotas`)
      .then((response) => {
        this.MaioresNotas = response.data;
      });

    axios
      .get(`${process.env.VUE_APP_API}/senador/${this.id}/MaioresFornecedores`)
      .then((response) => {
        this.MaioresFornecedores = response.data;
      });

    const loaderSenadorGastosComPessoalPorAno = this.$loading.show({
      fullPage: false,
      container: this.$refs.SenadorGastosComPessoalPorAno,
    });

    axios
      .get(`${process.env.VUE_APP_API}/senador/${this.id}/GastosComPessoalPorAno`)
      .then((response) => {
        this.chartSenadorGastosComPessoalPorAnoOptions.series[0].data =
          response.data.series;
        this.chartSenadorGastosComPessoalPorAnoOptions.xAxis.categories =
          response.data.categories;

          loaderSenadorGastosComPessoalPorAno.hide();
      });
  },
};
</script>

<style scoped>
.chart-senador-presenca,
.chart-senador-gastos-por-mes {
  min-height: 440px;
}
.maiores-notas,
.maiores-fornecedores {
  min-height: 100px;
}
</style>
