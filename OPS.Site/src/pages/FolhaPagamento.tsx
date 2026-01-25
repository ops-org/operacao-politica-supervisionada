import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { ErrorMessage } from "@/components/ErrorMessage";
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
import { ChevronUpIcon, ChevronDownIcon, Search, Trash, Building2, Users, Calendar, Plus, Briefcase, User, FolderOpen, Link2, DollarSign, FileText, UserCheck } from "lucide-react";
import { apiClient, fetchRemuneracao, fetchVinculos, fetchCategorias, fetchCargos, fetchLotacoes, fetchParliamentMembers, fetchGruposFuncionais, fetchFuncionarios, RemuneracaoData, RemuneracaoApiResponse, DropDownOptions, ParliamentSearchRequest } from "@/lib/api";
import { delay } from "@/lib/utils";
import { Alert, AlertDescription } from "@/components/ui/alert";

const formatNameAcronym = (value: string, acronym: string): string => {
    if (!acronym) return value;
    return String(`${value} (${acronym})`);
}

const typeConfigs = {
    "deputado-federal": {
        title: "Remuneração - Câmara dos Deputados",
        subtitle: "Consulte e analise os dados de remuneração dos deputados federais",
        apiType: "deputado",
        detailRoute: "/deputado-federal",
        imageBaseUrl: "//static.ops.org.br/deputado",
        defaultAno: (new Date().getFullYear() - 1).toString()
    },
    "senador": {
        title: "Remuneração no Senado",
        subtitle: "Consulte e analise os dados de remuneração no Senado Federal com visualizações interativas e filtros avançados",
        apiType: "senador",
        detailRoute: "/senador",
        imageBaseUrl: "//static.ops.org.br/senador",
        defaultAno: (new Date().getFullYear() - 1).toString()
    }
} as const;

const generateAnos = () => {
    const currentYear = new Date().getFullYear()-1; // TODO: Remove -1
    const anos = [{ value: null, label: "Selecione" }];
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

const getAgrupamentoOptions = (type?: "deputado-federal" | "senador") => {
    if (type === "deputado-federal") {
        return [
            { value: "1", label: "Grupo Funcional", icon: Users },
            { value: "3", label: "Deputado(a)", icon: User },
            { value: "4", label: "Funcionário(a)", icon: UserCheck },
            { value: "5", label: "Ano", icon: Calendar },
            { value: "6", label: "Não Agrupar", icon: FileText }
        ];
    }
    
    // Senador options
    return [
        { value: "1", label: "Lotação", icon: Building2 },
        { value: "2", label: "Cargo", icon: Briefcase },
        { value: "3", label: "Categoria", icon: FolderOpen },
        { value: "4", label: "Vinculo", icon: Link2 },
        { value: "7", label: "Senador(a)", icon: User },
        { value: "5", label: "Ano", icon: Calendar },
        { value: "6", label: "Folha de pagamento", icon: FileText }
    ];
};

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

const getColumnConfigs = (agrupamento: string, type?: "deputado-federal" | "senador"): ColumnConfig[] => {
    if (type === "deputado-federal") {
        switch (agrupamento) {
            case "1": // Grupo Funcional
            case "3": // Deputado
            case "4": // Funcionário
            case "5": // Ano
                return [
                    { key: "acoes", label: "", sortable: false, columnIndex: 0 },
                    { key: "descricao", label: agrupamento === "1" ? "Grupo Funcional" : agrupamento === "3" ? "Deputado(a)" : agrupamento === "4" ? "Funcionário(a)" : "Ano", sortable: true, columnIndex: 1 },
                    { key: "quantidade", label: "Flh(s). Pgto.", sortable: true, align: 'right', columnIndex: 2 },
                    { key: "valor_total", label: "Custo Total", sortable: true, align: 'right', columnIndex: 3 }
                ];
            case "6": // Não agrupar (detalhes)
                return [
                    { key: "deputado", label: "Deputado", sortable: true, columnIndex: 0 },
                    { key: "funcionario", label: "Funcionário", sortable: true, columnIndex: 1 },
                    { key: "ano_mes", label: "Ano/Mês", sortable: true, columnIndex: 2 },
                    { key: "valor_bruto", label: "Valor Bruto", sortable: true, align: 'right', columnIndex: 3 },
                    { key: "valor_outros", label: "Vantagens", sortable: true, align: 'right', columnIndex: 4 },
                    { key: "valor_total", label: "Custo Total", sortable: true, align: 'right', columnIndex: 5 }
                ];
            default:
                return getColumnConfigs("1", type);
        }
    }

    // Senador configurations (existing logic)
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
                { key: "lotacao", label: "Lotação", sortable: true, columnIndex: 3 },
                { key: "tipo_folha", label: "Folha", sortable: true, columnIndex: 4 },
                { key: "ano_mes", label: "Ano/Mês", sortable: true, columnIndex: 5 },
                { key: "valor_total", label: "Custo Total", sortable: true, align: 'right', columnIndex: 6 }
            ];
        default:
            return getColumnConfigs("7", type);
    }
};

