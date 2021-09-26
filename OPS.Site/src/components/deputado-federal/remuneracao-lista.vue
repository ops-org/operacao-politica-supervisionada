<template>
  <div class="container-fluid">
    <h3 class="page-title">[BETA] Remuneração no Câmara Federal</h3>

    <form id="form" autocomplete="off">
      <div class="row">
        <div class="form-group col-md-2">
          <label>Ano</label>
          <select class="form-control input-sm" v-model="filtro.ano">
            <option value=""></option>
            <option value="2021" selected>2021</option>
            <option value="2020">2020</option>
            <option value="2019">2019</option>
            <option value="2018">2018</option>
            <option value="2017">2017</option>
            <option value="2016">2016</option>
            <option value="2015">2015</option>
            <option value="2014">2014</option>
            <option value="2013">2013</option>
            <option value="2012">2012</option>
          </select>
        </div>
        <div class="form-group col-md-2">
          <label>Mês</label>
          <select class="form-control input-sm" v-model="filtro.mes">
            <option value></option>
            <option value="1">Janeiro</option>
            <option value="2">Fevereiro</option>
            <option value="3">Março</option>
            <option value="4">Abril</option>
            <option value="5">Maio</option>
            <option value="6">Junho</option>
            <option value="7">Julho</option>
            <option value="8">Agosto</option>
            <option value="9">Setembro</option>
            <option value="10">Outubro</option>
            <option value="11">Novembro</option>
            <option value="12">Dezembro</option>
          </select>
        </div>
        <div class="form-group col-md-5">
          <label>Grupo Funcional</label>
          <v-select
            :options="grupos_funcionais"
            v-model="filtro.grupo_funcional"
            class="form-control input-sm"
            multiple
            data-actions-box="true"
          />
        </div>
        <div class="form-group col-md-3">
          <label>Cargo</label>
          <v-select
            :options="cargos"
            v-model="filtro.cargo"
            class="form-control input-sm"
            multiple
            data-actions-box="true"
          />
        </div>
      </div>
      <div class="row">
        <div class="form-group col-md-2">
          <label>Agrupar por</label>
          <select class="form-control input-sm" v-model="filtro.agrupar">
            <option value="1" selected="true">Grupo Funcional</option>
            <option value="2">Cargo</option>
            <option value="3">Deputado(a)</option>
            <option value="4">Secretario(a)</option>
            <option value="5">Ano</option>
            <option value="6">Não agrupar</option>
          </select>
        </div>
        <div class="form-group col-md-5">
          <label>Deputado(a)</label>
          <multiselect v-model="filtro.parlamentar" :options="parlamentares" :multiple="true" placeholder="Selecione"
            :close-on-select="false" :clear-on-select="false" :preserve-search="true" label="text" track-by="id" 
            :searchable="true" :loading="isLoadingParlamentar" :internal-search="false" @search-change="BuscaParlamentar">

            <template slot="selection" slot-scope="{ values, isOpen }">
              <span class="multiselect__single" v-if="values.length > 1 && !isOpen">{{ values.length }} item(ns) selecionado(s)</span>
            </template>
            <span slot="noResult">Oops! Nenhum resultado encontrado.</span>
            <span slot="noOptions">Digite o nome do parlamentar.</span>
          </multiselect>
        </div>
        <div class="form-group col-md-5">
          <label>Secretario(a)</label>
          <multiselect v-model="filtro.secretario" :options="secretarios" :multiple="true" placeholder="Selecione"
            :close-on-select="false" :clear-on-select="false" :preserve-search="true" label="text" track-by="id" 
            :searchable="true" :loading="isLoadingSecretario" :internal-search="false" @search-change="BuscaSecretario">

            <template slot="selection" slot-scope="{ values, isOpen }">
              <span class="multiselect__single" v-if="values.length > 1 && !isOpen">{{ values.length }} item(ns) selecionado(s)</span>
            </template>
            <span slot="noResult">Oops! Nenhum resultado encontrado.</span>
            <span slot="noOptions">Digite o nome do parlamentar.</span>
          </multiselect>
        </div>
      </div>
      <div class="row">
        <div class="form-group col-md-12">
          <input
            type="button"
            id="ButtonPesquisar"
            v-on:click="Pesquisar(false);"
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
          <strong>Custo Total no Período: R$ {{valorTotal}}</strong>
          <small class="help-block mb-0">&nbsp;Custo Total considerando os filtros aplicados acima</small>
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
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('1')">Grupo Funcional</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('2')">Cargo</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('3')">Deputado(a)</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('4')">Secretario(a)</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('5')">Ano</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('6')">Detalhes</button>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-dismiss="modal">Cancelar</button>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script>
import jQuery from 'jquery';
import VSelect from '../vue-bootstrap-select';
const axios = require('axios');

