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
import axios from 'axios';

export default {
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
            .post(`${process.env.VUE_APP_API}/deputado/frequencia`, newData)
            .then((response) => {
              callback(response.data);

              loader.hide();
            });
        },
        pageLength: 100,
        dom: "tr<'row vdtnet-footer'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
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
          isLocal: false,
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
