import { Facebook, Twitter, Youtube, Github, ExternalLink } from "lucide-react";

export const Footer = () => {
  return (
    <footer className="border-t border-border bg-card py-8">
      <div className="container mx-auto px-4">
        <div className="flex flex-col items-center justify-between gap-4 md:flex-row">
          <div className="flex items-center gap-2">
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-primary-foreground font-bold text-sm">
              OPS
            </div>
            <span className="text-sm text-muted-foreground">
              Operação Política Supervisionada
            </span>
          </div>

          <nav className="flex flex-wrap justify-center gap-6 text-sm">
            <a href="/sobre" className="text-muted-foreground transition-colors hover:text-foreground">
              Sobre
            </a>
            <a href="https://institutoops.org.br/" target="_blank" rel="noopener noreferrer" className="text-muted-foreground transition-colors hover:text-foreground">
              Instituto OPS
            </a>
          </nav>

          <div className="flex gap-4">
            <a href="https://www.facebook.com/institutoops" target="_blank" rel="noopener noreferrer" className="text-muted-foreground transition-colors hover:text-foreground">
              <Facebook className="h-5 w-5" />
            </a>
            <a href="https://twitter.com/LucioBig" target="_blank" rel="noopener noreferrer" className="text-muted-foreground transition-colors hover:text-foreground">
              <Twitter className="h-5 w-5" />
            </a>
            <a href="https://www.youtube.com/channel/UCfQ98EX3oOv6IHBdUNMJq8Q" target="_blank" rel="noopener noreferrer" className="text-muted-foreground transition-colors hover:text-foreground">
              <Youtube className="h-5 w-5" />
            </a>
            <a href="https://github.com/ops-org/operacao-politica-supervisionada" target="_blank" rel="noopener noreferrer" className="text-muted-foreground transition-colors hover:text-foreground">
              <Github className="h-5 w-5" />
            </a>
          </div>
        </div>
      </div>
    </footer>
  );
};
