import { useParams, Link } from "react-router-dom";
import { useState, useEffect } from "react";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ChevronDown, ChevronUp, ExternalLink, Building2, MapPin, Phone, Mail, Calendar, DollarSign, Briefcase, Users, TrendingUp } from "lucide-react";
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
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { formatBrazilianPhone } from "@/lib/utils";

const formatCurrency = (value: string): string => {
    const numericValue = parseFloat(value.replace(/[^\d,.-]/g, '').replace(',', '.'));
    return numericValue.toLocaleString("pt-BR", {
        style: "currency",
        currency: "BRL",
    });
};

const FornecedorDetalhe = () => {
    const { id } = useParams();
    const [data, setData] = useState<FornecedorDetalheResponse | null>(null);
    const [recebimentosPorAno, setRecebimentosPorAno] = useState<RecebimentosPorAno | null>(null);
    const [maioresGastos, setMaioresGastos] = useState<MaiorGasto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [showDetailedInfo, setShowDetailedInfo] = useState(false);

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

    {/* Full-screen loading overlay */ }
    <LoadingOverlay isLoading={loading} content="Carregando informações do fornecedor..." />

    if (error || !data) {
        return (
            <div className="min-h-screen bg-background">
                <Header />
                <main className="container mx-auto px-4 py-8">
                    <div className="flex items-center justify-center h-64">
                        {error && <p className="text-destructive">{error || "Fornecedor não encontrado"}</p>}
                        {!data && <p className="text-muted-foreground">Carregando dados do fornecedor...</p>}
                    </div>
                </main>
                <Footer />
            </div>
        );
    }

    const { fornecedor, quadro_societario } = data;
    const enderecoCompleto = `${fornecedor.logradouro}, ${fornecedor.numero}${fornecedor.complemento ? ' - ' + fornecedor.complemento : ''}, ${fornecedor.cep} - ${fornecedor.bairro}, ${fornecedor.cidade}, ${fornecedor.estado}`;

    return (
        <div className="min-h-screen bg-background">
            <Header />
            <main className="container mx-auto px-4 py-8">
                <div className="space-y-8">
                    {/* Modern Header */}
                    <div className="text-center space-y-4">
                        <div className="flex items-center justify-center gap-3 mb-4">
                            <Building2 className="h-8 w-8 text-primary" />
                            <h1 className="text-4xl font-bold bg-gradient-to-r from-primary to-primary/80 bg-clip-text text-transparent">
                                Perfil do Fornecedor
                            </h1>
                        </div>
                        <p className="text-muted-foreground max-w-3xl mx-auto leading-relaxed">
                            Informações detalhadas sobre o fornecedor, seus recebimentos e atividades comerciais.
                        </p>
                    </div>

                    {/* Profile Card with Modern Design */}
                    <Card className="shadow-md border-0 bg-white overflow-hidden">
                        <div className={`relative overflow-hidden ${fornecedor.situacao_cadastral === 'ATIVA'
                            ? "bg-gradient-to-r from-blue-600 to-blue-700 text-white"
                            : "bg-gradient-to-r from-gray-500 to-gray-600 text-white"
                            }`}>
                            <div className="p-6">
                                <div className="flex flex-col md:flex-row gap-6 items-center md:items-start">
                                    {/* Icon Section */}
                                    <div className="flex-shrink-0">
                                        <div className="relative">
                                            <div className="w-24 h-24 rounded-full bg-white/20 flex items-center justify-center backdrop-blur-sm border-4 border-white/30">
                                                <Building2 className="h-12 w-12 text-white" />
                                            </div>
                                        </div>
                                    </div>

                                    {/* Main Info Section */}
                                    <div className="flex-1 text-center md:text-left space-y-3">
                                        <div className="flex items-center gap-3 flex-wrap justify-center md:justify-start">
                                            <h2 className="text-2xl font-bold">
                                                {fornecedor.nome}
                                            </h2>
                                            {fornecedor.nome_fantasia && (
                                                <Badge variant="secondary" className="bg-white/20 text-white border-white/30">
                                                    {fornecedor.nome_fantasia}
                                                </Badge>
                                            )}
                                        </div>

                                        <div className="flex items-center gap-2 flex-wrap justify-center md:justify-start">
                                            <Badge
                                                variant="secondary"
                                                className="bg-white/20 text-white border-white/30"
                                            >
                                                {fornecedor.cnpj_cpf}
                                            </Badge>
                                            {fornecedor.tipo && <Badge
                                                variant="secondary"
                                                className="bg-white/20 text-white border-white/30"
                                                title={fornecedor.tipo}
                                            >
                                                {fornecedor.tipo}
                                            </Badge>}
                                            {fornecedor.situacao_cadastral && <Badge variant="secondary" className="bg-white/20 text-white border-white/30">
                                                {fornecedor.situacao_cadastral}
                                            </Badge>}
                                        </div>

                                        {fornecedor.obtido_em && <p className="text-white/90 text-sm">
                                            Informações atualizadas em {fornecedor.obtido_em}
                                        </p>}
                                    </div>

                                    {/* Total Recebimentos Display */}
                                    <div className="text-center md:text-right space-y-2">
                                        <div className="bg-white/20 rounded-lg p-4 backdrop-blur-sm">
                                            <p className="text-sm text-white/80">Total Recebido</p>
                                            <p className="text-3xl font-bold text-white">
                                                R$ {recebimentosPorAno?.series?.reduce((a, b) => a + b, 0).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) || "0"}
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Contact Info Bar */}
                        {fornecedor.categoria == "PJ" && <div className="border-t border-gray-200 bg-gray-50 px-6 py-4">
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 text-sm">
                                <div className="flex items-center gap-2">
                                    <Calendar className="h-4 w-4 text-primary" />
                                    <span className="font-medium">Abertura:</span>
                                    <span>{fornecedor.data_de_abertura}</span>
                                </div>
                                <div className="flex items-center gap-2 md:col-span-2">
                                    <MapPin className="h-4 w-4 text-primary" />
                                    <span className="font-medium">Endereço:</span>
                                    <span>{enderecoCompleto}</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <DollarSign className="h-4 w-4 text-primary" />
                                    <span className="font-medium">Capital Social:</span>
                                    <span>{formatCurrency(fornecedor.capital_social)}</span>
                                </div>
                                {fornecedor.telefone && <div className="flex items-center gap-2">
                                    <Phone className="h-4 w-4 text-primary" />
                                    <span className="font-medium">Telefone:</span>
                                    <span>{formatBrazilianPhone(fornecedor.telefone)}</span>
                                </div>}
                                {fornecedor.endereco_eletronico && <div className="flex items-center gap-2">
                                    <Mail className="h-4 w-4 text-primary" />
                                    <span className="font-medium">E-mail:</span>
                                    <span>{fornecedor.endereco_eletronico}</span>
                                </div>}
                            </div>
                        </div>}
                    </Card>

                    {/* Detailed Information */}
                    {fornecedor.categoria == "PJ" && <Card className="shadow-md border-0 bg-white">
                        <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                            <div className="flex items-center justify-between">
                                <div className="flex items-center gap-2">
                                    <Briefcase className="h-5 w-5 text-primary" />
                                    <CardTitle className="text-lg">Mais Informações</CardTitle>
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
                                <div className="space-y-8">
                                    {/* Economic Activities */}
                                    <div className="grid gap-8 lg:grid-cols-2">
                                        <Card className="shadow-sm border">
                                            <CardHeader className="bg-gray-50 border-b">
                                                <CardTitle className="text-base">Atividade Econômica Principal (CNAE)</CardTitle>
                                            </CardHeader>
                                            <CardContent className="p-4">
                                                <p className="text-sm">{fornecedor.atividade_principal}</p>
                                            </CardContent>
                                        </Card>

                                        <Card className="shadow-sm border">
                                            <CardHeader className="bg-gray-50 border-b">
                                                <CardTitle className="text-base">Natureza Jurídica</CardTitle>
                                            </CardHeader>
                                            <CardContent className="p-4">
                                                <p className="text-sm">{fornecedor.natureza_juridica}</p>
                                            </CardContent>
                                        </Card>

                                        <Card className="shadow-sm border">
                                            <CardHeader className="bg-gray-50 border-b">
                                                <CardTitle className="text-base">Atividades Econômicas Secundárias</CardTitle>
                                            </CardHeader>
                                            <CardContent className="p-4">
                                                <div className="space-y-1">
                                                    {fornecedor.atividade_secundaria.length > 0 ? (
                                                        fornecedor.atividade_secundaria.map((atividade, index) => (
                                                            <p key={index} className="text-sm">{atividade}</p>
                                                        ))
                                                    ) : (
                                                        <p className="text-sm text-muted-foreground">Nenhuma atividade secundária informada</p>
                                                    )}
                                                </div>
                                            </CardContent>
                                        </Card>

                                        <Card className="shadow-sm border">
                                            <CardHeader className="bg-gray-50 border-b">
                                                <CardTitle className="text-base">Informações Adicionais</CardTitle>
                                            </CardHeader>
                                            <CardContent className="p-4">
                                                <div className="space-y-2 text-sm">
                                                    <p><strong>Data da situação cadastral:</strong> {fornecedor.data_da_situacao_cadastral}</p>
                                                    {fornecedor.motivo_situacao_cadastral && (
                                                        <p><strong>Motivo Situação Cadastral:</strong> {fornecedor.motivo_situacao_cadastral}</p>
                                                    )}
                                                    {fornecedor.situacao_especial && (
                                                        <p><strong>Situação Especial:</strong> {fornecedor.situacao_especial}</p>
                                                    )}
                                                    {fornecedor.data_situacao_especial && (
                                                        <p><strong>Data Situação Especial:</strong> {fornecedor.data_situacao_especial}</p>
                                                    )}
                                                    {fornecedor.ente_federativo_responsavel && (
                                                        <p><strong>Ente Federativo Responsável:</strong> {fornecedor.ente_federativo_responsavel}</p>
                                                    )}
                                                </div>
                                            </CardContent>
                                        </Card>
                                    </div>

                                    {/* Quadro Societário */}
                                    <Card className="shadow-sm border">
                                        <CardHeader className="bg-gray-50 border-b">
                                            <CardTitle className="text-base">Quadro de Sócios e Administradores - QSA</CardTitle>
                                        </CardHeader>
                                        <CardContent className="p-4">
                                            {quadro_societario && quadro_societario.length > 0 ? (
                                                <div className="max-h-[400px] overflow-auto">
                                                    <Table>
                                                        <TableHeader>
                                                            <TableRow>
                                                                <TableHead>Nome/Nome Empresarial</TableHead>
                                                                <TableHead>Qualificação</TableHead>
                                                                <TableHead>Nome do Repres. Legal</TableHead>
                                                                <TableHead>Qualif. Rep. Legal</TableHead>
                                                            </TableRow>
                                                        </TableHeader>
                                                        <TableBody>
                                                            {quadro_societario.map((socio, index) => (
                                                                <TableRow key={index}>
                                                                    <TableCell className="font-medium">{socio.nome}</TableCell>
                                                                    <TableCell>{socio.qualificacao}</TableCell>
                                                                    <TableCell>{socio.nome_representante_legal || "-"}</TableCell>
                                                                    <TableCell>{socio.qualificacao_representante_legal || "-"}</TableCell>
                                                                </TableRow>
                                                            ))
                                                            }
                                                        </TableBody>
                                                    </Table>
                                                </div>
                                            ) : (
                                                <p className="text-center text-gray-500">A natureza jurídica não permite o preenchimento do QSA</p>
                                            )}
                                        </CardContent>
                                    </Card>
                                </div>
                            </CardContent>
                        )}
                    </Card>}

                    {/* Charts and Tables */}
                    <div className="grid gap-8 lg:grid-cols-2">
                        {/* Annual Receipts Chart */}
                        {recebimentosPorAno && (
                            <Card className="shadow-md border-0 bg-white">
                                <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                                    <div className="flex items-center gap-2">
                                        <TrendingUp className="h-5 w-5 text-primary" />
                                        <CardTitle className="text-lg">Recebimentos por Ano</CardTitle>
                                    </div>
                                </CardHeader>
                                <CardContent className="py-4">
                                    <AnnualSummaryChart
                                        data={recebimentosPorAno.categories.map((year, index) => ({
                                            year: year.toString(),
                                            value: Math.round(recebimentosPorAno.series[index] || 0)
                                        })).reverse()}
                                    />
                                </CardContent>
                            </Card>
                        )}

                        {/* Top Parliamentarians Table */}
                        {maioresGastos.length > 0 && (
                            <Card className="shadow-md border-0 bg-white">
                                <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center gap-2">
                                            <Users className="h-5 w-5 text-primary" />
                                            <CardTitle className="text-lg">Parlamentares (Maiores Gastos)</CardTitle>
                                        </div>
                                    </div>
                                </CardHeader>
                                <CardContent className="p-6">
                                    <div className="mb-4 text-sm">
                                        Ver lista completa por{" "}
                                        <a href="/deputado-federal/ceap?Fornecedor=186&Periodo=0&Agrupamento=6" className="text-primary hover:underline inline-flex items-center gap-1 ml-1">Deputado Federal <ExternalLink className="h-3 w-3" /></a> ou{" "}
                                        <a href="/senador?Fornecedor=186&Periodo=0&Agrupamento=6" className="text-primary hover:underline inline-flex items-center gap-1 ml-1">Senador <ExternalLink className="h-3 w-3" /></a>
                                    </div>
                                    <Table>
                                        <TableHeader>
                                            <TableRow>
                                                <TableHead>Parlamentar</TableHead>
                                                <TableHead>Tipo</TableHead>
                                                <TableHead>Último Recibo</TableHead>
                                                <TableHead className="text-right">Valor</TableHead>
                                            </TableRow>
                                        </TableHeader>
                                        <TableBody>
                                            {maioresGastos.map((row) => (
                                                <TableRow key={row.id} className="hover:bg-gray-50 transition-colors">
                                                    <TableCell className="font-medium">
                                                        <Link
                                                            to={row.link_parlamentar.replace("#/", "")}
                                                        >
                                                            {row.nome_parlamentar}
                                                            <div className="font-mono text-xs text-muted-foreground">
                                                                {row.sigla_partido} / {row.sigla_estado}
                                                            </div>
                                                        </Link>
                                                    </TableCell>
                                                    <TableCell>{row.tipo}</TableCell>
                                                    <TableCell>{row.ultima_emissao}</TableCell>
                                                    <TableCell className="text-right text-primary">
                                                        <Link
                                                            to={row.link_despesas.replace("#/", "").replace("/deputado-federal", "/deputado-federal/ceap").replace("/deputado-estadual", "/deputado-estadual/ceap")}
                                                            className="hover:text-primary transition-colors font-bold font-mono"
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
            </main>
            <Footer />
        </div>
    );
};

export default FornecedorDetalhe;
