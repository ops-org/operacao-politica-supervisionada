// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import jQuery from 'jquery';
import datatable from 'datatables.net';
import VdtnetTable from 'vue-datatables-net';
import Highcharts from 'highcharts';
import VueLazyload from 'vue-lazyload';
import Loading from 'vue-loading-overlay';
import Multiselect from 'vue-multiselect';
import 'bootstrap-select';
import 'datatables.net-bs4';

import 'bootstrap/dist/css/bootstrap.min.css';
import 'font-awesome/css/font-awesome.min.css';
import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
import 'vue-loading-overlay/dist/vue-loading.css';
import 'vue-multiselect/dist/vue-multiselect.min.css';

import Vue from 'vue';
import App from './App';
import router from './router';

require('dotenv').config();
require('bootstrap');

window.$ = jQuery;
window.jQuery = jQuery;
window.$.fn.datatable = datatable;

window.$.extend(true, window.$.fn.dataTable.defaults, {
  processing: true,
  searching: false,
  destroy: true,
  ordering: true,
  serverSide: true,
  fixedHeader: true,
  saveState: true,
  lengthMenu: [
    [15, 100, 500, 1000],
    [15, 100, 500, 1000]
  ],
  language: {
    url: 'https://cdn.datatables.net/plug-ins/1.10.21/i18n/Portuguese-Brasil.json',
  },
});

Highcharts.setOptions({
  global: {
    useUTC: false,
  },
  lang: {
    decimalPoint: ',',
    thousandsSep: '.',
  },
});

// https://github.com/hilongjw/vue-lazyload
Vue.use(VueLazyload, {
  preLoad: 1.3,
  error: '/img/sem_foto.jpg',
  loading: '/img/loading.svg',
  attempt: 1,
});

// https://github.com/jkchao/vue-loading
Vue.use(Loading, {
  fullPage: true,
});


Vue.component('Multiselect', Multiselect);
Vue.component('VdtnetTable', VdtnetTable);

if(process.env.NODE_ENV === 'production'){
  Vue.config.productionTip = false;
  Vue.config.devtools = false;
}else{
  Vue.config.productionTip = true;
  Vue.config.devtools = true;
}

window.AddIfDontExists = function(arr, id, text) {
  if(arr.find(x => x.id == id) == null) {
    arr.push({id: id, text: text});
  }
}

window.GetIds = function(arr) {
  var ids = jQuery.map(arr, function (obj){
    return obj.id;
  });

  return ids;
}

new Vue({
  el: '#app',
  router,
  render: h => h(App)
})

// eslint-disable-next-line no-underscore-dangle
window._urq = window._urq || [];

jQuery(() => {
  setTimeout(() => {
    // eslint-disable-next-line no-underscore-dangle
    window._urq.push(['initSite', '9cf4c59a-d438-48b0-aa5e-e16f549b9c8c']);

    const ur = document.createElement('script');
    ur.type = 'text/javascript';
    ur.async = true;
    ur.src = `${document.location.protocol}//cdn.userreport.com/userreport.js`;
    const s = document.getElementsByTagName('script')[0];
    s.parentNode.insertBefore(ur, s);

    const interval = setInterval(() => {
      if (jQuery('#crowd-shortcut').length === 1) {
        clearInterval(interval);

        jQuery('#crowd-shortcut').parent().css('top', '54px');
      }
    }, 100);
  }, 3000); // Não bloquear a carga da tela
});

window.ReportarErro = (e) => {
  if (jQuery('#crowd-shortcut').length > 0) {
    e.preventDefault();

    // eslint-disable-next-line no-underscore-dangle
    window._urq.push(['Feedback_Open', 'submit/bug']);
    return false;
  }

  return true;
};

window.DeixarUmaIdeia = (e) => {
  if (jQuery('#crowd-shortcut').length > 0) {
    e.preventDefault();

    // eslint-disable-next-line no-underscore-dangle
    window._urq.push(['Feedback_Open', 'ideias/popular']);
    return false;
  }

  return true;
};
