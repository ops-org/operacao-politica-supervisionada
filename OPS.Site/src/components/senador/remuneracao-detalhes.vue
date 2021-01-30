<template>
  <div class="container">
    <h3 class="page-title">[BETA] Remuneração/Subsídio no Senado</h3>

    <div class="row mb-2">
      <!-- <div class="col-md-6">
        <p class="mb-1"><strong>Nome:</strong> {{ remuneracao.nome }}</p>
      </div> -->
      <div class="col-md-4">
        <p class="mb-1"><strong>Vínculo:</strong> {{ remuneracao.vinculo }}</p>
      </div>
      <!-- <div class="col-md-2">
        <p class="mb-1">
          <strong>Situação:</strong> {{ remuneracao.situacao }}
        </p>
      </div> -->

      <div class="col-md-6">
        <p class="mb-1"><strong>Lotação:</strong> {{ remuneracao.lotacao }}</p>
      </div>
      <div class="col-md-2">
        <p class="mb-1">
          <strong>Admissão:</strong> {{ remuneracao.admissao }}
        </p>
      </div>


      <div class="col-md-4">
        <p class="mb-1"><strong>Categoria:</strong> {{ remuneracao.categoria }}</p>
      </div>
      <div class="col-md-6">
        <p class="mb-1">
          <strong>Cargo/Plano:</strong> {{ remuneracao.cargo }}
        </p>
      </div>
      <div class="col-md-4" v-if="remuneracao.referencia_cargo">
        <p class="mb-1"><strong>Padrão:</strong> {{ remuneracao.referencia_cargo }}</p>
      </div>
       <div class="col-md-2" v-if="remuneracao.simbolo_funcao">
        <p class="mb-1"><strong>Função:</strong> {{ remuneracao.simbolo_funcao }}</p>
      </div>


      <!-- <div class="col-md-6">
        <p class="mb-1">
          <strong>Especialidade:</strong> {{ remuneracao.especialidade }}
        </p>
      </div> -->

      <!-- <div class="col-md-3">
        <p class="mb-1">
          <strong>Nome da Função:</strong> {{ remuneracao.funcao }}
        </p>
      </div> -->
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
            <td>- Horas Extras</td>
            <td class="text-right">{{ remuneracao.horas_extras }}</td>
          </tr>
          <tr>
            <td>- Outras Remunerações Eventuais/Provisórias</td>
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
            <td>- PSSS (Lei 12.618/12)</td>
            <td class="text-right">{{ remuneracao.previdencia }}</td>
          </tr>
          <tr>
            <td>- Faltas</td>
            <td class="text-right">{{ remuneracao.faltas }}</td>
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
    document.title = 'OPS :: Remuneração no Senado';
    const loader = vm.$loading.show();

    axios
      .get(`${process.env.VUE_APP_API}/senador/remuneracao/${vm.id}`)
      .then((response) => {
        this.remuneracao = response.data;
        loader.hide();
      });
  },
};
</script>
