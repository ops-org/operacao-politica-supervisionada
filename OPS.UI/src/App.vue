<template>
  <div id="app">
    <div id="navbar" class="p-3 px-md-4 mb-3 bg-white">
      <div class="container d-flex flex-column flex-md-row align-items-center">
        <h5 class="my-0 mr-md-auto font-weight-normal">
          <a
            class="logo"
            href="./#!/inicio"
            title="OPS - Operação Política Supervisionada"
            style="padding: 5px; margin-left: 0"
          >
            <img src="./assets/images/logo_ops.png" width="64" height="40" alt="OPS" />
          </a>
        </h5>
        <nav class="my-2 my-md-0 mr-md-3 text-center">
          <div class="dropdown p-2" style="display: inline-block;">
            <a
              class="dropdown-toggle text-dark mr-md-2"
              id="nav-camara"
              href="javascript:;"
              data-toggle="dropdown"
              aria-haspopup="true"
              aria-expanded="false"
            >Câmara</a>
            <div class="dropdown-menu dropdown-menu-left" aria-labelledby="nav-camara">
              <div class="dropdown-header">Câmara dos Deputados</div>
              <a class="dropdown-item" href="./#!/deputado-federal/conheca">Conheça os deputados</a>
              <a class="dropdown-item" href="./#!/deputado-federal">Cota parlamentar (CEAP)</a>
              <a class="dropdown-item" href="./#!/deputado-federal/frequencia">Frequência</a>
              <a class="dropdown-item" href="./#!/deputado-federal/secretario">Secretários</a>
            </div>
          </div>

          <div class="dropdown p-2" style="display: inline-block;">
            <a
              class="dropdown-toggle text-dark mr-md-2"
              id="nav-senado"
              href="javascript:;"
              data-toggle="dropdown"
              aria-haspopup="true"
              aria-expanded="false"
            >Senado</a>
            <div class="dropdown-menu dropdown-menu-left" aria-labelledby="nav-senado">
              <div class="dropdown-header">Senado Federal</div>
              <a class="dropdown-item" href="./#!/senador">Cota Parlamentar (CEAPS)</a>
            </div>
          </div>

          <a
            class="p-2 text-dark"
            href="https://institutoops.org.br"
            target="_blank"
            rel="noopener"
          >Instituto OPS</a>
          <a
            class="p-2 text-dark"
            href="https://luciobig.com.br"
            target="_blank"
            rel="noopener"
          >Blog</a>
          <a id="nav-sobre" class="p-2 text-dark" href="./#!/sobre">Sobre</a>
          <a
            class="p-2 text-dark social"
            href="https://www.facebook.com/operacaopoliticasupervisionada"
            title="OPS no Facebook"
            target="_blank"
            rel="noopener"
            aria-label="OPS no Facebook"
          >
            <i class="fa fa-facebook" aria-hidden="true"></i>
          </a>
          <a
            class="p-2 text-dark social"
            href="https://twitter.com/LucioBig"
            title="Lúcio Big no Twitter"
            target="_blank"
            rel="noopener"
            aria-label="Lúcio Big no Twitter"
          >
            <i class="fa fa-twitter" aria-hidden="true"></i>
          </a>
          <a
            class="p-2 text-dark social"
            href="https://www.youtube.com/channel/UCfQ98EX3oOv6IHBdUNMJq8Q"
            title="Lúcio Big no YouTube"
            target="_blank"
            rel="noopener"
            aria-label="Lúcio Big no YouTube"
          >
            <i class="fa fa-youtube" aria-hidden="true"></i>
          </a>
        </nav>
      </div>
    </div>

    <div class="page-container doc-container">
      <div id="app">
        <div id="container">
          <router-view />
        </div>

        <!-- <div id="loading-modal">
          <div class="modal-backdrop fade show"></div>
          <div id="loading-modal-text">
            <img class="loader-facebook" src="/assets/./assets/images/ajax-loader-facebook.gif" />
            Processando
          </div>
        </div> -->
      </div>
    </div>

    <footer class="footer">
      <div
        class="content-section text-muted text-center"
      >O controle social é indispensável para combatermos a corrupção em nosso país.</div>

      <div class="container">
        <div class="row">
          <div class="col-sm-6 text-xs-center text-left">
            OPS - Operação Política Supervisionada
          </div>
          <div class="col-sm-6 text-xs-center text-right">
            <a href="./#!/sobre">Sobre</a>
            <a
              href="https://github.com/ops-org/operacao-politica-supervisionada"
              onclick="return trackOutboundLink(this, true);"
              target="_blank"
              rel="nofolow"
            >Repositório no GitHub</a>
            <a
              href="https://feedback.userreport.com/9cf4c59a-d438-48b0-aa5e-e16f549b9c8c/#submit/bug"
              target="_blank"
              onclick="ReportarErro(event, this)"
            >Reportar um erro</a>
          </div>
        </div>
      </div>
    </footer>
  </div>
</template>

<script>
export default {
  name: 'App',
};
</script>

<style>
  @import "bootstrap";
  @import "font-awesome";

  @import './assets/styles/site.css';
</style>
