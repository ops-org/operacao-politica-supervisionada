import { useLocation, Link } from "react-router-dom";
import { useEffect } from "react";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { Button } from "@/components/ui/button";
import { Home, Search } from "lucide-react";

const NotFound = () => {
  const location = useLocation();

  useEffect(() => {
    console.error("404 Error: User attempted to access non-existent route:", location.pathname);
  }, [location.pathname]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-background to-muted/20">
      <Header />
      <main className="container mx-auto px-4 py-8">
        {/* Hero Section */}
        <div className="text-center mb-12">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-primary/10 rounded-full mb-4">
            <Search className="w-8 h-8 text-primary" />
          </div>
          <h1 className="text-4xl font-bold text-foreground mb-4">
            404 - Página Não Encontrada
          </h1>
          <p className="text-lg text-muted-foreground mx-auto max-w-2xl">
            A página que você está procurando pode ter sido movida ou não existe mais
          </p>
        </div>

        {/* Content Section */}
        <div className="max-w-2xl mx-auto text-center space-y-8">
          <div className="bg-muted/50 rounded-lg p-8">
            <p className="text-muted-foreground mb-6">
              Você pode ter digitado incorretamente o endereço ou a página pode ter sido movida.
              Verifique o URL ou use os links abaixo para navegar pelo site.
            </p>
            
            <img 
              src="//static.ops.net.br/img/404.gif" 
              alt="Travolta está desesperadamente perdido" 
              className="mx-auto rounded-lg shadow-md mb-8"
            />
            
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Button asChild className="bg-primary hover:bg-primary/90">
                <Link to="/">
                  <Home className="w-4 h-4 mr-2" />
                  Voltar ao Início
                </Link>
              </Button>
              <Button variant="outline" asChild>
                <Link to="/busca">
                  <Search className="w-4 h-4 mr-2" />
                  Buscar Parlamentar
                </Link>
              </Button>
            </div>
          </div>
        </div>
      </main>
      <Footer />
    </div>
  );
};

export default NotFound;
