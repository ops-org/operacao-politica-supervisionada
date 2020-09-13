// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import jQuery from 'jquery';
import datatable from 'datatables.net';
import Highcharts from 'highcharts';
import VueLazyload from 'vue-lazyload';
import Loading from 'vue-loading-overlay';
import VueGoogleTagManager from 'vue-gtm';
import 'vue-datatables-net';
import 'bootstrap-select';

import 'vue-loading-overlay/dist/vue-loading.css';

import Vue from 'vue';
import App from './App';
import router from './router';

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
  lengthMenu: [[15, 100, 500, 1000], [15, 100, 500, 1000]],
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
  error: '/static/img/sem_foto.jpg',
  loading: '/static/img/loading.svg',
  attempt: 1,
});

// https://github.com/jkchao/vue-loading
Vue.use(Loading, {
  fullPage: true,
});

// https://github.com/mib200/vue-gtm
Vue.use(VueGoogleTagManager, {
  id: 'GTM-MNBSLK6', // Your GTM single container ID or array of container ids ['GTM-xxxxxxx', 'GTM-yyyyyyy']
  defer: false, // defaults to false. Script can be set to `defer` to increase page-load-time at the cost of less accurate results (in case visitor leaves before script is loaded, which is unlikely but possible)
  enabled: true, // defaults to true. Plugin can be disabled by setting this to false for Ex: enabled: !!GDPR_Cookie (optional)
  debug: false, // Whether or not display console logs debugs (optional)
  loadScript: true, // Whether or not to load the GTM Script (Helpful if you are including GTM manually, but need the dataLayer functionality in your components) (optional)
  vueRouter: router, // Pass the router instance to automatically sync with router (optional)
  trackOnNextTick: false, // Whether or not call trackView in Vue.nextTick
});

Vue.config.productionTip = false;

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  components: { App },
  template: '<App/>',
});


// eslint-disable-next-line no-underscore-dangle
window._urq = window._urq || [];

jQuery(() => {
  setTimeout(() => {
    // eslint-disable-next-line no-underscore-dangle
    window._urq.push(['setGACode', 'UA-38537890-5']);
    // eslint-disable-next-line no-underscore-dangle
    window._urq.push(['setPerformInitialShorctutAnimation', false]);
    // eslint-disable-next-line no-underscore-dangle
    window._urq.push(['initSite', '9cf4c59a-d438-48b0-aa5e-e16f549b9c8c']);

    const ur = document.createElement('script'); ur.type = 'text/javascript'; ur.async = true;
    ur.src = `${document.location.protocol}//cdn.userreport.com/userreport.js`;
    const s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ur, s);

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
    // eslint-disable-next-line no-underscore-dangle
    window._urq.push(['Feedback_Open', 'submit/bug']);

    e.preventDefault();
    return false;
  }

  return true;
};

window.DeixarUmaIdeia = (e) => {
  if (jQuery('#crowd-shortcut').length > 0) {
    // eslint-disable-next-line no-underscore-dangle
    window._urq.push(['Feedback_Open', 'ideias/popular']);

    e.preventDefault();
    return false;
  }

  return true;
};
