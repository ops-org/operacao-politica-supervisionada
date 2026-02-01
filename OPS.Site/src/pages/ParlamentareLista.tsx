import { useState, useEffect, useCallback, useMemo } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
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

// Unified interfaces
interface ParlamentarBase {
  id_cf_deputado?: number;
  id_sf_senador?: number;
  id_cl_deputado?: number;
  nome_parlamentar: string;
  nome_civil: string;
  sigla_partido: string;
  nome_partido: string;
  sigla_estado: string;
  nome_estado: string;
  situacao: string;
  ativo: boolean;
  valor_total_ceap?: string;
  valor_total_ceaps?: string;
  valor_total_remuneracao: string;
}

interface FilterState {
  periodo: number;
  estado: string[];
  partido: string[];
}

type ParlamentarType = "deputado-federal" | "deputado-estadual" | "senador";

// Type configurations following CotaParlamentar pattern
const typeConfigs = {
  "deputado-federal": {
    title: "Deputados Federais",
    subtitle: "Resumo dos deputados federais, seus respectivos gastos com a cota parlamentar (CEAP) e com Verba de Gabinete.",
    apiEndpoint: "/api/deputado/lista",
    detailRoute: "/deputado-federal",
    imageBaseUrl: "//static.ops.org.br/depfederal",
    idField: "id_cf_deputado" as keyof ParlamentarBase,
    ceapField: "valor_total_ceap" as keyof ParlamentarBase,
    remunerationLabel: "Verba de Gabinete",
    documentTitle: "Deputado Federal",
    noResultsMessage: "Nenhum deputado encontrado",
    noResultsHint: "Tente ajustar os filtros de busca para encontrar os deputados desejados.",
    statsLabel: "Total de Deputados",
    activeLabel: "Deputados Ativos"
  },
  "senador": {
    title: "Senadores",
    subtitle: "Lista dos senadores, seus respectivos gastos com a cota parlamentar (CEAPS) e com Folha de pagamento (Própria e de seus secretários).",
    apiEndpoint: "/senador/lista",
    detailRoute: "/senador",
    imageBaseUrl: "//static.ops.org.br/senador",
    idField: "id_sf_senador" as keyof ParlamentarBase,
    ceapField: "valor_total_ceaps" as keyof ParlamentarBase,
    remunerationLabel: "Folha de pagamento",
    documentTitle: "Senador",
    noResultsMessage: "Nenhum senador encontrado",
    noResultsHint: "Tente ajustar os filtros de busca para encontrar os senadores desejados.",
    statsLabel: "Total de Senadores",
    activeLabel: "Senadores Ativos"
  },
  "deputado-estadual": {
    title: "Deputados Estaduais",
    subtitle: "Resumo dos deputados estaduais, seus respectivos gastos com a cota parlamentar (CEAP) e com Verba de Gabinete.",
    apiEndpoint: "/api/deputadoestadual/lista",
    detailRoute: "/deputado-estadual",
    imageBaseUrl: "//static.ops.org.br/depestadual",
    idField: "id_cl_deputado" as keyof ParlamentarBase,
    ceapField: "valor_total_ceap" as keyof ParlamentarBase,
    remunerationLabel: "Verba de Gabinete",
    documentTitle: "Deputado Estadual",
    noResultsMessage: "Nenhum deputado estadual encontrado",
    noResultsHint: "Tente ajustar os filtros de busca para encontrar os deputados estaduais desejados.",
    statsLabel: "Total de Deputados Estaduais",
    activeLabel: "Deputados Estaduais Ativos"
  }
} as const;

const legislaturas = [
  { id: 57, text: "57ª (fev/2023 à jan/2027)" },
  { id: 56, text: "56ª (fev/2019 à jan/2023)" },
  { id: 55, text: "55ª (fev/2015 à jan/2019)" },
  { id: 54, text: "54ª (fev/2011 à jan/2015)" },
  { id: 53, text: "53ª (fev/2007 à jan/2011)" },
];

