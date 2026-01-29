import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ExternalLink, Youtube, Users, FileText, HelpCircle, HelpCircleIcon, HandHelping, Info, CheckCheck } from "lucide-react";
import { useState, useEffect } from "react";
import { fetchImportacao, ImportacaoData } from "@/lib/api";

const Sobre = () => {
  const [importacaoData, setImportacaoData] = useState<ImportacaoData[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadData = async () => {
      try {
        const data = await fetchImportacao();
        const filteredData = data.filter(item => item.sigla !== "");
        setImportacaoData(filteredData);
      } catch (error) {
        console.error('Error fetching importacao data:', error);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

  return (
    <div className="min-h-screen bg-gradient-to-br from-background to-muted/20">
      <Header />
      <main className="container mx-auto px-4 py-8">
        {/* Hero Section */}
        <div className="text-center mb-12">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-primary/10 rounded-full mb-4">
            <Users className="w-8 h-8 text-primary" />
          </div>
          <h1 className="text-4xl font-bold text-foreground mb-4">
            Conheça a Operação Política Supervisionada
          </h1>
          <p className="text-lg text-muted-foreground mx-auto max-w-2xl">
            Fiscalização detalhada de gastos públicos com transparência e responsabilidade
          </p>
        </div>

        {/* Videos Section */}
        <div className="mb-12">
          <div className="text-center mb-8">
            <div className="inline-flex items-center justify-center w-12 h-12 bg-primary/10 rounded-full mb-4">
              <Youtube className="w-6 h-6 text-primary" />
            </div>
            <h2 className="text-2xl font-bold text-foreground mb-4">Vídeos Institucionais</h2>
            <p className="text-muted-foreground">Conheça mais sobre nosso trabalho de fiscalização</p>
          </div>
          <div className="grid gap-6 md:grid-cols-2">
            <Card className="overflow-hidden shadow-lg hover:shadow-xl transition-shadow duration-300">
              <div className="aspect-video bg-muted">
                <iframe
                  width="540"
                  height="303.75"
                  src="https://www.youtube-nocookie.com/embed/JqO0_EhbJUw"
                  frameBorder="0"
                  allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture"
                  allowFullScreen
                  className="w-full h-full"
                ></iframe>
              </div>
            </Card>
            <Card className="overflow-hidden shadow-lg hover:shadow-xl transition-shadow duration-300">
              <div className="aspect-video bg-muted">
                <iframe
                  width="540"
                  height="303.75"
                  src="https://www.youtube-nocookie.com/embed/lcNf9gi2FdY"
                  frameBorder="0"
                  allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture"
                  allowFullScreen
                  className="w-full h-full"
                ></iframe>
              </div>
            </Card>
          </div>
        </div>

        {/* Info Cards */}
        <div className="space-y-12 mb-8">
          <div className="text-center">
            <h2 className="text-2xl font-bold text-foreground mb-4">Informações Principais</h2>
            <p className="text-muted-foreground max-w-2xl mx-auto">
              Entenda mais sobre a OPS e o CEAP
            </p>
          </div>

          <div className="grid gap-8 md:grid-cols-2">
            <Card className="shadow-lg hover:shadow-xl transition-shadow duration-300">
              <CardHeader>
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-primary/10 rounded-lg">
                    <Users className="w-5 h-5 text-primary" />
                  </div>
                  <CardTitle className="text-lg">O que é a OPS?</CardTitle>
                </div>
              </CardHeader>
              <CardContent className="space-y-4 text-sm text-muted-foreground">
                <p>
                  A <strong className="text-foreground">Operação Política Supervisionada</strong> fiscaliza
                  detalhadamente gastos realizados com verba pública por deputados federais, senadores e de
                  deputados estaduais dos estados que possuem transparência.
                </p>
                <p>
                  Até o momento a OPS proporcionou{" "}
                  <a
                    href="https://institutoops.org.br/o-que-ja-fizemos/"
                    target="_blank"
                    rel="nofollow noopener noreferrer"
                  ><strong className="text-primary underline">economia os cofres públicos em valor superior a R$ 6
                    milhões</strong></a>{" "}
                  graças a este trabalho de fiscalização que permite exigir diretamente dos parlamentares
                  a devolução dos valores indevidamente utilizados.
                </p>
                <p>
                  A OPS conta com a ajuda de cidadãos espalhados pelo Brasil e exterior que ajudam
                  diretamente nas fiscalizações que são realizadas, no desenvolvimento de ferramentas de
                  tecnologia e no suporte financeiro que é feito por{" "}
                  <a
                    href="https://institutoops.org.br/apoio/"
                    target="_blank"
                    rel="nofollow noopener noreferrer"
                  ><strong className="text-primary underline">doações voluntárias</strong></a>.
                </p>
                <p>
                  Apesar de ser especialista na fiscalização dos gastos com a verba indenizatória, a OPS
                  também monitora contas públicas de estados e municípios, além de disponibilizar aos
                  cidadãos um{" "}
                  <a
                    href="https://institutoops.org.br/denunciar/"
                    target="_blank"
                    rel="nofollow noopener noreferrer"
                  ><strong className="text-primary underline">canal para o recebimento de denúncias</strong></a>, anônimas ou não.
                </p>
                <p>
                  A OPS é coordenada por seu fundador,{" "}
                  <a href="https://institutoops.org.br/quem-somos/" target="_blank" rel="nofollow noopener noreferrer"
                  ><strong className="text-primary underline">Lúcio Big</strong></a>.
                </p>
              </CardContent>
              <CardHeader>
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-blue-500/10 rounded-lg">
                    <FileText className="w-5 h-5 text-blue-600" />
                  </div>
                  <CardTitle className="text-lg">O que é CEAP?</CardTitle>
                </div>
              </CardHeader>
              <CardContent className="space-y-4 text-sm text-muted-foreground">
                <p>
                  CEAP é a sigla da <strong className="text-foreground">Cota para o Exercício da Atividade Parlamentar</strong>{" "}
                  (No Senado se chama CEAPS), também é conhecida como verba indenizatória. É um{" "}
                  <strong className="text-primary">recurso financeiro público</strong> disponibilizado a todos os{" "}
                  <strong className="text-foreground">deputados e senadores</strong> para o custeio de suas atividades parlamentares.{" "}
                  Nos estados essa verba pode receber diversos outros nomes.
                </p>
                <p>
                  Cada deputado federal tem a seu dispor valores mensais cumulativos, a depender do estado, que vão de{" "}
                  <a href="http://www.camara.leg.br/cota-parlamentar/ANEXO_ATO_DA_MESA_43_2009.pdf" target="_blank" rel="nofollow noopener noreferrer"
                    title="Clique para visualizar a lista oficial de valores por estado">
                    <strong className="text-primary underline">R$ 36,6 mil a R$ 51,4 mil</strong>
                  </a>, a depender do estado de origem, para custear despesas
                  com alimentação, viagens, hospedagens, combustível, serviços de consultoria, locação de carros, barcos, aviões e casas, além de muitos outras.
                  No Senado os valores vão{" "}
                  <a href="https://www12.senado.leg.br/transparencia/leg/pdf/CotaExercicioAtivParlamSenadores.pdf"
                    target="_blank" rel="nofollow noopener noreferrer"
                    title="Clique para visualizar a lista official de valores por estado">
                    <strong className="text-primary underline">de R$ 21 mil até R$ 44,2 mil</strong>
                  </a> por mês.
                </p>
                <p>
                  Apesar de ser dinheiro público, fato que já implicaria no seu enquadramento à Lei de
                  Licitações, o uso da verba dispensa qualquer tipo de burocracia exigida por essa lei,
                  dando ao político a livre escolha da empresa a ser contratada para fornecimento do
                  produto ou serviço. Há casos que nem mesmo empresas são contratadas, apenas pessoas
                  físicas.
                </p>
                <p>
                  Não é raro encontrar irregularidades no uso desta cota. Algumas são escandalosas, como
                  locações de veículos feitas em padarias ou até de pessoas que nem sabiam possuir o
                  veículo locado pelo parlamentar, até irregularidades de cunho fiscal, como a emissão
                  de notas fiscais sem lastro legal.
                </p>
                <p>
                  Para ter este recurso público liberado pela Câmara ou Senado, o parlamentar precisa apenas apresentar a nota fiscal ou recibo da despesa. Não é feita
                  licitação, tomada de preço ou qualquer outro recurso legal que permita a utilização de dinheiro público de forma eficiente, econômica, legal, impessoal e
                  moral. Por vezes nem o proprio CNPJ ou CPF do prestador de serviços é validado.
                </p>
                <p>
                  Veja as <a href="https://institutoops.org.br/o-que-ja-fizemos/" target="_blank"
                    rel="nofollow noopener noreferrer"><strong className="text-primary underline">irregularidades já descobertas pela OPS</strong></a>.
                </p>
              </CardContent>
              <CardHeader>
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-primary/10 rounded-lg">
                    <HandHelping className="w-5 h-5 text-primary" />
                  </div>
                  <CardTitle className="text-lg">Como posso ajudar?</CardTitle>
                </div>
              </CardHeader>
              <CardContent className="space-y-4 text-sm text-muted-foreground">
                <p>
                  Existem várias formas de contribuir com a OPS:
                </p>
                <ul className="list-disc pl-5 space-y-2">
                  <li>
                    <strong className="text-foreground">Fiscalizando:</strong> Use o site para
                    verificar os gastos dos parlamentares do seu estado
                  </li>
                  <li>
                    <strong className="text-foreground">Denunciando:</strong> Envie denúncias de
                    irregularidades através do nosso canal de denúncias
                  </li>
                  <li>
                    <strong className="text-foreground">Divulgando:</strong> Compartilhe nosso trabalho
                    nas redes sociais
                  </li>
                  <li>
                    <strong className="text-foreground">Doando:</strong>{" "}
                    <a href="https://institutoops.org.br/apoio/" target="_blank" rel="nofollow noopener noreferrer" className="hover:underline inline-flex items-center gap-1">
                      Contribua financeiramente para manter o projeto funcionando <ExternalLink className="h-3 w-3" /></a>
                  </li>
                </ul>
              </CardContent>
              <CardHeader>
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-primary/10 rounded-lg">
                    <Info className="w-5 h-5 text-primary" />
                  </div>
                  <CardTitle className="text-lg">Informações úteis para fiscalização</CardTitle>
                </div>
              </CardHeader>
              <CardContent className="space-y-4 text-sm text-muted-foreground">
                <ol className="list-decimal pl-5 space-y-4">
                  <li className="mb-3">
                    <strong className="text-foreground">Câmara de Deputados</strong>
                    <ul className="list-disc pl-5 mt-2 space-y-1">
                      <li>
                        <a
                          href="https://www2.camara.leg.br/comunicacao/assessoria-de-imprensa/guia-para-jornalistas/cota-parlamentar"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Regras para uso da cota parlamentar (Ato da mesa Nº 43) <ExternalLink className="h-3 w-3" />
                        </a>
                      </li>
                      <li>
                        <a
                          href="http://www.camara.gov.br/cota-parlamentar/ANEXO_ATO_DA_MESA_43_2009.pdf"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Limites mensais por parlamentar <ExternalLink className="h-3 w-3" />
                        </a>
                      </li>
                      <li>
                        <a
                          href="http://www.camara.gov.br/cota-parlamentar"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Explorar a cota parlamentar na câmara <ExternalLink className="h-3 w-3" />
                        </a>
                      </li>
                      <li>
                        <a
                          href="http://www2.camara.leg.br/deputados/pesquisa"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Conheça os Deputados <ExternalLink className="h-3 w-3" />
                        </a>
                      </li>
                    </ul>
                  </li>
                  <li className="mb-3">
                    <strong className="text-foreground">Senado Federal</strong>
                    <ul className="list-disc pl-5 mt-2 space-y-1">
                      <li>
                        <a
                          href="https://adm.senado.leg.br/normas/ui/pub/normaConsultada;jsessionid=FF712C2488692EF4C9A4517976A6F3FE.tomcat-1?0&idNorma=14380751"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Regras para uso da cota parlamentar <ExternalLink className="h-3 w-3" />
                        </a>
                      </li>
                      <li>
                        <a
                          href="https://www12.senado.leg.br/transparencia/leg/legislacao-relacionada"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Legislação relacionada <ExternalLink className="h-3 w-3" />
                        </a>
                      </li>
                      <li>
                        <a
                          href="http://www25.senado.leg.br/web/transparencia/sen"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Explorar a cota parlamentar no senado <ExternalLink className="h-3 w-3" />
                        </a>
                      </li>
                      <li>
                        <a
                          href="http://www25.senado.leg.br/web/senadores"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Conheça os senadores <ExternalLink className="h-3 w-3" />
                        </a>
                      </li>
                    </ul>
                  </li>
                </ol>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-primary/10 rounded-lg">
                    <CheckCheck className="w-5 h-5 text-primary" />
                  </div>
                  <CardTitle className="text-lg">Transparência</CardTitle>
                </div>
              </CardHeader>
              <CardContent className="space-y-4 text-sm text-muted-foreground">
                <p>
                  A OPS preza pela total transparência em suas ações. Todos os dados utilizados são
                  provenientes de fontes oficiais e públicas:
                </p>
                <ol className="list-decimal pl-5 space-y-4">
                  <li className="mb-3">
                    <strong className="text-foreground">Câmara de Deputados</strong>
                    <br />
                    <ul className="list-disc pl-5 mt-2 space-y-1">
                      <li>
                        <a
                          href="https://www.camara.leg.br/transparencia/gastos-parlamentares"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Cota parlamentar <ExternalLink className="h-3 w-3" />
                        </a>
                      </li>
                      <li>
                        <a
                          href="https://dadosabertos.camara.leg.br/"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Dados Abertos <ExternalLink className="h-3 w-3" />
                        </a>
                      </li>
                      <li>
                        <a
                          href="https://www.camara.leg.br/deputados/quem-sao"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Conheça os Deputados <ExternalLink className="h-3 w-3" />
                        </a>
                      </li>
                    </ul>
                  </li>
                  <li className="mb-3">
                    <strong className="text-foreground">Senado Federal</strong>
                    <br />
                    <ul className="list-disc pl-5 mt-2 space-y-1">
                      <li>
                        <a
                          href="https://www12.senado.leg.br/dados-abertos/conjuntos?portal=Administrativo&grupo=senadores"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Cota parlamentar (CSV) <ExternalLink className="h-3 w-3" />
                        </a>
                      </li>
                      <li>
                        <a
                          href="https://www25.senado.leg.br/web/senadores/em-exercicio"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Conheça os Senadores <ExternalLink className="h-3 w-3" />
                        </a>
                      </li>
                    </ul>
                  </li>
                  <li className="mb-3">
                    <strong className="text-foreground">Assembleias Legislativas</strong>
                    {(() => {
                      if (loading) {
                        return <div className="mt-2 text-muted-foreground">Carregando dados das assembleias legislativas...</div>;
                      }

                      if (importacaoData.length > 0) {
                        return (
                          <div className="mt-2 overflow-x-auto">
                            <table className="w-full text-sm">
                              <thead>
                                <tr className="border-b">
                                  <th className="text-left py-2 px-2">Assembleia</th>
                                  <th className="text-left py-2 px-2">Última Despesa</th>
                                  <th className="text-left py-2 px-2">Última Importação</th>
                                </tr>
                              </thead>
                              <tbody>
                                {importacaoData.map((item) => (
                                  <tr key={item.id} className="border-b hover:bg-muted/50">
                                    <td className="py-2 px-2">
                                      <a
                                        href={item.url}
                                        target="_blank"
                                        rel="nofollow noopener noreferrer"
                                        className="text-primary hover:underline inline-flex items-center gap-1"
                                      >
                                        {item.nome} {item.sigla && <>({item.sigla})</>} <ExternalLink className="h-3 w-3" />
                                      </a>
                                      {item.info && <div className="text-muted-foreground text-xs mt-1">* {item.info}</div>}
                                    </td>
                                    <td className="py-2 px-2 text-muted-foreground">
                                      {item.ultima_despesa || '-'}
                                    </td>
                                    <td className="py-2 px-2 text-muted-foreground">
                                      {item.ultima_importacao || '-'}
                                    </td>
                                  </tr>
                                ))}
                              </tbody>
                            </table>
                          </div>
                        );
                      }

                      return <div className="mt-2 text-muted-foreground">Nenhum dado disponível no momento.</div>;
                    })()}
                  </li>
                  <li className="mb-3">
                    <strong className="text-foreground">Fornecedores</strong>
                    <ul className="list-disc pl-5 mt-2 space-y-1">
                      <li>
                        <a
                          href="http://www.receita.fazenda.gov.br/pessoajuridica/cnpj/cnpjreva/cnpjreva_solicitacao.asp"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1"
                        >
                          Receita Federal <ExternalLink className="h-3 w-3" />
                        </a>{" "}
                        (via
                        <a
                          href="https://minhareceita.org"
                          target="_blank"
                          rel="nofollow noopener noreferrer"
                          className="text-primary hover:underline inline-flex items-center gap-1 ml-1"
                        >
                          Minha Receita <ExternalLink className="h-3 w-3" />
                        </a>
                        )
                      </li>
                    </ul>
                  </li>
                </ol>
              </CardContent>
            </Card>
          </div>
        </div>

        {/* Contact Section */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Entre em Contato</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4 text-sm text-muted-foreground">
            <p>
              Dúvidas, sugestões ou denúncias? Entre em contato conosco através dos nossos canais:
            </p>
            <div className="flex flex-wrap gap-4">
              <a href="https://institutoops.org.br/" target="_blank" rel="noopener noreferrer" className="text-primary hover:underline inline-flex items-center gap-1">
                Instituto OPS <ExternalLink className="h-3 w-3" />
              </a>
              <a href="mailto:contato@ops.org.br" className="text-primary hover:underline">
                contato@ops.org.br
              </a>
              <a href="https://twitter.com/LucioBig" target="_blank" rel="noopener noreferrer" className="text-primary hover:underline inline-flex items-center gap-1">
                Twitter <ExternalLink className="h-3 w-3" />
              </a>
              <a href="https://www.facebook.com/institutoops" target="_blank" rel="noopener noreferrer" className="text-primary hover:underline inline-flex items-center gap-1">
                Facebook <ExternalLink className="h-3 w-3" />
              </a>
              <a href="https://www.youtube.com/channel/UCfQ98EX3oOv6IHBdUNMJq8Q" target="_blank" rel="noopener noreferrer" className="text-primary hover:underline inline-flex items-center gap-1">
                YouTube <ExternalLink className="h-3 w-3" />
              </a>
              <a href="https://github.com/ops-org/operacao-politica-supervisionada" target="_blank" rel="noopener noreferrer" className="text-primary hover:underline inline-flex items-center gap-1">
                Github <ExternalLink className="h-3 w-3" />
              </a>
            </div>
          </CardContent>
        </Card>
      </main >
      <Footer />
    </div >
  );
};

export default Sobre;
