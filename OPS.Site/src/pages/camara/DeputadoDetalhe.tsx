import { useParams, Link } from "react-router-dom";
import { useState, useEffect } from "react";
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
import { ExternalLink, Phone, Mail, Users, TrendingUp, Calendar, MapPin, Briefcase, User, DollarSign, Building2 } from "lucide-react";
import { fetchDeputadoDetalhe, fetchMaioresFornecedores, fetchCustoAnual, DeputadoDetalhe as DeputadoDetalheType, Fornecedor, CustoAnual } from "@/lib/api";

const DeputadoFederalDetalhe = () => {
  const { id } = useParams();
  const [deputado, setDeputado] = useState<DeputadoDetalheType | null>(null);
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
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <Header />
      <main className="flex-1 container mx-auto px-4 py-8">
        <div className="space-y-8">
          {/* Modern Header */}
          <div className="text-center space-y-4">
            <div className="flex items-center justify-center gap-3 mb-4">
              <User className="h-8 w-8 text-primary" />
              <h1 className="text-4xl font-bold bg-gradient-to-r from-primary to-primary/80 bg-clip-text text-transparent">
                Perfil do Deputado Federal
              </h1>
            </div>
            <p className="text-muted-foreground max-w-3xl mx-auto leading-relaxed">
              Informações detalhadas sobre o parlamentar e seus gastos.
            </p>
          </div>

          {/* Profile Card with Modern Design */}
          <Card className="shadow-md border-0 bg-white overflow-hidden">
            <div className={`relative overflow-hidden ${deputado.situacao === 'Exercício'
              ? "bg-gradient-to-r from-emerald-600 to-emerald-500 text-white"
              : "bg-gradient-to-r from-gray-500 to-gray-600 text-white"
              }`}>
              <div className="p-6">
                <div className="flex flex-col md:flex-row gap-6 items-center md:items-start">
                  {/* Avatar Section */}
                  <div className="flex-shrink-0">
                    <div className="relative">
                      <Avatar className="h-32 w-24 rounded-lg border-4 border-white/30">
                        <AvatarImage src={fotoUrl} alt={deputado.nome_parlamentar} />
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
                          title="Clique para visitar a Página Oficial do parlamentar na Câmara Federal"
                          href={paginaOfficialUrl}
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
                      <span>-</span>
                      <Badge variant="secondary" className="bg-white/20 text-white border-white/30" title="Condição">
                        {deputado.condicao}
                      </Badge>
                      <Badge variant="secondary" className="bg-white/20 text-white border-white/30" title="Situação">
                        {deputado.situacao}
                      </Badge>
                    </div>

                    <p className="text-white/90">{deputado.nome_civil}</p>
                  </div>

                  {/* Total Cost Display */}
                  <div className="text-center md:text-right space-y-2">
                    <div className="bg-white/20 rounded-lg p-4 backdrop-blur-sm">
                      <p className="text-sm text-white/80">Custo Total Acumulado</p>
                      <p className="text-3xl font-bold text-white font-mono">
                        R$ {deputado.valor_total}
                      </p>
                      <p className="text-xs text-white/70">
                        * CEAP + Verba Gabinete + Salário + Auxílios
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Contact Info Bar */}
            {deputado.situacao == "Ativo" &&
              <div className="border-t border-gray-200 bg-gray-50 px-2 py-4">
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 text-sm">
                  <div className="flex items-center gap-2">
                    <Mail className="h-4 w-4 text-primary" />
                    <span className="font-medium">Email:</span>
                    <a href={`mailto:${deputado.email}`} className="text-primary hover:underline">
                      {deputado.email}
                    </a>
                  </div>
                  <div className="flex items-center gap-2">
                    <Phone className="h-4 w-4 text-primary" />
                    <span className="font-medium">Telefone:</span>
                    <span>{deputado.telefone}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <MapPin className="h-4 w-4 text-primary" />
                    <span className="font-medium">Gabinete:</span>
                    <span>Sala {deputado.sala} - Anexo {deputado.predio}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <Building2 className="h-4 w-4 text-primary" />
                    <span className="font-medium">Secretários Ativos:</span>
                    <span>{deputado.secretarios_ativos}</span>
                  </div>
                </div>
              </div>
               }
          </Card>

          {/* Modern Stats Cards */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <Card className="bg-gradient-to-br from-blue-50 to-blue-100 border-blue-200">
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-blue-600">Cota Parlamentar</p>
                    <p className="text-lg font-bold text-blue-900">R$ {deputado.valor_total_ceap}</p>
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
                    <p className="text-lg font-bold text-orange-900">R$ {deputado.valor_total_remuneracao}</p>
                  </div>
                  <Building2 className="h-8 w-8 text-orange-500" />
                </div>
              </CardContent>
            </Card>

            <Card className="bg-gradient-to-br from-purple-50 to-purple-100 border-purple-200">
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-purple-600">Salário Bruto</p>
                    <p className="text-lg font-bold text-purple-900">R$ {deputado.valor_total_salario}</p>
                  </div>
                  <TrendingUp className="h-8 w-8 text-purple-500" />
                </div>
              </CardContent>
            </Card>

            <Card className="bg-gradient-to-br from-green-50 to-green-100 border-green-200">
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-green-600">Auxílio Moradia</p>
                    <p className="text-lg font-bold text-green-900">R$ {deputado.valor_total_auxilio_moradia}</p>
                  </div>
                  <Users className="h-8 w-8 text-green-500" />
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="grid gap-8 lg:grid-cols-2">
            {/* Personal Information Card */}
            <Card className="shadow-md border-0 bg-white">
              <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                <div className="flex items-center gap-2">
                  <User className="h-5 w-5 text-blue-600" />
                  <CardTitle className="text-lg">Informações Pessoais</CardTitle>
                </div>
              </CardHeader>
              <CardContent className="p-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm mb-2">
                  <div className="flex items-start gap-2">
                    <User className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-gray-600">Nome Civil:</span>
                      <p className="text-gray-900">{deputado.nome_civil}</p>
                    </div>
                  </div>
                  {deputado.falecimento && (
                    <div className="flex items-start gap-2">
                      <Calendar className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                      <div>
                        <span className="font-medium text-gray-600">Falecimento:</span>
                        <p className="text-gray-900">{deputado.falecimento}</p>
                      </div>
                    </div>
                  )}
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                  <div className="flex items-start gap-2">
                    <MapPin className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-gray-600">Naturalidade:</span>
                      <p className="text-gray-900">{deputado.nome_municipio_nascimento} - {deputado.sigla_estado_nascimento}</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-2">
                    <Calendar className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-gray-600">Nascimento:</span>
                      <p className="text-gray-900">{deputado.nascimento}</p>
                    </div>
                  </div>
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
                  {deputado.escolaridade && (
                    <div className="flex items-start gap-2">
                      <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                      <div>
                        <span className="font-medium text-gray-600">Escolaridade:</span>
                        <p className="text-gray-900">{deputado.escolaridade}</p>
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
                    <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-gray-600">Condição:</span>
                      <p className="text-gray-900">{deputado.condicao}</p>
                    </div>
                  </div>
                  <div className="flex items-start gap-2">
                    <Briefcase className="h-4 w-4 text-primary mt-0.5 flex-shrink-0" />
                    <div>
                      <span className="font-medium text-gray-600">Situação:</span>
                      <p className="text-gray-900">{deputado.situacao}</p>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Charts and Tables */}

            {/* Annual Chart */}
            <Card className="shadow-md border-0 bg-white">
              <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                <div className="flex items-center gap-2">
                  <TrendingUp className="h-5 w-5 text-green-600" />
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
                    <Bar dataKey="salario_patronal" stackId="a" fill="#10b981" radius={[0, 0, 0, 0]} name="Salário Bruto" />
                    <Bar dataKey="auxilio_moradia" stackId="a" fill="#8b5cf6" radius={[0, 4, 4, 0]} name="Auxílio Moradia" />
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
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded" style={{ backgroundColor: '#10b981' }}></div>
                    <span>Salário Bruto</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded" style={{ backgroundColor: '#8b5cf6' }}></div>
                    <span>Auxílio Moradia</span>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="grid gap-8 lg:grid-cols-2 mb-8">
            {/* Principais Fornecedores */}
            <Card className="shadow-md border-0 bg-white">
              <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Building2 className="h-5 w-5 text-orange-600" />
                    <CardTitle className="text-lg">Principais Fornecedores (CEAP)</CardTitle>
                  </div>
                  <Link
                    to={`/deputado-federal/ceap?IdParlamentar=${deputado.id_cf_deputado}&Periodo=57&Agrupamento=3`}
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
                    {fornecedores.length > 0 ? (
                      fornecedores.map((row) => (
                        <TableRow key={row.id_fornecedor} className="hover:bg-gray-50 transition-colors">
                          <TableCell className="font-medium py-3">
                            <Link
                              to={`/fornecedor/${row.id_fornecedor}`}
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
                      ))
                    ) : (
                      <TableRow>
                        <TableCell colSpan={2} className="text-center text-muted-foreground py-8">
                          <div className="flex flex-col items-center gap-2">
                            <Building2 className="h-8 w-8 text-gray-400" />
                            <span>Nenhum fornecedor encontrado</span>
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
            <Card className="shadow-md border-0 bg-white">
              <CardHeader className="bg-gradient-to-r from-gray-50 to-gray-100 border-b">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <DollarSign className="h-5 w-5 text-purple-600" />
                    <CardTitle className="text-lg">Maiores Notas/Recibos</CardTitle>
                  </div>
                  <Link
                    to={`/deputado-federal/ceap?IdParlamentar=${deputado.id_cf_deputado}&Periodo=57&Agrupamento=6`}
                    className="px-3 py-1 text-xs bg-primary text-primary-foreground rounded hover:bg-primary/90 transition-colors shadow-xs"
                  >
                    Lista completa
                  </Link>
                </div>
              </CardHeader>
              <CardContent className="p-6">
                {maioresNotas?.length > 0 && (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead style={{ width: '80%' }}>Fornecedor</TableHead>
                        <TableHead style={{ width: '20%' }} className="text-right">Valor</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {maioresNotas.map((row) => (
                        <TableRow key={row.id_cf_despesa} className="hover:bg-gray-50 transition-colors">
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
                            <Link
                              to={`/deputado-federal/ceap/${row.id_cf_despesa}`}
                              className="text-primary hover:underline font-bold font-mono"
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
                  <div className="text-center text-muted-foreground py-8">
                    <div className="flex flex-col items-center gap-2">
                      <DollarSign className="h-8 w-8 text-gray-400" />
                      <span>Nenhuma nota/recibo encontrado</span>
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
