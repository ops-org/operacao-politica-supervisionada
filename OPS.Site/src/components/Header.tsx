import { Search, Menu, X, Sun, Moon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useTheme } from "@/contexts/ThemeContext";

export const Header = () => {
  const [searchQuery, setSearchQuery] = useState("");
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const navigate = useNavigate();
  const { theme, toggleTheme } = useTheme();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      navigate(`/busca?q=${encodeURIComponent(searchQuery.trim())}`);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      if (searchQuery.trim()) {
        navigate(`/busca?q=${encodeURIComponent(searchQuery.trim())}`);
      }
    }
  };

  return (
    <header className="sticky top-0 z-50 w-full border-b border-border/40 bg-gradient-to-r from-primary/10 via-accent/10 to-primary/10 backdrop-blur-md shadow-lg">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        <Link to="/" className="flex items-center gap-2" title="Operação Política Supervisionada">
          <div className="flex h-18 w-18 items-center justify-center rounded-full font-bold text-lg">
            <img src="https://static.ops.org.br/logo.png" width="64" height="40" alt="OPS"></img>
          </div>
          {/* <span className="hidden font-semibold text-foreground sm:inline-block">
            Operação Política Supervisionada
          </span> */}
        </Link>

        <nav className="hidden items-center gap-1 md:flex">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="text-foreground">
                Câmara
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent>
              <DropdownMenuItem asChild>
                <Link to="/deputado-federal">Conheça os Deputados</Link>
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link to="/deputado-federal/ceap">Cota parlamentar (CEAP)</Link>
              </DropdownMenuItem>
              {/* <DropdownMenuItem asChild>
                <Link to="/deputado-federal/folha-pagamento">Folha de Pagamento</Link>
              </DropdownMenuItem> */}
              {/* <DropdownMenuItem asChild>
                <Link to="/deputado-federal/frequencia">Frequência (descontinuada)</Link>
              </DropdownMenuItem> */}
              {/* <DropdownMenuItem>Gastos por Partido</DropdownMenuItem>
              <DropdownMenuItem>Ranking de Gastos</DropdownMenuItem> */}
            </DropdownMenuContent>
          </DropdownMenu>

          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="text-foreground">
                Senado
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent>
              <DropdownMenuItem asChild>
                <Link to="/senador">Conheça os Senadores</Link>
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link to="/senador/ceap">Cota Parlamentar (CEAPS)</Link>
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link to="/senador/folha-pagamento">Folha de Pagamento</Link>
              </DropdownMenuItem>
              {/* <DropdownMenuItem>Gastos por Partido</DropdownMenuItem>
              <DropdownMenuItem>Ranking de Gastos</DropdownMenuItem> */}
            </DropdownMenuContent>
          </DropdownMenu>

          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="text-foreground">
                Assembleias
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent>
              {/* <DropdownMenuItem asChild>
                <Link to="/deputado-estadual">Conheça os Deputados</Link>
              </DropdownMenuItem> */}
              <DropdownMenuItem asChild>
                <Link to="/deputado-estadual/ceap">Cota parlamentar (CEAP)</Link>
              </DropdownMenuItem>
              {/* <DropdownMenuItem>São Paulo</DropdownMenuItem>
              <DropdownMenuItem>Rio de Janeiro</DropdownMenuItem>
              <DropdownMenuItem>Minas Gerais</DropdownMenuItem> */}
            </DropdownMenuContent>
          </DropdownMenu>

          <Button variant="ghost" className="text-foreground" asChild>
            <Link to="/sobre">Sobre</Link>
          </Button>
        </nav>

        <div className="flex items-center gap-2">
          <form onSubmit={handleSearch} className="relative hidden sm:block">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              type="search"
              placeholder="Digite para buscar"
              className="w-64 pl-9"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onKeyDown={handleKeyDown}
            />
          </form>


          <Button
            variant="ghost"
            size="icon"
            onClick={toggleTheme}
            className="h-9 w-9 bg-gradient-to-br from-primary/20 to-accent/20 hover:from-primary/30 hover:to-accent/30 transition-all duration-300"
            title={theme === 'light' ? 'Mudar para tema escuro' : 'Mudar para tema claro'}
          >
            <div className="transition-transform duration-500 ease-in-out" style={{ transform: theme === 'dark' ? 'rotate(180deg)' : 'rotate(0deg)' }}>
              {theme === 'light' ? (
                <Moon className="h-4 w-4 text-primary" />
              ) : (
                <Sun className="h-4 w-4 text-accent" />
              )}
            </div>
          </Button>


          <Button
            variant="ghost"
            size="icon"
            className="md:hidden"
            onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
          >
            {isMobileMenuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </Button>
        </div>
      </div>

      {/* Mobile Menu */}
      {isMobileMenuOpen && (
        <div className="md:hidden border-t border-border/40 bg-gradient-to-br from-card/95 to-muted/95 backdrop-blur-lg">
          <nav className="container mx-auto px-4 py-4 space-y-2">
            {/* Search in Mobile Menu */}
            <form onSubmit={handleSearch} className="relative mb-4">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                type="search"
                placeholder="Digite para buscar"
                className="w-full pl-9"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onKeyDown={handleKeyDown}
              />
            </form>

            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground mb-2">Câmara</p>
              <Link
                to="/deputado-federal"
                className="block px-3 py-2 text-sm rounded-md hover:bg-accent hover:text-accent-foreground"
                onClick={() => setIsMobileMenuOpen(false)}
              >
                Conheça os Deputados
              </Link>
              <Link
                to="/deputado-federal/ceap"
                className="block px-3 py-2 text-sm rounded-md hover:bg-accent hover:text-accent-foreground"
                onClick={() => setIsMobileMenuOpen(false)}
              >
                Cota parlamentar (CEAP)
              </Link>
            </div>

            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground mb-2">Senado</p>
              <Link
                to="/senador"
                className="block px-3 py-2 text-sm rounded-md hover:bg-accent hover:text-accent-foreground"
                onClick={() => setIsMobileMenuOpen(false)}
              >
                Conheça os Senadores
              </Link>
              <Link
                to="/senador/ceap"
                className="block px-3 py-2 text-sm rounded-md hover:bg-accent hover:text-accent-foreground"
                onClick={() => setIsMobileMenuOpen(false)}
              >
                Cota Parlamentar (CEAPS)
              </Link>
              <Link
                to="/senador/folha-pagamento"
                className="block px-3 py-2 text-sm rounded-md hover:bg-accent hover:text-accent-foreground"
                onClick={() => setIsMobileMenuOpen(false)}
              >
                Folha de Pagamento
              </Link>
            </div>

            <div className="space-y-1">
              <p className="text-sm font-medium text-muted-foreground mb-2">Assembleias</p>
              <Link
                to="/deputado-estadual/ceap"
                className="block px-3 py-2 text-sm rounded-md hover:bg-accent hover:text-accent-foreground"
                onClick={() => setIsMobileMenuOpen(false)}
              >
                Cota parlamentar (CEAP)
              </Link>
            </div>

            <Link
              to="/sobre"
              className="block px-3 py-2 text-sm rounded-md hover:bg-accent hover:text-accent-foreground"
              onClick={() => setIsMobileMenuOpen(false)}
            >
              Sobre
            </Link>
          </nav>
        </div>
      )}
    </header>
  );
};
