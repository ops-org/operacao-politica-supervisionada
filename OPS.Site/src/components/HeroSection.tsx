import { Search, TrendingUp, Database, Shield } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { useState } from "react";
import { useNavigate } from "react-router-dom";

export const HeroSection = () => {
  const [searchQuery, setSearchQuery] = useState("");
  const navigate = useNavigate();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      navigate(`/busca?q=${encodeURIComponent(searchQuery.trim())}`);
    }
  };

  return (
    <section className="relative py-20 md:py-32 overflow-hidden">
      {/* Background gradient */}
      <div className="absolute inset-0 bg-gradient-to-br from-primary/5 via-background to-muted/20"></div>

      {/* Decorative elements */}
      <div className="absolute top-10 left-10 w-20 h-20 bg-primary/10 rounded-full blur-xl"></div>
      <div className="absolute top-32 right-20 w-32 h-32 bg-blue-500/10 rounded-full blur-2xl"></div>
      <div className="absolute bottom-20 left-1/4 w-24 h-24 bg-green-500/10 rounded-full blur-xl"></div>

      <div className="container relative mx-auto px-4">
        <div className="text-center space-y-8">
          {/* Header section */}
          <div className="space-y-6">
            <h1 className="font-serif text-4xl md:text-5xl lg:text-6xl font-bold text-foreground leading-tight">
              Operação Política Supervisionada
            </h1>

            <p className="text-xl md:text-2xl text-muted-foreground max-w-3xl mx-auto leading-relaxed">
              Indexador de dados públicos da cota parlamentar, verba de gabinete e salários dos deputados e senadores brasileiros.
            </p>
          </div>

          {/* Search form */}
          <div className="max-w-2xl mx-auto">
            <form onSubmit={handleSearch} className="relative">
              <div className="relative group">
                <Input
                  type="search"
                  placeholder="Buscar deputado, senador, empresa ou CNPJ..."
                  className="h-14 pl-6 pr-14 text-base bg-card border-2 border-border/50 rounded-xl shadow-lg focus:border-primary focus:shadow-xl transition-all duration-300 text-foreground placeholder:text-muted-foreground/60"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                />
                <Button
                  type="submit"
                  size="lg"
                  className="absolute right-2 top-1/2 -translate-y-1/2 h-10 w-10 rounded-lg bg-primary hover:bg-primary/90 transition-all duration-300 shadow-md hover:shadow-lg"
                >
                  <Search className="h-4 w-4" />
                </Button>
              </div>

              {/* Search suggestions */}
              {/* <div className="mt-3 flex flex-wrap justify-center gap-2">
                <span className="text-xs text-muted-foreground">Popular:</span>
                <button 
                  type="button"
                  onClick={() => setSearchQuery("")}
                  className="text-xs px-2 py-1 bg-muted/50 rounded-full hover:bg-muted transition-colors text-muted-foreground hover:text-foreground"
                >
                  Deputados
                </button>
                <button 
                  type="button"
                  onClick={() => setSearchQuery("")}
                  className="text-xs px-2 py-1 bg-muted/50 rounded-full hover:bg-muted transition-colors text-muted-foreground hover:text-foreground"
                >
                  Senadores
                </button>
                <button 
                  type="button"
                  onClick={() => setSearchQuery("")}
                  className="text-xs px-2 py-1 bg-muted/50 rounded-full hover:bg-muted transition-colors text-muted-foreground hover:text-foreground"
                >
                  Empresas
                </button>
              </div> */}
            </form>
          </div>

          {/* Feature highlights */}
          {/* <div className="grid gap-6 md:grid-cols-3 max-w-4xl mx-auto mt-16">
            <Card className="border-0 shadow-md bg-gradient-to-br from-primary/5 to-primary/10 hover:shadow-lg transition-shadow duration-300">
              <CardContent className="p-6 text-center">
                <div className="flex justify-center mb-4">
                  <div className="p-3 bg-primary/10 rounded-full">
                    <TrendingUp className="w-5 h-5 text-primary" />
                  </div>
                </div>
                <h3 className="font-semibold text-foreground mb-2">Análises em Tempo Real</h3>
                <p className="text-sm text-muted-foreground">
                  Dados atualizados e visualizações interativas
                </p>
              </CardContent>
            </Card>
            
            <Card className="border-0 shadow-md bg-gradient-to-br from-blue-500/5 to-blue-500/10 hover:shadow-lg transition-shadow duration-300">
              <CardContent className="p-6 text-center">
                <div className="flex justify-center mb-4">
                  <div className="p-3 bg-blue-500/10 rounded-full">
                    <Database className="w-5 h-5 text-blue-600" />
                  </div>
                </div>
                <h3 className="font-semibold text-foreground mb-2">Dados Completos</h3>
                <p className="text-sm text-muted-foreground">
                  Acesso a toda base de dados oficial
                </p>
              </CardContent>
            </Card>
            
            <Card className="border-0 shadow-md bg-gradient-to-br from-green-500/5 to-green-500/10 hover:shadow-lg transition-shadow duration-300">
              <CardContent className="p-6 text-center">
                <div className="flex justify-center mb-4">
                  <div className="p-3 bg-green-500/10 rounded-full">
                    <Shield className="w-5 h-5 text-green-600" />
                  </div>
                </div>
                <h3 className="font-semibold text-foreground mb-2">Transparência</h3>
                <p className="text-sm text-muted-foreground">
                  Fonte oficial e dados verificados
                </p>
              </CardContent>
            </Card>
          </div> */}

          {/* Call to action */}
          <div className="mt-12 space-y-4">
            <div className="flex items-center justify-center gap-4">
              {/* <Button 
                size="lg" 
                className="px-8 py-3 bg-primary hover:bg-primary/90 transition-all duration-300 shadow-lg hover:shadow-xl"
                onClick={() => navigate('/cota-parlamentar')}
              >
                Explorar Dados
              </Button>
              <Button 
                variant="outline" 
                size="lg" 
                className="px-8 py-3 border-2 hover:bg-muted transition-all duration-300"
                onClick={() => navigate('/sobre')}
              >
                Saiba Mais
              </Button> */}
              <div className="flex flex-wrap justify-center gap-4">
                <Button onClick={() => navigate('/senador/ceap')} size="lg" className="px-8 py-3 bg-primary hover:bg-primary/90 transition-all duration-300 shadow-lg hover:shadow-xl">
                  Senado Federal
                </Button>
                <Button onClick={() => navigate('/deputado-federal/ceap')} size="lg" className="px-8 py-3 bg-primary hover:bg-primary/90 transition-all duration-300 shadow-lg hover:shadow-xl">
                  Câmara dos Deputados
                </Button>
                <Button onClick={() => navigate('/deputado-estadual/ceap')} size="lg" className="px-8 py-3 bg-primary hover:bg-primary/90 transition-all duration-300 shadow-lg hover:shadow-xl">
                  Assembleias Legislativas
                </Button>
              </div>
            </div>
            {/* <p className="text-xs text-muted-foreground">
              Mais de 10 anos de dados parlamentares analisados
            </p> */}
            <p className="text-xs text-muted-foreground">
              O controle social é indispensável para combatermos a corrupção em nosso país.
            </p>
          </div>
        </div>
      </div>
    </section>
  );
};
