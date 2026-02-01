import { Header } from "@/components/Header";
import { AnnualSummaryChartWithCard } from "@/components/AnnualSummaryChart";
import { TopSpendersSection } from "@/components/TopSpendersSection";
import { Footer } from "@/components/Footer";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { Card, CardContent } from "@/components/ui/card";
import { fetchCamaraResumoAnual, fetchSenadoResumoAnual, AnnualSummary } from "@/lib/api";
import { useEffect, useState } from "react";
import { TrendingUp, Users, BarChart3 } from "lucide-react";
import { usePageTitle } from "@/hooks/usePageTitle";

const transformApiData = (data: AnnualSummary) => {
  return data.categories.map((year, index) => ({
    year: year.toString(),
    value: Math.round(data.series[index])
  }));
};

const Index = () => {
  usePageTitle("Início");
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
    <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
      <Header />
      <main className="container mx-auto px-4 py-8">
        {/* Hero Section */}
        <div className="text-center mt-4 mb-8">
          <h1 className="text-4xl font-bold bg-gradient-to-r from-primary via-accent to-primary bg-clip-text text-transparent mb-4">
            Resumo Anual da Cota Parlamentar
          </h1>
          <p className="text-lg text-muted-foreground mx-auto max-w-2xl">
            Acompanhe a evolução dos gastos ao longo dos anos e entenda as tendências
            do uso da cota parlamentar na Câmara dos Deputados e Senado Federal.
          </p>
        </div>

        {/* Charts Section */}
        <div className="space-y-8">
          <div className="grid gap-8 lg:grid-cols-2">
            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300 group">
              <CardContent className="p-0 relative z-10">
                <AnnualSummaryChartWithCard
                  title="Câmara dos Deputados"
                  subtitle="513 deputados"
                  data={camaraData}
                />
              </CardContent>
            </Card>
            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300 group">
              <CardContent className="p-0 relative z-10">
                <AnnualSummaryChartWithCard
                  title="Senado Federal"
                  subtitle="81 senadores"
                  data={senadoData}
                />
              </CardContent>
            </Card>
          </div>
        </div>

        {/* Stats Section */}
        <div className="grid gap-8 md:grid-cols-3 mt-6 hidden lg:grid">
          <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-2xl hover:-translate-y-2 transition-all duration-500 group overflow-hidden border-t-4 border-t-blue-500/20 hover:border-t-blue-500">
            <div className="absolute top-0 right-0 w-32 h-32 bg-blue-500/5 rounded-full blur-3xl -mr-16 -mt-16 group-hover:bg-blue-500/10 transition-colors" />
            <CardContent className="p-8 text-center relative z-10">
              <div className="flex justify-center mb-6">
                <div className="p-5 bg-blue-500/10 rounded-3xl shadow-inner border border-blue-500/10 group-hover:scale-110 group-hover:rotate-3 transition-transform duration-500">
                  <TrendingUp className="w-10 h-10 text-blue-600" />
                </div>
              </div>
              <h3 className="text-xl font-black text-foreground mb-3 tracking-tight">Análise Temporal</h3>
              <p className="text-sm text-muted-foreground leading-relaxed">
                Visualize a evolução dos gastos ao longo de uma década com gráficos interativos e precisos.
              </p>
            </CardContent>
          </Card>

          <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-2xl hover:-translate-y-2 transition-all duration-500 group overflow-hidden border-t-4 border-t-emerald-500/20 hover:border-t-emerald-500">
            <div className="absolute top-0 right-0 w-32 h-32 bg-emerald-500/5 rounded-full blur-3xl -mr-16 -mt-16 group-hover:bg-emerald-500/10 transition-colors" />
            <CardContent className="p-8 text-center relative z-10">
              <div className="flex justify-center mb-6">
                <div className="p-5 bg-emerald-500/10 rounded-3xl shadow-inner border border-emerald-500/10 group-hover:scale-110 group-hover:-rotate-3 transition-transform duration-500">
                  <Users className="w-10 h-10 text-emerald-600" />
                </div>
              </div>
              <h3 className="text-xl font-black text-foreground mb-3 tracking-tight">Dados Completos</h3>
              <p className="text-sm text-muted-foreground leading-relaxed">
                Informações detalhadas sobre todos os parlamentares, partidos e estados em um só lugar.
              </p>
            </CardContent>
          </Card>

          <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-2xl hover:-translate-y-2 transition-all duration-500 group overflow-hidden border-t-4 border-t-purple-500/20 hover:border-t-purple-500">
            <div className="absolute top-0 right-0 w-32 h-32 bg-purple-500/5 rounded-full blur-3xl -mr-16 -mt-16 group-hover:bg-purple-500/10 transition-colors" />
            <CardContent className="p-8 text-center relative z-10">
              <div className="flex justify-center mb-6">
                <div className="p-5 bg-purple-500/10 rounded-3xl shadow-inner border border-purple-500/10 group-hover:scale-110 group-hover:rotate-3 transition-transform duration-500">
                  <BarChart3 className="w-10 h-10 text-purple-600" />
                </div>
              </div>
              <h3 className="text-xl font-black text-foreground mb-3 tracking-tight">Transparência</h3>
              <p className="text-sm text-muted-foreground leading-relaxed">
                Acesso livre a todos os dados oficiais e auditorias cidadãs realizadas pela OPS.
              </p>
            </CardContent>
          </Card>
        </div>

        <TopSpendersSection />
      </main>
      <Footer />
    </div>
  );
};

export default Index;
