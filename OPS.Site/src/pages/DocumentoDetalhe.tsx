import { useParams, Link } from "react-router-dom";
import { useState, useEffect } from "react";
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

                // // Process URLs like in Vue.js
                // const urlCamara = 'http://www.camara.leg.br/cota-parlamentar/';
                // const processedDoc = { ...documentoData };

                // // Handle different field names based on type
                // const idDeputado = (processedDoc as any).id_deputado;
                // const idDespesaTipo = (processedDoc as any).id_despesa_tipo;
                // const idDespesa = (processedDoc as any).id_despesa;

                // if (processedDoc.link === 2) { // NF-e
                //     processedDoc.url_documento = `${urlCamara}nota-fiscal-eletronica?ideDocumentoFiscal=${processedDoc.id_documento}`;
                // } else if (processedDoc.link === 1) {
                //     processedDoc.url_documento = `${urlCamara}documentos/publ/${idDeputado}/${processedDoc.ano}/${processedDoc.id_documento}.pdf`;
                // }

                // processedDoc.url_demais_documentos_mes = `${urlCamara}sumarizado?nuDeputadoId=${idDeputado}&dataInicio=${processedDoc.competencia}&dataFim=${processedDoc.competencia}&despesa=${idDespesaTipo}&nomeHospede=&nomePassageiro=&nomeFornecedor=&cnpjFornecedor=&numDocumento=&sguf=`;
                // processedDoc.url_detalhes_documento = `${urlCamara}documento?nuDeputadoId=${idDeputado}&numMes=${processedDoc.mes}&numAno=${processedDoc.ano}&despesa=${idDespesaTipo}&cnpjFornecedor=${processedDoc.cnpj_cpf}&idDocumento=${processedDoc.numero_documento}`;
                // processedDoc.url_beneficiario = `/fornecedor/${processedDoc.id_fornecedor}`;
                // processedDoc.url_documentos_Deputado_beneficiario = `/${type}/ceap?IdParlamentar=${idDeputado}&Fornecedor=${processedDoc.id_fornecedor}&Periodo=57&Agrupamento=6`;

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

    const showValorDespesa = documento.valor_restituicao || documento.valor_glosa;
    const showGlosa = documento.valor_glosa;
    const showRestituicao = documento.valor_restituicao;
    const showPassageiro = documento.nome_passageiro;
    const showTrechoViagem = documento.trecho_viagem;

    return (
        <div className="min-h-screen bg-gradient-to-br from-background to-muted/20">
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
                        <Card className="border-0 shadow-lg">
                            <CardHeader className="bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-blue-950 dark:to-indigo-950 border-b">
                                <div className="flex items-center justify-between">
                                    <CardTitle className="flex items-center gap-3 text-xl">
                                        <div className="p-2 bg-blue-100 dark:bg-blue-900 rounded-lg">
                                            <Receipt className="h-5 w-5 text-blue-600 dark:text-blue-300" />
                                        </div>
                                        <div>
                                            <div className="font-semibold">Recibo #{documento.numero_documento || documento.id_documento}</div>
                                            <div className="text-sm font-normal text-muted-foreground">Detalhes completos do documento</div>
                                        </div>
                                    </CardTitle>
                                    <div className="text-right">
                                        <div className="text-2xl font-bold text-green-600 dark:text-green-400">
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
                                                <Link to={documento.url_beneficiario || `/fornecedor/${documento.id_fornecedor}`} className="text-primary hover:underline inline-flex items-center gap-2 font-medium">
                                                    {documento.nome_fornecedor} <ExternalLink className="h-3 w-3" />
                                                </Link>
                                            </div>
                                        </div>
                                        {documento.cnpj_cpf && <div className="space-y-1">
                                            <label className="text-sm font-medium text-muted-foreground">CNPJ/CPF</label>
                                            <div>
                                                <Link to={documento.url_beneficiario || `/fornecedor/${documento.id_fornecedor}`} className="text-primary hover:underline inline-flex items-center gap-2 font-mono text-sm">
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
                                                    <Badge variant="secondary" className="font-medium">
                                                        {documento.sigla_partido}
                                                    </Badge>
                                                    <Badge variant="outline" className="font-medium">
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
                                        <Button asChild className="flex-1" size="lg">
                                            <Link to={documento.url_documentos_beneficiario || `/${type}/${documento.id_parlamentar}`} className="flex items-center gap-2">
                                                <FileText className="h-4 w-4" />
                                                Ver todas as notas do parlamentar para o fornecedor
                                            </Link>
                                        </Button>
                                    </div>

                                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
                                        {documento.id_documento && (
                                            <>
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
                                            </>
                                        )}
                                        {documento.url_demais_documentos_mes && (
                                            <Button variant="outline" asChild className="w-full">
                                                <a href={documento.url_demais_documentos_mes} target="_blank" rel="nofollow noopener noreferrer" className="flex items-center gap-2">
                                                    <FileText className="h-4 w-4" />
                                                    Demais Recibos do mês
                                                </a>
                                            </Button>
                                        )}
                                        <Button variant="outline" asChild className="w-full">
                                            <a href="https://www.nfe.fazenda.gov.br/portal/consultaRecaptcha.aspx?tipoConsulta=resumo&tipoConteudo=d09fwabTnLk=" target="_blank" rel="nofollow noopener noreferrer" className="flex items-center gap-2">
                                                <ExternalLink className="h-4 w-4" />
                                                Visualizar NFe
                                            </a>
                                        </Button>
                                    </div>
                                </div>
                            </CardContent>
                        </Card>

                        {/* Sidebar */}
                        <div className="space-y-6">
                            {/* Related Documents - Side by Side */}
                            <div className="grid gap-6 lg:grid-cols-2">
                                {/* Documents from the same day */}
                                <Card className="border-0 shadow-lg">
                                    <CardHeader className="bg-gradient-to-r from-purple-50 to-pink-50 dark:from-purple-950 dark:to-pink-950 border-b">
                                        <CardTitle className="flex items-center gap-2 text-lg">
                                            <Calendar className="h-5 w-5 text-purple-600 dark:text-purple-300" />
                                            Notas/Recibos do Dia
                                        </CardTitle>
                                        <CardDescription className="text-sm">
                                            Do parlamentar no mesmo dia do recibo principal
                                        </CardDescription>
                                    </CardHeader>
                                    <CardContent className="p-4">
                                        {documentosDia.length > 0 ? (
                                            <Table>
                                                <TableHeader>
                                                    <TableRow>
                                                        <TableHead>Fornecedor</TableHead>
                                                        <TableHead>UF</TableHead>
                                                        <TableHead className="text-right">Valor</TableHead>
                                                    </TableRow>
                                                </TableHeader>
                                                <TableBody>
                                                    {documentosDia.map((row) => (
                                                        <TableRow key={row.id_despesa}>
                                                            <TableCell className="font-medium">
                                                                <Link to={`/fornecedor/${row.id_fornecedor}`} className="hover:text-primary transition-colors flex flex-col">
                                                                    {row.nome_fornecedor}
                                                                    <span className="font-mono text-xs text-muted-foreground">
                                                                        {row.cnpj_cpf}
                                                                    </span>
                                                                </Link>
                                                            </TableCell>
                                                            <TableCell>
                                                                {row.sigla_estado_fornecedor}
                                                            </TableCell>
                                                            <TableCell className="text-right">
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
                                <Card className="border-0 shadow-lg">
                                    <CardHeader className="bg-gradient-to-r from-green-50 to-emerald-50 dark:from-green-950 dark:to-emerald-950 border-b">
                                        <CardTitle className="flex items-center gap-2 text-lg">
                                            <Receipt className="h-5 w-5 text-green-600 dark:text-green-300" />
                                            Notas/Recibos da Subcota
                                        </CardTitle>
                                        <CardDescription className="text-sm">
                                            Do mesmo período de competência
                                        </CardDescription>
                                    </CardHeader>
                                    <CardContent className="p-4">
                                        {documentosSubquota.length > 0 ? (
                                            <Table>
                                                <TableHeader>
                                                    <TableRow>
                                                        <TableHead>Fornecedor</TableHead>
                                                        <TableHead className="text-right">Valor</TableHead>
                                                    </TableRow>
                                                </TableHeader>
                                                <TableBody>
                                                    {documentosSubquota.map((row) => (
                                                        <TableRow key={row.id_despesa}>
                                                            <TableCell className="font-medium">
                                                                <Link to={`/fornecedor/${row.id_fornecedor}`} className="hover:text-primary transition-colors flex flex-col">
                                                                    {row.nome_fornecedor}
                                                                    <span className="font-mono text-xs text-muted-foreground">
                                                                        {row.cnpj_cpf}
                                                                    </span>
                                                                </Link>
                                                            </TableCell>
                                                            <TableCell className="text-right">
                                                                <Link to={`/${type}/ceap/${row.id_despesa}`} className="hover:text-primary transition-colors">
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
