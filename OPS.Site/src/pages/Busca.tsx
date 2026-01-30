import { useState, useEffect } from "react";
import { useSearchParams, Link } from "react-router-dom";
import { usePageTitle } from "@/hooks/usePageTitle";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { Card, CardContent } from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Search, Users, Building2, MapPin, DollarSign } from "lucide-react";
import { apiClient } from "@/lib/api";

interface Senador {
    id_sf_senador: number;
    nome_parlamentar: string;
    nome_civil: string;
    sigla_partido: string;
    nome_partido: string;
    sigla_estado: string;
    nome_estado: string;
    valor_total_ceaps: string;
    situacao: string;
    ativo: boolean;
    valor_total_remuneracao: string;
}

interface Deputado {
    id_cf_deputado: number;
    nome_parlamentar: string;
    nome_civil: string;
    sigla_partido: string;
    nome_partido: string;
    sigla_estado: string;
    nome_estado: string;
    valor_total_ceap: string;
    situacao: string;
    ativo: boolean;
    valor_total_remuneracao: string;
}

interface DeputadoEstadual {
    id_cl_deputado: number;
    nome_parlamentar: string;
    nome_civil: string;
    sigla_partido: string;
    nome_partido: string;
    sigla_estado: string;
    nome_estado: string;
    valor_total_ceap: string;
    valor_total_verba_gabinete: string;
    situacao: string;
    ativo: boolean;
}

interface Fornecedor {
    id_fornecedor: string;
    cnpj: string;
    nome_fantasia: string;
    nome: string;
    estado: string;
    valor_total_ceap: string;
}

interface BuscaResponse {
    deputado_federal: Deputado[];
    deputado_estadual: DeputadoEstadual[];
    senador: Senador[];
    fornecedor: Fornecedor[];
}

const pluralize = (count: number, singular: string, plural: string) => {
    return count === 1 ? singular : plural;
};

