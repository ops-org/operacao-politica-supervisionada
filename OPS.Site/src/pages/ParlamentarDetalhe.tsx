import { useParams, Link } from "react-router-dom";
import { useState, useEffect } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
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
import {
    apiClient,
    fetchDeputadoDetalhe,
    fetchMaioresFornecedores,
    fetchCustoAnual,
    fetchSenadorDetalhe,
    fetchDeputadoEstadualData,
    DeputadoDetalhe as DeputadoFederalType,
    SenadorDetalhe as SenadorType,
    DeputadoEstadual as DeputadoEstadualType
} from "@/lib/api";

interface UnifiedParliamentarian {
    id: string;
    type: "deputado-federal" | "deputado-estadual" | "senador";
    nome_parlamentar: string;
    nome_civil: string;
    sigla_partido: string;
    nome_partido: string;
    sigla_estado: string;
    nome_estado: string;
    foto_url: string;
    pagina_oficial_url: string;
    valor_total: string;
    condicao?: string;
    situacao?: string;
    email?: string;
    telefone?: string;
    sala?: string;
    predio?: string;
    secretarios_ativos?: string;
    naturalidade?: string;
    nascimento?: string;
    falecimento?: string;
    profissao?: string;
    escolaridade?: string;
    municipio_nascimento?: string;
    estado_nascimento?: string;
    site?: string;

    // Summary values
    valor_total_ceap?: string;
    valor_total_remuneracao?: string;
    valor_total_salario?: string;
    valor_total_auxilio_moradia?: string;
}

