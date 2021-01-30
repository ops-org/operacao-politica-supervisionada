<template>
  <div class="container-fluid">
    <h3 class="page-title">Secretários Parlamentares - <a v-bind:href="'/deputado-federal/' + deputado.id_cf_deputado">{{deputado.nome_parlamentar}}</a></h3>

    <div class="form-group">
      <p class="mb-0">
        <strong>Secretários em exercício:</strong>
        {{deputado.quantidade_secretarios}}
      </p>
      <p class="mb-0">
        <strong>Custo Mensal Total:</strong>
        R$ {{deputado.custo_secretarios}}
      </p>
    </div>

    <div class="form-group">
      <h5 class="page-title">Secretários Parlamentares Ativos</h5>
      <vdtnet-table ref="table-ativos" :fields="fieldsAtivos" :opts="optionsAtivos"></vdtnet-table>
    </div>

    <!-- <div class="form-group">
      <h5 class="page-title">Recebimentos por Secretário</h5>
      <vdtnet-table ref="table-inativos" :fields="fieldsInativos" :opts="optionsInativos"></vdtnet-table>
    </div> -->

    <p style="padding-bottom: 20px;">
        Fonte: <a
          v-bind:href="'https://www.camara.leg.br/deputados/' + deputado.id_cf_deputado + '/pessoal-gabinete?ano=2020'"
          target="_blank">Câmara de Deputados - Pessoal de gabinete 2020</a>
    </p>
  </div>
</template>

<script>
// this demonstrate with buttons and responsive master/details row
import 'jquery';
import VdtnetTable from 'vue-datatables-net';
import 'datatables.net-bs4';
import axios from 'axios';

export default {
  name: 'App',
  components: { VdtnetTable },
  props: {
    id: Number,
  },
  data() {
    const vm = this;

    return {
      deputado: {},

      optionsAtivos: {
        ajax(data, callback) {
          const newData = data;
          delete newData.columns;
          delete newData.search;

          newData.filters = {
            ativo: 1,
          };

          axios
            .post(`${process.env.VUE_APP_API}/deputado/${vm.id}/secretariosativos`, newData)
            .then((response) => {
              callback(response.data);
            });
        },
        pageLength: 100,
        dom: "tr<'row vdtnet-footer'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
      },
      fieldsAtivos: {
        nome: {
          label: 'Nome do Secretário',
          sortable: true,
          searchable: true,
          render: (data, type, full) => {
            if (type === 'display') {
              return `<a href="https://www.camara.leg.br/deputados/remuneracao-pessoal-gabinete/${full.link}" target="_blank">${data}</a>`;
            }
            return data;
          },
        },
        cargo: {
          label: 'Cargo/Função',
          sortable: true,
        },
        periodo: {
          label: 'Período do Exercício',
          sortable: true,
        },
        valor_bruto: {
          label: 'Remuneração Bruta',
          sortable: true,
          className: 'text-right',
        },
        valor_liquido: {
          label: 'Remuneração Liquida',
          sortable: true,
          className: 'text-right',
        },
        valor_outros: {
          label: 'Benefícios',
          sortable: true,
          className: 'text-right',
        },
        custo_total: {
          label: 'Custo Total',
          sortable: true,
          className: 'text-right',
        },
        referencia: {
          label: 'Referência',
          sortable: true,
        },
      },

      // optionsInativos: {
      //   ajax(data, callback) {
      //     const newData = data;
      //     delete newData.columns;
      //     delete newData.search;

      //     newData.filters = {
      //       ativo: 0,
      //     };

      //     axios
      //       .post(`${process.env.VUE_APP_API}/deputado/${vm.id}/secretarioshistorico`, newData)
      //       .then((response) => {
      //         callback(response.data);
      //       });
      //   },
      //   pageLength: 100,
      //   dom: "tr<'row vdtnet-footer'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
      // },
      // fieldsInativos: {
      //   nome: {
      //     label: 'Nome do Secretário',
      //     sortable: true,
      //     render: (data, type, full) => {
      //       if (type === 'display') {
      //         return `<a href="https://www.camara.leg.br/deputados/remuneracao-pessoal-gabinete/${full.link}" target="_blank">${data}</a>`;
      //       }
      //       return data;
      //     },
      //   },
      //   custo_total: {
      //     label: 'Custo Total',
      //     sortable: true,
      //     className: 'text-right',
      //   },
      // },
    };
  },
  mounted() {
    const loader = this.$loading.show();

    axios
      .get(`${process.env.VUE_APP_API}/deputado/${this.id}`)
      .then((response) => {
        this.deputado = response.data;

        loader.hide();
      });
  },
};
</script>

<style scoped>
</style>
