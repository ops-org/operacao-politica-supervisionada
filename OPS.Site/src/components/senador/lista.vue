<template>
  <div class="container-fluid">
    <h3 class="page-title">Cota para Exercício da Atividade Parlamentar no Senado (CEAPS)</h3>

    <form id="form" autocomplete="off">
      <div class="row">
        <div class="form-group col-md-4">
          <label>Periodo</label>
          <select class="form-control input-sm" v-model="filtro.periodo">
            <option value="1">Mês Atual</option>
            <option value="2">Mês Anterior</option>
            <option value="3">Últimos 4 Meses</option>
            <option value="4">Ano Atual</option>
            <option value="5">Ano Anterior</option>
            <option value="9">56º (2019-2023)</option>
            <option value="8">55º (2015-2019)</option>
            <option value="7">54º (2011-2015)</option>
            <option value="6">53º (2007-2011)</option>
            <option value="0">Todas as Legislaturas</option>
          </select>
        </div>
        <div class="form-group col-md-4">
          <label>Senador</label>
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
          <label>Fornecedor</label>
          <div class="input-group input-group-sm">
            <input
              type="text"
              id="txtBeneficiario"
              class="form-control input-sm"
              disabled="disabled"
              v-model="filtro.fornecedor.nome"
            />
            <div class="input-group-append">
              <button
                type="button"
                class="btn btn-outline-secondary"
                v-on:click="AbreModalConsultaFornecedor();"
                title="Localizar Fornecedor"
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
            <option value="1" selected="true">Senador</option>
            <option value="2">Despesa</option>
            <option value="3">Fornecedor</option>
            <option value="4">Partido</option>
            <option value="5">Estado</option>
            <option value="6">Recibo</option>
          </select>
        </div>
        <div class="form-group col-md-4" v-if="filtro.agrupar=='6'">
          <label>Recibo</label>
          <input type="text" v-model="filtro.documento" class="form-control input-sm" />
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

    <div class="row">
      <div class="col-md-12">
        <div class="alert alert-warning" v-if="valorTotal">
          <b>Valor Total no Período: R$ {{valorTotal}}</b>
          <small class="help-block mb-0">Valor total considerando os filtros aplicados acima</small>
        </div>
      </div>
    </div>

    <div class="form-group" v-if="fields">
      <vdtnet-table ref="table" :fields="fields" :opts="options" @edit="AbrirModalDetalhar"></vdtnet-table>
    </div>

    <div class="modal" tabindex="-1" role="dialog" id="modal-detalhar" aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Detalhar por:</h5>
            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
              <span aria-hidden="true">&times;</span>
            </button>
          </div>
            <div class="list-group list-group-flush">
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('1')">Senador</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('2')">Despesa</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('3')">Fornecedor</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('4')">Partido</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('5')">Estado</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('6')">Recibo</button>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
          </div>
        </div>
      </div>
    </div>

    <div id="modal-fornecedor" class="modal fade" tabindex="-1" role="dialog" style="display: none;" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Pesquisar Fornecedor</h4>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">×</span>
                    </button>
                </div>
                <div class="modal-body text-justify">
                    <form class="form-group">
                        <div class="form-group">
                            <label for="inputNome">Nome</label>
                            <input type="text" class="form-control" v-model="fornecedor_busca.nome" placeholder="Informe um nome">
                        </div>
                        <div class="form-group">
                            <label for="inputCpfCnpj">CPF / CNPJ</label>
                            <input type="text" class="form-control" v-model="fornecedor_busca.cnpj" placeholder="Informe um CPF ou CNPJ">
                        </div>

                        <button type="button" class="btn btn-primary" v-on:click="ConsultaFornecedor();">Pesquisar</button>
                        <button type="reset" class="btn btn-light" v-on:click="LimparFiltroFornecedor();">Limpar</button>
                    </form>

                    <div class="list-group" v-if="fornecedores">
                        <div class="list-group-item">
                            Fornecedores
                        </div>
                        <a href="javascript:void(0);" class="list-group-item"
                          v-for="row in fornecedores" :key="row.id_fornecedor"
                          v-on:click="SelecionarFornecedor(row);">
                            <small>{{row.cnpj_cpf}} </small><br>
                            {{row.nome}}
                        </a>
                    </div>
                </div>
            </div>
        </div>
    </div>
  </div>
</template>

<script>
import jQuery from 'jquery';
import 'datatables.net-bs4';
import VdtnetTable from 'vue-datatables-net';

import VSelect from '../vue-bootstrap-select';

const axios = require('axios');

