<template>
  <div>
    <div class="container vld-parent" ref="Detalhes">
      <div class="form-group" v-if="fornecedor.genero==='nd'">
        <h2 class="page-title">Dados do Fornecedor</h2>
        <div class="col-md-12">
          <p class="mb-1">
            <strong>Nome:</strong>
            {{fornecedor.nome}}
          </p>
        </div>
      </div>

      <div id="fsDadosReceita" class="form-group" v-if="fornecedor.genero==='pf'">
        <h2 class="page-title">Dados do Fornecedor</h2>

        <div class="row">
          <div class="col-md-4">
            <p class="mb-1">
              <strong>CPF:</strong>
              {{fornecedor.cnpj_cpf}}
            </p>
          </div>
          <div class="col-md-4">
            <p class="mb-1">
              <strong>Nome:</strong>
              {{fornecedor.nome}}
            </p>
          </div>
          <div class="col-md-4">
            <p class="mb-1">
              <strong>Doador de campanha:</strong>
              <span>{{fornecedor.doador == 1 ? 'Sim' : 'Não'}}</span>
            </p>
          </div>
        </div>
      </div>

      <div
        id="fsConsultaReceita"
        class="form-group"
        v-if="fornecedor.genero==='pj'"
        style="display: none;"
      >
        <h2 class="page-title">Consulta de CNPJ na Receita Federal</h2>
        <div class="row">
          <div class="col-md-4 col-md-offset-2">
            <div class="col-md-12">
              <div class="form-group img-captcha" style="height: 50px">
                <img id="captcha_img" title="Informe o texto da imagem" alt="Captcha" src />
                <a
                  v-on:click="BuscarCaptchaReceita()"
                  class="btn btn-warning btn-sm"
                  style="margin-left: 3px"
                  title="Carregar outra imagem"
                >
                  <i class="fa fa-refresh"></i>
                </a>
              </div>
            </div>
            <div class="col-md-12">
              <div class="form-group">
                <strong class="control-strong">Digite o Captcha</strong>
                <div class="input-group">
                  <input type="text" id="img-input" value class="form-control input-sm input-sm" />
                  <span class="input-group-btn">
                    <button
                      type="button"
                      id="buscarDados-btn"
                      class="btn btn-danger btn-sm"
                      v-on:click="ConsultarCNPJ();"
                    >Buscar</button>
                  </span>
                </div>
                <p class="bg-danger pull-left hidden msg" style="padding: 10px 20px;">
                  <i class="fa fa-exclamation-triangle"></i>&nbsp;
                  <strong>
                    <span id="msgErro-span"></span>
                  </strong>
                </p>
              </div>
            </div>
          </div>
          <div class="col-md-4">
            <p class="text-justify">
              O Captcha exibido é necessário para você consultar os dados do fornecedor na página da Receita Federal.
              <br />Após a consulta as informações do fornecedor serão salvas para as próximas consultas e para os próximos
              usuários que auditarem este fornecedor.
              <br />Caso tenha alguma dúvida entre em
              <a href="mailto:suporte@ops.net.br">contato</a>.
            </p>
          </div>
        </div>
      </div>

      <div id="fsDadosReceita" class="form-group" v-if="fornecedor.genero==='pj'">
        <h2 class="page-title">Dados do Fornecedor</h2>

        <div class="row form-group" id="dvBotoesAcao">
          <div class="col-md-12 text-center">
            <a
              href="javascript:;"
              class="btn btn-primary"
              data-toggle="modal"
              data-target="#dvQueProcurar"
            >O Que Procurar?</a>&nbsp;

            <input
              id="btnReconsultarDadosReceita"
              type="button"
              v-on:click="ReconsultarDadosReceita()"
              value="Atualizar Dados"
              title="Reconsultar dados a partir da Receita Federal"
              class="btn btn-primary btn-sm"
              style="display: none;"
            />&nbsp;

            <input
              type="button"
              v-on:click="SituacaoCadastral(fornecedor)"
              value="Situação Cadastral Atualizada"
              title="Pesquisar Fornecedor na Receita Federal"
              class="btn btn-light"
            />

            <input
              type="button"
              v-on:click="PesquisarNoMaps(fornecedor)"
              value="Pesquisar no Maps"
              title="Pesquisar Fornecedor no Maps"
              class="btn btn-light"
            />&nbsp;

            <input
              type="button"
              v-on:click="PesquisarNoGoogle(fornecedor)"
              value="Pesquisar no Google"
              title="Pesquisar Fornecedor no Google"
              class="btn btn-light"
            />

          </div>
        </div>

        <div id="dvInfoDataConsultaCNPJ" class="alert alert-warning" v-if="fornecedor.obtido_em">
          As informações abaixo foram atualizadas em {{fornecedor.obtido_em}}.
          <!--Clique <a href='javascript:void(0);' v-on:click="ReconsultarDadosReceita()">aqui</a> para atualizar.-->
        </div>

        <div class="row mb-2">
          <div class="col-md-3">
            <p class="mb-1">
              <strong>CNPJ: </strong>
              {{fornecedor.cnpj_cpf}}
            </p>
          </div>
          <div class="col-md-3">
            <p class="mb-1">
              <strong>Tipo: </strong>
              {{fornecedor.tipo}}
            </p>
          </div>
          <div class="col-md-3">
            <p class="mb-1">
              <strong>Situação Cadastral: </strong>
              <span
                v-bind:class="fornecedor.situacao_cadastral == 'ATIVA' ? 'text-success' : 'text-warning'"
              >{{fornecedor.situacao_cadastral}}</span>
            </p>
          </div>
          <div class="col-md-3">
            <p class="mb-1">
              <strong>Doador de campanha: </strong>
              <span>{{fornecedor.doador == 1 ? 'Sim' : 'Não'}}</span>
            </p>
          </div>
          <div class="col-md-12">
            <p class="mb-1">
              <strong>Razão social: </strong>
              {{fornecedor.nome}}
            </p>
          </div>
          <div class="col-md-12">
            <p class="mb-1">
              <strong>Nome fantasia: </strong>
              {{fornecedor.nome_fantasia}}
            </p>
          </div>
          <div class="col-md-12">
            <p class="mb-1">
              <strong>Endereço: </strong>
              {{fornecedor.logradouro}}, {{fornecedor.numero}} - {{fornecedor.bairro}}
              <span
                v-if="fornecedor.complemento"
              >, {{fornecedor.complemento}}</span>
              , {{fornecedor.cep}}, {{fornecedor.cidade}}, {{fornecedor.estado}}
            </p>
          </div>
          <div class="col-md-6">
            <p class="mb-1">
              <strong>Data de abertura: </strong>
              {{fornecedor.data_de_abertura}}
            </p>
          </div>
          <div class="col-md-6">
            <p class="mb-1">
              <strong>E-mail: </strong>
              {{fornecedor.endereco_eletronico}}
            </p>
          </div>
          <div class="col-md-6" v-if="fornecedor.capital_social">
            <p class="mb-1">
              <strong>Capital Social: </strong>
              R$ {{fornecedor.capital_social}}
            </p>
          </div>
          <div class="col-md-6">
            <p class="mb-1">
              <strong>Telefone: </strong>
              {{fornecedor.telefone}}
            </p>
          </div>
        </div>

        <div class="form-group text-center">
          <button
            class="btn btn-primary"
            type="button"
            v-on:click="ExpandirContrairInformacoesAdicional($event)"
          >Ver mais</button>
        </div>

        <div id="collapseDadosEmpresaAdicional" style="display:none" class="form-group">
          <div class="row">
            <div class="col-md-12">
              <div class="form-group">
                <strong>Código e descrição da atividade econômica principal (CNAE):</strong>
                <div>{{fornecedor.atividade_principal}}</div>
              </div>
            </div>
          </div>
          <div class="row">
            <div class="col-md-12">
              <div class="form-group">
                <strong>Código e descrição das atividades econômicas secundárias:</strong>
                <div v-for="atividade in fornecedor.atividade_secundaria" :key="atividade">{{atividade}}</div>
              </div>
            </div>
          </div>
          <div class="row">
            <div class="col-md-12">
              <p class="mb-1">
                <strong>Código e descrição da natureza jurídica: </strong>
                {{fornecedor.natureza_juridica}}
              </p>
            </div>
          </div>
          <div class="row">
            <div class="col-md-4">
              <p class="mb-1">
                <strong>Data da situação cadastral: </strong>
                {{fornecedor.data_da_situacao_cadastral}}
              </p>
            </div>
            <div class="col-md-8" v-if="fornecedor.motivo_situacao_cadastral">
              <p class="mb-1">
                <strong>Motivo de situação cadastral: </strong>
                {{fornecedor.motivo_situacao_cadastral}}
              </p>
            </div>
            <div class="col-md-4" v-if="fornecedor.situacao_especial">
              <p class="mb-1">
                <strong>Situação especial: </strong>
                {{fornecedor.situacao_especial}}
              </p>
            </div>
            <div class="col-md-4" v-if="fornecedor.data_situacao_especial">
              <p class="mb-1">
                <strong>Data da situação especial: </strong>
                {{fornecedor.data_situacao_especial}}
              </p>
            </div>
            <div class="col-md-4" v-if="fornecedor.ente_federativo_responsavel">
              <p class="mb-1">
                <strong>Ente federativo responsável (EFR): </strong>
                {{fornecedor.ente_federativo_responsavel}}
              </p>
            </div>
          </div>

          <fieldset id="fsQuadroSocietario" class="form-group">
            <legend>Quadro de Sócios e Administradores - QSA</legend>
            <div class="row">
              <div class="col-md-12">
                <div class="form-group">
                  <table class="table table-condensed table-striped table-sm">
                    <thead>
                      <tr>
                        <th>Nome/Nome Empresarial</th>
                        <th>Qualificação</th>
                        <th>Nome do Repres. Legal</th>
                        <th>Qualif. Rep. Legal</th>
                      </tr>
                    </thead>
                    <tbody v-if="quadro_societario.length>0">
                      <tr v-for="row in quadro_societario" :key="row.nome">
                        <td>{{row.nome}}</td>
                        <td>{{row.qualificacao}}</td>
                        <td>{{row.nome_representante_legal}}</td>
                        <td>{{row.qualificacao_representante_legal}}</td>
                      </tr>
                    </tbody>
                    <tbody v-if="quadro_societario.length===0">
                      <tr>
                        <td
                          colspan="4"
                          class="text-center"
                        >A natureza jurídica não permite o preenchimento do QSA</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </fieldset>
        </div>
      </div>
   
      <div class="row form-group">
        <div class="col-sm-12">
          <div class="card mb-3">
            <div class="card-header bg-light">
              Recebimentos por ano
            </div>
            <div class="card-body vld-parent" ref="RecebimentosPorAno">
              <highcharts :options="chartRecebimentosPorAnoOptions" ref="chartRecebimentosPorAno"></highcharts>
            </div>
          </div>
        </div>
        <div class="col-sm-12">
          <div class="card mb-3">
            <div class="card-header bg-light">
              <div class="float-right">
                Ver lista completa por
                <a v-bind:href="'/deputado-federal?Fornecedor='+fornecedor.id_fornecedor+'&Periodo=0&Agrupamento=6'">Deputado Federal</a> ou 
                <a v-bind:href="'/senador?Fornecedor='+fornecedor.id_fornecedor+'&Periodo=0&Agrupamento=6'">Senador</a>
              </div>
              Parlamentares (Top 10 Acumulado)
            </div>
            <div class="card-body vld-parent" ref="MaioresGastos">
              <div class="table-responsive">
                <table class="table table-striped table-hover table-sm" style="margin: 0;" aria-label="">
                  <caption class="d-none"></caption>
                  <thead>
                    <tr>
                      <th id="parlamentar">Parlamentar</th>
                      <th id="tipo">Tipo</th>
                      <th id="estado">Estado</th>
                      <th id="partido">Partido</th>
                      <th id="valor" class="text-right">Valor</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="row in MaioresGastos" :key="row.id_fornecedor">
                      <td>
                        <a v-bind:href="row.link_parlamentar">{{row.nome_parlamentar}}</a>
                      </td>
                      <td>{{row.tipo}}</td>
                      <td>{{row.sigla_estado}}</td>
                      <td>{{row.sigla_partido}}</td>
                      <td class="text-right">
                        <a v-bind:href="row.link_despesas">{{row.valor_total}}</a>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div id="dvQueProcurar" class="modal fade" tabindex="-1" role="dialog">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h4 class="modal-title">O que procurar?</h4>
            <button type="button" class="close" data-dismiss="modal" aria-strong="Close">
              <span aria-hidden="true">&times;</span>
            </button>
          </div>
          <div class="modal-body text-justify">
            <p>A primeira coisa que sugerimos é começar a investigar o fornecedor. Em várias fiscalizações que a OPS já realizou, a empresa esteva registrada em endereços que não condiziam com a realidade do local. Um exemplo é a de uma locadora que dizia funcionar aonde, na realidade havia uma padaria. Você pode utilizar o Google Maps ou o Street View (se estiver disponível) para avaliar a localização da empresa. Visite o local, se for possível.</p>
            <p>Outra sugestão é verificar se a empresa possui uma página na internet e se ela realmente fornece o serviço prestado ao parlamentar. A OPS já encontrou “empresas” que faturavam alguns milhões de reais tendo como clientes apenas políticos. Além disso, estas empresas não possuíam qualquer publicidade na internet ou em outro lugar. Empresas sérias não se escondem e são facilmente localizadas.</p>
            <p>O foco da OPS não é a empresa prestadora de serviço ou fornecedora de produtos ao parlamentar, mas a nossa experiência nos mostra que muitas vezes empresas fajutas eram abertas apenas para emitirem notas a parlamentares.</p>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-primary" data-dismiss="modal">Entendi</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.card-body {
  min-height: 100px;
}
</style>

