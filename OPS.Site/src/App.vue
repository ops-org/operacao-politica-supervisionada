<template>
  <div id="app">
    <div id="navbar" class="p-3 px-md-4 mb-3 bg-white">
      <div class="container d-flex flex-column flex-md-row align-items-center">
        <h5 class="my-0 mr-md-auto font-weight-normal">
          <router-link
            :to="{ name: 'inicio' }"
            class="logo"
            title="OPS - Operação Política Supervisionada"
            style="padding: 5px; margin-left: 0"
          >
            <img src="/img/logo_ops.png" width="64" height="40" alt="OPS" />
          </router-link>
        </h5>
        <nav class="my-2 my-md-0 mr-md-3 text-center">
          <div class="dropdown p-2" style="display: inline-block">
            <a
              class="dropdown-toggle text-dark mr-md-2"
              id="nav-camara"
              href="javascript:;"
              data-toggle="dropdown"
              aria-haspopup="true"
              aria-expanded="false"
              >Câmara</a
            >
            <div
              class="dropdown-menu dropdown-menu-left"
              aria-labelledby="nav-camara"
            >
              <a class="dropdown-item" href="/#/deputado-federal/conheca"
                >Conheça os deputados</a
              >
              <a class="dropdown-item" href="/#/deputado-federal"
                >Cota parlamentar (CEAP)</a
              >
              <a class="dropdown-item" href="/#/deputado-federal/frequencia"
                >Frequência</a
              >
              <!-- <a class="dropdown-item" href="/#/deputado-federal/secretario">Secretários</a> -->
              <a class="dropdown-item" href="/#/deputado-federal/remuneracao"
                >Remuneração/Subsídio</a
              >
            </div>
          </div>

          <div class="dropdown p-2" style="display: inline-block">
            <a
              class="dropdown-toggle text-dark mr-md-2"
              id="nav-senado"
              href="javascript:;"
              data-toggle="dropdown"
              aria-haspopup="true"
              aria-expanded="false"
              >Senado</a
            >
            <div
              class="dropdown-menu dropdown-menu-left"
              aria-labelledby="nav-senado"
            >
              <a class="dropdown-item" href="/#/senador/conheca"
                >Conheça os senadores</a
              >
              <a class="dropdown-item" href="/#/senador"
                >Cota Parlamentar (CEAPS)</a
              >
              <a class="dropdown-item" href="/#/senado/remuneracao"
                >Remuneração/Subsídio</a
              >
            </div>
          </div>

          <div class="dropdown p-2" style="display: inline-block">
            <a
              class="dropdown-toggle text-dark mr-md-2"
              id="nav-assempleias-legistativas"
              href="javascript:;"
              data-toggle="dropdown"
              aria-haspopup="true"
              aria-expanded="false"
              >Assembleias Legislativas</a
            >
            <div
              class="dropdown-menu dropdown-menu-left"
              aria-labelledby="nav-assempleias-legistativas"
            >
              <a class="dropdown-item" href="/#/deputado-estadual"
                >Cota parlamentar (CEAP)</a
              >
            </div>
          </div>

          <a
            class="p-2 text-dark"
            href="https://institutoops.org.br"
            target="_blank"
            rel="noopener"
            >Instituto OPS</a
          >
          
          

          <a id="nav-sobre" class="p-2 text-dark" href="/#/sobre">Sobre</a>
          <a
            class="p-2 text-dark social"
            href="https://t.me/joinchat/JxqhEVALOgCGl3o1Mvrg0Q"
            title="Grupo de colaboradores no Telegram"
            target="_blank"
            rel="noopener"
            aria-label="Grupo de colaboradores no Telegram"
          >
            <i class="fa fa-telegram" aria-hidden="true"></i>
          </a>
          <a
            class="p-2 text-dark social"
            href="https://www.facebook.com/institutoops"
            title="OPS no Facebook"
            target="_blank"
            rel="noopener"
            aria-label="Instituto OPS no Facebook"
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
      </div>
    </div>

    <footer class="footer">
      <div class="content-section text-muted text-center">
        O controle social é indispensável para combatermos a corrupção em nosso
        país.
      </div>

      <div class="container">
        <div class="row">
          <div class="col-sm-6 text-xs-center text-left">
            OPS - Operação Política Supervisionada
          </div>
          <div class="col-sm-6 text-xs-center text-right">
            <a href="/#/sobre">Sobre</a>
            <a
              href="https://feedback.userreport.com/9cf4c59a-d438-48b0-aa5e-e16f549b9c8c/"
              onclick="DeixarUmaIdeia(event, this)"
              >Ideias e Feedback</a
            >
            <a
              href="https://feedback.userreport.com/9cf4c59a-d438-48b0-aa5e-e16f549b9c8c/#submit/bug"
              target="_blank"
              onclick="ReportarErro(event, this)"
              >Reportar um erro</a
            >
          </div>
        </div>
      </div>
    </footer>
  </div>
</template>

<script>
export default {
  name: "App",
};
</script>

<style>
@import "./assets/styles/site.css";
</style>
