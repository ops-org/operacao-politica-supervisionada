/* eslint-disable no-unused-vars */ /* eslint-disable func-names */
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
                        <form class="form-inline form-home"
                              role="search"
                              method="get"
                              v-on:submit.prevent="buscar()">
                            <div class="input-group mb-3" style="margin: 0 auto">
                                <input type="text"
                                       class="form-control"
                                       placeholder="Buscar deputado, senador ou empresa"
                                       name="q"
                                       v-model="q" />
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
                        <highcharts :options="chartCamaraResumoAnualOptions"
                                    ref="chartCamaraResumoAnual"></highcharts>
                    </div>
                </div>
                <div class="col-sm-6">
                    <h4>
                        Senado Federal
                        <small>(81 senadores)</small>
                    </h4>
                    <div class="chart-resumo-anual vld-parent" ref="SenadoResumoAnual">
                        <highcharts :options="chartSenadoResumoAnualOptions"
                                    ref="chartSenadoResumoAnual"></highcharts>
                    </div>
                </div>
            </div>
        </div>

    <!-- <div class="container form-group">
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
    </div> -->

    <div class="content-section form-group">
      <div class="container text-justify">
        <h2 class="page-title">O que é a cota parlamentar?</h2>
        <p>
          A
          <strong>cota parlamentar</strong>, também conhecida como verba indenizatória é
          um <strong>recurso financeiro público</strong> disponibilizado a todos os
          <strong>deputados federais e senadores</strong> para o custeio de seus mandatos.
        </p>
        <p>
          Cada deputado federal tem a seu dispor valores mensais que vão de
                    <a href="http://www.camara.leg.br/cota-parlamentar/ANEXO_ATO_DA_MESA_43_2009.pdf"
            target="_blank"
            rel="nofollow noopener noreferrer"
                        title="Clique para visualizar a lista oficial de valores por estado"><strong>R$ 36,6 mil a R$ 51,4 mil</strong></a>,
                    a depender do estado de origem, para custear despesas com alimentação, viagens, hospedagens, combustível,
                    serviços de consultoria, locação de carros, barcos, aviões e casas, além de muitos outras.
        </p>
        <p>
          No Senado os valores também variam de acordo com o estado que elegeu o
          parlamentar e partem

                    <a href="https://www12.senado.leg.br/transparencia/leg/pdf/CotaExercicioAtivParlamSenadores.pdf"
                       target="_blank"
                       rel="nofollow noopener noreferrer"
                       title="Clique para visualizar a lista official de valores por estado"><strong>de R$ 21 mil até R$ 44,2 mil por mês</strong></a>.
                </p>
                <p>
                    Para ter este recurso público liberado pela Câmara ou Senado, o
                    parlamentar precisa apenas apresentar a nota fiscal ou recibo da
                    despesa. Não é feita licitação, tomada de preço ou qualquer outro
                    recurso legal que permita a utilização de dinheiro público de forma
                    eficiente, econômica, legal, impessoal e moral. Muitos são as
                    <a href="https://institutoops.org.br/o-que-ja-fizemos/"
                       target="_blank"
                       rel="nofollow noopener noreferrer"><strong>irregularidades já descobertas pela OPS</strong></a>.
                </p>
            </div>
        </div>

        <div class="container form-group">
            <h2 class="page-title">Campeões de gastos</h2>
            <p>
                Os Parlamentares que mais gastaram dinheiro público da verba
                indenizatória da atual legislatura
            </p>
            <br />
            <div class="resumo-gastos vld-parent" ref="CampeoesGastos">
                <h4>
                    Deputados Federais
                    <small>(Desde fevereiro de 2023)</small>
                </h4>
                <div class="row">
                    <div class="col-xs-12 col-sm-6 col-md-3"
                         v-for="gasto in CampeoesGastos.camara_federal"
                         :key="gasto.id_cf_deputado">
                        <a v-bind:href="'/#/deputado-federal/' + gasto.id_cf_deputado"
                           title="Clique para visualizar o perfil do deputado(a)">
                            <div class="card mb-3">
                                <div class="row no-gutters">
                                    <div class="col-xs-4">
                                        <img v-lazy="
                        '//static.ops.org.br/depfederal/' +
                        gasto.id_cf_deputado +
                        '_120x160.jpg'
                      "
                                             v-bind:alt="gasto.nome_parlamentar"
                                             class="card-img" />
                                    </div>
                                    <div class="col-xs-8">
                                        <div class="card-body">
                                            <h6 class="card-title text-truncate">
                                                {{ gasto.nome_parlamentar }}
                                            </h6>
                                            <p class="card-text">
                                                {{ gasto.sigla_partido_estado }}
                                                <br />
                                                {{ gasto.valor_total }}
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
                    <small>(Desde fevereiro de 2023)</small>
                </h4>
                <div class="row">
                    <div class="col-xs-12 col-sm-6 col-md-3"
                         v-for="gasto in CampeoesGastos.senado"
                         :key="gasto.id_sf_senador">
                        <a v-bind:href="'/#/senador/' + gasto.id_sf_senador"
                           title="Clique para visualizar o perfil do senador(a)">
                            <div class="card mb-3">
                                <div class="row no-gutters">
                                    <div class="col-sx-4">
                                        <img v-lazy="
                        '//static.ops.org.br/senador/' +
                        gasto.id_sf_senador +
                        '_120x160.jpg'
                      "
                                             v-bind:alt="gasto.nome_parlamentar"
                                             class="card-img" />
                                    </div>
                                    <div class="col-sx-8">
                                        <div class="card-body">
                                            <h6 class="card-title text-truncate">
                                                {{ gasto.nome_parlamentar }}
                                            </h6>
                                            <p class="card-text">
                                                {{ gasto.sigla_partido_estado }}
                                                <br />
                                                {{ gasto.valor_total }}
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
                    <a href="/#/senador"
                       class="btn btn-danger btn-lg"
                       style="margin-bottom: 15px">Senado Federal</a>&nbsp;
                    <a href="/#/deputado-federal"
                       class="btn btn-danger btn-lg"
                       style="margin-bottom: 15px">Câmara dos Deputados</a>&nbsp;
                    <a href="/#/deputado-estadual"
                       class="btn btn-danger btn-lg"
                       style="margin-bottom: 15px">Assembleias Legislativas</a>
                </div>
            </div>
        </div>

        <div class="container"></div>
    </div>