const ParlamentareLista = ({ type }: { type?: ParlamentarType }) => {
  const [parlamentarType, setParlamentarType] = useState<ParlamentarType>(type);
  const [parlamentares, setParlamentares] = useState<ParlamentarBase[]>([]);
  const [estados, setEstados] = useState<DropDownOptions[]>([]);
  const [partidos, setPartidos] = useState<DropDownOptions[]>([]);
  const [searching, setSearching] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filters, setFilters] = useState<FilterState>({
    periodo: 57,
    estado: [],
    partido: [],
  });
  const [visibleImages, setVisibleImages] = useState<Set<string>>(new Set());

  const config = typeConfigs[parlamentarType];

  usePageTitle(config.documentTitle);

  useEffect(() => {
    loadInitialData();
  }, [parlamentarType]);

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

  const handleImageVisibility = useCallback((key: string) => {
    setVisibleImages(prev => new Set(prev).add(key));
  }, []);

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            const key = entry.target.getAttribute('data-image-id') || '0';
            handleImageVisibility(key);
            observer.unobserve(entry.target);
          }
        });
      },
      { threshold: 0.1 }
    );

    const imageElements = document.querySelectorAll('[data-image-id]');
    imageElements.forEach((el) => observer.observe(el));

    return () => observer.disconnect();
  }, [parlamentares, handleImageVisibility]);

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
    const totalParlamentares = parlamentares.length;
    const ativos = parlamentares.filter(d => d.ativo).length;
    const totalCeap = parlamentares.reduce((sum, d) => {
      const ceapValue = d[config.ceapField] as string;
      return sum + parseBrazilianCurrency(ceapValue || "0");
    }, 0);
    const totalRemuneracao = parlamentares.reduce((sum, d) => {
      return sum + parseBrazilianCurrency(d.valor_total_remuneracao);
    }, 0);

    return { totalParlamentares, ativos, totalCeap, totalRemuneracao };
  }, [parlamentares, parseBrazilianCurrency, config.ceapField]);

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

      const response = await apiClient.post<ParlamentarBase[]>(config.apiEndpoint, filtro);
      setParlamentares(response);
    } catch (err) {
      const errorMessage = parlamentarType === "deputado-federal"
        ? "deputados"
        : parlamentarType === "deputado-estadual"
          ? "deputados estaduais"
          : "senadores";
      setError(err instanceof Error ? err.message : `Erro ao buscar ${errorMessage}`);
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

  const getParlamentarId = (parlamentar: ParlamentarBase): string => {
    const id = parlamentar[config.idField];
    return id ? id.toString() : "";
  };

  const getParlamentarImageKey = (parlamentar: ParlamentarBase, index?: number): string => {
    const id = getParlamentarId(parlamentar);
    return parlamentarType === "senador" && index !== undefined ? `${id}-${index}` : id;
  };

  {/* Full-screen loading overlay */ }
  <LoadingOverlay isLoading={searching} content="Carregando informações do documento..." />

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
        <Header />
        <main className="container mx-auto px-4 py-8">
          {/* Hero Section */}
          <div className="text-center mb-12 pt-8">
            <div className="inline-flex items-center justify-center w-20 h-20 bg-gradient-to-br from-primary to-accent rounded-full mb-6 shadow-lg shadow-primary/20">
              <Users className="w-10 h-10 text-white" />
            </div>
            <h1 className="text-4xl font-bold bg-gradient-to-r from-primary via-accent to-primary bg-clip-text text-transparent mb-4">
              {config.title}
            </h1>
            <p className="text-lg text-muted-foreground mx-auto max-w-2xl">
              {config.subtitle}
            </p>
          </div>
          <Alert variant="destructive" className="max-w-2xl mx-auto border-red-200 bg-red-50 dark:bg-red-900/10">
            <AlertDescription className="text-center text-lg">{error}</AlertDescription>
          </Alert>
        </main>
        <Footer />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
      <Header />
      <main className="container mx-auto px-4 py-8">
        <div className="space-y-8">
          {/* Header with Type Selector */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent mb-2">{config.title}</h1>
              <p className="text-muted-foreground text-lg">{config.subtitle}</p>
              <p className="text-sm text-muted-foreground mt-1">* Valores acumulados desde 2008 de todas as legislaturas</p>
            </div>

            <div className="flex flex-col sm:flex-row items-center gap-3">
              {/* Type Selector */}
              <div className="inline-flex rounded-lg border p-1 bg-card shadow-sm">
                <Button
                  variant={parlamentarType === "deputado-federal" ? "default" : "ghost"}
                  size="sm"
                  onClick={() => setParlamentarType("deputado-federal")}
                  className="px-4 py-2"
                >
                  Deputados Federais
                </Button>
                <Button
                  variant={parlamentarType === "senador" ? "default" : "ghost"}
                  size="sm"
                  onClick={() => setParlamentarType("senador")}
                  className="px-4 py-2"
                >
                  Senadores
                </Button>
              </div>
            </div>
          </div>

          {/* Stats Cards */}
          {parlamentares.length > 0 && (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
              <Card className="shadow-lg border-0 bg-blue-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
                <CardContent className="p-6">
                  <div className="flex items-center justify-between">
                    <div className="space-y-1">
                      <p className="text-[10px] font-black text-blue-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">{config.statsLabel}</p>
                      <p className="text-2xl font-black text-blue-900 font-mono tracking-tighter">{stats.totalParlamentares}</p>
                    </div>
                    <div className="p-3 bg-blue-500/10 text-blue-600 rounded-2xl shadow-inner border border-blue-500/10 group-hover:scale-110 transition-transform">
                      <Users className="h-6 w-6" />
                    </div>
                  </div>
                </CardContent>
              </Card>

              <Card className="shadow-lg border-0 bg-emerald-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
                <CardContent className="p-6">
                  <div className="flex items-center justify-between">
                    <div className="space-y-1">
                      <p className="text-[10px] font-black text-emerald-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">{config.activeLabel}</p>
                      <p className="text-2xl font-black text-emerald-900 font-mono tracking-tighter">{stats.ativos}</p>
                    </div>
                    <div className="p-3 bg-emerald-500/10 text-emerald-600 rounded-2xl shadow-inner border border-emerald-500/10 group-hover:scale-110 transition-transform">
                      <TrendingUp className="h-6 w-6" />
                    </div>
                  </div>
                </CardContent>
              </Card>

              <Card className="shadow-lg border-0 bg-purple-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
                <CardContent className="p-6">
                  <div className="flex items-center justify-between">
                    <div className="space-y-1">
                      <p className="text-[10px] font-black text-purple-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">Cota Parlamentar</p>
                      <p className="text-xl font-black text-purple-900 font-mono tracking-tighter">{formatCurrency(stats.totalCeap.toString())}</p>
                    </div>
                    <div className="p-3 bg-purple-500/10 text-purple-600 rounded-2xl shadow-inner border border-purple-500/10 group-hover:scale-110 transition-transform">
                      <DollarSign className="h-6 w-6" />
                    </div>
                  </div>
                </CardContent>
              </Card>

              <Card className="shadow-lg border-0 bg-orange-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
                <CardContent className="p-6">
                  <div className="flex items-center justify-between">
                    <div className="space-y-1">
                      <p className="text-[10px] font-black text-orange-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">{config.remunerationLabel}</p>
                      <p className="text-xl font-black text-orange-900 font-mono tracking-tighter">{formatCurrency(stats.totalRemuneracao.toString())}</p>
                    </div>
                    <div className="p-3 bg-orange-500/10 text-orange-600 rounded-2xl shadow-inner border border-orange-500/10 group-hover:scale-110 transition-transform">
                      <Building2 className="h-6 w-6" />
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
          )}

          {/* Modern Filters */}
          <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden">
            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
              <div className="flex items-center gap-4">
                <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                  <Search className="h-6 w-6 text-primary" />
                </div>
                <div>
                  <CardTitle className="text-xl">Filtros de Busca</CardTitle>
                </div>
              </div>
            </CardHeader>
            <CardContent className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="space-y-2">
                  <label className="text-[10px] font-black uppercase tracking-widest text-muted-foreground flex items-center gap-2">
                    <Building2 className="h-3 w-3" />
                    Legislatura
                  </label>
                  <Select
                    value={filters.periodo.toString()}
                    onValueChange={(value) => setFilters(prev => ({ ...prev, periodo: parseInt(value) }))}
                  >
                    <SelectTrigger className="bg-background/50 border-muted-foreground/20 focus:ring-primary/20">
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

                <div className="space-y-2">
                  <label className="text-[10px] font-black uppercase tracking-widest text-muted-foreground flex items-center gap-2">
                    <MapPin className="h-3 w-3" />
                    Estado
                  </label>
                  <MultiSelectDropdown
                    items={estados}
                    placeholder="Selecione estados"
                    selectedItems={filters.estado}
                    onSelectionChange={(items) => setFilters(prev => ({ ...prev, estado: items }))}
                  />
                </div>

                <div className="space-y-2">
                  <label className="text-[10px] font-black uppercase tracking-widest text-muted-foreground flex items-center gap-2">
                    <Users className="h-3 w-3" />
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

              <div className="flex gap-3 mt-8">
                <Button
                  onClick={pesquisar}
                  disabled={searching}
                  className="bg-primary text-primary-foreground hover:bg-primary/90 shadow-md hover:shadow-lg transition-all duration-300 font-bold uppercase tracking-widest text-[10px] px-8"
                >
                  <Search className="h-4 w-4 mr-2" />
                  {searching ? "Pesquisando..." : "Pesquisar"}
                </Button>
                <Button
                  variant="outline"
                  onClick={limparFiltros}
                  className="border-muted-foreground/20 hover:bg-muted font-bold uppercase tracking-widest text-[10px] px-8"
                >
                  <RotateCcw className="h-4 w-4 mr-2" />
                  Limpar
                </Button>
              </div>
            </CardContent>
          </Card>

          {/* Modern Parlamentares Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {parlamentares.map((parlamentar, index) => (
              <Link
                key={`${getParlamentarId(parlamentar)}-${parlamentar.nome_parlamentar}-${index}`}
                to={`${config.detailRoute}/${getParlamentarId(parlamentar)}`}
                title="Clique para visualizar o perfil do parlamentar"
                className="block"
              >
                <Card
                  className="group hover:shadow-2xl transition-all duration-500 hover:-translate-y-2 border-0 bg-card/80 backdrop-blur-sm shadow-lg overflow-hidden cursor-pointer"
                >
                  {/* Card Header with Status Gradient */}
                  <div className={`relative overflow-hidden h-20 ${parlamentar.ativo
                    ? "bg-gradient-to-r from-primary/10 to-accent/5 group-hover:from-primary/20"
                    : "bg-gradient-to-r from-slate-500/10 to-transparent"
                    }`}>
                    <div className="absolute top-0 right-0 p-4 z-20">
                      {parlamentar.ativo ? (
                        <Badge className="bg-green-500/10 text-green-600 border-green-500/20 font-black text-[9px] uppercase tracking-widest px-2 py-0.5 backdrop-blur-md">
                          Ativo
                        </Badge>
                      ) : (
                        <Badge variant="secondary" className="bg-muted text-muted-foreground border-muted-foreground/20 font-black text-[9px] uppercase tracking-widest px-2 py-0.5 backdrop-blur-md border">
                          Inativo
                        </Badge>
                      )}
                    </div>
                    {/* Decorative shape */}
                    <div className="absolute -top-12 -left-12 w-24 h-24 bg-primary/10 rounded-full blur-2xl group-hover:scale-150 transition-transform duration-700" />
                  </div>

                  <CardContent className="p-0 -mt-12 relative z-10">
                    <div className="px-5 pb-6">
                      <div className="flex gap-4">
                        {/* Image with Lazy Loading */}
                        <div className="flex-shrink-0">
                          <div className="relative" data-image-id={getParlamentarImageKey(parlamentar, index)}>
                            <div className="absolute -inset-1 bg-gradient-to-br from-primary to-accent rounded-2xl blur opacity-25 group-hover:opacity-40 transition duration-500"></div>
                            {!visibleImages.has(getParlamentarImageKey(parlamentar, index)) ? (
                              <div className="h-32 w-24 rounded-2xl bg-muted animate-pulse relative z-10 border-2 border-background shadow-xl" />
                            ) : (
                              <Avatar className="h-32 w-24 rounded-2xl border-2 border-background shadow-xl group-hover:scale-105 transition-transform duration-500 relative z-10">
                                <AvatarImage
                                  src={`${config.imageBaseUrl}/${getParlamentarId(parlamentar)}.jpg`}
                                  alt={parlamentar.nome_parlamentar}
                                />
                                <AvatarFallback className="rounded-2xl text-xl font-black bg-muted text-muted-foreground uppercase shadow-inner">
                                  {parlamentar.nome_parlamentar.split(" ").filter(n => n.length > 2).slice(0, 2).map(n => n[0]).join("")}
                                </AvatarFallback>
                              </Avatar>
                            )}
                          </div>
                        </div>

                        {/* Info Section */}
                        <div className="flex-1 min-w-0 pt-14 space-y-4">
                          <div>
                            <h3 className="font-black text-lg leading-tight text-foreground group-hover:text-primary transition-colors truncate tracking-tight">
                              {parlamentar.nome_parlamentar}
                            </h3>
                            <p className="text-[10px] font-medium text-muted-foreground uppercase tracking-widest opacity-80 truncate">{parlamentar.nome_civil}</p>
                          </div>

                          <div className="flex items-center gap-1.5 flex-wrap">
                            <Badge className="font-black bg-primary/5 text-primary border-primary/10 text-[10px] uppercase tracking-tighter px-2.5 py-0.5" title={parlamentar.nome_partido}>
                              {parlamentar.sigla_partido}
                            </Badge>
                            <Badge variant="outline" className="flex items-center gap-1 font-bold border-muted-foreground/20 text-[10px] uppercase tracking-tighter px-2.5 py-0.5" title={parlamentar.nome_estado}>
                              <MapPin className="w-2.5 h-2.5" />
                              {parlamentar.sigla_estado}
                            </Badge>
                          </div>
                        </div>
                      </div>

                      {/* Financial Info Grid */}
                      <div className="grid grid-cols-2 gap-4 mt-6 pt-6 border-t border-border/50">
                        <div className="space-y-1">
                          <p className="text-[9px] font-black text-muted-foreground uppercase tracking-widest opacity-60">Cota Parlamentar</p>
                          <p className="font-black text-sm text-purple-600 font-mono tracking-tighter group-hover:scale-105 transition-transform origin-left">
                            {formatCurrency((parlamentar[config.ceapField] as string) || "0")}
                          </p>
                        </div>
                        <div className="space-y-1 text-right">
                          <p className="text-[9px] font-black text-muted-foreground uppercase tracking-widest opacity-60 truncate">{config.remunerationLabel}</p>
                          <p className="font-black text-sm text-orange-600 font-mono tracking-tighter group-hover:scale-105 transition-transform origin-right">
                            {formatCurrency(parlamentar.valor_total_remuneracao)}
                          </p>
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </Link>
            ))}
          </div>

          {/* Modern No Results */}
          {parlamentares.length === 0 && !searching && (
            <div className="text-center py-16">
              <div className="max-w-md mx-auto">
                <User className="h-16 w-16 text-gray-400 mx-auto mb-4" />
                <h3 className="text-xl font-semibold text-gray-600 mb-2">
                  {config.noResultsMessage}
                </h3>
                <p className="text-muted-foreground">
                  {config.noResultsHint}
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

export default ParlamentareLista;