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
import VdtnetTable from 'vue-datatables-net';
import axios from 'axios';

export default {
  components: { VdtnetTable },
  props: {
    id: Number,
  },
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
            .post(`http://localhost:5000/api/Deputado/Frequencia/${vm.id}`, newData)
            .then((response) => {
              callback(response.data);

              loader.hide();
            });
        },
        pageLength: 100,
        dom: "tr<'row vdtnet-footer'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
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