export default {
  components: {
    VSelect,
    VdtnetTable,
  },
  props: {
    qs: Object,
  },
  data() {
    const vm = this;

    return {
      selectedRow: {},
      valorTotal: null,
      senador: {},
      filtro: {
        agrupar: '1',
        periodo: '9',
        parlamentar: [],
        despesa: [],
        estado: [],
        partido: [],
        fornecedor: {},
      },
      fornecedor_busca: {},

      estados: [],
      partidos: [],
      parlamentares: [],
      despesas: [],
      fornecedores: [],

      options: {
        ajax(objData, callback) {
          const loader = vm.$loading.show();
          const newData = objData;
          delete newData.columns;
          delete newData.search;

          newData.filters = {
            Agrupamento: vm.filtro.agrupar,
            Periodo: vm.filtro.periodo,
            IdParlamentar: (vm.filtro.parlamentar || []).join(','),
            Despesa: (vm.filtro.despesa || []).join(','),
            Estado: (vm.filtro.estado || []).join(','),
            Partido: (vm.filtro.partido || []).join(','),
            Fornecedor: vm.filtro.fornecedor.id || null,
          };

          this.fields = null;

          axios
            .post(`${process.env.API}/senador/lancamentos`, newData)
            .then((response) => {
              vm.valorTotal = response.data.valorTotal;
              callback(response.data);

              loader.hide();
              jQuery.each(newData.filters, (key, value) => {
                if (value === '' || value === null) {
                  delete newData.filters[key];
                }
              });

              vm.$router.push({ path: 'senador', query: newData.filters }, () => { /* Necesario para não fazer redirect */ });
            });
        },
        pageLength: 100,
        ordering: true,
        dom: "tr<'row vdtnet-footer'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",

        responsive: false,
        processing: true,
        searching: true,
        searchDelay: 1500,
        destroy: true,
        lengthChange: true,
        serverSide: true,
        fixedHeader: true,
        saveState: true,
      },
      fields: {},
    };
  },
  mounted() {
    const vm = this;

    vm.filtro.agrupar = vm.qs.Agrupamento || '1';
    vm.filtro.periodo = vm.qs.Periodo || '9';
    vm.filtro.parlamentar = (vm.qs.IdParlamentar ? vm.qs.IdParlamentar.split(',') : []);
    vm.filtro.despesa = (vm.qs.Despesa ? vm.qs.Despesa.split(',') : []);
    vm.filtro.estado = (vm.qs.Estado ? vm.qs.Estado.split(',') : []);
    vm.filtro.partido = (vm.qs.Partido ? vm.qs.Partido.split(',') : []);
    vm.filtro.fornecedor = (vm.qs.Fornecedor ? { id: vm.qs.Fornecedor, nome: vm.qs.Fornecedor } : {});

    document.title = 'OPS :: Deputado Federal';

    axios.get(`${process.env.API}/estado`).then((response) => {
      this.estados = response.data;
    });

    axios.get(`${process.env.API}/partido`).then((response) => {
      this.partidos = response.data;
    });

    axios
      .get(`${process.env.API}/senador/tipodespesa`)
      .then((response) => {
        this.despesas = response.data;
      });

    axios.get(`${process.env.API}/senador`).then((response) => {
      this.parlamentares = response.data;
    });

    this.Pesquisar();
  },
  methods: {
    AbreModalConsultaFornecedor() {
      jQuery('#modal-fornecedor').modal();
    },
    ConsultaFornecedor() {
      const loader = this.$loading.show();

      axios
        .post(`${process.env.API}/fornecedor/consulta`, this.fornecedor_busca)
        .then((response) => {
          this.fornecedores = response.data;

          loader.hide();
        });
    },
    SelecionarFornecedor(f) {
      this.filtro.fornecedor = {
        id: f.id_fornecedor,
        cnpj: f.cnpj_cpf,
        nome: f.nome,
      };

      this.fornecedores = {};
      jQuery('#modal-fornecedor').modal('hide');
    },
    LimparFiltroFornecedor() {
      this.fornecedores = {};
      this.fornecedor_busca = {};
      this.filtro.fornecedor = {};
    },
    AbrirModalDetalhar(data) {
      this.selectedRow = data;
      jQuery('#modal-detalhar').modal();
    },
    Detalhar(agrupar) {
      const vm = this;

      switch (vm.filtro.agrupar) {
        case '1': // Deputado
          vm.filtro.parlamentar = [this.selectedRow.id_sf_senador];
          break;
        case '2': // Despesa
          vm.filtro.despesa = [this.selectedRow.id_sf_despesa_tipo];
          break;
        case '3': // Fornecedor
          vm.filtro.fornecedor = { id: this.selectedRow.id_fornecedor, cnpj: this.selectedRow.cnpj_cpf, nome: this.selectedRow.nome_fornecedor };
          break;
        case '4': // Partido
          vm.filtro.partido = [this.selectedRow.id_partido];
          break;
        case '5': // Estado
          vm.filtro.estado = [this.selectedRow.id_estado];
          break;
        default:
          break;
      }

      vm.filtro.agrupar = agrupar;
      jQuery('#modal-detalhar').modal('hide');

      vm.Pesquisar();
    },
    Pesquisar() {
      const vm = this;
      vm.senador = {};
      vm.fields = null;

      vm.$nextTick(() => {
        switch (vm.filtro.agrupar) {
          case '1': // Senador
            vm.fields = {
              id_sf_senador: {
                isLocal: true,
                label: '&nbsp;',
                render: (data, type) => {
                  if (type === 'display') {
                    return '<a href="javascript:void(0);" data-action="edit" class="btn btn-primary btn-sm">Detalhar</a>';
                  }
                  return data;
                },
                sortable: false,
              },
              nome_parlamentar: {
                label: 'Parlamentar',
                render: (data, type, full) => {
                  if (type === 'display') {
                    return `<a href="/senador/${full.id_sf_senador}">${data}</a>`;
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
                className: 'text-right',
              },
              valor_total: {
                label: 'Valor Total',
                sortable: true,
                className: 'text-right',
              },
            };
            break;


          case '2': // Despesa
            vm.fields = {
              id_sf_despesa_tipo: {
                label: '&nbsp;',
                render: (data, type) => {
                  if (type === 'display') {
                    return '<a href="javascript:void(0);" data-action="edit" class="btn btn-primary btn-sm">Detalhar</a>';
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
                className: 'text-right',
              },
              valor_total: {
                label: 'Valor Total',
                sortable: true,
                className: 'text-right',
              },
            };
            break;

          case '3': // Fornecedor
            vm.fields = {
              id_fornecedor: {
                label: '&nbsp;',
                render: (data, type) => {
                  if (type === 'display') {
                    return '<a href="javascript:void(0);" data-action="edit" class="btn btn-primary btn-sm">Detalhar</a>';
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
                label: 'Fornecedor',
                render: (data, type, full) => {
                  if (type === 'display') {
                    return `<a href="/fornecedor/${full.id_fornecedor}">${data}</a>`;
                  }
                  return data;
                },
                sortable: true,
              },
              total_notas: {
                label: 'Total Recibos',
                sortable: true,
                className: 'text-right',
              },
              valor_total: {
                label: 'Valor Total',
                sortable: true,
                className: 'text-right',
              },
            };
            break;

          case '4': // Partido
            vm.fields = {
              id_partido: {
                label: '&nbsp;',
                render: (data, type) => {
                  if (type === 'display') {
                    return '<a href="javascript:void(0);" data-action="edit" class="btn btn-primary btn-sm">Detalhar</a>';
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
                className: 'text-right',
              },
              total_senadores: {
                label: 'Senadores',
                sortable: true,
                className: 'text-right',
              },
              valor_medio_por_senador: {
                label: 'Val. Médio Senador',
                sortable: true,
                className: 'text-right',
              },
              valor_total: {
                label: 'Valor Total',
                sortable: true,
                className: 'text-right',
              },
            };
            break;

          case '5': // Estado
            vm.fields = {
              id_estado: {
                label: '&nbsp;',
                render: (data, type) => {
                  if (type === 'display') {
                    return '<a href="javascript:void(0);" data-action="edit" class="btn btn-primary btn-sm">Detalhar</a>';
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
                className: 'text-right',
              },
              valor_total: {
                label: 'Valor Total',
                sortable: true,
                className: 'text-right',
              },
            };
            break;

          case '6': // Recibo
            vm.fields = {
              data_documento: {
                label: 'Emissão',
                sortable: true,
              },
              cnpj_cpf: {
                label: 'CNPJ/CPF',
                sortable: true,
              },
              nome_fornecedor: {
                label: 'Fornecedor',
                render: (data, type, full) => {
                  if (type === 'display') {
                    return `<a href="/fornecedor/${full.id_fornecedor}">${data}</a>`;
                  }
                  return data;
                },
                sortable: true,
              },
              nome_parlamentar: {
                label: 'Parlamentar',
                render: (data, type, full) => {
                  if (type === 'display') {
                    return `<a href="/senador/${full.id_sf_senador}">${data}</a>`;
                  }
                  return data;
                },
                sortable: true,
              },
              valor_total: {
                label: 'Valor',
                sortable: true,
                className: 'text-right',
              },
            };
            break;

          default: break;
        }

        vm.$nextTick(() => {
          vm.$refs.table.reload(null, true);
        });
      });
    },
    LimparFiltros() {
      this.filtro = {
        agrupar: '1',
        periodo: '9',
        parlamentar: [],
        despesa: [],
        estado: [],
        partido: [],
        fornecedor: {},
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
