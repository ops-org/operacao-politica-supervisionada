import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { ThemeProvider } from "@/contexts/ThemeContext";

import Index from "./pages/Index";
import Busca from "./pages/Busca";
import Sobre from "./pages/Sobre";
import NotFound from "./pages/NotFound";
import CotaParlamentar from "./pages/CotaParlamentar";
import FolhaPagamento from "./pages/FolhaPagamento";
import FolhaPagamentoDetalhes from "./pages/FolhaPagamentoDetalhes";
import ParlamentareLista from "./pages/ParlamentareLista";
import FornecedorDetalhe from "./pages/fornecedor/FornecedorDetalhe";

import ParlamentarDetalhe from "./pages/ParlamentarDetalhe";
import DeputadoDespesaDocumentoDetalhe from "./pages/DocumentoDetalhe";

import { ScrollToTop } from "@/components/ScrollToTop";

const queryClient = new QueryClient();

const App = () => (
  <ThemeProvider>
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>
        <Toaster />
        <Sonner />
        <BrowserRouter>
          <ScrollToTop />
          <Routes>
            <Route path="/" element={<Index />} />
            <Route path="/busca" element={<Busca />} />
            <Route path="/sobre" element={<Sobre />} />
            <Route path="/fornecedor/:id" element={<FornecedorDetalhe />} />

            <Route path="/deputado-estadual" element={<ParlamentareLista key="deputado-estadual" type="deputado-estadual" />} />
            <Route path="/deputado-estadual/:id" element={<ParlamentarDetalhe type="deputado-estadual" />} />
            <Route path="/deputado-estadual/ceap" element={<CotaParlamentar key="deputado-estadual" type="deputado-estadual" />} />
            <Route path="/deputado-estadual/ceap/:id" element={<DeputadoDespesaDocumentoDetalhe key="deputado-estadual" type="deputado-estadual" />} />

            <Route path="/deputado-federal" element={<ParlamentareLista key="deputado-federal" type="deputado-federal" />} />
            <Route path="/deputado-federal/:id" element={<ParlamentarDetalhe type="deputado-federal" />} />
            <Route path="/deputado-federal/ceap" element={<CotaParlamentar key="deputado-federal" type="deputado-federal" />} />
            <Route path="/deputado-federal/ceap/:id" element={<DeputadoDespesaDocumentoDetalhe key="deputado-federal" type="deputado-federal" />} />
            <Route path="/deputado-federal/folha-pagamento" element={<FolhaPagamento key="deputado-federal" type="deputado-federal" />} />
            {/* <Route path="/deputado-federal/folha-pagamento/:id" element={<FolhaPagamentoDetalhes key="deputado-federal" />} /> */}

            <Route path="/senador" element={<ParlamentareLista key="senador" type="senador" />} />
            <Route path="/senador/:id" element={<ParlamentarDetalhe type="senador" />} />
            <Route path="/senador/ceap" element={<CotaParlamentar key="senador" type="senador" />} />
            <Route path="/senador/ceap/:id" element={<DeputadoDespesaDocumentoDetalhe key="senador" type="senador" />} />
            <Route path="/senador/folha-pagamento" element={<FolhaPagamento key="senador" type="senador" />} />
            <Route path="/senador/folha-pagamento/:id" element={<FolhaPagamentoDetalhes key="senador" />} />
            {/* ADD ALL CUSTOM ROUTES ABOVE THE CATCH-ALL "*" ROUTE */}
            <Route path="*" element={<NotFound />} />
          </Routes>
        </BrowserRouter>
      </TooltipProvider>
    </QueryClientProvider>
  </ThemeProvider>
);

export default App;
