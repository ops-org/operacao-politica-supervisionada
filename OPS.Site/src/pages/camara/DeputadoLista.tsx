import { useState, useEffect, useCallback, useMemo } from "react";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { MultiSelectDropdown } from "@/components/MultiSelectDropdown";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { Avatar, AvatarImage, AvatarFallback } from "@/components/ui/avatar";
import { Link } from "react-router-dom";
import { fetchEstados, fetchPartidos, DropDownOptions } from "@/lib/api";
import { apiClient } from "@/lib/api";
import { Search, RotateCcw, Users, MapPin, Building2, DollarSign, TrendingUp, User } from "lucide-react";

interface DeputadoLista {
  id_cf_deputado: number;
  nome_parlamentar: string;
  nome_civil: string;
  sigla_partido: string;
  nome_partido: string;
  sigla_estado: string;
  nome_estado: string;
  situacao: string;
  ativo: boolean;
  valor_total_ceap: string;
  valor_total_remuneracao: string;
}

interface FilterState {
  periodo: number;
  estado: string[];
  partido: string[];
}

const legislaturas = [
  { id: 57, text: "57ª (fev/2023 à jan/2027)" },
  { id: 56, text: "56ª (fev/2019 à jan/2023)" },
  { id: 55, text: "55ª (fev/2015 à jan/2019)" },
  { id: 54, text: "54ª (fev/2011 à jan/2015)" },
  { id: 53, text: "53ª (fev/2007 à jan/2011)" },
];

