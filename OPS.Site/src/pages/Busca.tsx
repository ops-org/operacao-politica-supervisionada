import { useState, useEffect } from "react";
import { useSearchParams, Link } from "react-router-dom";
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
        <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
            <Header />
            <main className="container mx-auto px- py-8">
                {/* Hero Section */}
                <div className="text-center mb-12">
                    <div className="inline-flex items-center justify-center w-16 h-16 bg-primary/10 rounded-full mb-4">
                        <Search className="w-8 h-8 text-primary" />
                    </div>
                    <h1 className="text-4xl font-bold text-foreground mb-4">
                        Busca por Parlamentar ou Fornecedor
                    </h1>
                    <p className="text-lg text-muted-foreground mx-auto">
                        Encontre os deputados, senadores e empresas que interagem com o poder público
                    </p>
                </div>

                {/* Search Form */}
                <Card className="mb-12 shadow-lg border-0 bg-white/80 backdrop-blur-sm">
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
                            <TabsList className="grid w-full grid-cols-5 bg-muted/50 p-1">
                                <TabsTrigger
                                    value="all"
                                    className="flex items-center gap-2 data-[state=active]:bg-background data-[state=active]:shadow-sm"
                                >
                                    <Search className="w-4 h-4" />
                                    Todos ({totalResults})
                                </TabsTrigger>
                                <TabsTrigger
                                    value="senadores"
                                    className="flex items-center gap-2 data-[state=active]:bg-background data-[state=active]:shadow-sm"
                                    disabled={results.senador.length === 0}
                                >
                                    <Users className="w-4 h-4" />
                                    Senadores ({results.senador.length})
                                </TabsTrigger>
                                <TabsTrigger
                                    value="deputados"
                                    className="flex items-center gap-2 data-[state=active]:bg-background data-[state=active]:shadow-sm"
                                    disabled={results.deputado_federal.length === 0}
                                >
                                    <Users className="w-4 h-4" />
                                    Deputados Federais ({results.deputado_federal.length})
                                </TabsTrigger>
                                <TabsTrigger
                                    value="deputados-estaduais"
                                    className="flex items-center gap-2 data-[state=active]:bg-background data-[state=active]:shadow-sm"
                                    disabled={results.deputado_estadual.length === 0}
                                >
                                    <Users className="w-4 h-4" />
                                    Deputados Estaduais ({results.deputado_estadual.length})
                                </TabsTrigger>
                                <TabsTrigger
                                    value="empresas"
                                    className="flex items-center gap-2 data-[state=active]:bg-background data-[state=active]:shadow-sm"
                                    disabled={results.fornecedor.length === 0}
                                >
                                    <Building2 className="w-4 h-4" />
                                    Empresas ({results.fornecedor.length})
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
                                                        className="group hover:shadow-md transition-all duration-300 hover:scale-105 border-0 bg-white shadow-xs overflow-hidden cursor-pointer"
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
                                                                    <Badge variant="secondary" className="text-xs bg-white/20 text-white border-white/30">
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
                                                                    <Avatar className="h-32 w-24 rounded-xl border-4 border-white shadow-lg group-hover:scale-105 transition-transform">
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
                                                                        <Badge variant="secondary" className="font-semibold" title={senador.nome_partido}>
                                                                            {senador.sigla_partido}
                                                                        </Badge>
                                                                        <Badge variant="outline" className="flex items-center gap-1" title={senador.nome_estado}>
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
                                                        className="group hover:shadow-md transition-all duration-300 hover:scale-105 border-0 bg-white shadow-xs overflow-hidden cursor-pointer"
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
                                                                    <Badge variant="secondary" className="text-xs bg-white/20 text-white border-white/30">
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
                                                                    <Avatar className="h-32 w-24 rounded-xl border-4 border-white shadow-lg group-hover:scale-105 transition-transform">
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
                                                                        <Badge variant="secondary" className="font-semibold bg-blue-100 text-blue-800" title={deputado.nome_partido}>
                                                                            {deputado.sigla_partido}
                                                                        </Badge>
                                                                        <Badge variant="outline" className="flex items-center gap-1 border-blue-200 text-blue-600" title={deputado.nome_estado}>
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
                                                        className="group hover:shadow-md transition-all duration-300 hover:scale-105 border-0 bg-white shadow-xs overflow-hidden cursor-pointer"
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
                                                                    {/* <Badge variant="secondary" className="text-xs bg-white/20 text-white border-white/30">
                                                                        {deputado.situacao || 'Ativo'}
                                                                    </Badge> */}
                                                                </div>
                                                            </div>
                                                        </div>

                                                        {/* Card Body */}
                                                        <CardContent className="p-4">
                                                            <div className="flex gap-3">
                                                                {/* Image */}
                                                                <div className="flex-shrink-0">
                                                                    <Avatar className="h-32 w-24 rounded-xl border-4 border-white shadow-lg group-hover:scale-105 transition-transform">
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
                                                                        <Badge variant="secondary" className="font-semibold bg-purple-100 text-purple-800" title={deputado.nome_partido}>
                                                                            {deputado.sigla_partido}
                                                                        </Badge>
                                                                        <Badge variant="outline" className="flex items-center gap-1 border-purple-200 text-purple-600" title={deputado.nome_estado}>
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
                                                                    {/* <div className="space-y-1">
                                                                        <div className="flex items-center gap-1">
                                                                            <Building2 className="h-3 w-3 text-orange-600" />
                                                                            <span className="text-xs text-muted-foreground">Verba de Gabinete</span>
                                                                        </div>
                                                                        <p className="font-bold text-sm text-orange-700">
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
                                        <Card className="shadow-lg border-0 bg-white">
                                            <CardContent className="p-0">
                                                <div className="divide-y divide-border/50">
                                                    {results.fornecedor.map((fornecedor, index) => (
                                                        <Link
                                                            key={fornecedor.id_fornecedor}
                                                            to={`/fornecedor/${fornecedor.id_fornecedor}`}
                                                            className={`block p-6 hover:bg-gradient-to-r hover:from-green-50/50 hover:to-transparent transition-all duration-200 ${index === 0 ? 'border-t' : ''}`}
                                                        >
                                                            <div className="flex items-center gap-4">
                                                                <div className="flex-shrink-0 p-3 bg-green-100 rounded-lg">
                                                                    <Building2 className="w-6 h-6 text-green-600" />
                                                                </div>
                                                                <div className="flex-1">
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
                                                                                <span className="text-xs text-muted-foreground">ValorTotal CEAP</span>
                                                                            </div>
                                                                            <p className="font-bold text-lg text-green-700">
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
                                                        className="group hover:shadow-md transition-all duration-300 hover:scale-105 border-0 bg-white shadow-xs overflow-hidden cursor-pointer"
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
                                                                    <Badge variant="secondary" className="text-xs bg-white/20 text-white border-white/30">
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
                                                                    <Avatar className="h-32 w-24 rounded-xl border-4 border-white shadow-lg group-hover:scale-105 transition-transform">
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
                                                                        <Badge variant="secondary" className="font-semibold" title={senador.nome_partido}>
                                                                            {senador.sigla_partido}
                                                                        </Badge>
                                                                        <Badge variant="outline" className="flex items-center gap-1" title={senador.nome_estado}>
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
                                                        className="group hover:shadow-md transition-all duration-300 hover:scale-105 border-0 bg-white shadow-xs overflow-hidden cursor-pointer"
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
                                                                    <Badge variant="secondary" className="text-xs bg-white/20 text-white border-white/30">
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
                                                                    <Avatar className="h-32 w-24 rounded-xl border-4 border-white shadow-lg group-hover:scale-105 transition-transform">
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
                                                                        <Badge variant="secondary" className="font-semibold bg-blue-100 text-blue-800" title={deputado.nome_partido}>
                                                                            {deputado.sigla_partido}
                                                                        </Badge>
                                                                        <Badge variant="outline" className="flex items-center gap-1 border-blue-200 text-blue-600" title={deputado.nome_estado}>
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
                                                        className="group hover:shadow-md transition-all duration-300 hover:scale-105 border-0 bg-white shadow-xs overflow-hidden cursor-pointer"
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
                                                                    <Badge variant="secondary" className="text-xs bg-white/20 text-white border-white/30">
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
                                                                    <Avatar className="h-32 w-24 rounded-xl border-4 border-white shadow-lg group-hover:scale-105 transition-transform">
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
                                                                        <Badge variant="secondary" className="font-semibold bg-purple-100 text-purple-800" title={deputado.nome_partido}>
                                                                            {deputado.sigla_partido}
                                                                        </Badge>
                                                                        <Badge variant="outline" className="flex items-center gap-1 border-purple-200 text-purple-600" title={deputado.nome_estado}>
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
                                        <Card className="shadow-lg border-0 bg-white">
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
