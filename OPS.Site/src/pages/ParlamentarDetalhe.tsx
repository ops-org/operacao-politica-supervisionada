import { useParams, Link } from "react-router-dom";
import { useState, useEffect, useRef } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
import { smoothScrollToElement, getHeaderHeight } from "@/lib/scroll";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { ErrorState } from "@/components/ErrorState";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { ParlamentarDetalheSkeleton } from "@/components/ParlamentarDetalheSkeleton";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip } from "recharts";
import { formatCurrency, formatValue } from "@/lib/utils";
import { ExternalLink, Phone, Mail, Users, TrendingUp, Calendar, MapPin, Briefcase, User, DollarSign, Building2, ArrowRight, Receipt } from "lucide-react";
import { PoliticianType } from "@/types/politician";
import {
    fetchPoliticianData,
    fetchCustoAnual,
    Parlamentar as ParlamentarType,
    Fornecedor as FornecedorType,
    CustoAnual as CustoAnualType,
    MaioresNotas as TopNotasType
} from "@/lib/api";


const ParlamentarDetalhe = ({ type }: { type: PoliticianType }) => {
    const { id } = useParams();
    const [data, setData] = useState<ParlamentarType | null>(null);
    const [fornecedores, setFornecedores] = useState<FornecedorType[]>([]);
    const [chartData, setChartData] = useState<CustoAnualType[]>([]);
    const [maioresNotas, setMaioresNotas] = useState<TopNotasType[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const valueSummaryRef = useRef<HTMLDivElement>(null);
    const fornecedoresRef = useRef<HTMLDivElement>(null);
    const custosAnoRef = useRef<HTMLDivElement>(null);

    const isFederal = type === "deputado-federal";
    const isState = type === "deputado-estadual";
    const isSenator = type === "senador";

    usePageTitle(data ? data.nome_parlamentar : "Parlamentar");

    useEffect(() => {
        const loadData = async () => {
            if (!id) {
                setError("ID não fornecido");
                setLoading(false);
                return;
            }

            try {
                setLoading(true);
                const response = await fetchPoliticianData(id, type);

                setData(response.detalhes);
                setFornecedores(response.maioresFornecedores || []);
                // Handle different data structures for chart data
                if (Array.isArray(response.custoAnual)) {
                    setChartData(response.custoAnual);
                } else if (response.custoAnual && 'categories' in response.custoAnual && 'series' in response.custoAnual) {
                    // Convert GastoPorAno to chart format
                    const gastosData = response.custoAnual as any;
                    setChartData(gastosData.categories.map((cat: string, index: number) => ({
                        ano: cat,
                        valor: gastosData.series[index] || 0
                    })));
                } else {
                    setChartData([]);
                }
                setMaioresNotas(response.maioresNotas || []);
            } catch (err) {
                console.error(err);
                setError(err instanceof Error ? err.message : "Erro ao carregar dados do parlamentar");
            } finally {
                setLoading(false);
            }
        };

        loadData();
    }, [id, type]);

    const scrollToValueSummary = () => {
        if (!isState)
            smoothScrollToElement(valueSummaryRef, getHeaderHeight());
        else
            smoothScrollToElement(custosAnoRef, getHeaderHeight());
    };

    const scrollToFornecedores = () => {
        smoothScrollToElement(fornecedoresRef, getHeaderHeight());
    };

    const scrollToCustosAno = () => {
        smoothScrollToElement(custosAnoRef, getHeaderHeight());
    };

    const folhaPagamentoUrl = data ? (type === "deputado-federal" ?
        `/deputado-federal/folha-pagamento?IdParlamentar=${data.id}` :
        `/senador/folha-pagamento?IdParlamentar=${data.id}`) : '#';

    if (loading) {
        return <ParlamentarDetalheSkeleton type={type} />;
    }

    if (error || !data) {
        return (
            <ErrorState
                title="Erro ao carregar parlamentar"
                message={error || "Não foi possível encontrar as informações deste parlamentar."}
            />
        );
    }



    const breadcrumbLabel = isSenator ? "Senadores" : isFederal ? "Deputados Federais" : "Deputados Estaduais";
    const breadcrumbLink = isSenator ? "/senador" : isFederal ? "/deputado-federal" : "/deputado-estadual";
    const detailLabel = isSenator ? "Perfil do Senador" : isFederal ? "Perfil do Deputado Federal" : "Perfil do Deputado Estadual";

    return (
        <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
            <Header />
            <main className="container mx-auto px-4 py-8">
                {/* Breadcrumb */}
                <div className="flex items-center gap-2 text-sm text-muted-foreground mb-8">
                    <Link to={breadcrumbLink} className="hover:text-foreground transition-colors">
                        {breadcrumbLabel}
                    </Link>
                    <ArrowRight className="h-4 w-4" />
                    <span className="text-foreground">{detailLabel}</span>
                </div>

                <div className="space-y-8">
                    {/* Profile Card */}
                    <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300 border-t-4 border-t-primary">
                        <div className={`relative overflow-hidden ${data.situacao && data.situacao !== 'Exercício'
                            ? "bg-gradient-to-r from-slate-500/10 to-transparent"
                            : "bg-gradient-to-r from-primary/10 to-accent/5"
                            }`}>
                            <div className="absolute top-[-20%] right-[-10%] w-64 h-64 bg-primary/5 rounded-full blur-3xl" />
                            <div className="absolute bottom-[-50%] left-[-10%] w-80 h-80 bg-accent/5 rounded-full blur-3xl" />

                            <div className="p-8 relative z-10">
                                <div className="flex flex-col md:flex-row gap-8 items-center md:items-start">
                                    {/* Avatar Section */}
                                    <div className="flex-shrink-0">
                                        <div className="relative group">
                                            <div className="absolute -inset-1 bg-gradient-to-br from-primary to-accent rounded-2xl blur opacity-25 group-hover:opacity-50 transition duration-1000 group-hover:duration-200"></div>
                                            <Avatar className={`h-40 w-32 rounded-2xl border-2 border-background shadow-2xl transition-all duration-500 group-hover:scale-105 ${data.situacao && data.situacao !== 'Exercício' ? "grayscale opacity-80" : ""}`}>
                                                <AvatarImage src={data.foto_url} alt={data.nome_parlamentar} />
                                                <AvatarFallback className="rounded-2xl text-3xl font-black bg-muted text-muted-foreground uppercase">
                                                    {data.nome_parlamentar.split(" ").filter(n => n.length > 2).slice(0, 2).map(n => n[0]).join("")}
                                                </AvatarFallback>
                                            </Avatar>
                                        </div>
                                    </div>

                                    {/* Main Info Section */}
                                    <div className="flex-1 text-center md:text-left space-y-4">
                                        <div className="space-y-2">
                                            <div className="flex items-center gap-3 flex-wrap justify-center md:justify-start">
                                                <h2 className="text-2xl md:text-3xl lg:text-4xl font-black text-foreground tracking-tight">
                                                    <a
                                                        title="Clique para visitar a Página Oficial"
                                                        href={data.pagina_oficial_url}
                                                        target="_blank"
                                                        className="hover:text-primary transition-colors inline-flex items-center gap-2 group"
                                                    >
                                                        {data.nome_parlamentar}
                                                        <ExternalLink className="h-5 w-5 opacity-50 group-hover:opacity-100 transition-opacity" />
                                                    </a>
                                                </h2>
                                            </div>

                                            <div className="flex items-center gap-2 flex-wrap justify-center md:justify-start">
                                                <Badge className="font-black bg-primary/10 text-primary border-primary/20 uppercase tracking-widest text-[10px] px-3 py-1" title={data.nome_partido}>
                                                    {data.sigla_partido}
                                                </Badge>
                                                <Badge variant="outline" className="flex items-center gap-1.5 bg-background/50 backdrop-blur-sm border-muted-foreground/20 font-bold text-[10px] uppercase tracking-widest px-3 py-1" title={data.nome_estado}>
                                                    <MapPin className="w-3 h-3 text-primary" />
                                                    {data.sigla_estado}
                                                </Badge>
                                                {data.condicao && (
                                                    <Badge variant="secondary" className="bg-muted text-muted-foreground border-muted-foreground/20 font-bold text-[10px] uppercase tracking-widest px-3 py-1" title="Condição">
                                                        {data.condicao}
                                                    </Badge>
                                                )}
                                                {data.situacao && (
                                                    <Badge className={`${data.situacao === 'Exercício' ? "bg-green-500/10 text-green-600 border-green-500/20" : "bg-muted text-muted-foreground border-muted-foreground/20"} font-bold text-[10px] uppercase tracking-widest px-3 py-1`}>
                                                        {data.situacao}
                                                    </Badge>
                                                )}
                                            </div>
                                            <p className="text-sm font-medium text-muted-foreground uppercase tracking-widest opacity-80">{data.nome_civil}</p>
                                        </div>
                                    </div>

                                    {/* Total Cost Display */}
                                    <div className="text-center md:text-right space-y-2 lg:min-w-[280px]">
                                        <button
                                            onClick={scrollToValueSummary}
                                            className="block w-full bg-gradient-to-br from-primary to-primary/80 rounded-2xl p-6 shadow-xl shadow-primary/20 text-white transform hover:scale-105 transition-all duration-300 group"
                                            title="Clique para ver detalhes dos valores"
                                        >
                                            <div className="flex items-center justify-center md:justify-end gap-2 mb-1 opacity-90">
                                                <TrendingUp className="h-4 w-4" />
                                                <p className="text-[10px] font-black uppercase tracking-widest">Custo Total Acumulado</p>
                                            </div>
                                            <p className="text-2xl md:text-4xl font-black font-mono tracking-tighter">
                                                R$ {data.valor_total}
                                            </p>
                                            <p className="text-[9px] font-medium text-white/70 uppercase tracking-tight mt-1">
                                                {isFederal ? "CEAP • GABINETE • SALÁRIO • AUXÍLIOS" : isSenator ? "CEAPS • VERBA GABINETE" : "CEAP • DIÁRIAS • SAÚDE"}
                                            </p>
                                            <div className="flex items-center justify-center md:justify-end gap-1 mt-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                                <span className="text-[8px] font-medium">Ver detalhes</span>
                                                <ArrowRight className="h-3 w-3" />
                                            </div>
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Contact Info Bar */}
                        {(!data.situacao || data.situacao === "Exercício") && (
                            <div className="border-t border-border/50 bg-muted/20 px-8 py-4">
                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 text-[11px] font-bold uppercase tracking-wider">
                                    {data.email && (
                                        <div className="flex items-center gap-3 group">
                                            <div className="p-2 bg-primary/10 rounded-lg text-primary group-hover:bg-primary group-hover:text-primary-foreground transition-colors">
                                                <Mail className="h-4 w-4" />
                                            </div>
                                            <div className="flex flex-col">
                                                <span className="text-[9px] text-muted-foreground font-black">Email oficial</span>
                                                <a href={`mailto:${data.email}`} className="text-foreground hover:text-primary transition-colors lowercase font-medium">
                                                    {data.email}
                                                </a>
                                            </div>
                                        </div>
                                    )}
                                    {data.telefone && (
                                        <div className="flex items-center gap-3">
                                            <div className="p-2 bg-primary/10 rounded-lg text-primary">
                                                <Phone className="h-4 w-4" />
                                            </div>
                                            <div className="flex flex-col">
                                                <span className="text-[9px] text-muted-foreground font-black">Telefone</span>
                                                <span className="text-foreground">{data.telefone}</span>
                                            </div>
                                        </div>
                                    )}
                                    {(data.sala || data.predio) && (
                                        <div className="flex items-center gap-3">
                                            <div className="p-2 bg-primary/10 rounded-lg text-primary">
                                                <MapPin className="h-4 w-4" />
                                            </div>
                                            <div className="flex flex-col">
                                                <span className="text-[9px] text-muted-foreground font-black">Gabinete</span>
                                                <span className="text-foreground">{data.sala ? `SALA ${data.sala}` : ''} {data.predio ? `• ANEXO ${data.predio}` : ''}</span>
                                            </div>
                                        </div>
                                    )}
                                    {data.secretarios_ativos && (
                                        <div className="flex items-center gap-3">
                                            <div className="p-2 bg-primary/10 rounded-lg text-primary">
                                                <Users className="h-4 w-4" />
                                            </div>
                                            <div className="flex flex-col">
                                                <span className="text-[9px] text-muted-foreground font-black">Secretários ativos</span>
                                                <span className="text-foreground">{data.secretarios_ativos}</span>
                                            </div>
                                        </div>
                                    )}
                                    {data.site && (
                                        <div className="flex items-center gap-3 group">
                                            <div className="p-2 bg-primary/10 rounded-lg text-primary group-hover:bg-primary group-hover:text-primary-foreground transition-colors">
                                                <ExternalLink className="h-4 w-4" />
                                            </div>
                                            <div className="flex flex-col">
                                                <span className="text-[9px] text-muted-foreground font-black">Site</span>
                                                <a href={data.site} target="_blank" rel="noopener noreferrer" className="text-foreground hover:text-primary transition-colors lowercase font-medium">
                                                    Acessar
                                                </a>
                                            </div>
                                        </div>
                                    )}
                                </div>
                            </div>
                        )}
                    </Card>

                    {/* Value Summary Cards (only for Federal and Senator) */}
                    {!isState && (
                        <div ref={valueSummaryRef} className={`grid grid-cols-1 md:grid-cols-2 ${isFederal ? "lg:grid-cols-4" : ""} gap-6`}>
                            <Card className="shadow-lg border-0 bg-blue-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
                                <button
                                    onClick={scrollToFornecedores}
                                    className="w-full p-6 text-left hover:bg-blue-500/10 transition-colors rounded-2xl"
                                    title="Clique para ver principais fornecedores"
                                >
                                    <div className="flex items-center justify-between">
                                        <div className="space-y-1">
                                            <p className="text-[10px] font-black text-blue-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">
                                                {isSenator ? "Cota Parlamentar (CEAPS)" : "Cota Parlamentar"}
                                            </p>
                                            <p className="text-2xl font-black text-blue-900 font-mono tracking-tighter">R$ {data.valor_total_ceap}</p>
                                        </div>
                                        <div className="p-3 bg-blue-500/10 text-blue-600 rounded-2xl shadow-inner border border-blue-500/10 group-hover:scale-110 transition-transform">
                                            <DollarSign className="h-6 w-6" />
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-1 mt-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                        <span className="text-[8px] font-medium text-blue-600">Ver fornecedores</span>
                                        <ArrowRight className="h-3 w-3 text-blue-600" />
                                    </div>
                                </button>
                            </Card>

                            <Card className="shadow-lg border-0 bg-orange-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
                                <button
                                    onClick={scrollToCustosAno}
                                    className="w-full p-6 text-left hover:bg-orange-500/10 transition-colors rounded-2xl"
                                    title="Clique para ver custos por ano"
                                >
                                    <div className="flex items-center justify-between">
                                        <div className="space-y-1">
                                            <p className="text-[10px] font-black text-orange-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">Verba de Gabinete</p>
                                            <p className="text-2xl font-black text-orange-900 font-mono tracking-tighter">R$ {data.valor_total_remuneracao}</p>
                                        </div>
                                        <div className="p-3 bg-orange-500/10 text-orange-600 rounded-2xl shadow-inner border border-orange-500/10 group-hover:scale-110 transition-transform">
                                            <Building2 className="h-6 w-6" />
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-1 mt-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                        <span className="text-[8px] font-medium text-orange-600">Ver custos por ano</span>
                                        <ArrowRight className="h-3 w-3 text-orange-600" />
                                    </div>
                                </button>
                            </Card>

                            {isFederal && (
                                <>
                                    <Card className="shadow-lg border-0 bg-purple-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
                                        <CardContent className="p-6">
                                            <div className="flex items-center justify-between">
                                                <div className="space-y-1">
                                                    <p className="text-[10px] font-black text-purple-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">Salário</p>
                                                    <p className="text-2xl font-black text-purple-900 font-mono tracking-tighter">R$ {data.valor_total_salario}</p>
                                                </div>
                                                <div className="p-3 bg-purple-500/10 text-purple-600 rounded-2xl shadow-inner border border-purple-500/10 group-hover:scale-110 transition-transform">
                                                    <TrendingUp className="h-6 w-6" />
                                                </div>
                                            </div>
                                        </CardContent>
                                    </Card>

                                    <Card className="shadow-lg border-0 bg-emerald-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
                                        <CardContent className="p-6">
                                            <div className="flex items-center justify-between">
                                                <div className="space-y-1">
                                                    <p className="text-[10px] font-black text-emerald-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">Auxílio Moradia</p>
                                                    <p className="text-2xl font-black text-emerald-900 font-mono tracking-tighter">R$ {data.valor_total_auxilio_moradia}</p>
                                                </div>
                                                <div className="p-3 bg-emerald-500/10 text-emerald-600 rounded-2xl shadow-inner border border-emerald-500/10 group-hover:scale-110 transition-transform">
                                                    <Users className="h-6 w-6" />
                                                </div>
                                            </div>
                                        </CardContent>
                                    </Card>
                                </>
                            )}
                        </div>
                    )}

                    <div className="grid gap-8 lg:grid-cols-2">
                        {/* Personal Information Card */}
                        <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
                            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                <div className="flex items-center gap-4">
                                    <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                                        <User className="h-6 w-6 text-primary" />
                                    </div>
                                    <CardTitle className="text-xl">Informações Pessoais</CardTitle>
                                </div>
                            </CardHeader>
                            <CardContent className="p-6">
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                                    <div className="flex items-start gap-2">
                                        <User className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                        <div>
                                            <span className="font-medium text-muted-foreground font-black uppercase text-[10px] tracking-wider">Nome Civil</span>
                                            <p className="text-foreground">{data.nome_civil}</p>
                                        </div>
                                    </div>

                                    {data.naturalidade && (
                                        <div className="flex items-start gap-2">
                                            <MapPin className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                            <div>
                                                <span className="font-medium text-muted-foreground font-black uppercase text-[10px] tracking-wider">Naturalidade</span>
                                                <p className="text-foreground">{data.naturalidade} {data.estado_nascimento ? `- ${data.estado_nascimento}` : ''}</p>
                                            </div>
                                        </div>
                                    )}

                                    <div className="flex items-start gap-2">
                                        <Calendar className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                        <div>
                                            <span className="font-medium text-muted-foreground font-black uppercase text-[10px] tracking-wider">Nascimento</span>
                                            <p className="text-foreground">{data.nascimento}</p>
                                        </div>
                                    </div>

                                    {data.falecimento && (
                                        <div className="flex items-start gap-2">
                                            <Calendar className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                            <div>
                                                <span className="font-medium text-muted-foreground font-black uppercase text-[10px] tracking-wider">Falecimento</span>
                                                <p className="text-foreground">{data.falecimento}</p>
                                            </div>
                                        </div>
                                    )}

                                    <div className="flex items-start gap-2">
                                        <MapPin className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                        <div>
                                            <span className="font-medium text-muted-foreground font-black uppercase text-[10px] tracking-wider">Estado</span>
                                            <p className="text-foreground">{data.nome_estado} ({data.sigla_estado})</p>
                                        </div>
                                    </div>

                                    <div className="flex items-start gap-2">
                                        <Building2 className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                        <div>
                                            <span className="font-medium text-muted-foreground font-black uppercase text-[10px] tracking-wider">Partido</span>
                                            <p className="text-foreground">{data.nome_partido} ({data.sigla_partido})</p>
                                        </div>
                                    </div>

                                    {data.escolaridade && (
                                        <div className="flex items-start gap-2">
                                            <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                            <div>
                                                <span className="font-medium text-muted-foreground font-black uppercase text-[10px] tracking-wider">Escolaridade</span>
                                                <p className="text-foreground">{data.escolaridade}</p>
                                            </div>
                                        </div>
                                    )}

                                    {data.profissao && (
                                        <div className="flex items-start gap-2">
                                            <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                            <div>
                                                <span className="font-medium text-muted-foreground font-black uppercase text-[10px] tracking-wider">Profissão</span>
                                                <p className="text-foreground">{data.profissao}</p>
                                            </div>
                                        </div>
                                    )}

                                    {data.condicao && (
                                        <div className="flex items-start gap-2">
                                            <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                            <div>
                                                <span className="font-medium text-muted-foreground font-black uppercase text-[10px] tracking-wider">Condição</span>
                                                <p className="text-foreground">{data.condicao}</p>
                                            </div>
                                        </div>
                                    )}

                                    {data.situacao && (
                                        <div className="flex items-start gap-2">
                                            <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                            <div>
                                                <span className="font-medium text-muted-foreground font-black uppercase text-[10px] tracking-wider">Situação</span>
                                                <p className="text-foreground">{data.situacao}</p>
                                            </div>
                                        </div>
                                    )}
                                </div>
                            </CardContent>
                        </Card>

                        {/* Annual Chart */}
                        <Card ref={custosAnoRef} className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
                            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                <div className="flex items-center gap-4">
                                    <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                                        <TrendingUp className="h-6 w-6 text-primary" />
                                    </div>
                                    <CardTitle className="text-xl">
                                        {isState ? "Gastos anuais com a cota parlamentar" : "Custos por Ano"}
                                    </CardTitle>
                                </div>
                            </CardHeader>
                            <CardContent className="p-6">
                                <ResponsiveContainer width="100%" height={300}>
                                    <BarChart data={chartData}>
                                        <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" vertical={false} />
                                        <XAxis dataKey="ano" tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} axisLine={false} tickLine={false} />
                                        <YAxis tickFormatter={formatValue} tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} axisLine={false} tickLine={false} />
                                        <Tooltip
                                            formatter={(value: number, name: string) => [formatCurrency(value), name === 'valor' ? 'Valor' : name]}
                                            labelFormatter={(label) => `Ano: ${label}`}
                                            contentStyle={{
                                                backgroundColor: 'hsl(var(--card))',
                                                border: '1px solid hsl(var(--border))',
                                                borderRadius: '12px',
                                                boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)',
                                                textAlign: 'right',
                                            }}
                                            cursor={{ fill: 'hsl(var(--muted))', opacity: 0.4 }}
                                        />
                                        {!isState && <Bar dataKey="verba_gabinete" stackId="a" fill="hsl(var(--chart-1))" radius={[0, 0, 0, 0]} name="Verba de Gabinete" className="transition-all duration-300 hover:opacity-80" />}
                                        <Bar dataKey="cota_parlamentar" stackId="a" fill="hsl(var(--chart-2))" radius={[0, 0, 0, 0]} name="Cota Parlamentar" className="transition-all duration-300 hover:opacity-80" />
                                        {isFederal && (
                                            <>
                                                <Bar dataKey="salario_patronal" stackId="a" fill="hsl(var(--chart-3))" radius={[0, 0, 0, 0]} name="Salário" className="transition-all duration-300 hover:opacity-80" />
                                                <Bar dataKey="auxilio_moradia" stackId="a" fill="hsl(var(--chart-4))" radius={[4, 4, 0, 0]} name="Auxílio Moradia" className="transition-all duration-300 hover:opacity-80" />
                                            </>
                                        )}
                                        {isState && (
                                            <>
                                                <Bar dataKey="auxilio_saude" stackId="a" fill="hsl(var(--chart-5))" radius={[0, 0, 0, 0]} name="Auxílio Saúde" className="transition-all duration-300 hover:opacity-80" />
                                                <Bar dataKey="diarias" stackId="a" fill="hsl(var(--chart-6))" radius={[4, 4, 0, 0]} name="Diárias" className="transition-all duration-300 hover:opacity-80" />
                                            </>
                                        )}
                                    </BarChart>
                                </ResponsiveContainer>

                                <div className="flex justify-center gap-6 mt-6 text-[10px] font-black uppercase tracking-widest flex-wrap">
                                    {(isFederal || isSenator) && (
                                        <>
                                            <div className="flex items-center gap-2">
                                                <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-1))' }}></div>
                                                <span className="text-muted-foreground">Verba de Gabinete</span>
                                            </div>
                                            {/* <Link
                                            to={folhaPagamentoUrl}
                                            className="flex items-center gap-2 hover:text-primary transition-colors cursor-pointer group"
                                            title="Clique para ver detalhes da folha de pagamento"
                                        >
                                            <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-1))' }}></div>
                                            <span className="text-muted-foreground group-hover:text-primary">Verba de Gabinete</span>
                                            <ArrowRight className="h-3 w-3 opacity-0 group-hover:opacity-100 transition-opacity" />
                                        </Link> */}
                                        </>
                                    )}
                                    <div className="flex items-center gap-2">
                                        <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-2))' }}></div>
                                        <span className="text-muted-foreground">Cota Parlamentar</span>
                                    </div>
                                    {isFederal && (
                                        <>
                                            <div className="flex items-center gap-2">
                                                <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-3))' }}></div>
                                                <span className="text-muted-foreground">Salário</span>
                                            </div>
                                            <div className="flex items-center gap-2">
                                                <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-4))' }}></div>
                                                <span className="text-muted-foreground">Auxílio Moradia</span>
                                            </div>
                                        </>
                                    )}
                                    {isState && (
                                        <>
                                            <div className="flex items-center gap-2">
                                                <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-5))' }}></div>
                                                <span className="text-muted-foreground">Diárias</span>
                                            </div>
                                            <div className="flex items-center gap-2">
                                                <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-6))' }}></div>
                                                <span className="text-muted-foreground">Auxílio Saúde</span>
                                            </div>
                                        </>
                                    )}
                                </div>

                            </CardContent>
                        </Card>
                    </div>

                    {/* SC State Warning */}
                    {isState && data.sigla_estado === 'SC' && (
                        <Alert className="border-yellow-200 bg-yellow-50">
                            <AlertDescription className="text-yellow-800">
                                Algumas notas podem não ter um fornecedor identificado e com isso não serão apresentadas abaixo!
                            </AlertDescription>
                        </Alert>
                    )}

                    <div className="grid gap-8 lg:grid-cols-2 mb-8">
                        {/* Principais Fornecedores */}
                        <Card ref={fornecedoresRef} className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300">
                            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                <div className="flex items-center justify-between">
                                    <div className="flex items-center gap-4">
                                        <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                                            <Building2 className="h-6 w-6 text-primary" />
                                        </div>
                                        <CardTitle className="text-xl">Principais Fornecedores ({!isState ? 'CEAPS' : 'CEAP'})</CardTitle>
                                    </div>
                                    <Link
                                        to={`${breadcrumbLink}/ceap?IdParlamentar=${data.id}&Periodo=0&Agrupamento=3`}
                                        className="px-3 py-1.5 text-xs font-bold bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-all shadow-md hover:shadow-lg active:scale-95"
                                    >
                                        Lista completa
                                    </Link>
                                </div>
                            </CardHeader>
                            <CardContent className="p-0">
                                <Table>
                                    <TableHeader className="bg-muted/30">
                                        <TableRow className="hover:bg-transparent">
                                            <TableHead className="py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b">Fornecedor</TableHead>
                                            <TableHead className="text-right py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b">Valor Total</TableHead>
                                        </TableRow>
                                    </TableHeader>
                                    <TableBody>
                                        {fornecedores.length > 0 ? (
                                            fornecedores.map((row) => (
                                                <TableRow key={row.id_fornecedor} className="hover:bg-muted/30 transition-all duration-300 border-b last:border-0 group">
                                                    <TableCell className="py-4 px-6">
                                                        <Link
                                                            to={`/fornecedor/${row.id_fornecedor}`}
                                                            className="font-bold text-primary hover:text-primary/80 transition-colors flex flex-col">
                                                            {row.nome_fornecedor}
                                                            {row.cnpj_cpf && (
                                                                <span className="font-mono text-[10px] font-black text-muted-foreground uppercase tracking-tight opacity-70 group-hover:opacity-100 transition-opacity">
                                                                    {row.cnpj_cpf}
                                                                </span>
                                                            )}
                                                        </Link>
                                                    </TableCell>
                                                    <TableCell className="text-right py-4 px-6 font-black font-mono text-foreground group-hover:text-primary transition-colors">
                                                        {isSenator ? (
                                                            <Link
                                                                to={`${breadcrumbLink}/ceap?IdParlamentar=${data.id}&Fornecedor=${row.id_fornecedor}&Periodo=0&Agrupamento=6`}
                                                                className="hover:underline underline-offset-4"
                                                            >
                                                                R$&nbsp;{row.valor_total}
                                                            </Link>
                                                        ) : `R$ ${row.valor_total}`}
                                                    </TableCell>
                                                </TableRow>
                                            ))
                                        ) : (
                                            <TableRow>
                                                <TableCell colSpan={2} className="text-center text-muted-foreground py-12">
                                                    <div className="flex flex-col items-center gap-3">
                                                        <div className="p-4 bg-muted/50 rounded-full">
                                                            <Building2 className="h-8 w-8 text-muted-foreground/30" />
                                                        </div>
                                                        <span className="text-sm font-medium uppercase tracking-widest opacity-50">Nenhum fornecedor encontrado</span>
                                                    </div>
                                                </TableCell>
                                            </TableRow>
                                        )}
                                    </TableBody>
                                </Table>
                            </CardContent>
                        </Card>

                        {/* Maiores Notas/Recibos */}
                        <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300">
                            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                <div className="flex items-center justify-between">
                                    <div className="flex items-center gap-4">
                                        <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                                            {isSenator ? <Receipt className="h-6 w-6 text-primary" /> : <DollarSign className="h-6 w-6 text-primary" />}
                                        </div>
                                        <CardTitle className="text-xl">Maiores Notas/Recibos</CardTitle>
                                    </div>
                                    <Link
                                        to={`${breadcrumbLink}/ceap?IdParlamentar=${data.id}&Periodo=0&Agrupamento=6`}
                                        className="px-3 py-1.5 text-xs font-bold bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-all shadow-md hover:shadow-lg active:scale-95"
                                    >
                                        Lista completa
                                    </Link>
                                </div>
                            </CardHeader>
                            <CardContent className="p-0">
                                {maioresNotas?.length > 0 ? (
                                    <Table>
                                        <TableHeader className="bg-muted/30">
                                            <TableRow className="hover:bg-transparent">
                                                <TableHead className="py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b" style={{ width: '80%' }}>Fornecedor</TableHead>
                                                <TableHead className="text-right py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b" style={{ width: '20%' }}>Valor</TableHead>
                                            </TableRow>
                                        </TableHeader>
                                        <TableBody>
                                            {maioresNotas.map((row, index) => (
                                                <TableRow key={index} className="hover:bg-muted/30 transition-all duration-300 border-b last:border-0 group">
                                                    <TableCell className="py-4 px-6">
                                                        <Link to={`/fornecedor/${row.id_fornecedor}`}
                                                            className="font-bold text-primary hover:text-primary/80 transition-colors flex flex-col">
                                                            {row.nome_fornecedor}
                                                            <span className="font-mono text-[10px] font-black text-muted-foreground uppercase tracking-tight opacity-70 group-hover:opacity-100 transition-opacity">
                                                                {row.cnpj_cpf}
                                                            </span>
                                                        </Link>
                                                    </TableCell>
                                                    <TableCell className="text-right py-4 px-6">
                                                        <Link
                                                            to={`${breadcrumbLink}/ceap/${row.id_despesa}`}
                                                            className="text-foreground hover:text-primary transition-colors font-black font-mono"
                                                        >
                                                            R$&nbsp;{row.valor_liquido}
                                                        </Link>
                                                    </TableCell>
                                                </TableRow>
                                            ))}
                                        </TableBody>
                                    </Table>
                                ) : (
                                    <div className="text-center text-muted-foreground py-12">
                                        <div className="flex flex-col items-center gap-3">
                                            <div className="p-4 bg-muted/50 rounded-full">
                                                <DollarSign className="h-8 w-8 text-muted-foreground/30" />
                                            </div>
                                            <span className="text-sm font-medium uppercase tracking-widest opacity-50">Nenhuma nota/recibo encontrado</span>
                                        </div>
                                    </div>
                                )}
                            </CardContent>
                        </Card>
                    </div>
                </div>
            </main>
            <Footer />
        </div >
    );
};

export default ParlamentarDetalhe;
