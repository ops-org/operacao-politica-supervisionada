import { Header } from "@/components/Header";
import { HeroSection } from "@/components/HeroSection";
import { AnnualSummaryChartWithCard } from "@/components/AnnualSummaryChart";
import { InfoSection } from "@/components/InfoSection";
import { TopSpendersSection } from "@/components/TopSpendersSection";
import { CTASection } from "@/components/CTASection";
import { Footer } from "@/components/Footer";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { fetchCamaraResumoAnual, fetchSenadoResumoAnual, AnnualSummary } from "@/lib/api";
import { useEffect, useState } from "react";
import { TrendingUp, Users, BarChart3 } from "lucide-react";

const transformApiData = (data: AnnualSummary) => {
  return data.categories.map((year, index) => ({
    year: year.toString(),
    value: Math.round(data.series[index])
  }));
};

const Index = () => {
  const [camaraData, setCamaraData] = useState<{ year: string, value: number }[]>([]);
  const [senadoData, setSenadoData] = useState<{ year: string, value: number }[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [camaraResponse, senadoResponse] = await Promise.all([
          fetchCamaraResumoAnual(),
          fetchSenadoResumoAnual()
        ]);

        setCamaraData(transformApiData(camaraResponse));
        setSenadoData(transformApiData(senadoResponse));
      } catch (error) {
        console.error('Error fetching annual summary data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  if (loading) {
    return (
      <div className="min-h-screen bg-background">
        <Header />
        <LoadingOverlay isLoading={loading} content="Carregando dados da plataforma..." />
      </div>
    );
  }
  return (
    <div className="min-h-screen bg-gradient-to-b from-background to-muted/20">
      <Header />
      <main>
        <HeroSection />

        <section className="container mx-auto px-4 py-8">
          <div className="space-y-8">
            <div className="text-center space-y-4">
              {/* <div className="flex items-center justify-center gap-2 mb-4">
                <div className="h-1 w-8 bg-primary rounded"></div>
                <Badge variant="secondary" className="text-xs font-medium">
                  <BarChart3 className="w-3 h-3 mr-1" />
                  Análise de Dados
                </Badge>
                <div className="h-1 w-8 bg-primary rounded"></div>
              </div> */}
              <h2 className="text-3xl md:text-4xl font-bold text-foreground">
                Resumo Anual da Cota Parlamentar
              </h2>
              {/* <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
                Acompanhe a evolução dos gastos ao longo dos anos e entenda as tendências 
                do uso da cota parlamentar na Câmara dos Deputados e Senado Federal.
              </p> */}
            </div>

            <div className="grid gap-8 lg:grid-cols-2">
              <Card className="border-0 shadow-lg overflow-hidden hover:shadow-xl transition-shadow duration-300">
                <CardContent className="p-0">
                  <AnnualSummaryChartWithCard
                    title="Câmara dos Deputados"
                    subtitle="513 deputados"
                    data={camaraData}
                  />
                </CardContent>
              </Card>
              <Card className="border-0 shadow-lg overflow-hidden hover:shadow-xl transition-shadow duration-300">
                <CardContent className="p-0">
                  <AnnualSummaryChartWithCard
                    title="Senado Federal"
                    subtitle="81 senadores"
                    data={senadoData}
                  />
                </CardContent>
              </Card>
            </div>

            {/* <div className="grid gap-6 md:grid-cols-3 mt-12">
              <Card className="border-0 shadow-md bg-gradient-to-br from-primary/5 to-primary/10">
                <CardContent className="p-6 text-center">
                  <div className="flex justify-center mb-4">
                    <div className="p-3 bg-primary/10 rounded-full">
                      <TrendingUp className="w-6 h-6 text-primary" />
                    </div>
                  </div>
                  <h3 className="font-semibold text-foreground mb-2">Análise Temporal</h3>
                  <p className="text-sm text-muted-foreground">
                    Visualize a evolução dos gastos ao longo de uma década
                  </p>
                </CardContent>
              </Card>
              <Card className="border-0 shadow-md bg-gradient-to-br from-blue-500/5 to-blue-500/10">
                <CardContent className="p-6 text-center">
                  <div className="flex justify-center mb-4">
                    <div className="p-3 bg-blue-500/10 rounded-full">
                      <Users className="w-6 h-6 text-blue-600" />
                    </div>
                  </div>
                  <h3 className="font-semibold text-foreground mb-2">Dados Completos</h3>
                  <p className="text-sm text-muted-foreground">
                    Informações detalhadas sobre todos os parlamentares
                  </p>
                </CardContent>
              </Card>
              <Card className="border-0 shadow-md bg-gradient-to-br from-green-500/5 to-green-500/10">
                <CardContent className="p-6 text-center">
                  <div className="flex justify-center mb-4">
                    <div className="p-3 bg-green-500/10 rounded-full">
                      <BarChart3 className="w-6 h-6 text-green-600" />
                    </div>
                  </div>
                  <h3 className="font-semibold text-foreground mb-2">Transparência</h3>
                  <p className="text-sm text-muted-foreground">
                    Acesso livre a todos os dados oficiais e auditorias
                  </p>
                </CardContent>
              </Card>
            </div> */}
          </div>
        </section>

        <InfoSection />
        <TopSpendersSection />
        {/* <CTASection /> */}
      </main>
      <Footer />
    </div>
  );
};

export default Index;
