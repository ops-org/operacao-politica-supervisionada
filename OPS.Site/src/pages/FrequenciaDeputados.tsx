import { useState, useEffect, useCallback } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { ChevronLeft, ChevronRight, ChevronDown, ChevronUp, Calendar, Users, UserCheck, UserX, Clock, Loader2 } from "lucide-react";
import { Link } from "react-router-dom";
import {
    fetchFrequenciaSessoes,
    fetchFrequenciaSessaoDetalhe,
    FrequenciaSessao,
    FrequenciaDetalhe,
} from "@/lib/api";

const ITEMS_PER_PAGE = 25;

const FrequenciaDeputados = () => {
    usePageTitle("Frequência dos Deputados");

    const [sessoes, setSessoes] = useState<FrequenciaSessao[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [page, setPage] = useState(1);
    const [totalRecords, setTotalRecords] = useState(0);
    const [sortField, setSortField] = useState<number | null>(null);
    const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');

    
    const [expandedSessionId, setExpandedSessionId] = useState<number | null>(null);
    const [sessionDetails, setSessionDetails] = useState<FrequenciaDetalhe[]>([]);
    const [loadingDetails, setLoadingDetails] = useState(false);

    const loadSessoes = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);
            const result = await fetchFrequenciaSessoes(page, ITEMS_PER_PAGE, sortField, sortOrder);
            setSessoes(result.data || []);
            setTotalRecords(result.recordsTotal || 0);
        } catch (err) {
            console.error(err);
            setError("Erro ao carregar dados de frequência.");
        } finally {
            setLoading(false);
        }
    }, [page, sortField, sortOrder]);

    useEffect(() => {
        loadSessoes();
    }, [loadSessoes]);

    const handleSort = (field: number) => {
        if (sortField === field) {
            setSortOrder(prev => prev === 'asc' ? 'desc' : 'asc');
        } else {
            setSortField(field);
            setSortOrder('desc');
        }
        setPage(1);
    };

    const handleExpandSession = async (sessionId: number) => {
        if (expandedSessionId === sessionId) {
            setExpandedSessionId(null);
            setSessionDetails([]);
            return;
        }

        setExpandedSessionId(sessionId);
        setLoadingDetails(true);
        try {
            const result = await fetchFrequenciaSessaoDetalhe(sessionId);
            setSessionDetails(result.data || []);
        } catch (err) {
            console.error(err);
            setSessionDetails([]);
        } finally {
            setLoadingDetails(false);
        }
    };

    const totalPages = Math.ceil(totalRecords / ITEMS_PER_PAGE);

    const getPresencaColor = (presenca: string) => {
        switch (presenca) {
            case "Presente":
            case "Presença externa":
                return "text-emerald-600 dark:text-emerald-400";
            case "Ausencia justificada":
                return "text-amber-600 dark:text-amber-400";
            case "Ausente":
                return "text-red-600 dark:text-red-400";
            default:
                return "";
        }
    };

    const getPresencaBg = (presenca: string) => {
        switch (presenca) {
            case "Presente":
            case "Presença externa":
                return "bg-emerald-100 dark:bg-emerald-900/30";
            case "Ausencia justificada":
                return "bg-amber-100 dark:bg-amber-900/30";
            case "Ausente":
                return "bg-red-100 dark:bg-red-900/30";
            default:
                return "";
        }
    };

    return (
        <div className="min-h-screen flex flex-col bg-gradient-to-br from-background to-muted/30">
            <Header />
            <main className="flex-1 container mx-auto px-4 py-8 max-w-7xl">
                {}
                <div className="mb-8">
                    <div className="flex items-center gap-3 mb-2">
                        <div className="p-2 rounded-lg bg-gradient-to-br from-primary/20 to-accent/20">
                            <Calendar className="h-6 w-6 text-primary" />
                        </div>
                        <h1 className="text-3xl font-bold bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">
                            Frequência em Plenário
                        </h1>
                    </div>
                    <p className="text-muted-foreground ml-14">
                        Sessões plenárias da Câmara dos Deputados e o registro de presença dos parlamentares.
                    </p>
                </div>

                {}
                {!loading && sessoes.length > 0 && (
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
                        <Card className="bg-gradient-to-br from-emerald-500/10 to-emerald-600/5 border-emerald-200/50 dark:border-emerald-800/50">
                            <CardContent className="p-4 flex items-center gap-3">
                                <UserCheck className="h-8 w-8 text-emerald-600 dark:text-emerald-400" />
                                <div>
                                    <p className="text-sm text-muted-foreground">Total de Sessões</p>
                                    <p className="text-2xl font-bold text-emerald-700 dark:text-emerald-300">{totalRecords.toLocaleString('pt-BR')}</p>
                                </div>
                            </CardContent>
                        </Card>
                        <Card className="bg-gradient-to-br from-blue-500/10 to-blue-600/5 border-blue-200/50 dark:border-blue-800/50">
                            <CardContent className="p-4 flex items-center gap-3">
                                <Users className="h-8 w-8 text-blue-600 dark:text-blue-400" />
                                <div>
                                    <p className="text-sm text-muted-foreground">Sessões na Página</p>
                                    <p className="text-2xl font-bold text-blue-700 dark:text-blue-300">{sessoes.length}</p>
                                </div>
                            </CardContent>
                        </Card>
                        <Card className="bg-gradient-to-br from-purple-500/10 to-purple-600/5 border-purple-200/50 dark:border-purple-800/50">
                            <CardContent className="p-4 flex items-center gap-3">
                                <Clock className="h-8 w-8 text-purple-600 dark:text-purple-400" />
                                <div>
                                    <p className="text-sm text-muted-foreground">Página</p>
                                    <p className="text-2xl font-bold text-purple-700 dark:text-purple-300">{page} de {totalPages || 1}</p>
                                </div>
                            </CardContent>
                        </Card>
                    </div>
                )}

                {}
                <Card className="shadow-lg border-border/50">
                    <CardHeader>
                        <CardTitle className="flex items-center gap-2">
                            <Calendar className="h-5 w-5 text-primary" />
                            Sessões Plenárias
                        </CardTitle>
                        <CardDescription>
                            Clique em uma sessão para ver a lista de presença dos deputados.
                        </CardDescription>
                    </CardHeader>
                    <CardContent>
                        {error && (
                            <div className="text-center py-8 text-red-500">
                                <p>{error}</p>
                                <Button variant="outline" className="mt-4" onClick={loadSessoes}>
                                    Tentar novamente
                                </Button>
                            </div>
                        )}

                        {loading ? (
                            <div className="flex items-center justify-center py-16">
                                <Loader2 className="h-8 w-8 animate-spin text-primary" />
                                <span className="ml-3 text-muted-foreground">Carregando sessões...</span>
                            </div>
                        ) : !error && sessoes.length === 0 ? (
                            <div className="text-center py-16 text-muted-foreground">
                                <UserX className="h-12 w-12 mx-auto mb-4 opacity-50" />
                                <p>Nenhuma sessão encontrada.</p>
                            </div>
                        ) : !error && (
                            <>
                                <div className="overflow-x-auto">
                                    <Table>
                                        <TableHeader>
                                            <TableRow>
                                                <TableHead className="w-[40px]"></TableHead>
                                                <TableHead
                                                    className="cursor-pointer hover:text-primary transition-colors"
                                                    onClick={() => handleSort(0)}
                                                >
                                                    Data/Hora {sortField === 0 && (sortOrder === 'asc' ? '↑' : '↓')}
                                                </TableHead>
                                                <TableHead>Tipo</TableHead>
                                                <TableHead>Nº</TableHead>
                                                <TableHead className="text-right text-emerald-600 dark:text-emerald-400">
                                                    Presenças
                                                </TableHead>
                                                <TableHead className="text-right text-red-600 dark:text-red-400">
                                                    Ausências
                                                </TableHead>
                                                <TableHead className="text-right text-amber-600 dark:text-amber-400">
                                                    Justificadas
                                                </TableHead>
                                                <TableHead className="text-right">Total</TableHead>
                                            </TableRow>
                                        </TableHeader>
                                        <TableBody>
                                            {sessoes.map((sessao) => (
                                                <>
                                                    <TableRow
                                                        key={sessao.id_cf_sessao}
                                                        className="cursor-pointer hover:bg-muted/50 transition-colors"
                                                        onClick={() => handleExpandSession(sessao.id_cf_sessao)}
                                                    >
                                                        <TableCell>
                                                            {expandedSessionId === sessao.id_cf_sessao ? (
                                                                <ChevronUp className="h-4 w-4 text-primary" />
                                                            ) : (
                                                                <ChevronDown className="h-4 w-4 text-muted-foreground" />
                                                            )}
                                                        </TableCell>
                                                        <TableCell className="font-medium">{sessao.inicio}</TableCell>
                                                        <TableCell>
                                                            <span className="px-2 py-1 rounded-full text-xs font-medium bg-primary/10 text-primary">
                                                                {sessao.tipo}
                                                            </span>
                                                        </TableCell>
                                                        <TableCell>{sessao.numero}</TableCell>
                                                        <TableCell className="text-right">
                                                            <span className="text-emerald-600 dark:text-emerald-400 font-medium">
                                                                {sessao.presenca}
                                                            </span>
                                                            {sessao.presenca_percentual && (
                                                                <span className="text-xs text-muted-foreground ml-1">
                                                                    ({sessao.presenca_percentual}%)
                                                                </span>
                                                            )}
                                                        </TableCell>
                                                        <TableCell className="text-right">
                                                            <span className="text-red-600 dark:text-red-400 font-medium">
                                                                {sessao.ausencia}
                                                            </span>
                                                            {sessao.ausencia_percentual && (
                                                                <span className="text-xs text-muted-foreground ml-1">
                                                                    ({sessao.ausencia_percentual}%)
                                                                </span>
                                                            )}
                                                        </TableCell>
                                                        <TableCell className="text-right">
                                                            <span className="text-amber-600 dark:text-amber-400 font-medium">
                                                                {sessao.ausencia_justificada}
                                                            </span>
                                                            {sessao.ausencia_justificada_percentual && (
                                                                <span className="text-xs text-muted-foreground ml-1">
                                                                    ({sessao.ausencia_justificada_percentual}%)
                                                                </span>
                                                            )}
                                                        </TableCell>
                                                        <TableCell className="text-right font-bold">{sessao.total}</TableCell>
                                                    </TableRow>

                                                    {}
                                                    {expandedSessionId === sessao.id_cf_sessao && (
                                                        <TableRow key={`detail-${sessao.id_cf_sessao}`}>
                                                            <TableCell colSpan={8} className="p-0">
                                                                <div className="bg-muted/30 border-t border-b border-border/50 p-4">
                                                                    {loadingDetails ? (
                                                                        <div className="flex items-center justify-center py-6">
                                                                            <Loader2 className="h-5 w-5 animate-spin text-primary" />
                                                                            <span className="ml-2 text-sm text-muted-foreground">Carregando presenças...</span>
                                                                        </div>
                                                                    ) : sessionDetails.length === 0 ? (
                                                                        <p className="text-center text-sm text-muted-foreground py-4">
                                                                            Nenhum registro de presença encontrado para esta sessão.
                                                                        </p>
                                                                    ) : (
                                                                        <div className="max-h-96 overflow-y-auto">
                                                                            <Table>
                                                                                <TableHeader>
                                                                                    <TableRow>
                                                                                        <TableHead>Deputado(a)</TableHead>
                                                                                        <TableHead>Situação</TableHead>
                                                                                        <TableHead>Justificativa</TableHead>
                                                                                    </TableRow>
                                                                                </TableHeader>
                                                                                <TableBody>
                                                                                    {sessionDetails.map((detalhe, idx) => (
                                                                                        <TableRow key={idx} className={getPresencaBg(detalhe.presenca)}>
                                                                                            <TableCell>
                                                                                                <Link
                                                                                                    to={`/deputado-federal/${detalhe.id_parlamentar}`}
                                                                                                    className="text-primary hover:underline font-medium"
                                                                                                >
                                                                                                    {detalhe.nome_parlamentar}
                                                                                                </Link>
                                                                                            </TableCell>
                                                                                            <TableCell>
                                                                                                <span className={`font-medium ${getPresencaColor(detalhe.presenca)}`}>
                                                                                                    {detalhe.presenca}
                                                                                                </span>
                                                                                            </TableCell>
                                                                                            <TableCell className="text-muted-foreground text-sm">
                                                                                                {detalhe.justificativa || "—"}
                                                                                            </TableCell>
                                                                                        </TableRow>
                                                                                    ))}
                                                                                </TableBody>
                                                                            </Table>
                                                                        </div>
                                                                    )}
                                                                </div>
                                                            </TableCell>
                                                        </TableRow>
                                                    )}
                                                </>
                                            ))}
                                        </TableBody>
                                    </Table>
                                </div>

                                {}
                                <div className="flex items-center justify-between mt-6 pt-4 border-t border-border/50">
                                    <p className="text-sm text-muted-foreground">
                                        Mostrando {((page - 1) * ITEMS_PER_PAGE) + 1} a {Math.min(page * ITEMS_PER_PAGE, totalRecords)} de {totalRecords.toLocaleString('pt-BR')} sessões
                                    </p>
                                    <div className="flex items-center gap-2">
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            disabled={page <= 1}
                                            onClick={() => setPage(prev => Math.max(1, prev - 1))}
                                        >
                                            <ChevronLeft className="h-4 w-4 mr-1" />
                                            Anterior
                                        </Button>
                                        <span className="text-sm font-medium px-3">
                                            {page} / {totalPages || 1}
                                        </span>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            disabled={page >= totalPages}
                                            onClick={() => setPage(prev => Math.min(totalPages, prev + 1))}
                                        >
                                            Próxima
                                            <ChevronRight className="h-4 w-4 ml-1" />
                                        </Button>
                                    </div>
                                </div>
                            </>
                        )}
                    </CardContent>
                </Card>
            </main>
            <Footer />
        </div>
    );
};

export default FrequenciaDeputados;
