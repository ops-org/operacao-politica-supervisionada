<template>
<div class="container-fluid">
    <h3 class="page-title">Deputados federais</h3>

    <p>
        Resumo dos deputados federais, seus respectivos gastos com a cota parlamentar (CEAP) e frequência nas sessões plenárias por legislatura (mandato).
    </p>
    <hr />

    <form id="form" autocomplete="off">
        <div class="row">
            <div class="form-group col-md-4">
                <label>Legislatura</label>
                <v-select :options="periodos" v-model.number="filtro.periodo" class="form-control input-sm" />
            </div>
            <div class="form-group col-md-4">
                <label>Estado</label>
                <v-select :options="estados" v-model="filtro.estado" class="form-control input-sm" multiple data-actions-box="true" />
            </div>
            <div class="form-group col-md-4">
                <label>Partido</label>
                <v-select :options="partidos" v-model="filtro.partido" class="form-control input-sm" multiple data-actions-box="true" />
            </div>
        </div>

        <div class="row">
            <div class="form-group col-md-12">
                <input type="button" id="ButtonPesquisar" v-on:click="Pesquisar();" value="Pesquisar" class="btn btn-danger btn-sm" />
                <input type="button" value="Limpar filtros" class="btn btn-light btn-sm" v-on:click="LimparFiltros();" />
            </div>
        </div>
    </form>
    <hr />

    <div class="row deputado-conheca">
        <div class="col-xs-12 col-sm-6 col-md-3" v-for="deputado in deputado_federal" :key="deputado.id_cf_deputado">

            <div class="card mb-3">
                <div class="card-header bg-light text-truncate">
                    <a v-bind:href="'/deputado-federal/' + deputado.id_cf_deputado" title="Clique para visualizar o perfil do deputado(a)">
                        {{deputado.nome_parlamentar}}
                    </a>
                </div>
                <div class="card-body p-2">
                    <div class="row no-gutters">
                        <div class="col-md-4">
                            <a v-bind:href="'/deputado-federal/' + deputado.id_cf_deputado" title="Clique para visualizar o perfil do deputado(a)">
                                <img class="media-thumbnail card-img" v-lazy="'https://ops.net.br/api/deputado/imagem/' + deputado.id_cf_deputado" />
                            </a>
                        </div>
                        <div class="col-md-8">
                            <div class="card-body p-0">
                                <h5 class="card-title mb-1"><span v-bind:title="deputado.nome_partido">{{deputado.sigla_partido}}</span> - <span v-bind:title="deputado.nome_estado">{{deputado.sigla_estado}}</span></h5>
                                <small class="text-muted">Cota parlamentar</small>
                                <h6 class="card-text">R$ {{deputado.valor_total_ceap}}</h6>
                                <small class="text-muted">Frequência nas sessões plenárias</small>
                                <h6 class="card-text" v-bind:title="'Compareceu à ' + deputado.total_presencas + ' de ' + deputado.total_sessoes + ' sessões'">{{deputado.total_presencas}}/{{deputado.total_sessoes}} - {{deputado.frequencia}}%</h6>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

</div>
</template>

<script>
import VSelect from '../vue-bootstrap-select';

const axios = require('axios');

export default {
  components: {
    VSelect,
  },
  data() {
    return {
      deputado_federal: {},
      filtro: {
        periodo: 9,
        estado: [],
        partido: [],
      },
      periodos: [
        { id: 9, text: '56º (2019-2023)' },
        { id: 8, text: '55º (2015-2019)' },
        { id: 7, text: '54º (2011-2015)' },
        { id: 6, text: '53º (2007-2011)' },
      ],
      estados: [],
      partidos: [],
    };
  },
  mounted() {
    document.title = 'OPS :: Deputado Federal';

    axios
      .get('http://localhost:5000/api/estado')
      .then((response) => {
        this.estados = response.data;
      });

    axios
      .get('http://localhost:5000/api/partido')
      .then((response) => {
        this.partidos = response.data;
      });

    this.Pesquisar();
  },
  methods: {
    Pesquisar() {
      const loader = this.$loading.show();
      this.deputado_federal = {};

      axios
        .post('http://localhost:5000/api/deputado/lista', this.filtro)
        .then((response) => {
          this.deputado_federal = response.data;

          loader.hide();
        });
    },
    LimparFiltros() {
      this.filtro = {
        periodo: 6,
        estado: [],
        partido: [],
      };
    },
  },
};
</script>

<style scoped>
    .deputado-conheca a {
        color: #333333;
        text-decoration: none;
    }

    img.card-img {
        width: 90px;
        height: 120px;
        border: 1px solid #dddddd;
    }
</style>
