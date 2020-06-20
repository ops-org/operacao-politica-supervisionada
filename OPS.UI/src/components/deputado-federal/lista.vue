<template>
  <div class="container-fluid">
    <h3 class="page-title">Cota para Exercício da Atividade Parlamentar (CEAP)</h3>

    <form id="form" autocomplete="off">
      <div class="row">
        <div class="form-group col-md-4">
          <label>Periodo</label>
          <v-select
            :options="periodos"
            v-model.number="filtro.periodo"
            class="form-control input-sm"
          />
        </div>
        <div class="form-group col-md-4">
          <label>Deputado / Liderança</label>
          <v-select
            :options="parlamentares"
            v-model="filtro.parlamentar"
            class="form-control input-sm"
            multiple
            data-actions-box="true"
          />
        </div>
        <div class="form-group col-md-4">
          <label>Tipo de Despesa</label>
          <v-select
            :options="despesas"
            v-model="filtro.despesa"
            class="form-control input-sm"
            multiple
            data-actions-box="true"
          />
        </div>
      </div>

      <div class="row">
        <div class="form-group col-md-4">
          <label>Estado</label>
          <v-select
            :options="estados"
            v-model="filtro.estado"
            class="form-control input-sm"
            multiple
            data-actions-box="true"
          />
        </div>
        <div class="form-group col-md-4">
          <label>Partido</label>
          <v-select
            :options="partidos"
            v-model="filtro.partido"
            class="form-control input-sm"
            multiple
            data-actions-box="true"
          />
        </div>
        <div class="form-group col-md-4">
          <label>Beneficiário</label>
          <div class="input-group input-group-sm">
            <input
              type="text"
              id="txtBeneficiario"
              class="form-control input-sm"
              disabled="disabled"
            />
            <div class="input-group-append">
              <button
                type="button"
                class="btn btn-outline-secondary"
                v-on:click="AbreModalConsultaFornecedor();"
                title="Localizar beneficiário"
              >
                <span class="fa fa-search"></span>
              </button>
              <button
                type="button"
                class="btn btn-outline-secondary"
                v-on:click="LimparFiltroFornecedor();"
                title="Limpar"
              >
                <span class="fa fa-times"></span>
              </button>
            </div>
          </div>
        </div>
      </div>
      <div class="row">
        <div class="form-group col-md-4">
          <label>Agrupar por</label>
          <select class="form-control input-sm" v-model="filtro.agrupar">
            <option value="1">Deputado</option>
            <option value="2">Beneficiário</option>
            <option value="3">Despesa</option>
            <option value="4">Partido</option>
            <option value="5">Estado</option>
            <option value="6">Recibo</option>
          </select>
        </div>
        <div class="form-group col-md-4" v-if="filtro.agrupar=='6'">
          <label>Recibo</label>
          <input type="text" id="txtDocumento" class="form-control input-sm" />
        </div>
        <div class="form-group col-md-4 competencia" v-if="filtro.agrupar=='6'">
          <label>Competência</label>
          <div>
            <div class="input-group" style="width: 225px">
              <div class="input-group-prepend">
                <span class="input-group-text" id="basic-addon1">De</span>
              </div>
              <select id="lstPeridoMesInicio" class="form-control" style="width: 80px">
                <option value></option>
                <option value="01">Janeiro</option>
                <option value="02">Fevereiro</option>
                <option value="03">Março</option>
                <option value="04">Abril</option>
                <option value="05">Maio</option>
                <option value="06">Junho</option>
                <option value="08">Julho</option>
                <option value="09">Agosto</option>
                <option value="10">Outubro</option>
                <option value="11">Novembro</option>
                <option value="12">Dezembro</option>
              </select>
              <select id="lstPeridoAnoInicio" class="form-control" style="width: 80px">
                <option value></option>
                <option value="2007">2007</option>
                <option value="2008">2008</option>
                <option value="2009">2009</option>
                <option value="2010">2010</option>
                <option value="2011">2011</option>
                <option value="2012">2012</option>
                <option value="2013">2013</option>
                <option value="2014">2014</option>
                <option value="2015">2015</option>
                <option value="2016">2016</option>
                <option value="2017">2017</option>
                <option value="2018">2018</option>
                <option value="2019">2019</option>
                <option value="2020">2020</option>
              </select>
            </div>
            <div class="input-group" style="width: 225px">
              <div class="input-group-prepend">
                <span class="input-group-text" id="basic-addon1">Até</span>
              </div>
              <select id="lstPeridoMesFinal" class="form-control" style="width: 80px">
                <option value></option>
                <option value="01">Janeiro</option>
                <option value="02">Fevereiro</option>
                <option value="03">Março</option>
                <option value="04">Abril</option>
                <option value="05">Maio</option>
                <option value="06">Junho</option>
                <option value="08">Julho</option>
                <option value="09">Agosto</option>
                <option value="10">Outubro</option>
                <option value="11">Novembro</option>
                <option value="12">Dezembro</option>
              </select>
              <select id="lstPeridoAnoFinal" class="form-control" style="width: 80px">
                <option value></option>
                <option value="2007">2007</option>
                <option value="2008">2008</option>
                <option value="2009">2009</option>
                <option value="2010">2010</option>
                <option value="2011">2011</option>
                <option value="2012">2012</option>
                <option value="2013">2013</option>
                <option value="2014">2014</option>
                <option value="2015">2015</option>
                <option value="2016">2016</option>
                <option value="2017">2017</option>
                <option value="2018">2018</option>
                <option value="2019">2019</option>
                <option value="2020">2020</option>
              </select>
            </div>

            <div class="clearfix"></div>
          </div>
        </div>
      </div>
      <div class="row">
        <div class="form-group col-md-12">
          <input
            type="button"
            id="ButtonPesquisar"
            v-on:click="Pesquisar();"
            value="Pesquisar"
            class="btn btn-danger btn-sm"
          />
          <input
            type="button"
            value="Limpar filtros"
            class="btn btn-light btn-sm"
            v-on:click="LimparFiltros();"
          />
        </div>
      </div>
    </form>

    <div class="form-group" v-if="fields">
      <vdtnet-table ref="table" :fields="fields" :opts="options"></vdtnet-table>
    </div>
  </div>
