<template>
  <div>
    <vdtnet-table
      :fields="fields"
      :opts="options"
      ref="table" />
  </div>
</template>

<script>
import VdtnetTable from 'vue-datatables-net';
import 'datatables.net-bs4';
import * as vue from 'vue';

import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
import 'bootstrap/dist/css/bootstrap.min.css';

window.jQuery = require('jquery');

window.$ = window.jQuery;

window.Vue = vue;

// const axios = require('axios');

export default {
  name: 'DeputadoFederalConheca',
  components: { VdtnetTable },
  data() {
    return {
      options: {
        ajax: {
          url: 'https://jsonplaceholder.typicode.com/users',
          dataSrc: json => json,
        },
        // buttons: ['copy', 'csv', 'print'],
        /*eslint-disable */
        // dom: "Btr<'row vdtnet-footer'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'pl>>",
        /* eslint-enable */
        pageLength: 500,
        processing: true,
        searching: true,
        searchDelay: 1500,
        destroy: true,
        ordering: true,
        lengthChange: true,
        serverSide: true,
        fixedHeader: true,
        saveState: true,
        // rowId
        // fixedHeader
        // deferLoading
        // autoWidth
        // columnDefs
      },
      fields: {
        id: { label: 'ID', sortable: true },
        // actions: {
        //   isLocal: true,
        //   label: 'Actions',
        //   defaultContent: '<a href="javascript:void(0);" data-action="edit" class="btn btn-primary btn-sm"><i class="mdi mdi-square-edit-outline"></i> Edit</a>' +
        //     '<span data-action="delete" class="btn btn-danger btn-sm"><i class="mdi mdi-delete"></i> Delete</span>',
        // },
        // name: { label: 'Name', sortable: true, searchable: true, defaultOrder: 'desc' },
        // username: { label: 'Username', sortable: false, searchable: true },
        // email: { label: 'Email' },
        // address1: {
        //   label: 'Address1',
        //   data: 'address',
        //   template: '{{ data.street }}, {{ data.suite }}',
        // },
        // address2: {
        //   label: 'Address2',
        //   data: 'address',
        // },
        // phone: { label: 'Phone' },
        // website: {
        //   label: 'Website',
        //   render: data => `https://${data}`,
        // },
      },
    };
  },

  // methods: {
  //   doLoadTable(cb) {
  //     axios
  //       .get('https://jsonplaceholder.typicode.com/users')
  //       .then((response) => {
  //         cb(response.data);
  //       });
  //   },
  //   doAlertEdit() {
  //     // window.alert(`row edit click for item ID: ${data.id}`)
  //   },
  // },
};
</script>
