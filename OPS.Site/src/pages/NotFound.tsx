import { useLocation, Link } from "react-router-dom";
import { useEffect } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { Button } from "@/components/ui/button";
import { Home, Search } from "lucide-react";

const NotFound = () => {
  const location = useLocation();
  usePageTitle("Página não encontrada");

  useEffect(() => {
    console.error("404 Error: User attempted to access non-existent route:", location.pathname);
  }, [location.pathname]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
      <Header />
      <main className="container mx-auto px-4 py-8 flex items-center justify-center min-h-[calc(100vh-200px)]">
        {/* Custom Header Layout */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-12">
          <div className="relative group mb-10 w-full max-w-md mx-auto">
            <div className="absolute -inset-1 bg-gradient-to-r from-primary to-accent rounded-xl blur opacity-25 group-hover:opacity-60 transition duration-1000 group-hover:duration-200"></div>
            <img
              src="https://static.ops.net.br/img/404.gif"
              alt="Travolta está desesperadamente perdido"
              className="relative rounded-xl shadow-2xl w-full transform transition duration-500 hover:scale-[1.01]"
            />
          </div>

        </div>

        <div className="text-center w-full max-w-4xl flex flex-col items-center">
          <h1 className="text-4xl font-bold bg-gradient-to-r from-primary via-accent to-primary bg-clip-text text-transparent mb-6">
            Ops! - Página Não Encontrada
          </h1>
          <p className="text-xl text-muted-foreground mx-auto max-w-2xl mb-8">
            A página procurada pode ter sido movida ou não existe mais.
          </p>

          <div className="flex flex-col sm:flex-row gap-4 justify-center">

            <Button asChild className="bg-gradient-to-r from-primary to-accent hover:opacity-90 transition-all shadow-lg shadow-primary/25 h-12 px-8 text-lg border-0">
              <Link to="/">
                <Home className="w-5 h-5 mr-2" />
                Voltar ao Início
              </Link>
            </Button>
            <Button variant="outline" asChild className="border-2 hover:bg-muted/50 h-12 px-8 text-lg">
              <Link to="/busca">
                <Search className="w-5 h-5 mr-2" />
                Buscar Parlamentar
              </Link>
            </Button>
          </div>
        </div>
      </main>
      <Footer />
    </div>
  );
};

export default NotFound;
