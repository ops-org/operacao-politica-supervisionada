<template>
  <div class="container">
    <h3 class="page-title">[BETA] Remuneração no Senado</h3>

    <form id="form" autocomplete="off">
      <div class="row">
        <div class="form-group col-md-2">
          <label>Ano</label>
          <select class="form-control input-sm" v-model="filtro.ano">
            <option value="2023" selected>2023</option>
            <option value="2022">2022</option>
            <option value="2021">2021</option>
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
        <div class="form-group col-md-4">
          <label>Vinculo</label>
          <v-select
            :options="vinculos"
            v-model="filtro.vinculo"
            class="form-control input-sm"
            multiple
            data-actions-box="true"
          />
        </div>
        <div class="form-group col-md-4">
          <label>Categoria</label>
          <v-select
            :options="categorias"
            v-model="filtro.categoria"
            class="form-control input-sm"
            multiple
            data-actions-box="true"
          />
        </div>
      </div>

      <div class="row">
        <div class="form-group col-md-4">
          <label>Cargo</label>
          <v-select
            :options="cargos"
            v-model="filtro.cargo"
            class="form-control input-sm"
            multiple
            data-actions-box="true"
          />
        </div>
        <div class="form-group col-md-4">
          <label>Lotação</label>
          <v-select
            :options="lotacoes"
            v-model="filtro.lotacao"
            class="form-control input-sm"
            multiple
            data-actions-box="true"
          />
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
      </div>
      <div class="row">
        <div class="form-group col-md-4">
          <label>Agrupar por</label>
          <select class="form-control input-sm" v-model="filtro.agrupar">
            <option value="1" selected="true">Lotação</option>
            <option value="2">Cargo</option>
            <option value="3">Categoria</option>
            <option value="4">Vinculo</option>
            <option value="7">Senador(a)</option>
            <option value="5">Ano</option>
            <option value="6">Não agrupar</option>
          </select>
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
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('1')">Lotação</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('2')">Cargo</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('3')">Categoria</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('4')">Vinculo</button>
              <button type="button" class="list-group-item list-group-item-action" v-on:click="Detalhar('7')">Senador(a)</button>
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
      loading: false,
      pageLoad: true,
      selectedRow: {},
      valorTotal: null,
      filtro: {
        agrupar: '7',
        ano: '2023',
        mes: '',
        lotacao: [],
        cargo: [],
        categori: [],
        vinculo: [],
        parlamentar: [],
      },

      lotacoes: [],
      cargos: [],
      categorias: [],
      vinculos: [],
      parlamentares: [],

      options: {
        ajax(objData, callback) {
          if (objData.draw === 1) return;
          // if (!vm.loading) vm.loading = true;
          // else return;

          const loader = vm.$loading.show();
          const newData = objData;
          delete newData.columns;
          delete newData.search;

          newData.filters = {
            ag: vm.filtro.agrupar,
            an: vm.filtro.ano,
            ms: vm.filtro.mes,
            lt: (vm.filtro.lotacao || []).join(','),
            cr: (vm.filtro.cargo || []).join(','),
            ct: (vm.filtro.categoria || []).join(','),
            vn: (vm.filtro.vinculo || []).join(','),
            sn: (vm.filtro.parlamentar || []).join(','),
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
            .post(`${process.env.VUE_APP_API}/senador/remuneracao`, newData)
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
        order: [],
      },
      fields: {},
    };
  },
  mounted() {
    const vm = this;

    vm.filtro.agrupar = vm.qs.ag || '7';
    vm.filtro.ano = vm.qs.an || '2023';
    vm.filtro.mes = vm.qs.ms || '';
    vm.filtro.lotacao = (vm.qs.lt ? vm.qs.lt.split(',') : []);
    vm.filtro.cargo = (vm.qs.cr ? vm.qs.cr.split(',') : []);
    vm.filtro.categoria = (vm.qs.ct ? vm.qs.ct.split(',') : []);
    vm.filtro.vinculo = (vm.qs.vn ? vm.qs.vn.split(',') : []);
    vm.filtro.parlamentar = (vm.qs.sn ? vm.qs.sn.split(',') : []);

    document.title = 'OPS :: Remuneração no Senado';

    axios.get(`${process.env.VUE_APP_API}/senador/lotacao`).then((response) => {
      this.lotacoes = response.data;
    });

    axios.get(`${process.env.VUE_APP_API}/senador/cargo`).then((response) => {
      this.cargos = response.data;
    });

    axios.get(`${process.env.VUE_APP_API}/senador/categoria`).then((response) => {
      this.categorias = response.data;
    });

    axios.get(`${process.env.VUE_APP_API}/senador/vinculo`).then((response) => {
      this.vinculos = response.data;
    });

    axios.get(`${process.env.VUE_APP_API}/senador`).then((response) => {
      this.parlamentares = response.data;
    });

    this.Pesquisar(true);
  },
  methods: {
    AbrirModalDetalhar(data) {
      this.selectedRow = data;
      jQuery('#modal-detalhar').modal();
    },
    Detalhar(agrupar) {
      const vm = this;

      switch (vm.filtro.agrupar) {
        case '1': // Lotação
          vm.filtro.lotacao = [this.selectedRow.id];
          break;
        case '2': // Cargo
          vm.filtro.cargo = [this.selectedRow.id];
          break;
        case '3': // Categoria
          vm.filtro.categoria = [this.selectedRow.id];
          break;
        case '4': // Vinculo
          vm.filtro.vinculo = [this.selectedRow.id];
          break;
        case '6': // Senador
          vm.filtro.parlamentar = [this.selectedRow.id];
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
      vm.senador = {};
      vm.pageLoad = pageLoad;

      if(vm.ultimoAgrupamento == vm.filtro.agrupar) {
        vm.$refs.table.reload();
        return;
      }

      vm.fields = null;

      vm.$nextTick(() => {
        switch (vm.filtro.agrupar) {
          case '1': // Lotação
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
                label: 'Lotação',
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
                defaultOrder: 'desc',
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
                defaultOrder: 'desc',
              },
            };
            break;

          case '3': // Categoria
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
                label: 'Categoria',
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
                defaultOrder: 'desc',
              },
            };
            break;

          case '4': // Vinculo
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
                label: 'Vinculo',
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
                defaultOrder: 'desc',
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
                defaultOrder: 'desc',
              },
            };
            break;

          case '7': // Senador
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
                label: 'Senador',
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
                defaultOrder: 'desc',
              },
            };
            break;

          case '6': // Mês
            vm.fields = {
              vinculo: {
                label: 'Vinculo',
                sortable: true,
              },
              categoria: {
                label: 'Categoria',
                render: (data, type, full) => {
                  if (type === 'display') {
                    return data + (full.simbolo_funcao ? ` (${full.simbolo_funcao})` : '');
                  }
                  return data;
                },
                sortable: true,
              },
              cargo: {
                label: 'Cargo',
                render: (data, type, full) => {
                  if (type === 'display') {
                    return data + (full.referencia_cargo ? ` (${full.referencia_cargo})` : '');
                  }
                  return data;
                },
              },
              lotacao: {
                label: 'Lotação',
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
              valor_total: {
                label: 'Custo Total',
                render: (data, type, full) => {
                  if (type === 'display') {
                    return `<a href="/senado/remuneracao/${full.id}">${data}</a>`;
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

        vm.ultimoAgrupamento = vm.filtro.agrupar;
        if(!pageLoad && vm.options.order.length > 0){
          vm.options.order = [];
        }

        vm.$nextTick(() => {
          vm.$refs.table.reload(null, true);
        });
      });
    },
    LimparFiltros() {
      this.filtro = {
        agrupar: '1',
        ano: '2023',
        mes: '',
        lotacao: [],
        cargo: [],
        categori: [],
        vinculo: [],
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