const ParlamentarDetalhe = ({ type }: { type: "deputado-federal" | "deputado-estadual" | "senador" }) => {
    const { id } = useParams();
    const [data, setData] = useState<UnifiedParliamentarian | null>(null);
    const [fornecedores, setFornecedores] = useState<any[]>([]);
    const [chartData, setChartData] = useState<any[]>([]);
    const [maioresNotas, setMaioresNotas] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

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
                if (type === "deputado-federal") {
                    const [dep, forn, cust, notas] = await Promise.all([
                        fetchDeputadoDetalhe(id),
                        fetchMaioresFornecedores(id),
                        fetchCustoAnual(id, type),
                        apiClient.get<any[]>(`/api/deputado/${id}/MaioresNotas`)
                    ]);

                    setData({
                        id: dep.id_cf_deputado.toString(),
                        type: "deputado-federal",
                        nome_parlamentar: dep.nome_parlamentar,
                        nome_civil: dep.nome_civil,
                        sigla_partido: dep.sigla_partido,
                        nome_partido: dep.nome_partido,
                        sigla_estado: dep.sigla_estado,
                        nome_estado: dep.nome_estado,
                        foto_url: `https://static.ops.org.br/depfederal/${dep.id_cf_deputado}_120x160.jpg`,
                        pagina_oficial_url: `https://www.camara.leg.br/deputados/${dep.id_cf_deputado}`,
                        valor_total: dep.valor_total,
                        condicao: dep.condicao,
                        situacao: dep.situacao,
                        email: dep.email,
                        telefone: dep.telefone,
                        sala: dep.sala,
                        predio: dep.predio,
                        secretarios_ativos: dep.secretarios_ativos,
                        naturalidade: dep.nome_municipio_nascimento,
                        nascimento: dep.nascimento,
                        falecimento: dep.falecimento,
                        profissao: dep.profissao,
                        escolaridade: dep.escolaridade,
                        municipio_nascimento: dep.nome_municipio_nascimento,
                        estado_nascimento: dep.sigla_estado_nascimento,
                        valor_total_ceap: dep.valor_total_ceap,
                        valor_total_remuneracao: dep.valor_total_remuneracao,
                        valor_total_salario: dep.valor_total_salario,
                        valor_total_auxilio_moradia: dep.valor_total_auxilio_moradia
                    });
                    setFornecedores(forn);
                    setChartData(cust);
                    setMaioresNotas(notas);
                } else if (type === "senador") {
                    const [sen, gstAnu, gstPes, notas, forn] = await Promise.all([
                        fetchSenadorDetalhe(id),
                        apiClient.get<any>(`/senador/${id}/GastosPorAno`),
                        apiClient.get<any>(`/senador/${id}/GastosComPessoalPorAno`),
                        apiClient.get<any[]>(`/senador/${id}/MaioresNotas`),
                        apiClient.get<any[]>(`/senador/${id}/MaioresFornecedores`)
                    ]);

                    setData({
                        id: sen.id_sf_senador.toString(),
                        type: "senador",
                        nome_parlamentar: sen.nome_parlamentar,
                        nome_civil: sen.nome_civil,
                        sigla_partido: sen.sigla_partido,
                        nome_partido: sen.nome_partido,
                        sigla_estado: sen.sigla_estado,
                        nome_estado: sen.nome_estado,
                        foto_url: `https://static.ops.org.br/senador/${sen.id_sf_senador}_240x300.jpg`,
                        pagina_oficial_url: `http://www25.senado.leg.br/web/senadores/senador/-/perfil/${sen.id_sf_senador}`,
                        valor_total: sen.valor_total,
                        condicao: sen.condicao,
                        email: sen.email,
                        naturalidade: sen.naturalidade,
                        nascimento: sen.nascimento,
                        valor_total_ceap: sen.valor_total_ceaps,
                        valor_total_remuneracao: sen.valor_total_remuneracao
                    });
                    setFornecedores(forn);
                    setMaioresNotas(notas);

                    const allYears = new Set([
                        ...(gstAnu?.categories || []),
                        ...(gstPes?.categories || [])
                    ]);
                    const mergedChart = Array.from(allYears).map(year => {
                        const ceapsIndex = gstAnu?.categories?.indexOf(year);
                        const pessoalIndex = gstPes?.categories?.indexOf(year);
                        return {
                            ano: year.toString(),
                            cota_parlamentar: ceapsIndex !== undefined && ceapsIndex >= 0 ? (gstAnu?.series?.[ceapsIndex] || 0) : 0,
                            verba_gabinete: pessoalIndex !== undefined && pessoalIndex >= 0 ? (gstPes?.series?.[pessoalIndex] || 0) : 0
                        };
                    }).sort((a, b) => a.ano.localeCompare(b.ano));
                    setChartData(mergedChart);
                } else if (type === "deputado-estadual") {
                    const [res, custos] = await Promise.all([
                        fetchDeputadoEstadualData(id),
                        fetchCustoAnual(id, type)
                    ]);

                    const dep = res.deputado;
                    setData({
                        id: dep.id_cl_deputado.toString(),
                        type: "deputado-estadual",
                        nome_parlamentar: dep.nome_parlamentar,
                        nome_civil: dep.nome_civil,
                        sigla_partido: dep.sigla_partido,
                        nome_partido: dep.nome_partido,
                        sigla_estado: dep.sigla_estado,
                        nome_estado: dep.nome_estado,
                        foto_url: dep.foto || "",
                        pagina_oficial_url: dep.perfil || "",
                        valor_total: dep.valor_total,
                        email: dep.email,
                        telefone: dep.telefone,
                        site: dep.site,
                        naturalidade: dep.naturalidade,
                        nascimento: dep.nascimento,
                        profissao: dep.profissao
                    });
                    setFornecedores(res.maioresFornecedores);
                    setMaioresNotas(res.maioresNotas);
                    setChartData(custos);
                }
            } catch (err) {
                console.error(err);
                setError(err instanceof Error ? err.message : "Erro ao carregar dados do parlamentar");
            } finally {
                setLoading(false);
            }
        };

        loadData();
    }, [id, type]);

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

    const isFederal = type === "deputado-federal";
    const isState = type === "deputado-estadual";
    const isSenator = type === "senador";

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
                                        <div className="bg-gradient-to-br from-primary to-primary/80 rounded-2xl p-6 shadow-xl shadow-primary/20 text-white transform hover:scale-105 transition-transform duration-300">
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
                                        </div>
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
                        <div className={`grid grid-cols-1 md:grid-cols-2 ${isFederal ? "lg:grid-cols-4" : ""} gap-6`}>
                            <Card className="shadow-lg border-0 bg-blue-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
                                <CardContent className="p-6">
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
                                </CardContent>
                            </Card>

                            <Card className="shadow-lg border-0 bg-orange-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
                                <CardContent className="p-6">
                                    <div className="flex items-center justify-between">
                                        <div className="space-y-1">
                                            <p className="text-[10px] font-black text-orange-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">Verba de Gabinete</p>
                                            <p className="text-2xl font-black text-orange-900 font-mono tracking-tighter">R$ {data.valor_total_remuneracao}</p>
                                        </div>
                                        <div className="p-3 bg-orange-500/10 text-orange-600 rounded-2xl shadow-inner border border-orange-500/10 group-hover:scale-110 transition-transform">
                                            <Building2 className="h-6 w-6" />
                                        </div>
                                    </div>
                                </CardContent>
                            </Card>

                            {isFederal && (
                                <>
                                    <Card className="shadow-lg border-0 bg-purple-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
                                        <CardContent className="p-6">
                                            <div className="flex items-center justify-between">
                                                <div className="space-y-1">
                                                    <p className="text-[10px] font-black text-purple-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">Salário Bruto</p>
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
                        <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
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
                                                <Bar dataKey="salario_patronal" stackId="a" fill="hsl(var(--chart-3))" radius={[0, 0, 0, 0]} name="Salário Bruto" className="transition-all duration-300 hover:opacity-80" />
                                                <Bar dataKey="auxilio_moradia" stackId="a" fill="hsl(var(--chart-4))" radius={[4, 4, 0, 0]} name="Auxílio Moradia" className="transition-all duration-300 hover:opacity-80" />
                                            </>
                                        )}
                                        {isState && (
                                            <>
                                                <Bar dataKey="auxilio_saude" stackId="a" fill="hsl(var(--chart-3))" radius={[0, 0, 0, 0]} name="Auxílio Saúde" className="transition-all duration-300 hover:opacity-80" />
                                                <Bar dataKey="diarias" stackId="a" fill="hsl(var(--chart-4))" radius={[4, 4, 0, 0]} name="Diárias" className="transition-all duration-300 hover:opacity-80" />
                                            </>
                                        )}
                                    </BarChart>
                                </ResponsiveContainer>
                                {(isFederal || isSenator) && (
                                    <div className="flex justify-center gap-6 mt-6 text-[10px] font-black uppercase tracking-widest flex-wrap">
                                        <div className="flex items-center gap-2">
                                            <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-3))' }}></div>
                                            <span className="text-muted-foreground">Verba de Gabinete</span>
                                        </div>
                                        <div className="flex items-center gap-2">
                                            <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-2))' }}></div>
                                            <span className="text-muted-foreground">Cota Parlamentar</span>
                                        </div>
                                        {isFederal && (
                                            <>
                                                <div className="flex items-center gap-2">
                                                    <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-4))' }}></div>
                                                    <span className="text-muted-foreground">Salário Bruto</span>
                                                </div>
                                                <div className="flex items-center gap-2">
                                                    <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-5))' }}></div>
                                                    <span className="text-muted-foreground">Auxílio Moradia</span>
                                                </div>
                                            </>
                                        )}
                                    </div>
                                )}
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
                        <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300">
                            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                <div className="flex items-center justify-between">
                                    <div className="flex items-center gap-4">
                                        <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                                            <Building2 className="h-6 w-6 text-primary" />
                                        </div>
                                        <CardTitle className="text-xl">Principais Fornecedores ({!isState ? 'CEAPS' : 'CEAP'})</CardTitle>
                                    </div>
                                    <Link
                                        to={`${breadcrumbLink}/ceap?IdParlamentar=${data.id}&Periodo=${isSenator ? '0' : '57'}&Agrupamento=3`}
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
                                        to={`${breadcrumbLink}/ceap?IdParlamentar=${data.id}&Periodo=${isSenator ? '0' : '57'}&Agrupamento=6`}
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
                                                        {isFederal ? (
                                                            <Link
                                                                to={`${breadcrumbLink}/ceap/${row.id_cf_despesa}`}
                                                                className="text-foreground hover:text-primary transition-colors font-black font-mono"
                                                            >
                                                                R$&nbsp;{row.valor_liquido}
                                                            </Link>
                                                        ) : (
                                                            <span className="font-black font-mono text-foreground group-hover:text-primary transition-colors">
                                                                R$&nbsp;{row.valor_liquido || row.valor}
                                                            </span>
                                                        )}
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
