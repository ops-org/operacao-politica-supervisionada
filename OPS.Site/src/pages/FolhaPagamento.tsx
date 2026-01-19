import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { useState, useEffect } from "react";
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
import { ChevronUpIcon, ChevronDownIcon, Search, Trash, Building2, Users, Calendar, Plus, Briefcase, User, FolderOpen, Link2, DollarSign, FileText } from "lucide-react";
import { apiClient, fetchRemuneracao, fetchVinculos, fetchCategorias, fetchCargos, fetchLotacoes, RemuneracaoData, RemuneracaoApiResponse, DropDownOptions } from "@/lib/api";
import { delay } from "@/lib/utils";
import { Alert, AlertDescription } from "@/components/ui/alert";

const formatNameAcronym = (value: string, acronym: string): string => {
    if (!acronym) return value;
    return String(`${value} (${acronym})`);
}

const generateAnos = () => {
    const currentYear = new Date().getFullYear();
    const anos = [];
    for (let year = currentYear; year >= 2012; year--) {
        anos.push({ value: year.toString(), label: year.toString() });
    }
    return anos;
};

const anos = generateAnos();

const meses = [
    { value: "1", label: "Janeiro" },
    { value: "2", label: "Fevereiro" },
    { value: "3", label: "Março" },
    { value: "4", label: "Abril" },
    { value: "5", label: "Maio" },
    { value: "6", label: "Junho" },
    { value: "7", label: "Julho" },
    { value: "8", label: "Agosto" },
    { value: "9", label: "Setembro" },
    { value: "10", label: "Outubro" },
    { value: "11", label: "Novembro" },
    { value: "12", label: "Dezembro" },
];