export default function FolhaPagamento({ type }: { type?: "deputado-federal" | "senador" }) {
    const config = type ? typeConfigs[type] : typeConfigs["senador"];
    const [showFilters, setShowFilters] = useState(false);
    const [searchParams, setSearchParams] = useSearchParams();
    const [selectedFilters, setSelectedFilters] = useState({
        ano: config.defaultAno,
        mes: "",
        vinculo: [] as string[],
        categoria: [] as string[],
        cargo: [] as string[],
        lotacao: [] as string[],
        parlamentar: [] as string[],
        grupo_funcional: [] as string[],
        funcionario: [] as string[],
        agrupar: type === "deputado-federal" ? "1" : "7"
    });

    const [currentPage, setCurrentPage] = useState(1);
    const [sortField, setSortField] = useState<number | null>(null);
    const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedRow, setSelectedRow] = useState<RemuneracaoData | null>(null);
    const [activeAgrupamento, setActiveAgrupamento] = useState("7");
    const itemsPerPage = 50;

    const columns = getColumnConfigs(activeAgrupamento, type);
    const agrupamentoOptions = getAgrupamentoOptions(type);

    const getApiFilters = () => ({
        ag: selectedFilters.agrupar,
        an: selectedFilters.ano,
        ms: selectedFilters.mes,
        vn: selectedFilters.vinculo.join(","),
        ct: selectedFilters.categoria.join(","),
        cr: selectedFilters.cargo.join(","),
        lt: selectedFilters.lotacao.join(","),
        df: selectedFilters.parlamentar.join(","),
        gf: selectedFilters.grupo_funcional.join(","),
        sc: selectedFilters.funcionario.join(",")
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
        queryFn: () => fetchRemuneracao(currentPage, itemsPerPage, sortField, sortOrder, getApiFilters(), type),
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

        if (type === "deputado-federal") {
            switch (selectedFilters.agrupar) {
                case "1": // Grupo Funcional
                    updatedFilters.grupo_funcional = [selectedRow.id?.toString() || ""];
                    break;
                case "3": // Deputado
                    updatedFilters.parlamentar = [selectedRow.id?.toString() || ""];
                    break;
                case "4": // Funcionário
                    updatedFilters.funcionario = [selectedRow.id?.toString() || ""];
                    break;
                case "5": // Ano
                    updatedFilters.ano = selectedRow.descricao || "";
                    break;
            }
        } else {
            // Senador logic (existing)
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
            grupo_funcional: [],
            funcionario: [],
            agrupar: type === "deputado-federal" ? "1" : "7"
        });

        setSearchParams({});
        setCurrentPage(1);
        setSortField(null);
        setSortOrder('desc');

        delay(refetch);
    };

    // Only show filter options for deputado-federal type
    const { data: gruposFuncionaisData = [] } = useQuery({
        queryKey: ["grupos-funcionais"],
        queryFn: () => fetchGruposFuncionais(),
        staleTime: 60 * 60 * 1000,
        enabled: type === "deputado-federal"
    });

    const { data: funcionariosData = [] } = useQuery({
        queryKey: ["funcionarios", selectedFilters.ano, selectedFilters.mes],
        queryFn: () => fetchFuncionarios(selectedFilters.ano),
        staleTime: 10 * 60 * 1000,
        enabled: type === "deputado-federal" && !!selectedFilters.ano
    });

    // Only show filter options for senador type
    const { data: vinculosData = [] } = useQuery({
        queryKey: ["vinculos"],
        queryFn: () => fetchVinculos(),
        staleTime: 60 * 60 * 1000,
        enabled: type === "senador"
    });

    const { data: categoriasData = [] } = useQuery({
        queryKey: ["categorias"],
        queryFn: () => fetchCategorias(),
        staleTime: 60 * 60 * 1000,
        enabled: type === "senador"
    });

    const { data: cargosData = [] } = useQuery({
        queryKey: ["cargos"],
        queryFn: () => fetchCargos(),
        staleTime: 60 * 60 * 1000,
        enabled: type === "senador"
    });

    const { data: lotacoesData = [] } = useQuery({
        queryKey: ["lotacoes"],
        queryFn: () => fetchLotacoes(),
        staleTime: 60 * 60 * 1000,
        enabled: type === "senador"
    });

    const { data: parlamentaresData = [] } = useQuery({
        queryKey: ["parlamentares", type, selectedFilters.ano, selectedFilters.mes],
        queryFn: () => fetchParliamentMembers(type, "", selectedFilters.ano),
        staleTime: 10 * 60 * 1000,
        enabled: !!type
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
            filters.push(`${selectedFilters.parlamentar.length} ${type === "deputado-federal" ? "deputado(s)" : "senador(es)"}`);

        return filters;
    };

    const filterSummary = getFilterSummary();

    if (error) {
        return (
            <div className="min-h-screen bg-background">
                <Header />
                <ErrorMessage
                    onRetry={() => refetch()}
                />
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
                        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                            <div>
                                <h1 className="text-3xl font-bold text-foreground mb-2">{config.title}</h1>
                                <p className="text-muted-foreground">{config.subtitle}</p>
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

                        {filterSummary.length > 0 && (
                            <Card className="border-l-4 border-l-primary">
                                <CardContent className="pt-6">
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center gap-3">
                                            <div className="h-2 w-2 bg-primary rounded-full"></div>
                                            <span className="text-sm font-medium text-foreground">Filtros aplicados:</span>
                                            <div className="flex gap-2 flex-wrap">
                                                {filterSummary.map((filter, index) => (
                                                    <Badge key={index} variant="secondary" className="text-xs">
                                                        {filter}
                                                    </Badge>
                                                ))}
                                            </div>
                                        </div>
                                        <Button
                                            variant="ghost"
                                            size="sm"
                                            onClick={handleClearFilters}
                                            className="text-muted-foreground hover:text-foreground"
                                        >
                                            <Trash className="h-4 w-4 mr-1" />
                                            Limpar
                                        </Button>
                                    </div>
                                </CardContent>
                            </Card>
                        )}

                        {showFilters && (
                            <Card className="border-0 shadow-lg">
                                <CardContent className="p-6">
                                    <div className="space-y-6">
                                        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
                                            <div className="space-y-3">
                                                <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                    <div className="h-1 w-4 bg-primary rounded"></div>
                                                    Ano
                                                </label>
                                                <Select value={selectedFilters.ano} onValueChange={(value) => setSelectedFilters(prev => ({ ...prev, ano: value }))}>
                                                    <SelectTrigger className="h-11">
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

                                            {type === "deputado-federal" && (
                                                <div className="space-y-3">
                                                    <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                        <div className="h-1 w-4 bg-primary rounded"></div>
                                                        Mês
                                                    </label>
                                                    <Select value={selectedFilters.mes} onValueChange={(value) => setSelectedFilters(prev => ({ ...prev, mes: value }))}>
                                                        <SelectTrigger className="h-11">
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
                                            )}

                                            <div className="space-y-3">
                                                <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                    <div className="h-1 w-4 bg-primary rounded"></div>
                                                    {type === "deputado-federal" ? "Deputado(a)" : "Senador(a)"}
                                                </label>
                                                <MultiSelectDropdown
                                                    items={parlamentaresData}
                                                    placeholder={`Selecione ${type === "deputado-federal" ? "deputados" : "senadores"}`}
                                                    selectedItems={selectedFilters.parlamentar}
                                                    onSelectionChange={(items) => setSelectedFilters(prev => ({ ...prev, parlamentar: items }))}
                                                />
                                            </div>
                                        </div>

                                        {type === "deputado-federal" && (
                                            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
                                                <div className="space-y-3">
                                                    <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                        <div className="h-1 w-4 bg-primary rounded"></div>
                                                        Grupo Funcional
                                                    </label>
                                                    <MultiSelectDropdown
                                                        items={gruposFuncionaisData}
                                                        placeholder="Selecione grupos funcionais"
                                                        selectedItems={selectedFilters.grupo_funcional}
                                                        onSelectionChange={(items) => setSelectedFilters(prev => ({ ...prev, grupo_funcional: items }))}
                                                    />
                                                </div>

                                                <div className="space-y-3">
                                                    <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                        <div className="h-1 w-4 bg-primary rounded"></div>
                                                        Funcionário(a)
                                                    </label>
                                                    <MultiSelectDropdown
                                                        items={funcionariosData}
                                                        placeholder="Selecione funcionários"
                                                        selectedItems={selectedFilters.funcionario}
                                                        onSelectionChange={(items) => setSelectedFilters(prev => ({ ...prev, funcionario: items }))}
                                                    />
                                                </div>
                                            </div>
                                        )}

                                        {type === "senador" && (
                                            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
                                                <div className="space-y-3">
                                                    <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                        <div className="h-1 w-4 bg-primary rounded"></div>
                                                        Mês
                                                    </label>
                                                    <Select value={selectedFilters.mes} onValueChange={(value) => setSelectedFilters(prev => ({ ...prev, mes: value }))}>
                                                        <SelectTrigger className="h-11">
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
                                            </div>
                                        )}

                                        <div className="border-t pt-6">
                                            <div className="space-y-4">
                                                <label className="text-sm font-semibold text-foreground flex items-center gap-2">
                                                    <div className="h-1 w-4 bg-primary rounded"></div>
                                                    Agrupar por
                                                </label>
                                                <RadioGroup
                                                    value={selectedFilters.agrupar}
                                                    onValueChange={(value) => setSelectedFilters(prev => ({ ...prev, agrupar: value }))}
                                                    className={`grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-7 gap-3`}
                                                >
                                                    {agrupamentoOptions.map((option) => {
                                                        const Icon = option.icon;
                                                        return (
                                                            <div className="relative" key={option.value}>
                                                                <RadioGroupItem
                                                                    value={option.value}
                                                                    id={`agrupamento-${option.value}`}
                                                                    className="peer sr-only"
                                                                />
                                                                <Label
                                                                    htmlFor={`agrupamento-${option.value}`}
                                                                    className="flex flex-col items-center justify-center rounded-lg border-2 border-muted bg-popover p-3 hover:bg-accent hover:text-accent-foreground peer-data-[state=checked]:border-primary peer-data-[state=checked]:bg-primary/10 peer-data-[state=checked]:text-primary cursor-pointer transition-all duration-200"
                                                                >
                                                                    <Icon className="h-4 w-4 mb-1" />
                                                                    <div className="text-xs font-medium text-center">{option.label}</div>
                                                                </Label>
                                                            </div>
                                                        );
                                                    })}
                                                </RadioGroup>
                                            </div>
                                        </div>

                                        <div className="flex gap-3 pt-4 border-t">
                                            <Button onClick={handleSearch} size="lg" className="flex-1 sm:flex-none">
                                                <Search className="h-4 w-4 mr-2" />
                                                Pesquisar
                                            </Button>
                                            <Button variant="outline" onClick={handleClearFilters} size="lg" className="flex-1 sm:flex-none">
                                                <Trash className="h-4 w-4 mr-2" />
                                                Limpar filtros
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
                                {remuneracaoData.length > 0 && (
                                    <p className="text-sm text-muted-foreground mt-1">
                                        {formatNumber(apiData?.recordsTotal || 0)} resultado{apiData?.recordsTotal !== 1 ? 's' : ''} encontrado{apiData?.recordsTotal !== 1 ? 's' : ''}
                                        {valorTotal > 0 && (
                                            <> • Custo total: <span className="font-semibold text-primary">{formatCurrency(valorTotal)}</span></>
                                        )}
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

                        <Card className="border-0 shadow-lg overflow-hidden">
                            <div className="overflow-x-auto">
                                <Table key={activeAgrupamento}>
                                    <TableHeader className="bg-muted/50">
                                        <TableRow>
                                            {columns.map((column) => {
                                                return (
                                                    <TableHead
                                                        key={column.key}
                                                        className={`${column.align === 'right' ? 'text-right' : ''} ${column.sortable ? 'cursor-pointer hover:bg-muted/80 transition-colors' : ''} font-semibold text-foreground ${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}
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
                                        {remuneracaoData.length > 0 ? (
                                            remuneracaoData.map((row, index) => (
                                                <TableRow key={index} className="hover:bg-muted/50 transition-colors">
                                                    {columns.map((column) => {
                                                        if (column.key === 'acoes' && activeAgrupamento !== '6') {
                                                            return (
                                                                <TableCell key={column.key} className={`${column.hideOnSmallScreen ? 'hidden sm:table-cell' : ''}`}>
                                                                    <div className="flex gap-2">
                                                                        <Button
                                                                            size="sm"
                                                                            className="bg-primary hover:bg-primary/90 transition-colors hidden sm:flex"
                                                                            onClick={() => openModal(row)}
                                                                        >
                                                                            Detalhar
                                                                        </Button>
                                                                        <Button
                                                                            size="sm"
                                                                            variant="outline"
                                                                            className="sm:hidden hover:bg-primary hover:text-primary-foreground transition-colors"
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
                                                                );
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
                        </Card>

                        {remuneracaoData.length > 0 && (() => {
                            const totalPages = Math.ceil((apiData?.recordsTotal || 0) / itemsPerPage);
                            return totalPages > 1 && (
                                <Card className="border-0 shadow-lg">
                                    <CardContent className="p-4">
                                        <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
                                            <div className="text-sm text-muted-foreground">
                                                Mostrando {((currentPage - 1) * itemsPerPage) + 1} a {Math.min(currentPage * itemsPerPage, apiData?.recordsTotal || 0)} de {formatNumber(apiData?.recordsTotal || 0)} resultados
                                            </div>
                                            <div className="flex items-center gap-1">
                                                <Button
                                                    variant="outline"
                                                    size="sm"
                                                    onClick={() => handlePageChange(1)}
                                                    disabled={currentPage <= 1}
                                                    className="h-8 px-3"
                                                >
                                                    Primeira
                                                </Button>
                                                <Button
                                                    variant="outline"
                                                    size="sm"
                                                    onClick={() => handlePageChange(currentPage - 1)}
                                                    disabled={currentPage <= 1}
                                                    className="h-8 px-3"
                                                >
                                                    <ChevronUpIcon className="w-4 h-4 rotate-270" />
                                                </Button>
                                                <div className="flex items-center gap-1 mx-2">
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
                                                                className={`min-w-8 h-8 ${currentPage === page ? "shadow-lg" : ""}`}
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
                                                    className="h-8 px-3"
                                                >
                                                    <ChevronDownIcon className="w-4 h-4 rotate-90" />
                                                </Button>
                                                <Button
                                                    variant="outline"
                                                    size="sm"
                                                    onClick={() => handlePageChange(totalPages)}
                                                    disabled={currentPage >= totalPages}
                                                    className="h-8 px-3"
                                                >
                                                    Última
                                                </Button>
                                            </div>
                                        </div>
                                    </CardContent>
                                </Card>
                            );
                        })()}
                    </div>
                </main>

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
                                        variant={option.value === selectedFilters.agrupar ? "default" : "outline"}
                                        className={`justify-start h-auto p-4 text-left transition-all ${option.value === selectedFilters.agrupar
                                            ? "shadow-lg border-primary bg-primary/10"
                                            : "hover:bg-muted/50 hover:border-primary/50"
                                            }`}
                                        onClick={() => handleAgrupamentoChange(option.value)}
                                    >
                                        <div className="flex items-center justify-between w-full">
                                            <div className="flex items-center gap-3">
                                                <Icon className={`h-5 w-5 ${option.value === selectedFilters.agrupar ? "text-primary" : "text-muted-foreground"}`} />
                                                <div>
                                                    <div className="font-medium text-foreground">{option.label}</div>
                                                    {option.value === selectedFilters.agrupar && (
                                                        <div className="text-sm text-primary font-medium mt-1">
                                                            ✓ Atualmente selecionado
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                            {option.value === selectedFilters.agrupar && (
                                                <div className="h-2 w-2 bg-primary rounded-full"></div>
                                            )}
                                        </div>
                                    </Button>
                                );
                            })}
                        </div>
                    </DialogContent>
                </Dialog>

                <Footer />
            </div>
        </>
    );
}
