import { useLocation } from "react-router-dom";
import { useEffect } from "react";

const NotFound = () => {
  const location = useLocation();

  useEffect(() => {
    console.error("404 Error: User attempted to access non-existent route:", location.pathname);
  }, [location.pathname]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-muted">
      <div id="e404" className="max-w-[600px] mx-auto text-center">
        <h2 className="mb-4 text-4xl font-bold">404 - Esta página não existe</h2>
        <p className="mb-4 text-xl text-muted-foreground">
          Você pode ter digitado incorretamente o endereço ou a página pode ter sido movida. Gostaria de voltar para o{" "}
          <a href="/" className="text-primary underline hover:text-primary/90">
            início
          </a>
          ?
        </p>
        <img 
          src="//static.ops.net.br/img/404.gif" 
          alt="Travolta está desesperadamente perdido" 
          className="mx-auto mt-4"
        />
      </div>
    </div>
  );
};

export default NotFound;
