import { useState, useEffect } from "react";
import { TopSpenderCard } from "./TopSpenderCard";
import { fetchTopSpenders, TopSpender } from "@/lib/api";

const parsePartyState = (siglaPartidoEstado: string): { party: string; state: string } => {
  const [party, state] = siglaPartidoEstado.split(" / ");
  return { party: party || "", state: state || "" };
};

export const TopSpendersSection = () => {
  const [data, setData] = useState<{ senadores: any[]; deputadosFederais: any[]; deputadosEstaduais: any[]; } | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const response = await fetchTopSpenders();

        const senadores = response.senado.map((item: TopSpender) => {
          const { party, state } = parsePartyState(item.sigla_partido_estado);
          return {
            name: item.nome_parlamentar,
            party,
            state,
            amount: item.valor_total,
            id: item.id_sf_senador,
            type: "senador" as const
          };
        });

        const deputadosFederais = response.camara_federal.map((item: TopSpender) => {
          const { party, state } = parsePartyState(item.sigla_partido_estado);
          return {
            name: item.nome_parlamentar,
            party,
            state,
            amount: item.valor_total,
            id: item.id_cf_deputado,
            type: "deputado-federal" as const
          };
        });

        const deputadosEstaduais = response.camara_estadual.map((item: TopSpender) => {
          const { party, state } = parsePartyState(item.sigla_partido_estado);
          return {
            name: item.nome_parlamentar,
            party,
            state,
            amount: item.valor_total,
            id: item.id_cl_deputado,
            type: "deputado-estadual" as const
          };
        });

        setData({ senadores, deputadosFederais, deputadosEstaduais });
      } catch (err) {
        setError("Falha ao carregar dados dos parlamentares");
        console.error("Error fetching top spenders:", err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  if (loading) {
    return (
      <section className="py-12">
        <div className="container mx-auto px-4">
          <h2 className="mb-2 text-2xl font-bold text-foreground">
            Campeões de gastos
          </h2>
          <p className="mb-8 text-muted-foreground">
            Os Parlamentares que mais gastaram dinheiro público da verba indenizatória da atual legislatura
          </p>
          <div className="text-center py-8">
            <p className="text-muted-foreground">Carregando...</p>
          </div>
        </div>
      </section>
    );
  }

  if (error || !data) {
    return (
      <section className="py-12">
        <div className="container mx-auto px-4">
          <h2 className="mb-2 text-2xl font-bold text-foreground">
            Campeões de gastos
          </h2>
          <p className="mb-8 text-muted-foreground">
            Os Parlamentares que mais gastaram dinheiro público da verba indenizatória da atual legislatura
          </p>
          <div className="text-center py-8">
            {error && <p className="text-destructive">{error || "Dados não disponíveis"}</p>}
            {!data && <p className="text-muted-foreground">Carregando dados...</p>}
          </div>
        </div>
      </section>
    );
  }

  return (
    <section className="py-12">
      <div className="container mx-auto px-4">
        <h2 className="mb-2 text-2xl font-bold text-foreground">
          Campeões de gastos
        </h2>
        <p className="mb-8 text-muted-foreground">
          Os Parlamentares que mais gastaram dinheiro público da verba indenizatória da atual legislatura
        </p>

        <div className="mb-10">
          <h3 className="mb-4 text-lg font-semibold text-foreground">
            Senadores <span className="font-normal text-muted-foreground">(Desde fevereiro de 2023)</span>
          </h3>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {data.senadores.map((senador) => (
              <TopSpenderCard key={senador.id} {...senador} />
            ))}
          </div>
        </div>

        <div className="mb-10">
          <h3 className="mb-4 text-lg font-semibold text-foreground">
            Deputados Federais <span className="font-normal text-muted-foreground">(Desde fevereiro de 2023)</span>
          </h3>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {data.deputadosFederais.map((deputado) => (
              <TopSpenderCard key={deputado.id} {...deputado} />
            ))}
          </div>
        </div>

        <div>
          <h3 className="mb-4 text-lg font-semibold text-foreground">
            Deputados Estaduais <span className="font-normal text-muted-foreground"></span>
          </h3>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {data.deputadosEstaduais.length > 0 ? (
              data.deputadosEstaduais.map((deputado) => (
                <TopSpenderCard key={deputado.id} {...deputado} />
              ))
            ) : (
              <div className="col-span-full text-center text-muted-foreground">
                (Em breve)
              </div>
            )}
          </div>
        </div>
      </div>
    </section >
  );
};
