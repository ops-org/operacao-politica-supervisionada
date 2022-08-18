<template>
  <div class="container">
    <h3 class="page-title">[BETA] Folha de Pagamento na Câmara de Deputados Federais</h3>

    <div class="row mb-2">
      <div class="col-md-6">
        <p class="mb-1"><strong>Grupo Funcional:</strong> {{ remuneracao.grupo_funcional }}</p>
      </div>
      <div class="col-md-2" v-if="remuneracao.cargo">
        <p class="mb-1">
          <strong>Cargo:</strong> {{ remuneracao.cargo }}
        </p>
      </div>
      <div class="col-md-4">
        <p class="mb-1">
          <strong>Tipo Folha:</strong> {{ remuneracao.tipo_folha }}
        </p>
      </div>
     <div class="col-md-6" v-if="remuneracao.deputado">
        <p class="mb-1"><strong>Deputado:</strong> <a v-bind:href="'/deputado-federal/' + remuneracao.id_cf_deputado">{{ remuneracao.deputado }}</a></p>
      </div>
       <div class="col-md-6" v-if="remuneracao.secretario">
        <p class="mb-1"><strong>Secretário:</strong> <a v-bind:href="'https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/' + remuneracao.chave" target="_blank">{{ remuneracao.secretario }}</a></p>
      </div>
    </div>

    <div class="mb-5">
      <h5 class="page-title">Dados de Remuneração</h5>
      <table
        class="table table-borderless table-hover table-sm"
        style="width: auto; margin: 0 auto"
      >
        <thead>
          <tr class="border-bottom">
            <th>
              Folha {{ remuneracao.tipo_folha }} ({{ remuneracao.ano_mes }})
            </th>
            <th class="text-right">Valores em R$</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <th>Remuneratória Básica</th>
            <td class="text-right">{{ remuneracao.remun_basica }}</td>
          </tr>
          <tr>
            <th>Vantagens Pessoais</th>
            <td class="text-right">{{ remuneracao.vant_pessoais }}</td>
          </tr>
          <tr>
            <th>Vantagens Eventuais</th>
            <td class="text-right"></td>
          </tr>
          <tr>
            <td>- Função Comissionada</td>
            <td class="text-right">{{ remuneracao.func_comissionada }}</td>
          </tr>
          <tr>
            <td>- Antecipação e Gratificação Natalina</td>
            <td class="text-right">{{ remuneracao.grat_natalina }}</td>
          </tr>
          <tr>
            <td>- Férias (1/3 Constitucional)</td>
            <td class="text-right">{{ remuneracao.horas_extras }}</td>
          </tr>
          <tr>
            <td>- Outras Remunerações Eventuais/Provisórias(*)</td>
            <td class="text-right">{{ remuneracao.outras_eventuais }}</td>
          </tr>
          <tr>
            <th>Abono de Permanência</th>
            <td class="text-right">{{ remuneracao.abono_permanencia }}</td>
          </tr>
          <tr>
            <th>Descontos Obrigatórios</th>
            <td class="text-right"></td>
          </tr>
          <tr>
            <td>- Reversão do Teto Constitucional</td>
            <td class="text-right">{{ remuneracao.reversao_teto_const }}</td>
          </tr>
          <tr>
            <td>- Imposto de Renda</td>
            <td class="text-right">{{ remuneracao.imposto_renda }}</td>
          </tr>
          <tr>
            <td>- Contribuição Previdenciária</td>
            <td class="text-right">{{ remuneracao.previdencia }}</td>
          </tr>
          <tr>
            <th>Remuneração Após Descontos Obrigatórios</th>
            <td class="text-right">{{ remuneracao.rem_liquida }}</td>
          </tr>
          <tr>
            <th>Vantagens Indenizatórias e Compensatórias</th>
            <td class="text-right"></td>
          </tr>
          <tr>
            <td>- Diárias</td>
            <td class="text-right">{{ remuneracao.diarias }}</td>
          </tr>
          <tr>
            <td>- Auxílios</td>
            <td class="text-right">{{ remuneracao.auxilios }}</td>
          </tr>
          <tr>
            <td>- Outras Vantagens Indenizatórias</td>
            <td class="text-right">{{ remuneracao.vant_indenizatorias }}</td>
          </tr>
          <tr
            class="border-top"
            title="Esse é o custo efetivo para os cofres públicos"
          >
            <th>Custo Total</th>
            <td class="text-right">{{ remuneracao.custo_total }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="mb-5">
      <p><strong><a href="Entenda os itens que compõem a remuneração">Entenda os itens que compõem a remuneração</a></strong></p>
      <p>(*) Este item pode ter resultado negativo em razão de acertos de meses anteriores e descontos por falta e impontualidade.</p>
      <p>Atenção: os descontos de natureza pessoal (pensão alimentícia, consignações diversas e outros descontos por determinação judicial) não podem ser divulgados. Portanto, o valor recebido pode ser menor que o informado acima.</p>
    </div>
    <br>
  </div>
</template>

<script>
const axios = require('axios');

export default {
  props: {
    id: Number,
  },
  data() {
    // const vm = this;

    return {
      remuneracao: {},
    };
  },
  mounted() {
    const vm = this;
    document.title = 'OPS :: Remuneração na Câmara';
    const loader = vm.$loading.show();

    axios
      .get(`${process.env.VUE_APP_API}/deputado/remuneracao/${vm.id}`)
      .then((response) => {
        this.remuneracao = response.data;
        loader.hide();
      });
  },
};
</script>