<script>
import { Chart } from 'highcharts-vue';
import jQuery from 'jquery';

const axios = require('axios');

export default {
  name: 'FornecedorDetalhes',
  components: {
    highcharts: Chart,
  },
  props: {
    id: Number,
  },
  data() {
    return {
      fornecedor: {},
      quadro_societario: {},
      MaioresGastos: {},

      // chartRecebimentosPorAnoOptions: {
      //   chart: {
      //     type: 'bar',
      //   },

      //   title: {
      //     text: null,
      //   },

      //   xAxis: {
      //     categories: [
      //       'Jan',
      //       'Fev',
      //       'Mar',
      //       'Abr',
      //       'Maio',
      //       'Jun',
      //       'Jul',
      //       'Ago',
      //       'Set',
      //       'Out',
      //       'Nov',
      //       'Dez',
      //     ],
      //   },

      //   yAxis: [
      //     {
      //       tickAmount: 10,
      //       title: {
      //         text: 'Valor (em reais)',
      //       },
      //       labels: {
      //         align: 'left',
      //         x: 3,
      //         y: 16,
      //         format: '{value:.,0f}',
      //       },
      //       showFirstLabel: false,
      //     },
      //   ],

      //   tooltip: {
      //     shared: true,
      //     crosshairs: true,
      //     pointFormat:
      //       '<span style=color:{point.color}">\u25CF</span> {series.name}: <b class="legend">{point.y:.,2f}</b><br/>',
      //   },

      //   plotOptions: {
      //     series: {
      //       stacking: 'normal',
      //     },
      //   },

      //   legend: {
      //     layout: 'vertical',
      //     align: 'right',
      //     verticalAlign: 'top',
      //     floating: true,
      //     borderWidth: 1,
      //     shadow: true,
      //   },

      //   series: [],
      // },

      chartRecebimentosPorAnoOptions: {
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

        plotOptions: {
            series: {
                borderWidth: 0,
                dataLabels: {
                    enabled: true
                }
            }
        },

        series: [{
          pointWidth: 20,
          name: 'Fornecedor',
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
    };
  },
  mounted() {
    window.document.title = 'OPS :: Fornecedor';
    const loaderDetalhes = this.$loading.show({
      fullPage: false,
      container: this.$refs.Detalhes,
    });

    const loaderMaioresGastos = this.$loading.show({
      fullPage: false,
      container: this.$refs.MaioresGastos,
    });

    const loaderRecebimentosPorMes = this.$loading.show({
      fullPage: false,
      container: this.$refs.RecebimentosPorMes,
    });

    axios.get(`${process.env.VUE_APP_API}/fornecedor/${this.id}`).then((response) => {
      const fornecedor = response.data.fornecedor;

      if (fornecedor.cnpj_cpf.length === 18) {
        fornecedor.genero = 'pj';

        // if (authService.authentication && authService.authentication.isAuth) {
        //   setTimeout(() => {
        //     $('#btnReconsultarDadosReceita').show();

        //     if (!response.fornecedor.data_de_abertura) {
        //       $scope.ReconsultarDadosReceita();
        //     }
        //   }, 100);
        // }
      } else if (fornecedor.cnpj_cpf.length === 14) {
        fornecedor.genero = 'pf';
      } else {
        fornecedor.genero = 'nd'; // fornecedor interno / sem cnpj
      }

      this.fornecedor = fornecedor;
      this.quadro_societario = response.data.quadro_societario;

      window.document.title = `OPS :: Fornecedor - ${fornecedor.nome}`;
      loaderDetalhes.hide();
    });

    axios
      .get(
        `${process.env.VUE_APP_API}/fornecedor/${this.id}/MaioresGastos`,
      )
      .then((response) => {
        this.MaioresGastos = response.data;
        loaderMaioresGastos.hide();
      });

    axios
      .get(
        `${process.env.VUE_APP_API}/fornecedor/${this.id}/RecebimentosPorAno`,
      )
      .then((response) => {
        this.chartRecebimentosPorAnoOptions.series[0].data = response.data.series;
        this.chartRecebimentosPorAnoOptions.xAxis.categories = response.data.categories;
        loaderRecebimentosPorMes.hide();
      });
  },
  methods: {
    PesquisarNoMaps(f) {
      window.open(
        `https://www.google.com/maps/place/${`${f.logradouro},${
          f.numero
        },${f.cep.replace('.', '')},${f.estado},Brasil`
          .split(' ')
          .join('+')}`,
      );
    },
    PesquisarNoGoogle(f) {
      window.open(
        `https://www.google.com.br/search?q=${f.nome},${f.cidade},${f.estado}`,
      );
    },
    SituacaoCadastral(f) {
      window.open(
        `https://servicos.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp?cnpj=${f.cnpj_cpf}`,
      );
    },
    ExpandirContrairInformacoesAdicional(e) {
      if (jQuery('#collapseDadosEmpresaAdicional').is(':visible')) {
        jQuery(e.target).text('Ver mais');
        jQuery('#collapseDadosEmpresaAdicional').hide();
      } else {
        jQuery(e.target).text('Ver menos');
        jQuery('#collapseDadosEmpresaAdicional').show();
      }
    },
  },
};
</script>
