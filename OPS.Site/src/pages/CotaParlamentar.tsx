import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { ErrorMessage } from "@/components/ErrorMessage";
import { useState, useEffect } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useSearchParams } from "react-router-dom";
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow, } from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue, } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Label } from "@/components/ui/label";
import { MultiSelectDropdown } from "@/components/MultiSelectDropdown";
import { FornecedorSearchModal } from "@/components/FornecedorSearchModal";
import { ChevronUpIcon, ChevronDownIcon, Search, Trash, Plus, Calendar, AlertTriangle, Clock, Zap, Users, Building2, Briefcase, FolderOpen, Link2, User, FileText } from "lucide-react";
import { fetchParliamentMembers, fetchEstados, fetchPartidos, fetchTiposDespesa, fetchDespesasCotaParlamentar, DropDownOptions, TipoDespesa, Filters, SortOrder, DespesaCotaParlamentar } from "@/lib/api";
import { delay } from "@/lib/utils";
import { Alert, AlertDescription } from "@/components/ui/alert";


const legislaturas = [
  { value: "57", label: "57ª (fev/2023 à jan/2027)" },
  { value: "56", label: "56ª (fev/2019 à jan/2023)" },
  { value: "55", label: "55ª (fev/2015 à jan/2019)" },
  { value: "54", label: "54ª (fev/2011 à jan/2015)" },
  { value: "53", label: "53ª (fev/2007 à jan/2011)" },
];

const agrupamentoOptions = [
  { value: "1", label: "Parlamentar / Liderança", icon: Users },
  { value: "2", label: "Despesa", icon: FolderOpen },
  { value: "3", label: "Fornecedor", icon: Building2 },
  { value: "4", label: "Partido", icon: Briefcase },
  { value: "5", label: "Estado", icon: Link2 },
  { value: "6", label: "Recibo", icon: FileText }
];