const agrupamentoOptions = [
    { value: "1", label: "Lotação", icon: Building2 },
    { value: "2", label: "Cargo", icon: Briefcase },
    { value: "3", label: "Categoria", icon: FolderOpen },
    { value: "4", label: "Vinculo", icon: Link2 },
    { value: "7", label: "Senador(a)", icon: User },
    { value: "5", label: "Ano", icon: Calendar },
    { value: "6", label: "Folha de pagamento", icon: FileText },
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

interface ColumnConfig {
    key: string;
    label: string;
    sortable: boolean;
    columnIndex?: number;
    align?: 'left' | 'right';
    hideOnSmallScreen?: boolean;
}

const getColumnConfigs = (agrupamento: string): ColumnConfig[] => {
    switch (agrupamento) {
        case "1": // Lotação
        case "2": // Cargo  
        case "3": // Categoria
        case "4": // Vinculo
        case "5": // Ano
        case "7": // Senador
            return [
                { key: "acoes", label: "", sortable: false, columnIndex: 0 },
                { key: "descricao", label: agrupamento === "7" ? "Senador" : agrupamento === "1" ? "Lotação" : agrupamento === "2" ? "Cargo" : agrupamento === "3" ? "Categoria" : agrupamento === "4" ? "Vinculo" : "Ano", sortable: true, columnIndex: 1 },
                { key: "quantidade", label: "Qtd.", sortable: true, align: 'right', columnIndex: 2 },
                { key: "valor_total", label: "Custo Total", sortable: true, align: 'right', columnIndex: 3 }
            ];
        case "6": // Não agrupar (detalhes)
            return [
                { key: "vinculo", label: "Vinculo", sortable: true, columnIndex: 0 },
                { key: "categoria", label: "Categoria/Cargo", sortable: true, columnIndex: 1 },
                // { key: "cargo", label: "Cargo", sortable: true, columnIndex: 2 },
                { key: "lotacao", label: "Lotação", sortable: true, columnIndex: 3 },
                { key: "tipo_folha", label: "Folha", sortable: true, columnIndex: 4 },
                { key: "ano_mes", label: "Ano/Mês", sortable: true, columnIndex: 5 },
                { key: "valor_total", label: "Custo Total", sortable: true, align: 'right', columnIndex: 6 }
            ];
        default:
            return getColumnConfigs("7");
    }
};

export default function FolhaPagamento() {
    const [showFilters, setShowFilters] = useState(false);
    const [searchParams, setSearchParams] = useSearchParams();
    const [selectedFilters, setSelectedFilters] = useState({
        ano: anos[0].value,
        mes: "",
        vinculo: [] as string[],
        categoria: [] as string[],
        cargo: [] as string[],
        lotacao: [] as string[],
        parlamentar: [] as string[],
        agrupar: "7"
    });

    const [currentPage, setCurrentPage] = useState(1);
    const [sortField, setSortField] = useState<number | null>(null);
    const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedRow, setSelectedRow] = useState<RemuneracaoData | null>(null);
    const [activeAgrupamento, setActiveAgrupamento] = useState("7");
    const itemsPerPage = 50;

    const columns = getColumnConfigs(activeAgrupamento);

    const getApiFilters = () => ({
        ag: selectedFilters.agrupar,
        an: selectedFilters.ano,
        ms: selectedFilters.mes,
        vn: selectedFilters.vinculo.join(","),
        ct: selectedFilters.categoria.join(","),
        cr: selectedFilters.cargo.join(","),
        lt: selectedFilters.lotacao.join(","),
        sn: selectedFilters.parlamentar.join(","),
    });

    useEffect(() => {
        const urlFilters: Record<string, string> = {};
        const urlKeys = ["ag", "an", "ms", "vn", "ct", "cr", "lt", "sn"];
        urlKeys.forEach(key => {
            const value = searchParams.get(key);
            if (value) urlFilters[key] = value;
        });

        if (Object.keys(urlFilters).length > 0) {
            setSelectedFilters(prev => ({
                ...prev,
                agrupar: urlFilters.ag || prev.agrupar,
                ano: urlFilters.an || prev.ano,
                mes: urlFilters.ms || prev.mes,
                vinculo: urlFilters.vn ? urlFilters.vn.split(",") : prev.vinculo,
                categoria: urlFilters.ct ? urlFilters.ct.split(",") : prev.categoria,
                cargo: urlFilters.cr ? urlFilters.cr.split(",") : prev.cargo,
                lotacao: urlFilters.lt ? urlFilters.lt.split(",") : prev.lotacao,
                parlamentar: urlFilters.sn ? urlFilters.sn.split(",") : prev.parlamentar,
            }));
        }

        delay(refetch);
    }, []);

    const { data: apiData, isRefetching: isLoading, error, refetch } = useQuery({
        queryKey: ['remuneracao-data'],
        queryFn: () => fetchRemuneracao(currentPage, itemsPerPage, sortField, sortOrder, getApiFilters()),
        staleTime: 0,
        enabled: false
    });

    useEffect(() => {
        if (apiData && !isLoading) {
            setActiveAgrupamento(selectedFilters.agrupar);
        }
    }, [apiData, isLoading]);

    const handlePageChange = (page: number) => {
        setCurrentPage(page);
        delay(refetch);
    };

    const handleSort = (field: number) => {
        if (sortField === field) {
            setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
        } else {
            setSortField(field);
            setSortOrder('desc');
        }
        setCurrentPage(1);
        delay(refetch);
    };

    const handleAgrupamentoChange = (newAgrupamento: string) => {
        if (!selectedRow) return;

        const updatedFilters = { ...selectedFilters, agrupar: newAgrupamento };

        switch (selectedFilters.agrupar) {
            case "1": // Lotação
                updatedFilters.lotacao = [selectedRow.id?.toString() || ""];
                break;
            case "2": // Cargo
                updatedFilters.cargo = [selectedRow.id?.toString() || ""];
                break;
            case "3": // Categoria
                updatedFilters.categoria = [selectedRow.id?.toString() || ""];
                break;
            case "4": // Vinculo
                updatedFilters.vinculo = [selectedRow.id?.toString() || ""];
                break;
            case "7": // Senador
                updatedFilters.parlamentar = [selectedRow.id?.toString() || ""];
                break;
        }

        setSelectedFilters(updatedFilters);
        setCurrentPage(1);
        setSortField(null);
        setSortOrder('desc');

        setIsModalOpen(false);
        delay(refetch);
    };

    const openModal = (row: RemuneracaoData) => {
        setSelectedRow(row);
        setIsModalOpen(true);
    };

    const SortIcon = ({ field }: { field: number }) => {
        if (sortField !== field) {
            return <ChevronUpIcon className="w-4 h-4 opacity-50" />;
        }
        return sortOrder === 'asc' ?
            <ChevronUpIcon className="w-4 h-4" /> :
            <ChevronDownIcon className="w-4 h-4" />;
    };

    const handleSearch = () => {
        const apiFilters = getApiFilters();
        const params = new URLSearchParams();

        Object.entries(apiFilters).forEach(([key, value]) => {
            if (value && value !== "") {
                params.set(key, value);
            }
        });

        setSearchParams(params);
        setCurrentPage(1);
        setSortField(null);
        setSortOrder('desc');

        delay(refetch);
    };

    const handleClearFilters = () => {
        setSelectedFilters({
            ano: anos[0].value,
            mes: "",
            vinculo: [],
            categoria: [],
            cargo: [],
            lotacao: [],
            parlamentar: [],
            agrupar: "7"
        });

        setSearchParams({});
        setCurrentPage(1);
        setSortField(null);
        setSortOrder('desc');

        delay(refetch);
    };

    // API queries for filter components
    const { data: vinculosData = [] } = useQuery({
        queryKey: ["vinculos"],
        queryFn: () => fetchVinculos(),
        staleTime: 60 * 60 * 1000,
    });

    const { data: categoriasData = [] } = useQuery({
        queryKey: ["categorias"],
        queryFn: () => fetchCategorias(),
        staleTime: 60 * 60 * 1000,
    });

    const { data: cargosData = [] } = useQuery({
        queryKey: ["cargos"],
        queryFn: () => fetchCargos(),
        staleTime: 60 * 60 * 1000,
    });

    const { data: lotacoesData = [] } = useQuery({
        queryKey: ["lotacoes"],
        queryFn: () => fetchLotacoes(),
        staleTime: 60 * 60 * 1000,
    });

    const { data: parlamentaresData = [] } = useQuery({
        queryKey: ["parlamentares", selectedFilters.ano, selectedFilters.mes],
        queryFn: () => apiClient.post<DropDownOptions[]>("/senador/pesquisa", { ano: parseInt(selectedFilters.ano ?? "0"), mes: parseInt(selectedFilters.mes ?? "0") }),
        staleTime: 10 * 60 * 1000,
    });

    const getFilterSummary = () => {
        const filters = [];

        if (selectedFilters.ano)
            filters.push(`Ano: ${selectedFilters.ano}`);

        if (selectedFilters.mes)
            filters.push(`Mês: ${meses.find(m => m.value === selectedFilters.mes)?.label}`);

        if (selectedFilters.vinculo.length > 0)
            filters.push(`${selectedFilters.vinculo.length} vínculo(s)`);

        if (selectedFilters.categoria.length > 0)
            filters.push(`${selectedFilters.categoria.length} categoria(s)`);

        if (selectedFilters.cargo.length > 0)
            filters.push(`${selectedFilters.cargo.length} cargo(s)`);

        if (selectedFilters.lotacao.length > 0)
            filters.push(`${selectedFilters.lotacao.length} lotação(ões)`);

        if (selectedFilters.parlamentar.length > 0)
            filters.push(`${selectedFilters.parlamentar.length} senador(es)`);

        return filters;
    };

    const filterSummary = getFilterSummary();

    if (error) {
        return (
            <div className="min-h-screen flex flex-col bg-gray-50">
                <Header />
                <main className="flex-1 container mx-auto px-4 py-8">
                    <div className="max-w-2xl mx-auto">
                        <div className="relative overflow-hidden rounded-2xl bg-red-50 border border-red-200 shadow-lg">
                            <div className="relative p-8 text-center">
                                <div className="w-16 h-16 bg-red-500 rounded-full flex items-center justify-center mx-auto mb-4 shadow-lg">
                                    <span className="text-white text-2xl font-bold">!</span>
                                </div>
                                <h3 className="text-xl font-bold text-red-800 mb-2">Erro ao carregar dados</h3>
                                <p className="text-red-600 mb-6">Por favor, tente novamente.</p>
                                <Button
                                    onClick={() => refetch()}
                                    className="bg-red-500 hover:bg-red-600 text-white font-semibold rounded-xl shadow-lg hover:shadow-xl transition-all duration-300"
                                >
                                    Tentar novamente
                                </Button>
                            </div>
                        </div>
                    </div>
                </main>
                <Footer />
            </div>
        );
    }

    const remuneracaoData = apiData?.data || [];
    const valorTotal = apiData?.valorTotal || 0;

    return (
        <>
            <div className="min-h-screen bg-background">
                <LoadingOverlay isLoading={isLoading} content="Carregando informações da remuneração..." />

                <Header />
                <main className="container mx-auto px-4 py-8">
                    <div className="space-y-6">
                        <div className="relative overflow-hidden rounded-3xl bg-blue-600 p-8 text-white shadow-2xl">
                            <div className="relative flex flex-col sm:flex-row sm:items-center justify-between gap-6">
                                <div className="space-y-3">
                                    <h1 className="text-4xl sm:text-5xl font-bold text-white">
                                        Remuneração no Senado
                                    </h1>
                                    <p className="text-lg text-blue-100 max-w-2xl">
                                        Consulte e analise os dados de remuneração no Senado Federal com visualizações interativas e filtros avançados
                                    </p>
                                </div>
                                <Button
                                    onClick={() => setShowFilters(!showFilters)}
                                    variant={showFilters ? "secondary" : "outline"}
                                    className={`flex-shrink-0 px-6 py-3 rounded-xl font-semibold transition-all duration-300 ${showFilters
                                            ? "bg-white text-blue-600 hover:bg-blue-50 shadow-lg"
                                            : "bg-white/20 backdrop-blur-sm text-white border-white/30 hover:bg-white/30 hover:shadow-lg"
                                        }`}
                                    size="lg"
                                >
                                    <Search className="h-5 w-5 mr-2" />
                                    {showFilters ? "Ocultar filtros" : "Mostrar filtros"}
                                </Button>
                            </div>
                        </div>

                        {filterSummary.length > 0 && (
                            <div className="relative overflow-hidden rounded-2xl bg-blue-50 border border-blue-200 shadow-lg">
                                <div className="relative p-6">
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center gap-4">
                                            <div className="flex items-center gap-2">
                                                <div className="h-3 w-3 bg-blue-500 rounded-full animate-pulse"></div>
                                                <span className="text-sm font-semibold text-gray-800">Filtros aplicados:</span>
                                            </div>
                                            <div className="flex gap-2 flex-wrap">
                                                {filterSummary.map((filter, index) => (
                                                    <Badge
                                                        key={index}
                                                        className="text-xs px-3 py-1 bg-blue-500 text-white border-0 shadow-md hover:shadow-lg transition-all duration-300"
                                                    >
                                                        {filter}
                                                    </Badge>
                                                ))}
                                            </div>
                                        </div>
                                        <Button
                                            variant="ghost"
                                            size="sm"
                                            onClick={handleClearFilters}
                                            className="text-red-600 hover:text-red-700 hover:bg-red-50 rounded-lg transition-all duration-300 hover:shadow-md"
                                        >
                                            <Trash className="h-4 w-4 mr-1" />
                                            Limpar
                                        </Button>
                                    </div>
                                </div>
                            </div>
                        )}

                        {showFilters && (
                            <div className="relative overflow-hidden rounded-2xl bg-white border border-blue-200 shadow-xl">
                                <div className="relative p-8">
                                    <div className="space-y-6">
                                        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
                                            <div className="space-y-3">
                                                <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                    <div className="h-1 w-4 bg-primary rounded"></div>
                                                    Ano
                                                </label>
                                                <Select value={selectedFilters.ano} onValueChange={(value) => setSelectedFilters(prev => ({ ...prev, ano: value }))}>
                                                    <SelectTrigger className="h-12 bg-white border border-blue-200 rounded-lg hover:bg-gray-50 transition-all duration-300 focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500">
                                                        <SelectValue placeholder="Selecione o ano" />
                                                    </SelectTrigger>
                                                    <SelectContent>
                                                        {anos.map((ano) => (
                                                            <SelectItem key={ano.value} value={ano.value}>
                                                                {ano.label}
                                                            </SelectItem>
                                                        ))}
                                                    </SelectContent>
                                                </Select>
                                            </div>

                                            <div className="space-y-3">
                                                <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                    <div className="h-1 w-4 bg-primary rounded"></div>
                                                    Mês
                                                </label>
                                                <Select value={selectedFilters.mes} onValueChange={(value) => setSelectedFilters(prev => ({ ...prev, mes: value }))}>
                                                    <SelectTrigger className="h-12 bg-white border border-blue-200 rounded-lg hover:bg-gray-50 transition-all duration-300 focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500">
                                                        <SelectValue placeholder="Selecione o mês" />
                                                    </SelectTrigger>
                                                    <SelectContent>
                                                        {meses.map((mes) => (
                                                            <SelectItem key={mes.value} value={mes.value}>
                                                                {mes.label}
                                                            </SelectItem>
                                                        ))}
                                                    </SelectContent>
                                                </Select>
                                            </div>

                                            <div className="space-y-3">
                                                <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                    <div className="h-1 w-4 bg-primary rounded"></div>
                                                    Vínculo
                                                </label>
                                                <MultiSelectDropdown
                                                    items={vinculosData}
                                                    placeholder="Selecione vínculos"
                                                    selectedItems={selectedFilters.vinculo}
                                                    onSelectionChange={(items) => setSelectedFilters(prev => ({ ...prev, vinculo: items }))}
                                                />
                                            </div>

                                            <div className="space-y-3">
                                                <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                    <div className="h-1 w-4 bg-primary rounded"></div>
                                                    Categoria
                                                </label>
                                                <MultiSelectDropdown
                                                    items={categoriasData}
                                                    placeholder="Selecione categorias"
                                                    selectedItems={selectedFilters.categoria}
                                                    onSelectionChange={(items) => setSelectedFilters(prev => ({ ...prev, categoria: items }))}
                                                />
                                            </div>

                                            <div className="space-y-3">
                                                <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                    <div className="h-1 w-4 bg-primary rounded"></div>
                                                    Cargo
                                                </label>
                                                <MultiSelectDropdown
                                                    items={cargosData}
                                                    placeholder="Selecione cargos"
                                                    selectedItems={selectedFilters.cargo}
                                                    onSelectionChange={(items) => setSelectedFilters(prev => ({ ...prev, cargo: items }))}
                                                />
                                            </div>

                                            <div className="space-y-3">
                                                <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                    <div className="h-1 w-4 bg-primary rounded"></div>
                                                    Lotação
                                                </label>
                                                <MultiSelectDropdown
                                                    items={lotacoesData}
                                                    placeholder="Selecione lotações"
                                                    selectedItems={selectedFilters.lotacao}
                                                    onSelectionChange={(items) => setSelectedFilters(prev => ({ ...prev, lotacao: items }))}
                                                />
                                            </div>


                                            <div className="space-y-3">
                                                <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                    <div className="h-1 w-4 bg-primary rounded"></div>
                                                    Senador(a)
                                                </label>
                                                <MultiSelectDropdown
                                                    items={parlamentaresData}
                                                    placeholder="Selecione senadores"
                                                    selectedItems={selectedFilters.parlamentar}
                                                    onSelectionChange={(items) => setSelectedFilters(prev => ({ ...prev, parlamentar: items }))}
                                                />
                                            </div>

                                        </div>

                                        <div className="flex gap-3 pt-4">
                                            <Button
                                                onClick={handleSearch}
                                                className="flex-1 bg-blue-500 hover:bg-blue-600 text-white font-semibold rounded-xl shadow-lg hover:shadow-xl transition-all duration-300"
                                            >
                                                <Search className="h-5 w-5 mr-2" />
                                                Buscar
                                            </Button>
                                            <Button
                                                variant="outline"
                                                onClick={handleClearFilters}
                                                className="bg-white border border-red-200 text-red-600 hover:bg-red-500 hover:text-white hover:shadow-md transition-all duration-300 font-medium rounded-xl"
                                            >
                                                <Trash className="h-5 w-5 mr-2" />
                                                Limpar
                                            </Button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        )}

                        {/* Tabs for agrupamento */}
                        <div className="relative overflow-hidden rounded-2xl bg-gray-50 border-0 shadow-xl">
                            <div className="relative flex flex-wrap gap-2 p-4 justify-center sm:justify-between">
                                {agrupamentoOptions.map((option) => {
                                    const Icon = option.icon;
                                    const isActive = option.value === selectedFilters.agrupar;
                                    return (
                                        <Button
                                            key={option.value}
                                            variant={isActive ? "default" : "ghost"}
                                            size="sm"
                                            className={`transition-all duration-300 flex items-center gap-2 px-4 py-2 rounded-xl font-medium ${isActive
                                                ? "shadow-lg bg-blue-500 text-white hover:bg-blue-600"
                                                : "bg-white text-gray-700 hover:bg-gray-100 hover:text-gray-900 hover:shadow-md border border-gray-200"
                                                }`}
                                            onClick={() => {
                                                setSelectedFilters(prev => ({ ...prev, agrupar: option.value }));
                                                setCurrentPage(1);
                                                setSortField(null);
                                                setSortOrder('desc');
                                                delay(refetch);
                                            }}
                                        >
                                            <Icon className={`h-4 w-4 ${isActive ? "text-white" : "text-blue-600"}`} />
                                            <span className="whitespace-nowrap">{option.label}</span>
                                        </Button>
                                    );
                                })}
                            </div>
                        </div>

                        <div className="relative overflow-hidden rounded-2xl bg-white border border-blue-200 shadow-xl">
                            <div className="relative p-8">
                                <div className="space-y-6">
                                    <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                                        <div className="space-y-2">
                                            <h2 className="text-2xl font-bold text-blue-600">Resultados</h2>
                                            <p className="text-gray-600">
                                                {remuneracaoData.length > 0 && (
                                                    <>
                                                        <span className="font-semibold text-blue-600">{formatNumber(apiData?.recordsTotal || 0)}</span> registros encontrados
                                                        {valorTotal > 0 && (
                                                            <> • <span className="font-semibold text-green-600">Custo total: {formatCurrency(valorTotal)}</span></>
                                                        )}
                                                    </>
                                                )}
                                            </p>
                                        </div>

                                        <div className="flex items-center gap-3 bg-blue-50 px-4 py-2 rounded-xl border border-blue-200">
                                            <span className="text-sm font-medium text-gray-700">Agrupando por</span>
                                            <span className="text-sm font-bold text-blue-600">
                                                {agrupamentoOptions.find(option => option.value === activeAgrupamento)?.label}
                                            </span>
                                        </div>
                                    </div>

                                    <div className="rounded-xl overflow-hidden border border-blue-200 shadow-lg bg-white">
                                        <Table>
                                            <TableHeader>
                                                <TableRow className="bg-blue-50 border-b border-blue-200">
                                                    {columns.map((column) => {
                                                        return (
                                                            <TableHead
                                                                key={column.key}
                                                                className={`${column.align === 'right' ? 'text-right' : ''} ${column.sortable ? 'cursor-pointer hover:bg-blue-100 transition-all duration-300' : ''} font-bold text-gray-800 ${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}
                                                                onClick={() => column.sortable && handleSort(column.columnIndex)}
                                                            >
                                                                {column.label && (
                                                                    <div className={`flex items-center gap-2 ${column.align === 'right' ? 'justify-end' : ''}`}>
                                                                        <span className="text-sm">{column.label}</span>
                                                                        {column.sortable && <SortIcon field={column.columnIndex} />}
                                                                    </div>
                                                                )}
                                                            </TableHead>
                                                        );
                                                    })}
                                                </TableRow>
                                            </TableHeader>
                                            <TableBody>
                                                {remuneracaoData.length > 0 ? (
                                                    remuneracaoData.map((row, index) => (
                                                        <TableRow key={index} className="hover:bg-blue-50 transition-all duration-300 border-b border-gray-100">
                                                            {columns.map((column) => {
                                                                if (column.key === 'acoes' && activeAgrupamento !== '6') {
                                                                    return (
                                                                        <TableCell key={column.key} className={`${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}>
                                                                            <div className="flex gap-2">
                                                                                <Button
                                                                                    size="sm"
                                                                                    className="bg-blue-500 hover:bg-blue-600 text-white font-medium rounded-lg shadow-md hover:shadow-lg transition-all duration-300 hidden sm:flex"
                                                                                    onClick={() => openModal(row)}
                                                                                >
                                                                                    Detalhar
                                                                                </Button>
                                                                                <Button
                                                                                    size="sm"
                                                                                    variant="outline"
                                                                                    className="sm:hidden bg-white border border-blue-200 hover:bg-blue-500 hover:text-white hover:shadow-md transition-all duration-300 rounded-lg"
                                                                                    onClick={() => openModal(row)}
                                                                                    title="Detalhar"
                                                                                >
                                                                                    <Plus className="h-4 w-4" />
                                                                                </Button>
                                                                            </div>
                                                                        </TableCell>
                                                                    );
                                                                }

                                                                let cellContent = row[column.key];

                                                                if (cellContent) {
                                                                    if (column.key === 'categoria') {
                                                                        return (
                                                                            <TableCell key={column.key} className={`${column.align === 'right' ? 'text-right whitespace-nowrap font-mono' : ''} font-medium ${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}>
                                                                                {formatNameAcronym(cellContent, row.simbolo_funcao)}<br/>
                                                                                <small>{formatNameAcronym(cellContent, row.referencia_cargo)}</small>
                                                                            </TableCell>
                                                                        )
                                                                        // if (column.key === 'categoria') {
                                                                        //     cellContent = formatNameAcronym(cellContent, row.simbolo_funcao)
                                                                        // } else if (column.key === 'cargo') {
                                                                        //     cellContent =  formatNameAcronym(cellContent, row.referencia_cargo)
                                                                    } else if (column.key === 'valor_total') {
                                                                        if (selectedFilters.agrupar == "6") { // Sem agrupamento
                                                                            return (
                                                                                <TableCell key={column.key} className={`${column.align === 'right' ? 'text-right whitespace-nowrap font-mono' : ''} font-medium ${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}>
                                                                                    <Link
                                                                                        to={`./${row.id}`}
                                                                                        className="text-primary hover:text-primary/80 transition-colors font-bold font-mono"
                                                                                    >
                                                                                        R$ {cellContent}
                                                                                    </Link>
                                                                                </TableCell>
                                                                            );
                                                                        }

                                                                        cellContent = `R$ ${cellContent}`;
                                                                    } else if (column.key === 'descricao' && selectedFilters.agrupar == "7") { // Senador
                                                                        return (
                                                                            <TableCell key={column.key} className={`${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}>
                                                                                <Link
                                                                                    to={`/senador/${row.id}`}
                                                                                    className="text-primary hover:text-primary/80 transition-colors font-medium"
                                                                                >
                                                                                    {cellContent}
                                                                                </Link>
                                                                            </TableCell>
                                                                        );
                                                                    }
                                                                }

                                                                return (
                                                                    <TableCell key={column.key} className={`${column.align === 'right' ? 'text-right whitespace-nowrap font-mono' : ''} font-medium ${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}>
                                                                        {cellContent}
                                                                    </TableCell>
                                                                );
                                                            })}
                                                        </TableRow>
                                                    ))
                                                ) : (
                                                    <TableRow>
                                                        <TableCell colSpan={columns.length} className="text-center py-8">
                                                            <div className="text-muted-foreground">
                                                                {isLoading ? 'Carregando...' : 'Nenhum resultado encontrado'}
                                                            </div>
                                                        </TableCell>
                                                    </TableRow>
                                                )}
                                            </TableBody>
                                        </Table>
                                    </div>

                                    {remuneracaoData.length > 0 && (() => {
                                        const totalPages = Math.ceil((apiData?.recordsTotal || 0) / itemsPerPage);
                                        return totalPages > 1 && (
                                            <div className="relative overflow-hidden rounded-xl bg-gray-50 border border-blue-200 shadow-lg">
                                                <div className="relative p-6">
                                                    <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
                                                        <div className="text-sm font-medium text-gray-700 bg-white px-4 py-2 rounded-lg border border-blue-200">
                                                            Mostrando <span className="font-bold text-blue-600">{((currentPage - 1) * itemsPerPage) + 1}</span> a <span className="font-bold text-blue-600">{Math.min(currentPage * itemsPerPage, apiData?.recordsTotal || 0)}</span> de <span className="font-bold text-blue-600">{formatNumber(apiData?.recordsTotal || 0)}</span> resultados
                                                        </div>
                                                        <div className="flex items-center gap-2">
                                                            <Button
                                                                variant="outline"
                                                                size="sm"
                                                                onClick={() => handlePageChange(1)}
                                                                disabled={currentPage <= 1}
                                                                className="h-9 px-4 rounded-lg bg-white border border-blue-200 hover:bg-blue-500 hover:text-white hover:shadow-md transition-all duration-300 font-medium"
                                                            >
                                                                Primeira
                                                            </Button>
                                                            <Button
                                                                variant="outline"
                                                                size="sm"
                                                                onClick={() => handlePageChange(currentPage - 1)}
                                                                disabled={currentPage <= 1}
                                                                className="h-9 px-3 rounded-lg bg-white border border-blue-200 hover:bg-blue-500 hover:text-white hover:shadow-md transition-all duration-300"
                                                            >
                                                                <ChevronUpIcon className="w-4 h-4 rotate-270" />
                                                            </Button>
                                                            <div className="flex items-center gap-1 mx-3">
                                                                {(() => {
                                                                    const startPage = Math.max(1, currentPage - 3);
                                                                    const endPage = Math.min(totalPages, currentPage + 3);
                                                                    const pages = [];

                                                                    for (let i = startPage; i <= endPage; i++) {
                                                                        pages.push(i);
                                                                    }

                                                                    return pages.map((page) => (
                                                                        <Button
                                                                            key={page}
                                                                            variant={currentPage === page ? "default" : "outline"}
                                                                            size="sm"
                                                                            onClick={() => handlePageChange(page)}
                                                                            className={`min-w-9 h-9 rounded-lg font-semibold transition-all duration-300 ${currentPage === page
                                                                                    ? "bg-blue-500 text-white shadow-lg"
                                                                                    : "bg-white border border-blue-200 hover:bg-blue-500 hover:text-white hover:shadow-md"
                                                                                }`}
                                                                        >
                                                                            {page}
                                                                        </Button>
                                                                    ));
                                                                })()}
                                                            </div>
                                                            <Button
                                                                variant="outline"
                                                                size="sm"
                                                                onClick={() => handlePageChange(currentPage + 1)}
                                                                disabled={currentPage >= totalPages}
                                                                className="h-9 px-3 rounded-lg bg-white border border-blue-200 hover:bg-blue-500 hover:text-white hover:shadow-md transition-all duration-300"
                                                            >
                                                                <ChevronDownIcon className="w-4 h-4 rotate-90" />
                                                            </Button>
                                                            <Button
                                                                variant="outline"
                                                                size="sm"
                                                                onClick={() => handlePageChange(totalPages)}
                                                                disabled={currentPage >= totalPages}
                                                                className="h-9 px-4 rounded-lg bg-white border border-blue-200 hover:bg-blue-500 hover:text-white hover:shadow-md transition-all duration-300 font-medium"
                                                            >
                                                                Última
                                                            </Button>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        );
                                    })()}
                                </div>
                            </div>
                        </div>
                    </div>
                </main>
            </div>

            <Dialog open={isModalOpen} onOpenChange={setIsModalOpen}>
                <DialogContent className="sm:max-w-[550px] border-0 shadow-2xl bg-white">
                    <div className="relative">
                        <DialogHeader className="pb-6 border-b border-blue-200">
                            <DialogTitle className="text-2xl font-bold text-blue-600 flex items-center gap-3">
                                <div className="h-2 w-6 bg-blue-500 rounded-full"></div>
                                Agrupar por
                            </DialogTitle>
                        </DialogHeader>
                        <div className="grid gap-3 py-6">
                            {agrupamentoOptions.map((option) => {
                                const Icon = option.icon;
                                const isActive = option.value === selectedFilters.agrupar;
                                return (
                                    <Button
                                        key={option.value}
                                        variant={isActive ? "default" : "ghost"}
                                        className={`justify-start h-auto p-4 text-left transition-all duration-300 rounded-xl ${isActive
                                                ? "bg-blue-500 text-white shadow-lg"
                                                : "bg-white border border-blue-200 hover:bg-blue-50 hover:border-blue-300 hover:shadow-md"
                                            }`}
                                        onClick={() => handleAgrupamentoChange(option.value)}
                                    >
                                        <div className="flex items-center justify-between w-full">
                                            <div className="flex items-center gap-3">
                                                <Icon className={`h-5 w-5 ${isActive ? "text-white" : "text-blue-600"}`} />
                                                <div>
                                                    <div className={`font-semibold ${isActive ? "text-white" : "text-gray-800"}`}>{option.label}</div>
                                                    {isActive && (
                                                        <div className="text-sm text-white/90 font-medium mt-1">
                                                            ✓ Atualmente selecionado
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                            {isActive && (
                                                <div className="h-3 w-3 bg-white rounded-full shadow-lg"></div>
                                            )}
                                        </div>
                                    </Button>
                                );
                            })}
                        </div>
                    </div>
                </DialogContent>
            </Dialog>

            <Footer />
        </>
    );
}
