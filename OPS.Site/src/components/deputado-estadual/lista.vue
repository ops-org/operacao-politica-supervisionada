<template>
  <div class="container">
    <h3 class="page-title">
      [BETA] Cota para Exercício da Atividade Parlamentar (CEAP)
    </h3>

    <form id="form" autocomplete="off">
      <div class="row">
        <div class="form-group col-md-4">
          <label>Legislatura</label>
          <select class="form-control input-sm" v-model="filtro.periodo">
            <option value="57">57ª (fev/2023 à jan/2027)</option>
            <option value="56">56ª (fev/2019 à jan/2023)</option>
            <option value="55">55ª (fev/2015 à jan/2019)</option>
            <option value="54">54ª (fev/2011 à jan/2015)</option>
            <option value="53">53ª (fev/2007 à jan/2011)</option>
          </select>
        </div>
        <div class="form-group col-md-4">
          <label>Deputado / Liderança</label>
          <multiselect
            v-model="filtro.parlamentar"
            :options="parlamentares"
            :multiple="true"
            placeholder="Selecione"
            :close-on-select="false"
            :clear-on-select="false"
            :preserve-search="true"
            label="text"
            track-by="id"
            :searchable="true"
            :loading="isLoadingParlamentar"
            :internal-search="false"
            @search-change="BuscaParlamentar"
          >
            <template slot="selection" slot-scope="{ values, isOpen }">
              <span
                class="multiselect__single"
                v-if="values.length > 1 && !isOpen"
                >{{ values.length }} item(ns) selecionado(s)</span
              >
            </template>
            <span slot="noResult">Oops! Nenhum resultado encontrado.</span>
            <span slot="noOptions">Digite o nome do parlamentar.</span>
          </multiselect>
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
                v-on:click="AbreModalConsultaFornecedor()"
                title="Localizar Fornecedor"
              >
                <span class="fa fa-search"></span>
              </button>
              <button
                type="button"
                class="btn btn-outline-secondary"
                v-on:click="LimparFiltroFornecedor()"
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
            <option value="1">Deputado / Liderança</option>
            <option value="2">Despesa</option>
            <option value="3">Fornecedor</option>
            <option value="4">Partido</option>
            <option value="5">Estado</option>
            <option value="6">Recibo</option>
          </select>
        </div>
        <!--<div class="form-group col-md-4" v-if="filtro.agrupar=='6'">
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
                <option value="2021">2021</option>
                <option value="2022">2022</option>
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
                <option value="2021">2021</option>
                <option value="2022">2022</option>
              </select>
            </div>

            <div class="clearfix"></div>
          </div>
        </div>-->
      </div>
      <div class="row">
        <div class="form-group col-md-12">
          <input
            type="button"
            id="ButtonPesquisar"
            v-on:click="Pesquisar(false)"
            value="Pesquisar"
            class="btn btn-danger btn-sm"
          />&nbsp;
          <input
            type="button"
            value="Limpar filtros"
            class="btn btn-light btn-sm"
            v-on:click="LimparFiltros()"
          />

          <a
            href="https://institutoops.org.br/als-pedidos-via-lai/"
            class="btn btn-sm btn-link float-right"
            title="Solicitação de cópia das notas fiscais à assembleia"
            target="_blank"
            rel="nofollow noopener noreferrer"
            >Pedido de Informação via e-SIC</a
          >
        </div>
      </div>
    </form>

    <!-- <div class="row">
      <div class="col-md-12">
        <div class="alert alert-warning" v-if="valorTotal">
          <strong>Valor Total no Período: R$ {{valorTotal}}</strong>
          <small class="help-block mb-0">Valor total considerando os filtros aplicados acima</small>
        </div>
      </div>
    </div> -->

    <div class="form-group" v-if="fields">
      <vdtnet-table
        ref="table"
        :fields="fields"
        :opts="options"
        @edit="AbrirModalDetalhar"
      ></vdtnet-table>
    </div>

    <div
      class="modal"
      tabindex="-1"
      role="dialog"
      id="modal-detalhar"
      aria-hidden="true"
    >
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Detalhar por:</h5>
            <button
              type="button"
              class="close"
              data-dismiss="modal"
              aria-label="Close"
            >
              <span aria-hidden="true">&times;</span>
            </button>
          </div>
          <div class="list-group list-group-flush">
            <button
              type="button"
              class="list-group-item list-group-item-action"
              v-on:click="Detalhar('1')"
            >
              Deputado / Liderança
            </button>
            <button
              type="button"
              class="list-group-item list-group-item-action"
              v-on:click="Detalhar('2')"
            >
              Despesa
            </button>
            <button
              type="button"
              class="list-group-item list-group-item-action"
              v-on:click="Detalhar('3')"
            >
              Fornecedor
            </button>
            <button
              type="button"
              class="list-group-item list-group-item-action"
              v-on:click="Detalhar('4')"
            >
              Partido
            </button>
            <button
              type="button"
              class="list-group-item list-group-item-action"
              v-on:click="Detalhar('5')"
            >
              Estado
            </button>
            <button
              type="button"
              class="list-group-item list-group-item-action"
              v-on:click="Detalhar('6')"
            >
              Recibo
            </button>
          </div>
          <div class="modal-footer">
            <button
              type="button"
              class="btn btn-secondary"
              data-dismiss="modal"
            >
              Cancelar
            </button>
          </div>
        </div>
      </div>
    </div>

    <div
      id="modal-fornecedor"
      class="modal fade"
      tabindex="-1"
      role="dialog"
      style="display: none"
      aria-hidden="true"
    >
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h4 class="modal-title">Pesquisar Fornecedor</h4>
            <button
              type="button"
              class="close"
              data-dismiss="modal"
              aria-label="Close"
            >
              <span aria-hidden="true">×</span>
            </button>
          </div>
          <div class="modal-body text-justify">
            <form class="form-group">
              <div class="form-group">
                <label for="inputNome">Nome</label>
                <input
                  type="text"
                  class="form-control"
                  v-model="fornecedor_busca.nome"
                  placeholder="Informe um nome"
                />
              </div>
              <div class="form-group">
                <label for="inputCpfCnpj">CPF / CNPJ</label>
                <input
                  type="text"
                  class="form-control"
                  v-model="fornecedor_busca.cnpj"
                  placeholder="Informe um CPF ou CNPJ"
                />
              </div>

              <button
                type="button"
                class="btn btn-primary"
                v-on:click="ConsultaFornecedor()"
              >
                Pesquisar
              </button>
              <button
                type="reset"
                class="btn btn-light"
                v-on:click="LimparFiltroFornecedor()"
              >
                Limpar
              </button>
            </form>

            <div class="list-group" v-if="fornecedores">
              <div class="list-group-item">Fornecedores</div>
              <a
                href="javascript:void(0);"
                class="list-group-item"
                v-for="row in fornecedores"
                :key="row.id_fornecedor"
                v-on:click="SelecionarFornecedor(row)"
              >
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
        periodo: "57",
        parlamentar: [],
        despesa: [],
        estado: [],
        partido: [],
        fornecedor: {},
      },
      fornecedor_busca: {},

      estados: [
        // {"id":12,"text":"Acre (AC)"},
        // {"id":27,"text":"Alagoas (AL)"},
        // {"id":16,"text":"Amapá (AP)"},
        // {"id":13,"text":"Amazonas (AM)"},
        { id: 29, text: "Bahia (BA)" },
        // {"id":23,"text":"Ceará (CE)"},
        { id: 53, text: "Distrito Federal (DF)" },
        // {"id":32,"text":"Espírito Santo (ES)"},
        { id: 52, text: "Goiás (GO)" },
        // {"id":21,"text":"Maranhão (MA)"},
        // {"id":51,"text":"Mato Grosso (MT)"},
        // {"id":50,"text":"Mato Grosso do Sul (MS)"},
        { id: 31, text: "Minas Gerais (MG)" },
        // {"id":15,"text":"Pará (PA)"},
        // {"id":25,"text":"Paraíba (PB)"},
        // {"id":41,"text":"Paraná (PR)"},
        // {"id":26,"text":"Pernambuco (PE)"},
        // {"id":22,"text":"Piauí (PI)"},
        // {"id":33,"text":"Rio de Janeiro (RJ)"},
        // {"id":24,"text":"Rio Grande do Norte (RN)"},
        // {"id":43,"text":"Rio Grande do Sul (RS)"},
        // {"id":11,"text":"Rondônia (RO)"},
        // {"id":14,"text":"Roraima (RR)"},
        { id: 42, text: "Santa Catarina (SC)" },
        { id: 35, text: "São Paulo (SP)" },
        // {"id":28,"text":"Sergipe (SE)"},
        // {"id":17,"text":"Tocantins (TO)"}
      ],
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
            IdParlamentar: window.GetIds(vm.filtro.parlamentar).join(","),
            Despesa: (vm.filtro.despesa || []).join(","),
            Estado: (vm.filtro.estado || []).join(","),
            Partido: (vm.filtro.partido || []).join(","),
            Fornecedor: vm.filtro.fornecedor.id || null,
          };

          jQuery.each(newData.filters, (key, value) => {
            if (value === "" || value === null) {
              delete newData.filters[key];
            }
          });

          if (!vm.pageLoad) {
            vm.$router.push(
              { path: "deputado-estadual", query: newData.filters },
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
              `${process.env.VUE_APP_API}/deputadoestadual/lancamentos`,
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

    if (vm.qs.IdParlamentar) {
      var pDeputado = axios.post(
        `${process.env.VUE_APP_API}/deputadoestadual/pesquisa`,
        { ids: vm.qs.IdParlamentar }
      );
      pDeputado.then((response) => {
        vm.filtro.parlamentar = response.data;
      });

      lstPromises.push(pDeputado);
    }

    vm.filtro.agrupar = vm.qs.Agrupamento || "1";
    vm.filtro.periodo = vm.qs.Periodo || "57";
    vm.filtro.despesa = vm.qs.Despesa ? vm.qs.Despesa.split(",") : [];
    vm.filtro.estado = vm.qs.Estado ? vm.qs.Estado.split(",") : [];
    vm.filtro.partido = vm.qs.Partido ? vm.qs.Partido.split(",") : [];
    vm.filtro.fornecedor = vm.qs.Fornecedor
      ? { id: vm.qs.Fornecedor, nome: vm.qs.Fornecedor }
      : {};

    document.title = "OPS :: Cota Parlamentar na Câmara dos Deputados";

    // axios.get(`${process.env.VUE_APP_API}/estado`).then((response) => {
    //   this.estados = response.data;
    // });

    axios.get(`${process.env.VUE_APP_API}/partido`).then((response) => {
      this.partidos = response.data;
    });

    axios
      .get(`${process.env.VUE_APP_API}/deputadoestadual/tipodespesa`)
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
    BuscaParlamentar(busca) {
      this.isLoadingParlamentar = true;

      axios
        .post(`${process.env.VUE_APP_API}/deputadoestadual/pesquisa`, {
          busca: busca,
          periodo: parseInt(this.filtro.periodo || "57"),
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
        case "1": // Deputado
          window.AddIfDontExists(
            vm.filtro.parlamentar,
            this.selectedRow.id_cl_deputado,
            this.selectedRow.nome_parlamentar
          );
          break;
        case "2": // Despesa
          vm.filtro.despesa = [this.selectedRow.id_cl_despesa_tipo];
          break;
        case "3": // Fornecedor
          vm.filtro.fornecedor = {
            id: this.selectedRow.id_fornecedor,
            cnpj: this.selectedRow.cnpj_cpf,
            nome: this.selectedRow.nome_fornecedor,
          };
          break;
        case "4": // Partido
          vm.filtro.partido = [this.selectedRow.id_partido];
          break;
        case "5": // Estado
          vm.filtro.estado = [this.selectedRow.id_estado];
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
          case "1": // Deputado
            vm.fields = {
              id_cl_deputado: {
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
              nome_parlamentar: {
                label: "Parlamentar",
                render: (data, type, full) => {
                  if (type === "display") {
                    return `<a href="/deputado-estadual/${full.id_cl_deputado}">${data}</a>`;
                  }
                  return data;
                },
                sortable: true,
              },
              sigla_estado: {
                label: "UF",
                sortable: true,
              },
              sigla_partido: {
                label: "Partido",
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
              id_cl_despesa_tipo: {
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

          case "4": // Partido
            vm.fields = {
              id_partido: {
                label: "&nbsp;",
                render: (data, type) => {
                  if (type === "display") {
                    return '<a href="javascript:void(0);" data-action="edit" class="btn btn-primary btn-sm">Detalhar</a>';
                  }
                  return data;
                },
                sortable: false,
              },
              nome_partido: {
                label: "Partido",
                sortable: true,
              },
              total_notas: {
                label: "Recibos",
                sortable: true,
                className: "text-right",
              },
              total_deputados: {
                label: "Deputados",
                sortable: true,
                className: "text-right",
              },
              valor_medio_por_deputado: {
                label: "Val. Médio Deputado",
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

          case "5": // Estado
            vm.fields = {
              id_estado: {
                label: "&nbsp;",
                render: (data, type) => {
                  if (type === "display") {
                    return '<a href="javascript:void(0);" data-action="edit" class="btn btn-primary btn-sm">Detalhar</a>';
                  }
                  return data;
                },
                sortable: false,
              },
              nome_estado: {
                label: "Estado",
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

          case "6": // Recibo
            vm.fields = {
              data_emissao: {
                label: "Emissão",
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
              nome_parlamentar: {
                label: "Parlamentar",
                render: (data, type, full) => {
                  if (type === "display") {
                    return `<a href="/deputado-estadual/${full.id_cl_deputado}">${data}</a><br><small>${full.sigla_partido} / ${full.sigla_estado}</small>`;
                  }
                  return data;
                },
                sortable: false,
              },
              valor_liquido: {
                label: "Valor",
                // render: (data, type, full) => {
                //   if (type === 'display') {
                //     return `<a href="/deputado-estadual/documento/${full.id_cl_despesa}">${data}</a>`;
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
        periodo: "57",
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
