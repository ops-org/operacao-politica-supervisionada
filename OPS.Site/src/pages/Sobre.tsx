import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ExternalLink, Youtube, Users, FileText, HelpCircle, HelpCircleIcon, HandHelping, Info, CheckCheck } from "lucide-react";
import { useState, useEffect } from "react";
import { fetchImportacao, ImportacaoData } from "@/lib/api";
import { usePageTitle } from "@/hooks/usePageTitle";

const Sobre = () => {
  usePageTitle("Sobre");
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
    <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
      <Header />
      <main className="container mx-auto px-4 py-8">
        {/* Hero Section */}
        <div className="text-center mt-24 mb-16">
          <h1 className="text-4xl md:text-5xl font-bold bg-gradient-to-r from-primary via-accent to-primary bg-clip-text text-transparent mb-6">
            Conheça a Operação Política Supervisionada
          </h1>
          <p className="text-xl text-muted-foreground mx-auto leading-relaxed">
            Fiscalização detalhada de gastos públicos com transparência, responsabilidade e tecnologia cívica.
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
            <Card className="overflow-hidden shadow-lg border-0 bg-card/50 backdrop-blur-sm group hover:shadow-2xl hover:scale-[1.02] transition-all duration-300">
              <div className="aspect-video bg-muted relative rounded-t-xl overflow-hidden">
                <div className="absolute inset-0 bg-gradient-to-t from-black/20 to-transparent pointer-events-none z-10" />
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
            <Card className="overflow-hidden shadow-lg border-0 bg-card/50 backdrop-blur-sm group hover:shadow-2xl hover:scale-[1.02] transition-all duration-300">
              <div className="aspect-video bg-muted relative rounded-t-xl overflow-hidden">
                <div className="absolute inset-0 bg-gradient-to-t from-black/20 to-transparent pointer-events-none z-10" />
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
            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
              <CardHeader className="pb-2">
                <div className="flex items-center gap-4">
                  <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                    <Users className="w-6 h-6 text-primary" />
                  </div>
                  <CardTitle className="text-xl">O que é a OPS?</CardTitle>
                </div>
              </CardHeader>
              <CardContent className="space-y-4 text-sm text-muted-foreground leading-relaxed pt-2">
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
                  ><strong className="text-primary hover:text-accent transition-colors underline decoration-2 underline-offset-2">economia aos cofres públicos em valor superior a R$ 6
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
                  ><strong className="text-primary hover:text-accent transition-colors underline decoration-2 underline-offset-2">doações voluntárias</strong></a>.
                </p>
                <p>
                  Apesar de ser especialista na fiscalização dos gastos com a verba indenizatória, a OPS
                  também monitora contas públicas de estados e municípios, além de disponibilizar aos
                  cidadãos um{" "}
                  <a
                    href="https://institutoops.org.br/denunciar/"
                    target="_blank"
                    rel="nofollow noopener noreferrer"
                  ><strong className="text-primary hover:text-accent transition-colors underline decoration-2 underline-offset-2">canal para o recebimento de denúncias</strong></a>, anônimas ou não.
                </p>
                <p>
                  A OPS é coordenada por seu fundador,{" "}
                  <a href="https://institutoops.org.br/quem-somos/" target="_blank" rel="nofollow noopener noreferrer"
                  ><strong className="text-primary hover:text-accent transition-colors underline decoration-2 underline-offset-2">Lúcio Big</strong></a>.
                </p>
              </CardContent>
            </Card>

            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
              <CardHeader className="pb-2">
                <div className="flex items-center gap-4">
                  <div className="p-3 bg-gradient-to-br from-accent/10 to-accent/20 rounded-xl shadow-inner border border-accent/10">
                    <FileText className="w-6 h-6 text-accent" />
                  </div>
                  <CardTitle className="text-xl">O que é CEAP?</CardTitle>
                </div>
              </CardHeader>
              <CardContent className="space-y-4 text-sm text-muted-foreground leading-relaxed pt-2">
                <p>
                  CEAP é a sigla da <strong className="text-foreground">Cota para o Exercício da Atividade Parlamentar</strong>{" "}
                  (No Senado se chama CEAPS), também é conhecida como verba indenizatória. É um{" "}
                  <strong className="text-primary font-medium">recurso financeiro público</strong> disponibilizado a todos os{" "}
                  <strong className="text-foreground">deputados e senadores</strong> para o custeio de suas atividades parlamentares.
                </p>
                <p>
                  Cada deputado federal tem a seu dispor valores mensais cumulativos de{" "}
                  <a href="http://www.camara.leg.br/cota-parlamentar/ANEXO_ATO_DA_MESA_43_2009.pdf" target="_blank" rel="nofollow noopener noreferrer"
                    title="Clique para visualizar a lista oficial de valores por estado">
                    <strong className="text-primary hover:text-accent transition-colors underline decoration-2 underline-offset-2">R$ 36,6 mil a R$ 51,4 mil</strong>
                  </a>, a depender do estado de origem, para custear despesas
                  com alimentação, viagens, hospedagens, combustível, serviços de consultoria, locação de carros, barcos, aviões e casas.
                  No Senado os valores vão{" "}
                  <a href="https://www12.senado.leg.br/transparencia/leg/pdf/CotaExercicioAtivParlamSenadores.pdf"
                    target="_blank" rel="nofollow noopener noreferrer"
                    title="Clique para visualizar a lista official de valores por estado">
                    <strong className="text-primary hover:text-accent transition-colors underline decoration-2 underline-offset-2">de R$ 21 mil até R$ 44,2 mil</strong>
                  </a> por mês.
                </p>
                <p>
                  Apesar de ser dinheiro público, o uso da verba dispensa qualquer tipo de burocracia exigida pela Lei de Licitações,
                  dando ao político a livre escolha da empresa a ser contratada.
                </p>
                <p>
                  Veja as <a href="https://institutoops.org.br/o-que-ja-fizemos/" target="_blank"
                    rel="nofollow noopener noreferrer"><strong className="text-primary hover:text-accent transition-colors underline decoration-2 underline-offset-2">irregularidades já descobertas pela OPS</strong></a>.
                </p>
              </CardContent>
            </Card>

            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
              <CardHeader className="pb-2">
                <div className="flex items-center gap-4">
                  <div className="p-3 bg-gradient-to-br from-secondary/10 to-secondary/20 rounded-xl shadow-inner border border-secondary/10">
                    <HandHelping className="w-6 h-6 text-secondary" />
                  </div>
                  <CardTitle className="text-xl">Como posso ajudar?</CardTitle>
                </div>
              </CardHeader>
              <CardContent className="space-y-4 text-sm text-muted-foreground leading-relaxed pt-2">
                <p>
                  Existem várias formas de contribuir com a OPS:
                </p>
                <ul className="grid gap-3 pt-2">
                  <li className="flex items-start gap-3 p-3 rounded-lg bg-muted/50 hover:bg-muted transition-colors">
                    <span className="bg-primary/10 text-primary rounded-full p-1 mt-0.5"><CheckCheck className="w-3 h-3" /></span>
                    <div>
                      <strong className="text-foreground block mb-1">Fiscalizando</strong>
                      Use o site para verificar os gastos dos parlamentares do seu estado
                    </div>
                  </li>
                  <li className="flex items-start gap-3 p-3 rounded-lg bg-muted/50 hover:bg-muted transition-colors">
                    <span className="bg-primary/10 text-primary rounded-full p-1 mt-0.5"><CheckCheck className="w-3 h-3" /></span>
                    <div>
                      <strong className="text-foreground block mb-1">Denunciando</strong>
                      Envie denúncias de irregularidades através do nosso canal
                    </div>
                  </li>
                  <li className="flex items-start gap-3 p-3 rounded-lg bg-muted/50 hover:bg-muted transition-colors">
                    <span className="bg-primary/10 text-primary rounded-full p-1 mt-0.5"><CheckCheck className="w-3 h-3" /></span>
                    <div>
                      <strong className="text-foreground block mb-1">Divulgando</strong>
                      Compartilhe nosso trabalho nas redes sociais
                    </div>
                  </li>
                  <li className="flex items-start gap-3 p-3 rounded-lg bg-primary/5 hover:bg-primary/10 transition-colors border border-primary/10">
                    <span className="bg-primary/10 text-primary rounded-full p-1 mt-0.5"><CheckCheck className="w-3 h-3" /></span>
                    <div>
                      <strong className="text-foreground block mb-1">Doando</strong>
                      <a href="https://institutoops.org.br/apoio/" target="_blank" rel="nofollow noopener noreferrer" className="text-primary hover:underline inline-flex items-center gap-1 font-medium">
                        Contribua financeiramente <ExternalLink className="h-3 w-3" />
                      </a>
                    </div>
                  </li>
                </ul>
              </CardContent>
            </Card>

            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
              <CardHeader className="pb-2">
                <div className="flex items-center gap-4">
                  <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                    <Info className="w-6 h-6 text-primary" />
                  </div>
                  <CardTitle className="text-xl">Informações úteis</CardTitle>
                </div>
              </CardHeader>
              <CardContent className="space-y-4 text-sm text-muted-foreground leading-relaxed pt-2">
                <div className="space-y-6">
                  <div>
                    <h4 className="font-semibold text-foreground flex items-center gap-2 mb-3">
                      <span className="w-1.5 h-1.5 rounded-full bg-primary/50"></span> Câmara de Deputados
                    </h4>
                    <ul className="space-y-2 pl-4 border-l-2 border-muted">
                      <li>
                        <a href="https://www2.camara.leg.br/comunicacao/assessoria-de-imprensa/guia-para-jornalistas/cota-parlamentar" target="_blank" rel="nofollow noopener noreferrer" className="text-muted-foreground hover:text-primary transition-colors hover:translate-x-1 inline-flex items-center gap-1 duration-200 block">
                          Regras para uso da cota <ExternalLink className="h-3 w-3 opacity-50" />
                        </a>
                      </li>
                      <li>
                        <a href="http://www.camara.gov.br/cota-parlamentar/ANEXO_ATO_DA_MESA_43_2009.pdf" target="_blank" rel="nofollow noopener noreferrer" className="text-muted-foreground hover:text-primary transition-colors hover:translate-x-1 inline-flex items-center gap-1 duration-200 block">
                          Limites mensais por parlamentar <ExternalLink className="h-3 w-3 opacity-50" />
                        </a>
                      </li>
                      <li>
                        <a href="http://www.camara.gov.br/cota-parlamentar" target="_blank" rel="nofollow noopener noreferrer" className="text-muted-foreground hover:text-primary transition-colors hover:translate-x-1 inline-flex items-center gap-1 duration-200 block">
                          Explorar a cota parlamentar <ExternalLink className="h-3 w-3 opacity-50" />
                        </a>
                      </li>
                    </ul>
                  </div>

                  <div>
                    <h4 className="font-semibold text-foreground flex items-center gap-2 mb-3">
                      <span className="w-1.5 h-1.5 rounded-full bg-accent/50"></span> Senado Federal
                    </h4>
                    <ul className="space-y-2 pl-4 border-l-2 border-muted">
                      <li>
                        <a href="https://adm.senado.leg.br/normas/ui/pub/normaConsultada;jsessionid=FF712C2488692EF4C9A4517976A6F3FE.tomcat-1?0&idNorma=14380751" target="_blank" rel="nofollow noopener noreferrer" className="text-muted-foreground hover:text-primary transition-colors hover:translate-x-1 inline-flex items-center gap-1 duration-200 block">
                          Regras para uso da cota <ExternalLink className="h-3 w-3 opacity-50" />
                        </a>
                      </li>
                      <li>
                        <a href="https://www12.senado.leg.br/transparencia/leg/legislacao-relacionada" target="_blank" rel="nofollow noopener noreferrer" className="text-muted-foreground hover:text-primary transition-colors hover:translate-x-1 inline-flex items-center gap-1 duration-200 block">
                          Legislação relacionada <ExternalLink className="h-3 w-3 opacity-50" />
                        </a>
                      </li>
                      <li>
                        <a href="http://www25.senado.leg.br/web/transparencia/sen" target="_blank" rel="nofollow noopener noreferrer" className="text-muted-foreground hover:text-primary transition-colors hover:translate-x-1 inline-flex items-center gap-1 duration-200 block">
                          Explorar a cota no senado <ExternalLink className="h-3 w-3 opacity-50" />
                        </a>
                      </li>
                    </ul>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm md:col-span-2 hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
              <CardHeader className="pb-2">
                <div className="flex items-center gap-4">
                  <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                    <CheckCheck className="w-6 h-6 text-primary" />
                  </div>
                  <CardTitle className="text-xl">Transparência</CardTitle>
                </div>
              </CardHeader>
              <CardContent className="space-y-4 text-sm text-muted-foreground leading-relaxed pt-2">
                <p>
                  A OPS preza pela total transparência em suas ações. Todos os dados utilizados são
                  provenientes de fontes oficiais e públicas:
                </p>

                <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3 pt-2">
                  <div className="space-y-2">
                    <h4 className="font-medium text-foreground text-sm uppercase tracking-wider text-xs bg-muted/50 p-2 rounded-md">Câmara de Deputados</h4>
                    <ul className="space-y-1 pl-1">
                      <li>
                        <a href="https://www.camara.leg.br/transparencia/gastos-parlamentares" target="_blank" rel="nofollow noopener noreferrer" className="text-muted-foreground hover:text-primary transition-colors hover:translate-x-1 inline-flex items-center gap-1 duration-200 py-1 block">
                          Cota parlamentar <ExternalLink className="h-3 w-3 opacity-50" />
                        </a>
                      </li>
                      <li>
                        <a href="https://dadosabertos.camara.leg.br/" target="_blank" rel="nofollow noopener noreferrer" className="text-muted-foreground hover:text-primary transition-colors hover:translate-x-1 inline-flex items-center gap-1 duration-200 py-1 block">
                          Dados Abertos <ExternalLink className="h-3 w-3 opacity-50" />
                        </a>
                      </li>
                      <li>
                        <a href="https://www.camara.leg.br/deputados/quem-sao" target="_blank" rel="nofollow noopener noreferrer" className="text-muted-foreground hover:text-primary transition-colors hover:translate-x-1 inline-flex items-center gap-1 duration-200 py-1 block">
                          Conheça os Deputados <ExternalLink className="h-3 w-3 opacity-50" />
                        </a>
                      </li>
                    </ul>
                  </div>

                  <div className="space-y-2">
                    <h4 className="font-medium text-foreground text-sm uppercase tracking-wider text-xs bg-muted/50 p-2 rounded-md">Senado Federal</h4>
                    <ul className="space-y-1 pl-1">
                      <li>
                        <a href="https://www12.senado.leg.br/dados-abertos/conjuntos?portal=Administrativo&grupo=senadores" target="_blank" rel="nofollow noopener noreferrer" className="text-muted-foreground hover:text-primary transition-colors hover:translate-x-1 inline-flex items-center gap-1 duration-200 py-1 block">
                          Cota parlamentar (CSV) <ExternalLink className="h-3 w-3 opacity-50" />
                        </a>
                      </li>
                      <li>
                        <a href="https://www25.senado.leg.br/web/senadores/em-exercicio" target="_blank" rel="nofollow noopener noreferrer" className="text-muted-foreground hover:text-primary transition-colors hover:translate-x-1 inline-flex items-center gap-1 duration-200 py-1 block">
                          Conheça os Senadores <ExternalLink className="h-3 w-3 opacity-50" />
                        </a>
                      </li>
                    </ul>
                  </div>

                  <div className="space-y-2">
                    <h4 className="font-medium text-foreground text-sm uppercase tracking-wider text-xs bg-muted/50 p-2 rounded-md">Outros</h4>
                    <ul className="space-y-1 pl-1">
                      <li>
                        <a href="http://www.receita.fazenda.gov.br/pessoajuridica/cnpj/cnpjreva/cnpjreva_solicitacao.asp" target="_blank" rel="nofollow noopener noreferrer" className="text-muted-foreground hover:text-primary transition-colors hover:translate-x-1 inline-flex items-center gap-1 duration-200 py-1 block">
                          Receita Federal <ExternalLink className="h-3 w-3 opacity-50" />
                        </a>
                      </li>
                      <li>
                        <a href="https://minhareceita.org" target="_blank" rel="nofollow noopener noreferrer" className="text-muted-foreground hover:text-primary transition-colors hover:translate-x-1 inline-flex items-center gap-1 duration-200 py-1 block">
                          Minha Receita <ExternalLink className="h-3 w-3 opacity-50" />
                        </a>
                      </li>
                    </ul>
                  </div>
                </div>

                <div className="mt-6 pt-6 border-t border-border/50">
                  <h4 className="font-semibold text-foreground mb-4 flex items-center gap-2">
                    <span className="w-1.5 h-1.5 rounded-full bg-primary/50"></span> Assembleias Legislativas
                  </h4>
                  {(() => {
                    if (loading) {
                      return <div className="mt-2 text-muted-foreground flex items-center gap-2 animate-pulse"><div className="w-4 h-4 rounded-full bg-muted"></div> Carregando dados das assembleias legislativas...</div>;
                    }

                    if (importacaoData.length > 0) {
                      return (
                        <div className="mt-2 overflow-hidden rounded-lg border border-border/50 bg-background/50">
                          <div className="overflow-x-auto">
                            <table className="w-full text-sm">
                              <thead>
                                <tr className="border-b border-border/50 bg-muted/20">
                                  <th className="text-left py-3 px-4 font-medium text-foreground">Assembleia</th>
                                  <th className="text-left py-3 px-4 font-medium text-foreground">Última Despesa</th>
                                  <th className="text-left py-3 px-4 font-medium text-foreground">Última Importação</th>
                                </tr>
                              </thead>
                              <tbody>
                                {importacaoData.map((item) => (
                                  <tr key={item.id} className="border-b border-border/50 hover:bg-muted/30 transition-colors last:border-0">
                                    <td className="py-3 px-4">
                                      <a
                                        href={item.url}
                                        target="_blank"
                                        rel="nofollow noopener noreferrer"
                                        className="text-primary hover:text-accent font-medium hover:underline inline-flex items-center gap-1 transition-colors"
                                      >
                                        {item.nome} {item.sigla && <span className="text-muted-foreground text-xs font-normal">({item.sigla})</span>} <ExternalLink className="h-3 w-3 opacity-50" />
                                      </a>
                                      {item.info && <div className="text-muted-foreground text-xs mt-1 bg-yellow-500/10 text-yellow-600 dark:text-yellow-400 p-1 rounded inline-block">{item.info}</div>}
                                    </td>
                                    <td className="py-3 px-4 text-muted-foreground tabular-nums">
                                      {item.ultima_despesa || '-'}
                                    </td>
                                    <td className="py-3 px-4 text-muted-foreground tabular-nums">
                                      {item.ultima_importacao || '-'}
                                    </td>
                                  </tr>
                                ))}
                              </tbody>
                            </table>
                          </div>
                        </div>
                      );
                    }

                    return <div className="mt-2 text-muted-foreground">Nenhum dado disponível no momento.</div>;
                  })()}
                </div>
              </CardContent>
            </Card>
          </div>
        </div>

        {/* Contact Section */}
        <div className="mt-8">
          <Card className="shadow-lg border-0 bg-primary/5 border-primary/10 backdrop-blur-sm overflow-hidden">
            <CardHeader className="pb-2">
              <CardTitle className="text-xl flex items-center gap-2">
                <span className="p-2 bg-primary/10 rounded-lg text-primary"><Users className="w-5 h-5" /></span>
                Entre em Contato
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-6 pt-2">
              <p className="text-muted-foreground">
                Dúvidas, sugestões ou denúncias? Entre em contato conosco através dos nossos canais:
              </p>
              <div className="flex flex-wrap gap-4">
                <a href="https://institutoops.org.br/" target="_blank" rel="noopener noreferrer"
                  className="flex items-center gap-2 px-4 py-2 rounded-lg bg-background shadow-sm border border-border/50 hover:bg-primary/5 hover:border-primary/20 hover:text-primary transition-all duration-300 group">
                  <ExternalLink className="h-4 w-4 text-muted-foreground group-hover:text-primary transition-colors" />
                  <span className="font-medium">Instituto OPS</span>
                </a>
                <a href="mailto:contato@ops.org.br"
                  className="flex items-center gap-2 px-4 py-2 rounded-lg bg-background shadow-sm border border-border/50 hover:bg-primary/5 hover:border-primary/20 hover:text-primary transition-all duration-300 group">
                  <span className="font-medium">contato@ops.org.br</span>
                </a>
                <a href="https://twitter.com/LucioBig" target="_blank" rel="noopener noreferrer"
                  className="flex items-center gap-2 px-4 py-2 rounded-lg bg-background shadow-sm border border-border/50 hover:bg-primary/5 hover:border-primary/20 hover:text-primary transition-all duration-300 group">
                  <span className="text-xs font-bold bg-muted-foreground/10 p-1 rounded group-hover:bg-primary/10 transition-colors">X</span>
                  <span className="font-medium">Twitter</span>
                </a>
                <a href="https://www.facebook.com/institutoops" target="_blank" rel="noopener noreferrer"
                  className="flex items-center gap-2 px-4 py-2 rounded-lg bg-background shadow-sm border border-border/50 hover:bg-primary/5 hover:border-primary/20 hover:text-primary transition-all duration-300 group">
                  <span className="font-bold text-blue-600 group-hover:text-primary transition-colors">f</span>
                  <span className="font-medium">Facebook</span>
                </a>
                <a href="https://www.youtube.com/channel/UCfQ98EX3oOv6IHBdUNMJq8Q" target="_blank" rel="noopener noreferrer"
                  className="flex items-center gap-2 px-4 py-2 rounded-lg bg-background shadow-sm border border-border/50 hover:bg-primary/5 hover:border-primary/20 hover:text-primary transition-all duration-300 group">
                  <Youtube className="h-4 w-4 text-red-600 group-hover:text-primary transition-colors" />
                  <span className="font-medium">YouTube</span>
                </a>
                <a href="https://github.com/ops-org/operacao-politica-supervisionada" target="_blank" rel="noopener noreferrer"
                  className="flex items-center gap-2 px-4 py-2 rounded-lg bg-background shadow-sm border border-border/50 hover:bg-primary/5 hover:border-primary/20 hover:text-primary transition-all duration-300 group">
                  <span className="font-bold">Git</span>
                  <span className="font-medium">Github</span>
                </a>
              </div>
            </CardContent>
          </Card>
        </div>
      </main >
      <Footer />
    </div >
  );
};

export default Sobre;