</template>

<script>
import 'jquery';
import 'datatables.net-bs4';
import VdtnetTable from 'vue-datatables-net';

import VSelect from '../vue-bootstrap-select';

const axios = require('axios');

export default {
  components: {
    VSelect,
    VdtnetTable,
  },
  data() {
    const vm = this;

    return {
      deputado_federal: {},
      filtro: {
        agrupar: '1',
        periodo: 9,
        parlamentar: [],
        despesa: [],
        estado: [],
        partido: [],
        beneficiario: [],
      },
      periodos: [
        { id: 1, text: 'Mês Atual' },
        { id: 2, text: 'Mês Anterior' },
        { id: 3, text: 'Últimos 4 Meses' },
        { id: 4, text: 'Ano Atual' },
        { id: 5, text: 'Ano Anterior' },
        { id: 9, text: '56º (2019-2023)' },
        { id: 8, text: '55º (2015-2019)' },
        { id: 7, text: '54º (2011-2015)' },
        { id: 6, text: '53º (2007-2011)' },
        { id: 0, text: 'Todas as Legislaturas' },
      ],
      estados: [],
      partidos: [],
      parlamentares: [],
      despesas: [],
      beneficiario: {},

      options: {
        ajax(objData, callback) {
          const loader = vm.$loading.show();
          const newData = objData;
          delete newData.columns;
          delete newData.search;

          axios
            .post('http://localhost:5000/api/Deputado/Lancamentos', newData)
            .then((response) => {
              switch (vm.filtro.agrupar) {
                case '1': // Deputado
                  vm.fields = {
                    id_cf_deputado: {
                      isLocal: true,
                      label: '&nbsp;',
                      render: (data, type) => {
                        if (type === 'display') {
                          return '<a class="btn btn-primary btn-sm">Detalhar</a>';
                        }
                        return data;
                      },
                      sortable: false,
                    },
                    nome_parlamentar: {
                      label: 'Parlamentar',
                      render: (data, type, full) => {
                        if (type === 'display') {
                          return `<a v-on:href="./deputado-federal/${full.id_cf_deputado}">${data}</a>`;
                        }
                        return data;
                      },
                      sortable: true,
                    },
                    sigla_estado: {
                      label: 'UF',
                      sortable: true,
                    },
                    sigla_partido: {
                      label: 'Partido',
                      sortable: true,
                    },
                    total_notas: {
                      label: 'Total Recibos',
                      sortable: true,
                    },
                    valor_total: {
                      label: 'Valor Total',
                      sortable: true,
                    },
                  };
                  break;

                case '2': // Beneficiário
                  vm.fields = {
                    id_fornecedor: {
                      label: '&nbsp;',
                      render: (data, type) => {
                        if (type === 'display') {
                          return '<a class="btn btn-primary btn-sm">Detalhar</a>';
                        }
                        return data;
                      },
                      sortable: false,
                    },
                    cnpj_cpf: {
                      label: 'CNPJ/CPF',
                      sortable: true,
                    },
                    nome_fornecedor: {
                      label: 'Beneficiário',
                      render: (data, type, full) => {
                        if (type === 'display') {
                          return `<a v-on:href="./fornecedor/${full.id_fornecedor}">${data}</a>`;
                        }
                        return data;
                      },
                      sortable: true,
                    },
                    sigla_partido: {
                      label: 'Partido',
                      sortable: true,
                    },
                    total_notas: {
                      label: 'Total Recibos',
                      sortable: true,
                    },
                    valor_total: {
                      label: 'Valor Total',
                      sortable: true,
                    },
                  };
                  break;

                case '3': // Despesa
                  vm.fields = {
                    id_cf_despesa_tipo: {
                      label: '&nbsp;',
                      render: (data, type) => {
                        if (type === 'display') {
                          return '<a class="btn btn-primary btn-sm">Detalhar</a>';
                        }
                        return data;
                      },
                      sortable: false,
                    },
                    descricao: {
                      label: 'Despesa',
                      sortable: true,
                    },
                    total_notas: {
                      label: 'Total Recibos',
                      sortable: true,
                    },
                    valor_total: {
                      label: 'Valor Total',
                      sortable: true,
                    },
                  };
                  break;

                case '4': // Partido
                  vm.fields = {
                    id_partido: {
                      label: '&nbsp;',
                      render: (data, type) => {
                        if (type === 'display') {
                          return '<a class="btn btn-primary btn-sm">Detalhar</a>';
                        }
                        return data;
                      },
                      sortable: false,
                    },
                    nome_partido: {
                      label: 'Partido',
                      sortable: true,
                    },
                    total_notas: {
                      label: 'Recibos',
                      sortable: true,
                    },
                    total_deputados: {
                      label: 'Deputados',
                      sortable: true,
                    },
                    valor_medio_por_deputado: {
                      label: 'Val. Médio Deputado',
                      sortable: true,
                    },
                    valor_total: {
                      label: 'Valor Total',
                      sortable: true,
                    },
                  };
                  break;

                case '5': // Estado
                  vm.fields = {
                    id_estado: {
                      label: '&nbsp;',
                      render: (data, type) => {
                        if (type === 'display') {
                          return '<a class="btn btn-primary btn-sm">Detalhar</a>';
                        }
                        return data;
                      },
                      sortable: false,
                    },
                    nome_estado: {
                      label: 'Estado',
                      sortable: true,
                    },
                    total_notas: {
                      label: 'Total Recibos',
                      sortable: true,
                    },
                    valor_total: {
                      label: 'Valor Total',
                      sortable: true,
                    },
                  };
                  break;

                case '6': // Recibo
                  vm.fields = {
                    data_emissao: {
                      label: 'Emissão',
                      sortable: true,
                    },
                    cnpj_cpf: {
                      label: 'CNPJ/CPF',
                      sortable: true,
                    },
                    nome_fornecedor: {
                      label: 'Beneficiário',
                      render: (data, type, full) => {
                        if (type === 'display') {
                          return `<a v-on:href="./fornecedor/${full.id_fornecedor}">${data}</a>`;
                        }
                        return data;
                      },
                      sortable: true,
                    },
                    sigla_estado_fornecedor: {
                      label: 'UF',
                      sortable: true,
                    },
                    nome_parlamentar: {
                      label: 'Parlamentar',
                      render: (data, type, full) => {
                        if (type === 'display') {
                          return `<a v-on:href="./deputado-federal/${full.id_cf_deputado}">${data}</a>`;
                        }
                        return data;
                      },
                      sortable: true,
                    },
                    numero_documento: {
                      label: 'Nº Recibo',
                      sortable: true,
                    },
                    trecho_viagem: {
                      label: 'Trecho',
                      sortable: true,
                    },
                    valor_liquido: {
                      label: 'Valor',
                      render: (data, type, full) => {
                        if (type === 'display') {
                          return `<a v-on:href="./deputado-federal/documento/${full.id_cf_despesa}">${data}</a>`;
                        }
                        return data;
                      },
                      sortable: true,
                    },
                  };
                  break;

                default: break;
              }


              callback(response.data);

              loader.hide();
            });
        },
        pageLength: 100,
        dom: "tr<'row vdtnet-footer'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
      },
      fields: {
        id_cf_deputado: {
          isLocal: true,
          label: '&nbsp;',
          render: (data, type) => {
            if (type === 'display') {
              return `<a href="/deputado-federal/${data}/secretario" class="btn btn-primary btn-sm">Ver secretários</a>`;
            }
            return data;
          },
          sortable: false,
        },
        nome_parlamentar: {
          label: 'Parlamentar',
          sortable: true,
        },
        quantidade_secretarios: {
          label: 'Secretários',
          sortable: true,
        },
        custo_secretarios: {
          label: 'Custo Mensal',
          sortable: true,
        },
      },
    };
  },
  mounted() {
    document.title = 'OPS :: Deputado Federal';

    axios.get('http://localhost:5000/api/estado').then((response) => {
      this.estados = response.data;
    });

    axios.get('http://localhost:5000/api/partido').then((response) => {
      this.partidos = response.data;
    });

    axios
      .get('http://localhost:5000/api/deputado/tipodespesa')
      .then((response) => {
        this.despesas = response.data;
      });

    axios.get('http://localhost:5000/api/deputado/pesquisa').then((response) => {
      this.parlamentares = response.data;
    });

    this.Pesquisar();
  },
  methods: {
    Pesquisar() {
      // const loader = this.$loading.show();
      this.deputado_federal = {};
      this.fields = null;

      this.$refs.table.reload();

      // axios
      //   .post('http://localhost:5000/api/deputado/lancamentos', this.filtro)
      //   .then((response) => {
      //     this.deputado_federal = response.data;

      //     loader.hide();
      //   });
    },
    LimparFiltros() {
      this.filtro = {
        periodo: 6,
        estado: [],
        partido: [],
      };
    },
  },
};
</script>

<style>
.competencia div.input-group {
  float: left;
}
</style>