const Busca = () => {
    usePageTitle("Busca Geral");
    const [searchParams, setSearchParams] = useSearchParams();
    const [query, setQuery] = useState(searchParams.get("q") || "");
    const [results, setResults] = useState<BuscaResponse>({
        deputado_federal: [],
        deputado_estadual: [],
        senador: [],
        fornecedor: [],
    });
    const [loading, setLoading] = useState(false);
    const [searched, setSearched] = useState(false);
    const [activeTab, setActiveTab] = useState("all");

    const buscar = async (searchTerm?: string) => {
        const queryToUse = searchTerm || query;

        if (!queryToUse.trim()) {
            setResults({
                deputado_federal: [],
                deputado_estadual: [],
                senador: [],
                fornecedor: [],
            });
            setSearched(false);
            return;
        }

        setLoading(true);
        setSearched(true);

        try {
            console.log("busca", queryToUse)
            const response = await apiClient.get<BuscaResponse>(`/inicio/busca?value=${encodeURIComponent(queryToUse)}`);
            setResults(response);
        } catch (error) {
            console.error("Erro na busca:", error);
            setResults({
                deputado_federal: [],
                deputado_estadual: [],
                senador: [],
                fornecedor: [],
            });
        } finally {
            setLoading(false);
        }
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        setSearchParams({ q: query });
        buscar();
    };

    useEffect(() => {
        const q = searchParams.get("q");
        console.log("searchParams", q, query)
        if (q != query) {
            setQuery(q);
        }

        if (q && q.trim()) {
            buscar(q);
        }
    }, [searchParams.get("q")]);

    const hasResults = results.deputado_federal.length > 0 || results.deputado_estadual.length > 0 || results.senador.length > 0 || results.fornecedor.length > 0;
    const noResults = searched && !hasResults && !loading;

    const totalResults = results.senador.length + results.deputado_federal.length + results.deputado_estadual.length + results.fornecedor.length;
    const showTabs = hasResults && totalResults > 0;

    return (
        <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
            <Header />
            <main className="container mx-auto px-4 py-8">
                {/* Custom Header Layout */}
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-12">
                    <div>
                        <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent mb-2">
                            Busca por Parlamentar ou Fornecedor
                        </h1>
                        <p className="text-muted-foreground text-lg">
                            Encontre os deputados, senadores e empresas que interagem com o poder público.
                        </p>
                    </div>
                </div>

                {/* Search Form */}
                <Card className="mb-12 shadow-lg border-0 bg-card/80 backdrop-blur-sm">
                    <CardContent className="p-8">
                        <form onSubmit={handleSubmit} className="space-y-6">
                            <div className="flex gap-4">
                                <div className="relative flex-1">
                                    <Search className="absolute left-4 top-1/2 transform -translate-y-1/2 text-muted-foreground w-5 h-5" />
                                    <Input
                                        type="text"
                                        placeholder="Buscar por deputado, senador ou empresa..."
                                        value={query}
                                        onChange={(e) => setQuery(e.target.value)}
                                        className="w-full pl-12 pr-4 py-4 text-lg border-2 focus:border-primary transition-colors"
                                    />
                                </div>
                                <Button
                                    type="submit"
                                    disabled={loading}
                                    className="px-8 py-4 text-lg font-semibold bg-gradient-to-r from-primary to-primary/80 hover:from-primary/90 hover:to-primary/70 transition-all transform hover:scale-105 whitespace-nowrap"
                                >
                                    {loading ? (
                                        <div className="flex items-center gap-2">
                                            <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                                            Pesquisando...
                                        </div>
                                    ) : (
                                        <div className="flex items-center gap-2">
                                            <Search className="w-5 h-5" />
                                            Pesquisar
                                        </div>
                                    )}
                                </Button>
                            </div>
                        </form>
                    </CardContent>
                </Card>

                {/* No Results Alert */}
                {noResults && (
                    <div className="text-center py-12">
                        <div className="inline-flex items-center justify-center w-16 h-16 bg-muted rounded-full mb-4">
                            <Search className="w-8 h-8 text-muted-foreground" />
                        </div>
                        <Alert className="max-w-md mx-auto border-0 bg-muted/50">
                            <AlertDescription className="text-center">
                                Nenhum resultado encontrado para "{query}"
                            </AlertDescription>
                        </Alert>
                    </div>
                )}

                {/* Results Tabs */}
                {showTabs && (
                    <div className="mb-8">
                        <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
                            <TabsList className="grid w-full grid-cols-5 bg-gradient-to-r from-primary/10 to-accent/10 p-1.5 rounded-xl">
                                <TabsTrigger
                                    value="all"
                                    className="flex items-center justify-center gap-2 data-[state=active]:bg-background data-[state=active]:text-primary data-[state=active]:shadow-md data-[state=active]:scale-105 transition-all duration-300 rounded-lg text-xs md:text-sm"
                                >
                                    <Search className="w-4 h-4" />
                                    <span className="hidden md:inline">Todos ({totalResults})</span>
                                    <span className="md:hidden">Todos</span>
                                </TabsTrigger>
                                <TabsTrigger
                                    value="senadores"
                                    className="flex items-center justify-center gap-2 data-[state=active]:bg-background data-[state=active]:text-primary data-[state=active]:shadow-md data-[state=active]:scale-105 transition-all duration-300 rounded-lg text-xs md:text-sm"
                                    disabled={results.senador.length === 0}
                                >
                                    <Users className="w-4 h-4" />
                                    <span className="hidden md:inline">Senadores ({results.senador.length})</span>
                                    <span className="md:hidden">Sen</span>
                                </TabsTrigger>
                                <TabsTrigger
                                    value="deputados"
                                    className="flex items-center justify-center gap-2 data-[state=active]:bg-background data-[state=active]:text-primary data-[state=active]:shadow-md data-[state=active]:scale-105 transition-all duration-300 rounded-lg text-xs md:text-sm"
                                    disabled={results.deputado_federal.length === 0}
                                >
                                    <Users className="w-4 h-4" />
                                    <span className="hidden md:inline">Dep. Federais ({results.deputado_federal.length})</span>
                                    <span className="md:hidden">Fed</span>
                                </TabsTrigger>
                                <TabsTrigger
                                    value="deputados-estaduais"
                                    className="flex items-center justify-center gap-2 data-[state=active]:bg-background data-[state=active]:text-primary data-[state=active]:shadow-md data-[state=active]:scale-105 transition-all duration-300 rounded-lg text-xs md:text-sm"
                                    disabled={results.deputado_estadual.length === 0}
                                >
                                    <Users className="w-4 h-4" />
                                    <span className="hidden md:inline">Dep. Estaduais ({results.deputado_estadual.length})</span>
                                    <span className="md:hidden">Est</span>
                                </TabsTrigger>
                                <TabsTrigger
                                    value="empresas"
                                    className="flex items-center justify-center gap-2 data-[state=active]:bg-background data-[state=active]:text-primary data-[state=active]:shadow-md data-[state=active]:scale-105 transition-all duration-300 rounded-lg text-xs md:text-sm"
                                    disabled={results.fornecedor.length === 0}
                                >
                                    <Building2 className="w-4 h-4" />
                                    <span className="hidden md:inline">Empresas ({results.fornecedor.length})</span>
                                    <span className="md:hidden">Emp</span>
                                </TabsTrigger>
                            </TabsList>

                            <TabsContent value="all" className="mt-8 space-y-12">
                                {results.senador.length > 0 && (
                                    <div>
                                        <div className="flex items-center gap-3 mb-6">
                                            <div className="p-2 bg-primary/10 rounded-lg">
                                                <Users className="w-5 h-5 text-primary" />
                                            </div>
                                            <h2 className="text-2xl font-bold text-foreground">
                                                {results.senador.length} senador{pluralize(results.senador.length, '', 'es')} encontrado{pluralize(results.senador.length, '', 's')}
                                            </h2>
                                        </div>
                                        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                                            {results.senador.map((senador) => (
                                                <Link
                                                    key={senador.id_sf_senador}
                                                    to={`/senador/${senador.id_sf_senador}`}
                                                    title="Clique para visualizar o perfil do senador(a)"
                                                    className="block"
                                                >
                                                    <Card
                                                        className="group hover:shadow-2xl transition-all duration-500 hover:-translate-y-2 border-0 bg-card/80 backdrop-blur-sm shadow-lg overflow-hidden cursor-pointer h-full"
                                                    >
                                                        {/* Card Header with Status Gradient */}
                                                        <div className={`relative overflow-hidden h-24 ${senador.ativo
                                                            ? "bg-gradient-to-r from-primary/10 to-accent/5 group-hover:from-primary/20"
                                                            : "bg-gradient-to-r from-slate-500/10 to-transparent"
                                                            }`}>
                                                            <div className="absolute top-0 right-0 p-4 z-20">
                                                                <Badge className={`${senador.ativo ? "bg-green-500/10 text-green-600 border-green-500/20" : "bg-muted text-muted-foreground border-muted-foreground/20"} font-black text-[9px] uppercase tracking-widest px-2 py-0.5 backdrop-blur-md border`}>
                                                                    {senador.ativo ? "Ativo" : "Inativo"}
                                                                </Badge>
                                                            </div>
                                                            {/* Decorative shape */}
                                                            <div className="absolute -top-12 -left-12 w-24 h-24 bg-primary/10 rounded-full blur-2xl group-hover:scale-150 transition-transform duration-700" />
                                                        </div>

                                                        {/* Card Body */}
                                                        <CardContent className="p-0 -mt-12 relative z-10">
                                                            <div className="px-5 pb-6">
                                                                <div className="flex gap-4">
                                                                    {/* Image */}
                                                                    <div className="flex-shrink-0">
                                                                        <div className="relative">
                                                                            <div className="absolute -inset-1 bg-gradient-to-br from-primary to-accent rounded-2xl blur opacity-25 group-hover:opacity-40 transition duration-500"></div>
                                                                            <Avatar className={`h-32 w-24 rounded-2xl border-2 border-background shadow-xl group-hover:scale-105 transition-all duration-500 relative z-10 ${!senador.ativo ? "grayscale opacity-80" : ""}`}>
                                                                                <AvatarImage
                                                                                    src={`//static.ops.org.br/senador/${senador.id_sf_senador}_120x160.jpg`}
                                                                                    alt={senador.nome_parlamentar}
                                                                                />
                                                                                <AvatarFallback className="rounded-2xl text-xl font-black bg-muted text-muted-foreground uppercase shadow-inner">
                                                                                    {senador.nome_parlamentar.split(" ").filter(n => n.length > 2).slice(0, 2).map(n => n[0]).join("")}
                                                                                </AvatarFallback>
                                                                            </Avatar>
                                                                        </div>
                                                                    </div>

                                                                    {/* Info Section */}
                                                                    <div className="flex-1 min-w-0 pt-14 space-y-4">
                                                                        <div>
                                                                            <h3 className="font-black text-lg leading-tight text-foreground group-hover:text-primary transition-colors truncate tracking-tight">
                                                                                {senador.nome_parlamentar}
                                                                            </h3>
                                                                            <p className="text-[10px] font-medium text-muted-foreground uppercase tracking-widest opacity-80 truncate">{senador.nome_civil}</p>
                                                                        </div>

                                                                        <div className="flex items-center gap-1.5 flex-wrap">
                                                                            <Badge className="font-black bg-primary/5 text-primary border-primary/10 text-[10px] uppercase tracking-tighter px-2.5 py-0.5" title={senador.nome_partido}>
                                                                                {senador.sigla_partido}
                                                                            </Badge>
                                                                            <Badge variant="outline" className="flex items-center gap-1 font-bold border-muted-foreground/20 text-[10px] uppercase tracking-tighter px-2.5 py-0.5" title={senador.nome_estado}>
                                                                                <MapPin className="w-2.5 h-2.5" />
                                                                                {senador.sigla_estado}
                                                                            </Badge>
                                                                        </div>
                                                                    </div>
                                                                </div>

                                                                {/* Financial Info Grid */}
                                                                <div className="grid grid-cols-2 gap-4 mt-6 pt-6 border-t border-border/50">
                                                                    <div className="space-y-1">
                                                                        <p className="text-[9px] font-black text-muted-foreground uppercase tracking-widest opacity-60">Cota Parlamentar</p>
                                                                        <p className="font-black text-sm text-purple-600 font-mono tracking-tighter group-hover:scale-105 transition-transform origin-left">
                                                                            R$ {senador.valor_total_ceaps}
                                                                        </p>
                                                                    </div>
                                                                    <div className="space-y-1 text-right">
                                                                        <p className="text-[9px] font-black text-muted-foreground uppercase tracking-widest opacity-60 truncate">Remuneração</p>
                                                                        <p className="font-black text-sm text-orange-600 font-mono tracking-tighter group-hover:scale-105 transition-transform origin-right">
                                                                            R$ {senador.valor_total_remuneracao}
                                                                        </p>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </CardContent>
                                                    </Card>
                                                </Link>
                                            ))}
                                        </div>
                                    </div>
                                )}

                                {results.deputado_federal.length > 0 && (
                                    <div>
                                        <div className="flex items-center gap-3 mb-6">
                                            <div className="p-2 bg-blue-500/10 rounded-lg">
                                                <Users className="w-5 h-5 text-blue-600" />
                                            </div>
                                            <h2 className="text-2xl font-bold text-foreground">
                                                {results.deputado_federal.length} deputado{results.deputado_federal.length !== 1 ? 's' : ''} federa{results.deputado_federal.length !== 1 ? 'is' : 'l'} encontrado{results.deputado_federal.length !== 1 ? 's' : ''}
                                            </h2>
                                        </div>
                                        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                                            {results.deputado_federal.map((deputado) => (
                                                <Link
                                                    key={deputado.id_cf_deputado}
                                                    to={`/deputado-federal/${deputado.id_cf_deputado}`}
                                                    title="Clique para visualizar o perfil do deputado(a)"
                                                    className="block"
                                                >
                                                    <Card
                                                        className="group hover:shadow-2xl transition-all duration-500 hover:-translate-y-2 border-0 bg-card/80 backdrop-blur-sm shadow-lg overflow-hidden cursor-pointer h-full"
                                                    >
                                                        {/* Card Header with Status Gradient */}
                                                        <div className={`relative overflow-hidden h-24 ${deputado.ativo
                                                            ? "bg-gradient-to-r from-blue-600/10 to-blue-500/5 group-hover:from-blue-600/20"
                                                            : "bg-gradient-to-r from-slate-500/10 to-transparent"
                                                            }`}>
                                                            <div className="absolute top-0 right-0 p-4 z-20">
                                                                <Badge className={`${deputado.ativo ? "bg-green-500/10 text-green-600 border-green-500/20" : "bg-muted text-muted-foreground border-muted-foreground/20"} font-black text-[9px] uppercase tracking-widest px-2 py-0.5 backdrop-blur-md border`}>
                                                                    {deputado.ativo ? "Ativo" : "Inativo"}
                                                                </Badge>
                                                            </div>
                                                            {/* Decorative shape */}
                                                            <div className="absolute -top-12 -left-12 w-24 h-24 bg-blue-500/10 rounded-full blur-2xl group-hover:scale-150 transition-transform duration-700" />
                                                        </div>

                                                        {/* Card Body */}
                                                        <CardContent className="p-0 -mt-12 relative z-10">
                                                            <div className="px-5 pb-6">
                                                                <div className="flex gap-4">
                                                                    {/* Image */}
                                                                    <div className="flex-shrink-0">
                                                                        <div className="relative">
                                                                            <div className="absolute -inset-1 bg-gradient-to-br from-primary to-accent rounded-2xl blur opacity-25 group-hover:opacity-40 transition duration-500"></div>
                                                                            <Avatar className={`h-32 w-24 rounded-2xl border-2 border-background shadow-xl group-hover:scale-105 transition-all duration-500 relative z-10 ${!deputado.ativo ? "grayscale opacity-80" : ""}`}>
                                                                                <AvatarImage
                                                                                    src={`//static.ops.org.br/depfederal/${deputado.id_cf_deputado}.jpg`}
                                                                                    alt={deputado.nome_parlamentar}
                                                                                />
                                                                                <AvatarFallback className="rounded-2xl text-xl font-black bg-muted text-muted-foreground uppercase shadow-inner">
                                                                                    {deputado.nome_parlamentar.split(" ").filter(n => n.length > 2).slice(0, 2).map(n => n[0]).join("")}
                                                                                </AvatarFallback>
                                                                            </Avatar>
                                                                        </div>
                                                                    </div>

                                                                    {/* Info Section */}
                                                                    <div className="flex-1 min-w-0 pt-14 space-y-4">
                                                                        <div>
                                                                            <h3 className="font-black text-lg leading-tight text-foreground group-hover:text-primary transition-colors truncate tracking-tight">
                                                                                {deputado.nome_parlamentar}
                                                                            </h3>
                                                                            <p className="text-[10px] font-medium text-muted-foreground uppercase tracking-widest opacity-80 truncate">{deputado.nome_civil}</p>
                                                                        </div>

                                                                        <div className="flex items-center gap-1.5 flex-wrap">
                                                                            <Badge className="font-black bg-blue-500/5 text-blue-600 border-blue-500/10 text-[10px] uppercase tracking-tighter px-2.5 py-0.5" title={deputado.nome_partido}>
                                                                                {deputado.sigla_partido}
                                                                            </Badge>
                                                                            <Badge variant="outline" className="flex items-center gap-1 font-bold border-muted-foreground/20 text-[10px] uppercase tracking-tighter px-2.5 py-0.5" title={deputado.nome_estado}>
                                                                                <MapPin className="w-2.5 h-2.5" />
                                                                                {deputado.sigla_estado}
                                                                            </Badge>
                                                                        </div>
                                                                    </div>
                                                                </div>

                                                                {/* Financial Info Grid */}
                                                                <div className="grid grid-cols-2 gap-4 mt-6 pt-6 border-t border-border/50">
                                                                    <div className="space-y-1">
                                                                        <p className="text-[9px] font-black text-muted-foreground uppercase tracking-widest opacity-60">Cota Parlamentar</p>
                                                                        <p className="font-black text-sm text-purple-600 font-mono tracking-tighter group-hover:scale-105 transition-transform origin-left">
                                                                            R$ {deputado.valor_total_ceap}
                                                                        </p>
                                                                    </div>
                                                                    <div className="space-y-1 text-right">
                                                                        <p className="text-[9px] font-black text-muted-foreground uppercase tracking-widest opacity-60 truncate">Remuneração</p>
                                                                        <p className="font-black text-sm text-orange-600 font-mono tracking-tighter group-hover:scale-105 transition-transform origin-right">
                                                                            R$ {deputado.valor_total_remuneracao}
                                                                        </p>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </CardContent>
                                                    </Card>
                                                </Link>
                                            ))}
                                        </div>
                                    </div>
                                )}

                                {results.deputado_estadual.length > 0 && (
                                    <div>
                                        <div className="flex items-center gap-3 mb-6">
                                            <div className="p-2 bg-purple-500/10 rounded-lg">
                                                <Users className="w-5 h-5 text-purple-600" />
                                            </div>
                                            <h2 className="text-2xl font-bold text-foreground">
                                                {results.deputado_estadual.length} deputado{pluralize(results.deputado_estadual.length, '', 's')} estadua{pluralize(results.deputado_estadual.length, 'l', 'is')} encontrado{pluralize(results.deputado_estadual.length, '', 's')}
                                            </h2>
                                        </div>
                                        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                                            {results.deputado_estadual.map((deputado) => (
                                                <Link
                                                    key={deputado.id_cl_deputado}
                                                    to={`/deputado-estadual/${deputado.id_cl_deputado}`}
                                                    title="Clique para visualizar o perfil do deputado(a) estadual"
                                                    className="block"
                                                >
                                                    <Card
                                                        className="group hover:shadow-2xl transition-all duration-500 hover:-translate-y-2 border-0 bg-card/80 backdrop-blur-sm shadow-lg overflow-hidden cursor-pointer h-full"
                                                    >
                                                        {/* Card Header with Status Gradient */}
                                                        <div className={`relative overflow-hidden h-24 ${deputado.ativo
                                                            ? "bg-gradient-to-r from-purple-600/10 to-purple-500/5 group-hover:from-purple-600/20"
                                                            : "bg-gradient-to-r from-slate-500/10 to-transparent"
                                                            }`}>
                                                            {/* <div className="absolute top-0 right-0 p-4 z-20">
                                                                <Badge className={`${deputado.ativo ? "bg-green-500/10 text-green-600 border-green-500/20" : "bg-muted text-muted-foreground border-muted-foreground/20"} font-black text-[9px] uppercase tracking-widest px-2 py-0.5 backdrop-blur-md border`}>
                                                                    {deputado.ativo ? "Ativo" : "Inativo"}
                                                                </Badge>
                                                            </div> */}
                                                            {/* Decorative shape */}
                                                            <div className="absolute -top-12 -left-12 w-24 h-24 bg-purple-500/10 rounded-full blur-2xl group-hover:scale-150 transition-transform duration-700" />
                                                        </div>

                                                        {/* Card Body */}
                                                        <CardContent className="p-0 -mt-12 relative z-10">
                                                            <div className="px-5 pb-6">
                                                                <div className="flex gap-4">
                                                                    {/* Image */}
                                                                    <div className="flex-shrink-0">
                                                                        <div className="relative">
                                                                            <div className="absolute -inset-1 bg-gradient-to-br from-primary to-accent rounded-2xl blur opacity-25 group-hover:opacity-40 transition duration-500"></div>
                                                                            <Avatar className={`h-32 w-24 rounded-2xl border-2 border-background shadow-xl group-hover:scale-105 transition-all duration-500 relative z-10 ${!deputado.ativo ? "grayscale opacity-80" : ""}`}>
                                                                                <AvatarImage
                                                                                    src={`//static.ops.org.br/depestadual/${deputado.id_cl_deputado}.jpg`}
                                                                                    alt={deputado.nome_parlamentar}
                                                                                />
                                                                                <AvatarFallback className="rounded-2xl text-xl font-black bg-muted text-muted-foreground uppercase shadow-inner">
                                                                                    {deputado.nome_parlamentar.split(" ").filter(n => n.length > 2).slice(0, 2).map(n => n[0]).join("")}
                                                                                </AvatarFallback>
                                                                            </Avatar>
                                                                        </div>
                                                                    </div>

                                                                    {/* Info Section */}
                                                                    <div className="flex-1 min-w-0 pt-14 space-y-4">
                                                                        <div>
                                                                            <h3 className="font-black text-lg leading-tight text-foreground group-hover:text-primary transition-colors truncate tracking-tight">
                                                                                {deputado.nome_parlamentar}
                                                                            </h3>
                                                                            <p className="text-[10px] font-medium text-muted-foreground uppercase tracking-widest opacity-80 truncate">{deputado.nome_civil}</p>
                                                                        </div>

                                                                        <div className="flex items-center gap-1.5 flex-wrap">
                                                                            <Badge className="font-black bg-purple-500/5 text-purple-600 border-purple-500/10 text-[10px] uppercase tracking-tighter px-2.5 py-0.5" title={deputado.nome_partido}>
                                                                                {deputado.sigla_partido}
                                                                            </Badge>
                                                                            <Badge variant="outline" className="flex items-center gap-1 font-bold border-muted-foreground/20 text-[10px] uppercase tracking-tighter px-2.5 py-0.5" title={deputado.nome_estado}>
                                                                                <MapPin className="w-2.5 h-2.5" />
                                                                                {deputado.sigla_estado}
                                                                            </Badge>
                                                                        </div>
                                                                    </div>
                                                                </div>

                                                                {/* Financial Info Grid */}
                                                                <div className="grid grid-cols-2 gap-4 mt-6 pt-6 border-t border-border/50">
                                                                    <div className="space-y-1">
                                                                        <p className="text-[9px] font-black text-muted-foreground uppercase tracking-widest opacity-60">Cota Parlamentar</p>
                                                                        <p className="font-black text-sm text-purple-600 font-mono tracking-tighter group-hover:scale-105 transition-transform origin-left">
                                                                            R$ {deputado.valor_total_ceap}
                                                                        </p>
                                                                    </div>
                                                                    {/* <div className="space-y-1 text-right">
                                                                        <p className="text-[9px] font-black text-muted-foreground uppercase tracking-widest opacity-60 truncate">Verba Gabinete</p>
                                                                        <p className="font-black text-sm text-orange-600 font-mono tracking-tighter group-hover:scale-105 transition-transform origin-right">
                                                                            R$ {deputado.valor_total_verba_gabinete}
                                                                        </p>
                                                                    </div> */}
                                                                </div>
                                                            </div>
                                                        </CardContent>
                                                    </Card>
                                                </Link>
                                            ))}
                                        </div>
                                    </div>
                                )}

                                {results.fornecedor.length > 0 && (
                                    <div>
                                        <div className="flex items-center gap-3 mb-6">
                                            <div className="p-2 bg-green-500/10 rounded-lg">
                                                <Building2 className="w-5 h-5 text-green-600" />
                                            </div>
                                            <h2 className="text-2xl font-bold text-foreground">
                                                {results.fornecedor.length} empres{pluralize(results.fornecedor.length, 'a', 'as')} encontrad{pluralize(results.fornecedor.length, 'a', 'as')}
                                            </h2>
                                        </div>
                                        <Card className="shadow-xl border-0 bg-card/80 backdrop-blur-sm overflow-hidden transition-all duration-300">
                                            <CardContent className="p-0">
                                                <div className="divide-y divide-border/50">
                                                    {results.fornecedor.map((fornecedor, index) => (
                                                        <Link
                                                            key={fornecedor.id_fornecedor}
                                                            to={`/fornecedor/${fornecedor.id_fornecedor}`}
                                                            className="group block p-6 hover:bg-gradient-to-r hover:from-primary/5 hover:to-accent/5 transition-all duration-300 relative overflow-hidden"
                                                        >
                                                            <div className="absolute left-0 top-0 bottom-0 w-1 bg-primary scale-y-0 group-hover:scale-y-100 transition-transform duration-300" />

                                                            <div className="flex flex-col md:flex-row md:items-center gap-6 relative z-10">
                                                                <div className="flex-shrink-0 flex items-center justify-center w-14 h-14 bg-gradient-to-br from-primary/10 to-primary/20 rounded-2xl shadow-inner border border-primary/10 group-hover:scale-110 transition-transform duration-500">
                                                                    <Building2 className="w-7 h-7 text-primary" />
                                                                </div>

                                                                <div className="flex-1 min-w-0">
                                                                    <div className="flex items-center gap-3 mb-1">
                                                                        <span className="font-mono text-[10px] font-black text-primary px-2 py-0.5 bg-primary/5 rounded-full border border-primary/10">
                                                                            {fornecedor.cnpj}
                                                                        </span>
                                                                        <Badge variant="outline" className="flex items-center gap-1 font-bold border-muted-foreground/20 text-[9px] uppercase tracking-tighter px-2 py-0">
                                                                            <MapPin className="w-2.5 h-2.5" />
                                                                            {fornecedor.estado}
                                                                        </Badge>
                                                                    </div>
                                                                    <h3 className="font-black text-lg leading-tight text-foreground group-hover:text-primary transition-colors truncate tracking-tight uppercase">
                                                                        {fornecedor.nome_fantasia || fornecedor.nome}
                                                                    </h3>
                                                                    {fornecedor.nome_fantasia && fornecedor.nome_fantasia !== fornecedor.nome && (
                                                                        <p className="text-[10px] font-medium text-muted-foreground uppercase tracking-widest opacity-80 truncate mt-1">
                                                                            {fornecedor.nome}
                                                                        </p>
                                                                    )}
                                                                </div>

                                                                <div className="flex md:flex-col items-end gap-1 md:gap-0 justify-between md:justify-center min-w-[150px] pt-4 md:pt-0 border-t md:border-t-0 border-border/50">
                                                                    {fornecedor.valor_total_ceap && (
                                                                        <div className="text-right">
                                                                            <p className="text-[9px] font-black text-muted-foreground uppercase tracking-widest opacity-60">Valor Total CEAP</p>
                                                                            <p className="font-black text-xl text-primary font-mono tracking-tighter group-hover:scale-105 transition-transform origin-right">
                                                                                R$ {fornecedor.valor_total_ceap}
                                                                            </p>
                                                                        </div>
                                                                    )}
                                                                </div>

                                                                <div className="hidden md:flex items-center justify-center w-10 h-10 rounded-full bg-muted/50 group-hover:bg-primary group-hover:text-primary-foreground transition-all duration-300">
                                                                    <div className="w-2.5 h-2.5 border-t-2 border-r-2 border-current transform rotate-45" />
                                                                </div>
                                                            </div>
                                                        </Link>
                                                    ))}
                                                </div>
                                            </CardContent>
                                        </Card>
                                    </div>
                                )}
                            </TabsContent>

                            <TabsContent value="senadores" className="mt-8">
                                {results.senador.length > 0 && (
                                    <div>
                                        <div className="flex items-center gap-3 mb-6">
                                            <div className="p-2 bg-primary/10 rounded-lg">
                                                <Users className="w-5 h-5 text-primary" />
                                            </div>
                                            <h2 className="text-2xl font-bold text-foreground">
                                                {results.senador.length} senador{pluralize(results.senador.length, '', 'es')} encontrado{pluralize(results.senador.length, '', 's')}
                                            </h2>
                                        </div>
                                        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                                            {results.senador.map((senador) => (
                                                <Link
                                                    key={senador.id_sf_senador}
                                                    to={`/senador/${senador.id_sf_senador}`}
                                                    title="Clique para visualizar o perfil do senador(a)"
                                                    className="block"
                                                >
                                                    <Card
                                                        className="group hover:shadow-xl transition-all duration-300 hover:-translate-y-2 border-0 bg-card/80 backdrop-blur-sm shadow-lg overflow-hidden cursor-pointer"
                                                    >
                                                        {/* Card Header */}
                                                        <div className={`relative overflow-hidden ${senador.ativo
                                                            ? "bg-gradient-to-r from-emerald-600 to-emerald-500 text-white"
                                                            : "bg-gradient-to-r from-gray-500 to-gray-600 text-white"
                                                            }`}>
                                                            <div className="block p-4 hover:bg-black/10 transition-colors">
                                                                <div className="flex justify-between items-start gap-2">
                                                                    <div className="flex-1 min-w-0">
                                                                        <h3 className="font-bold text-lg leading-tight truncate">
                                                                            {senador.nome_parlamentar}
                                                                        </h3>
                                                                        <p className="text-sm opacity-90 truncate">{senador.nome_civil}</p>
                                                                    </div>
                                                                    <Badge variant="secondary" className="text-xs bg-background/20 text-foreground border-border/30">
                                                                        {senador.situacao || 'Ativo'}
                                                                    </Badge>
                                                                </div>
                                                            </div>
                                                        </div>

                                                        {/* Card Body */}
                                                        <CardContent className="p-4">
                                                            <div className="flex gap-3">
                                                                {/* Image */}
                                                                <div className="flex-shrink-0">
                                                                    <Avatar className="h-32 w-24 rounded-xl border-4 border-background shadow-lg group-hover:scale-105 transition-transform">
                                                                        <AvatarImage
                                                                            src={`//static.ops.org.br/senador/${senador.id_sf_senador}_120x160.jpg`}
                                                                            alt={senador.nome_parlamentar}
                                                                        />
                                                                        <AvatarFallback className="rounded-xl text-xl font-semibold bg-gradient-to-br from-primary/20 to-primary/10">
                                                                            {senador.nome_parlamentar.split(" ").map(n => n[0]).join("")}
                                                                        </AvatarFallback>
                                                                    </Avatar>
                                                                </div>

                                                                {/* Info Section */}
                                                                <div className="flex-1 min-w-0 space-y-3">
                                                                    {/* Party and State */}
                                                                    <div className="flex items-center gap-1 flex-wrap">
                                                                        <Badge variant="secondary" className="font-semibold text-[10px] px-2.5 py-0.5" title={senador.nome_partido}>
                                                                            {senador.sigla_partido}
                                                                        </Badge>
                                                                        <Badge variant="outline" className="flex items-center gap-1 text-[10px] px-2.5 py-0.5" title={senador.nome_estado}>
                                                                            <MapPin className="w-3 h-3" />
                                                                            {senador.sigla_estado}
                                                                        </Badge>
                                                                    </div>

                                                                    {/* Financial Info */}
                                                                    <div className="space-y-1">
                                                                        <div className="flex items-center gap-1">
                                                                            <DollarSign className="h-3 w-3 text-purple-600" />
                                                                            <span className="text-xs text-muted-foreground">Cota Parlamentar</span>
                                                                        </div>
                                                                        <p className="font-bold text-sm text-purple-700">
                                                                            R$ {senador.valor_total_ceaps}
                                                                        </p>
                                                                    </div>
                                                                    <div className="space-y-1">
                                                                        <div className="flex items-center gap-1">
                                                                            <Building2 className="h-3 w-3 text-orange-600" />
                                                                            <span className="text-xs text-muted-foreground">Folha de pagamento</span>
                                                                        </div>
                                                                        <p className="font-bold text-sm text-orange-700">
                                                                            R$ {senador.valor_total_remuneracao}
                                                                        </p>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </CardContent>
                                                    </Card>
                                                </Link>
                                            ))}
                                        </div>
                                    </div>
                                )}
                            </TabsContent>

                            <TabsContent value="deputados" className="mt-8">
                                {results.deputado_federal.length > 0 && (
                                    <div>
                                        <div className="flex items-center gap-3 mb-6">
                                            <div className="p-2 bg-blue-500/10 rounded-lg">
                                                <Users className="w-5 h-5 text-blue-600" />
                                            </div>
                                            <h2 className="text-2xl font-bold text-foreground">
                                                {results.deputado_federal.length} deputado{pluralize(results.deputado_federal.length, '', 's')} federal{pluralize(results.deputado_federal.length, '', 'is')} encontrado{pluralize(results.deputado_federal.length, '', 's')}
                                            </h2>
                                        </div>
                                        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                                            {results.deputado_federal.map((deputado) => (
                                                <Link
                                                    key={deputado.id_cf_deputado}
                                                    to={`/deputado-federal/${deputado.id_cf_deputado}`}
                                                    title="Clique para visualizar o perfil do deputado(a)"
                                                    className="block"
                                                >
                                                    <Card
                                                        className="group hover:shadow-xl transition-all duration-300 hover:-translate-y-2 border-0 bg-card/80 backdrop-blur-sm shadow-lg overflow-hidden cursor-pointer"
                                                    >
                                                        {/* Card Header */}
                                                        <div className={`relative overflow-hidden ${deputado.ativo
                                                            ? "bg-gradient-to-r from-blue-600 to-blue-500 text-white"
                                                            : "bg-gradient-to-r from-gray-500 to-gray-600 text-white"
                                                            }`}>
                                                            <div className="block p-4 hover:bg-black/10 transition-colors">
                                                                <div className="flex justify-between items-start gap-2">
                                                                    <div className="flex-1 min-w-0">
                                                                        <h3 className="font-bold text-lg leading-tight truncate">
                                                                            {deputado.nome_parlamentar}
                                                                        </h3>
                                                                        <p className="text-sm opacity-90 truncate">{deputado.nome_civil}</p>
                                                                    </div>
                                                                    <Badge variant="secondary" className="text-xs bg-background/20 text-foreground border-border/30">
                                                                        {deputado.situacao || 'Ativo'}
                                                                    </Badge>
                                                                </div>
                                                            </div>
                                                        </div>

                                                        {/* Card Body */}
                                                        <CardContent className="p-4">
                                                            <div className="flex gap-3">
                                                                {/* Image */}
                                                                <div className="flex-shrink-0">
                                                                    <Avatar className="h-32 w-24 rounded-xl border-4 border-background shadow-lg group-hover:scale-105 transition-transform">
                                                                        <AvatarImage
                                                                            src={`//static.ops.org.br/depfederal/${deputado.id_cf_deputado}.jpg`}
                                                                            alt={deputado.nome_parlamentar}
                                                                        />
                                                                        <AvatarFallback className="rounded-xl text-xl font-semibold bg-gradient-to-br from-blue-500/20 to-blue-500/10">
                                                                            {deputado.nome_parlamentar.split(" ").map(n => n[0]).join("")}
                                                                        </AvatarFallback>
                                                                    </Avatar>
                                                                </div>

                                                                {/* Info Section */}
                                                                <div className="flex-1 min-w-0 space-y-3">
                                                                    {/* Party and State */}
                                                                    <div className="flex items-center gap-1 flex-wrap">
                                                                        <Badge variant="secondary" className="font-semibold bg-blue-100 text-blue-800 text-[10px] px-2.5 py-0.5" title={deputado.nome_partido}>
                                                                            {deputado.sigla_partido}
                                                                        </Badge>
                                                                        <Badge variant="outline" className="flex items-center gap-1 border-blue-200 text-blue-600 text-[10px] px-2.5 py-0.5" title={deputado.nome_estado}>
                                                                            <MapPin className="w-3 h-3" />
                                                                            {deputado.sigla_estado}
                                                                        </Badge>
                                                                    </div>

                                                                    {/* Financial Info */}
                                                                    <div className="space-y-1">
                                                                        <div className="flex items-center gap-1">
                                                                            <DollarSign className="h-3 w-3 text-purple-600" />
                                                                            <span className="text-xs text-muted-foreground">Cota Parlamentar</span>
                                                                        </div>
                                                                        <p className="font-bold text-sm text-purple-700">
                                                                            R$ {deputado.valor_total_ceap}
                                                                        </p>
                                                                    </div>
                                                                    <div className="space-y-1">
                                                                        <div className="flex items-center gap-1">
                                                                            <Building2 className="h-3 w-3 text-orange-600" />
                                                                            <span className="text-xs text-muted-foreground">Verba de Gabinete</span>
                                                                        </div>
                                                                        <p className="font-bold text-sm text-orange-700">
                                                                            R$ {deputado.valor_total_remuneracao}
                                                                        </p>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </CardContent>
                                                    </Card>
                                                </Link>
                                            ))}
                                        </div>
                                    </div>
                                )}
                            </TabsContent>

                            <TabsContent value="deputados-estaduais" className="mt-8">
                                {results.deputado_estadual.length > 0 && (
                                    <div>
                                        <div className="flex items-center gap-3 mb-6">
                                            <div className="p-2 bg-purple-500/10 rounded-lg">
                                                <Users className="w-5 h-5 text-purple-600" />
                                            </div>
                                            <h2 className="text-2xl font-bold text-foreground">
                                                {results.deputado_estadual.length} deputado{pluralize(results.deputado_estadual.length, '', 's')} estadual{pluralize(results.deputado_estadual.length, '', 'is')} encontrado{pluralize(results.deputado_estadual.length, '', 's')}
                                            </h2>
                                        </div>
                                        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                                            {results.deputado_estadual.map((deputado) => (
                                                <Link
                                                    key={deputado.id_cl_deputado}
                                                    to={`/deputado-estadual/${deputado.id_cl_deputado}`}
                                                    title="Clique para visualizar o perfil do deputado(a) estadual"
                                                    className="block"
                                                >
                                                    <Card
                                                        className="group hover:shadow-xl transition-all duration-300 hover:-translate-y-2 border-0 bg-card/80 backdrop-blur-sm shadow-lg overflow-hidden cursor-pointer"
                                                    >
                                                        {/* Card Header */}
                                                        <div className={`relative overflow-hidden ${deputado.ativo
                                                            ? "bg-gradient-to-r from-purple-600 to-purple-500 text-white"
                                                            : "bg-gradient-to-r from-gray-500 to-gray-600 text-white"
                                                            }`}>
                                                            <div className="block p-4 hover:bg-black/10 transition-colors">
                                                                <div className="flex justify-between items-start gap-2">
                                                                    <div className="flex-1 min-w-0">
                                                                        <h3 className="font-bold text-lg leading-tight truncate">
                                                                            {deputado.nome_parlamentar}
                                                                        </h3>
                                                                        <p className="text-sm opacity-90 truncate">{deputado.nome_civil}</p>
                                                                    </div>
                                                                    <Badge variant="secondary" className="text-xs bg-background/20 text-foreground border-border/30">
                                                                        {deputado.situacao || 'Ativo'}
                                                                    </Badge>
                                                                </div>
                                                            </div>
                                                        </div>

                                                        {/* Card Body */}
                                                        <CardContent className="p-4">
                                                            <div className="flex gap-3">
                                                                {/* Image */}
                                                                <div className="flex-shrink-0">
                                                                    <Avatar className="h-32 w-24 rounded-xl border-4 border-background shadow-lg group-hover:scale-105 transition-transform">
                                                                        <AvatarImage
                                                                            src={`//static.ops.org.br/depestadual/${deputado.id_cl_deputado}.jpg`}
                                                                            alt={deputado.nome_parlamentar}
                                                                        />
                                                                        <AvatarFallback className="rounded-xl text-xl font-semibold bg-gradient-to-br from-purple-500/20 to-purple-500/10">
                                                                            {deputado.nome_parlamentar.split(" ").map(n => n[0]).join("")}
                                                                        </AvatarFallback>
                                                                    </Avatar>
                                                                </div>

                                                                {/* Info Section */}
                                                                <div className="flex-1 min-w-0 space-y-3">
                                                                    {/* Party and State */}
                                                                    <div className="flex items-center gap-1 flex-wrap">
                                                                        <Badge variant="secondary" className="font-semibold bg-purple-100 text-purple-800 text-[10px] px-2.5 py-0.5" title={deputado.nome_partido}>
                                                                            {deputado.sigla_partido}
                                                                        </Badge>
                                                                        <Badge variant="outline" className="flex items-center gap-1 border-purple-200 text-purple-600 text-[10px] px-2.5 py-0.5" title={deputado.nome_estado}>
                                                                            <MapPin className="w-3 h-3" />
                                                                            {deputado.sigla_estado}
                                                                        </Badge>
                                                                    </div>

                                                                    {/* Financial Info */}
                                                                    <div className="space-y-1">
                                                                        <div className="flex items-center gap-1">
                                                                            <DollarSign className="h-3 w-3 text-purple-600" />
                                                                            <span className="text-xs text-muted-foreground">Cota Parlamentar</span>
                                                                        </div>
                                                                        <p className="font-bold text-sm text-purple-700">
                                                                            R$ {deputado.valor_total_ceap}
                                                                        </p>
                                                                    </div>
                                                                    <div className="space-y-1">
                                                                        <div className="flex items-center gap-1">
                                                                            <Building2 className="h-3 w-3 text-orange-600" />
                                                                            <span className="text-xs text-muted-foreground">Verba de Gabinete</span>
                                                                        </div>
                                                                        <p className="font-bold text-sm text-orange-700">
                                                                            R$ {deputado.valor_total_verba_gabinete}
                                                                        </p>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </CardContent>
                                                    </Card>
                                                </Link>
                                            ))}
                                        </div>
                                    </div>
                                )}
                            </TabsContent>

                            <TabsContent value="empresas" className="mt-8">
                                {results.fornecedor.length > 0 && (
                                    <div>
                                        <div className="flex items-center gap-3 mb-6">
                                            <div className="p-2 bg-green-500/10 rounded-lg">
                                                <Building2 className="w-5 h-5 text-green-600" />
                                            </div>
                                            <h2 className="text-2xl font-bold text-foreground">
                                                {results.fornecedor.length} empres{pluralize(results.fornecedor.length, 'a', 'as')} encontrad{pluralize(results.fornecedor.length, 'a', 'as')}
                                            </h2>
                                        </div>
                                        <Card className="shadow-lg border-0 bg-card">
                                            <CardContent className="p-0">
                                                <div className="divide-y divide-border/50">
                                                    {results.fornecedor.map((fornecedor, index) => (
                                                        <Link
                                                            key={fornecedor.id_fornecedor}
                                                            to={`/fornecedor/${fornecedor.id_fornecedor}`}
                                                            className={`block p-6 hover:bg-gradient-to-r hover:from-green-50/50 hover:to-transparent transition-all duration-200 ${index === 0 ? 'border-t' : ''}`}
                                                        >
                                                            <div className="flex items-start gap-4">
                                                                <div className="flex-shrink-0 p-3 bg-green-100 rounded-lg">
                                                                    <Building2 className="w-6 h-6 text-green-600" />
                                                                </div>
                                                                <div className="flex-1 space-y-2">
                                                                    <div className="flex items-center gap-3">
                                                                        <p className="font-mono text-sm font-semibold text-green-600">
                                                                            {fornecedor.cnpj}
                                                                        </p>
                                                                        <Badge variant="outline" className="flex items-center gap-1 border-green-200 text-green-700">
                                                                            <MapPin className="w-3 h-3" />
                                                                            {fornecedor.estado}
                                                                        </Badge>
                                                                    </div>
                                                                    <p className="font-semibold text-foreground text-lg">
                                                                        {fornecedor.nome_fantasia || fornecedor.nome}
                                                                    </p>
                                                                    {fornecedor.nome_fantasia && fornecedor.nome_fantasia !== fornecedor.nome && (
                                                                        <p className="text-sm text-muted-foreground">
                                                                            {fornecedor.nome}
                                                                        </p>
                                                                    )}
                                                                </div>
                                                                <div className="flex-shrink-0 text-center flex flex-col justify-center min-w-[120px]">
                                                                    {fornecedor.valor_total_ceap && (
                                                                        <div className="space-y-1">
                                                                            <div className="flex items-center gap-1 justify-center">
                                                                                <DollarSign className="h-3 w-3 text-green-600" />
                                                                                <span className="text-xs text-muted-foreground">Total CEAP</span>
                                                                            </div>
                                                                            <p className="font-bold text-sm text-green-700">
                                                                                R$ {fornecedor.valor_total_ceap}
                                                                            </p>
                                                                        </div>
                                                                    )}
                                                                </div>
                                                                <div className="flex-shrink-0">
                                                                    <div className="w-8 h-8 rounded-full bg-green-100 flex items-center justify-center group-hover:bg-green-200 transition-colors">
                                                                        <div className="w-2 h-2 border-t-2 border-r-2 border-green-600 transform rotate-45"></div>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </Link>
                                                    ))}
                                                </div>
                                            </CardContent>
                                        </Card>
                                    </div>
                                )}
                            </TabsContent>
                        </Tabs>
                    </div>
                )}
            </main>
            <Footer />
        </div>
    );
};

export default Busca;
