import Vue from 'vue';
import Router from 'vue-router';
import Inicio from '@/components/Inicio';
import DeputadoFederalLista from '@/components/DeputadoFederalLista';

Vue.use(Router);

export default new Router({
  mode: 'history',
  routes: [
    {
      path: '/',
      name: 'Inicio',
      component: Inicio,
    },
    {
      path: '/deputado-federal',
      name: 'DeputadoFederalLista',
      component: DeputadoFederalLista,
    },
  ],
});

// nginx
// location / {
//   try_files $uri $uri/ /index.html;
// }
