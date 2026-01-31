import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { ErrorMessage } from "@/components/ErrorMessage";
import { useEffect } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useParams, Link } from "react-router-dom";
import { useQuery } from '@tanstack/react-query';
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Calendar, DollarSign, User, FileText, TrendingUp, Shield, Calculator, ArrowRight } from "lucide-react";
import { fetchRemuneracaoDetalhe } from "@/lib/api";

const formatValue = (value: string): string => {
    if (!value) return '-';
    return String('R$ ' + value);
};

const formatNameAcronym = (value: string, acronym: string): string => {
    if (!acronym) return value;
    return String(`${value} (${acronym})`);
}

const InfoCard = ({ icon: Icon, title, value, className = "" }: { icon: any, title: string, value: string, className?: string }) => (
    <div className={`flex items-start gap-3 p-4 bg-gradient-to-r from-background to-muted/20 border border-border/50 hover:border-border transition-all duration-200 ${className}`}>
        <div className="p-2 bg-primary/10">
            <Icon className="h-5 w-5 text-primary" />
        </div>
        <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-muted-foreground mb-1">{title}</p>
            <p className="text-sm font-semibold text-foreground truncate">{value}</p>
        </div>
    </div>
);

export default function FolhaPagamentoDetalhes() {
    const { id } = useParams<{ id: string }>();

    const { data: remuneracao, isLoading, error } = useQuery({
        queryKey: ['remuneracao-detalhe', id],
        queryFn: () => fetchRemuneracaoDetalhe(id!),
        staleTime: 0,
        enabled: !!id
    });

    usePageTitle("Remuneração no Senado - Detalhes");

    if (error) {
        return (
            <div className="min-h-screen bg-background">
                <Header />
                <ErrorMessage
                    onRetry={() => window.location.reload()}
                />
                <Footer />
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-background to-muted/20">
            <LoadingOverlay isLoading={isLoading} content="Carregando detalhes da remuneração..." />

            <Header />
            <main className="container mx-auto px-4 py-8">
                {/* Breadcrumb */}
                <div className="flex items-center gap-2 text-sm text-muted-foreground mb-8">
                    <Link to="/senador/folha-pagamento" className="hover:text-foreground transition-colors">
                        Folha de Pagamento
                    </Link>
                    <ArrowRight className="h-4 w-4" />
                    <span className="text-foreground">Detalhes da Remuneração</span>
                </div>

                <div className="space-y-6">
                    {remuneracao && (
                        <>
                            {/* Basic Information */}
                            <div className="bg-card border border-border shadow-sm overflow-hidden">
                                <div className="bg-primary/5 px-6 py-4 border-b border-primary/10">
                                    <h3 className="text-lg font-semibold text-foreground flex items-center gap-2">
                                        <User className="h-5 w-5 text-primary" />
                                        Informações Funcionais
                                    </h3>
                                </div>
                                <div className="p-6">
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                        {remuneracao.cargo && (
                                            <div>
                                                <span className="text-sm text-muted-foreground">Cargo</span>
                                                <p className="text-sm font-medium text-foreground mt-1">
                                                    {formatNameAcronym(remuneracao.cargo, remuneracao.referencia_cargo)}
                                                </p>
                                            </div>
                                        )}

                                        {remuneracao.vinculo && (
                                            <div>
                                                <span className="text-sm text-muted-foreground">Vínculo</span>
                                                <p className="text-sm font-medium text-foreground mt-1">{remuneracao.vinculo}</p>
                                            </div>
                                        )}

                                        {remuneracao.lotacao && (
                                            <div>
                                                <span className="text-sm text-muted-foreground">Lotação</span>
                                                <p className="text-sm font-medium text-foreground mt-1">{remuneracao.lotacao}</p>
                                            </div>
                                        )}

                                        {remuneracao.admissao && (
                                            <div>
                                                <span className="text-sm text-muted-foreground">Admissão</span>
                                                <p className="text-sm font-medium text-foreground mt-1">{remuneracao.admissao}</p>
                                            </div>
                                        )}

                                        {remuneracao.categoria && (
                                            <div>
                                                <span className="text-sm text-muted-foreground">Categoria</span>
                                                <p className="text-sm font-medium text-foreground mt-1">
                                                    {formatNameAcronym(remuneracao.categoria, remuneracao.simbolo_funcao)}
                                                </p>
                                            </div>
                                        )}

                                        <div>
                                            <span className="text-sm text-muted-foreground">Folha</span>
                                            <p className="text-sm font-medium text-foreground mt-1">
                                                {remuneracao.tipo_folha} ({remuneracao.ano_mes})
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            {/* Remuneration Details */}
                            <div className="bg-card border border-border shadow-sm overflow-hidden">
                                <div className="bg-primary/5 px-6 py-4 border-b border-primary/10">
                                    <div className="flex items-center justify-between">
                                        <h3 className="text-lg font-semibold text-foreground flex items-center gap-2">
                                            <Calculator className="h-5 w-5 text-primary" />
                                            Dados de Remuneração
                                        </h3>
                                        <Badge variant="outline" className="text-xs font-semibold border-primary/20 text-primary bg-primary/5">
                                            Folha {remuneracao.tipo_folha} ({remuneracao.ano_mes})
                                        </Badge>
                                    </div>
                                </div>
                                <div className="p-6">
                                    <div className="space-y-6">
                                        {/* Summary Cards */}
                                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                                            <Card className="bg-gradient-to-br from-primary/5 to-primary/10 border-primary/20">
                                                <CardContent className="p-6">
                                                    <div className="flex items-center justify-between">
                                                        <div>
                                                            <p className="text-sm font-medium text-primary">Remuneração Básica</p>
                                                            <p className="text-lg font-bold text-primary">{formatValue(remuneracao.remun_basica)}</p>
                                                        </div>
                                                        <FileText className="h-8 w-8 text-primary/50" />
                                                    </div>
                                                </CardContent>
                                            </Card>

                                            <Card className="bg-gradient-to-br from-accent/5 to-accent/10 border-accent/20">
                                                <CardContent className="p-6">
                                                    <div className="flex items-center justify-between">
                                                        <div>
                                                            <p className="text-sm font-medium text-accent">Líquido + Vantagens</p>
                                                            <p className="text-lg font-bold text-accent">{formatValue(remuneracao.total_liquido)}</p>
                                                        </div>
                                                        <Calculator className="h-8 w-8 text-accent/50" />
                                                    </div>
                                                </CardContent>
                                            </Card>

                                            <Card className="bg-gradient-to-br from-primary to-accent border-0 shadow-lg">
                                                <CardContent className="p-6 text-white">
                                                    <div className="flex items-center justify-between">
                                                        <div>
                                                            <p className="text-sm font-medium text-white/80">Custo Total</p>
                                                            <p className="text-lg font-bold text-white">{formatValue(remuneracao.custo_total)}</p>
                                                        </div>
                                                        <TrendingUp className="h-8 w-8 text-white/50" />
                                                    </div>
                                                </CardContent>
                                            </Card>
                                        </div>

                                        {/* Detailed Table */}
                                        <div className="overflow-x-auto">
                                            <div className="border border-border overflow-hidden rounded-lg shadow-sm">
                                                <table className="w-full">
                                                    <thead>
                                                        <tr className="bg-muted/50 border-b border-border">
                                                            <th className="text-left py-4 px-6 font-semibold text-foreground text-sm uppercase tracking-wider">
                                                                <div className="flex items-center gap-2">
                                                                    <FileText className="h-4 w-4 text-muted-foreground" />
                                                                    Componente
                                                                </div>
                                                            </th>
                                                            <th className="text-right py-4 px-6 font-semibold text-foreground text-sm uppercase tracking-wider">
                                                                <div className="flex items-center justify-end gap-2">
                                                                    <DollarSign className="h-4 w-4 text-muted-foreground" />
                                                                    Valor (R$)
                                                                </div>
                                                            </th>
                                                        </tr>
                                                    </thead>
                                                    <tbody className="divide-y divide-border">
                                                        <tr className="bg-primary/5">
                                                            <td className="py-4 px-6 font-medium text-foreground flex items-center gap-3">
                                                                <div className="p-2 bg-primary/10 rounded">
                                                                    <FileText className="h-4 w-4 text-primary" />
                                                                </div>
                                                                <span>Remuneração Básica</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 font-mono font-bold text-primary">{formatValue(remuneracao.remun_basica)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-muted/30 transition-colors">
                                                            <td className="py-4 px-6 text-foreground flex items-center gap-3">
                                                                <div className="p-2 bg-secondary/10 rounded">
                                                                    <Shield className="h-4 w-4 text-secondary" />
                                                                </div>
                                                                <span>Vantagens Pessoais</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 font-mono font-semibold text-secondary">{formatValue(remuneracao.vant_pessoais)}</td>
                                                        </tr>
                                                        <tr className="bg-muted/20">
                                                            <td className="py-4 px-6 font-semibold text-muted-foreground flex items-center gap-3">
                                                                <div className="p-2 bg-muted rounded">
                                                                    <Calculator className="h-4 w-4 text-muted-foreground" />
                                                                </div>
                                                                <span>Vantagens Eventuais</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 text-muted-foreground font-mono">-</td>
                                                        </tr>
                                                        <tr className="hover:bg-primary/5 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-muted-foreground flex items-center gap-3">
                                                                <div className="w-1.5 h-1.5 bg-primary/40 rounded-full ml-4"></div>
                                                                <span className="text-sm">Função Comissionada</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-xs font-medium">{formatValue(remuneracao.func_comissionada)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-primary/5 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-muted-foreground flex items-center gap-3">
                                                                <div className="w-1.5 h-1.5 bg-primary/40 rounded-full ml-4"></div>
                                                                <span className="text-sm">Antecipação e Gratificação Natalina</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-xs font-medium">{formatValue(remuneracao.grat_natalina)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-primary/5 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-muted-foreground flex items-center gap-3">
                                                                <div className="w-1.5 h-1.5 bg-primary/40 rounded-full ml-4"></div>
                                                                <span className="text-sm">Horas Extras</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-xs font-medium">{formatValue(remuneracao.horas_extras)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-primary/5 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-muted-foreground flex items-center gap-3">
                                                                <div className="w-1.5 h-1.5 bg-primary/40 rounded-full ml-4"></div>
                                                                <span className="text-sm">Outras Eventuais/Provisórias</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-xs font-medium">{formatValue(remuneracao.outras_eventuais)}</td>
                                                        </tr>
                                                        {parseFloat(remuneracao.abono_permanencia) > 0 && (
                                                            <tr className="hover:bg-primary/5 transition-colors">
                                                                <td className="py-4 px-6 font-medium text-foreground flex items-center gap-3">
                                                                    <div className="p-2 bg-accent/10 rounded">
                                                                        <Calendar className="h-4 w-4 text-accent" />
                                                                    </div>
                                                                    <span>Abono de Permanência</span>
                                                                </td>
                                                                <td className="text-right py-4 px-6 font-mono font-semibold text-accent">{formatValue(remuneracao.abono_permanencia)}</td>
                                                            </tr>
                                                        )}
                                                        <tr className="bg-destructive/5">
                                                            <td className="py-4 px-6 font-semibold text-destructive flex items-center gap-3">
                                                                <div className="p-2 bg-destructive/10 rounded">
                                                                    <TrendingUp className="h-4 w-4 text-destructive rotate-180" />
                                                                </div>
                                                                <span>Descontos Obrigatórios</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 font-mono text-destructive">-</td>
                                                        </tr>
                                                        <tr className="hover:bg-destructive/5 transition-colors">
                                                            <td className="py-3 px-6 text-muted-foreground flex items-center gap-3">
                                                                <div className="w-1.5 h-1.5 bg-destructive/40 rounded-full ml-4"></div>
                                                                <span className="text-sm">Imposto de Renda</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-xs text-destructive font-medium">-{formatValue(remuneracao.imposto_renda)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-destructive/5 transition-colors">
                                                            <td className="py-3 px-6 text-muted-foreground flex items-center gap-3">
                                                                <div className="w-1.5 h-1.5 bg-destructive/40 rounded-full ml-4"></div>
                                                                <span className="text-sm">Previdência (PSSS)</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-xs text-destructive font-medium">-{formatValue(remuneracao.previdencia)}</td>
                                                        </tr>
                                                        {parseFloat(remuneracao.reversao_teto_const) > 0 && (
                                                            <tr className="hover:bg-destructive/5 transition-colors">
                                                                <td className="py-3 px-6 text-muted-foreground flex items-center gap-3">
                                                                    <div className="w-1.5 h-1.5 bg-destructive/40 rounded-full ml-4"></div>
                                                                    <span className="text-sm">Teto Constitucional</span>
                                                                </td>
                                                                <td className="text-right py-3 px-6 font-mono text-xs text-destructive font-medium">-{formatValue(remuneracao.reversao_teto_const)}</td>
                                                            </tr>
                                                        )}
                                                        <tr className="bg-accent/5">
                                                            <td className="py-4 px-6 font-bold text-accent flex items-center gap-3">
                                                                <div className="p-2 bg-accent/10 rounded">
                                                                    <Calculator className="h-4 w-4" />
                                                                </div>
                                                                <span>Remuneração Líquida (Pós Descontos)</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 font-mono font-bold text-accent">{formatValue(remuneracao.rem_liquida)}</td>
                                                        </tr>
                                                        <tr className="bg-muted/10">
                                                            <td className="py-4 px-6 font-semibold text-muted-foreground flex items-center gap-3">
                                                                <div className="p-2 bg-muted rounded">
                                                                    <DollarSign className="h-4 w-4" />
                                                                </div>
                                                                <span>Vantagens Indenizatórias</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 font-mono text-muted-foreground">-</td>
                                                        </tr>
                                                        <tr className="hover:bg-primary/5 transition-colors">
                                                            <td className="py-3 px-6 text-muted-foreground flex items-center gap-3">
                                                                <div className="w-1.5 h-1.5 bg-primary/40 rounded-full ml-4"></div>
                                                                <span className="text-sm">Auxílios e Diárias</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-xs font-medium text-foreground">{formatValue(remuneracao.auxilios)}</td>
                                                        </tr>
                                                        <tr className="bg-gradient-to-r from-primary to-accent text-white">
                                                            <td className="py-6 px-6 font-bold flex items-center gap-3">
                                                                <div className="p-2 bg-white/20 rounded">
                                                                    <TrendingUp className="h-6 w-6" />
                                                                </div>
                                                                <div className="flex flex-col">
                                                                    <span className="text-lg">Custo Total Efetivo</span>
                                                                    <span className="text-xs font-normal opacity-80">Valor total para os cofres públicos</span>
                                                                </div>
                                                            </td>
                                                            <td className="text-right py-6 px-6 font-mono font-black text-2xl">
                                                                {formatValue(remuneracao.custo_total)}
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </>
                    )}
                </div>
            </main>
            <Footer />
        </div>
    );
}