import { Facebook, Twitter, Youtube, Github, ExternalLink } from "lucide-react";

export const Footer = () => {
  return (
    <footer className="relative mt-10 border-t border-border/20 bg-card/50 backdrop-blur-sm pt-8 lg:pt-10 pb-8">
      <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-primary via-accent to-primary opacity-50"></div>
      <div className="container mx-auto px-4">
        <div className="flex flex-col lg:flex-row items-center justify-between gap-8 lg:gap-12 mb-6">
          {/* Brand */}
          <div className="text-center lg:text-left">
            <div className="flex items-center justify-center lg:justify-start gap-2 mb-2 lg:mb-4">
              <span className="font-bold text-2xl bg-clip-text text-transparent bg-gradient-to-r from-primary to-foreground tracking-tighter">
                OPS
              </span>
            </div>
            <p className="text-sm text-muted-foreground leading-snug lg:leading-relaxed max-w-xs mx-auto lg:mx-0">
              Acompanhe e fiscalize o uso do seu dinheiro.
            </p>
          </div>

          {/* Fiscalize - Horizontal */}
          <div className="text-center">
            <h4 className="font-bold text-[11px] uppercase tracking-widest mb-4 lg:mb-6 text-foreground/70">Fiscalize</h4>
            <nav className="flex flex-wrap items-center justify-center gap-x-8 gap-y-3 lg:gap-y-4 text-sm font-medium">
              <a href="/deputado-federal/ceap" className="text-muted-foreground transition-colors hover:text-primary">Câmara Federal</a>
              <a href="/senador/ceap" className="text-muted-foreground transition-colors hover:text-primary">Senado Federal</a>
              <a href="/deputado-estadual/ceap" className="text-muted-foreground transition-colors hover:text-primary">Assembleias</a>
              <a href="https://institutoops.org.br/" target="_blank" rel="noopener noreferrer" className="hidden lg:inline-flex text-muted-foreground transition-colors hover:text-primary items-center gap-1.5 focus:outline-none focus:ring-2 focus:ring-primary/20 rounded">
                Instituto <ExternalLink className="h-3 w-3" />
              </a>
            </nav>
          </div>

          {/* Social */}
          <div className="text-center lg:text-right">
            <h4 className="font-bold text-[11px] uppercase tracking-widest mb-4 lg:mb-6 text-foreground/70">Redes Sociais</h4>
            <div className="flex justify-center lg:justify-end gap-5 lg:gap-6">
              <a href="https://www.facebook.com/institutoops" target="_blank" rel="noopener noreferrer" className="text-muted-foreground transition-all hover:text-primary hover:-translate-y-1">
                <Facebook className="h-5 w-5" />
              </a>
              <a href="https://twitter.com/LucioBig" target="_blank" rel="noopener noreferrer" className="text-muted-foreground transition-all hover:text-sky-500 hover:-translate-y-1">
                <Twitter className="h-5 w-5" />
              </a>
              <a href="https://www.youtube.com/channel/UCfQ98EX3oOv6IHBdUNMJq8Q" target="_blank" rel="noopener noreferrer" className="text-muted-foreground transition-all hover:text-red-600 hover:-translate-y-1">
                <Youtube className="h-5 w-5" />
              </a>
              <a href="https://github.com/ops-org/operacao-politica-supervisionada" target="_blank" rel="noopener noreferrer" className="text-muted-foreground transition-all hover:text-foreground hover:-translate-y-1">
                <Github className="h-5 w-5" />
              </a>
            </div>
          </div>
        </div>

        <div className="border-t border-border/20 pt-6 lg:pt-8 flex flex-col items-center justify-between gap-4 lg:gap-6 md:flex-row text-[12px] text-muted-foreground font-medium uppercase tracking-wider">
          <p>© {new Date().getFullYear()} Operação Política Supervisionada</p>
          <div className="flex items-center gap-2">
            <span>Desenvolvido com</span>
            <span className="text-red-500 animate-pulse">❤</span>
            <span>pela comunidade</span>
          </div>
        </div>
      </div>
    </footer>
  );
};
