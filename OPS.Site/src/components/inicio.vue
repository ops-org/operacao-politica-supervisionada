/* eslint-disable no-unused-vars */
/* eslint-disable func-names */
<template>
  <div class="bg-landing-page">
    <div class="intro-header">
      <div class="container">
        <div class="row">
          <div class="col-sm-12 form-group">
            <h1>Operação Política Supervisionada</h1>
            <h3>Indexador de dados públicos da cota parlamentar</h3>
          </div>
          <div class="col-sm-12 form-group align-content-center">
            <form
              class="form-inline form-home"
              role="search"
              method="get"
              v-on:submit.prevent="buscar()"
            >
              <div class="input-group mb-3" style="margin: 0 auto;">
                <input
                  type="text"
                  class="form-control"
                  placeholder="Buscar deputado, senador ou empresa"
                  name="q"
                  v-model="q"
                />
                <div class="input-group-append">
                  <button class="btn btn-outline-secondary" type="submit">
                    <i class="fa fa-search"></i>
                  </button>
                </div>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>

    <div class="container form-group">
      <h2 class="page-title">Resumo anual da cota parlamentar</h2>
      <div class="row">
        <div class="col-sm-6">
          <h4>
            Câmara dos Deputados
            <small>(513 deputados)</small>
          </h4>
          <div class="chart-resumo-anual vld-parent" ref="CamaraResumoAnual">
            <highcharts :options="chartCamaraResumoAnualOptions" ref="chartCamaraResumoAnual"></highcharts>
          </div>
        </div>
        <div class="col-sm-6">
          <h4>
            Senado Federal
            <small>(81 senadores)</small>
          </h4>
          <div class="chart-resumo-anual vld-parent" ref="SenadoResumoAnual">
            <highcharts :options="chartSenadoResumoAnualOptions" ref="chartSenadoResumoAnual"></highcharts>
          </div>
        </div>
      </div>
    </div>

    <div class="container form-group">
      <h2 class="page-title">Resumo mensal da cota parlamentar</h2>

      <h4>
        Câmara Federal
        <small>(513 deputados)</small>
      </h4>
      <div class="chart-resumo-mensal vld-parent" ref="CamaraResumoMensal">
        <label>Legislatura:</label>
        <div id="camara-legislatura" class="btn-group">
          <label class="btn btn-light">
            <input type="radio" name="cl-options" value="0" v-model.number="camaraLegislatura" /> Todas
          </label>
          <label class="btn btn-light">
            <input type="radio" name="cl-options" value="53" v-model.number="camaraLegislatura" /> 53º (2007-2011)
          </label>
          <label class="btn btn-light">
            <input type="radio" name="cl-options" value="54" v-model.number="camaraLegislatura" /> 54º (2011-2015)
          </label>
          <label class="btn btn-light">
            <input type="radio" name="cl-options" value="55" v-model.number="camaraLegislatura" /> 55º (2015-2019)
          </label>
          <label class="btn btn-light">
            <input type="radio" name="cl-options" value="56" v-model.number="camaraLegislatura" /> 56º (2019-2023)
          </label>
        </div>
        <highcharts :options="chartCamaraResumoMensalOptions" ref="chartCamaraResumoMensal"></highcharts>
      </div>

      <h4>
        Senado Federal
        <small>(81 senadores)</small>
      </h4>
      <div class="chart-resumo-mensal vld-parent" ref="SenadoResumoMensal">
        <label>Legislatura:</label>
        <div id="senado-legislatura" class="btn-group">
          <label class="btn btn-light">
            <input type="radio" name="sl-options" value="0" v-model.number="senadoLegislatura" /> Todas
          </label>
          <label class="btn btn-light">
            <input type="radio" name="sl-options" value="52" v-model.number="senadoLegislatura" /> 52º (2007-2011)
          </label>
          <label class="btn btn-light">
            <input type="radio" name="sl-options" value="53" v-model.number="senadoLegislatura" /> 53º (2011-2015)
          </label>
          <label class="btn btn-light">
            <input type="radio" name="sl-options" value="54" v-model.number="senadoLegislatura" /> 54º (2015-2019)
          </label>
          <label class="btn btn-light">
            <input type="radio" name="sl-options" value="55" v-model.number="senadoLegislatura" /> 55º (2019-2023)
          </label>
        </div>
        <highcharts :options="chartcSenadoResumoMensalOptions" ref="chartSenadoResumoMensal"></highcharts>
      </div>
    </div>

    <div class="content-section form-group">
      <div class="container text-justify">
        <h2 class="page-title">O que é a cota parlamentar?</h2>
        <p>
          A
          <strong>cota parlamentar</strong>, também conhecida como verba indenizatória é um
          <strong>recurso financeiro público</strong> disponibilizado a todos os
          <strong>deputados federais e senadores</strong> para o custeio de seus mandatos.
        </p>
        <p>
          Cada deputado federal tem direito a restituir até
          <strong>
            <a
              href="http://www.camara.gov.br/cota-parlamentar/ANEXO_ATO_DA_MESA_43_2009.pdf"
              target="_blank"
              rel="nofollow"
              title="Clique para visualizar a lista oficial de valores por estado"
            >R$ 45.612,53 por mês</a>
          </strong> com sua
          <strong>alimentação, passagens aereas, combustivel, consultoria</strong>, entre outros.
        </p>
        <p>
          No Senado o valor chega a
          <strong>
            <a
              href="https://www12.senado.leg.br/transparencia/leg/pdf/CotaExercicioAtivParlamSenadores.pdf"
              target="_blank"
              rel="nofollow"
              title="Clique para visualizar a lista official de valores por estado"
            >R$ 44.276,60 por mês.</a>
          </strong>
        </p>
        <p>
          Contrariando a lei de licitações, o uso deste dinheiro é feito sem licitação e
          <strong>basta o parlamentar apresentar a nota fiscal ou recibo</strong>.
        </p>
      </div>
    </div>

    <div class="container form-group">
      <h2 class="page-title">Campeões de gastos</h2>
      <p>
        Os Parlamentares que mais gastaram dinheiro público
        da verba indenizatória da atual legislatura
      </p>
      <br />
      <div class="resumo-gastos vld-parent" ref="CampeoesGastos">
        <h4>
          Deputados Federais
          <small>(Desde fevereiro de 2019)</small>
        </h4>
        <div class="row">
          <div
            class="col-xs-12 col-sm-6 col-md-3"
            v-for="gasto in CampeoesGastos.camara_federal"
            :key="gasto.id_cf_deputado"
          >
            <a
              v-bind:href="'./deputado-federal/' + gasto.id_cf_deputado"
              title="Clique para visualizar o perfil do deputado(a)"
            >
              <div class="card mb-3">
                <div class="row no-gutters">
                  <div class="col-xs-4">
                    <img
                      v-lazy="API + '/deputado/imagem/' + gasto.id_cf_deputado + '_120x160'"
                      v-bind:alt="gasto.nome_parlamentar"
                      class="card-img"
                    />
                  </div>
                  <div class="col-xs-8">
                    <div class="card-body">
                      <h6 class="card-title text-truncate">{{gasto.nome_parlamentar}}</h6>
                      <p class="card-text">
                        {{gasto.sigla_partido_estado}}
                        <br />
                        {{gasto.valor_total}}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </a>
          </div>
        </div>
        <br />
        <h4>
          Senadores
          <small>(Desde fevereiro de 2019)</small>
        </h4>
        <div class="row">
          <div
            class="col-xs-12 col-sm-6 col-md-3"
            v-for="gasto in CampeoesGastos.senado"
            :key="gasto.id_sf_senador"
          >
            <a
              v-bind:href="'./senador/' + gasto.id_sf_senador"
              title="Clique para visualizar o perfil do senador(a)"
            >
              <div class="card mb-3">
                <div class="row no-gutters">
                  <div class="col-sx-4">
                    <img
                      v-lazy="API + '/senador/imagem/' + gasto.id_sf_senador + '_120x160'"
                      v-bind:alt="gasto.nome_parlamentar"
                      class="card-img"
                    />
                  </div>
                  <div class="col-sx-8">
                    <div class="card-body">
                      <h6 class="card-title text-truncate">{{gasto.nome_parlamentar}}</h6>
                      <p class="card-text">
                        {{gasto.sigla_partido_estado}}
                        <br />
                        {{gasto.valor_total}}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </a>
          </div>
        </div>
      </div>
    </div>
    <br />

    <div class="content-section form-group">
      <div class="container text-center pt-4 pb-3">
        <h2>Escolha uma casa legislativa para explorar</h2>
        <div class="text-xs-center">
          <a
            href="/deputado-federal"
            class="btn btn-danger btn-lg"
            style="margin-bottom: 15px;"
          >Câmara dos Deputados</a>
          <a
            href="/senador"
            class="btn btn-danger btn-lg"
            style="margin-bottom: 15px;"
          >Senado Federal</a>
        </div>
      </div>
    </div>

    <div class="container">
      <Disqus shortname='ops-net-br' :pageConfig="pageConfig" />
    </div>
  </div>
