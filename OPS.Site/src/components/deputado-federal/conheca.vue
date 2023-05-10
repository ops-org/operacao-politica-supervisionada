<template>
<div class="container">
    <h3 class="page-title">Deputados federais</h3>

    <p>
        Resumo dos deputados federais, seus respectivos gastos com a cota parlamentar (CEAP) e com Folha de pagamento (Propria e de seus secretários).
        * Valores acumulados de todas as legislaturas.
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

    <div class="row">
      <div class="col-md-12">
        <div class="alert alert-warning" v-if="deputado_federal.length > 0">
          <strong>Exibindo {{deputado_federal.length}} deputados</strong>
        </div>
      </div>
    </div>

    <div class="row deputado-conheca">
        <div class="col-xs-12 col-sm-6 col-md-4" v-for="deputado in deputado_federal" :key="deputado.id_cf_deputado">

            <div class="card border-primary mb-3">
                <div class="card-header text-truncate" v-bind:class="{ 'text-white bg-primary': deputado.ativo }">
                    <a v-bind:href="'/#/deputado-federal/' + deputado.id_cf_deputado" title="Clique para visualizar o perfil do deputado(a)">
                        <small class="float-right">* {{deputado.situacao}}</small>
                        {{deputado.nome_parlamentar}}<br>
                        <small>{{deputado.nome_civil}}</small>
                    </a>
                </div>
                <div class="card-body p-2">
                    <div class="row no-gutters">
                        <div class="col-md-4">
                            <a v-bind:href="'/#/deputado-federal/' + deputado.id_cf_deputado" title="Clique para visualizar o perfil do deputado(a)">
                                <img class="media-thumbnail card-img" v-lazy="'//static.ops.net.br/depfederal/' + deputado.id_cf_deputado + '.jpg'" />
                            </a>
                        </div>
                        <div class="col-md-8">
                            <div class="card-body p-0">
                                <h6 class="card-title mb-1"><span v-bind:title="deputado.nome_partido">{{deputado.sigla_partido}}</span> - <span v-bind:title="deputado.nome_estado">{{deputado.sigla_estado}}</span></h6>
                                <small class="text-muted">Cota parlamentar</small>
                                <h6 class="card-text">R$ {{deputado.valor_total_ceap}}</h6>
                                <small class="text-muted">Folha de Pagamento</small>
                                <h6 class="card-text">R$ {{deputado.valor_total_remuneracao}}</h6>
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
import jQuery from 'jquery';

import VSelect from '../vue-bootstrap-select';
const axios = require('axios');

export default {
   components: {
    VSelect
  },
  data() {
    return {
      API: '',
      deputado_federal: {},
      filtro: {
        periodo: 57,
        estado: [],
        partido: [],
      },
      periodos: [
        { id: 57, text: '57ª (fev/2023 à jan/2027)' },
        { id: 56, text: '56ª (fev/2019 à jan/2023)' },
        { id: 55, text: '55ª (fev/2015 à jan/2019)' },
        { id: 54, text: '54ª (fev/2011 à jan/2015)' },
        { id: 53, text: '53ª (fev/2007 à jan/2011)' },
      ],
      estados: [],
      partidos: [],
    };
  },
  mounted() {
    document.title = 'OPS :: Deputado Federal';
    this.API = process.env.VUE_APP_API;

    axios
      .get(`${process.env.VUE_APP_API}/estado`)
      .then((response) => {
        this.estados = response.data;
      });

    axios
      .get(`${process.env.VUE_APP_API}/partido`)
      .then((response) => {
        this.partidos = response.data;
      });

    this.Pesquisar();
  },
  methods: {
    Pesquisar() {
      const vm = this;
      const loader = this.$loading.show();
      this.deputado_federal = {};

      const filtro = {
        periodo: vm.filtro.periodo,
        estado: (vm.filtro.estado || []).join(','),
        partido: (vm.filtro.partido || []).join(','),
      };

      jQuery.each(filtro, (key, value) => {
        if (value === '' || value === null) {
          delete filtro[key];
        }
      });

      axios
        .post(`${process.env.VUE_APP_API}/deputado/lista`, filtro)
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
      color: #000;
      text-decoration: none;
  }

  .deputado-conheca div.text-white a {
    color: #FFF;
  }

    img.card-img {
        width: 90px;
        height: 120px;
        border: 1px solid #dddddd;
    }
</style>
