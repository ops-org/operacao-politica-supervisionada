import { useParams, Link } from "react-router-dom";
import { useState, useEffect } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Separator } from "@/components/ui/separator";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import {
    fetchDocumentoDetalhe,
    fetchDocumentosDoMesmoDia,
    fetchDocumentosDaSubquotaMes,
    type DocumentoDetalhe as DocumentoDetalhe,
    type DocumentoDoDia,
    type DocumentoDaSubquota,
} from "@/lib/api";
import { formatCurrency } from "@/lib/utils";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { AlertTriangle, ExternalLink, FileText, Download, Calendar, User, Building2, Receipt, ArrowRight, Info } from "lucide-react";

interface DocumentoDetalheProps {
    type?: "deputado-federal" | "deputado-estadual" | "senador";
}

const DespesaDocumentoDetalhe: React.FC<DocumentoDetalheProps> = ({ type = "deputado-federal" }) => {
    const { id } = useParams();
    usePageTitle("Detalhes do Documento");
    const [documento, setDocumento] = useState<DocumentoDetalhe | null>(null);
    const [documentosDia, setDocumentosDia] = useState<DocumentoDoDia[]>([]);
    const [documentosSubquota, setDocumentosSubquota] = useState<DocumentoDaSubquota[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const loadData = async () => {
            if (!id) {
                setError("ID do documento não fornecido");
                setLoading(false);
                return;
            }

            try {
                const [documentoData, documentosDiaData, documentosSubquotaData] = await Promise.all([
                    fetchDocumentoDetalhe(id, type),
                    fetchDocumentosDoMesmoDia(id, type),
                    fetchDocumentosDaSubquotaMes(id, type)
                ]);

                documentoData.url_documentos_beneficiario =
                    `/${type}/ceap?IdParlamentar=${documentoData.id_parlamentar}&Fornecedor=${documentoData.id_fornecedor}&Periodo=57&Agrupamento=6`;

                setDocumento(documentoData);
                setDocumentosDia(documentosDiaData);
                setDocumentosSubquota(documentosSubquotaData);
            } catch (err) {
                setError(err instanceof Error ? err.message : "Erro ao carregar dados do documento");
            } finally {
                setLoading(false);
            }
        };

        loadData();
    }, [id]);

    if (loading) {
        return <LoadingOverlay isLoading={true} />;
    }

    if (error) {
        return (
            <div className="min-h-screen flex flex-col">
                <Header />
                <main className="flex-1 container mx-auto px-4 py-8">
                    <Alert variant="destructive">
                        <AlertTriangle className="h-4 w-4" />
                        <AlertDescription>{error}</AlertDescription>
                    </Alert>
                </main>
                <Footer />
            </div>
        );
    }

    if (!documento) {
        return (
            <div className="min-h-screen flex flex-col">
                <Header />
                <main className="flex-1 container mx-auto px-4 py-8">
                    <Alert>
                        <AlertTriangle className="h-4 w-4" />
                        <AlertDescription>Documento não encontrado</AlertDescription>
                    </Alert>
                </main>
                <Footer />
            </div>
        );
    }

    const showValorDespesa =
        ((documento.valor_restituicao && documento.valor_restituicao != "0,00") ||
            (documento.valor_glosa && documento.valor_glosa != "0,00")) &&
        (documento.valor_liquido != documento.valor_restituicao);

    const showGlosa = documento.valor_glosa && documento.valor_glosa != "0,00";
    const showRestituicao = documento.valor_restituicao && documento.valor_restituicao != "0,00";
    const showPassageiro = documento.nome_passageiro;
    const showTrechoViagem = documento.trecho_viagem;

    return (
        <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
            <Header />
            <main className="container mx-auto px-4 py-8">
                {/* Breadcrumb */}
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Link to={`/${type}/ceap`} className="hover:text-foreground transition-colors">
                        Cota Parlamentar
                    </Link>
                    <ArrowRight className="h-4 w-4" />
                    <span className="text-foreground">Detalhes do Documento</span>
                </div>

                {/* Warning Alert */}
                <Alert className="border-amber-200 bg-amber-50 dark:border-amber-800 dark:bg-amber-950 mt-5">
                    <Info className="h-4 w-4 text-amber-600 dark:text-amber-400" />
                    <AlertDescription className="text-amber-800 dark:text-amber-200">
                        <span className="font-medium">Atenção:</span> Essa URL é dinâmica e pode mudar, portanto não a utilize para compartilhamento.
                        Para compartilhamento utilize este{" "}
                        {documento.url_documentos_beneficiario && (
                            <Link to={documento.url_documentos_beneficiario} className="underline ml-1 text-amber-700 dark:text-amber-300 hover:text-amber-800 dark:hover:text-amber-100 font-medium">
                                link permanente
                            </Link>
                        )}.
                    </AlertDescription>
                </Alert>

                <div className="grid gap-8 mt-6">
                    {/* Main Document Details */}
                    <div className="lg:col-span-2 space-y-6">
                        {/* Document Header Card */}
                        <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden border-t-4 border-t-primary">
                            <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                <div className="flex items-center justify-between">
                                    <div className="flex items-center gap-4">
                                        <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                                            <Receipt className="h-6 w-6 text-primary" />
                                        </div>
                                        <div>
                                            <CardTitle className="text-2xl font-bold">Recibo #{documento.numero_documento || documento.id_documento}</CardTitle>
                                            <CardDescription className="font-medium text-muted-foreground/80">Detalhes do documento fiscal</CardDescription>
                                        </div>
                                    </div>
                                    <div className="text-right hidden md:block">
                                        <div className="text-xs font-bold uppercase tracking-wider text-muted-foreground mb-1">Valor Total</div>
                                        <div className="text-3xl font-black text-primary font-mono whitespace-nowrap">
                                            {formatCurrency(parseFloat(documento.valor_liquido.replace('.', '').replace(',', '.')))}
                                        </div>
                                    </div>
                                </div>
                            </CardHeader>
                            <CardContent className="p-6 space-y-4">
                                {/* Beneficiary Details */}
                                <div className="space-y-3">
                                    <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                                        <Building2 className="h-4 w-4" />
                                        Beneficiário
                                    </div>
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                        <div className="space-y-1">
                                            <label className="text-sm font-medium text-muted-foreground">Nome</label>
                                            <div>
                                                <Link to={`/fornecedor/${documento.id_fornecedor}`} className="text-primary hover:underline inline-flex items-center gap-2 font-medium">
                                                    {documento.nome_fornecedor} <ExternalLink className="h-3 w-3" />
                                                </Link>
                                            </div>
                                        </div>
                                        {documento.cnpj_cpf && <div className="space-y-1">
                                            <label className="text-sm font-medium text-muted-foreground">CNPJ/CPF</label>
                                            <div>
                                                <Link to={`/fornecedor/${documento.id_fornecedor}`} className="text-primary hover:underline inline-flex items-center gap-2 font-mono text-sm">
                                                    {documento.cnpj_cpf} <ExternalLink className="h-3 w-3" />
                                                </Link>
                                            </div>
                                        </div>}
                                    </div>
                                </div>

                                <Separator />

                                {/* Parliamentarian Details */}
                                <div className="space-y-3">
                                    <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                                        <User className="h-4 w-4" />
                                        Parlamentar
                                    </div>
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                        <div className="space-y-1">
                                            <label className="text-sm font-medium text-muted-foreground">Nome</label>
                                            <div>
                                                <Link to={`/${type}/${documento.id_parlamentar}`} className="text-primary hover:underline inline-flex items-center gap-2 font-medium">
                                                    {documento.nome_parlamentar} <ExternalLink className="h-3 w-3" />
                                                </Link>
                                            </div>
                                        </div>
                                        <div className="space-y-1">
                                            <label className="text-sm font-medium text-muted-foreground">Partido/UF</label>
                                            <div>
                                                <div className="flex items-center gap-2">
                                                    <Badge variant="secondary" className="font-medium text-[10px] px-2.5 py-0.5">
                                                        {documento.sigla_partido}
                                                    </Badge>
                                                    <Badge variant="outline" className="font-medium text-[10px] px-2.5 py-0.5">
                                                        {documento.sigla_estado}
                                                    </Badge>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <Separator />

                                {/* Document Information */}
                                <div className="space-y-3">
                                    <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                                        <FileText className="h-4 w-4" />
                                        Detalhes da Despesa
                                    </div>

                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
                                        <div className="space-y-1">
                                            <label className="text-sm font-medium text-muted-foreground">Tipo de Despesa</label>
                                            <div>
                                                <p className="font-medium">{documento.descricao_despesa}</p>
                                            </div>
                                        </div>
                                        {documento.descricao_despesa_especificacao && <div className="space-y-1">
                                            <label className="text-sm font-medium text-muted-foreground">Despesa Especificação</label>
                                            <div>
                                                <p className="font-medium">{documento.descricao_despesa_especificacao}</p>
                                            </div>
                                        </div>}
                                        {documento.favorecido && <div className="space-y-1">
                                            <label className="text-sm font-medium text-muted-foreground">Favorecido</label>
                                            <div>
                                                <p className="font-medium">{documento.favorecido}</p>
                                            </div>
                                        </div>}
                                        {documento.observacao && <div className="space-y-1">
                                            <label className="text-sm font-medium text-muted-foreground">Observação</label>
                                            <div>
                                                <p className="font-medium">{documento.observacao}</p>
                                            </div>
                                        </div>}
                                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
                                            <div className="space-y-1">
                                                <label className="text-sm font-medium text-muted-foreground">Data da Nota</label>
                                                <div>
                                                    <p className="font-medium">{documento.data_emissao}</p>
                                                </div>
                                            </div>
                                            <div className="space-y-1">
                                                <label className="text-sm font-medium text-muted-foreground">Competência</label>
                                                <div>
                                                    <p className="font-medium">{documento.competencia}</p>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <Separator />

                                {/* Financial Information */}
                                <div className="space-y-3">
                                    <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                                        <Receipt className="h-4 w-4" />
                                        Valores
                                    </div>
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
                                        {showValorDespesa && (
                                            <div className="space-y-1">
                                                <label className="text-sm font-medium text-muted-foreground">Valor da Despesa</label>
                                                <div>
                                                    <p className="text-lg font-semibold">
                                                        {formatCurrency(parseFloat(documento.valor_documento.replace('.', '').replace(',', '.')))}
                                                    </p>
                                                </div>
                                            </div>
                                        )}
                                        {showGlosa && (
                                            <div className="space-y-1">
                                                <label className="text-sm font-medium text-muted-foreground">Glosas</label>
                                                <div className="p-3 bg-red-50 dark:bg-red-950 border border-red-200 dark:border-red-800 rounded-lg">
                                                    <p className="text-lg font-semibold text-red-600 dark:text-red-400">
                                                        {formatCurrency(parseFloat(documento.valor_glosa.replace('.', '').replace(',', '.')))}
                                                    </p>
                                                </div>
                                            </div>
                                        )}
                                        {showRestituicao && (
                                            <div className="space-y-1">
                                                <label className="text-sm font-medium text-muted-foreground">Restituições</label>
                                                <div className="p-3 bg-amber-50 dark:bg-amber-950 border border-amber-200 dark:border-amber-800 rounded-lg">
                                                    <p className="text-lg font-semibold text-amber-600 dark:text-amber-400">
                                                        {formatCurrency(parseFloat(documento.valor_restituicao.replace('.', '').replace(',', '.')))}
                                                    </p>
                                                </div>
                                            </div>
                                        )}
                                        <div className="space-y-1">
                                            <label className="text-sm font-medium text-muted-foreground">Valor Reembolsado</label>
                                            <div className="p-3 bg-green-50 dark:bg-green-950 border border-green-200 dark:border-green-800 rounded-lg">
                                                <p className="text-lg font-bold text-green-600 dark:text-green-400">
                                                    {formatCurrency(parseFloat(documento.valor_liquido.replace('.', '').replace(',', '.')))}
                                                </p>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                {/* Travel Details (Conditional) */}
                                {(showPassageiro || showTrechoViagem) && (
                                    <>
                                        <Separator />
                                        <div className="space-y-3">
                                            <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                                                <User className="h-4 w-4" />
                                                Informações de Viagem
                                            </div>
                                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                                {showPassageiro && (
                                                    <div className="space-y-1">
                                                        <label className="text-sm font-medium text-muted-foreground">Nome do Passageiro</label>
                                                        <div>
                                                            <p className="font-medium">{documento.nome_passageiro}</p>
                                                        </div>
                                                    </div>
                                                )}
                                                {showTrechoViagem && (
                                                    <div className="space-y-1">
                                                        <label className="text-sm font-medium text-muted-foreground">Trecho da Viagem</label>
                                                        <div>
                                                            <p className="font-medium">{documento.trecho_viagem}</p>
                                                        </div>
                                                    </div>
                                                )}
                                            </div>
                                        </div>
                                    </>
                                )}

                                <Separator />

                                {/* Action Buttons */}
                                <div className="space-y-3">
                                    <div className="flex flex-col sm:flex-row gap-3">
                                        <Button asChild className="p-2 sm:p-3 flex-1 text-xs sm:text-sm" size="sm">
                                            <Link to={documento.url_documentos_beneficiario || `/${type}/${documento.id_parlamentar}`} className="flex items-center gap-1 sm:gap-2">
                                                <FileText className="h-3 w-3 sm:h-4 sm:w-4" />
                                                <span className="hidden sm:inline">Ver todas as notas do parlamentar para o fornecedor</span>
                                                <span className="sm:hidden">Notas do parlamentar</span>
                                            </Link>
                                        </Button>
                                    </div>

                                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
                                        {documento.url_documento && (
                                            <Button variant="destructive" asChild className="w-full">
                                                <a href={documento.url_documento} target="_blank" rel="nofollow noopener noreferrer" className="flex items-center gap-2">
                                                    <Download className="h-4 w-4" />
                                                    Recibo
                                                </a>
                                            </Button>
                                        )}
                                        {documento.url_documento_nfe && (
                                            <Button variant="destructive" asChild className="w-full">
                                                <a href={documento.url_documento_nfe} target="_blank" rel="nofollow noopener noreferrer" className="flex items-center gap-2">
                                                    <Download className="h-4 w-4" />
                                                    Recibo (NF-e)
                                                </a>
                                            </Button>
                                        )}
                                        {documento.url_detalhes_documento && (
                                            <Button variant="outline" asChild className="w-full">
                                                <a href={documento.url_detalhes_documento} target="_blank" rel="nofollow noopener noreferrer" className="flex items-center gap-2">
                                                    <FileText className="h-4 w-4" />
                                                    Detalhes do recibo
                                                </a>
                                            </Button>
                                        )}
                                        {documento.url_demais_documentos_mes && (
                                            <Button variant="outline" asChild className="w-full">
                                                <a href={documento.url_demais_documentos_mes} target="_blank" rel="nofollow noopener noreferrer" className="flex items-center gap-2">
                                                    <FileText className="h-4 w-4" />
                                                    Demais Recibos do mês
                                                </a>
                                            </Button>
                                        )}
                                        {type === 'deputado-federal' && <Button variant="outline" asChild className="w-full">
                                            <a href="https://www.nfe.fazenda.gov.br/portal/consultaRecaptcha.aspx?tipoConsulta=resumo&tipoConteudo=d09fwabTnLk=" target="_blank" rel="nofollow noopener noreferrer" className="flex items-center gap-2 hidden md:flex">
                                                <ExternalLink className="h-4 w-4" />
                                                Visualizar NFe
                                            </a>
                                        </Button>}
                                    </div>
                                </div>
                            </CardContent>
                        </Card>

                        {/* Sidebar */}
                        <div className="space-y-6">
                            {/* Related Documents - Side by Side */}
                            <div className="grid gap-6 lg:grid-cols-2">
                                {/* Documents from the same day */}
                                <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden">
                                    <CardHeader className="bg-gradient-to-r from-purple-500/10 to-transparent border-b">
                                        <div className="flex items-center gap-4">
                                            <div className="p-3 bg-gradient-to-br from-purple-100 to-purple-50 rounded-xl shadow-inner border border-purple-200">
                                                <Calendar className="h-5 w-5 text-purple-600" />
                                            </div>
                                            <div>
                                                <CardTitle className="text-lg font-bold">Notas/Recibos do Dia</CardTitle>
                                                <CardDescription className="text-xs">Do parlamentar no mesmo dia</CardDescription>
                                            </div>
                                        </div>
                                    </CardHeader>
                                    <CardContent className="p-0">
                                        {documentosDia.length > 0 ? (
                                            <Table>
                                                <TableHeader className="bg-muted/30">
                                                    <TableRow className="hover:bg-transparent">
                                                        <TableHead className="py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b">Fornecedor</TableHead>
                                                        <TableHead className="py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b">UF</TableHead>
                                                        <TableHead className="text-right py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b">Valor</TableHead>
                                                    </TableRow>
                                                </TableHeader>
                                                <TableBody>
                                                    {documentosDia.map((row) => (
                                                        <TableRow key={row.id_despesa} className="hover:bg-muted/30 transition-colors border-b last:border-0">
                                                            <TableCell className="py-4 px-6 font-medium">
                                                                <Link to={`/fornecedor/${row.id_fornecedor}`} className="text-primary hover:text-primary/80 transition-colors flex flex-col">
                                                                    {row.nome_fornecedor}
                                                                    <span className="font-mono text-xs text-muted-foreground">
                                                                        {row.cnpj_cpf}
                                                                    </span>
                                                                </Link>
                                                            </TableCell>
                                                            <TableCell className="py-4 px-6">
                                                                {row.sigla_estado_fornecedor}
                                                            </TableCell>
                                                            <TableCell className="text-right py-4 px-6">
                                                                <Link to={`/${type}/ceap/${row.id_despesa}`} className="text-primary hover:underline font-bold font-mono">
                                                                    R$&nbsp;{row.valor_liquido}
                                                                </Link>
                                                            </TableCell>
                                                        </TableRow>
                                                    ))}
                                                </TableBody>
                                            </Table>
                                        ) : (
                                            <div className="text-center py-6 text-muted-foreground">
                                                <Calendar className="h-8 w-8 mx-auto mb-2 opacity-50" />
                                                <p className="text-sm">Nenhum registro encontrado</p>
                                            </div>
                                        )}
                                    </CardContent>
                                </Card>

                                {/* Documents from the subquota in the same month */}
                                <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden">
                                    <CardHeader className="bg-gradient-to-r from-primary/10 to-transparent border-b">
                                        <div className="flex items-center gap-4">
                                            <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/5 rounded-xl shadow-inner border border-primary/10">
                                                <Receipt className="h-5 w-5 text-primary" />
                                            </div>
                                            <div>
                                                <CardTitle className="text-lg font-bold">Notas/Recibos da Subcota</CardTitle>
                                                <CardDescription className="text-xs">Competência: {documento.competencia}</CardDescription>
                                            </div>
                                        </div>
                                    </CardHeader>
                                    <CardContent className="p-0">
                                        {documentosSubquota.length > 0 ? (
                                            <Table>
                                                <TableHeader className="bg-muted/30">
                                                    <TableRow className="hover:bg-transparent">
                                                        <TableHead className="py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b">Fornecedor</TableHead>
                                                        <TableHead className="text-right py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b">Valor</TableHead>
                                                    </TableRow>
                                                </TableHeader>
                                                <TableBody>
                                                    {documentosSubquota.map((row) => (
                                                        <TableRow key={row.id_despesa} className="hover:bg-muted/30 transition-colors border-b last:border-0">
                                                            <TableCell className="py-4 px-6 font-medium">
                                                                <Link to={`/fornecedor/${row.id_fornecedor}`} className="text-primary hover:text-primary/80 transition-colors flex flex-col">
                                                                    {row.nome_fornecedor}
                                                                    <span className="font-mono text-xs text-muted-foreground">
                                                                        {row.cnpj_cpf}
                                                                    </span>
                                                                </Link>
                                                            </TableCell>
                                                            <TableCell className="text-right py-4 px-6">
                                                                <Link to={`/${type}/ceap/${row.id_despesa}`} className="text-primary hover:text-primary/80 font-bold font-mono">
                                                                    {formatCurrency(parseFloat(row.valor_liquido.replace('.', '').replace(',', '.')))}
                                                                </Link>
                                                            </TableCell>
                                                        </TableRow>
                                                    ))}
                                                </TableBody>
                                            </Table>
                                        ) : (
                                            <div className="text-center py-6 text-muted-foreground">
                                                <Receipt className="h-8 w-8 mx-auto mb-2 opacity-50" />
                                                <p className="text-sm">Nenhum registro encontrado</p>
                                            </div>
                                        )}
                                    </CardContent>
                                </Card>
                            </div>
                        </div>
                    </div>
                </div>
            </main>
            <Footer />
        </div>
    );
};

export default DespesaDocumentoDetalhe;
