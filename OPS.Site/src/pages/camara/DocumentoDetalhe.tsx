import { useParams, Link } from "react-router-dom";
import { useState, useEffect } from "react";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription } from "@/components/ui/alert";
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
    type DocumentoDetalhe as DeputadoDespesaDocumentoDetalhe,
    type DocumentoDoDia,
    type DocumentoDaSubquota,
} from "@/lib/api";
import { formatCurrency } from "@/lib/utils";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { AlertTriangle, ExternalLink, FileText, Download } from "lucide-react";

const DeputadoDespesaDocumentoDetalhe = () => {
    const { id } = useParams();
    const [documento, setDocumento] = useState<DeputadoDespesaDocumentoDetalhe | null>(null);
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
                    fetchDocumentoDetalhe(id),
                    fetchDocumentosDoMesmoDia(id),
                    fetchDocumentosDaSubquotaMes(id)
                ]);

                // Process URLs like in Vue.js
                const urlCamara = 'http://www.camara.leg.br/cota-parlamentar/';
                const processedDoc = { ...documentoData };

                if (processedDoc.link === 2) { // NF-e
                    processedDoc.url_documento = `${urlCamara}nota-fiscal-eletronica?ideDocumentoFiscal=${processedDoc.id_documento}`;
                } else if (processedDoc.link === 1) {
                    processedDoc.url_documento = `${urlCamara}documentos/publ/${processedDoc.id_deputado}/${processedDoc.ano}/${processedDoc.id_documento}.pdf`;
                }

                processedDoc.url_demais_documentos_mes = `${urlCamara}sumarizado?nuDeputadoId=${processedDoc.id_deputado}&dataInicio=${processedDoc.competencia}&dataFim=${processedDoc.competencia}&despesa=${processedDoc.id_cf_despesa_tipo}&nomeHospede=&nomePassageiro=&nomeFornecedor=&cnpjFornecedor=&numDocumento=&sguf=`;
                processedDoc.url_detalhes_documento = `${urlCamara}documento?nuDeputadoId=${processedDoc.id_deputado}&numMes=${processedDoc.mes}&numAno=${processedDoc.ano}&despesa=${processedDoc.id_cf_despesa_tipo}&cnpjFornecedor=${processedDoc.cnpj_cpf}&idDocumento=${processedDoc.numero_documento}`;
                processedDoc.url_beneficiario = `/fornecedor/${processedDoc.id_fornecedor}`;
                processedDoc.url_documentos_Deputado_beneficiario = `/deputado-federal/ceap?IdParlamentar=${processedDoc.id_cf_deputado}&Fornecedor=${processedDoc.id_fornecedor}&Periodo=57&Agrupamento=6`;

                setDocumento(processedDoc);
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

    const showValorDespesa = documento.valor_restituicao !== '0,00' || documento.valor_glosa !== '0,00';
    const showGlosa = documento.valor_glosa !== '0,00';
    const showRestituicao = documento.valor_restituicao !== '0,00';
    const showPassageiro = documento.nome_passageiro;
    const showTrechoViagem = documento.trecho_viagem;

    return (
        <div className="min-h-screen flex flex-col">
            <Header />
            <main className="flex-1 container mx-auto px-4 py-8">
                <div className="space-y-6">
                    {/* Warning about dynamic URL */}
                    <Alert>
                        <AlertTriangle className="h-4 w-4" />
                        <AlertDescription>
                            Atenção: Essa URL é dinâmica e pode mudar, portanto não a utilize para compartilhamento.
                            Para compartilhamento utilize este{" "}
                            {documento.url_documentos_Deputado_beneficiario && (
                                <Link to={documento.url_documentos_Deputado_beneficiario} className="underline ml-1">
                                    link
                                </Link>
                            )}.
                        </AlertDescription>
                    </Alert>

                    <div className="grid gap-8 lg:grid-cols-2">
                        <div className="space-y-4">
                            {/* Document Header */}
                            <Card>
                                <CardHeader>
                                    <CardTitle className="flex items-center gap-2">
                                        <FileText className="h-5 w-5" />
                                        Recibo: {documento.numero_documento || documento.id_documento}
                                    </CardTitle>
                                </CardHeader>
                                <CardContent className="space-y-4">
                                    {/* Parliamentarian Details */}
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                        <div>
                                            <h3 className="font-semibold text-sm text-gray-600 mb-1">Nome Parlamentar</h3>
                                            <p className="text-lg">
                                                <Link to={`/deputado-federal/${documento.id_deputado}`} className="text-primary hover:underline inline-flex items-center gap-1">
                                                    {documento.nome_parlamentar} <ExternalLink className="h-3 w-3" />
                                                </Link>
                                            </p>
                                        </div>
                                        <div>
                                            <h3 className="font-semibold text-sm text-gray-600 mb-1">Partido/UF</h3>
                                            <p>{documento.sigla_partido}/{documento.sigla_estado}</p>
                                        </div>
                                    </div>

                                    {/* Beneficiary Details */}
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                        <div>
                                            <h3 className="font-semibold text-sm text-gray-600 mb-1">Beneficiário</h3>
                                            <p className="text-lg">
                                                <Link to={documento.url_beneficiario || `/fornecedor/${documento.id_fornecedor}`} className="text-primary hover:underline inline-flex items-center gap-1">
                                                    {documento.nome_fornecedor} <ExternalLink className="h-3 w-3" />
                                                </Link>
                                            </p>
                                        </div>
                                        <div>
                                            <h3 className="font-semibold text-sm text-gray-600 mb-1">CNPJ/CPF</h3>
                                            <p>
                                                <Link to={documento.url_beneficiario || `/fornecedor/${documento.id_fornecedor}`} className="text-primary hover:underline inline-flex items-center gap-1">
                                                    {documento.cnpj_cpf} <ExternalLink className="h-3 w-3" />
                                                </Link>
                                            </p>
                                        </div>
                                    </div>

                                    {/* Expense Details */}
                                    <div>
                                        <h3 className="font-semibold text-sm text-gray-600 mb-1">Despesa</h3>
                                        <p className="text-lg">{documento.descricao_despesa}</p>
                                    </div>

                                    {/* Document Details - First Row */}
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 pt-4">
                                        <div>
                                            <h3 className="font-semibold text-sm text-gray-600 mb-1">Data da Nota</h3>
                                            <p>{documento.data_emissao}</p>
                                        </div>
                                        <div>
                                            <h3 className="font-semibold text-sm text-gray-600 mb-1">Competência</h3>
                                            <p>{documento.competencia}</p>
                                        </div>
                                        <div>
                                            <h3 className="font-semibold text-sm text-gray-600 mb-1">Tipo do Documento</h3>
                                            <p>{documento.tipo_documento}</p>
                                        </div>
                                    </div>

                                    {/* Document Details - Second Row (Conditional Values) */}
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                                        {showValorDespesa && (
                                            <div>
                                                <h3 className="font-semibold text-sm text-gray-600 mb-1">Valor da Despesa</h3>
                                                <p className="text-lg">
                                                    {formatCurrency(parseFloat(documento.valor_documento.replace('.', '').replace(',', '.')))}
                                                </p>
                                            </div>
                                        )}
                                        {showGlosa && (
                                            <div>
                                                <h3 className="font-semibold text-sm text-gray-600 mb-1">Glosas</h3>
                                                <p className="text-lg">
                                                    {formatCurrency(parseFloat(documento.valor_glosa.replace('.', '').replace(',', '.')))}
                                                </p>
                                            </div>
                                        )}
                                        {showRestituicao && (
                                            <div>
                                                <h3 className="font-semibold text-sm text-gray-600 mb-1">Restituições</h3>
                                                <p className="text-lg">
                                                    {formatCurrency(parseFloat(documento.valor_restituicao.replace('.', '').replace(',', '.')))}
                                                </p>
                                            </div>
                                        )}
                                        <div>
                                            <h3 className="font-semibold text-sm text-gray-600 mb-1">Valor Reembolsado</h3>
                                            <p className="text-lg font-semibold text-green-600">
                                                {formatCurrency(parseFloat(documento.valor_liquido.replace('.', '').replace(',', '.')))}
                                            </p>
                                        </div>
                                    </div>

                                    {/* Travel Details (Conditional) */}
                                    {(showPassageiro || showTrechoViagem) && (
                                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-4">
                                            {showPassageiro && (
                                                <div>
                                                    <h3 className="font-semibold text-sm text-gray-600 mb-1">Nome do Passageiro</h3>
                                                    <p className="text-lg">{documento.nome_passageiro}</p>
                                                </div>
                                            )}
                                            {showTrechoViagem && (
                                                <div>
                                                    <h3 className="font-semibold text-sm text-gray-600 mb-1">Trecho da Viagem</h3>
                                                    <p className="text-lg">{documento.trecho_viagem}</p>
                                                </div>
                                            )}
                                        </div>
                                    )}

                                    {/* Action Buttons */}
                                    <div className="border-t pt-4">
                                        <div className="flex justify-center mb-4">
                                            <Button asChild className="w-full md:w-auto">
                                                <Link to={documento.url_documentos_Deputado_beneficiario || `/deputado-federal/${documento.id_deputado}`}>
                                                    Ver todas as notas do deputado para o beneficiário
                                                </Link>
                                            </Button>
                                        </div>

                                        <div className="flex justify-center gap-2 flex-wrap">
                                            {documento.id_documento && (
                                                <>
                                                    {documento.url_documento && (
                                                        <Button variant="destructive" asChild className="w-full md:w-auto">
                                                            <a href={documento.url_documento} target="_blank" rel="nofollow noopener noreferrer" className="flex items-center gap-2">
                                                                <Download className="h-4 w-4" />
                                                                Recibo
                                                            </a>
                                                        </Button>
                                                    )}
                                                    {documento.url_documento_nfe && (
                                                        <Button variant="destructive" asChild className="w-full md:w-auto">
                                                            <a href={documento.url_documento_nfe} target="_blank" rel="nofollow noopener noreferrer" className="flex items-center gap-2">
                                                                <Download className="h-4 w-4" />
                                                                Recibo (NF-e)
                                                            </a>
                                                        </Button>
                                                    )}
                                                    {documento.url_detalhes_documento && (
                                                        <Button variant="outline" asChild className="w-full md:w-auto">
                                                            <a href={documento.url_detalhes_documento} target="_blank" rel="nofollow noopener noreferrer" className="flex items-center gap-2">
                                                                <FileText className="h-4 w-4" />
                                                                Detalhes do recibo
                                                            </a>
                                                        </Button>
                                                    )}
                                                </>
                                            )}
                                            {documento.url_demais_documentos_mes && (
                                                <Button variant="outline" asChild className="w-full md:w-auto">
                                                    <a href={documento.url_demais_documentos_mes} target="_blank" rel="nofollow noopener noreferrer" className="flex items-center gap-2">
                                                        <FileText className="h-4 w-4" />
                                                        Demais Recibos do mês
                                                    </a>
                                                </Button>
                                            )}
                                            <Button variant="outline" asChild className="w-full md:w-auto">
                                                <a href="https://www.nfe.fazenda.gov.br/portal/consultaRecaptcha.aspx?tipoConsulta=resumo&tipoConteudo=d09fwabTnLk=" target="_blank" rel="nofollow noopener noreferrer" className="flex items-center gap-2">
                                                    <ExternalLink className="h-4 w-4" />
                                                    Visualizar NFe
                                                </a>
                                            </Button>
                                        </div>
                                    </div>
                                </CardContent>
                            </Card>

                            {/* Documents from the same day */}
                            <Card>
                                <CardHeader>
                                    <CardTitle>Notas/recibos do dia</CardTitle>
                                </CardHeader>
                                <CardContent>
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
                                                    <TableRow key={row.id_cf_despesa}>
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
                                                            <Link to={`/deputado-federal/documento/${row.id_cf_despesa}`} className="text-primary hover:underline font-bold font-mono">
                                                                R$&nbsp;{row.valor_liquido}
                                                            </Link>
                                                        </TableCell>
                                                    </TableRow>
                                                ))}
                                            </TableBody>
                                        </Table>
                                    ) : (
                                        <p className="text-gray-500 text-center py-4">Nenhum registro encontrado</p>
                                    )}
                                </CardContent>
                            </Card>
                        </div>

                        {/* Documents from the subquota in the same month */}
                        <Card>
                            <CardHeader>
                                <CardTitle>Notas/recibos da subcota no mês</CardTitle>
                            </CardHeader>
                            <CardContent>
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
                                                <TableRow key={row.id_cf_despesa}>
                                                    <TableCell className="font-medium">
                                                        <Link to={`/fornecedor/${row.id_fornecedor}`} className="hover:text-primary transition-colors flex flex-col">
                                                            {row.nome_fornecedor}
                                                            <span className="font-mono text-xs text-muted-foreground">
                                                                {row.cnpj_cpf}
                                                            </span>
                                                        </Link>
                                                    </TableCell>
                                                    <TableCell className="text-right">
                                                        <Link to={`/deputado-federal/documento/${row.id_cf_despesa}`} className="hover:text-primary transition-colors">
                                                            {formatCurrency(parseFloat(row.valor_liquido.replace('.', '').replace(',', '.')))}
                                                        </Link>
                                                    </TableCell>
                                                </TableRow>
                                            ))}
                                        </TableBody>
                                    </Table>
                                ) : (
                                    <p className="text-gray-500 text-center py-4">Nenhum registro encontrado</p>
                                )}
                            </CardContent>
                        </Card>
                    </div>
                </div>
            </main>
            <Footer />
        </div>
    );
};

export default DeputadoDespesaDocumentoDetalhe;