</template>

<script>
import { Chart } from 'highcharts-vue';
import { Disqus } from 'vue-disqus';

const axios = require('axios');

export default {
  name: 'Inicio',
  components: {
    highcharts: Chart,
    Disqus,
  },
  props: {
    q: String,
  },
  data() {
    return {
      API: '',
      pageConfig: {
        url: 'http://ops.net.br',
        identifier: 'Pagina Inicial',
      },
      camaraLegislatura: 56,
      senadoLegislatura: 55,
      CampeoesGastos: [],
      chartCamaraResumoMensalOptions: {
        chart: {
          type: 'column',
        },

        title: {
          text: null, // 'Gasto mensal com a cota parlamentar'
        },

        xAxis: {
          categories: [
            'Jan',
            'Fev',
            'Mar',
            'Abr',
            'Maio',
            'Jun',
            'Jul',
            'Ago',
            'Set',
            'Out',
            'Nov',
            'Dez',
          ],
        },

        yAxis: [
          {
            // left y axis
            title: {
              text: 'Valor (em reais)',
            },
            labels: {
              align: 'left',
              x: 3,
              y: 16,
              format: '{value:.,0f}',
            },
            showFirstLabel: false,
          },
        ],

        tooltip: {
          shared: true,
          crosshairs: true,
          pointFormat:
            '<span style=color:{point.color}">\u25CF</span> {series.name}: <b class="legend">{point.y:.,2f}</b><br/>',
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
            fontFamily: 'Verdana, sans-serif',
          },
        },

        series: [],
      },
      chartcSenadoResumoMensalOptions: {
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
            align: 'left',
            x: 3,
            y: 16,
            format: '{value:.,0f}',
          },
          showFirstLabel: false,
        }],

        tooltip: {
          shared: true,
          crosshairs: true,
          pointFormat: '<span style=color:{point.color}">\u25CF</span> {series.name}: <b class="legend">{point.y:.,2f}</b><br/>',
        },

        series: [],
      },
      chartCamaraResumoAnualOptions: {
        chart: {
          type: 'bar',
          // height: 500,
        },

        title: {
          text: null,
        },

        xAxis:
        {
          categories: [],
        },

        yAxis: [{ // left y axis
          title: {
            text: 'Valor (em reais)',
          },
          // labels: {
          //    align: 'left',
          //    x: 3,
          //    y: 16,
          //    format: '{value:,.0f}'
          // },
          showFirstLabel: false,
        }],

        legend: {
          enabled: false,
        },

        tooltip: {
          shared: true,
          crosshairs: true,
          pointFormat: '<span style=color:{point.color}">\u25CF</span> {series.name}: <b class="legend">{point.y:,.2f}</b><br/>',
        },

        series: [{
          pointWidth: 20,
          name: 'Câmara',
          data: [],
          dataLabels: {
            enabled: true,
            // rotation: -90,
            color: '#FFFFFF',
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
      chartSenadoResumoAnualOptions: {
        chart: {
          type: 'bar',
        },

        title: {
          text: null,
        },

        xAxis:
        {
          categories: [],
        },

        yAxis: [{ // left y axis
          title: {
            text: 'Valor (em reais)',
          },
          // labels: {
          //    align: 'left',
          //    x: 3,
          //    y: 16,
          //    format: '{value:,.0f}'
          // },
          showFirstLabel: false,
        }],

        legend: {
          enabled: false,
        },

        tooltip: {
          shared: true,
          crosshairs: true,
          pointFormat: '<span style=color:{point.color}">\u25CF</span> {series.name}: <b class="legend">{point.y:,.2f}</b><br/>',
        },

        series: [{
          name: 'Senado',
          pointWidth: 20,
          data: [],
          dataLabels: {
            enabled: true,
            // rotation: -90,
            color: '#FFFFFF',
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
    };
  },
  mounted() {
    const chartCamaraResumoMensal = this.$refs.chartCamaraResumoMensal.chart;
    const chartSenadoResumoMensal = this.$refs.chartSenadoResumoMensal.chart;
    this.API = process.env.API;

    const loaderCampeoesGastos = this.$loading.show({
      fullPage: false,
      container: this.$refs.CampeoesGastos,
    });

    const loaderCamaraResumoMensal = this.$loading.show({
      fullPage: false,
      container: this.$refs.CamaraResumoMensal,
    });

    const loaderSenadoResumoMensal = this.$loading.show({
      fullPage: false,
      container: this.$refs.SenadoResumoMensal,
    });

    const loaderCamaraResumoAnual = this.$loading.show({
      fullPage: false,
      container: this.$refs.CamaraResumoAnual,
    });

    const loaderSenadoResumoAnual = this.$loading.show({
      fullPage: false,
      container: this.$refs.SenadoResumoAnual,
    });

    axios
      .get(`${process.env.API}/inicio/parlamentarresumogastos`)
      .then((response) => {
        this.CampeoesGastos = response.data;

        loaderCampeoesGastos.hide();
      });

    axios
      .get(`${process.env.API}/deputado/camararesumomensal`)
      .then((response) => {
        this.chartCamaraResumoMensalOptions.series = response.data;

        this.$nextTick(() => {
          for (let i = 0; i <= response.data.length - 3; i += 1) {
            chartCamaraResumoMensal.series[i].hide();
          }

          loaderCamaraResumoMensal.hide();
        });
      });

    axios
      .get(`${process.env.API}/senador/senadoresumomensal`)
      .then((response) => {
        this.chartcSenadoResumoMensalOptions.series = response.data;

        this.$nextTick(() => {
          for (let i = 0; i <= response.data.length - 3; i += 1) {
            chartSenadoResumoMensal.series[i].hide();
          }

          loaderSenadoResumoMensal.hide();
        });
      });

    axios
      .get(`${process.env.API}/deputado/camararesumoanual`)
      .then((response) => {
        this.chartCamaraResumoAnualOptions.series[0].data = response.data.series;
        this.chartCamaraResumoAnualOptions.xAxis.categories = response.data.categories;

        loaderCamaraResumoAnual.hide();
      });

    axios
      .get(`${process.env.API}/senador/senadoresumoanual`)
      .then((response) => {
        this.chartSenadoResumoAnualOptions.series[0].data = response.data.series;
        this.chartSenadoResumoAnualOptions.xAxis.categories = response.data.categories;

        loaderSenadoResumoAnual.hide();
      });
  },
  methods: {
    buscar() {
      this.$router.push(`/busca?q=${this.q || ''}`);
    },
  },
  watch: {
    camaraLegislatura() {
      const chart = this.$refs.chartCamaraResumoMensal.chart;

      switch (this.camaraLegislatura) {
        case 56:
          chart.series[0].hide(); // 2009
          chart.series[1].hide(); // 2010
          chart.series[2].hide(); // 2011
          chart.series[3].hide(); // 2012
          chart.series[4].hide(); // 2013
          chart.series[5].hide(); // 2014
          chart.series[6].hide(); // 2015
          chart.series[7].hide(); // 2016
          chart.series[8].hide(); // 2017
          chart.series[9].hide(); // 2018
          chart.series[10].show(); // 2019
          break;
        case 55:
          chart.series[0].hide(); // 2009
          chart.series[1].hide(); // 2010
          chart.series[2].hide(); // 2011
          chart.series[3].hide(); // 2012
          chart.series[4].hide(); // 2013
          chart.series[5].hide(); // 2014
          chart.series[6].show(); // 2015
          chart.series[7].show(); // 2016
          chart.series[8].show(); // 2017
          chart.series[9].show(); // 2018
          chart.series[10].hide(); // 2019
          break;
        case 54:
          chart.series[0].hide(); // 2009
          chart.series[1].hide(); // 2010
          chart.series[2].show(); // 2011
          chart.series[3].show(); // 2012
          chart.series[4].show(); // 2013
          chart.series[5].show(); // 2014
          chart.series[6].hide(); // 2015
          chart.series[7].hide(); // 2016
          chart.series[8].hide(); // 2017
          chart.series[9].hide(); // 2018
          chart.series[10].hide(); // 2019
          break;
        case 53:
          chart.series[0].show(); // 2009
          chart.series[1].show(); // 2010
          chart.series[2].hide(); // 2011
          chart.series[3].hide(); // 2012
          chart.series[4].hide(); // 2013
          chart.series[5].hide(); // 2014
          chart.series[6].hide(); // 2015
          chart.series[7].hide(); // 2016
          chart.series[8].hide(); // 2017
          chart.series[9].hide(); // 2018
          chart.series[10].hide(); // 2019
          break;
        default:
          for (let i = 0; i < chart.series.length; i += 1) {
            chart.series[i].show();
          }
          break;
      }
    },
    senadoLegislatura() {
      const chart = this.$refs.chartSenadoResumoMensal.chart;

      switch (this.senadoLegislatura) {
        case 55:
          chart.series[0].hide(); // 2008
          chart.series[1].hide(); // 2009
          chart.series[2].hide(); // 2010
          chart.series[3].hide(); // 2011
          chart.series[4].hide(); // 2012
          chart.series[5].hide(); // 2013
          chart.series[6].hide(); // 2014
          chart.series[7].hide(); // 2015
          chart.series[8].hide(); // 2016
          chart.series[9].hide(); // 2017
          chart.series[10].hide(); // 2018
          chart.series[11].show(); // 2019
          break;
        case 54:
          chart.series[0].hide(); // 2008
          chart.series[1].hide(); // 2009
          chart.series[2].hide(); // 2010
          chart.series[3].hide(); // 2011
          chart.series[4].hide(); // 2012
          chart.series[5].hide(); // 2013
          chart.series[6].hide(); // 2014
          chart.series[7].show(); // 2015
          chart.series[8].show(); // 2016
          chart.series[9].show(); // 2017
          chart.series[10].show(); // 2018
          chart.series[11].hide(); // 2019
          break;
        case 53:
          chart.series[0].hide(); // 2008
          chart.series[1].hide(); // 2009
          chart.series[2].hide(); // 2010
          chart.series[3].show(); // 2011
          chart.series[4].show(); // 2012
          chart.series[5].show(); // 2013
          chart.series[6].show(); // 2014
          chart.series[7].hide(); // 2015
          chart.series[8].hide(); // 2016
          chart.series[9].hide(); // 2017
          chart.series[10].hide(); // 2018
          chart.series[11].hide(); // 2019
          break;
        case 52:
          chart.series[0].show(); // 2008
          chart.series[1].show(); // 2009
          chart.series[2].show(); // 2010
          chart.series[3].hide(); // 2011
          chart.series[4].hide(); // 2012
          chart.series[5].hide(); // 2013
          chart.series[6].hide(); // 2014
          chart.series[7].hide(); // 2015
          chart.series[8].hide(); // 2016
          chart.series[9].hide(); // 2017
          chart.series[10].hide(); // 2018
          chart.series[11].hide(); // 2019
          break;
        default:
          for (let i = 0; i < chart.series.length; i += 1) {
            chart.series[i].show();
          }
          break;
      }
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
  .card-img{
    width: 90px;
    height: 113px;
  }

  .chart-resumo-mensal, .chart-resumo-anual{
    min-height: 400px;
  }

  .resumo-gastos{
    min-height: 358px;
  }
</style>
