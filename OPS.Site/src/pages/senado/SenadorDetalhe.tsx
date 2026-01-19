import { useParams, Link } from "react-router-dom";
import { useState, useEffect } from "react";
import { Header } from "@/components/Header";
import { Footer } from "@/components/Footer";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { LoadingOverlay } from "@/components/LoadingOverlay";
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
import { ExternalLink, Building2, Receipt, User, Mail, Calendar, MapPin, Briefcase, DollarSign, TrendingUp } from "lucide-react";
import { fetchSenadorDetalhe, SenadorDetalhe as SenadorDetalheType } from "@/lib/api";

const SenadorDetalhe = () => {
  const { id } = useParams();
  const [senador, setSenador] = useState<SenadorDetalheType | null>(null);
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
  <LoadingOverlay isLoading={loading || !senador} content="Carregando informações do parlamentar..." />

  if (error || !senador) {
    return (
      <div className="min-h-screen bg-background">
        <Header />
        <main className="container mx-auto px-4 py-8">
          <div className="flex items-center justify-center h-64">
            {error && <p className="text-destructive">{error || "Parlamentar não encontrado"}</p>}
            {!senador && <p className="text-muted-foreground">Carregando dados do parlamentar...</p>}
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  const fotoUrl = `https://static.ops.org.br/senador/${senador.id_sf_senador}_240x300.jpg`;
  const paginaOfficialUrl = `http://www25.senado.leg.br/web/senadores/senador/-/perfil/${senador.id_sf_senador}`;

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
                Perfil do Senador
              </h1>
            </div>
            <p className="text-muted-foreground max-w-3xl mx-auto leading-relaxed">
              Informações detalhadas sobre o parlamentar e seus gastos.
            </p>
          </div>

          {/* Profile Card with Modern Design */}
          <Card className="shadow-md border-0 bg-white overflow-hidden">
            <div className="relative overflow-hidden bg-gradient-to-r from-emerald-600 to-emerald-500 text-white">
              <div className="p-6">
                <div className="flex flex-col md:flex-row gap-6 items-center md:items-start">
                  {/* Avatar Section */}
                  <div className="flex-shrink-0">
                    <div className="relative">
                      <Avatar className="h-32 w-24 rounded-lg border-4 border-white/30">
                        <AvatarImage src={fotoUrl} alt={senador.nome_parlamentar} />
                        <AvatarFallback className="rounded-lg text-2xl bg-white/20 text-white">
                          {senador.nome_parlamentar.split(" ").map(n => n[0]).join("")}
                        </AvatarFallback>
                      </Avatar>
                    </div>
                  </div>

                  {/* Main Info Section */}
                  <div className="flex-1 text-center md:text-left space-y-3">
                    <div className="flex items-center gap-3 flex-wrap justify-center md:justify-start">
                      <h2 className="text-2xl font-bold">
                        <a
                          title="Clique para visitar a Página Oficial do parlamentar no Senado Federal"
                          href={paginaOfficialUrl}
                          target="_blank"
                          className="transition-colors inline-flex items-center gap-1"
                        >
                          {senador.nome_parlamentar}
                          <ExternalLink className="h-4 w-4" />
                        </a>
                      </h2>
                    </div>

                    <div className="flex items-center gap-2 flex-wrap justify-center md:justify-start">
                      <Badge variant="secondary" className="font-semibold bg-white/20 text-white border-white/30" title={senador.nome_partido}>
                        {senador.sigla_partido}
                      </Badge>
                      <Badge variant="outline" className="flex items-center gap-1 bg-white/20 text-white border-white/30" title={senador.sigla_estado}>
                        <MapPin className="w-3 h-3" />
                        {senador.sigla_estado}
                      </Badge>
                      <span>-</span>
                      <Badge variant="secondary" className="bg-white/20 text-white border-white/30" title="Condição">
                        {senador.condicao}
                      </Badge>
                    </div>

                    <p className="text-white/90">{senador.nome_civil}</p>
                  </div>

                  {/* Total Cost Display */}
                  <div className="text-center md:text-right space-y-2">
                    <div className="bg-white/20 rounded-lg p-4 backdrop-blur-sm">
                      <p className="text-sm text-white/80">Custo Total Acumulado</p>
                      <p className="text-3xl font-bold text-white font-mono">
                        R$&nbsp;{senador.valor_total}
                      </p>
                      <p className="text-xs text-white/70">
                        * Cota Parlamentar + Verba Gabinete
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Contact Info Bar */}
            <div className="border-t border-gray-200 bg-gray-50 px-2 py-4">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 text-sm">
                <div className="flex items-center gap-2">
                  <Mail className="h-4 w-4 text-primary" />
                  <span className="font-medium">Email:</span>
                  <a href={`mailto:${senador.email}`} className="text-primary hover:underline">
                    {senador.email}
                  </a>
                </div>
              </div>
            </div>
          </Card>

          {/* Modern Stats Cards */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-6">
            <Card className="bg-gradient-to-br from-blue-50 to-blue-100 border-blue-200">
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-blue-600">Cota Parlamentar (CEAPS)</p>
                    <p className="text-lg font-bold text-blue-900">R$ {senador.valor_total_ceaps}</p>
                  </div>
                  <DollarSign className="h-8 w-8 text-blue-500" />
                </div>
              </CardContent>
            </Card>

            <Card className="bg-gradient-to-br from-orange-50 to-orange-100 border-orange-200">
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-orange-600">Verba de Gabinete</p>
                    <p className="text-lg font-bold text-orange-900">R$ {senador.valor_total_remuneracao}</p>
                  </div>
                  <Building2 className="h-8 w-8 text-orange-500" />
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="grid gap-8 lg:grid-cols-2">

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
                <Card className="shadow-md border-0 bg-white">
                  <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                    <div className="flex items-center gap-2">
                      <TrendingUp className="h-5 w-5 text-primary" />
                      <CardTitle className="text-lg">Custos por Ano</CardTitle>
                    </div>
                  </CardHeader>
                  <CardContent className="p-6">
                    <ResponsiveContainer width="100%" height={300}>
                      <BarChart data={custoAnual}>
                        <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                        <XAxis dataKey="ano" type="category" tick={{ fill: '#6b7280' }} />
                        <YAxis type="number" tickFormatter={formatValue} tick={{ fill: '#6b7280' }} />
                        <Tooltip
                          formatter={(value: number, name: string) => [formatCurrency(value), name]}
                          labelFormatter={(label) => `Ano: ${label}`}
                          contentStyle={{ backgroundColor: '#ffffff', border: '1px solid #e5e7eb', borderRadius: '8px' }}
                        />
                        <Bar dataKey="verba_gabinete" stackId="a" fill="#f97316" radius={[0, 0, 0, 0]} name="Verba de Gabinete" />
                        <Bar dataKey="cota_parlamentar" stackId="a" fill="#3b82f6" radius={[0, 0, 0, 0]} name="Cota Parlamentar" />
                        {/* <Bar dataKey="salario_patronal" stackId="a" fill="#10b981" radius={[0, 0, 0, 0]} name="Salário Bruto" />
                        <Bar dataKey="auxilio_moradia" stackId="a" fill="#8b5cf6" radius={[0, 4, 4, 0]} name="Auxílio Moradia" /> */}
                      </BarChart>
                    </ResponsiveContainer>
                    <div className="flex justify-center gap-6 mt-4 text-xs flex-wrap">
                      <div className="flex items-center gap-2">
                        <div className="w-3 h-3 rounded" style={{ backgroundColor: '#f97316' }}></div>
                        <span>Verba de Gabinete</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <div className="w-3 h-3 rounded" style={{ backgroundColor: '#3b82f6' }}></div>
                        <span>Cota Parlamentar</span>
                      </div>
                      {/* <div className="flex items-center gap-2">
                        <div className="w-3 h-3 rounded" style={{ backgroundColor: '#10b981' }}></div>
                        <span>Salário Bruto</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <div className="w-3 h-3 rounded" style={{ backgroundColor: '#8b5cf6' }}></div>
                        <span>Auxílio Moradia</span>
                      </div> */}
                    </div>
                  </CardContent>
                </Card>
              );
            })()}
          </div>

          {/* Fornecedores and Notas Section */}
          <div className="grid gap-8 lg:grid-cols-2">
            {/* Maiores Fornecedores */}
            <Card className="shadow-md border-0 bg-white">
              <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Building2 className="h-5 w-5 text-primary" />
                    <CardTitle className="text-lg">Maiores fornecedores</CardTitle>
                  </div>
                  <Link
                    to={`/senador?IdParlamentar=${senador.id_sf_senador}&Periodo=0&Agrupamento=3`}
                    className="px-3 py-1 text-xs bg-primary text-primary-foreground rounded hover:bg-primary/90 transition-colors shadow-xs"
                  >
                    Lista completa
                  </Link>
                </div>
              </CardHeader>
              <CardContent className="p-6">
                <div >
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Fornecedor</TableHead>
                        <TableHead className="text-right">Valor Total</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {Array.isArray(maioresFornecedores) && maioresFornecedores.length > 0 ? (
                        maioresFornecedores.map((row) => (
                          <TableRow key={row.id_fornecedor} className="hover:bg-gray-50 transition-colors">
                            <TableCell className="font-medium py-3">
                              <Link
                                to={`/fornecedor/${row.id_fornecedor}`}
                                className="hover:text-primary transition-colors flex flex-col"
                              >
                                {row.nome_fornecedor}
                                {row.cnpj_cpf && (
                                  <span className="font-mono text-xs text-muted-foreground">
                                    {row.cnpj_cpf}
                                  </span>
                                )}
                              </Link>
                            </TableCell>
                            <TableCell className="text-right text-primary py-3 font-medium">
                              <Link
                                to={`/senador?IdParlamentar=${senador.id_sf_senador}&Fornecedor=${row.id_fornecedor}&Periodo=0&Agrupamento=6`}
                                className="hover:underline font-bold font-mono"
                              >
                                R$&nbsp;{row.valor_total}
                              </Link>
                            </TableCell>
                          </TableRow>
                        ))
                      ) : (
                        <TableRow>
                          <TableCell colSpan={2} className="text-center text-muted-foreground py-8">
                            <div className="flex flex-col items-center gap-2">
                              <Building2 className="h-8 w-8 text-gray-400" />
                              <span>
                                {Array.isArray(maioresFornecedores) ? 'Nenhum fornecedor encontrado' : 'Carregando...'}
                              </span>
                            </div>
                          </TableCell>
                        </TableRow>
                      )}
                    </TableBody>
                  </Table>
                </div>
              </CardContent>
            </Card>

            {/* Maiores Notas/Recibos */}
            <Card className="shadow-md border-0 bg-white">
              <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Receipt className="h-5 w-5 text-primary" />
                    <CardTitle className="text-lg">Maiores Notas/Recibos</CardTitle>
                  </div>
                  <Link
                    to={`/senador?IdParlamentar=${senador.id_sf_senador}&Periodo=0&Agrupamento=6`}
                    className="px-3 py-1 text-xs bg-primary text-primary-foreground rounded hover:bg-primary/90 transition-colors shadow-xs"
                  >
                    Lista completa
                  </Link>
                </div>
              </CardHeader>
              <CardContent className="p-6">
                <div>
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Fornecedor</TableHead>
                        <TableHead className="text-right">Valor</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {Array.isArray(maioresNotas) && maioresNotas.length > 0 ? (
                        maioresNotas.map((row) => (
                          <TableRow key={row.id_sf_despesa} className="hover:bg-gray-50 transition-colors">
                            <TableCell className="font-medium py-3">
                              <Link
                                to={`/fornecedor/${row.id_fornecedor}`}
                                className="hover:text-primary transition-colors flex flex-col"
                              >
                                {row.nome_fornecedor}
                                {row.cnpj_cpf && (
                                  <span className="font-mono text-xs text-muted-foreground">
                                    {row.cnpj_cpf}
                                  </span>
                                )}
                              </Link>
                            </TableCell>
                            <TableCell className="text-right text-primary py-3 font-bold font-mono">
                              R$&nbsp;{row.valor}
                            </TableCell>
                          </TableRow>
                        ))
                      ) : (
                        <TableRow>
                          <TableCell colSpan={2} className="text-center text-muted-foreground py-8">
                            <div className="flex flex-col items-center gap-2">
                              <Receipt className="h-8 w-8 text-gray-400" />
                              <span>
                                {Array.isArray(maioresNotas) ? 'Nenhuma nota/recibo encontrado' : 'Carregando...'}
                              </span>
                            </div>
                          </TableCell>
                        </TableRow>
                      )}
                    </TableBody>
                  </Table>
                </div>
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
