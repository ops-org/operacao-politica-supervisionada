<template>
  <div class="container">
    <h3 class="page-title">Frequência nas sessões plenárias da Câmara Federal</h3>

    <div class="form-group">
        <vdtnet-table ref="table" :fields="fields" :opts="options"></vdtnet-table>
    </div>

    <p style="padding-bottom: 20px;">
      Fonte:
      <a
        target="_blank"
        href="http://www2.camara.leg.br/atividade-legislativa/plenario/resultadoVotacao"
      >Câmara de Deputados - Resultado da votação eletrônica e lista de presença</a>
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
  data() {
    return {
      options: {
        ajax(data, callback) {
          const newData = data;
          delete newData.columns;
          delete newData.search;

          axios
            .post('http://localhost:5000/api/Deputado/Frequencia', newData)
            .then((response) => {
              callback(response.data);
            });
        },
        // ajax: {
        //   url: 'http://localhost:5000/api/Deputado/Frequencia',
        //   type: 'POST',
        //   data(d) {
        //     const data = d;

        //     delete data.columns;
        //     delete data.search;

        //     // window.$.extend(data, ajax);

        //     return data;
        //   },
        //   dataSrc: 'data',
        // },
        processing: true,
        searching: false,
        destroy: true,
        ordering: true,
        serverSide: true,
        fixedHeader: true,
        saveState: true,
        lengthMenu: [[15, 100, 500, 1000], [15, 100, 500, 1000]],
        pageLength: 100,
      },
      fields: {
        id_cf_sessao: {
          isLocal: true,
          label: '&nbsp;',
          render: (data, type) => {
            if (type === 'display') {
              return `<a href="/deputado-federal/frequencia/${data}" class="btn btn-primary btn-sm">Detalhar sessão</a>`;
            }
            return data;
          },
          sortable: false,
        },
        numero: {
          label: 'Número',
          sortable: true,
        },
        inicio: {
          label: 'Ínicio sessão',
          sortable: true,
        },
        tipo: {
          label: 'Tipo',
          sortable: true,
        },
        presenca: {
          label: 'Presenças',
          sortable: true,
          render: (data, type, full) => {
            if (type === 'display') {
              return `${data} <small class="text-muted">(${full.presenca_percentual}%)</small>`;
            }
            return data;
          },
        },
        ausencia: {
          label: 'Ausencias',
          sortable: true,
          render: (data, type, full) => {
            if (type === 'display') {
              return `${data} <small class="text-muted">(${full.ausencia_percentual}%)</small>`;
            }
            return data;
          },
        },
        ausencia_justificada: {
          label: 'Ausencias Justificadas',
          sortable: true,
          render: (data, type, full) => {
            if (type === 'display' && full.ausencia_justificada_percentual > 0) {
              return `${data} <small class="text-muted">(${full.ausencia_justificada_percentual}%)</small>`;
            }
            return data;
          },
        },
        total: {
          label: 'Participantes',
          sortable: true,
        },
      },
    };
  },
};
</script>

<style scoped>
</style>
