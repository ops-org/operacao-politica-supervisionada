import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { ErrorMessage } from "@/components/ErrorMessage";
import { useEffect } from "react";
import { useParams } from "react-router-dom";
import { useQuery } from '@tanstack/react-query';
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Calendar, DollarSign, User, FileText, TrendingUp, Shield, Calculator } from "lucide-react";
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

    useEffect(() => {
        if (remuneracao) {
            document.title = 'OPS :: Remuneração no Senado - Detalhes';
        }
    }, [remuneracao]);

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
        <div className="min-h-screen bg-background">
            <LoadingOverlay isLoading={isLoading} content="Carregando detalhes da remuneração..." />

            <Header />
            <main className="container mx-auto px-4 py-8">
                <div className="space-y-6">
                    {/* Header */}
                    <div className="text-center space-y-4">
                        <div className="flex items-center justify-center gap-3 mb-4">
                            <User className="h-8 w-8 text-primary" />
                            <h1 className="text-4xl font-bold bg-gradient-to-r from-primary to-primary/80 bg-clip-text text-transparent">
                                Remuneração/Subsídio no Senado
                            </h1>
                        </div>
                        <p className="text-muted-foreground max-w-3xl mx-auto leading-relaxed">
                            Visualização completa dos dados de remuneração.
                        </p>
                    </div>

                    {remuneracao && (
                        <>
                            {/* Basic Information */}
                            <div className="bg-white border border-gray-200 shadow-sm overflow-hidden">
                                <div className="bg-gray-50 px-6 py-4 border-b border-gray-200">
                                    <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                                        <User className="h-5 w-5 text-blue-600" />
                                        Informações Funcionais
                                    </h3>
                                </div>
                                <div className="p-6">
                                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                        {remuneracao.cargo && (
                                            <div>
                                                <span className="text-sm text-gray-500">Cargo</span>
                                                <p className="text-sm font-medium text-gray-900 mt-1">
                                                    {formatNameAcronym(remuneracao.cargo, remuneracao.referencia_cargo)}
                                                </p>
                                            </div>
                                        )}

                                        {remuneracao.vinculo && (
                                            <div>
                                                <span className="text-sm text-gray-500">Vínculo</span>
                                                <p className="text-sm font-medium text-gray-900 mt-1">{remuneracao.vinculo}</p>
                                            </div>
                                        )}

                                        {remuneracao.lotacao && (
                                            <div>
                                                <span className="text-sm text-gray-500">Lotação</span>
                                                <p className="text-sm font-medium text-gray-900 mt-1">{remuneracao.lotacao}</p>
                                            </div>
                                        )}

                                        {remuneracao.admissao && (
                                            <div>
                                                <span className="text-sm text-gray-500">Admissão</span>
                                                <p className="text-sm font-medium text-gray-900 mt-1">{remuneracao.admissao}</p>
                                            </div>
                                        )}

                                        {remuneracao.categoria && (
                                            <div>
                                                <span className="text-sm text-gray-500">Categoria</span>
                                                <p className="text-sm font-medium text-gray-900 mt-1">
                                                    {formatNameAcronym(remuneracao.categoria, remuneracao.simbolo_funcao)}
                                                </p>
                                            </div>
                                        )}

                                        <div>
                                            <span className="text-sm text-gray-500">Folha</span>
                                            <p className="text-sm font-medium text-gray-900 mt-1">
                                                {remuneracao.tipo_folha} ({remuneracao.ano_mes})
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            {/* Remuneration Details */}
                            <div className="bg-white border border-gray-200 shadow-sm overflow-hidden">
                                <div className="bg-gray-50 px-6 py-4 border-b border-gray-200">
                                    <div className="flex items-center justify-between">
                                        <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                                            <DollarSign className="h-5 w-5 text-blue-600" />
                                            Dados de Remuneração
                                        </h3>
                                        <Badge variant="outline" className="text-xs font-semibold border-blue-200 text-blue-700 bg-blue-50">
                                            Folha {remuneracao.tipo_folha} ({remuneracao.ano_mes})
                                        </Badge>
                                    </div>
                                </div>
                                <div className="p-6">
                                    <div className="space-y-6">
                                        {/* Summary Cards */}
                                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                                            <Card className="bg-gradient-to-br from-blue-50 to-blue-100 border-blue-200">
                                                <CardContent className="p-6">
                                                    <div className="flex items-center justify-between">
                                                        <div>
                                                            <p className="text-sm font-medium text-blue-600">Remuneração Básica</p>
                                                            <p className="text-lg font-bold text-blue-900">{formatValue(remuneracao.remun_basica)}</p>
                                                        </div>
                                                        <FileText className="h-8 w-8 text-blue-500" />
                                                    </div>
                                                </CardContent>
                                            </Card>

                                            <Card className="bg-gradient-to-br from-orange-50 to-orange-100 border-orange-200">
                                                <CardContent className="p-6">
                                                    <div className="flex items-center justify-between">
                                                        <div>
                                                            <p className="text-sm font-medium text-orange-600">Salário Líquido + Vantagens</p>
                                                            <p className="text-lg font-bold text-orange-900">{formatValue(remuneracao.total_liquido)}</p>
                                                        </div>
                                                        <Calculator className="h-8 w-8 text-orange-500" />
                                                    </div>
                                                </CardContent>
                                            </Card>

                                            <Card className="bg-gradient-to-br from-green-50 to-green-100 border-green-200">
                                                <CardContent className="p-6">
                                                    <div className="flex items-center justify-between">
                                                        <div>
                                                            <p className="text-sm font-medium text-green-600">Custo Total</p>
                                                            <p className="text-lg font-bold text-green-900">{formatValue(remuneracao.custo_total)}</p>
                                                        </div>
                                                        <TrendingUp className="h-8 w-8 text-green-500" />
                                                    </div>
                                                </CardContent>
                                            </Card>
                                        </div>

                                        {/* Detailed Table */}
                                        <div className="overflow-x-auto">
                                            <div className="border border-gray-200 overflow-hidden shadow-sm">
                                                <table className="w-full">
                                                    <thead>
                                                        <tr className="bg-gradient-to-r from-gray-50 to-gray-100 border-b border-gray-200">
                                                            <th className="text-left py-4 px-6 font-semibold text-gray-900">
                                                                <div className="flex items-center gap-2">
                                                                    <FileText className="h-4 w-4 text-gray-600" />
                                                                    Componente
                                                                </div>
                                                            </th>
                                                            <th className="text-right py-4 px-6 font-semibold text-gray-900">
                                                                <div className="flex items-center justify-end gap-2">
                                                                    <DollarSign className="h-4 w-4 text-gray-600" />
                                                                    Valor (R$)
                                                                </div>
                                                            </th>
                                                        </tr>
                                                    </thead>
                                                    <tbody className="divide-y divide-gray-200">
                                                        <tr className="bg-gradient-to-br from-blue-50 to-blue-100 border-blue-200">
                                                            <td className="py-4 px-6 font-medium text-gray-900 flex items-center gap-3">
                                                                <div className="p-2 bg-blue-100">
                                                                    <FileText className="h-4 w-4 text-blue-600" />
                                                                </div>
                                                                <span>Remuneração Básica</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 font-mono font-semibold text-blue-900">{formatValue(remuneracao.remun_basica)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-purple-50/50 transition-colors">
                                                            <td className="py-4 px-6 font-medium text-gray-900 flex items-center gap-3">
                                                                <div className="p-2 bg-purple-100">
                                                                    <Shield className="h-4 w-4 text-purple-600" />
                                                                </div>
                                                                <span>Vantagens Pessoais</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 font-mono font-semibold text-purple-900">{formatValue(remuneracao.vant_pessoais)}</td>
                                                        </tr>
                                                        <tr className="bg-gray-50/50">
                                                            <td className="py-4 px-6 font-semibold text-gray-700 flex items-center gap-3">
                                                                <div className="p-2 bg-gray-200">
                                                                    <Calculator className="h-4 w-4 text-gray-600" />
                                                                </div>
                                                                <span>Vantagens Eventuais</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 text-gray-500">-</td>
                                                        </tr>
                                                        <tr className="hover:bg-blue-50/30 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-gray-600 flex items-center gap-3">
                                                                <div className="w-1 h-1 bg-blue-400"></div>
                                                                <span className="text-sm">Função Comissionada</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-sm text-gray-700">{formatValue(remuneracao.func_comissionada)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-blue-50/30 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-gray-600 flex items-center gap-3">
                                                                <div className="w-1 h-1 bg-blue-400"></div>
                                                                <span className="text-sm">Antecipação e Gratificação Natalina</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-sm text-gray-700">{formatValue(remuneracao.grat_natalina)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-blue-50/30 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-gray-600 flex items-center gap-3">
                                                                <div className="w-1 h-1 bg-blue-400"></div>
                                                                <span className="text-sm">Horas Extras</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-sm text-gray-700">{formatValue(remuneracao.horas_extras)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-blue-50/30 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-gray-600 flex items-center gap-3">
                                                                <div className="w-1 h-1 bg-blue-400"></div>
                                                                <span className="text-sm">Outras Remunerações Eventuais/Provisórias</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-sm text-gray-700">{formatValue(remuneracao.outras_eventuais)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-yellow-50/50 transition-colors">
                                                            <td className="py-4 px-6 font-medium text-gray-900 flex items-center gap-3">
                                                                <div className="p-2 bg-yellow-100">
                                                                    <Calendar className="h-4 w-4 text-yellow-600" />
                                                                </div>
                                                                <span>Abono de Permanência</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 font-mono font-semibold text-yellow-900">{formatValue(remuneracao.abono_permanencia)}</td>
                                                        </tr>
                                                        <tr className="bg-gray-50/50">
                                                            <td className="py-4 px-6 font-semibold text-gray-700 flex items-center gap-3">
                                                                <div className="p-2 bg-red-100">
                                                                    <TrendingUp className="h-4 w-4 text-red-600" />
                                                                </div>
                                                                <span>Descontos Obrigatórios</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 text-gray-500">-</td>
                                                        </tr>
                                                        <tr className="hover:bg-red-50/30 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-gray-600 flex items-center gap-3">
                                                                <div className="w-1 h-1 bg-red-400"></div>
                                                                <span className="text-sm">Reversão do Teto Constitucional</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-sm text-red-600 font-medium">-{formatValue(remuneracao.reversao_teto_const)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-red-50/30 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-gray-600 flex items-center gap-3">
                                                                <div className="w-1 h-1 bg-red-400"></div>
                                                                <span className="text-sm">Imposto de Renda</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-sm text-red-600 font-medium">-{formatValue(remuneracao.imposto_renda)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-red-50/30 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-gray-600 flex items-center gap-3">
                                                                <div className="w-1 h-1 bg-red-400"></div>
                                                                <span className="text-sm">PSSS (Lei 12.618/12)</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-sm text-red-600 font-medium">-{formatValue(remuneracao.previdencia)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-red-50/30 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-gray-600 flex items-center gap-3">
                                                                <div className="w-1 h-1 bg-red-400"></div>
                                                                <span className="text-sm">Faltas</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-sm text-red-600 font-medium">-{formatValue(remuneracao.faltas)}</td>
                                                        </tr>
                                                        <tr className="bg-gray-50/50">
                                                            <td className="py-4 px-6 font-semibold text-orange-900 flex items-center gap-3">
                                                                <div className="p-2 bg-orange-100">
                                                                    <Calculator className="h-4 w-4 text-orange-600" />
                                                                </div>
                                                                <span>Remuneração Liquida Após Descontos Obrigatórios</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 font-mono font-bold text-orange-900">{formatValue(remuneracao.rem_liquida)}</td>
                                                        </tr>
                                                        <tr className="bg-gray-50/50">
                                                            <td className="py-4 px-6 font-semibold text-gray-700 flex items-center gap-3">
                                                                <div className="p-2 bg-green-100">
                                                                    <DollarSign className="h-4 w-4 text-green-600" />
                                                                </div>
                                                                <span>Vantagens Indenizatórias e Compensatórias</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 text-gray-500">-</td>
                                                        </tr>
                                                        <tr className="hover:bg-green-50/30 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-gray-600 flex items-center gap-3">
                                                                <div className="w-1 h-1 bg-green-400"></div>
                                                                <span className="text-sm">Diárias</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-sm text-gray-700">{formatValue(remuneracao.diarias)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-green-50/30 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-gray-600 flex items-center gap-3">
                                                                <div className="w-1 h-1 bg-green-400"></div>
                                                                <span className="text-sm">Auxílios</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-sm text-gray-700">{formatValue(remuneracao.auxilios)}</td>
                                                        </tr>
                                                        <tr className="hover:bg-green-50/30 transition-colors pl-6">
                                                            <td className="py-3 px-6 text-gray-600 flex items-center gap-3">
                                                                <div className="w-1 h-1 bg-green-400"></div>
                                                                <span className="text-sm">Outras Vantagens Indenizatórias</span>
                                                            </td>
                                                            <td className="text-right py-3 px-6 font-mono text-sm text-gray-700">{formatValue(remuneracao.vant_indenizatorias)}</td>
                                                        </tr>
                                                        <tr className="bg-orange-50/50">
                                                            <td className="py-4 px-6 font-semibold text-orange-900 flex items-center gap-3">
                                                                <div className="p-2 bg-orange-100">
                                                                    <Calculator className="h-4 w-4 text-orange-600" />
                                                                </div>
                                                                <span>Remuneração Liquida + Vantagens</span>
                                                            </td>
                                                            <td className="text-right py-4 px-6 font-mono font-bold text-orange-900">{formatValue(remuneracao.total_liquido)}</td>
                                                        </tr>
                                                        <tr className="border-t-2 border-green-500 bg-gradient-to-r from-green-50 to-emerald-50">
                                                            <td className="py-5 px-6 font-bold text-green-900 flex items-center gap-3" title="Esse é o custo efetivo para os cofres públicos">
                                                                <div className="p-2 bg-green-500/20">
                                                                    <TrendingUp className="h-5 w-5 text-green-600" />
                                                                </div>
                                                                <span>Custo Total</span>
                                                            </td>
                                                            <td className="text-right py-5 px-6 font-mono font-bold text-xl text-green-900">
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