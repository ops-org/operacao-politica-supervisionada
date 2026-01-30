import { Facebook, Twitter, Youtube, Github, ExternalLink } from "lucide-react";

export const Footer = () => {
  return (
    <footer className="relative mt-10 border-t border-border/20 bg-card/50 backdrop-blur-sm pt-10 pb-8">
      <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-primary via-accent to-primary opacity-50"></div>
      <div className="container mx-auto px-4">
        <div className="grid gap-8 md:grid-cols-4 mb-6">
          <div className="col-span-1 md:col-span-1">
            <div className="flex items-center gap-2 mb-4">
              <span className="font-bold text-lg bg-clip-text text-transparent bg-gradient-to-r from-primary to-foreground">
                Operação Política Supervisionada
              </span>
            </div>
            <p className="text-sm text-muted-foreground leading-relaxed">
              Fiscalize os gastos públicos. Transparência e controle social ao seu alcance.
            </p>
          </div>

          <div className="col-span-1">
            <h4 className="font-semibold mb-4 text-foreground">Links Rápidos</h4>
            <nav className="flex flex-col gap-2 text-sm">
              <a href="/" className="text-muted-foreground transition-colors hover:text-primary hover:translate-x-1 duration-200">
                Início
              </a>
              <a href="/sobre" className="text-muted-foreground transition-colors hover:text-primary hover:translate-x-1 duration-200">
                Sobre
              </a>
              <a href="https://institutoops.org.br/" target="_blank" rel="noopener noreferrer" className="text-muted-foreground transition-colors hover:text-primary hover:translate-x-1 duration-200 inline-flex items-center gap-1">
                Instituto OPS <ExternalLink className="h-3 w-3" />
              </a>
            </nav>
          </div>

          <div className="col-span-1">
            <h4 className="font-semibold mb-4 text-foreground">Cota Parlamentar</h4>
            <nav className="flex flex-col gap-2 text-sm">
              <a href="/deputado-federal/ceap" className="text-muted-foreground transition-colors hover:text-primary hover:translate-x-1 duration-200">
                Câmara Federal
              </a>
              <a href="/senador/ceap" className="text-muted-foreground transition-colors hover:text-primary hover:translate-x-1 duration-200">
                Senado Federal
              </a>
              <a href="/deputado-estadual/ceap" className="text-muted-foreground transition-colors hover:text-primary hover:translate-x-1 duration-200">
                Assembleias Legislativas
              </a>
            </nav>
          </div>

          <div className="col-span-1">
            <h4 className="font-semibold mb-4 text-foreground">Redes Sociais</h4>
            <div className="flex gap-3">
              <a href="https://www.facebook.com/institutoops" target="_blank" rel="noopener noreferrer" className="flex h-10 w-10 items-center justify-center rounded-full bg-muted/50 text-muted-foreground transition-all hover:bg-primary hover:text-white hover:-translate-y-1 shadow-sm hover:shadow-md">
                <Facebook className="h-5 w-5" />
              </a>
              <a href="https://twitter.com/LucioBig" target="_blank" rel="noopener noreferrer" className="flex h-10 w-10 items-center justify-center rounded-full bg-muted/50 text-muted-foreground transition-all hover:bg-sky-500 hover:text-white hover:-translate-y-1 shadow-sm hover:shadow-md">
                <Twitter className="h-5 w-5" />
              </a>
              <a href="https://www.youtube.com/channel/UCfQ98EX3oOv6IHBdUNMJq8Q" target="_blank" rel="noopener noreferrer" className="flex h-10 w-10 items-center justify-center rounded-full bg-muted/50 text-muted-foreground transition-all hover:bg-red-600 hover:text-white hover:-translate-y-1 shadow-sm hover:shadow-md">
                <Youtube className="h-5 w-5" />
              </a>
              <a href="https://github.com/ops-org/operacao-politica-supervisionada" target="_blank" rel="noopener noreferrer" className="flex h-10 w-10 items-center justify-center rounded-full bg-muted/50 text-muted-foreground transition-all hover:bg-gray-800 hover:text-white hover:-translate-y-1 shadow-sm hover:shadow-md">
                <Github className="h-5 w-5" />
              </a>
            </div>
          </div>
        </div>

        <div className="border-t border-border/40 pt-8 flex flex-col items-center justify-between gap-4 md:flex-row text-sm text-muted-foreground">
          <p>© {new Date().getFullYear()} Operação Política Supervisionada. Todos os direitos reservados.</p>
          <div className="flex items-center gap-1">
            <span>Desenvolvido com</span>
            <span className="text-red-500 animate-pulse">❤</span>
            <span>pela comunidade</span>
          </div>
        </div>
      </div>
    </footer>
  );
};
