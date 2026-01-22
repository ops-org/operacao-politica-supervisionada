import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { LoadingOverlay } from '@/components/LoadingOverlay';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from '@/components/ui/table';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { formatCurrency } from '@/lib/utils';
import {
    fetchDeputadoEstadualData,
    DeputadoEstadual,
    GastoPorAno,
    FornecedorEstadual,
    NotaEstadual
} from '@/lib/api';
import { ExternalLink, Phone, Mail, Users, TrendingUp, Calendar, MapPin, Briefcase, User, DollarSign, Building2 } from 'lucide-react';
import { Header } from '@/components/Header';
import { Footer } from '@/components/Footer';

const DeputadoEstadualDetalhe = () => {
    const { id } = useParams<{ id: string }>();
    const [loading, setLoading] = useState(true);
    const [deputado, setDeputado] = useState<DeputadoEstadual | null>(null);
    const [gastosPorAno, setGastosPorAno] = useState<GastoPorAno | null>(null);
    const [maioresFornecedores, setMaioresFornecedores] = useState<FornecedorEstadual[]>([]);
    const [maioresNotas, setMaioresNotas] = useState<NotaEstadual[]>([]);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchData = async () => {
            if (!id) return;

            try {
                const data = await fetchDeputadoEstadualData(id);
                console.log(data)

                setDeputado(data.deputado);
                setGastosPorAno(data.gastosPorAno);
                setMaioresFornecedores(data.maioresFornecedores);
                setMaioresNotas(data.maioresNotas);

                document.title = `OPS :: Deputado Estadual - ${data.deputado.nome_parlamentar}`;
            } catch (err) {
                setError('Erro ao carregar dados do deputado');
                console.error('Error fetching data:', err);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [id]);

    {/* Full-screen loading overlay */ }
    if (loading || !deputado) {
        return (
            <div className="min-h-screen bg-background">
                <Header />
                <main className="container mx-auto px-4 py-8">
                    <div className="flex items-center justify-center h-64">
                        {error && <p className="text-destructive">{error || "Parlamentar não encontrado"}</p>}
                        {!deputado && <p className="text-muted-foreground">Carregando dados do parlamentar...</p>}
                    </div>
                </main>
                <Footer />
            </div>
        );
    }

    const chartData = gastosPorAno?.categories.map((category, index) => ({
        key: `chart-${index}`,
        name: category,
        valor: gastosPorAno.series[index] || 0
    })) || [];

    return (
        <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
            <Header />
            <main className="flex-1 container mx-auto px-4 py-8">
                <div className="space-y-8">
                    {/* Modern Header */}
                    <div className="text-center space-y-4">
                        <div className="flex items-center justify-center gap-3 mb-4">
                            <User className="h-8 w-8 text-primary" />
                            <h1 className="text-4xl font-bold bg-gradient-to-r from-primary to-primary/80 bg-clip-text text-transparent">
                                Perfil do Deputado Estadual
                            </h1>
                        </div>
                        <p className="text-muted-foreground max-w-3xl mx-auto leading-relaxed">
                            Informações detalhadas sobre o parlamentar e seus gastos.
                        </p>
                    </div>

                    {/* Profile Card with Modern Design */}
                    <Card className="shadow-md border-0 bg-white overflow-hidden">
                        <div className="relative overflow-hidden bg-gradient-to-r from-primary to-primary/80 text-white">
                            <div className="p-6">
                                <div className="flex flex-col md:flex-row gap-6 items-center md:items-start">
                                    {/* Avatar Section */}
                                    <div className="flex-shrink-0">
                                        <div className="relative">
                                            <Avatar className="h-32 w-24 rounded-lg border-4 border-white/30">
                                                <AvatarImage src={deputado.foto} alt={deputado.nome_parlamentar} />
                                                <AvatarFallback className="rounded-lg text-2xl bg-white/20 text-white">
                                                    {deputado.nome_parlamentar.split(" ").map(n => n[0]).join("")}
                                                </AvatarFallback>
                                            </Avatar>
                                        </div>
                                    </div>

                                    {/* Main Info Section */}
                                    <div className="flex-1 text-center md:text-left space-y-3">
                                        <div className="flex items-center gap-3 flex-wrap justify-center md:justify-start">
                                            <h2 className="text-2xl font-bold">
                                                <a
                                                    title="Clique para visitar a Página Oficial do parlamentar"
                                                    href={deputado.perfil}
                                                    target="_blank"
                                                    className="transition-colors inline-flex items-center gap-1"
                                                >
                                                    {deputado.nome_parlamentar}
                                                    <ExternalLink className="h-4 w-4" />
                                                </a>
                                            </h2>
                                        </div>

                                        <div className="flex items-center gap-2 flex-wrap justify-center md:justify-start">
                                            <Badge variant="secondary" className="font-semibold bg-white/20 text-white border-white/30" title={deputado.nome_partido}>
                                                {deputado.sigla_partido}
                                            </Badge>
                                            <Badge variant="outline" className="flex items-center gap-1 bg-white/20 text-white border-white/30" title={deputado.nome_estado}>
                                                <MapPin className="w-3 h-3" />
                                                {deputado.sigla_estado}
                                            </Badge>
                                        </div>

                                        <p className="text-white/90">{deputado.nome_civil}</p>
                                    </div>

                                    {/* Total Cost Display */}
                                    <div className="text-center md:text-right space-y-2">
                                        <div className="bg-white/20 rounded-lg p-4 backdrop-blur-sm">
                                            <p className="text-sm text-white/80">Custo Total Acumulado</p>
                                            <p className="text-3xl font-bold text-white">
                                                R$ {deputado.valor_total}
                                            </p>
                                            <p className="text-xs text-white/70">
                                                * CEAP + Diarias
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Contact Info Bar */}
                        <div className="border-t border-gray-200 bg-gray-50 px-6 py-4">
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 text-sm">
                                {deputado.email && <div className="flex items-center gap-2">
                                    <Mail className="h-4 w-4 text-primary" />
                                    <span className="font-medium">Email:</span>
                                    <a href={`mailto:${deputado.email}`} className="text-primary hover:underline">
                                        {deputado.email}
                                    </a>
                                </div>}
                                {deputado.telefone && <div className="flex items-center gap-2">
                                    <Phone className="h-4 w-4 text-primary" />
                                    <span className="font-medium">Telefone:</span>
                                    <span>{deputado.telefone}</span>
                                </div>}
                                {deputado.site && (
                                    <div className="flex items-center gap-2">
                                        <ExternalLink className="h-4 w-4 text-primary" />
                                        <span className="font-medium">Site:</span>
                                        <a
                                            href={deputado.site}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="text-primary hover:underline"
                                        >
                                            Acessar
                                        </a>
                                    </div>
                                )}
                            </div>
                        </div>
                    </Card>

                    <div className="grid gap-8 md:grid-cols-2 mb-8">
                        {/* Personal Information Card */}
                        <Card className="shadow-md border-0 bg-white">
                            <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                                <div className="flex items-center gap-2">
                                    <User className="h-5 w-5 text-primary" />
                                    <CardTitle className="text-lg">Informações Pessoais</CardTitle>
                                </div>
                            </CardHeader>
                            <CardContent className="p-6">
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                                    <div className="flex items-start gap-2">
                                        <User className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                        <div>
                                            <span className="font-medium text-gray-600">Nome Civil:</span>
                                            <p className="text-gray-900">{deputado.nome_civil}</p>
                                        </div>
                                    </div>
                                    {deputado.naturalidade && (
                                        <div className="flex items-start gap-2">
                                            <MapPin className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                            <div>
                                                <span className="font-medium text-gray-600">Naturalidade:</span>
                                                <p className="text-gray-900">{deputado.naturalidade}</p>
                                            </div>
                                        </div>
                                    )}
                                    {deputado.nascimento && (
                                        <div className="flex items-start gap-2">
                                            <Calendar className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                            <div>
                                                <span className="font-medium text-gray-600">Nascimento:</span>
                                                <p className="text-gray-900">{deputado.nascimento}</p>
                                            </div>
                                        </div>
                                    )}
                                    {deputado.profissao && (
                                        <div className="flex items-start gap-2">
                                            <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                            <div>
                                                <span className="font-medium text-gray-600">Profissão:</span>
                                                <p className="text-gray-900">{deputado.profissao}</p>
                                            </div>
                                        </div>
                                    )}
                                    <div className="flex items-start gap-2">
                                        <MapPin className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                        <div>
                                            <span className="font-medium text-gray-600">Estado:</span>
                                            <p className="text-gray-900">{deputado.nome_estado} ({deputado.sigla_estado})</p>
                                        </div>
                                    </div>
                                    <div className="flex items-start gap-2">
                                        <Building2 className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                        <div>
                                            <span className="font-medium text-gray-600">Partido:</span>
                                            <p className="text-gray-900">{deputado.nome_partido} ({deputado.sigla_partido})</p>
                                        </div>
                                    </div>

                                </div>
                            </CardContent>
                        </Card>


                        {/* Annual Expenses Chart */}
                        {chartData.length > 0 && (
                            <Card className="shadow-md border-0 bg-white">
                                <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                                    <div className="flex items-center gap-2">
                                        <TrendingUp className="h-5 w-5 text-primary" />
                                        <CardTitle className="text-lg">Gastos anuais com a cota parlamentar</CardTitle>
                                    </div>
                                </CardHeader>
                                <CardContent className="p-6">
                                    <ResponsiveContainer width="100%" height={300}>
                                        <BarChart data={chartData}>
                                            <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                                            <XAxis dataKey="name" tick={{ fill: '#6b7280' }} />
                                            <YAxis tickFormatter={(value) => `R$ ${(value / 1000).toFixed(0)}K`} tick={{ fill: '#6b7280' }} />
                                            <Tooltip
                                                formatter={(value: number) => formatCurrency(value)}
                                                contentStyle={{ backgroundColor: '#ffffff', border: '1px solid #e5e7eb', borderRadius: '8px' }}
                                            />
                                            <Bar dataKey="valor" fill="#3b82f6" radius={[4, 4, 0, 0]} />
                                        </BarChart>
                                    </ResponsiveContainer>
                                </CardContent>
                            </Card>
                        )}
                    </div>

                    {/* SC State Warning */}
                    {deputado.sigla_estado === 'SC' && (
                        <Alert className="border-yellow-200 bg-yellow-50">
                            <AlertDescription className="text-yellow-800">
                                Algumas notas podem não ter um fornecedor identificado e com isso não serão apresentadas abaixo!
                            </AlertDescription>
                        </Alert>
                    )}

                    <div className="grid gap-8 lg:grid-cols-2">
                        {/* Principais Fornecedores */}
                        {maioresFornecedores.length > 0 && (
                            <Card className="shadow-md border-0 bg-white">
                                <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center gap-2">
                                            <Building2 className="h-5 w-5 text-primary" />
                                            <CardTitle className="text-lg">Maiores Fornecedores</CardTitle>
                                        </div>
                                        <Link
                                            to={`/deputado-estadual/ceap?IdParlamentar=${id}&Periodo=57&Agrupamento=3`}
                                            className="px-3 py-1 text-xs bg-primary text-primary-foreground rounded hover:bg-primary/90 transition-colors shadow-xs"
                                        >
                                            Lista completa
                                        </Link>
                                    </div>
                                </CardHeader>
                                <CardContent className="p-6">
                                    <Table>
                                        <TableHeader>
                                            <TableRow>
                                                <TableHead>Fornecedor</TableHead>
                                                <TableHead className="text-right">Valor Total</TableHead>
                                            </TableRow>
                                        </TableHeader>
                                        <TableBody>
                                            {maioresFornecedores.map((row) => (
                                                <TableRow key={`fornecedor-${row.id_fornecedor}`} className="hover:bg-gray-50 transition-colors">
                                                    <TableCell>
                                                        <Link to={`/fornecedor/${row.id_fornecedor}`}
                                                            className="hover:text-primary transition-colors flex flex-col">
                                                            {row.nome_fornecedor}
                                                            <span className="font-mono text-xs text-muted-foreground">
                                                                {row.cnpj_cpf}
                                                            </span>
                                                        </Link>
                                                    </TableCell>
                                                    <TableCell className="text-right text-primary py-3 font-bold font-mono">
                                                        R$&nbsp;{row.valor_total}
                                                    </TableCell>
                                                </TableRow>
                                            ))}
                                        </TableBody>
                                    </Table>
                                </CardContent>
                            </Card>
                        )}

                        {/* Maiores Notas/Recibos */}
                        {maioresNotas.length > 0 && (
                            <Card className="shadow-md border-0 bg-white">
                                <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center gap-2">
                                            <DollarSign className="h-5 w-5 text-primary" />
                                            <CardTitle className="text-lg">Maiores Notas/Recibos</CardTitle>
                                        </div>
                                        <Link
                                            to={`/deputado-estadual/ceap?IdParlamentar=${id}&Periodo=57&Agrupamento=6`}
                                            className="px-3 py-1 text-xs bg-primary text-primary-foreground rounded hover:bg-primary/90 transition-colors shadow-xs"
                                        >
                                            Lista completa
                                        </Link>
                                    </div>
                                </CardHeader>
                                <CardContent className="p-6">
                                    <Table>
                                        <TableHeader>
                                            <TableRow>
                                                <TableHead style={{ width: '80%' }}>Fornecedor</TableHead>
                                                <TableHead style={{ width: '20%' }} className="text-right">Valor</TableHead>
                                            </TableRow>
                                        </TableHeader>
                                        <TableBody>
                                            {maioresNotas.map((row) => (
                                                <TableRow key={`nota-${row.id_cl_despesa || row.nome_fornecedor}-${Math.random()}`} className="hover:bg-gray-50 transition-colors">
                                                    <TableCell>
                                                        <Link to={`/fornecedor/${row.id_fornecedor}`}
                                                            className="hover:text-primary transition-colors flex flex-col">
                                                            {row.nome_fornecedor}
                                                            <span className="font-mono text-xs text-muted-foreground">
                                                                {row.cnpj_cpf}
                                                            </span>
                                                        </Link>
                                                    </TableCell>
                                                    <TableCell className="text-right">
                                                        <span className="text-primary font-medium font-mono">
                                                            R$&nbsp;{row.valor_liquido}
                                                        </span>
                                                    </TableCell>
                                                </TableRow>
                                            ))}
                                        </TableBody>
                                    </Table>
                                </CardContent>
                            </Card>
                        )}

                        {/* Empty States */}
                        {maioresFornecedores.length === 0 && maioresNotas.length === 0 && (
                            <div className="lg:col-span-2">
                                <Card className="shadow-md border-0 bg-white">
                                    <CardContent className="p-12">
                                        <div className="flex flex-col items-center gap-4 text-center">
                                            <Building2 className="h-16 w-16 text-gray-300" />
                                            <div>
                                                <h3 className="text-lg font-medium text-gray-900 mb-2">Nenhum dado disponível</h3>
                                                <p className="text-gray-500 max-w-md">
                                                    Não foram encontrados registros de fornecedores ou notas para este parlamentar no período selecionado.
                                                </p>
                                            </div>
                                        </div>
                                    </CardContent>
                                </Card>
                            </div>
                        )}
                    </div>
                </div>
            </main >
            <Footer />
        </div >
    );
};

export default DeputadoEstadualDetalhe;