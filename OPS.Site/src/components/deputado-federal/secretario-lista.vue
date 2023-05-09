<template>
  <div class="container">
    <h3 class="page-title">Secretários Parlamentares por Deputado Federal</h3>

    <div class="form-group">
        <vdtnet-table ref="table" :fields="fields" :opts="options"></vdtnet-table>
    </div>

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
  data() {
    const vm = this;

    return {
      options: {
        ajax(data, callback) {
          const loader = vm.$loading.show();
          const newData = data;
          delete newData.columns;
          delete newData.search;

          axios
            .post(`${process.env.VUE_APP_API}/deputado/secretarios`, newData)
            .then((response) => {
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
              return `<a href="/#/deputado-federal/${data}/secretario" class="btn btn-primary btn-sm">Ver secretários</a>`;
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
          label: 'Secretários Ativos',
          sortable: true,
        },
        custo_secretarios: {
          label: 'Custo Mensal',
          sortable: true,
        },
        // custo_total_secretarios: {
        //   label: 'Custo Total',
        //   sortable: true,
        // },
      },
    };
  },
};
</script>

<style scoped>
</style>
