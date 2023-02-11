<template>
  <div class="container">
    <h3 class="page-title">
      [BETA] Cartão de Pagamento dos Presidentes
    </h3>

    <form id="form" autocomplete="off">
      <div class="row">
        <div class="form-group col-md-2">
          <label>Ano</label>
          <v-select :options="anos"
                    v-model="filtro.ano"
                    class="form-control input-sm"
                    multiple
                    data-actions-box="true" />
        </div>
        <div class="form-group col-md-6">
          <label>Tipo de Despesa</label>
          <v-select :options="despesas"
                    v-model="filtro.despesa"
                    class="form-control input-sm"
                    multiple
                    data-actions-box="true" />
        </div>
        <div class="form-group col-md-4">
          <label>Fornecedor</label>
          <div class="input-group input-group-sm">
            <input type="text"
                   id="txtBeneficiario"
                   class="form-control input-sm"
                   disabled="disabled"
                   v-model="filtro.fornecedor.nome" />
            <div class="input-group-append">
              <button type="button"
                      class="btn btn-outline-secondary"
                      v-on:click="AbreModalConsultaFornecedor()"
                      title="Localizar Fornecedor">
                <span class="fa fa-search"></span>
              </button>
              <button type="button"
                      class="btn btn-outline-secondary"
                      v-on:click="LimparFiltroFornecedor()"
                      title="Limpar">
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
            <option value="1">Servidor</option>
            <option value="2">Despesa</option>
            <option value="3">Fornecedor</option>
            <option value="7">Ano</option>
            <option value="6">Recibo</option>
          </select>
        </div>
      </div>
      <div class="row">
        <div class="form-group col-md-12">
          <input type="button"
                 id="ButtonPesquisar"
                 v-on:click="Pesquisar(false)"
                 value="Pesquisar"
                 class="btn btn-danger btn-sm" />&nbsp;
          <input type="button"
                 value="Limpar filtros"
                 class="btn btn-light btn-sm"
                 v-on:click="LimparFiltros()" />
        </div>
      </div>
    </form>

    <div class="form-group" v-if="fields">
      <vdtnet-table ref="table"
                    :fields="fields"
                    :opts="options"
                    @edit="AbrirModalDetalhar"></vdtnet-table>
    </div>

    <div class="modal"
         tabindex="-1"
         role="dialog"
         id="modal-detalhar"
         aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Detalhar por:</h5>
            <button type="button"
                    class="close"
                    data-dismiss="modal"
                    aria-label="Close">
              <span aria-hidden="true">&times;</span>
            </button>
          </div>
          <div class="list-group list-group-flush">
            <button type="button"
                    class="list-group-item list-group-item-action"
                    v-on:click="Detalhar('1')">
              Servidor
            </button>
            <button type="button"
                    class="list-group-item list-group-item-action"
                    v-on:click="Detalhar('2')">
              Despesa
            </button>
            <button type="button"
                    class="list-group-item list-group-item-action"
                    v-on:click="Detalhar('3')">
              Fornecedor
            </button>
            <button type="button"
                    class="list-group-item list-group-item-action"
                    v-on:click="Detalhar('7')">
              Ano
            </button>
            <button type="button"
                    class="list-group-item list-group-item-action"
                    v-on:click="Detalhar('6')">
              Recido
            </button>
          </div>
          <div class="modal-footer">
            <button type="button"
                    class="btn btn-secondary"
                    data-dismiss="modal">
              Cancelar
            </button>
          </div>
        </div>
      </div>
    </div>

    <div id="modal-fornecedor"
         class="modal fade"
         tabindex="-1"
         role="dialog"
         style="display: none"
         aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h4 class="modal-title">Pesquisar Fornecedor</h4>
            <button type="button"
                    class="close"
                    data-dismiss="modal"
                    aria-label="Close">
              <span aria-hidden="true">×</span>
            </button>
          </div>
          <div class="modal-body text-justify">
            <form class="form-group">
              <div class="form-group">
                <label for="inputNome">Nome</label>
                <input type="text"
                       class="form-control"
                       v-model="fornecedor_busca.nome"
                       placeholder="Informe um nome" />
              </div>
              <div class="form-group">
                <label for="inputCpfCnpj">CPF / CNPJ</label>
                <input type="text"
                       class="form-control"
                       v-model="fornecedor_busca.cnpj"
                       placeholder="Informe um CPF ou CNPJ" />
              </div>

              <button type="button"
                      class="btn btn-primary"
                      v-on:click="ConsultaFornecedor()">
                Pesquisar
              </button>
              <button type="reset"
                      class="btn btn-light"
                      v-on:click="LimparFiltroFornecedor()">
                Limpar
              </button>
            </form>

            <div class="list-group" v-if="fornecedores">
              <div class="list-group-item">Fornecedores</div>
              <a href="javascript:void(0);"
                 class="list-group-item"
                 v-for="row in fornecedores"
                 :key="row.id_fornecedor"
                 v-on:click="SelecionarFornecedor(row)">
                <small>{{ row.cnpj_cpf }} </small><br />
                {{ row.nome }}
              </a>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
  import jQuery from "jquery";

  import VSelect from "../vue-bootstrap-select";
  const axios = require("axios");

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
        pageLoad: true,
        ultimoAgrupamento: 0,
        isLoadingParlamentar: null,
        selectedRow: {},
        valorTotal: null,
        deputado_federal: {},
        filtro: {
          agrupar: "1",
          ano: [],
          servidor: [],
          despesa: [],
          estado: [],
          partido: [],
          fornecedor: {},
        },
        fornecedor_busca: {},
        servidores: [],
        despesas: [],
        fornecedores: [],
        anos: [
            {id: 2022, text: "2022" },
            {id: 2021, text: "2021" },
            {id: 2020, text: "2020" },
            {id: 2019, text: "2019" },
            {id: 2018, text: "2018" },
            {id: 2017, text: "2017" },
            {id: 2016, text: "2016" },
            {id: 2015, text: "2015" },
            {id: 2014, text: "2014" },
            {id: 2013, text: "2013" },
            {id: 2012, text: "2012" },
            {id: 2011, text: "2011" },
            {id: 2010, text: "2010" },
            {id: 2009, text: "2009" },
            {id: 2008, text: "2008" },
            {id: 2007, text: "2007" },
            {id: 2006, text: "2006" },
            {id: 2005, text: "2005" },
            {id: 2004, text: "2004" },
            {id: 2003, text: "2003" }
        ],

        options: {
          ajax(objData, callback) {
            const loader = vm.$loading.show();
            const newData = objData;
            delete newData.columns;
            delete newData.search;

            newData.filters = {
              Agrupamento: vm.filtro.agrupar,
              Ano: (vm.filtro.ano || []).join(","),
              Servidor: window.GetIds(vm.filtro.servidor).join(","),
              Despesa: (vm.filtro.despesa || []).join(","),
              Fornecedor: vm.filtro.fornecedor.id || null,
            };

            jQuery.each(newData.filters, (key, value) => {
              if (value === "" || value === null) {
                delete newData.filters[key];
              }
            });

            if (!vm.pageLoad) {
              vm.$router.push(
                { path: "presidencia", query: newData.filters },
                () => {
                  /* Necesario para não fazer redirect */
                }
              );
            }

            this.fields = null;
            //this.fnSort([]);
            // vm.options.orders = [];

            axios
              .post(
                `${process.env.VUE_APP_API}/presidencia/lancamentos`,
                newData
              )
              .then((response) => {
                vm.valorTotal = response.data.valorTotal;
                callback(response.data);

                loader.hide();

                if (!vm.pageLoad) {
                  setTimeout(function () {
                    jQuery("html, body").animate(
                      {
                        scrollTop: jQuery(".vdtnet-container").offset().top,
                      },
                      500
                    );
                  }, 100);
                }
              });
          },
          pageLength: 50,
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
      var lstPromises = [];

      if (vm.qs.Servidor) {
        var servidorRequest = axios.post(
          `${process.env.VUE_APP_API}/presidencia/servidor`,
          { ids: vm.qs.Servidor }
        );
        servidorRequest.then((response) => {
          vm.filtro.parlamentar = response.data;
        });

        lstPromises.push(servidorRequest);
      }

      vm.filtro.agrupar = vm.qs.Agrupamento || "1";
      vm.filtro.ano = vm.qs.Ano ? vm.qs.Ano.split(",") : [];
      vm.filtro.despesa = vm.qs.Despesa ? vm.qs.Despesa.split(",") : [];
      vm.filtro.fornecedor = vm.qs.Fornecedor
        ? { id: vm.qs.Fornecedor, nome: vm.qs.Fornecedor }
        : {};

      document.title = "OPS :: Cartão de Pagamento dos Presidentes";

      // axios.get(`${process.env.VUE_APP_API}/estado`).then((response) => {
      //   this.estados = response.data;
      // });

      axios.get(`${process.env.VUE_APP_API}/partido`).then((response) => {
        this.partidos = response.data;
      });

      axios
        .get(`${process.env.VUE_APP_API}/presidencia/tipodespesa`)
        .then((response) => {
          this.despesas = response.data;
        });

      if (lstPromises.length == 0) {
        this.Pesquisar(true);
      } else {
        Promise.all(lstPromises).then(() => vm.Pesquisar(true));
      }
    },
    methods: {
      BuscaServidor(busca) {
        this.isLoadingParlamentar = true;

        axios
          .post(`${process.env.VUE_APP_API}/presidencia/servidor`, {
            busca: busca,
          })
          .then((response) => {
            this.parlamentares = response.data;

            this.isLoadingParlamentar = false;
          });
      },
      AbreModalConsultaFornecedor() {
        jQuery("#modal-fornecedor").modal();
      },
      ConsultaFornecedor() {
        const loader = this.$loading.show();

        axios
          .post(
            `${process.env.VUE_APP_API}/fornecedor/consulta`,
            this.fornecedor_busca
          )
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

        this.fornecedor_busca = {};
        this.fornecedores = [];
        jQuery("#modal-fornecedor").modal("hide");
      },
      LimparFiltroFornecedor() {
        this.fornecedores = [];
        this.fornecedor_busca = {};
        this.filtro.fornecedor = {};
      },
      AbrirModalDetalhar(data) {
        this.selectedRow = data;
        jQuery("#modal-detalhar").modal();
      },
      Detalhar(agrupar) {
        const vm = this;

        switch (vm.filtro.agrupar) {
          case "1": // Servidor
            window.AddIfDontExists(
              vm.filtro.parlamentar,
              this.selectedRow.id_servidor,
              this.selectedRow.nome_servidor
            );
            break;
          case "2": // Despesa
            vm.filtro.despesa = [this.selectedRow.id_pr_despesa_tipo];
            break;
          case "3": // Fornecedor
            vm.filtro.fornecedor = {
              id: this.selectedRow.id_fornecedor,
              cnpj: this.selectedRow.cnpj_cpf,
              nome: this.selectedRow.nome_fornecedor,
            };
            break;
          case "7": // Partido
            vm.filtro.ano = [this.selectedRow.ano];
            break;
          default:
            break;
        }

        vm.filtro.agrupar = agrupar;
        jQuery("#modal-detalhar").modal("hide");

        vm.Pesquisar(false);
      },
      Pesquisar(pageLoad) {
        const vm = this;
        vm.deputado_federal = {};
        vm.pageLoad = pageLoad;

        if (vm.ultimoAgrupamento == vm.filtro.agrupar) {
          vm.$refs.table.reload();
          return;
        }

        vm.fields = null;
        vm.$nextTick(() => {
          switch (vm.filtro.agrupar) {
            case "1": // Servidor
              vm.fields = {
                id_servidor: {
                  isLocal: true,
                  label: "&nbsp;",
                  render: (data, type) => {
                    if (type === "display") {
                      return '<a href="javascript:void(0);" data-action="edit" class="btn btn-primary btn-sm">Detalhar</a>';
                    }
                    return data;
                  },
                  sortable: false,
                },
                nome_servidor: {
                  label: "Servidor",
                  sortable: true,
                },
                total_notas: {
                  label: "Qtd. Recibos",
                  sortable: true,
                  className: "text-right",
                },
                valor_total: {
                  label: "Valor Total",
                  sortable: true,
                  className: "text-right",
                  defaultOrder: "desc",
                },
              };
              break;

            case "2": // Despesa
              vm.fields = {
                id_pr_despesa_tipo: {
                  label: "&nbsp;",
                  render: (data, type) => {
                    if (type === "display") {
                      return '<a href="javascript:void(0);" data-action="edit" class="btn btn-primary btn-sm">Detalhar</a>';
                    }
                    return data;
                  },
                  sortable: false,
                },
                descricao: {
                  label: "Despesa",
                  sortable: true,
                },
                total_notas: {
                  label: "Qtd. Recibos",
                  sortable: true,
                  className: "text-right",
                },
                valor_total: {
                  label: "Valor Total",
                  sortable: true,
                  className: "text-right",
                  defaultOrder: "desc",
                },
              };
              break;

            case "3": // Fornecedor
              vm.fields = {
                id_fornecedor: {
                  label: "&nbsp;",
                  render: (data, type) => {
                    if (type === "display") {
                      return '<a href="javascript:void(0);" data-action="edit" class="btn btn-primary btn-sm">Detalhar</a>';
                    }
                    return data;
                  },
                  sortable: false,
                },
                nome_fornecedor: {
                  label: "Fornecedor",
                  render: (data, type, full) => {
                    if (type === "display") {
                      if (full.id_fornecedor)
                        return `<a href="/fornecedor/${full.id_fornecedor}">${data}</a><br><small>${full.cnpj_cpf}</small>`;

                      return "";
                    }
                    return data;
                  },
                  sortable: true,
                },
                total_notas: {
                  label: "Qtd. Recibos",
                  sortable: true,
                  className: "text-right",
                },
                valor_total: {
                  label: "Valor Total",
                  sortable: true,
                  className: "text-right",
                  defaultOrder: "desc",
                },
              };
              break;

              case "7": // Ano
              vm.fields = {
                id_ano: {
                  label: "&nbsp;",
                  render: (data, type) => {
                    if (type === "display") {
                      return '<a href="javascript:void(0);" data-action="edit" class="btn btn-primary btn-sm">Detalhar</a>';
                    }
                    return data;
                  },
                  sortable: false,
                },
                ano: {
                  label: "Ano",
                  sortable: true,
                },
                total_notas: {
                  label: "Recibos",
                  sortable: true,
                  className: "text-right",
                },
                valor_total: {
                  label: "Valor Total",
                  sortable: true,
                  className: "text-right",
                  defaultOrder: "desc",
                },
              };
              break;

            case "6": // Recibo
              vm.fields = {
                data_pgto: {
                  label: "Pagamento",
                  sortable: true,
                },
                nome_fornecedor: {
                  label: "Fornecedor",
                  render: (data, type, full) => {
                    var texto = ``;
                    if (type === "display") {
                      if (full.id_fornecedor) {
                        texto = `<a href="/fornecedor/${full.id_fornecedor}">${data}</a><br><small>${full.cnpj_cpf}</small>`;

                        if (full.favorecido) texto += ` (${full.favorecido})`;
                        if (full.despesa_tipo)
                          texto += `<br><small>${full.despesa_tipo}</small>`;
                        if (full.despesa_especificacao)
                          texto += ` (<small>${full.despesa_especificacao}</small>)`;

                        return texto;
                      } else {
                        if (full.favorecido) texto += full.favorecido;
                        if (full.despesa_tipo)
                          texto += `<br><small>${full.despesa_tipo}</small>`;
                        if (full.despesa_especificacao)
                          texto += ` (<small>${full.despesa_especificacao}</small>)`;

                        return texto;
                      }
                    }
                    return data;
                  },
                  sortable: false,
                },
                nome_servidor: {
                  label: "Servidor",
                  render: (data, type, full) => {
                    if (type === "display") {
                      return `<a href="/presidencia/${full.id_servidor}">${data}</a>`;
                    }
                    return data;
                  },
                  sortable: false,
                },
                valor_liquido: {
                  label: "Valor",
                  // render: (data, type, full) => {
                  //   if (type === 'display') {
                  //     return `<a href="/presidencia/documento/${full.id_pr_despesa}">${data}</a>`;
                  //   }
                  //   return data;
                  // },
                  sortable: true,
                  className: "text-right",
                },
              };
              break;

            default:
              break;
          }

          vm.ultimoAgrupamento = vm.filtro.agrupar;
          if (!pageLoad && vm.options.order.length > 0) {
            vm.options.order = [];
          }

          // vm.$nextTick(() => {
          //   vm.$refs.table.reload();
          // });
        });
      },
      LimparFiltros() {
        this.filtro = {
          agrupar: "1",
          ano: [],
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