const DeputadoLista = () => {
  const [deputados, setDeputados] = useState<DeputadoLista[]>([]);
  const [estados, setEstados] = useState<DropDownOptions[]>([]);
  const [partidos, setPartidos] = useState<DropDownOptions[]>([]);
  const [searching, setSearching] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filters, setFilters] = useState<FilterState>({
    periodo: 57,
    estado: [],
    partido: [],
  });
  const [visibleImages, setVisibleImages] = useState<Set<number>>(new Set());

  useEffect(() => {
    document.title = "OPS :: Deputado Federal";
    loadInitialData();
  }, []);

  const loadInitialData = async () => {
    try {
      const [estadosData, partidosData] = await Promise.all([
        fetchEstados(),
        fetchPartidos(),
      ]);
      setEstados(estadosData);
      setPartidos(partidosData);

      // Load initial data
      await pesquisar();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro ao carregar dados");
    }
  };

  const handleImageVisibility = useCallback((id: number) => {
    setVisibleImages(prev => new Set(prev).add(id));
  }, []);

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            const id = parseInt(entry.target.getAttribute('data-image-id') || '0');
            handleImageVisibility(id);
            observer.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.1 }
    );

    const imageElements = document.querySelectorAll('[data-image-id]');
    imageElements.forEach((el) => observer.observe(el));

    return () => observer.disconnect();
  }, [deputados, handleImageVisibility]);

  const parseBrazilianCurrency = useCallback((value: string) => {
    // Remove "R$" and spaces
    let cleaned = value.replace(/[R$\s]/g, '');

    // Check if the value contains commas (Brazilian format) or just dots (already in decimal format)
    if (cleaned.includes(',')) {
      // Brazilian format: 1.234.567,89 -> 1234567.89
      // Remove thousand separators (dots), then convert decimal comma to dot
      cleaned = cleaned.replace(/\./g, '').replace(',', '.');
    }
    // If no commas, keep as is (already in decimal format with dots)

    const num = parseFloat(cleaned);
    return isNaN(num) ? 0 : num;
  }, []);

  const formatCurrency = useCallback((value: string) => {
    const num = parseBrazilianCurrency(value);
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(num);
  }, [parseBrazilianCurrency]);

  const stats = useMemo(() => {
    const totalDeputados = deputados.length;
    const ativos = deputados.filter(d => d.ativo).length;
    const totalCeap = deputados.reduce((sum, d) => {
      return sum + parseBrazilianCurrency(d.valor_total_ceap);
    }, 0);
    const totalRemuneracao = deputados.reduce((sum, d) => {
      return sum + parseBrazilianCurrency(d.valor_total_remuneracao);
    }, 0);

    return { totalDeputados, ativos, totalCeap, totalRemuneracao };
  }, [deputados, parseBrazilianCurrency]);

  const pesquisar = async () => {
    setSearching(true);
    setError(null);

    try {
      const filtro = {
        periodo: filters.periodo,
        estado: filters.estado.length > 0 ? filters.estado.join(",") : undefined,
        partido: filters.partido.length > 0 ? filters.partido.join(",") : undefined,
      };

      // Remove undefined values
      Object.keys(filtro).forEach(key => {
        if (filtro[key as keyof typeof filtro] === undefined) {
          delete filtro[key as keyof typeof filtro];
        }
      });

      const response = await apiClient.post<DeputadoLista[]>("/api/deputado/lista", filtro);
      setDeputados(response);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erro ao buscar deputados");
    } finally {
      setSearching(false);
    }
  };

  const limparFiltros = () => {
    setFilters({
      periodo: 57,
      estado: [],
      partido: [],
    });
  };

  {/* Full-screen loading overlay */ }
  <LoadingOverlay isLoading={searching} content="Carregando informações do documento..." />

  if (error) {
    return (
      <div className="min-h-screen flex flex-col">
        <Header />
        <main className="flex-1 container mx-auto px-4 py-8">
          <Alert variant="destructive">
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        </main>
        <Footer />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <Header />
      <main className="flex-1 container mx-auto px-4 py-8">
        <div className="space-y-8">
          {/* Modern Header */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent mb-2">
                Deputados Federais
              </h1>
              <p className="text-muted-foreground text-lg leading-relaxed">
                Resumo dos deputados federais, seus respectivos gastos com a cota parlamentar (CEAP) e com Folha de pagamento (Própria e de seus secretários).
                <br />
                <span className="text-sm font-bold">* Valores acumulados desde 2008 de todas as legislaturas.</span>
              </p>
            </div>
          </div>

          {/* Stats Cards */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 overflow-hidden group">
              <div className="absolute inset-0 bg-gradient-to-br from-blue-500/5 to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
              <CardContent className="p-6 relative">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-bold text-blue-600/80 uppercase tracking-wider mb-1">Total de Deputados</p>
                    <p className="text-3xl font-black text-blue-900 font-mono tracking-tighter">{stats.totalDeputados}</p>
                  </div>
                  <div className="p-4 bg-gradient-to-br from-blue-100 to-blue-50 rounded-2xl shadow-inner border border-blue-200/50">
                    <Users className="h-7 w-7 text-blue-600" />
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 overflow-hidden group">
              <div className="absolute inset-0 bg-gradient-to-br from-green-500/5 to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
              <CardContent className="p-6 relative">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-bold text-green-600/80 uppercase tracking-wider mb-1">Deputados Ativos</p>
                    <p className="text-3xl font-black text-green-900 font-mono tracking-tighter">{stats.ativos}</p>
                  </div>
                  <div className="p-4 bg-gradient-to-br from-green-100 to-green-50 rounded-2xl shadow-inner border border-green-200/50">
                    <TrendingUp className="h-7 w-7 text-green-600" />
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 overflow-hidden group">
              <div className="absolute inset-0 bg-gradient-to-br from-purple-500/5 to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
              <CardContent className="p-6 relative">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-bold text-purple-600/80 uppercase tracking-wider mb-1">Cota Parlamentar</p>
                    <p className="text-2xl font-black text-purple-900 font-mono tracking-tighter">{formatCurrency(stats.totalCeap.toString())}</p>
                  </div>
                  <div className="p-4 bg-gradient-to-br from-purple-100 to-purple-50 rounded-2xl shadow-inner border border-purple-200/50">
                    <DollarSign className="h-7 w-7 text-purple-600" />
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 overflow-hidden group">
              <div className="absolute inset-0 bg-gradient-to-br from-orange-500/5 to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
              <CardContent className="p-6 relative">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-bold text-orange-600/80 uppercase tracking-wider mb-1">Verba de Gabinete</p>
                    <p className="text-2xl font-black text-orange-900 font-mono tracking-tighter">{formatCurrency(stats.totalRemuneracao.toString())}</p>
                  </div>
                  <div className="p-4 bg-gradient-to-br from-orange-100 to-orange-50 rounded-2xl shadow-inner border border-orange-200/50">
                    <Building2 className="h-7 w-7 text-orange-600" />
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Modern Filters */}
          <Card className="shadow-xl border-0 bg-card/80 backdrop-blur-sm overflow-hidden">
            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
              <div className="flex items-center gap-4">
                <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                  <Search className="h-6 w-6 text-primary" />
                </div>
                <div>
                  <CardTitle className="text-xl">Filtros de Busca</CardTitle>
                  <CardDescription className="font-medium">Refine a lista por legislatura, estado ou partido</CardDescription>
                </div>
              </div>
            </CardHeader>
            <CardContent className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="space-y-3">
                  <label className="text-sm font-bold text-foreground flex items-center gap-2">
                    <Building2 className="h-4 w-4 text-primary" />
                    Legislatura
                  </label>
                  <Select
                    value={filters.periodo.toString()}
                    onValueChange={(value) => setFilters(prev => ({ ...prev, periodo: parseInt(value) }))}
                  >
                    <SelectTrigger className="h-11 bg-background/50 border-muted">
                      <SelectValue placeholder="Selecione a legislatura" />
                    </SelectTrigger>
                    <SelectContent>
                      {legislaturas.map((leg) => (
                        <SelectItem key={leg.id} value={leg.id.toString()}>
                          {leg.text}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-3">
                  <label className="text-sm font-bold text-foreground flex items-center gap-2">
                    <MapPin className="h-4 w-4 text-primary" />
                    Estado
                  </label>
                  <MultiSelectDropdown
                    items={estados}
                    placeholder="Selecione estados"
                    selectedItems={filters.estado}
                    onSelectionChange={(items) => setFilters(prev => ({ ...prev, estado: items }))}
                  />
                </div>

                <div className="space-y-3">
                  <label className="text-sm font-bold text-foreground flex items-center gap-2">
                    <Users className="h-4 w-4 text-primary" />
                    Partido
                  </label>
                  <MultiSelectDropdown
                    items={partidos}
                    placeholder="Selecione partidos"
                    selectedItems={filters.partido}
                    onSelectionChange={(items) => setFilters(prev => ({ ...prev, partido: items }))}
                  />
                </div>
              </div>

              <div className="flex flex-col sm:flex-row gap-3 mt-8 pt-6 border-t border-muted/50">
                <Button
                  onClick={pesquisar}
                  disabled={searching}
                  size="lg"
                  className="flex-1 bg-primary hover:bg-primary/90 text-white shadow-lg shadow-primary/20 transition-all duration-300 hover:scale-[1.02]"
                >
                  <Search className="h-5 w-5 mr-2" />
                  {searching ? "Pesquisando..." : "Pesquisar agora"}
                </Button>
                <Button
                  variant="outline"
                  onClick={limparFiltros}
                  size="lg"
                  className="flex-1 sm:flex-none border-muted hover:bg-accent transition-colors"
                >
                  <RotateCcw className="h-5 w-5 mr-2" />
                  Limpar todos
                </Button>
              </div>
            </CardContent>
          </Card>

          {/* Modern Deputados Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {deputados.map((deputado) => (
              <Link
                key={deputado.id_cf_deputado}
                to={`/deputado-federal/${deputado.id_cf_deputado}`}
                title="Clique para visualizar o perfil do parlamentar"
                className="block"
              >
                <Card
                  className="group hover:shadow-2xl transition-all duration-500 hover:-translate-y-2 border-0 bg-card/80 backdrop-blur-sm shadow-lg overflow-hidden cursor-pointer flex flex-col h-full"
                >
                  {/* Card Header with Gradient Overlay */}
                  <div className="relative h-24 overflow-hidden">
                    <div className={`absolute inset-0 z-0 ${deputado.ativo
                      ? "bg-gradient-to-br from-primary via-primary/80 to-accent"
                      : "bg-gradient-to-r from-slate-500/20 to-slate-500/5"
                      }`} />

                    {/* Animated geometric shapes for premium feel */}
                    <div className="absolute top-[-20%] right-[-10%] w-32 h-32 bg-white/10 rounded-full blur-2xl group-hover:bg-white/20 transition-colors duration-500" />
                    <div className="absolute bottom-[-50%] left-[-10%] w-40 h-40 bg-black/10 rounded-full blur-2xl" />

                    <div className="relative z-10 p-5 h-full flex flex-col justify-between">
                      <div className="flex justify-between items-start gap-2">
                        <Badge className={`${deputado.ativo ? "bg-white/20 text-white border-white/30" : "bg-slate-200/50 text-slate-700 border-slate-300/50"} px-2 py-0.5 text-[10px] font-black uppercase tracking-widest backdrop-blur-md`}>
                          {deputado.ativo ? "Ativo" : "Inativo"}
                        </Badge>
                        <div className={`h-2 w-2 rounded-full shadow-[0_0_8px_rgba(255,255,255,0.5)] ${deputado.ativo ? "bg-green-400 animate-pulse" : "bg-slate-300"}`} />
                      </div>
                      <div className="truncate">
                        <h3 className={`font-black text-lg leading-tight truncate group-hover:translate-x-1 transition-transform duration-300 ${deputado.ativo ? "text-white" : "text-slate-700"}`}>
                          {deputado.nome_parlamentar}
                        </h3>
                      </div>
                    </div>
                  </div>

                  {/* Card Body */}
                  <CardContent className="p-0 relative flex-1 flex flex-col">
                    <div className="px-5 py-4 space-y-4 flex-1">
                      <div className="flex gap-4 items-start">
                        {/* Avatar Column */}
                        <div className="flex-shrink-0 -mt-10 relative z-20">
                          <div data-image-id={deputado.id_cf_deputado}>
                            {visibleImages.has(deputado.id_cf_deputado) && (
                              <Avatar className={`h-28 w-20 rounded-xl border-4 border-card shadow-xl group-hover:scale-110 transition-all duration-500 ring-1 ring-black/5 ${!deputado.ativo ? "grayscale opacity-80" : ""}`}>
                                <AvatarImage
                                  src={`//static.ops.org.br/depfederal/${deputado.id_cf_deputado}.jpg`}
                                  alt={deputado.nome_parlamentar}
                                />
                                <AvatarFallback className="rounded-xl text-xl font-black bg-muted text-muted-foreground">
                                  {deputado.nome_parlamentar.split(" ").map(n => n[0]).join("")}
                                </AvatarFallback>
                              </Avatar>
                            )}
                            {!visibleImages.has(deputado.id_cf_deputado) && (
                              <div className="h-28 w-20 rounded-xl bg-muted animate-pulse" />
                            )}
                          </div>
                        </div>

                        {/* Text Group */}
                        <div className="flex-1 min-w-0 pt-1">
                          <p className="text-xs font-bold text-muted-foreground truncate uppercase tracking-tight mb-3" title={deputado.nome_civil}>
                            {deputado.nome_civil}
                          </p>

                          <div className="flex flex-wrap gap-2">
                            <Badge variant="secondary" className="font-black text-[10px] px-2.5 py-0.5 bg-primary/10 text-primary border-primary/20" title={deputado.nome_partido}>
                              {deputado.sigla_partido}
                            </Badge>
                            <Badge variant="outline" className="font-black text-[10px] px-2.5 py-0.5 flex items-center gap-1 border-muted-foreground/30 bg-muted/30" title={deputado.nome_estado}>
                              <MapPin className="w-2.5 h-2.5" />
                              {deputado.sigla_estado}
                            </Badge>
                          </div>
                        </div>
                      </div>

                      {/* Financial Info Grid - Premium Style */}
                      <div className="grid grid-cols-1 gap-3 pt-2">
                        <div className="p-3 rounded-xl bg-gradient-to-br from-purple-500/5 to-purple-500/10 border border-purple-500/10 hover:border-purple-500/30 transition-colors">
                          <div className="flex items-center justify-between mb-1">
                            <span className="text-[10px] font-black uppercase tracking-wider text-purple-600/80">Cota Parlamentar</span>
                            <DollarSign className="h-3 w-3 text-purple-500" />
                          </div>
                          <p className="font-black text-sm text-purple-900 font-mono">
                            {formatCurrency(deputado.valor_total_ceap)}
                          </p>
                        </div>

                        <div className="p-3 rounded-xl bg-gradient-to-br from-orange-500/5 to-orange-500/10 border border-orange-500/10 hover:border-orange-500/30 transition-colors">
                          <div className="flex items-center justify-between mb-1">
                            <span className="text-[10px] font-black uppercase tracking-wider text-orange-600/80">Verba de Gabinete</span>
                            <Building2 className="h-3 w-3 text-orange-500" />
                          </div>
                          <p className="font-black text-sm text-orange-900 font-mono">
                            {formatCurrency(deputado.valor_total_remuneracao)}
                          </p>
                        </div>
                      </div>
                    </div>

                    {/* Hover Footer Action */}
                    <div className="px-5 py-3 bg-muted/30 border-t border-muted/50 flex items-center justify-center text-[10px] font-black uppercase tracking-widest text-primary opacity-0 group-hover:opacity-100 transition-all duration-300">
                      Ver Perfil Completo
                    </div>
                  </CardContent>
                </Card>
              </Link>
            ))}
          </div>

          {/* Modern No Results */}
          {deputados.length === 0 && !searching && (
            <div className="text-center py-16">
              <div className="max-w-md mx-auto">
                <User className="h-16 w-16 text-gray-400 mx-auto mb-4" />
                <h3 className="text-xl font-semibold text-gray-600 mb-2">
                  Nenhum deputado encontrado
                </h3>
                <p className="text-muted-foreground">
                  Tente ajustar os filtros de busca para encontrar os deputados desejados.
                </p>
              </div>
            </div>
          )}
        </div>
      </main>
      <Footer />
    </div>
  );
};

export default DeputadoLista;
