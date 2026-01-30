import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
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
import { formatCurrency, formatValue } from '@/lib/utils';
import {
    fetchDeputadoEstadualData,
    DeputadoEstadual,
    GastoPorAno,
    FornecedorEstadual,
    NotaEstadual
} from '@/lib/api';
import { ExternalLink, Phone, Mail, Users, TrendingUp, Calendar, MapPin, Briefcase, User, DollarSign, Building2, ArrowRight } from 'lucide-react';
import { Header } from '@/components/Header';
import { Footer } from '@/components/Footer';
import { ErrorState } from '@/components/ErrorState';
import { LoadingOverlay } from '@/components/LoadingOverlay';
import { DeputadoEstadualDetalheSkeleton } from '@/components/DeputadoEstadualDetalheSkeleton';

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

    <LoadingOverlay isLoading={loading} content="Carregando informações do parlamentar..." />

    if (error) {
        return (
            <ErrorState
                title="Erro ao carregar deputado estadual"
                message={error || "Não foi possível encontrar as informações deste deputado estadual. Verifique se o ID está correto ou tente novamente mais tarde."}
            />
        );
    }

    if (!deputado) {
        return <DeputadoEstadualDetalheSkeleton />;
    }

    const chartData = gastosPorAno?.categories.map((category, index) => ({
        key: `chart-${index}`,
        name: category,
        valor: gastosPorAno.series[index] || 0
    })) || [];

    return (
        <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
            <Header />
            <main className="container mx-auto px-4 py-8">
                {/* Breadcrumb */}
                <div className="flex items-center gap-2 text-sm text-muted-foreground mb-8">
                    <Link to="/deputado-estadual" className="hover:text-foreground transition-colors">
                        Deputados Estaduais
                    </Link>
                    <ArrowRight className="h-4 w-4" />
                    <span className="text-foreground">Perfil do Deputado Estadual</span>
                </div>

                <div className="space-y-8">
                    {/* Profile Card with Modern Design */}
                    <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300 border-t-4 border-t-primary">
                        <div className="relative overflow-hidden bg-gradient-to-r from-primary/10 to-accent/5">
                            {/* Animated geometric shapes for premium feel */}
                            <div className="absolute top-[-20%] right-[-10%] w-64 h-64 bg-primary/5 rounded-full blur-3xl" />
                            <div className="absolute bottom-[-50%] left-[-10%] w-80 h-80 bg-accent/5 rounded-full blur-3xl" />

                            <div className="p-8 relative z-10">
                                <div className="flex flex-col md:flex-row gap-8 items-center md:items-start">
                                    {/* Avatar Section */}
                                    <div className="flex-shrink-0">
                                        <div className="relative group">
                                            <div className="absolute -inset-1 bg-gradient-to-br from-primary to-accent rounded-2xl blur opacity-25 group-hover:opacity-50 transition duration-1000 group-hover:duration-200"></div>
                                            <Avatar className="h-40 w-32 rounded-2xl border-2 border-background shadow-2xl transition-transform duration-500 group-hover:scale-105">
                                                <AvatarImage src={deputado.foto} alt={deputado.nome_parlamentar} />
                                                <AvatarFallback className="rounded-2xl text-3xl font-black bg-muted text-muted-foreground uppercase">
                                                    {deputado.nome_parlamentar.split(" ").filter(n => n.length > 2).slice(0, 2).map(n => n[0]).join("")}
                                                </AvatarFallback>
                                            </Avatar>
                                        </div>
                                    </div>

                                    {/* Main Info Section */}
                                    <div className="flex-1 text-center md:text-left space-y-4">
                                        <div className="space-y-2">
                                            <div className="flex items-center gap-3 flex-wrap justify-center md:justify-start">
                                                <h2 className="text-4xl font-black text-foreground tracking-tight">
                                                    <a
                                                        title="Clique para visitar a Página Oficial do parlamentar"
                                                        href={deputado.perfil}
                                                        target="_blank"
                                                        className="hover:text-primary transition-colors inline-flex items-center gap-2 group"
                                                    >
                                                        {deputado.nome_parlamentar}
                                                        <ExternalLink className="h-5 w-5 opacity-50 group-hover:opacity-100 transition-opacity" />
                                                    </a>
                                                </h2>
                                            </div>

                                            <div className="flex items-center gap-2 flex-wrap justify-center md:justify-start">
                                                <Badge className="font-black bg-primary/10 text-primary border-primary/20 uppercase tracking-widest text-[10px] px-3 py-1" title={deputado.nome_partido}>
                                                    {deputado.sigla_partido}
                                                </Badge>
                                                <Badge variant="outline" className="flex items-center gap-1.5 bg-background/50 backdrop-blur-sm border-muted-foreground/20 font-bold text-[10px] uppercase tracking-widest px-3 py-1" title={deputado.nome_estado}>
                                                    <MapPin className="w-3 h-3 text-primary" />
                                                    {deputado.sigla_estado}
                                                </Badge>
                                            </div>

                                            <p className="text-sm font-medium text-muted-foreground uppercase tracking-widest opacity-80">{deputado.nome_civil}</p>
                                        </div>
                                    </div>

                                    {/* Total Cost Display */}
                                    <div className="text-center md:text-right space-y-2 lg:min-w-[280px]">
                                        <div className="bg-gradient-to-br from-primary to-primary/80 rounded-2xl p-6 shadow-xl shadow-primary/20 text-white transform hover:scale-105 transition-transform duration-300">
                                            <div className="flex items-center justify-center md:justify-end gap-2 mb-1 opacity-90">
                                                <TrendingUp className="h-4 w-4" />
                                                <p className="text-[10px] font-black uppercase tracking-widest">Custo Total Acumulado</p>
                                            </div>
                                            <p className="text-4xl font-black font-mono tracking-tighter">
                                                R$ {deputado.valor_total}
                                            </p>
                                            <p className="text-[9px] font-medium text-white/70 uppercase tracking-tight mt-1">
                                                CEAP • DIÁRIAS
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Contact Info Bar */}
                        <div className="border-t border-border/50 bg-muted/20 px-8 py-4">
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 text-[11px] font-bold uppercase tracking-wider">
                                {deputado.email && <div className="flex items-center gap-3 group">
                                    <div className="p-2 bg-primary/10 rounded-lg text-primary group-hover:bg-primary group-hover:text-primary-foreground transition-colors">
                                        <Mail className="h-4 w-4" />
                                    </div>
                                    <div className="flex flex-col">
                                        <span className="text-[9px] text-muted-foreground font-black">Email oficial</span>
                                        <a href={`mailto:${deputado.email}`} className="text-foreground hover:text-primary transition-colors lowercase font-medium">
                                            {deputado.email}
                                        </a>
                                    </div>
                                </div>}
                                {deputado.telefone && <div className="flex items-center gap-3">
                                    <div className="p-2 bg-primary/10 rounded-lg text-primary">
                                        <Phone className="h-4 w-4" />
                                    </div>
                                    <div className="flex flex-col">
                                        <span className="text-[9px] text-muted-foreground font-black">Telefone</span>
                                        <span className="text-foreground">{deputado.telefone}</span>
                                    </div>
                                </div>}
                                {deputado.site && (
                                    <div className="flex items-center gap-3 group">
                                        <div className="p-2 bg-primary/10 rounded-lg text-primary group-hover:bg-primary group-hover:text-primary-foreground transition-colors">
                                            <ExternalLink className="h-4 w-4" />
                                        </div>
                                        <div className="flex flex-col">
                                            <span className="text-[9px] text-muted-foreground font-black">Site</span>
                                            <a
                                                href={deputado.site}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                className="text-foreground hover:text-primary transition-colors lowercase font-medium"
                                            >
                                                Acessar
                                            </a>
                                        </div>
                                    </div>
                                )}
                            </div>
                        </div>
                    </Card>

                    <div className="grid gap-8 md:grid-cols-2 mb-8">
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
                                            <span className="font-medium text-muted-foreground">Nome Civil:</span>
                                            <p className="text-foreground">{deputado.nome_civil}</p>
                                        </div>
                                    </div>
                                    {deputado.naturalidade && (
                                        <div className="flex items-start gap-2">
                                            <MapPin className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                            <div>
                                                <span className="font-medium text-muted-foreground">Naturalidade:</span>
                                                <p className="text-foreground">{deputado.naturalidade}</p>
                                            </div>
                                        </div>
                                    )}
                                    {deputado.nascimento && (
                                        <div className="flex items-start gap-2">
                                            <Calendar className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                            <div>
                                                <span className="font-medium text-muted-foreground">Nascimento:</span>
                                                <p className="text-foreground">{deputado.nascimento}</p>
                                            </div>
                                        </div>
                                    )}
                                    {deputado.profissao && (
                                        <div className="flex items-start gap-2">
                                            <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                            <div>
                                                <span className="font-medium text-muted-foreground">Profissão:</span>
                                                <p className="text-foreground">{deputado.profissao}</p>
                                            </div>
                                        </div>
                                    )}
                                    <div className="flex items-start gap-2">
                                        <MapPin className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                        <div>
                                            <span className="font-medium text-muted-foreground">Estado:</span>
                                            <p className="text-foreground">{deputado.nome_estado} ({deputado.sigla_estado})</p>
                                        </div>
                                    </div>
                                    <div className="flex items-start gap-2">
                                        <Building2 className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                                        <div>
                                            <span className="font-medium text-muted-foreground">Partido:</span>
                                            <p className="text-foreground">{deputado.nome_partido} ({deputado.sigla_partido})</p>
                                        </div>
                                    </div>

                                </div>
                            </CardContent>
                        </Card>


                        {/* Annual Expenses Chart */}
                        {chartData.length > 0 && (
                            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
                                <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                    <div className="flex items-center gap-4">
                                        <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                                            <TrendingUp className="h-6 w-6 text-primary" />
                                        </div>
                                        <CardTitle className="text-xl">Gastos anuais com a cota parlamentar</CardTitle>
                                    </div>
                                </CardHeader>
                                <CardContent className="p-6">
                                    <ResponsiveContainer width="100%" height={300}>
                                        <BarChart data={chartData}>
                                            <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" vertical={false} />
                                            <XAxis dataKey="name" tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} axisLine={false} tickLine={false} />
                                            <YAxis tickFormatter={formatValue} tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} axisLine={false} tickLine={false} />
                                            <Tooltip
                                                formatter={(value: number) => [formatCurrency(value), 'Valor']}
                                                contentStyle={{
                                                    backgroundColor: 'hsl(var(--card))',
                                                    border: '1px solid hsl(var(--border))',
                                                    borderRadius: '12px',
                                                    boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)',
                                                }}
                                                cursor={{ fill: 'hsl(var(--muted))', opacity: 0.4 }}
                                            />
                                            <Bar dataKey="valor" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} className="transition-all duration-300 hover:opacity-80" />
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
                            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300">
                                <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center gap-4">
                                            <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                                                <Building2 className="h-6 w-6 text-primary" />
                                            </div>
                                            <CardTitle className="text-xl">Maiores Fornecedores</CardTitle>
                                        </div>
                                        <Link
                                            to={`/deputado-estadual/ceap?IdParlamentar=${id}&Periodo=57&Agrupamento=3`}
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
                                            {maioresFornecedores.map((row) => (
                                                <TableRow key={`fornecedor-${row.id_fornecedor}`} className="hover:bg-muted/30 transition-all duration-300 border-b last:border-0 group">
                                                    <TableCell className="py-4 px-6">
                                                        <Link to={`/fornecedor/${row.id_fornecedor}`}
                                                            className="font-bold text-primary hover:text-primary/80 transition-colors flex flex-col">
                                                            {row.nome_fornecedor}
                                                            <span className="font-mono text-[10px] font-black text-muted-foreground uppercase tracking-tight opacity-70 group-hover:opacity-100 transition-opacity">
                                                                {row.cnpj_cpf}
                                                            </span>
                                                        </Link>
                                                    </TableCell>
                                                    <TableCell className="text-right py-4 px-6 font-black font-mono text-foreground group-hover:text-primary transition-colors">
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
                            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300">
                                <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                                    <div className="flex items-center justify-between">
                                        <div className="flex items-center gap-4">
                                            <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                                                <DollarSign className="h-6 w-6 text-primary" />
                                            </div>
                                            <CardTitle className="text-xl">Maiores Notas/Recibos</CardTitle>
                                        </div>
                                        <Link
                                            to={`/deputado-estadual/ceap?IdParlamentar=${id}&Periodo=57&Agrupamento=6`}
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
                                                <TableHead className="py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b" style={{ width: '80%' }}>Fornecedor</TableHead>
                                                <TableHead className="text-right py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b" style={{ width: '20%' }}>Valor</TableHead>
                                            </TableRow>
                                        </TableHeader>
                                        <TableBody>
                                            {maioresNotas.map((row, index) => (
                                                <TableRow key={`nota-${row.id_cl_despesa}-${index}`} className="hover:bg-muted/30 transition-all duration-300 border-b last:border-0 group">
                                                    <TableCell className="py-4 px-6">
                                                        <Link to={`/fornecedor/${row.id_fornecedor}`}
                                                            className="font-bold text-primary hover:text-primary/80 transition-colors flex flex-col">
                                                            {row.nome_fornecedor}
                                                            <span className="font-mono text-[10px] font-black text-muted-foreground uppercase tracking-tight opacity-70 group-hover:opacity-100 transition-opacity">
                                                                {row.cnpj_cpf}
                                                            </span>
                                                        </Link>
                                                    </TableCell>
                                                    <TableCell className="text-right py-4 px-6 font-black font-mono text-foreground group-hover:text-primary transition-colors">
                                                        R$&nbsp;{row.valor_liquido}
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
                                <Card className="shadow-md border-0 bg-card">
                                    <CardContent className="p-12">
                                        <div className="flex flex-col items-center gap-4 text-center">
                                            <Building2 className="h-16 w-16 text-muted-foreground/50" />
                                            <div>
                                                <h3 className="text-lg font-medium text-foreground mb-2">Nenhum dado disponível</h3>
                                                <p className="text-muted-foreground max-w-md">
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