const formatCurrency = (value: number): string => {
  return 'R$ ' + value.toLocaleString("pt-BR", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
};

const formatNumber = (value: number): string => {
  return value.toLocaleString("pt-BR", {
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  });
};

const typeConfigs = {
  "deputado-federal": {
    title: "Cota Parlamentar - Câmara dos Deputados",
    subtitle: "Consulte e analise as despesas dos deputados federais",
    apiType: "deputado",
    detailRoute: "/deputado-federal",
    imageBaseUrl: "//static.ops.org.br/deputado",
    documentRoute: "/deputado-federal",
    defaultPeriod: "57"
  },
  "deputado-estadual": {
    title: "Cota Parlamentar - Assembleias Legislativas",
    subtitle: "Consulte e analise as despesas dos deputados estaduais",
    apiType: "deputadoestadual",
    detailRoute: "/deputado-estadual",
    imageBaseUrl: "//static.ops.org.br/deputadoestadual",
    documentRoute: "/deputado-estadual",
    defaultPeriod: "57"
  },
  "senador": {
    title: "Cota Parlamentar - Senado Federal",
    subtitle: "Consulte e analise as despesas dos senadores",
    apiType: "senador",
    detailRoute: "/senador",
    imageBaseUrl: "//static.ops.org.br/senador",
    documentRoute: "/senador",
    defaultPeriod: "57"
  }
} as const;

const getColumnConfigs = (agrupamento: string): ColumnConfig[] => {
  switch (agrupamento) {
    case "1": // Parlamentar / Liderança
      return [
        { key: "acoes", label: "", sortable: false, columnIndex: 0 },
        { key: "nome_parlamentar", label: "Parlamentar", sortable: true, columnIndex: 1 },
        { key: "total_notas", label: "Qtd. Recibos", sortable: true, columnIndex: 4, align: 'right', hideOnSmallScreen: true },
        { key: "valor_total", label: "Valor Total", sortable: true, columnIndex: 5, align: 'right' }
      ];
    case "2": // Despesa
      return [
        { key: "acoes", label: "", sortable: false, columnIndex: 0 },
        { key: "descricao", label: "Despesa", sortable: true, columnIndex: 1 },
        { key: "total_notas", label: "Qtd. Recibos", sortable: true, columnIndex: 2, align: 'right', hideOnSmallScreen: true },
        { key: "valor_total", label: "Valor Total", sortable: true, columnIndex: 3, align: 'right' }
      ];
    case "3": // Fornecedor
      return [
        { key: "acoes", label: "", sortable: false, columnIndex: 0 },
        { key: "nome_fornecedor", label: "Fornecedor", sortable: true, columnIndex: 1 },
        { key: "total_notas", label: "Qtd. Recibos", sortable: true, columnIndex: 2, align: 'right', hideOnSmallScreen: true },
        { key: "valor_total", label: "Valor Total", sortable: true, columnIndex: 3, align: 'right' }
      ];
    case "4": // Partido
      return [
        { key: "acoes", label: "", sortable: false, columnIndex: 0 },
        { key: "nome_partido", label: "Partido", sortable: true, columnIndex: 1 },
        { key: "total_notas", label: "Qtd. Recibos", sortable: true, columnIndex: 2, align: 'right', hideOnSmallScreen: true },
        { key: "valor_total", label: "Valor Total", sortable: true, columnIndex: 5, align: 'right' }
      ];
    case "5": // Estado
      return [
        { key: "acoes", label: "", sortable: false, columnIndex: 0 },
        { key: "nome_estado", label: "Estado", sortable: true, columnIndex: 1 },
        { key: "total_notas", label: "Qtd. Recibos", sortable: true, columnIndex: 2, align: 'right', hideOnSmallScreen: true },
        { key: "valor_total", label: "Valor Total", sortable: true, columnIndex: 3, align: 'right' }
      ];
    case "6": // Recibo
      return [
        { key: "acoes", label: "", sortable: false, columnIndex: -1 },
        { key: "data_emissao", label: "Emissão", sortable: true, columnIndex: 0 },
        { key: "nome_parlamentar", label: "Parlamentar", sortable: false, columnIndex: 0 },
        { key: "nome_fornecedor", label: "Fornecedor", sortable: false, columnIndex: 0 },
        { key: "despesa_tipo", label: "Tipo de Despesa", sortable: false, columnIndex: 0 },
        { key: "valor_liquido", label: "Valor", sortable: true, columnIndex: 3, align: 'right' }
      ];
    default:
      return getColumnConfigs("1");
  }
};

interface ColumnConfig {
  key: string;
  label: string;
  sortable: boolean;
  columnIndex: number;
  align?: 'left' | 'right';
  hideOnSmallScreen?: boolean;
}

export default function CotaParlamentar({ type }: { type?: "deputado-federal" | "deputado-estadual" | "senador" }) {
  const config = type ? typeConfigs[type] : typeConfigs["deputado-federal"];
  usePageTitle(config.title);
  const [showFilters, setShowFilters] = useState(false);
  const [searchParams, setSearchParams] = useSearchParams();

  // Single unified state for all filters
  const [selectedFilters, setSelectedFilters] = useState({
    parlamentares: [] as string[],
    estados: [] as string[],
    partidos: [] as string[],
    despesas: [] as string[],
    fornecedores: [] as string[],
    periodo: config.defaultPeriod as "57" | "0",
    agrupamento: "1"
  });

  // Table States
  const [currentPage, setCurrentPage] = useState(1);
  const [sortField, setSortField] = useState<number | null>(null);
  const [sortOrder, setSortOrder] = useState<SortOrder>('desc');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedParlamentar, setSelectedParlamentar] = useState<DespesaCotaParlamentar | null>(null);
  const [activeAgrupamento, setActiveAgrupamento] = useState("1");
  const itemsPerPage = 50;

  const columns = getColumnConfigs(activeAgrupamento);

  // Helper function to get filters in API format
  const getApiFilters = (): Filters => ({
    Agrupamento: selectedFilters.agrupamento,
    Periodo: selectedFilters.periodo,
    IdParlamentar: selectedFilters.parlamentares.join(','),
    Despesa: selectedFilters.despesas.join(','),
    Estado: selectedFilters.estados.join(','),
    Partido: selectedFilters.partidos.join(','),
    Fornecedor: selectedFilters.fornecedores.join(',')
  });

  // Parse query string on component mount
  useEffect(() => {
    const urlFilters: Record<string, string> = {};

    const urlKeys = ["Agrupamento", "Periodo", "IdParlamentar", "Despesa", "Estado", "Partido", "Fornecedor"];
    urlKeys.forEach(key => {
      const value = searchParams.get(key);
      if (value) urlFilters[key] = value;
    });

    console.log('Filtros: ', urlFilters)

    if (Object.keys(urlFilters).length > 0) {
      setSelectedFilters(prev => ({
        ...prev,
        periodo: (urlFilters.Periodo || prev.periodo) as "0" | "57",
        agrupamento: urlFilters.Agrupamento || prev.agrupamento,
        parlamentares: urlFilters.IdParlamentar ? urlFilters.IdParlamentar?.split(',') : prev.parlamentares,
        estados: urlFilters.Estado ? urlFilters.Estado?.split(',') : prev.estados,
        partidos: urlFilters.Partido ? urlFilters.Partido?.split(',') : prev.partidos,
        despesas: urlFilters.Despesa ? urlFilters.Despesa?.split(',') : prev.despesas,
        fornecedores: urlFilters.Fornecedor ? urlFilters.Fornecedor?.split(',') : prev.fornecedores
      }));

      delay(refetch);
    }
  }, [searchParams]);

  const { data: apiData, isLoading, isFetching, error, refetch } = useQuery({
    queryKey: ['parlamentar-data'], // Static key to prevent automatic refetching
    queryFn: () => fetchDespesasCotaParlamentar(type, currentPage, itemsPerPage, sortField, sortOrder, getApiFilters()),
    staleTime: 0, // Disable caching to ensure immediate refetch
    enabled: false // Disable automatic fetching - only use refetch
  });

  // Update active agrupamento when data is successfully fetched
  useEffect(() => {
    if (apiData && !isLoading) {
      setActiveAgrupamento(selectedFilters.agrupamento);
    }
  }, [apiData, isLoading]);

  useEffect(() => {
    delay(refetch);
  }, []);

  const handleSort = (field: number) => {
    if (sortField === field) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');

      console.log("Changing sort order to", field, sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortOrder('desc');

      console.log("Changing sort field to", field, "desc");
    }

    setCurrentPage(1);
    delay(refetch);
  };

  const handleAgrupamentoChange = (newAgrupamento: string) => {
    if (!selectedParlamentar) return;

    // Apply specific filters based on the row data and selected Agrupamento
    const updatedFilters = { ...selectedFilters, agrupamento: newAgrupamento };

    switch (selectedFilters.agrupamento) {
      case "1":
        updatedFilters.parlamentares = [selectedParlamentar.id_parlamentar?.toString() || ""];
        break;
      case "2":
        updatedFilters.despesas = [selectedParlamentar.id_despesa_tipo?.toString() || ""];
        break;
      case "3":
        updatedFilters.fornecedores = [selectedParlamentar.id_fornecedor?.toString() || ""];
        break;
      case "4":
        updatedFilters.partidos = [selectedParlamentar.id_partido?.toString() || ""];
        break;
      case "5":
        updatedFilters.estados = [selectedParlamentar.id_estado?.toString() || ""];
        break;
    }

    setSelectedFilters(updatedFilters);
    setIsModalOpen(false);
    delay(refetch);
  };

  const openModal = (parlamentar: DespesaCotaParlamentar) => {
    setSelectedParlamentar(parlamentar);
    setIsModalOpen(true);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
    delay(refetch);
  };

  const SortIcon = ({ field }: { field: number }) => {
    if (sortField !== field) {
      return <ChevronUpIcon className="w-4 h-4 opacity-50" />;
    }
    return sortOrder === 'asc' ?
      <ChevronUpIcon className="w-4 h-4" /> :
      <ChevronDownIcon className="w-4 h-4" />;
  };

  const resetPaginationAndSorting = (newAgrupamento: string) => {
    if (newAgrupamento != activeAgrupamento) {
      console.log("Resetting pagination and sorting");
      setSortField(null);
      setSortOrder('desc');
      setCurrentPage(1);
    }
  }

  const handleSearch = () => {
    const apiFilters = getApiFilters();
    const params = new URLSearchParams();

    if (apiFilters.Agrupamento && apiFilters.Agrupamento !== "1")
      params.set("Agrupamento", apiFilters.Agrupamento);
    if (apiFilters.Periodo && apiFilters.Periodo !== config.defaultPeriod)
      params.set("Periodo", apiFilters.Periodo);
    if (apiFilters.IdParlamentar)
      params.set("IdParlamentar", apiFilters.IdParlamentar);
    if (apiFilters.Despesa)
      params.set("Despesa", apiFilters.Despesa);
    if (apiFilters.Estado)
      params.set("Estado", apiFilters.Estado);
    if (apiFilters.Partido)
      params.set("Partido", apiFilters.Partido);
    if (apiFilters.Fornecedor)
      params.set("Fornecedor", apiFilters.Fornecedor);

    setSearchParams(params);
    resetPaginationAndSorting(apiFilters.Agrupamento);

    delay(refetch);
  };

  const handleClearFilters = () => {
    setSelectedFilters({
      parlamentares: [],
      estados: [],
      partidos: [],
      despesas: [],
      fornecedores: [],
      periodo: config.defaultPeriod as "57" | "0",
      agrupamento: "1"
    });

    setSearchParams({});
    resetPaginationAndSorting("1");

    delay(refetch);
  };

  // API queries for filter components
  const { data: parlamentaresData = [] } = useQuery({
    queryKey: ["pesquisa", type, selectedFilters.periodo],
    queryFn: () => fetchParliamentMembers(type, "", selectedFilters.periodo),
    staleTime: 10 * 60 * 1000,
  });

  const { data: estadosData = [] as DropDownOptions[] } = useQuery({
    queryKey: ["estados"],
    queryFn: () => fetchEstados(),
    staleTime: 60 * 60 * 1000,
  });

  const { data: partidosData = [] as DropDownOptions[] } = useQuery({
    queryKey: ["partidos"],
    queryFn: () => fetchPartidos(),
    staleTime: 60 * 60 * 1000,
  });

  const { data: tiposDespesaData = [] as TipoDespesa[] } = useQuery({
    queryKey: ["tipos-despesa", type],
    queryFn: () => fetchTiposDespesa(type),
    staleTime: 60 * 60 * 1000,
  });

  const getFilterSummary = () => {
    const filters = [];

    if (selectedFilters.parlamentares.length > 0)
      filters.push(`${selectedFilters.parlamentares.length} Parlamentar(es) / Liderança(s)`);

    if (selectedFilters.despesas.length > 0)
      filters.push(`${selectedFilters.despesas.length} despesa(s)`);

    if (selectedFilters.estados.length > 0)
      filters.push(`${selectedFilters.estados.length} estado(s)`);

    if (selectedFilters.partidos.length > 0)
      filters.push(`${selectedFilters.partidos.length} partido(s)`);

    if (selectedFilters.fornecedores.length > 0)
      filters.push(`${selectedFilters.fornecedores.length} fornecedor(es)`);

    return filters;
  };

  const filterSummary = getFilterSummary();

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
        <Header />
        <main className="container mx-auto px-4 py-8">
          {/* Custom Header Layout */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
            <div>
              <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent mb-2">
                {config.title}
              </h1>
              <p className="text-muted-foreground text-lg">
                {config.subtitle}
              </p>
            </div>
          </div>
          <ErrorMessage
            onRetry={() => refetch()}
          />
        </main>
        <Footer />
      </div>
    );
  }

  const parlamentares = apiData?.data || [];
  const totalRecords = apiData?.records_total || 0;
  const totalPages = Math.ceil(totalRecords / itemsPerPage);
  const pagination = {
    page: currentPage,
    limit: itemsPerPage,
    total: totalRecords,
    totalPages: totalPages
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
      {/* Full-screen loading overlay */}
      <LoadingOverlay isLoading={isLoading || isFetching} content="Carregando informações das despesas..." />

      <Header />
      <main className="container mx-auto px-4 py-8">
        {/* Filter Section */}
        <div className="space-y-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent mb-2">{config.title}</h1>
              <p className="text-muted-foreground text-lg">{config.subtitle}</p>
            </div>
            <Button
              onClick={() => setShowFilters(!showFilters)}
              variant={showFilters ? "default" : "outline"}
              className="flex-shrink-0"
              size="lg"
            >
              <Search className="h-4 w-4 mr-2" />
              {showFilters ? "Ocultar filtros" : "Mostrar filtros"}
            </Button>
          </div>

          {/* Alert for deputados-estaduais */}
          {type === "deputado-estadual" && (
            <Alert className="border-blue-200 bg-blue-50">
              <AlertDescription className="text-blue-800">
                Alguns estados também apresentam os valores de diárias e verbas de saúde que não fazem parte da cota parlamentar. Se necessário é possivel desconsiderar os valores pelo filtro "Tipo de Despesa".
              </AlertDescription>
            </Alert>
          )}

          {filterSummary.length > 0 && (
            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm border-l-4 border-l-primary overflow-hidden">
              <CardContent className="pt-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div className="h-2 w-2 bg-primary rounded-full animate-pulse"></div>
                    <span className="text-sm font-bold text-foreground">Filtros aplicados:</span>
                    <div className="flex gap-2 flex-wrap">
                      {filterSummary.map((filter, index) => (
                        <Badge key={index} variant="secondary" className="text-xs font-semibold">
                          {filter}
                        </Badge>
                      ))}
                    </div>
                  </div>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={handleClearFilters}
                    className="text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors"
                  >
                    <Trash className="h-4 w-4 mr-1" />
                    Limpar
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}

          {showFilters && (
            <Card className="shadow-xl border-0 bg-card/80 backdrop-blur-sm overflow-hidden">
              <CardContent className="p-6">
                <div className="space-y-6">
                  <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                    {/* Filter fields with improved label wrapping */}
                    <div className="space-y-3">
                      <label className="text-sm font-bold text-foreground flex items-center gap-2">
                        <div className="p-1 px-2 bg-primary/10 rounded text-primary text-[10px] font-bold uppercase tracking-wider">01</div>
                        Legislatura
                      </label>
                      <Select value={selectedFilters.periodo} onValueChange={(value) => setSelectedFilters(prev => ({ ...prev, periodo: value as "0" | "57" }))}>
                        <SelectTrigger className="h-11 bg-background/50 border-muted">
                          <SelectValue placeholder="Selecione a legislatura" />
                        </SelectTrigger>
                        <SelectContent>
                          {legislaturas.map((leg) => (
                            <SelectItem key={leg.value} value={leg.value}>
                              {leg.label}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>

                    <div className="space-y-3">
                      <label className="text-sm font-bold text-foreground flex items-center gap-2">
                        <div className="p-1 px-2 bg-primary/10 rounded text-primary text-[10px] font-bold uppercase tracking-wider">02</div>
                        Parlamentar / Liderança
                      </label>
                      <MultiSelectDropdown
                        items={parlamentaresData}
                        placeholder={`Selecione Parlamentares / Lideranças`}
                        selectedItems={selectedFilters.parlamentares}
                        onSelectionChange={(items) => setSelectedFilters(prev => ({ ...prev, parlamentares: items }))}
                      />
                    </div>

                    <div className="space-y-3">
                      <label className="text-sm font-bold text-foreground flex items-center gap-2">
                        <div className="p-1 px-2 bg-primary/10 rounded text-primary text-[10px] font-bold uppercase tracking-wider">03</div>
                        Tipo de Despesa
                      </label>
                      <MultiSelectDropdown
                        items={tiposDespesaData}
                        placeholder="Selecione tipos de despesa"
                        selectedItems={selectedFilters.despesas}
                        onSelectionChange={(items) => setSelectedFilters(prev => ({ ...prev, despesas: items }))}
                      />
                    </div>

                    <div className="space-y-3">
                      <label className="text-sm font-bold text-foreground flex items-center gap-2">
                        <div className="p-1 px-2 bg-primary/10 rounded text-primary text-[10px] font-bold uppercase tracking-wider">04</div>
                        Estado
                      </label>
                      <MultiSelectDropdown
                        items={estadosData}
                        placeholder="Selecione estados"
                        selectedItems={selectedFilters.estados}
                        onSelectionChange={(items) => setSelectedFilters(prev => ({ ...prev, estados: items }))}
                      />
                    </div>

                    <div className="space-y-3">
                      <label className="text-sm font-bold text-foreground flex items-center gap-2">
                        <div className="p-1 px-2 bg-primary/10 rounded text-primary text-[10px] font-bold uppercase tracking-wider">05</div>
                        Partido
                      </label>
                      <MultiSelectDropdown
                        items={partidosData}
                        placeholder="Selecione partidos"
                        selectedItems={selectedFilters.partidos}
                        onSelectionChange={(items) => setSelectedFilters(prev => ({ ...prev, partidos: items }))}
                      />
                    </div>

                    <div className="space-y-3">
                      <label className="text-sm font-bold text-foreground flex items-center gap-2">
                        <div className="p-1 px-2 bg-primary/10 rounded text-primary text-[10px] font-bold uppercase tracking-wider">06</div>
                        Fornecedor
                      </label>
                      <div className="flex gap-2">
                        <div className="flex-1">
                          <FornecedorSearchModal
                            selectedFornecedores={selectedFilters.fornecedores}
                            onSelectionChange={(items) => setSelectedFilters(prev => ({ ...prev, fornecedores: items }))}
                          />
                        </div>
                        {selectedFilters.fornecedores.length > 0 && (
                          <Button
                            variant="outline"
                            size="icon"
                            title="Limpar seleção"
                            className="h-11 w-11 p-0 hover:bg-destructive hover:text-destructive-foreground transition-colors border-muted bg-background/50"
                            onClick={() => setSelectedFilters(prev => ({ ...prev, fornecedores: [] }))}
                          >
                            <Trash className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </div>
                  </div>

                  <div className="border-t border-muted/50 pt-6">
                    <div className="space-y-4">
                      <label className="text-sm font-bold text-foreground flex items-center gap-2">
                        <div className="p-1 px-2 bg-primary/10 rounded text-primary text-[10px] font-bold uppercase tracking-wider">07</div>
                        Agrupar por
                      </label>
                      <RadioGroup
                        value={selectedFilters.agrupamento}
                        onValueChange={(value) => setSelectedFilters(prev => ({ ...prev, agrupamento: value }))}
                        className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-6 gap-3"
                      >
                        {agrupamentoOptions.map((option) => {
                          const Icon = option.icon;
                          const isSelected = selectedFilters.agrupamento === option.value;
                          return (
                            <div className="relative" key={option.value}>
                              <RadioGroupItem
                                value={option.value}
                                id={`agrupamento-${option.value}`}
                                className="peer sr-only"
                              />
                              <Label
                                htmlFor={`agrupamento-${option.value}`}
                                className={`flex flex-col items-center justify-center rounded-xl border-2 p-3 cursor-pointer transition-all duration-300 ${isSelected
                                    ? "border-primary bg-primary/10 text-primary shadow-inner"
                                    : "border-muted bg-background/50 hover:bg-accent hover:border-accent-foreground/30 text-muted-foreground"
                                  }`}
                              >
                                <Icon className={`h-5 w-5 mb-1.5 transition-transform duration-300 ${isSelected ? "scale-110" : ""}`} />
                                <div className="text-xs font-bold uppercase tracking-tight text-center">{option.label}</div>
                              </Label>
                            </div>
                          );
                        })}
                      </RadioGroup>
                    </div>
                  </div>

                  <div className="flex flex-col sm:flex-row gap-3 pt-6 border-t border-muted/50">
                    <Button onClick={handleSearch} size="lg" className="flex-1 bg-primary hover:bg-primary/90 text-white shadow-lg shadow-primary/20 transition-all duration-300 hover:scale-[1.02]">
                      <Search className="h-5 w-5 mr-2" />
                      Pesquisar agora
                    </Button>
                    <Button variant="outline" onClick={handleClearFilters} size="lg" className="flex-1 sm:flex-none border-muted hover:bg-accent transition-colors">
                      <Trash className="h-5 w-5 mr-2" />
                      Limpar todos
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}
        </div>

        {/* Table Section */}
        <div className="space-y-6 mt-6">
          <div className="flex items-center justify-between">
            <div>
              {pagination && (
                <p className="text-sm text-muted-foreground mt-1">
                  {formatNumber(pagination.total)} resultado{pagination.total !== 1 ? 's' : ''} encontrado{pagination.total !== 1 ? 's' : ''}
                </p>
              )}
            </div>
            <div className="flex items-center gap-2">
              <span className="text-sm text-muted-foreground">Agrupando por</span>
              <span className="text-sm font-medium text-foreground">
                {agrupamentoOptions.find(option => option.value === activeAgrupamento)?.label}
              </span>
            </div>
          </div>

          <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden">
            <div className="overflow-x-auto">
              <Table key={activeAgrupamento}>
                <TableHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                  <TableRow className="hover:bg-transparent">
                    {columns.map((column) => {
                      return (
                        <TableHead
                          key={column.key}
                          className={`${column.align === 'right' ? 'text-right' : ''} ${column.sortable ? 'cursor-pointer hover:bg-muted/80 transition-colors' : ''} py-4 px-6 text-xs font-bold uppercase tracking-wider text-foreground border-b-2 border-primary/10 ${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}
                          onClick={() => column.sortable && handleSort(column.columnIndex)}
                        >
                          {column.label && (
                            <div className={`flex items-center gap-2 ${column.align === 'right' ? 'justify-end' : ''}`}>
                              {column.label}
                              {column.sortable && <SortIcon field={column.columnIndex} />}
                            </div>
                          )}
                        </TableHead>
                      );
                    })}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {parlamentares.map((parlamentar, index) => (
                    <TableRow key={`${parlamentar.id_parla || 'no-id'}-${index}`} className="hover:bg-muted/30 transition-colors border-b last:border-0">
                      {columns.map((column) => {
                        if (column.key === 'acoes' && activeAgrupamento !== '6') {
                          return (
                            <TableCell key={column.key} className={`py-4 px-6 ${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}>
                              <div className="flex gap-2">
                                <Button
                                  size="sm"
                                  className="bg-primary hover:bg-primary/90 transition-colors hidden sm:flex"
                                  onClick={() => openModal(parlamentar)}
                                >
                                  Detalhar
                                </Button>
                                <Button
                                  size="sm"
                                  variant="outline"
                                  className="sm:hidden hover:bg-primary hover:text-primary-foreground transition-colors"
                                  onClick={() => openModal(parlamentar)}
                                  title="Detalhar"
                                >
                                  <Plus className="h-4 w-4" />
                                </Button>
                              </div>
                            </TableCell>
                          );
                        }

                        let cellContent = parlamentar[column.key];

                        // Apply formatting for specific columns
                        if (cellContent) {
                          // if (column.key === 'total_notas') {
                          //   cellContent = parseInt(cellContent.replace(/\./g, '')).toLocaleString("pt-BR");
                          // } else 
                          if (column.key === 'valor_total' || column.key === 'valor_liquido') {
                            if (parlamentar.id_despesa)
                              return (
                                <TableCell key={column.key} className="py-4 px-6 text-right">
                                  <Link
                                    to={`./${parlamentar.id_despesa}`}
                                    className="text-primary hover:text-primary/80 transition-colors font-bold font-mono"
                                  >
                                    R$&nbsp;{cellContent}
                                  </Link>
                                </TableCell>
                              );
                            else {
                              return (
                                <TableCell key={column.key} className="py-4 px-6 text-right font-bold font-mono">
                                  {cellContent}
                                </TableCell>
                              );
                            }
                          } else if (column.key === 'nome_parlamentar') {
                            return (
                              <TableCell key={column.key} className={`py-4 px-6 ${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}>
                                <Link
                                  to={`${config.detailRoute}/${parlamentar.id_parlamentar}`}
                                  className="text-primary hover:text-primary/80 transition-colors font-medium"
                                >
                                  <div className="flex flex-col">
                                    <span>{cellContent}</span>
                                    <div className="font-mono text-xs text-muted-foreground mt-1">
                                      {parlamentar.sigla_partido} / {parlamentar.sigla_estado}
                                    </div>
                                  </div>
                                </Link>
                              </TableCell>
                            );
                          } else if (column.key === 'nome_fornecedor') {
                            return (
                              <TableCell key={column.key} className={`py-4 px-6 ${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}>
                                <Link
                                  to={`/fornecedor/${parlamentar.id_fornecedor}`}
                                  className="text-primary hover:text-primary/80 transition-colors font-medium"
                                >
                                  <div className="flex flex-col">
                                    <span>{cellContent}</span>
                                    <div className="font-mono text-xs text-muted-foreground mt-1">
                                      {parlamentar.cnpj_cpf}
                                    </div>
                                  </div>
                                </Link>
                              </TableCell>
                            );
                          } else if (column.key === 'despesa_tipo') {
                            return (
                              <TableCell key={column.key} className={`py-4 px-6 ${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}>
                                <div className="flex flex-col">
                                  <span>{cellContent}</span>
                                  <div className="font-mono text-xs text-muted-foreground mt-1">
                                    {parlamentar.despesa_especificacao}
                                  </div>
                                </div>
                              </TableCell>
                            );
                          }
                        }

                        return (
                          <TableCell key={column.key} className={`py-4 px-6 ${column.align === 'right' ? 'text-right font-mono' : ''} font-medium ${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}>
                            {cellContent}
                          </TableCell>
                        );
                      })}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </Card>

          {pagination && pagination.totalPages > 1 && (
            <Card className="border-0 shadow-lg">
              <CardContent className="p-4">
                <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
                  <div className="text-sm text-muted-foreground">
                    Mostrando {((pagination.page - 1) * pagination.limit) + 1} a {Math.min(pagination.page * pagination.limit, pagination.total)} de {formatNumber(pagination.total)} resultados
                  </div>
                  <div className="flex items-center gap-1">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePageChange(1)}
                      disabled={pagination.page <= 1}
                      className="h-8 px-3"
                    >
                      Primeira
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePageChange(pagination.page - 1)}
                      disabled={pagination.page <= 1}
                      className="h-8 px-3"
                    >
                      <ChevronUpIcon className="w-4 h-4 rotate-270" />
                    </Button>
                    <div className="flex items-center gap-1 mx-2">
                      {(() => {
                        const startPage = Math.max(1, pagination.page - 3);
                        const endPage = Math.min(pagination.totalPages, pagination.page + 3);
                        const pages = [];

                        for (let i = startPage; i <= endPage; i++) {
                          pages.push(i);
                        }

                        return pages.map((page) => (
                          <Button
                            key={page}
                            variant={pagination.page === page ? "default" : "outline"}
                            size="sm"
                            onClick={() => handlePageChange(page)}
                            className={`min-w-8 h-8 ${pagination.page === page ? "shadow-lg" : ""}`}
                          >
                            {page}
                          </Button>
                        ));
                      })()}
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePageChange(pagination.page + 1)}
                      disabled={pagination.page >= pagination.totalPages}
                      className="h-8 px-3"
                    >
                      <ChevronDownIcon className="w-4 h-4 rotate-90" />
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePageChange(pagination.totalPages)}
                      disabled={pagination.page >= pagination.totalPages}
                      className="h-8 px-3"
                    >
                      Última
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Modal */}
          <Dialog open={isModalOpen} onOpenChange={setIsModalOpen}>
            <DialogContent className="sm:max-w-[500px] border-0 shadow-xl">
              <DialogHeader className="pb-4 border-b">
                <DialogTitle className="text-xl font-semibold flex items-center gap-2">
                  <div className="h-1 w-4 bg-primary rounded"></div>
                  Agrupar por
                </DialogTitle>
              </DialogHeader>
              <div className="grid gap-3 py-4">
                {agrupamentoOptions.map((option) => {
                  const Icon = option.icon;
                  return (
                    <Button
                      key={option.value}
                      variant={option.value === selectedFilters.agrupamento ? "default" : "outline"}
                      className={`justify-start h-auto p-4 text-left transition-all ${option.value === selectedFilters.agrupamento
                        ? "shadow-lg border-primary bg-primary/10"
                        : "hover:bg-muted/50 hover:border-primary/50"
                        }`}
                      onClick={() => handleAgrupamentoChange(option.value)}
                    >
                      <div className="flex items-center justify-between w-full">
                        <div className="flex items-center gap-3">
                          <Icon className={`h-5 w-5 ${option.value === selectedFilters.agrupamento ? "text-primary" : "text-muted-foreground"}`} />
                          <div>
                            <div className="font-medium text-foreground">{option.label}</div>
                            {option.value === selectedFilters.agrupamento && (
                              <div className="text-sm text-primary font-medium mt-1">
                                ✓ Atualmente selecionado
                              </div>
                            )}
                          </div>
                        </div>
                        {option.value === selectedFilters.agrupamento && (
                          <div className="h-2 w-2 bg-primary rounded-full"></div>
                        )}
                      </div>
                    </Button>
                  );
                })}
              </div>
            </DialogContent>
          </Dialog>
        </div>
      </main>
      <Footer />
    </div>
  );
}