</template>

<script>
    import { Chart } from "highcharts-vue";

    const axios = require("axios");

    export default {
      name: "Inicio",
      components: {
        highcharts: Chart,
      },
      props: {
        q: String,
      },
      data() {
        return {
          API: "",
          pageConfig: {
            url: "http://ops.org.br",
            identifier: "Pagina Inicial",
          },
          CampeoesGastos: [],
          chartCamaraResumoAnualOptions: {
            chart: {
              type: "bar",
            },

            title: {
              text: null,
            },

            xAxis: {
              categories: [],
            },

            yAxis: [
              {
                title: {
                  text: "Valor (em reais)",
                },
                showFirstLabel: false,
              },
            ],

            legend: {
              enabled: false,
            },

            tooltip: {
              shared: true,
              crosshairs: true,
              pointFormat:
                '<span style=color:{point.color}">\u25CF</span> {series.name}: <b class="legend">{point.y:,.2f}</b><br/>',
            },

            series: [
              {
                pointWidth: 20,
                name: "Câmara",
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
          chartSenadoResumoAnualOptions: {
            chart: {
              type: "bar",
            },

            title: {
              text: null,
            },

            xAxis: {
              categories: [],
            },

            yAxis: [
              {
                title: {
                  text: "Valor (em reais)",
                },
                showFirstLabel: false,
              },
            ],

            legend: {
              enabled: false,
            },

            tooltip: {
              shared: true,
              crosshairs: true,
              pointFormat:
                '<span style=color:{point.color}">\u25CF</span> {series.name}: <b class="legend">{point.y:,.2f}</b><br/>',
            },

            series: [
              {
                name: "Senado",
                pointWidth: 20,
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
        this.API = process.env.VUE_APP_API;

        const loaderCampeoesGastos = this.$loading.show({
          fullPage: false,
          container: this.$refs.CampeoesGastos,
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
          .get(`${process.env.VUE_APP_API}/inicio/parlamentarresumogastos`)
          .then((response) => {
            this.CampeoesGastos = response.data;

            loaderCampeoesGastos.hide();
          });

        axios
          .get(`${process.env.VUE_APP_API}/deputado/camararesumoanual`)
          .then((response) => {
            this.chartCamaraResumoAnualOptions.series[0].data =
              response.data.series;
            this.chartCamaraResumoAnualOptions.xAxis.categories =
              response.data.categories;

            loaderCamaraResumoAnual.hide();
          });

        axios
          .get(`${process.env.VUE_APP_API}/senador/senadoresumoanual`)
          .then((response) => {
            this.chartSenadoResumoAnualOptions.series[0].data =
              response.data.series;
            this.chartSenadoResumoAnualOptions.xAxis.categories =
              response.data.categories;

            loaderSenadoResumoAnual.hide();
          });
      },
      methods: {
        buscar() {
          this.$router.push(`/busca?q=${this.q || ""}`);
        },
      },
    };
</script>

<style scoped>
    .card-img {
        width: 90px;
        height: 113px;
    }

    .chart-resumo-mensal,
    .chart-resumo-anual {
        min-height: 400px;
    }

    .resumo-gastos {
        min-height: 358px;
    }
</style>
