import { useParams, Link } from "react-router-dom";
import { useState, useEffect } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { ErrorState } from "@/components/ErrorState";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { FornecedorDetalheSkeleton } from "@/components/FornecedorDetalheSkeleton";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ChevronDown, ChevronUp, ExternalLink, Building2, MapPin, Phone, Mail, Calendar, DollarSign, Briefcase, Users, TrendingUp, RefreshCw, FileBadge, Copy, ShieldX, ShieldCheck, CircleUserRound, Building, Earth, CakeSlice, ClipboardList, Drama, MapPinned, AlertTriangle, ArrowRight } from "lucide-react";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { fetchFornecedorDetalhe, fetchRecebimentosPorAno, fetchMaioresGastos, FornecedorDetalheResponse, QuadroSocietario, RecebimentosPorAno, MaiorGasto } from "@/lib/api";
import { AnnualSummaryChart } from "@/components/AnnualSummaryChart";
import { formatBrazilianPhone } from "@/lib/utils";
import { Alert, AlertDescription } from "@/components/ui/alert";

const FornecedorDetalhe = () => {
    const { id } = useParams();
    const [data, setData] = useState<FornecedorDetalheResponse | null>(null);
    const [recebimentosPorAno, setRecebimentosPorAno] = useState<RecebimentosPorAno | null>(null);
    const [maioresGastos, setMaioresGastos] = useState<MaiorGasto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [showDetailedInfo, setShowDetailedInfo] = useState(false);

    usePageTitle(data?.fornecedor ? (data.fornecedor.nome_fantasia || data.fornecedor.nome) : "Fornecedor");

    useEffect(() => {
        const loadData = async () => {
            if (!id) {
                setError("ID do fornecedor não fornecido");
                setLoading(false);
                return;
            }

            try {
                const [fornecedorData, recebimentosData, gastosData] = await Promise.all([
                    fetchFornecedorDetalhe(id),
                    fetchRecebimentosPorAno(id),
                    fetchMaioresGastos(id)
                ]);
                setData(fornecedorData);
                setRecebimentosPorAno(recebimentosData);
                setMaioresGastos(gastosData);
            } catch (err) {
                setError(err instanceof Error ? err.message : "Erro ao carregar os dados do fornecedor");
            } finally {
                setLoading(false);
            }
        };

        loadData();
    }, [id]);

    if (error) {
        return (
            <ErrorState
                title="Erro ao carregar fornecedor"
                message={error || "Não foi possível encontrar as informações deste fornecedor. Verifique se o ID está correto ou tente novamente mais tarde."}
            />
        );
    }

    if (!data) {
        return <FornecedorDetalheSkeleton />;
    }

    const { fornecedor, quadro_societario } = data;
    const enderecoCompleto = `${fornecedor.logradouro}, ${fornecedor.numero}${fornecedor.complemento ? ' - ' + fornecedor.complemento : ''}, ${fornecedor.cep} - ${fornecedor.bairro}, ${fornecedor.cidade}, ${fornecedor.estado}`;
    const hasAdditionalInfo = fornecedor.motivo_situacao_cadastral || fornecedor.situacao_especial || fornecedor.data_situacao_especial || fornecedor.ente_federativo_responsavel;

    return (
        <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
            {/* Full-screen loading overlay */}
            <LoadingOverlay isLoading={loading} content="Carregando informações do fornecedor..." />

            <Header />
            <main className="container mx-auto px-4 py-8">
                {/* Breadcrumb */}
                <div className="flex items-center gap-2 text-sm text-muted-foreground mb-8">
                    <Link to="/busca" className="hover:text-foreground transition-colors">
                        Busca
                    </Link>
                    <ArrowRight className="h-4 w-4" />
                    <span className="text-foreground">Perfil do Fornecedor</span>
                </div>

                <div className="space-y-8">
                    {/* Profile Card with Modern Design */}
                    <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300 border-t-4 border-t-primary">
                        <div className={`relative overflow-hidden ${fornecedor.situacao_cadastral === 'ATIVA'
                            ? "bg-gradient-to-r from-primary/10 to-accent/5"
                            : "bg-gradient-to-r from-slate-500/10 to-transparent"
                            }`}>
                            {/* Animated geometric shapes for premium feel */}
                            <div className="absolute top-[-20%] right-[-10%] w-64 h-64 bg-primary/5 rounded-full blur-3xl" />
                            <div className="absolute bottom-[-50%] left-[-10%] w-80 h-80 bg-accent/5 rounded-full blur-3xl" />

                            <div className="p-8 relative z-10">
                                <div className="flex flex-col md:flex-row gap-8 items-center md:items-start">
                                    {/* Icon Section */}
                                    <div className="flex-shrink-0">
                                        <div className="relative group">
                                            <div className="absolute -inset-1 bg-gradient-to-br from-primary to-accent rounded-2xl blur opacity-25 group-hover:opacity-50 transition duration-1000 group-hover:duration-200"></div>
                                            <div className="relative w-28 h-28 rounded-2xl bg-card flex items-center justify-center shadow-inner border border-primary/10 transition-transform duration-500 group-hover:scale-105">
                                                <Building2 className="h-14 w-14 text-primary" />
                                            </div>
                                        </div>
                                    </div>

                                    {/* Main Info Section */}
                                    <div className="flex-1 space-y-4 text-center md:text-left">
                                        <div className="space-y-2">
                                            <div className="flex items-center justify-center md:justify-start gap-3">
                                                <h2 className="text-3xl font-black text-foreground tracking-tight">
                                                    {fornecedor.nome_fantasia || fornecedor.nome}
                                                </h2>
                                                <Button
                                                    variant="ghost"
                                                    size="icon"
                                                    className="h-8 w-8 text-muted-foreground hover:text-primary hover:bg-primary/10 rounded-full"
                                                    onClick={() => navigator.clipboard.writeText(fornecedor.nome_fantasia || fornecedor.nome)}
                                                >
                                                    <Copy className="h-4 w-4" />
                                                </Button>
                                            </div>

                                            <div className="flex flex-wrap justify-center md:justify-start gap-3 items-center">
                                                <Badge variant="outline" className="font-mono text-sm px-3 py-1 bg-background/50 backdrop-blur-sm border-muted-foreground/20">
                                                    {fornecedor.cnpj_cpf}
                                                </Badge>
                                                {fornecedor.tipo && <Badge
                                                    className="bg-primary/10 text-primary border-primary/20 hover:bg-primary/20 transition-colors px-3 py-1"
                                                >
                                                    {fornecedor.tipo}
                                                </Badge>}
                                                {/* Status Badge */}
                                                {fornecedor.situacao_cadastral && <div className="flex items-center gap-1.5 ml-2">
                                                    {fornecedor.situacao_cadastral === "ATIVA" ? (
                                                        <Badge className="bg-green-500/10 text-green-600 border-green-500/20 px-3 py-1 flex items-center gap-1">
                                                            <ShieldCheck className="w-3.5 h-3.5" />
                                                            {fornecedor.situacao_cadastral}
                                                        </Badge>
                                                    ) : (
                                                        <Badge variant="destructive" className="px-3 py-1 flex items-center gap-1">
                                                            <ShieldX className="w-3.5 h-3.5" />
                                                            {fornecedor.situacao_cadastral}
                                                        </Badge>
                                                    )}
                                                </div>}
                                            </div>

                                            {fornecedor.data_de_abertura && <div className="flex flex-wrap justify-center md:justify-start gap-x-6 gap-y-2 text-sm text-muted-foreground font-medium pt-2">
                                                {/* Operation Period */}
                                                <div className="flex items-center gap-2">
                                                    <Calendar className="w-4 h-4 text-primary/70" />
                                                    <span>
                                                        Abertura: <span className="text-foreground">{fornecedor.data_de_abertura}</span>
                                                        {fornecedor.situacao_cadastral !== "ATIVA" && (
                                                            <> • Encerramento: <span className="text-foreground">{fornecedor.data_da_situacao_cadastral}</span></>
                                                        )}
                                                    </span>
                                                </div>
                                            </div>}

                                            {fornecedor.nome_fantasia && fornecedor.nome_fantasia !== fornecedor.nome && (
                                                <p className="text-sm text-muted-foreground italic mt-2">Razão Social: {fornecedor.nome}</p>
                                            )}
                                        </div>
                                    </div>

                                    {/* Total Recebimentos Display */}
                                    <div className="text-center md:text-right space-y-2 lg:min-w-[280px]">
                                        <div className="bg-gradient-to-br from-primary to-primary/80 rounded-2xl p-6 shadow-xl shadow-primary/20 text-white transform hover:scale-105 transition-transform duration-300">
                                            <div className="flex items-center justify-center md:justify-end gap-2 mb-1 opacity-90">
                                                <TrendingUp className="h-4 w-4" />
                                                <p className="text-xs font-bold uppercase tracking-widest">Total Recebido</p>
                                            </div>
                                            <p className="text-4xl font-black font-mono tracking-tighter">
                                                R$ {recebimentosPorAno?.series?.reduce((a, b) => a + b, 0).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) || "0"}
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Detailed Info Section */}
                        {fornecedor.categoria == "PJ" && <div className="p-6">
                            <div className="space-y-4">
                                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 text-sm">
                                    {/* Legal Nature */}
                                    <div className="flex items-center gap-1.5">
                                        <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path d="m14 13-8.381 8.38a1 1 0 0 1-3.001-3l8.384-8.381"></path>
                                            <path d="m16 16 6-6"></path>
                                            <path d="m21.5 10.5-8-8"></path>
                                            <path d="m8 8 6-6"></path>
                                            <path d="m8.5 7.5 8 8"></path>
                                        </svg>
                                        <span>{fornecedor.natureza_juridica}</span>
                                    </div>

                                    {/* Capital Social */}
                                    <div className="flex items-center gap-1.5">
                                        <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path d="M9 5v4"></path>
                                            <rect x="7" y="9" width="4" height="6" rx="1"></rect>
                                            <path d="M9 15v2"></path>
                                            <path d="M17 3v2"></path>
                                            <rect x="15" y="5" width="4" height="8" rx="1"></rect>
                                            <path d="M17 13v3"></path>
                                            <path d="M3 3v16a2 2 0 0 0 2 2h16"></path>
                                        </svg>
                                        <span>Capital Social: R$ {fornecedor.capital_social}</span>
                                    </div>

                                    {/* Email */}
                                    {fornecedor.endereco_eletronico && (
                                        <div className="flex items-center gap-1.5">
                                            <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path d="m22 7-8.991 5.727a2 2 0 0 1-2.009 0L2 7"></path>
                                                <rect x="2" y="4" width="20" height="16" rx="2"></rect>
                                            </svg>
                                            <span className="break-all">{fornecedor.endereco_eletronico}</span>
                                            <Button
                                                variant="ghost"
                                                size="sm"
                                                className="h-6 w-6 p-0"
                                                onClick={() => navigator.clipboard.writeText(fornecedor.endereco_eletronico)}
                                            >
                                                <Copy className="w-2 h-2" />
                                            </Button>
                                        </div>
                                    )}

                                    {/* Phone */}
                                    {fornecedor.telefone && (
                                        <div className="flex items-center gap-1.5">
                                            <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path d="M13.832 16.568a1 1 0 0 0 1.213-.303l.355-.465A2 2 0 0 1 17 15h3a2 2 0 0 1 2 2v3a2 2 0 0 1-2 2A18 18 0 0 1 2 4a2 2 0 0 1 2-2h3a2 2 0 0 1 2 2v3a2 2 0 0 1-.8 1.6l-.468.351a1 1 0 0 0-.292 1.233 14 14 0 0 0 6.392 6.384"></path>
                                            </svg>
                                            <span>{formatBrazilianPhone(fornecedor.telefone)}</span>
                                            <Button
                                                variant="ghost"
                                                size="sm"
                                                className="h-6 w-6 p-0"
                                                onClick={() => navigator.clipboard.writeText(formatBrazilianPhone(fornecedor.telefone))}
                                            >
                                                <Copy className="w-2 h-2" />
                                            </Button>
                                        </div>
                                    )}

                                    {/* Address */}
                                    <div className="flex items-center gap-1.5 lg:col-span-4">
                                        <MapPin className="w-3.5 h-3.5" />
                                        <div className="flex items-center gap-2">
                                            <span>{enderecoCompleto}</span>
                                            <Button
                                                variant="ghost"
                                                size="sm"
                                                className="h-6 w-6 p-0"
                                                onClick={() => navigator.clipboard.writeText(enderecoCompleto)}
                                            >
                                                <Copy className="w-2 h-2" />
                                            </Button>

                                            <Button
                                                variant="ghost"
                                                size="sm"
                                                className="h-6 w-6 p-0"
                                                title="Google Maps"
                                                onClick={() => {
                                                    window.open(`https://maps.google.com/maps?q=${encodeURIComponent(enderecoCompleto)}`, '_blank');
                                                }}
                                            >
                                                <MapPinned className="h-2 w-2" />
                                            </Button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>}

                        {/* Footer with Update Info */}
                        {fornecedor.obtido_em && <div className="flex flex-col justify-between gap-4 rounded-b-md border-t border-border/50 bg-muted/30 px-6 sm:flex-row sm:items-center py-3">
                            <div className="flex items-center gap-1.5 text-muted-foreground text-xs">
                                <svg className="w-3.5 h-3.5 text-primary" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path d="M18 6 7 17l-5-5"></path>
                                    <path d="m22 10-7.5 7.5L13 16"></path>
                                </svg>
                                <span>
                                    Atualizado em {fornecedor.obtido_em} via
                                    <a
                                        href={
                                            fornecedor.origem === "Receita Federal"
                                                ? "https://solucoes.receita.fazenda.gov.br/Servicos/cnpjreva/cnpjreva_solicitacao.asp"
                                                : fornecedor.origem === "Receita WS"
                                                    ? "https://www.receitaws.com.br/"
                                                    : fornecedor.origem === "Minha Receita"
                                                        ? "https://minhareceita.org/"
                                                        : "#"
                                        }
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="text-secondary underline-offset-4 hover:underline hover:text-primary p-0 cursor-pointer ml-1 inline-flex items-center gap-1"
                                    >
                                        {fornecedor.origem}
                                        <ExternalLink className="h-3 w-3" />
                                    </a>
                                </span>
                            </div>
                        </div>}
                    </Card>

                    {/* {!fornecedor.categoria &&  <Alert variant="warning" className="mt-4">
                        <AlertTriangle className="h-4 w-4" />
                        <AlertDescription>Este fornecedor foi cadastrado incorretamente ou incompleto devido a LGPD.</AlertDescription>
                    </Alert>} */}

                    {/* Detailed Information */}
                    {fornecedor.categoria == "PJ" && <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
                        <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                            <div className="flex items-center justify-between">
                                <div className="flex items-center gap-4">
                                    <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                                        <Briefcase className="h-6 w-6 text-primary" />
                                    </div>
                                    <CardTitle className="text-xl">Informações Detalhadas</CardTitle>
                                </div>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => setShowDetailedInfo(!showDetailedInfo)}
                                    className="flex items-center gap-2"
                                >
                                    {showDetailedInfo ? (
                                        <>
                                            <ChevronUp className="h-4 w-4" />
                                            Esconder
                                        </>
                                    ) : (
                                        <>
                                            <ChevronDown className="h-4 w-4" />
                                            Mostrar
                                        </>
                                    )}
                                </Button>
                            </div>
                        </CardHeader>
                        {showDetailedInfo && (
                            <CardContent className="p-6">
                                <div className="space-y-6">
                                    {/* Two Column Layout */}
                                    <div className="grid gap-6 lg:grid-cols-2">

                                        {/* Quadro Societário */}
                                        {quadro_societario && quadro_societario.length > 0 && (
                                            <div className="space-y-3">
                                                <div className="flex items-center gap-2">
                                                    <div className="h-px bg-border flex-1"></div>
                                                    <h4 className="font-semibold text-sm text-muted-foreground uppercase tracking-wide whitespace-nowrap">Sócios e Administradores</h4>
                                                    <div className="h-px bg-border flex-1"></div>
                                                </div>

                                                <div className="space-y-2">
                                                    {quadro_societario.map((socio, index) => (
                                                        <div key={index} className="border-l-2 border-muted pl-4 py-2">
                                                            <div className="flex items-center">
                                                                <Link
                                                                    to={`/busca?q=${encodeURIComponent(socio.nome)}`}
                                                                    className="text-sm font-medium hover:underline"
                                                                >
                                                                    {socio.nome}
                                                                </Link>
                                                                <Button
                                                                    variant="ghost"
                                                                    size="sm"
                                                                    className="h-6 w-6 p-0 hover:bg-muted"
                                                                    onClick={() => navigator.clipboard.writeText(socio.nome)}
                                                                >
                                                                    <Copy className="w-3 h-3 text-muted-foreground" />
                                                                </Button>
                                                            </div>
                                                            <div className="flex flex-wrap items-center gap-1 text-xs text-muted-foreground mt-1">
                                                                <span>{socio.qualificacao}</span>
                                                                <span className="text-muted-foreground/50">•</span>
                                                                <span>{socio.data_entrada_sociedade}</span>
                                                                <span className="text-muted-foreground/50">•</span>
                                                                <span>{socio.cnpj_cpf}</span>
                                                            </div>
                                                        </div>
                                                    ))}
                                                </div>
                                            </div>
                                        )}

                                        {/* Economic Activities */}
                                        <div className="space-y-4">
                                            <div className="flex items-center gap-2">
                                                <div className="h-px bg-border flex-1"></div>
                                                <h4 className="font-semibold text-sm text-muted-foreground uppercase tracking-wide whitespace-nowrap">Atividades Econômicas</h4>
                                                <div className="h-px bg-border flex-1"></div>
                                            </div>

                                            <div className="grid gap-4">
                                                {/* Primary Activity */}
                                                <div className="bg-gradient-to-r from-primary/5 to-primary/10 border border-primary/20 rounded-lg p-4">
                                                    <div className="flex items-center gap-2 mb-2">
                                                        <div className="w-2 h-2 bg-primary rounded-full"></div>
                                                        <span className="font-medium text-sm text-primary">Atividade Principal</span>
                                                    </div>
                                                    <p className="text-sm text-foreground">{fornecedor.atividade_principal}</p>
                                                </div>

                                                {/* Secondary Activities */}
                                                {fornecedor.atividade_secundaria.length > 0 && (
                                                    <div className="bg-muted/30 border border-muted rounded-lg p-4">
                                                        <div className="flex items-center gap-2 mb-3">
                                                            <div className="w-2 h-2 bg-muted-foreground rounded-full"></div>
                                                            <span className="font-medium text-sm text-muted-foreground">Atividades Secundárias</span>
                                                            <Badge variant="secondary" className="text-xs">
                                                                {fornecedor.atividade_secundaria.length}
                                                            </Badge>
                                                        </div>
                                                        <div className="space-y-2">
                                                            {fornecedor.atividade_secundaria.map((atividade, index) => (
                                                                <div key={index} className="flex items-start gap-2">
                                                                    <div className="w-1 h-1 bg-muted-foreground rounded-full mt-2 flex-shrink-0"></div>
                                                                    <p className="text-sm text-muted-foreground">{atividade}</p>
                                                                </div>
                                                            ))}
                                                        </div>
                                                    </div>
                                                )}
                                            </div>
                                        </div>

                                        {/* Additional Information */}
                                        {hasAdditionalInfo && <div className="space-y-4">
                                            <div className="flex items-center gap-2">
                                                <div className="h-px bg-border flex-1"></div>
                                                <h4 className="font-semibold text-sm text-muted-foreground uppercase tracking-wide whitespace-nowrap">Informações Cadastrais</h4>
                                                <div className="h-px bg-border flex-1"></div>
                                            </div>
                                            <div className="grid grid-cols-1 gap-4 text-sm">
                                                {fornecedor.motivo_situacao_cadastral && <div>
                                                    <span className="font-medium">Situação Cadastral:</span>
                                                    <p className="text-muted-foreground">{fornecedor.data_da_situacao_cadastral}</p>
                                                </div>}
                                                {fornecedor.motivo_situacao_cadastral && (
                                                    <div>
                                                        <span className="font-medium">Motivo:</span>
                                                        <p className="text-muted-foreground">{fornecedor.motivo_situacao_cadastral}</p>
                                                    </div>
                                                )}
                                                {fornecedor.situacao_especial && (
                                                    <div>
                                                        <span className="font-medium">Situação Especial:</span>
                                                        <p className="text-muted-foreground">{fornecedor.situacao_especial}</p>
                                                    </div>
                                                )}
                                                {fornecedor.data_situacao_especial && (
                                                    <div>
                                                        <span className="font-medium">Data Situação Especial:</span>
                                                        <p className="text-muted-foreground">{fornecedor.data_situacao_especial}</p>
                                                    </div>
                                                )}
                                                {fornecedor.ente_federativo_responsavel && (
                                                    <div className="md:col-span-2">
                                                        <span className="font-medium">Ente Federativo Responsável:</span>
                                                        <p className="text-muted-foreground">{fornecedor.ente_federativo_responsavel}</p>
                                                    </div>
                                                )}
                                            </div>
                                        </div>}
                                    </div>
                                </div>
                            </CardContent>
                        )}
                    </Card>}

                    {/* Charts and Tables */}
                    <div className="grid gap-8 lg:grid-cols-2">
                        {/* Annual Receipts Chart */}
                        {recebimentosPorAno && (
                            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
                                <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                    <div className="flex items-center gap-4">
                                        <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                                            <TrendingUp className="h-6 w-6 text-primary" />
                                        </div>
                                        <CardTitle className="text-xl">Recebimentos por Ano</CardTitle>
                                    </div>
                                </CardHeader>
                                <CardContent className="py-4">
                                    <AnnualSummaryChart
                                        data={recebimentosPorAno.categories.map((year, index) => ({
                                            year: year.toString(),
                                            value: Math.round(recebimentosPorAno.series[index] || 0)
                                        }))}
                                    />
                                </CardContent>
                            </Card>
                        )}

                        {/* Top Parliamentarians Table */}
                        {maioresGastos.length > 0 && (
                            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300">
                                <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center gap-4">
                                            <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                                                <Users className="h-6 w-6 text-primary" />
                                            </div>
                                            <CardTitle className="text-xl">Parlamentares (Maiores Gastos)</CardTitle>
                                        </div>
                                    </div>
                                </CardHeader>
                                <CardContent className="p-0">
                                    <div className="px-6 pt-6 mb-4 text-sm">
                                        Ver lista completa por{" "}
                                        <a href="/deputado-federal/ceap?Fornecedor=186&Periodo=0&Agrupamento=6" className="text-primary hover:underline inline-flex items-center gap-1 ml-1">Deputado Federal <ExternalLink className="h-3 w-3" /></a> ou{" "}
                                        <a href="/senador?Fornecedor=186&Periodo=0&Agrupamento=6" className="text-primary hover:underline inline-flex items-center gap-1 ml-1">Senador <ExternalLink className="h-3 w-3" /></a>
                                    </div>
                                    <Table>
                                        <TableHeader className="bg-muted/30">
                                            <TableRow className="hover:bg-transparent">
                                                <TableHead className="py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b">Parlamentar</TableHead>
                                                <TableHead className="py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b">Tipo</TableHead>
                                                <TableHead className="py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b">Último Recibo</TableHead>
                                                <TableHead className="text-right py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b">Valor</TableHead>
                                            </TableRow>
                                        </TableHeader>
                                        <TableBody>
                                            {maioresGastos.map((row) => (
                                                <TableRow key={row.id} className="hover:bg-muted/30 transition-all duration-300 border-b last:border-0 group">
                                                    <TableCell className="py-4 px-6">
                                                        <Link
                                                            to={row.link_parlamentar.replace("#/", "")}
                                                            className="font-bold text-primary hover:text-primary/80 transition-colors block"
                                                        >
                                                            {row.nome_parlamentar}
                                                            <div className="font-mono text-[10px] font-black text-muted-foreground mt-1 uppercase tracking-tight opacity-70 group-hover:opacity-100 transition-opacity">
                                                                {row.sigla_partido} / {row.sigla_estado}
                                                            </div>
                                                        </Link>
                                                    </TableCell>
                                                    <TableCell className="py-4 px-6">
                                                        {row.tipo}
                                                    </TableCell>
                                                    <TableCell className="py-4 px-6 text-sm font-medium text-muted-foreground whitespace-nowrap">{row.ultima_emissao}</TableCell>
                                                    <TableCell className="text-right py-4 px-6">
                                                        <Link
                                                            to={row.link_despesas.replace("#/", "").replace("/deputado-federal", "/deputado-federal/ceap").replace("/deputado-estadual", "/deputado-estadual/ceap")}
                                                            className="hover:text-primary transition-colors font-black font-mono text-foreground"
                                                        >
                                                            R$&nbsp;{row.valor_total}
                                                        </Link>
                                                    </TableCell>
                                                </TableRow>
                                            ))}
                                        </TableBody>
                                    </Table>
                                </CardContent>
                            </Card>
                        )}
                    </div>
                </div>
            </main >
            <Footer />
        </div >
    );
};

export default FornecedorDetalhe;
