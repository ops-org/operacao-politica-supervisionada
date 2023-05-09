<template>
  <div>
    <div class="container">
      <h3 class="page-title">Recibo: {{ documento.numero_documento || documento.id_documento }}</h3>

      <div class="alert alert-warning" role="alert">
        Atenção: Essa URL é dinâmica e pode mudar, portanto não a utilize para compartilhamento.
        Para compartilhamento utilize esse
        <a
          v-bind:href="documento.url_documentos_Deputado_beneficiario"
        >link</a>.
      </div>

      <div class="form-group">
        <div class="row">
          <div class="col-md-8">
            <p class="mb-1">
              <strong>Nome Parlamentar:</strong>
              <a
                v-bind:href="'/deputado-federal/' + documento.id_cf_deputado"
                title="Visualisar perfil do parlamentar"
              >{{documento.nome_parlamentar}}</a>
            </p>
          </div>
          <div class="col-md-4">
            <p class="mb-1">
              <strong>Partido / UF:</strong>
              {{documento.sigla_partido}} / {{documento.sigla_estado}}
            </p>
          </div>
        </div>
        <div class="row">
          <div class="col-md-8">
            <p class="mb-1">
              <strong>Beneficiário:</strong>
              <a
                v-bind:href="documento.url_beneficiario"
                title="Visualizar perfil do beneficiario"
              >{{documento.nome_fornecedor}}</a>
            </p>
          </div>
          <div class="col-md-4">
            <p class="mb-1">
              <strong>CNPJ / CPF:</strong>
              <a
                v-bind:href="documento.url_beneficiario"
                title="Visualizar perfil do beneficiario"
              >{{documento.cnpj_cpf}}</a>
            </p>
          </div>
        </div>
        <div class="row">
          <div class="col-md-8">
            <p class="mb-1">
              <strong>Despesa:</strong>
              {{documento.descricao_despesa}}
            </p>
          </div>
          <div class="col-md-4">
            <p class="mb-1">
              <strong>Data do pedido de reembolso:</strong>
              {{documento.competencia}}
            </p>
          </div>
        </div>
        <div class="row">
          <div class="col-md-4">
            <p class="mb-1">
              <strong>Valor da Despesa:</strong>
              {{documento.valor_documento}}
            </p>
          </div>
          <div class="col-md-4">
            <p class="mb-1">
              <strong>Valor Reembolsado:</strong>
              {{documento.valor_liquido}}
            </p>
          </div>
          <div class="col-md-4">
            <p class="mb-1">
              <strong>Data da Despesa:</strong>
              {{documento.data_emissao}}
            </p>
          </div>
        </div>
        <div class="row">
          <div class="col-md-4">
            <p class="mb-1">
              <strong>Tipo do Documento:</strong>
              {{documento.tipo_documento}}
            </p>
          </div>
          <div class="col-md-4" v-if="documento.nome_passageiro">
            <p class="mb-1">
              <strong>Nome do Passageiro:</strong>
              {{documento.nome_passageiro}}
            </p>
          </div>
          <div class="col-md-4" v-if="documento.trecho_viagem">
            <p class="mb-1">
              <strong>Trecho da Viagem:</strong>
              {{documento.trecho_viagem}}
            </p>
          </div>
        </div>
      </div>

      <div class="form-group text-center">
        <a
          v-bind:href="documento.url_documentos_Deputado_beneficiario"
          class="btn btn-primary"
        >Ver todas as notas do deputado para o beneficiário.</a>
      </div>

      <div class="form-group text-center">
        <span v-if="documento.id_documento">
          <a
            v-if="documento.url_documento"
            class="btn btn-danger"
            v-bind:href="documento.url_documento"
            target="_blank"
            rel="nofollow noopener noreferrer"
          >
            Recibo&nbsp;
            <i class="fa fa-download"></i>
          </a>
          <a
            v-if="documento.url_documento_nfe"
            class="btn btn-danger"
            v-bind:href="documento.url_documento_nfe"
            target="_blank"
            rel="nofollow noopener noreferrer"
          >
            Recibo (NF-e)&nbsp;
            <i class="fa fa-download"></i>
          </a>
          <a
            class="btn btn-light"
            v-bind:href="documento.url_detalhes_documento"
            target="_blank"
            rel="nofollow noopener noreferrer"
          >
            Detalhes do recibo&nbsp;
            <i class="fa fa-plus"></i>
          </a>
        </span>
        <a
          class="btn btn-light"
          v-bind:href="documento.url_demais_documentos_mes"
          target="_blank"
          rel="nofollow noopener noreferrer"
        >
          Demais Recibos do mês&nbsp;
          <i class="fa fa-plus"></i>
        </a>
        <a
          class="btn btn-light"
          href="https://www.nfe.fazenda.gov.br/portal/consultaRecaptcha.aspx?tipoConsulta=resumo&tipoConteudo=d09fwabTnLk="
          target="_blank"
          rel="nofollow noopener noreferrer"
        >
          Visualizar NFe&nbsp;
          <i class="fa fa-plus"></i>
        </a>
      </div>

      <div class="row form-group">
        <div class="col-xs-12 col-sm-6">
          <div class="card mb-3">
            <div class="card-header bg-light">Notas/recibos do dia</div>
            <div class="card-body">
              <div class="table-responsive">
                <table class="table table-striped table-hover table-sm">
                  <thead>
                    <tr>
                      <th>Beneficiário</th>
                      <th>UF</th>
                      <th>Valor</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="row in documentos_mesmo_dia" :key="row.id_fornecedor">
                      <td>
                        <a
                          v-bind:href="'/fornecedor/' + row.id_fornecedor"
                        >{{row.nome_fornecedor}}</a>
                      </td>
                      <td>{{row.sigla_estado_fornecedor}}</td>
                      <td>
                        <a
                          v-bind:href="'/deputado-federal/documento/' + row.id_cf_despesa"
                        >{{row.valor_liquido}}</a>
                      </td>
                    </tr>
                    <tr v-if="!documentos_mesmo_dia || documentos_mesmo_dia.length==0">
                      <td colspan="3" class="text-center">Nenhum registro encontrado</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
        <div class="col-xs-12 col-sm-6">
          <div class="card mb-3">
            <div class="card-header bg-light">Notas/recibos da subcota no mês</div>
            <div class="card-body">
              <div class="table-responsive">
                <table class="table table-striped table-hover table-sm">
                  <thead>
                    <tr>
                      <th>Beneficiário</th>
                      <th>UF</th>
                      <th>Valor</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="row in documentos_subcota_mes" :key="row.id_fornecedor">
                      <td>
                        <a
                          v-bind:href="'/fornecedor/' + row.id_fornecedor"
                        >{{row.nome_fornecedor}}</a>
                      </td>
                      <td>{{row.sigla_estado_fornecedor}}</td>
                      <td>
                        <a
                          v-bind:href="'/deputado-federal/documento/' + row.id_cf_despesa"
                        >{{row.valor_liquido}}</a>
                      </td>
                    </tr>
                    <tr v-if="!documentos_subcota_mes || documentos_subcota_mes.length==0">
                      <td colspan="3" class="text-center">Nenhum registro encontrado</td>
                    </tr>
                  </tbody>
                </table>
              </div>
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
  name: 'DeputadoFederalDocumento',
  props: {
    id: Number,
  },
  data() {
    return {
      documento: {},
      documentos_mesmo_dia: {},
      documentos_subcota_mes: {},
    };
  },
  mounted() {
    window.document.title = 'OPS :: Deputado Federal - Recibo';

    axios.get(`${process.env.VUE_APP_API}/deputado/documento/${this.id}`).then((response) => {
      window.document.title = `OPS :: Deputado Federal - Recibo: ${(response.data.numero_documento || response.data.id_documento)}`;
      const doc = response.data;

      const urlCamara = 'http://www.camara.leg.br/cota-parlamentar/';

      if (doc.url_documento) { // NF-e
        if (doc.url_documento.toLowerCase().indexOf('idedocumentofiscal') !== -1) {
          doc.url_documento = doc.url_documento.toLowerCase().replace('idedocumentofiscal', 'ideDocumentoFiscal');
        } else {
          doc.url_documento_nfe = doc.url_documento.toLowerCase();
        }
      } else if (doc.link === 2) { // NF-e
        doc.url_documento = `${urlCamara}nota-fiscal-eletronica?ideDocumentoFiscal=${doc.id_documento}`;
      } else if (doc.link === 3) {
        doc.url_documento = `${urlCamara}documentos/publ/${doc.id_deputado}/${doc.ano}/${doc.id_documento}.pdf`;
      } else if (doc.link !== 1) {
        doc.url_documento_nfe = `${urlCamara}nota-fiscal-eletronica?ideDocumentoFiscal=${doc.id_documento}`;
        doc.url_documento = `${urlCamara}documentos/publ/${doc.id_deputado}/${doc.ano}/${doc.id_documento}.pdf`;
      }

      doc.url_demais_documentos_mes = `${urlCamara}sumarizado?nuDeputadoId=${doc.id_deputado}&dataInicio=${doc.mes}/${doc.ano}&dataFim=${doc.mes}/${doc.ano}&despesa=${doc.id_cf_despesa_tipo}&nomeHospede=&nomePassageiro=&nomeFornecedor=&cnpjFornecedor=&numDocumento=&sguf=`;
      doc.url_detalhes_documento = `${urlCamara}documento?nuDeputadoId=${doc.id_deputado}&numMes=${doc.mes}&numAno=${doc.ano}&despesa=${doc.id_cf_despesa_tipo}&cnpjFornecedor=${doc.cnpj_cpf}&idDocumento=${doc.numero_documento}`;

      doc.url_beneficiario = `/fornecedor/${doc.id_fornecedor}`;
      doc.url_documentos_Deputado_beneficiario = `/deputado-federal?IdParlamentar=${doc.id_cf_deputado}&Fornecedor=${doc.id_fornecedor}&Periodo=0&Agrupamento=6`;

      this.documento = doc;
    });

    axios
      .get(`${process.env.VUE_APP_API}/deputado/${this.id}/documentosdomesmodia`)
      .then((response) => {
        this.documentos_mesmo_dia = response.data;
      });

    axios
      .get(`${process.env.VUE_APP_API}/deputado/${this.id}/documentosdasubcotames`)
      .then((response) => {
        this.documentos_subcota_mes = response.data;
      });
  },
};
</script>
