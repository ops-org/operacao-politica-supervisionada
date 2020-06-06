<template>
  <div class="container-fluid">
    <h3 class="page-title">Secretários Parlamentares - <a v-bind:href="'/deputado-federal/' + deputado.id_cf_deputado">{{deputado.nome_parlamentar}}</a></h3>

    <div class="form-group">
      <p class="mb-0">
        <strong>Secretários em exercício:</strong>
        {{deputado.quantidade_secretarios}}
      </p>
      <p class="mb-0">
        <strong>Custo Mensal Total (03/2020):</strong>
        R$ {{deputado.custo_secretarios}}
      </p>
    </div>

    <div class="form-group">
        <vdtnet-table ref="table-ativos" :fields="fieldsAtivos" :opts="optionsAtivos"></vdtnet-table>
    </div>

    <div class="form-group">
        <vdtnet-table ref="table-inativos" :fields="fieldsInativos" :opts="optionsInativos"></vdtnet-table>
    </div>

    <p style="padding-bottom: 20px;">
        Fonte: <a target="_blank" v-bind:href="'https://www.camara.leg.br/deputados/' + deputado.id_cf_deputado + '/pessoal-gabinete?ano=2020'">Câmara de Deputados - Pessoal de gabinete 2020</a>
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
    const self = this;

    return {
      deputado: {},

      optionsAtivos: {
        ajax(data, callback) {
          const newData = data;
          delete newData.columns;
          delete newData.search;

          newData.filters = {
            ativo: true,
          };

          axios
            .post(`http://localhost:5000/api/deputado/${self.id}/secretarios`, newData)
            .then((response) => {
              callback(response.data);
            });
        },
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
      fieldsAtivos: {
        nome: {
          label: 'Nome do Secretário',
          sortable: true,
          searchable: true,
          render: (data, type, full) => {
            if (type === 'display') {
              return `<a href="${full.link}">${data}</a>`;
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

      optionsInativos: {
        ajax(data, callback) {
          const newData = data;
          delete newData.columns;
          delete newData.search;

          newData.filters = {
            ativo: false,
          };

          axios
            .post(`http://localhost:5000/api/deputado/${self.id}/secretarios`, newData)
            .then((response) => {
              callback(response.data);
            });
        },
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
      fieldsInativos: {
        nome: {
          label: 'Nome do Secretário',
          sortable: true,
          render: (data, type, full) => {
            if (type === 'display') {
              return `<a href="${full.link}">${data}</a>`;
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
    };
  },
  mounted() {
    axios
      .get(`http://localhost:5000/api/deputado/${this.id}`)
      .then((response) => {
        this.deputado = response.data;
      });
  },
};
</script>

<style scoped>
</style>