export default {
  components: {
    VSelect,
  },
  props: {
    qs: Object,
  },
  data() {
    const vm = this;

    return {
      isLoadingParlamentar: false,
      isLoadingSecretario: false,
      loading: false,
      pageLoad: true,
      selectedRow: {},
      valorTotal: null,
      filtro: {
        agrupar: '1',
        ano: '2021',
        mes: '',
        grupo_funcional: [],
        cargo: [],
        parlamentar: [],
        secretario: [],
      },

      grupos_funcionais: [],
      cargos: [],
      parlamentares: [],
      secretarios: [],

      options: {
        ajax(objData, callback) {
          if (objData.draw === 1) return;
          // if (!vm.loading) vm.loading = true;
          // else return;

          const loader = vm.$loading.show();
          const newData = objData;
          delete newData.columns;
          delete newData.search;

           /* eslint-disable no-debugger */
        debugger;
        /* eslint-enable no-debugger */
          newData.filters = {
            ag: vm.filtro.agrupar,
            an: vm.filtro.ano,
            ms: vm.filtro.mes,
            gf: (vm.filtro.grupo_funcional || []).join(','),
            cr: (vm.filtro.cargo || []).join(','),
            df: window.GetIds(vm.filtro.parlamentar).join(','),
            sc: window.GetIds(vm.filtro.secretario).join(','),
          };

          jQuery.each(newData.filters, (key, value) => {
            if (value === '' || value === null) {
              delete newData.filters[key];
            }
          });

          if (!vm.pageLoad) {
            vm.$router.push({ path: 'remuneracao', query: newData.filters }, () => { /* Necesario para não fazer redirect */ });
          }
          this.fields = null;

          axios
            .post(`${process.env.VUE_APP_API}/deputado/remuneracao`, newData)
            .then((response) => {
              vm.loading = false;
              vm.valorTotal = response.data.valorTotal;
              callback(response.data);

              loader.hide();
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
    var lstPromises = [];

    vm.filtro.agrupar = vm.qs.ag || '1';
    vm.filtro.ano = vm.qs.an || '2021';
    vm.filtro.mes = vm.qs.ms || '';
    vm.filtro.grupo_funcional = (vm.qs.gf ? vm.qs.gf.split(',') : []);
    vm.filtro.cargo = (vm.qs.cr ? vm.qs.cr.split(',') : []);

    if(vm.qs.df){
      var pDeputado = axios.post(`${process.env.VUE_APP_API}/deputado/pesquisa`, { ids: vm.qs.df })
      pDeputado.then((response) => {
        vm.filtro.parlamentar = response.data;
      });

      lstPromises.push(pDeputado);
    }

    if(vm.qs.sc){
      var pSecretario = axios.post(`${process.env.VUE_APP_API}/deputado/secretariopesquisa`, { ids: vm.qs.sc })
      pSecretario.then((response) => {
        vm.filtro.secretario = response.data;
      });

      lstPromises.push(pSecretario);
    }

    document.title = 'OPS :: Remuneração na Câmara Federal';

    axios.get(`${process.env.VUE_APP_API}/deputado/grupofuncional`).then((response) => {
      this.grupos_funcionais = response.data;
    });

    axios.get(`${process.env.VUE_APP_API}/deputado/cargo`).then((response) => {
      this.cargos = response.data;
    });

    if(lstPromises.length == 0){
        this.Pesquisar(true);
    }else{
      Promise.all(lstPromises).then(() => vm.Pesquisar(true));
    }
  },
  methods: {
    BuscaParlamentar(busca) {
      this.isLoadingParlamentar = true;

      axios
        .post(`${process.env.VUE_APP_API}/deputado/pesquisa`, { busca: busca, periodo: parseInt(this.filtro.periodo || "0") })
        .then((response) => {
          this.parlamentares = response.data;

          this.isLoadingParlamentar = false;
        });
    },
    BuscaSecretario(busca) {
      this.isLoadingsecretario = true;

      axios
        .post(`${process.env.VUE_APP_API}/deputado/secretariopesquisa`, { busca: busca, periodo: parseInt(this.filtro.periodo || "0") })
        .then((response) => {
          this.secretarios = response.data;

          this.isLoadingsecretario = false;
        });
    },
    AbrirModalDetalhar(data) {
      this.selectedRow = data;
      jQuery('#modal-detalhar').modal();
    },
    Detalhar(agrupar) {
      const vm = this;

      switch (vm.filtro.agrupar) {
        case '1': // Grupo Funcional
          vm.filtro.grupo_funcional = [this.selectedRow.id];
          break;
        case '2': // Cargo
          vm.filtro.cargo = [this.selectedRow.id];
          break;
        case '3': // Deputado
          vm.filtro.parlamentar = [{ id: this.selectedRow.id, text: this.selectedRow.descricao }];
          break;
        case '4': // Secretario
          vm.filtro.secretario = [{ id: this.selectedRow.id, text: this.selectedRow.descricao }];
          break;
        default:
          break;
      }

      vm.filtro.agrupar = agrupar;
      jQuery('#modal-detalhar').modal('hide');

      vm.Pesquisar(false);
    },
    Pesquisar(pageLoad) {
      const vm = this;
      vm.deputado = {};
      vm.fields = null;
      vm.pageLoad = pageLoad;

      vm.$nextTick(() => {
        switch (vm.filtro.agrupar) {
          case '1': // Grupo Funcional
            vm.fields = {
              id: {
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
              descricao: {
                label: 'Grupo Funcional',
                sortable: true,
              },
              quantidade: {
                label: 'Qtd.',
                sortable: true,
                className: 'text-right',
              },
              valor_total: {
                label: 'Custo Total',
                sortable: true,
                className: 'text-right',
              },
            };
            break;


          case '2': // Cargo
            vm.fields = {
              id: {
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
                label: 'Cargo',
                sortable: true,
              },
              quantidade: {
                label: 'Qtd.',
                sortable: true,
                className: 'text-right',
              },
              valor_total: {
                label: 'Custo Total',
                sortable: true,
                className: 'text-right',
              },
            };
            break;

            case '3': // Deputado Federal
              vm.fields = {
                id: {
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
                  label: 'Deputado',
                  sortable: true,
                },
                quantidade: {
                  label: 'Qtd.',
                  sortable: true,
                  className: 'text-right',
                },
                valor_total: {
                  label: 'Custo Total',
                  sortable: true,
                  className: 'text-right',
                },
              };
            break;

            case '4': // Secretario
              vm.fields = {
                id: {
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
                  label: 'Secretario',
                  sortable: true,
                },
                quantidade: {
                  label: 'Qtd.',
                  sortable: true,
                  className: 'text-right',
                },
                valor_total: {
                  label: 'Custo Total',
                  sortable: true,
                  className: 'text-right',
                },
              };
            break;

          case '5': // Ano
            vm.fields = {
              id: {
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
                label: 'Ano',
                sortable: true,
              },
              quantidade: {
                label: 'Qtd.',
                sortable: true,
                className: 'text-right',
              },
              valor_total: {
                label: 'Custo Total',
                sortable: true,
                className: 'text-right',
              },
            };
            break;

          case '6': // Mês
            vm.fields = {
              deputado: {
                label: 'Deputado',
                sortable: true,
              },
              secretario: {
                label: 'Secretario',
                sortable: true,
              },
              grupo_funcional: {
                label: 'Grupo Funcional',
                sortable: true,
              },
              cargo: {
                label: 'Cargo',
                sortable: true,
              },
              tipo_folha: {
                label: 'Tipo Folha',
                sortable: true,
              },
              ano_mes: {
                label: 'Ano/Mês',
                sortable: true,
              },
               valor_bruto: {
                label: 'Valor Bruto',
                sortable: true,
                className: 'text-right',
              },
              valor_outros: {
                label: 'Vantagens',
                sortable: true,
                className: 'text-right',
              },
              valor_total: {
                label: 'Custo Total',
                render: (data, type, full) => {
                  if (type === 'display') {
                    return `<a href="/deputado-federal/remuneracao/${full.id}">${data}</a>`;
                  }
                  return data;
                },
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
        ano: '2021',
        mes: '',
        grupo_funcional: [],
        cargo: [],
        parlamentar: [],
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
