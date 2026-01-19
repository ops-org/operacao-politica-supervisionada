import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route } from "react-router-dom";

import Index from "./pages/Index";
import Busca from "./pages/Busca";
import Sobre from "./pages/Sobre";
import NotFound from "./pages/NotFound";
import CotaParlamentar from "./pages/CotaParlamentar";
import FolhaPagamento from "./pages/FolhaPagamento";
import FolhaPagamentoDetalhes from "./pages/FolhaPagamentoDetalhes";
import ParlamentareLista from "./pages/ParlamentareLista";
import FornecedorDetalhe from "./pages/fornecedor/FornecedorDetalhe";

import DeputadoEstadualDetalhe from "./pages/assembleia/DeputadoDetalhe";

import DeputadoFederalDetalhe from "./pages/camara/DeputadoDetalhe";
import DeputadoDespesaDocumentoDetalhe from "./pages/camara/DocumentoDetalhe";

import SenadorDetalhe from "./pages/senado/SenadorDetalhe";

const queryClient = new QueryClient();

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <Toaster />
      <Sonner />
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Index />} />
          <Route path="/busca" element={<Busca />} />
          <Route path="/sobre" element={<Sobre />} />
          <Route path="/fornecedor/:id" element={<FornecedorDetalhe />} />

          <Route path="/deputado-estadual" element={<ParlamentareLista key="deputado-estadual" type="deputado-estadual" />} />
          <Route path="/deputado-estadual/:id" element={<DeputadoEstadualDetalhe />} />
          <Route path="/deputado-estadual/ceap" element={<CotaParlamentar key="deputado-estadual" type="deputado-estadual" />} />

          <Route path="/deputado-federal" element={<ParlamentareLista key="deputado-federal" type="deputado-federal" />} />
          <Route path="/deputado-federal/:id" element={<DeputadoFederalDetalhe />} />
          <Route path="/deputado-federal/ceap" element={<CotaParlamentar key="deputado-federal" type="deputado-federal" />} />
          <Route path="/deputado-federal/ceap/:id" element={<DeputadoDespesaDocumentoDetalhe />} />

          <Route path="/senador" element={<ParlamentareLista key="senador" type="senador" />} />
          <Route path="/senador/:id" element={<SenadorDetalhe />} />
          <Route path="/senador/ceap" element={<CotaParlamentar key="senador" type="senador" />} />
          <Route path="/senador/folha-pagamento" element={<FolhaPagamento key="senador" />} />
          <Route path="/senador/folha-pagamento/:id" element={<FolhaPagamentoDetalhes key="senador" />} />
          {/* ADD ALL CUSTOM ROUTES ABOVE THE CATCH-ALL "*" ROUTE */}
          <Route path="*" element={<NotFound />} />
        </Routes>
      </BrowserRouter>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
