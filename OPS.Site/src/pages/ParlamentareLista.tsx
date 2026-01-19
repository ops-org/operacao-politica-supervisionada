import { useState, useEffect, useCallback, useMemo } from "react";
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
    documentTitle: "OPS :: Deputado Federal",
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
    documentTitle: "OPS :: Senador",
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
    documentTitle: "OPS :: Deputado Estadual",
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

  useEffect(() => {
    document.title = config.documentTitle;
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

  {/* Full-screen loading overlay */}
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
          {/* Modern Header with Type Selector */}
          <div className="text-center space-y-4">
            <div className="flex items-center justify-center gap-3 mb-4">
              <Users className="h-8 w-8 text-primary" />
              <h1 className="text-4xl font-bold bg-gradient-to-r from-primary to-primary/80 bg-clip-text text-transparent">
                {config.title}
              </h1>
            </div>
            
            {/* Type Selector */}
            <div className="flex justify-center mb-4">
              <div className="inline-flex rounded-lg border p-1 bg-white shadow-sm">
                <Button
                  variant={parlamentarType === "deputado-federal" ? "default" : "ghost"}
                  size="sm"
                  onClick={() => setParlamentarType("deputado-federal")}
                  className="px-4 py-2"
                >
                  Deputados Federais
                </Button>
                {/* <Button
                  variant={parlamentarType === "deputado-estadual" ? "default" : "ghost"}
                  size="sm"
                  onClick={() => setParlamentarType("deputado-estadual")}
                  className="px-4 py-2"
                >
                  Deputados Estaduais
                </Button> */}
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

            <p className="text-muted-foreground mx-auto leading-relaxed">
              {config.subtitle}
              <br />
              <span className="text-sm font-bold">* Valores acumulados desde 2008 de todas as legislaturas</span>
            </p>
          </div>

          {/* Stats Cards */}
          {parlamentares.length > 0 && (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
              <Card className="bg-gradient-to-br from-blue-50 to-blue-100 border-blue-200">
                <CardContent className="p-6">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium text-blue-600">{config.statsLabel}</p>
                      <p className="text-2xl font-bold text-blue-900">{stats.totalParlamentares}</p>
                    </div>
                    <Users className="h-8 w-8 text-blue-500" />
                  </div>
                </CardContent>
              </Card>

              <Card className="bg-gradient-to-br from-green-50 to-green-100 border-green-200">
                <CardContent className="p-6">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium text-green-600">{config.activeLabel}</p>
                      <p className="text-2xl font-bold text-green-900">{stats.ativos}</p>
                    </div>
                    <TrendingUp className="h-8 w-8 text-green-500" />
                  </div>
                </CardContent>
              </Card>

              <Card className="bg-gradient-to-br from-purple-50 to-purple-100 border-purple-200">
                <CardContent className="p-6">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium text-purple-600">Cota Parlamentar</p>
                      <p className="text-lg font-bold text-purple-900">{formatCurrency(stats.totalCeap.toString())}</p>
                    </div>
                    <DollarSign className="h-8 w-8 text-purple-500" />
                  </div>
                </CardContent>
              </Card>

              <Card className="bg-gradient-to-br from-orange-50 to-orange-100 border-orange-200">
                <CardContent className="p-6">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium text-orange-600">{config.remunerationLabel}</p>
                      <p className="text-lg font-bold text-orange-900">{formatCurrency(stats.totalRemuneracao.toString())}</p>
                    </div>
                    <Building2 className="h-8 w-8 text-orange-500" />
                  </div>
                </CardContent>
              </Card>
            </div>
          )}

          {/* Modern Filters */}
          <Card className="shadow-md border-0">
            <CardContent className="pt-6">
              <div className="flex items-center gap-2 mb-4">
                <Search className="h-5 w-5 text-primary" />
                <h2 className="text-lg font-semibold">Filtros de Busca</h2>
              </div>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="space-y-2">
                  <label className="text-sm font-medium flex items-center gap-2">
                    <Building2 className="h-4 w-4" />
                    Legislatura
                  </label>
                  <Select
                    value={filters.periodo.toString()}
                    onValueChange={(value) => setFilters(prev => ({ ...prev, periodo: parseInt(value) }))}
                  >
                    <SelectTrigger className="bg-white">
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
                  <label className="text-sm font-medium flex items-center gap-2">
                    <MapPin className="h-4 w-4" />
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
                  <label className="text-sm font-medium flex items-center gap-2">
                    <Users className="h-4 w-4" />
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

              <div className="flex gap-3 mt-6">
                <Button
                  onClick={pesquisar}
                  disabled={searching}
                  className="bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 text-white shadow-xs"
                >
                  <Search className="h-4 w-4 mr-2" />
                  {searching ? "Pesquisando..." : "Pesquisar"}
                </Button>
                <Button
                  variant="outline"
                  onClick={limparFiltros}
                  className="border-gray-300 hover:bg-gray-50"
                >
                  <RotateCcw className="h-4 w-4 mr-2" />
                  Limpar filtros
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
                  className="group hover:shadow-md transition-all duration-300 hover:scale-105 border-0 bg-white shadow-xs overflow-hidden cursor-pointer"
                >
                {/* Card Header */}
                <div className={`relative overflow-hidden ${parlamentar.ativo
                  ? "bg-gradient-to-r from-emerald-600 to-emerald-500 text-white"
                  : "bg-gradient-to-r from-gray-500 to-gray-600 text-white"
                  }`}>
                  <div className="block p-4 hover:bg-black/10 transition-colors">
                    <div className="flex justify-between items-start gap-2">
                      <div className="flex-1 min-w-0">
                        <h3 className="font-bold text-lg leading-tight truncate">
                          {parlamentar.nome_parlamentar}
                        </h3>
                        <p className="text-sm opacity-90 truncate">{parlamentar.nome_civil}</p>
                      </div>
                      <Badge variant="secondary" className="text-xs bg-white/20 text-white border-white/30">
                        {parlamentar.situacao}
                      </Badge>
                    </div>
                  </div>
                </div>

                {/* Card Body */}
                <CardContent className="p-4">
                  <div className="flex gap-3">
                    {/* Image with Lazy Loading */}
                    <div className="flex-shrink-0">
                      <div className="relative" data-image-id={getParlamentarImageKey(parlamentar, index)}>
                        {!visibleImages.has(getParlamentarImageKey(parlamentar, index)) && (
                          <div className="absolute inset-0 bg-gray-200 animate-pulse rounded-xl" />
                        )}
                        {visibleImages.has(getParlamentarImageKey(parlamentar, index)) && (
                          <Avatar className="h-32 w-24 rounded-xl border-4 border-white shadow-lg group-hover:scale-105 transition-transform">
                            <AvatarImage
                              src={`${config.imageBaseUrl}/${getParlamentarId(parlamentar)}.jpg`}
                              alt={parlamentar.nome_parlamentar}
                            />
                            <AvatarFallback className="rounded-xl text-xl font-semibold bg-gradient-to-br from-primary/20 to-primary/10">
                              {parlamentar.nome_parlamentar.split(" ").map(n => n[0]).join("")}
                            </AvatarFallback>
                          </Avatar>
                        )}
                      </div>
                    </div>

                    {/* Info Section */}
                    <div className="flex-1 min-w-0 space-y-3">
                      {/* Party and State */}
                      <div className="flex items-center gap-1 flex-wrap">
                        <Badge variant="secondary" className="font-semibold" title={parlamentar.nome_partido}>
                          {parlamentar.sigla_partido}
                        </Badge>
                        <Badge variant="outline" className="flex items-center gap-1" title={parlamentar.nome_estado}>
                          <MapPin className="w-3 h-3" />
                          {parlamentar.sigla_estado}
                        </Badge>
                      </div>

                      {/* Financial Info */}
                      <div className="space-y-1">
                        <div className="flex items-center gap-1">
                          <DollarSign className="h-3 w-3 text-purple-600" />
                          <span className="text-xs text-muted-foreground">Cota Parlamentar</span>
                        </div>
                        <p className="font-bold text-sm text-purple-700">
                          {formatCurrency((parlamentar[config.ceapField] as string) || "0")}
                        </p>
                      </div>
                      <div className="space-y-1">
                        <div className="flex items-center gap-1">
                          <Building2 className="h-3 w-3 text-orange-600" />
                          <span className="text-xs text-muted-foreground">{config.remunerationLabel}</span>
                        </div>
                        <p className="font-bold text-sm text-orange-700">
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