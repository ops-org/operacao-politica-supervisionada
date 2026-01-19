export const InfoSection = () => {
  return (
    <section className="bg-muted/50 py-12">
      <div className="container mx-auto px-4">
        <h2 className="mb-6 text-2xl font-bold text-foreground">
          O que é a cota parlamentar?
        </h2>

        <div className="space-y-4 text-foreground/80">
          <p>
            A <strong className="text-foreground">cota parlamentar</strong>, também conhecida como verba indenizatória é um{" "}
            <strong className="text-primary">recurso financeiro público</strong> disponibilizado a todos os{" "}
            <strong className="text-foreground">deputados federais e senadores</strong> para o custeio de seus mandatos.
          </p>

          <p>
            Cada deputado federal tem a seu dispor valores mensais cumulativos, a depender do estado, que vão de{" "}
            <a href="http://www.camara.leg.br/cota-parlamentar/ANEXO_ATO_DA_MESA_43_2009.pdf" target="_blank"
              rel="nofollow noopener noreferrer"
              title="Clique para visualizar a lista oficial de valores por estado"><strong className="text-primary underline">R$ 36,6 mil
                a R$ 51,4 mil</strong></a>, a depender do estado de origem, para custear despesas
            com alimentação, viagens, hospedagens, combustível, serviços de consultoria, locação de carros, barcos, aviões e casas, além de muitos outras.
          </p>

          <p>
            No Senado os valores vão{" "}
            <a href="https://www12.senado.leg.br/transparencia/leg/pdf/CotaExercicioAtivParlamSenadores.pdf"
              target="_blank" rel="nofollow noopener noreferrer"
              title="Clique para visualizar a lista official de valores por estado"><strong className="text-primary underline">de R$ 21
                mil até R$ 44,2 mil</strong></a> por mês.
          </p>

          <p>
            Para ter este recurso público liberado pela Câmara ou Senado, o parlamentar precisa apenas apresentar a nota fiscal ou recibo da despesa. Não é feita
            licitação, tomada de preço ou qualquer outro recurso legal que permita a utilização de dinheiro público de forma eficiente, econômica, legal, impessoal e
            moral.
          </p>

          <p>
            Veja as <a href="https://institutoops.org.br/o-que-ja-fizemos/" target="_blank"
              rel="nofollow noopener noreferrer"><strong className="text-primary underline">irregularidades já descobertas pela OPS</strong></a>.
          </p>
        </div>
      </div>
    </section>
  );
};