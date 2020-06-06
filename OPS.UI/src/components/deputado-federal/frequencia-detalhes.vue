<template>
  <div class="container">
    <h3 class="page-title">Frequência nas sessões plenárias da Câmara Federal</h3>

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
  props: {
    id: Number,
  },
  data() {
    const self = this;

    return {
      options: {
        ajax(data, callback) {
          const newData = data;
          delete newData.columns;
          delete newData.search;

          axios
            .post(`http://localhost:5000/api/Deputado/Frequencia/${self.id}`, newData)
            .then((response) => {
              callback(response.data);
            });
        },
        processing: true,
        searching: false,
        destroy: true,
        ordering: false,
        serverSide: true,
        fixedHeader: true,
        saveState: true,
        lengthMenu: [[15, 100, 500, 1000], [15, 100, 500, 1000]],
        pageLength: 100,
      },
      fields: {
        nome_parlamentar: {
          label: 'Parlamentar',
          sortable: true,
          render: (data, type, full) => {
            if (type === 'display') {
              return `<a href="/deputado-federal/${full.id_cf_deputado}">${data}</a>`;
            }
            return data;
          },
        },
        presenca: {
          label: 'Presença',
          sortable: true,
        },
        justificativa: {
          label: 'Justificativa',
          sortable: true,
        },
      },
    };
  },
};
</script>

<style scoped>
</style>
