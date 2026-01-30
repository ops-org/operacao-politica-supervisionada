import { useParams, Link } from "react-router-dom";
import { useState, useEffect } from "react";
import { usePageTitle } from "@/hooks/usePageTitle";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { ErrorState } from "@/components/ErrorState";
import { LoadingOverlay } from "@/components/LoadingOverlay";
import { DeputadoDetalheSkeleton } from "@/components/DeputadoDetalheSkeleton";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { apiClient } from "@/lib/api";
import { ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip } from "recharts";
import { formatCurrency, formatValue } from "@/lib/utils";
import { ExternalLink, Phone, Mail, Users, TrendingUp, Calendar, MapPin, Briefcase, User, DollarSign, Building2, ArrowRight } from "lucide-react";
import { fetchDeputadoDetalhe, fetchMaioresFornecedores, fetchCustoAnual, DeputadoDetalhe as DeputadoDetalheType, Fornecedor, CustoAnual } from "@/lib/api";

const DeputadoFederalDetalhe = () => {
  const { id } = useParams();
  const [deputado, setDeputado] = useState<DeputadoDetalheType | null>(null);
  usePageTitle(deputado ? deputado.nome_parlamentar : "Deputado Federal");
  const [fornecedores, setFornecedores] = useState<Fornecedor[]>([]);
  const [custoAnual, setCustoAnual] = useState<CustoAnual[]>([]);
  const [maioresNotas, setMaioresNotas] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadData = async () => {
      if (!id) {
        setError("ID do deputado não fornecido");
        setLoading(false);
        return;
      }

      try {
        // Load all data including Vue.js template endpoints
        const [
          deputadoData,
          fornecedoresData,
          custoAnualData,
          maioresNotasData
        ] = await Promise.all([
          fetchDeputadoDetalhe(id),
          fetchMaioresFornecedores(id),
          fetchCustoAnual(id),
          apiClient.get(`/api/deputado/${id}/MaioresNotas`).then((res: any) => { return res; })
        ]);

        setDeputado(deputadoData);
        setFornecedores(fornecedoresData);
        setCustoAnual(custoAnualData);
        setMaioresNotas(maioresNotasData);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Erro ao carregar dados do deputado");
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
        title="Erro ao carregar deputado"
        message={error || "Não foi possível encontrar as informações deste deputado. Verifique se o ID está correto ou tente novamente mais tarde."}
      />
    );
  }

  if (!deputado) {
    return <DeputadoDetalheSkeleton />;
  }

  const fotoUrl = `https://static.ops.org.br/depfederal/${deputado.id_cf_deputado}_120x160.jpg`;
  const paginaOfficialUrl = `https://www.camara.leg.br/deputados/${deputado.id_cf_deputado}`;

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-primary/5 to-accent/5">
      <Header />
      <main className="container mx-auto px-4 py-8">
        {/* Breadcrumb */}
        <div className="flex items-center gap-2 text-sm text-muted-foreground mb-8">
          <Link to="/deputado-federal" className="hover:text-foreground transition-colors">
            Deputados Federais
          </Link>
          <ArrowRight className="h-4 w-4" />
          <span className="text-foreground">Perfil do Deputado Federal</span>
        </div>

        <div className="space-y-8">
          {/* Profile Card with Modern Design */}
          <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300 border-t-4 border-t-primary">
            <div className={`relative overflow-hidden ${deputado.situacao === 'Exercício'
              ? "bg-gradient-to-r from-primary/10 to-accent/5"
              : "bg-gradient-to-r from-slate-500/10 to-transparent"
              }`}>
              {/* Animated geometric shapes for premium feel */}
              <div className="absolute top-[-20%] right-[-10%] w-64 h-64 bg-primary/5 rounded-full blur-3xl" />
              <div className="absolute bottom-[-50%] left-[-10%] w-80 h-80 bg-accent/5 rounded-full blur-3xl" />

              <div className="p-8 relative z-10">
                <div className="flex flex-col md:flex-row gap-8 items-center md:items-start">
                  {/* Avatar Section */}
                  <div className="flex-shrink-0">
                    <div className="relative group">
                      <div className="absolute -inset-1 bg-gradient-to-br from-primary to-accent rounded-2xl blur opacity-25 group-hover:opacity-50 transition duration-1000 group-hover:duration-200"></div>
                      <Avatar className={`h-40 w-32 rounded-2xl border-2 border-background shadow-2xl transition-all duration-500 group-hover:scale-105 ${deputado.situacao !== 'Exercício' ? "grayscale opacity-80" : ""}`}>
                        <AvatarImage src={fotoUrl} alt={deputado.nome_parlamentar} />
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
                            title="Clique para visitar a Página Oficial do parlamentar na Câmara Federal"
                            href={paginaOfficialUrl}
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
                        <Badge variant="secondary" className="bg-muted text-muted-foreground border-muted-foreground/20 font-bold text-[10px] uppercase tracking-widest px-3 py-1" title="Condição">
                          {deputado.condicao}
                        </Badge>
                        {deputado.situacao === 'Exercício' ? (
                          <Badge className="bg-green-500/10 text-green-600 border-green-500/20 font-bold text-[10px] uppercase tracking-widest px-3 py-1">
                            {deputado.situacao}
                          </Badge>
                        ) : (
                          <Badge className="bg-muted text-muted-foreground border-muted-foreground/20 font-bold text-[10px] uppercase tracking-widest px-3 py-1">
                            {deputado.situacao}
                          </Badge>
                        )}
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
                        CEAP • GABINETE • SALÁRIO • AUXÍLIOS
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Contact Info Bar */}
            {deputado.situacao == "Exercício" &&
              <div className="border-t border-border/50 bg-muted/20 px-8 py-4">
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 text-[11px] font-bold uppercase tracking-wider">
                  <div className="flex items-center gap-3 group">
                    <div className="p-2 bg-primary/10 rounded-lg text-primary group-hover:bg-primary group-hover:text-primary-foreground transition-colors">
                      <Mail className="h-4 w-4" />
                    </div>
                    <div className="flex flex-col">
                      <span className="text-[9px] text-muted-foreground font-black">Email oficial</span>
                      <a href={`mailto:${deputado.email}`} className="text-foreground hover:text-primary transition-colors lowercase font-medium">
                        {deputado.email}
                      </a>
                    </div>
                  </div>
                  {deputado.telefone && <div className="flex items-center gap-3">
                    <div className="p-2 bg-primary/10 rounded-lg text-primary">
                      <Phone className="h-4 w-4" />
                    </div>
                    <div className="flex flex-col">
                      <span className="text-[9px] text-muted-foreground font-black">Telefone</span>
                      <span className="text-foreground">{deputado.telefone}</span>
                    </div>
                  </div>}
                  {deputado.sala && <div className="flex items-center gap-3">
                    <div className="p-2 bg-primary/10 rounded-lg text-primary">
                      <MapPin className="h-4 w-4" />
                    </div>
                    <div className="flex flex-col">
                      <span className="text-[9px] text-muted-foreground font-black">Gabinete</span>
                      <span className="text-foreground">SALA {deputado.sala} • ANEXO {deputado.predio}</span>
                    </div>
                  </div>}
                  {deputado.secretarios_ativos && <div className="flex items-center gap-3">
                    <div className="p-2 bg-primary/10 rounded-lg text-primary">
                      <Users className="h-4 w-4" />
                    </div>
                    <div className="flex flex-col">
                      <span className="text-[9px] text-muted-foreground font-black">Secretários ativos</span>
                      <span className="text-foreground">{deputado.secretarios_ativos}</span>
                    </div>
                  </div>}
                </div>
              </div>
            }
          </Card>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <Card className="shadow-lg border-0 bg-blue-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div className="space-y-1">
                    <p className="text-[10px] font-black text-blue-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">Cota Parlamentar</p>
                    <p className="text-2xl font-black text-blue-900 font-mono tracking-tighter">R$ {deputado.valor_total_ceap}</p>
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
                    <p className="text-2xl font-black text-orange-900 font-mono tracking-tighter">R$ {deputado.valor_total_remuneracao}</p>
                  </div>
                  <div className="p-3 bg-orange-500/10 text-orange-600 rounded-2xl shadow-inner border border-orange-500/10 group-hover:scale-110 transition-transform">
                    <Building2 className="h-6 w-6" />
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card className="shadow-lg border-0 bg-purple-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div className="space-y-1">
                    <p className="text-[10px] font-black text-purple-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">Salário Bruto</p>
                    <p className="text-2xl font-black text-purple-900 font-mono tracking-tighter">R$ {deputado.valor_total_salario}</p>
                  </div>
                  <div className="p-3 bg-purple-500/10 text-purple-600 rounded-2xl shadow-inner border border-purple-500/10 group-hover:scale-110 transition-transform">
                    <TrendingUp className="h-6 w-6" />
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card className="shadow-lg border-0 bg-emerald-500/5 backdrop-blur-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 group">
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div className="space-y-1">
                    <p className="text-[10px] font-black text-emerald-600 uppercase tracking-widest opacity-70 group-hover:opacity-100 transition-opacity">Auxílio Moradia</p>
                    <p className="text-2xl font-black text-emerald-900 font-mono tracking-tighter">R$ {deputado.valor_total_auxilio_moradia}</p>
                  </div>
                  <div className="p-3 bg-emerald-500/10 text-emerald-600 rounded-2xl shadow-inner border border-emerald-500/10 group-hover:scale-110 transition-transform">
                    <Users className="h-6 w-6" />
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
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm mb-2">
                  <div className="flex items-start gap-2">
                    <User className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-muted-foreground">Nome Civil:</span>
                      <p className="text-foreground">{deputado.nome_civil}</p>
                    </div>
                  </div>
                  {deputado.falecimento && (
                    <div className="flex items-start gap-2">
                      <Calendar className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                      <div>
                        <span className="font-medium text-muted-foreground">Falecimento:</span>
                        <p className="text-foreground">{deputado.falecimento}</p>
                      </div>
                    </div>
                  )}
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                  <div className="flex items-start gap-2">
                    <MapPin className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-muted-foreground">Naturalidade:</span>
                      <p className="text-foreground">{deputado.nome_municipio_nascimento} - {deputado.sigla_estado_nascimento}</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-2">
                    <Calendar className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-muted-foreground">Nascimento:</span>
                      <p className="text-foreground">{deputado.nascimento}</p>
                    </div>
                  </div>
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
                  {deputado.escolaridade && (
                    <div className="flex items-start gap-2">
                      <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                      <div>
                        <span className="font-medium text-muted-foreground">Escolaridade:</span>
                        <p className="text-foreground">{deputado.escolaridade}</p>
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
                    <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-muted-foreground">Condição:</span>
                      <p className="text-foreground">{deputado.condicao}</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-2">
                    <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-muted-foreground">Situação:</span>
                      <p className="text-foreground">{deputado.situacao}</p>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Charts and Tables */}

            {/* Annual Chart */}
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
                    <Bar dataKey="cota_parlamentar" stackId="a" fill="hsl(var(--chart-2))" radius={[0, 0, 0, 0]} name="Cota Parlamentar" className="transition-all duration-300 hover:opacity-80" />
                    <Bar dataKey="salario_patronal" stackId="a" fill="hsl(var(--chart-4))" radius={[0, 0, 0, 0]} name="Salário Bruto" className="transition-all duration-300 hover:opacity-80" />
                    <Bar dataKey="auxilio_moradia" stackId="a" fill="hsl(var(--chart-5))" radius={[4, 4, 0, 0]} name="Auxílio Moradia" className="transition-all duration-300 hover:opacity-80" />
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
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-4))' }}></div>
                    <span className="text-muted-foreground">Salário Bruto</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-5))' }}></div>
                    <span className="text-muted-foreground">Auxílio Moradia</span>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="grid gap-8 lg:grid-cols-2 mb-8">
            {/* Principais Fornecedores */}
            <Card className="shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden hover:shadow-xl transition-all duration-300">
              <CardHeader className="bg-gradient-to-r from-muted/50 to-muted/10 border-b">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4">
                    <div className="p-3 bg-gradient-to-br from-primary/10 to-primary/20 rounded-xl shadow-inner border border-primary/10">
                      <Building2 className="h-6 w-6 text-primary" />
                    </div>
                    <CardTitle className="text-xl">Principais Fornecedores (CEAP)</CardTitle>
                  </div>
                  <Link
                    to={`/deputado-federal/ceap?IdParlamentar=${deputado.id_cf_deputado}&Periodo=57&Agrupamento=3`}
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
                    {fornecedores.length > 0 ? (
                      fornecedores.map((row) => (
                        <TableRow key={row.id_fornecedor} className="hover:bg-muted/30 transition-all duration-300 border-b last:border-0 group">
                          <TableCell className="py-4 px-6">
                            <Link
                              to={`/fornecedor/${row.id_fornecedor}`}
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
                      ))
                    ) : (
                      <TableRow>
                        <TableCell colSpan={2} className="text-center text-muted-foreground py-12">
                          <div className="flex flex-col items-center gap-3">
                            <div className="p-4 bg-muted/50 rounded-full">
                              <Building2 className="h-8 w-8 text-muted-foreground/30" />
                            </div>
                            <span className="text-sm font-medium uppercase tracking-widest opacity-50">Nenhum fornecedor encontrado</span>
                          </div>
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>

            {/* Recent Expenses */}
            {/* <Card>
            <CardHeader>
              <CardTitle className="text-lg">Últimas Despesas</CardTitle>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Data</TableHead>
                    <TableHead>Fornecedor</TableHead>
                    <TableHead>Tipo</TableHead>
                    <TableHead className="text-right">Valor</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  <TableRow>
                    <TableCell>15/11/2024</TableCell>
                    <TableCell>Empresa de Combustíveis LTDA</TableCell>
                    <TableCell>Combustíveis</TableCell>
                    <TableCell className="text-right text-primary">R$ 1.234,56</TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>12/11/2024</TableCell>
                    <TableCell>Cia Aérea ABC</TableCell>
                    <TableCell>Passagem Aérea</TableCell>
                    <TableCell className="text-right text-primary">R$ 2.345,67</TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>10/11/2024</TableCell>
                    <TableCell>Locadora de Veículos XYZ</TableCell>
                    <TableCell>Locação de Veículos</TableCell>
                    <TableCell className="text-right text-primary">R$ 4.567,89</TableCell>
                  </TableRow>
                  <TableRow>
                    <TableCell>08/11/2024</TableCell>
                    <TableCell>Gráfica e Editora ABC</TableCell>
                    <TableCell>Divulgação</TableCell>
                    <TableCell className="text-right text-primary">R$ 3.456,78</TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            </CardContent>
          </Card> */}

            {/* Maiores Notas/Recibos */}
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
                    to={`/deputado-federal/ceap?IdParlamentar=${deputado.id_cf_deputado}&Periodo=57&Agrupamento=6`}
                    className="px-3 py-1.5 text-xs font-bold bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-all shadow-md hover:shadow-lg active:scale-95"
                  >
                    Lista completa
                  </Link>
                </div>
              </CardHeader>
              <CardContent className="p-0">
                {maioresNotas?.length > 0 && (
                  <Table>
                    <TableHeader className="bg-muted/30">
                      <TableRow className="hover:bg-transparent">
                        <TableHead className="py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b" style={{ width: '80%' }}>Fornecedor</TableHead>
                        <TableHead className="text-right py-4 px-6 text-[10px] font-black uppercase tracking-widest text-muted-foreground border-b" style={{ width: '20%' }}>Valor</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {maioresNotas.map((row) => (
                        <TableRow key={row.id_cf_despesa} className="hover:bg-muted/30 transition-all duration-300 border-b last:border-0 group">
                          <TableCell className="py-4 px-6">
                            <Link to={`/fornecedor/${row.id_fornecedor}`}
                              className="font-bold text-primary hover:text-primary/80 transition-colors flex flex-col">
                              {row.nome_fornecedor}
                              <span className="font-mono text-[10px] font-black text-muted-foreground uppercase tracking-tight opacity-70 group-hover:opacity-100 transition-opacity">
                                {row.cnpj_cpf}
                              </span>
                            </Link>
                          </TableCell>
                          <TableCell className="text-right py-4 px-6">
                            <Link
                              to={`/deputado-federal/ceap/${row.id_cf_despesa}`}
                              className="text-foreground hover:text-primary transition-colors font-black font-mono"
                            >
                              R$&nbsp;{row.valor_liquido}
                            </Link>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                )}
                {maioresNotas?.length === 0 && (
                  <div className="text-center text-muted-foreground py-12">
                    <div className="flex flex-col items-center gap-3">
                      <div className="p-4 bg-muted/50 rounded-full">
                        <DollarSign className="h-8 w-8 text-muted-foreground/30" />
                      </div>
                      <span className="text-sm font-medium uppercase tracking-widest opacity-50">Nenhuma nota/recibo encontrado</span>
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
        </div>
      </main>
      <Footer />
    </div >
  );
};

export default DeputadoFederalDetalhe;
