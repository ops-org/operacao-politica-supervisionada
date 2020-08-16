
<template>
    <div>
      <div class="container">
          <h3 class="page-title">Busca</h3>

          <form id="form" autocomplete="off" v-on:submit.prevent="buscar()">
              <div class="row">
                  <div class="col-md-12 form-group">
                      <input type="text" id="txtBusca" class="form-control input-sm" placeholder="Buscar por deputado, senador ou empresa" v-model="q" />
                  </div>

                  <div class="col-md-12 form-group">
                      <input type="submit" id="ButtonPesquisar" value="Pesquisar" class="btn btn-danger btn-sm" />
                  </div>
              </div>
          </form>

          <div class="alert alert-warning" v-if="deputado_federal.length===0&&senador.length===0&&fornecedor.length===0">
              Nenhum resultado encontrado!
          </div>
      </div>

      <div class="container">
        <div class="row">
              <div class="col-md-12 form-group" v-if="senador.length!==0">
                  <h4>{{senador.length}} senador(es) encontrado(s)</h4>
                  <div class="row senador-conheca">
                      <div class="col-xs-12 col-sm-6 col-md-4 senador" v-for="senador in senador" :key="senador.id_sf_senador">
                          <div class="card form-group">
                              <div class="card-header bg-light">
                                  <a v-bind:href="'./senador/' + senador.id_sf_senador" title="Clique para visualizar o perfil do senador(a)">
                                      {{senador.nome}}
                                  </a>
                              </div>
                              <div class="row no-gutters">
                                  <div class="col-md-4">
                                      <a v-bind:href="'./senador/' + senador.id_sf_senador" title="Clique para visualizar o perfil do senador(a)">
                                          <img class="media-thumbnail" v-lazy="'https://ops.net.br/api/senador/imagem/' + senador.id_sf_senador + '_120x160'" />
                                      </a>
                                  </div>
                                  <div class="col-md-8">
                                      <div class="card-body p-3">
                                          <h6 class="card-title mb-0" v-bind="senador.nome_completo"></h6>
                                          <p class="mb-0">
                                              <span v-bind:title="senador.nome_partido">{{senador.sigla_partido}}</span> - <span v-bind:title="senador.nome_estado">{{senador.sigla_estado}}</span><br />
                                              <small class="text-muted">Cota parlamentar</small><br />
                                              R$ {{senador.valor_total_ceaps}}
                                          </p>
                                      </div>
                                  </div>
                              </div>
                          </div>
                      </div>
                  </div>
              </div>
          </div>

          <div class="row">
              <div class="col-md-12 form-group" v-if="deputado_federal.length!==0">
                  <h4>{{deputado_federal.length}} deputado(s) encontrado(s)</h4>
                  <div class="row deputado-conheca">
                      <div class="col-xs-12 col-sm-6 col-md-4 deputado" v-for="deputado in deputado_federal" :key="deputado.id_cf_deputado">
                          <div class="card form-group">
                              <div class="card-header bg-light">
                                  <a v-bind:href="'./deputado-federal/' + deputado.id_cf_deputado" title="Clique para visualizar o perfil do deputado(a)">
                                      {{deputado.nome_parlamentar}}
                                  </a>
                              </div>
                              <div class="row no-gutters">
                                  <div class="col-md-4">
                                      <a v-bind:href="'./deputado-federal/' + deputado.id_cf_deputado" title="Clique para visualizar o perfil do deputado(a)">
                                          <img class="media-thumbnail" v-lazy="'https://ops.net.br/api/deputado/imagem/' + deputado.id_cf_deputado" />
                                      </a>
                                  </div>
                                  <div class="col-md-8">
                                      <div class="card-body p-3">
                                          <h6 class="card-title mb-0" v-bind="deputado.nome_civil"></h6>
                                          <p class="mb-0">
                                              <span v-bind:title="deputado.nome_partido">{{deputado.sigla_partido}}</span> - <span v-bind:title="deputado.nome_estado">{{deputado.sigla_estado}}</span><br />
                                              <small class="text-muted">Cota parlamentar</small><br />
                                              R$ {{deputado.valor_total_ceap}}
                                          </p>
                                      </div>
                                  </div>
                              </div>
                          </div>
                      </div>
                  </div>
              </div>
          </div>

          <div class="row">
              <div class="col-md-12" v-if="fornecedor.length!==0">
                  <div class="row fornecedor-conheca">
                      <div class="list-group form-group" style="width: 100%">
                          <div class="list-group-item active">
                              {{fornecedor.length}} empresas encontradas
                          </div>
                          <a v-for="fornecedor in fornecedor" :key="fornecedor.id_fornecedor" v-bind:href="'./fornecedor/' + fornecedor.id_fornecedor" class="list-group-item">
                              {{fornecedor.cnpj}} - {{fornecedor.estado}}<br />
                              {{fornecedor.nome_fantasia}}<br />
                              {{fornecedor.nome}}
                          </a>
                      </div>
                  </div>
              </div>
          </div>
      </div>
  </div>
</template>

<script>
const axios = require('axios');

export default {
  name: 'Busca',
  data() {
    return {
      q: undefined,
      deputado_federal: [],
      senador: [],
      fornecedor: [],
    };
  },
  mounted() {
    this.q = this.$route.query.q;

    this.buscar();
  },
  methods: {
    buscar() {
      if (this.q) {
        const loader = this.$loading.show();

        this.$router.push({ path: '/busca', query: { q: this.q } });

        axios
          .get(`${process.env.API}/indicadores/busca?value=${this.q}`)
          .then((response) => {
            this.deputado_federal = response.data.deputado_federal;
            this.senador = response.data.senador;
            this.fornecedor = response.data.fornecedor;

            loader.hide();
          });
      } else {
        this.deputado_federal = [];
        this.senador = [];
        this.fornecedor = [];
      }
    },
  },
};
</script>
