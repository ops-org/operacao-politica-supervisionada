import { useParams, Link } from "react-router-dom";
import { useState, useEffect } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { ErrorState } from "@/components/ErrorState";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { SenadorDetalheSkeleton } from "@/components/SenadorDetalheSkeleton";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { AnnualSummaryChartWithCard } from "@/components/AnnualSummaryChart";
import { StackedAnnualChartWithCard } from "@/components/StackedAnnualChart";
import { formatCurrency, formatValue } from "@/lib/utils";
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from "recharts";
import { apiClient } from "@/lib/api";
import { ExternalLink, Building2, Receipt, User, Mail, Calendar, MapPin, Briefcase, DollarSign, TrendingUp, ArrowRight } from "lucide-react";
import { fetchSenadorDetalhe, SenadorDetalhe as SenadorDetalheType } from "@/lib/api";

const SenadorDetalhe = () => {
  const { id } = useParams();
  const [senador, setSenador] = useState<SenadorDetalheType | null>(null);
  usePageTitle(senador ? senador.nome_parlamentar : "Senador");
  const [gastosAnuais, setGastosAnuais] = useState<any>({});
  const [gastosPessoal, setGastosPessoal] = useState<any>({});
  const [maioresNotas, setMaioresNotas] = useState<any[]>([]);
  const [maioresFornecedores, setMaioresFornecedores] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadData = async () => {
      if (!id) {
        setError("ID do senador não fornecido");
        setLoading(false);
        return;
      }

      try {
        const [
          senadorData,
          gastosAnuaisData,
          gastosPessoalData,
          maioresNotasData,
          maioresFornecedoresData
        ] = await Promise.all([
          fetchSenadorDetalhe(id),
          apiClient.get<any>(`/senador/${id}/GastosPorAno`),
          apiClient.get<any>(`/senador/${id}/GastosComPessoalPorAno`),
          apiClient.get<any[]>(`/senador/${id}/MaioresNotas`),
          apiClient.get<any[]>(`/senador/${id}/MaioresFornecedores`)
        ]);

        setSenador(senadorData);
        setGastosAnuais(gastosAnuaisData);
        setGastosPessoal(gastosPessoalData);
        setMaioresNotas(maioresNotasData);
        setMaioresFornecedores(maioresFornecedoresData);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Erro ao carregar dados do senador");
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [id]);

  {/* Full-screen loading overlay */ }
  <LoadingOverlay isLoading={loading} content="Carregando informações do parlamentar..." />

  if (error) {
    return (
      <ErrorState
        title="Erro ao carregar senador"
        message={error || "Não foi possível encontrar as informações deste senador. Verifique se o ID está correto ou tente novamente mais tarde."}
      />
    );
  }

  if (!senador) {
    return <SenadorDetalheSkeleton />;
  }

  const fotoUrl = `https://static.ops.org.br/senador/${senador.id_sf_senador}_240x300.jpg`;
  const paginaOfficialUrl = `http://www25.senado.leg.br/web/senadores/senador/-/perfil/${senador.id_sf_senador}`;

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
      <Header />
      <main className="container mx-auto px-4 py-8">
        {/* Breadcrumb */}
        <div className="flex items-center gap-2 text-sm text-muted-foreground mb-8">
          <Link to="/senador" className="hover:text-foreground transition-colors">
            Senadores
          </Link>
          <ArrowRight className="h-4 w-4" />
          <span className="text-foreground">Perfil do Senador</span>
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
                        <AvatarImage src={fotoUrl} alt={senador.nome_parlamentar} />
                        <AvatarFallback className="rounded-2xl text-3xl font-black bg-muted text-muted-foreground uppercase">
                          {senador.nome_parlamentar.split(" ").filter(n => n.length > 2).slice(0, 2).map(n => n[0]).join("")}
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
                            title="Clique para visitar a Página Oficial do parlamentar no Senado Federal"
                            href={paginaOfficialUrl}
                            target="_blank"
                            className="hover:text-primary transition-colors inline-flex items-center gap-2 group"
                          >
                            {senador.nome_parlamentar}
                            <ExternalLink className="h-5 w-5 opacity-50 group-hover:opacity-100 transition-opacity" />
                          </a>
                        </h2>
                      </div>

                      <div className="flex items-center gap-2 flex-wrap justify-center md:justify-start">
                        <Badge className="font-black bg-primary/10 text-primary border-primary/20 uppercase tracking-widest text-[10px] px-3 py-1" title={senador.nome_partido}>
                          {senador.sigla_partido}
                        </Badge>
                        <Badge variant="outline" className="flex items-center gap-1.5 bg-background/50 backdrop-blur-sm border-muted-foreground/20 font-bold text-[10px] uppercase tracking-widest px-3 py-1" title={senador.sigla_estado}>
                          <MapPin className="w-3 h-3 text-primary" />
                          {senador.sigla_estado}
                        </Badge>
                        <Badge variant="secondary" className="bg-muted text-muted-foreground border-muted-foreground/20 font-bold text-[10px] uppercase tracking-widest px-3 py-1" title="Condição">
                          {senador.condicao}
                        </Badge>
                      </div>

                      <p className="text-sm font-medium text-muted-foreground uppercase tracking-widest opacity-80">{senador.nome_civil}</p>
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
                        R$ {senador.valor_total}
                      </p>
                      <p className="text-[9px] font-medium text-white/70 uppercase tracking-tight mt-1">
                        CEAPS • VERBA GABINETE
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Contact Info Bar */}
            <div className="border-t border-border/50 bg-muted/20 px-8 py-4">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 text-[11px] font-bold uppercase tracking-wider">
                <div className="flex items-center gap-3 group">
                  <div className="p-2 bg-primary/10 rounded-lg text-primary group-hover:bg-primary group-hover:text-primary-foreground transition-colors">
                    <Mail className="h-4 w-4" />
                  </div>
                  <div className="flex flex-col">
                    <span className="text-[9px] text-muted-foreground font-black">Email oficial</span>
                    <a href={`mailto:${senador.email}`} className="text-foreground hover:text-primary transition-colors lowercase font-medium">
                      {senador.email}
                    </a>
                  </div>
                </div>
              </div>
            </div>
          </Card>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <Card className="shadow-lg border-0 bg-blue-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div className="space-y-1">
                    <p className="text-[10px] font-black text-blue-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">Cota Parlamentar (CEAPS)</p>
                    <p className="text-2xl font-black text-blue-900 font-mono tracking-tighter">R$ {senador.valor_total_ceaps}</p>
                  </div>
                  <div className="p-3 bg-blue-500/10 text-blue-600 rounded-2xl shadow-inner border border-blue-500/10 group-hover:scale-110 transition-transform">
                    <DollarSign className="h-6 w-6" />
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card className="shadow-lg border-0 bg-orange-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div className="space-y-1">
                    <p className="text-[10px] font-black text-orange-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">Verba de Gabinete</p>
                    <p className="text-2xl font-black text-orange-900 font-mono tracking-tighter">R$ {senador.valor_total_remuneracao}</p>
                  </div>
                  <div className="p-3 bg-orange-500/10 text-orange-600 rounded-2xl shadow-inner border border-orange-500/10 group-hover:scale-110 transition-transform">
                    <Building2 className="h-6 w-6" />
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="grid gap-8 lg:grid-cols-2">

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
                      <span className="font-medium text-gray-600">Nome Civil:</span>
                      <p className="text-gray-900">{senador.nome_civil}</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-2">
                    <MapPin className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-gray-600">Naturalidade:</span>
                      <p className="text-gray-900">{senador.naturalidade}</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-2">
                    <Calendar className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-gray-600">Nascimento:</span>
                      <p className="text-gray-900">{senador.nascimento}</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-2">
                    <Building2 className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-gray-600">Partido:</span>
                      <p className="text-gray-900">{senador.nome_partido} ({senador.sigla_partido})</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-2">
                    <MapPin className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-gray-600">Estado:</span>
                      <p className="text-gray-900">{senador.nome_estado} ({senador.sigla_estado})</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-2">
                    <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-gray-600">Condição:</span>
                      <p className="text-gray-900">{senador.condicao}</p>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Charts Section */}
            {(() => {
              // Merge data from both datasets for stacked chart
              const allYears = new Set([
                ...(gastosAnuais?.categories || []),
                ...(gastosPessoal?.categories || [])
              ]);

              const custoAnual = Array.from(allYears).map(year => {
                const ceapsIndex = gastosAnuais?.categories?.indexOf(year);
                const pessoalIndex = gastosPessoal?.categories?.indexOf(year);

                return {
                  ano: year.toString(),
                  cota_parlamentar: ceapsIndex !== undefined && ceapsIndex >= 0 ? (gastosAnuais?.series?.[ceapsIndex] || 0) : 0,
                  verba_gabinete: pessoalIndex !== undefined && pessoalIndex >= 0 ? (gastosPessoal?.series?.[pessoalIndex] || 0) : 0,
                  salario_patronal: 0,
                  auxilio_moradia: 0
                };
              }).sort((a, b) => a.ano.localeCompare(b.ano));

              return custoAnual.length > 0 && (
                <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
                  <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                    <div className="flex items-center gap-4">
                      <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                        <TrendingUp className="h-6 w-6 text-primary" />
                      </div>
                      <CardTitle className="text-xl">Custos por Ano</CardTitle>
                    </div>
                  </CardHeader>
                  <CardContent className="p-6">
                    <ResponsiveContainer width="100%" height={300}>
                      <BarChart data={custoAnual}>
                        <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" vertical={false} />
                        <XAxis dataKey="ano" type="category" tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} axisLine={false} tickLine={false} />
                        <YAxis type="number" tickFormatter={formatValue} tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} axisLine={false} tickLine={false} />
                        <Tooltip
                          formatter={(value: number, name: string) => [formatCurrency(value), name]}
                          labelFormatter={(label) => `Ano: ${label}`}
                          contentStyle={{
                            backgroundColor: 'hsl(var(--card))',
                            border: '1px solid hsl(var(--border))',
                            borderRadius: '12px',
                            boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)',
                          }}
                          cursor={{ fill: 'hsl(var(--muted))', opacity: 0.4 }}
                        />
                        <Bar dataKey="verba_gabinete" stackId="a" fill="hsl(var(--chart-3))" radius={[0, 0, 0, 0]} name="Verba de Gabinete" className="transition-all duration-300 hover:opacity-80" />
                        <Bar dataKey="cota_parlamentar" stackId="a" fill="hsl(var(--chart-2))" radius={[4, 4, 0, 0]} name="Cota Parlamentar" className="transition-all duration-300 hover:opacity-80" />
                      </BarChart>
                    </ResponsiveContainer>
                    <div className="flex justify-center gap-6 mt-6 text-[10px] font-black uppercase tracking-widest flex-wrap">
                      <div className="flex items-center gap-2">
                        <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-3))' }}></div>
                        <span className="text-muted-foreground">Verba de Gabinete</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-2))' }}></div>
                        <span className="text-muted-foreground">Cota Parlamentar</span>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              );
            })()}
          </div>

          {/* Fornecedores and Notas Section */}
          <div className="grid gap-8 lg:grid-cols-2">
            {/* Maiores Fornecedores */}
            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300">
              <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4">
                    <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                      <Building2 className="h-6 w-6 text-primary" />
                    </div>
                    <CardTitle className="text-xl">Maiores fornecedores</CardTitle>
                  </div>
                  <Link
                    to={`/senador/ceap?IdParlamentar=${senador.id_sf_senador}&Periodo=0&Agrupamento=3`}
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
                    {Array.isArray(maioresFornecedores) && maioresFornecedores.length > 0 ? (
                      maioresFornecedores.map((row) => (
                        <TableRow key={row.id_fornecedor} className="hover:bg-muted/30 transition-all duration-300 border-b last:border-0 group">
                          <TableCell className="py-4 px-6">
                            <Link
                              to={`/fornecedor/${row.id_fornecedor}`}
                              className="font-bold text-primary hover:text-primary/80 transition-colors flex flex-col"
                            >
                              {row.nome_fornecedor}
                              {row.cnpj_cpf && (
                                <span className="font-mono text-[10px] font-black text-muted-foreground uppercase tracking-tight opacity-70 group-hover:opacity-100 transition-opacity">
                                  {row.cnpj_cpf}
                                </span>
                              )}
                            </Link>
                          </TableCell>
                          <TableCell className="text-right py-4 px-6">
                            <Link
                              to={`/senador/ceap?IdParlamentar=${senador.id_sf_senador}&Fornecedor=${row.id_fornecedor}&Periodo=0&Agrupamento=6`}
                              className="text-foreground hover:text-primary transition-colors font-black font-mono underline-offset-4 hover:underline"
                            >
                              R$&nbsp;{row.valor_total}
                            </Link>
                          </TableCell>
                        </TableRow>
                      ))
                    ) : (
                      <TableRow>
                        <TableCell colSpan={2} className="text-center text-muted-foreground py-12">
                          <div className="flex flex-col items-center gap-3">
                            <div className="p-4 bg-muted/50 rounded-full">
                              <Building2 className="h-8 w-8 text-muted-foreground/30" />
                            </div>
                            <span className="text-sm font-medium uppercase tracking-widest opacity-50">
                              {Array.isArray(maioresFornecedores) ? 'Nenhum fornecedor encontrado' : 'Carregando...'}
                            </span>
                          </div>
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>

            {/* Maiores Notas/Recibos */}
            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300">
              <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4">
                    <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                      <Receipt className="h-6 w-6 text-primary" />
                    </div>
                    <CardTitle className="text-xl">Maiores Notas/Recibos</CardTitle>
                  </div>
                  <Link
                    to={`/senador/ceap?IdParlamentar=${senador.id_sf_senador}&Periodo=0&Agrupamento=6`}
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
                      <TableHead className="text-right py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b">Valor</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {Array.isArray(maioresNotas) && maioresNotas.length > 0 ? (
                      maioresNotas.map((row) => (
                        <TableRow key={row.id_sf_despesa} className="hover:bg-muted/30 transition-all duration-300 border-b last:border-0 group">
                          <TableCell className="py-4 px-6">
                            <Link
                              to={`/fornecedor/${row.id_fornecedor}`}
                              className="font-bold text-primary hover:text-primary/80 transition-colors flex flex-col"
                            >
                              {row.nome_fornecedor}
                              <span className="font-mono text-[10px] font-black text-muted-foreground uppercase tracking-tight opacity-70 group-hover:opacity-100 transition-opacity">
                                {row.cnpj_cpf}
                              </span>
                            </Link>
                          </TableCell>
                          <TableCell className="text-right py-4 px-6 font-black font-mono text-foreground group-hover:text-primary transition-colors">
                            R$&nbsp;{row.valor}
                          </TableCell>
                        </TableRow>
                      ))
                    ) : (
                      <TableRow>
                        <TableCell colSpan={2} className="text-center text-muted-foreground py-12">
                          <div className="flex flex-col items-center gap-3">
                            <div className="p-4 bg-muted/50 rounded-full">
                              <Receipt className="h-8 w-8 text-muted-foreground/30" />
                            </div>
                            <span>
                              {Array.isArray(maioresNotas) ? 'Nenhuma nota/recibo encontrado' : 'Carregando...'}
                            </span>
                          </div>
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </div>
        </div>
      </main>
      <Footer />
    </div>
  );
};

export default SenadorDetalhe;
