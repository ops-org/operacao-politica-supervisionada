import Vue from 'vue';
import Router from 'vue-router';
import Inicio from '@/components/inicio';
import Busca from '@/components/busca';

import DeputadoFederalLista from '@/components/deputado-federal/lista';
import DeputadoFederalDetalhes from '@/components/deputado-federal/detalhes';
import DeputadoFederalConheca from '@/components/deputado-federal/conheca';
import DeputadoFederalSecretarioLista from '@/components/deputado-federal/secretario-lista';
import DeputadoFederalSecretarioDetalhes from '@/components/deputado-federal/secretario-detalhes';
import DeputadoFederalFrequenciaLista from '@/components/deputado-federal/frequencia-lista';
import DeputadoFederalFrequenciaDetalhes from '@/components/deputado-federal/frequencia-detalhes';
import DeputadoFederalRemuneracaoLista from '@/components/deputado-federal/remuneracao-lista';
import DeputadoFederalRemuneracaoDetalhes from '@/components/deputado-federal/remuneracao-detalhes';
import DeputadoFederalDocumento from '@/components/deputado-federal/documento';

import SenadorLista from '@/components/senador/lista';
import SenadorDetalhes from '@/components/senador/detalhes';
import SenadorConheca from '@/components/senador/conheca';
import SenadoRemuneracaoLista from '@/components/senador/remuneracao-lista';
import SenadoRemuneracaoDetalhes from '@/components/senador/remuneracao-detalhes';

import FornecedorDetalhes from '@/components/fornecedor/detalhes';

import Sobre from '@/components/sobre';
import Erro404 from '@/components/erro/404';

Vue.use(Router);

const router = new Router({
  mode: 'history',
  routes: [
    {
      path: '/',
      name: 'inicio',
      component: Inicio,
    },
    {
      path: '/busca',
      name: 'busca',
      component: Busca,
      props: route => ({ q: route.query.q }),
    },
    {
      path: '/deputado-federal',
      name: 'deputado-federal-lista',
      component: DeputadoFederalLista,
      props: route => ({ qs: route.query }),
    },
    {
      path: '/deputado-federal/conheca',
      name: 'deputado-federal-conheca',
      component: DeputadoFederalConheca,
    },
    {
      path: '/deputado-federal/secretario',
      name: 'deputado-federal-secretario-lista',
      component: DeputadoFederalSecretarioLista,
    },
    {
      path: '/deputado-federal/:id/secretario',
      name: 'deputado-federal-secretario-detalhes',
      component: DeputadoFederalSecretarioDetalhes,
      props: route => ({ id: parseInt(route.params.id, 10) }),
    },
    {
      path: '/deputado-federal/frequencia',
      name: 'deputado-federal-frequencia-lista',
      component: DeputadoFederalFrequenciaLista,
    },
    {
      path: '/deputado-federal/frequencia/:id',
      name: 'deputado-federal-frequencia-detalhes',
      component: DeputadoFederalFrequenciaDetalhes,
      props: route => ({ id: parseInt(route.params.id, 10) }),
    },
    {
      path: '/deputado-federal/remuneracao',
      name: 'deputado-federal-remuneracao-lista',
      component: DeputadoFederalRemuneracaoLista,
      props: route => ({ qs: route.query }),
    },
    {
      path: '/deputado-federal/remuneracao/:id',
      name: 'deputado-federal-remuneracao-detalhes',
      component: DeputadoFederalRemuneracaoDetalhes,
      props: route => ({ id: parseInt(route.params.id, 10) }),
    },
    {
      path: '/deputado-federal/documento/:id',
      name: 'deputado-federal-documento',
      component: DeputadoFederalDocumento,
      props: route => ({ id: parseInt(route.params.id, 10) }),
    },
    {
      path: '/deputado-federal/:id',
      name: 'deputado-federal-detalhes',
      component: DeputadoFederalDetalhes,
      props: route => ({ id: parseInt(route.params.id, 10) }),
    },
    {
      path: '/senador',
      name: 'senador-lista',
      component: SenadorLista,
      props: route => ({ qs: route.query }),
    },
    {
      path: '/senador/conheca',
      name: 'senador-conheca',
      component: SenadorConheca,
      props: route => ({ qs: route.query }),
    },
    {
      path: '/senado/remuneracao',
      name: 'senador-remuneracao-lista',
      component: SenadoRemuneracaoLista,
      props: route => ({ qs: route.query }),
    },
    {
      path: '/senado/remuneracao/:id',
      name: 'senador-remuneracao-detalhes',
      component: SenadoRemuneracaoDetalhes,
      props: route => ({ id: parseInt(route.params.id, 10) }),
    },
    {
      path: '/senador/:id',
      name: 'senador-detalhes',
      component: SenadorDetalhes,
      props: route => ({ id: parseInt(route.params.id, 10) }),
    },
    {
      path: '/fornecedor/:id',
      name: 'fornecedor-detalhes',
      component: FornecedorDetalhes,
      props: route => ({ id: parseInt(route.params.id, 10) }),
    },
    {
      path: '/sobre',
      name: 'sobre',
      component: Sobre,
    },
    {
      path: '*',
      component: Erro404,
    },
  ],
});

// router.beforeEach((to, from, next) => {
//   console.log(from.fullPath);
//   console.log(to.fullPath);
//   next();
// });

export default router;

// nginx
// location / {
//   try_files $uri $uri/ /index.html;
// }
