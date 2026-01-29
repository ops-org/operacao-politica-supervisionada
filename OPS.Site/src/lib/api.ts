interface ApiResponse<T> {
  data?: T;
  error?: string;
}

interface FetchOptions {
  method?: 'GET' | 'POST';
  headers?: Record<string, string>;
  body?: any;
}

class ApiError extends Error {
  constructor(message: string, public status?: number) {
    super(message);
    this.name = 'ApiError';
  }
}

const getApiBaseUrl = (): string => {
  const supabaseUrl = import.meta.env.VITE_SUPABASE_URL;
  if (supabaseUrl) {
    return `${supabaseUrl}/functions/v1/api-proxy`;
  }
  // Fallback for local development
  return '/api';
};

export const apiClient = {
  async post<T>(endpoint: string, data?: any, options?: FetchOptions): Promise<T> {
    const baseUrl = getApiBaseUrl();
    const path = endpoint.startsWith('/api') ? endpoint.replace('/api', '') : endpoint;
    const url = `${baseUrl}${path}`;

    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...options?.headers,
      },
      body: data ? JSON.stringify(data) : undefined,
      ...options,
    });

    if (!response.ok) {
      throw new ApiError(`Failed to fetch ${endpoint}`, response.status);
    }

    return response.json();
  },

  async get<T>(endpoint: string, options?: FetchOptions): Promise<T> {
    const baseUrl = getApiBaseUrl();
    const path = endpoint.startsWith('/api') ? endpoint.replace('/api', '') : endpoint;
    const url = `${baseUrl}${path}`;

    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        ...options?.headers,
      },
      ...options,
    });

    if (!response.ok) {
      throw new ApiError(`Failed to fetch ${endpoint}`, response.status);
    }

    return response.json();
  }
};

// Generic fetch function for parliament members
export interface DropDownOptions {
  id: string;
  text: string;
  helpText: string;
  image: string;
  tokens: string[];
}

export interface ParliamentSearchRequest {
  busca?: string;
  periodo: number;
}

export const fetchParliamentMembers = async (
  type: "deputado-federal" | "deputado-estadual" | "senador",
  busca: string = "",
  periodo: string = "57"
): Promise<DropDownOptions[]> => {

  const apiType = type.replace("deputado-federal", "deputado").replace("deputado-estadual", "deputadoestadual");
  const endpoint = `/api/${apiType}/pesquisa`;

  const payload: ParliamentSearchRequest = {
    busca,
    periodo: parseInt(periodo)
  };

  const result = await apiClient.post<DropDownOptions[]>(endpoint, payload);
  return result || [];
};

export const fetchEstados = async (): Promise<DropDownOptions[]> => {
  return await apiClient.get<DropDownOptions[]>("/api/estado");
};

export const fetchPartidos = async (): Promise<DropDownOptions[]> => {
  return await apiClient.get<DropDownOptions[]>("/api/partido");
};

export interface FornecedorSearchRequest {
  nome?: string;
  cnpj?: string;
}

export interface FornecedorResult {
  id_fornecedor: string;
  cnpj_cpf: string;
  nome: string;
}

export const fetchFornecedores = async (searchParams: FornecedorSearchRequest): Promise<FornecedorResult[]> => {
  const result = await apiClient.post<FornecedorResult[]>("/api/fornecedor/consulta", searchParams);
  return result || [];
};

export interface DeputadoDetalhe {
  id_cf_deputado: number;
  id_partido: number;
  sigla_estado: string;
  nome_partido: string;
  id_estado: number;
  sigla_partido: string;
  nome_estado: string;
  nome_parlamentar: string;
  nome_civil: string;
  condicao: string;
  situacao: string;
  sexo: string;
  id_cf_gabinete: number;
  predio: string;
  sala: string;
  andar: string;
  telefone: string;
  email: string;
  escolaridade: string;
  profissao: string;
  nascimento: string;
  falecimento: string;
  sigla_estado_nascimento: string;
  nome_municipio_nascimento: string;
  valor_total_ceap: string;
  secretarios_ativos: string;
  valor_mensal_secretarios: string;
  valor_total_remuneracao: string;
  valor_total_salario: string;
  valor_total_auxilio_moradia: string;
  valor_total: string;
}

export const fetchDeputadoDetalhe = async (id: string): Promise<DeputadoDetalhe> => {
  return await apiClient.get<DeputadoDetalhe>(`/api/deputado/${id}`);
};

export interface Fornecedor {
  id_fornecedor: string;
  cnpj_cpf: string;
  nome_fornecedor: string;
  valor_total: string;
}

export const fetchMaioresFornecedores = async (id: string): Promise<Fornecedor[]> => {
  return await apiClient.get<Fornecedor[]>(`/api/deputado/${id}/MaioresFornecedores`);
};

export interface CustoAnual {
  ano: number;
  cota_parlamentar: string;
  verba_gabinete: string;
  salario_patronal: string;
  auxilio_moradia: string;
}

export const fetchCustoAnual = async (id: string): Promise<CustoAnual[]> => {
  return await apiClient.get<CustoAnual[]>(`/api/deputado/${id}/CustoAnual`);
};

export interface TopSpender {
  id_cf_deputado?: number;
  id_cl_deputado?: number;
  id_sf_senador?: number;
  nome_parlamentar: string;
  valor_total: string;
  sigla_partido_estado: string;
}

export interface TopSpendersResponse {
  senado: TopSpender[];
  camara_federal: TopSpender[];
  camara_estadual: TopSpender[];
}

export const fetchTopSpenders = async (): Promise<TopSpendersResponse> => {
  return await apiClient.get<TopSpendersResponse>("/api/inicio/parlamentarresumogastos");
};

export interface FornecedorDetalhe {
  id_fornecedor: string;
  cnpj_cpf: string;
  data_de_abertura: string;
  categoria: string;
  tipo: string;
  nome: string;
  nome_fantasia: string;
  atividade_principal: string;
  natureza_juridica: string;
  logradouro: string;
  numero: string;
  complemento: string;
  cep: string;
  bairro: string;
  cidade: string;
  estado: string;
  situacao_cadastral: string;
  data_da_situacao_cadastral: string;
  motivo_situacao_cadastral: string;
  situacao_especial: string;
  data_situacao_especial: string;
  endereco_eletronico: string;
  telefone: string;
  telefone2: string;
  ente_federativo_responsavel: string;
  origem: string;
  obtido_em: string;
  capital_social: string;
  doador: number;
  atividade_secundaria: string[];
}

export interface QuadroSocietario {
  nome: string;
  cnpj_cpf: string;
  pais_origem: string | null;
  data_entrada_sociedade: string;
  faixa_etaria: string;
  qualificacao: string;
  nome_representante: string;
  cpf_representante: string | null;
  qualificacao_representante: string;
}

export interface FornecedorDetalheResponse {
  fornecedor: FornecedorDetalhe;
  quadro_societario: QuadroSocietario[];
}

export const fetchFornecedorDetalhe = async (id: string): Promise<FornecedorDetalheResponse> => {
  return await apiClient.get<FornecedorDetalheResponse>(`/api/fornecedor/${id}`);
};

export interface RecebimentosPorAno {
  categories: number[];
  series: number[];
}

export const fetchRecebimentosPorAno = async (id: string): Promise<RecebimentosPorAno> => {
  return await apiClient.get<RecebimentosPorAno>(`/api/fornecedor/${id}/RecebimentosPorAno`);
};

export interface MaiorGasto {
  id: number;
  tipo: string;
  nome_parlamentar: string;
  sigla_partido: string;
  sigla_estado: string;
  ultima_emissao: string;
  valor_total: string;
  link_parlamentar: string;
  link_despesas: string;
}

export const fetchMaioresGastos = async (id: string): Promise<MaiorGasto[]> => {
  return await apiClient.get<MaiorGasto[]>(`/api/fornecedor/${id}/MaioresGastos`);
};

export interface AnnualSummary {
  categories: number[];
  series: number[];
}

export const fetchCamaraResumoAnual = async (): Promise<AnnualSummary> => {
  return await apiClient.get<AnnualSummary>('/api/deputado/camararesumoanual');
};

export interface SenadorDetalhe {
  id_sf_senador: number;
  nome_parlamentar: string;
  sigla_partido: string;
  sigla_estado: string;
  nome_partido: string;
  nome_estado: string;
  nome_civil: string;
  condicao: string;
  naturalidade: string;
  nascimento: string;
  valor_total_remuneracao: string;
  valor_total_ceaps: string;
  valor_total: string;
  email: string;
}

export const fetchSenadorDetalhe = async (id: string): Promise<SenadorDetalhe> => {
  return await apiClient.get<SenadorDetalhe>(`/api/senador/${id}`);
};

export const fetchSenadoResumoAnual = async (): Promise<AnnualSummary> => {
  return await apiClient.get<AnnualSummary>('/api/senador/senadoresumoanual');
};

export interface DocumentoDetalhe {
  id_despesa: number;
  id_documento: number;
  numero_documento: string;
  tipo_documento: string;
  data_emissao: string;
  valor_documento: string;
  valor_glosa: string;
  valor_liquido: string;
  valor_restituicao: string;
  nome_passageiro: string | null;
  trecho_viagem: string | null;
  ano: number;
  mes: number;
  competencia: string;
  id_despesa_tipo: number;
  descricao_despesa: string;
  descricao_despesa_especificacao: string;
  id_parlamentar: number;
  id_deputado: number;
  nome_parlamentar: string;
  sigla_estado: string;
  sigla_partido: string;
  id_fornecedor: number;
  cnpj_cpf: string;
  nome_fornecedor: string;
  favorecido: string;
  observacao: string;
  link: number;
  url_documento?: string;
  url_documento_nfe?: string;
  url_detalhes_documento?: string;
  url_demais_documentos_mes?: string;
  url_beneficiario?: string;
  url_documentos_beneficiario?: string;
}

export interface DocumentoDoDia {
  id_despesa: number;
  id_fornecedor: number;
  cnpj_cpf: string;
  nome_fornecedor: string;
  sigla_estado_fornecedor: string;
  valor_liquido: string;
}

export interface DocumentoDaSubquota {
  id_despesa: number;
  id_fornecedor: number;
  cnpj_cpf: string;
  nome_fornecedor: string;
  sigla_estado_fornecedor: string;
  valor_liquido: string;
}

export const fetchDocumentoDetalhe = async (
  id: string,
  type: "deputado-federal" | "deputado-estadual" | "senador" = "deputado-federal"
): Promise<DocumentoDetalhe> => {
  const apiType = type.replace("deputado-federal", "deputado").replace("deputado-estadual", "deputadoestadual");
  return await apiClient.get<DocumentoDetalhe>(`/api/${apiType}/documento/${id}`);
};

export const fetchDocumentosDoMesmoDia = async (
  id: string,
  type: "deputado-federal" | "deputado-estadual" | "senador" = "deputado-federal"
): Promise<DocumentoDoDia[]> => {
  const apiType = type.replace("deputado-federal", "deputado").replace("deputado-estadual", "deputadoestadual");
  return await apiClient.get<DocumentoDoDia[]>(`/api/${apiType}/${id}/documentosdomesmodia`);
};

export const fetchDocumentosDaSubquotaMes = async (
  id: string,
  type: "deputado-federal" | "deputado-estadual" | "senador" = "deputado-federal"
): Promise<DocumentoDaSubquota[]> => {
  const apiType = type.replace("deputado-federal", "deputado").replace("deputado-estadual", "deputadoestadual");
  return await apiClient.get<DocumentoDaSubquota[]>(`/api/${apiType}/${id}/documentosdasubcotames`);
};

export interface TipoDespesa {
  id: string;
  text: string;
}

export interface Filters {
  Agrupamento: string;
  Periodo: string;
  IdParlamentar: string;
  Despesa: string;
  Estado: string;
  Partido: string;
  Fornecedor: string;
}

export interface DespesaCotaParlamentar {
  id_parlamentar?: number;
  nome_parlamentar?: string;

  id_estado?: number;
  nome_estado?: string;

  id_partido?: number;
  nome_partido?: string;

  id_fornecedor?: number;
  cnpj_cpf?: string;
  nome_fornecedor?: string;
  numero_recibo?: string;

  id_despesa?: number;
  despesa_especificacao?: string;

  id_despesa_tipo?: number;
  despesa_tipo?: string; // tipo_despesa

  total_notas?: string;
  valor_total?: string; // Agrupamentos
  valor_liquido?: string; // Sem Agrupamento
  [key: string]: any;
}

export interface DespesaCotaParlamentarApiResponse {
  records_total: number;
  records_filtered: number;
  data: DespesaCotaParlamentar[];
}

export type SortOrder = 'asc' | 'desc';

export const fetchTiposDespesa = async (
  type: "deputado-federal" | "deputado-estadual" | "senador"
): Promise<TipoDespesa[]> => {
  const apiType = type.replace("deputado-federal", "deputado").replace("deputado-estadual", "deputadoestadual");
  const endpoint = `/api/${apiType}/tipodespesa`;

  const result = await apiClient.get<TipoDespesa[]>(endpoint);
  return result || [];
};

export const fetchDespesasCotaParlamentar = async (
  type: "deputado-federal" | "deputado-estadual" | "senador",
  page: number,
  limit: number,
  sortField: number | null,
  sortOrder: SortOrder,
  filters?: Filters
): Promise<DespesaCotaParlamentarApiResponse> => {

  const apiType = type.replace("deputado-federal", "deputado").replace("deputado-estadual", "deputadoestadual");
  const endpoint = `/api/${apiType}/lancamentos`;

  const order = sortField !== null ? [{
    column: sortField,
    dir: sortOrder
  }] : [];

  const payload = {
    draw: page,
    order: order,
    start: (page - 1) * limit,
    length: limit,
    filters: {
      Agrupamento: filters?.Agrupamento || "1",
      Periodo: filters?.Periodo || "57",
      IdParlamentar: filters?.IdParlamentar || "",
      Despesa: filters?.Despesa || "",
      Estado: filters?.Estado || "",
      Partido: filters?.Partido || "",
      Fornecedor: filters?.Fornecedor || ""
    }
  };

  const result = await apiClient.post<any>(endpoint, payload);

  return {
    records_total: result.records_total || 0,
    records_filtered: result.records_filtered || 0,
    data: parseData(result) || []
  };

  function parseData(apiResult: any): DespesaCotaParlamentar[] {
    let parsedData = apiResult.data || [];

    if (type === 'deputado-federal') {
      parsedData = parsedData.map(d => ({
        ...d,
        id_parlamentar: d.id_cf_deputado,
        id_despesa: d.id_cf_despesa,
        id_despesa_tipo: d.id_sf_despesa_tipo
      }));
    }

    if (type === 'deputado-estadual') {
      parsedData = parsedData.map(d => ({
        ...d,
        id_parlamentar: d.id_cl_deputado,
        id_despesa: d.id_cl_despesa,
        id_despesa_tipo: d.id_cl_despesa_tipo
      }));
    }

    if (type === 'senador') {
      parsedData = parsedData.map(d => ({
        ...d,
        id_parlamentar: d.id_sf_senador,
        id_despesa: d.id_sf_despesa,
        id_despesa_tipo: d.id_sf_despesa_tipo,
        valor_liquido: d.valor_total // TODO: Rename on API (Sem Agrupamento)
      }));
    }

    return parsedData;
  }
};

export interface DeputadoEstadual {
  id: number;
  nome_parlamentar: string;
  nome_civil: string;
  sigla_partido: string;
  nome_partido: string;
  sigla_estado: string;
  nome_estado: string;
  profissao?: string;
  naturalidade?: string;
  nascimento?: string;
  telefone: string;
  email: string;
  perfil?: string;
  site?: string;
  foto?: string;
  valor_total: string;
}

export interface GastoPorAno {
  categories: string[];
  series: number[];
}

export interface FornecedorEstadual {
  id_fornecedor: number;
  cnpj_cpf: string;
  nome_fornecedor: string;
  valor_total: number;
}

export interface NotaEstadual {
  id_cl_despesa: number;
  id_fornecedor: number;
  cnpj_cpf: string;
  nome_fornecedor: string;
  valor_liquido: number;
}

export const fetchDeputadoEstadualDetalhe = async (id: string): Promise<DeputadoEstadual> => {
  return await apiClient.get<DeputadoEstadual>(`/api/deputadoestadual/${id}`);
};

export const fetchDeputadoEstadualGastosPorAno = async (id: string): Promise<GastoPorAno> => {
  return await apiClient.get<GastoPorAno>(`/api/deputadoestadual/${id}/GastosPorAno`);
};

export const fetchDeputadoEstadualMaioresFornecedores = async (id: string): Promise<FornecedorEstadual[]> => {
  return await apiClient.get<FornecedorEstadual[]>(`/api/deputadoestadual/${id}/MaioresFornecedores`);
};

export const fetchDeputadoEstadualMaioresNotas = async (id: string): Promise<NotaEstadual[]> => {
  return await apiClient.get<NotaEstadual[]>(`/api/deputadoestadual/${id}/MaioresNotas`);
};

export const fetchDeputadoEstadualData = async (id: string) => {
  const [deputado, gastosPorAno, maioresFornecedores, maioresNotas] = await Promise.all([
    fetchDeputadoEstadualDetalhe(id),
    fetchDeputadoEstadualGastosPorAno(id),
    fetchDeputadoEstadualMaioresFornecedores(id),
    fetchDeputadoEstadualMaioresNotas(id)
  ]);

  return {
    deputado,
    gastosPorAno,
    maioresFornecedores,
    maioresNotas
  };
};

export interface RemuneracaoData {
  id?: string | number;
  descricao?: string;
  quantidade?: number;
  valor_total?: number;
  vinculo?: string;
  categoria?: string;
  cargo?: string;
  lotacao?: string;
  tipo_folha?: string;
  ano_mes?: string;
  simbolo_funcao?: string;
  referencia_cargo?: string;
}

export interface RemuneracaoApiResponse {
  data: RemuneracaoData[];
  valorTotal?: number;
  recordsTotal?: number;
  draw?: number;
}

export interface RemuneracaoDetalhe {
  nome?: string;
  vinculo?: string;
  situacao?: string;
  lotacao?: string;
  admissao?: string;
  categoria?: string;
  cargo?: string;
  referencia_cargo?: string;
  simbolo_funcao?: string;
  especialidade?: string;
  funcao?: string;
  tipo_folha?: string;
  ano_mes?: string;
  remun_basica?: string;
  vant_pessoais?: string;
  func_comissionada?: string;
  grat_natalina?: string;
  horas_extras?: string;
  outras_eventuais?: string;
  abono_permanencia?: string;
  reversao_teto_const?: string;
  imposto_renda?: string;
  previdencia?: string;
  faltas?: string;
  rem_liquida?: string;
  diarias?: string;
  auxilios?: string;
  vant_indenizatorias?: string;
  total_liquido?: string;
  custo_total?: string;
}

export const fetchRemuneracao = async (
  page: number,
  limit: number,
  sortField: number | null,
  sortOrder: 'asc' | 'desc',
  filters: any,
  type?: "deputado-federal" | "senador"
): Promise<RemuneracaoApiResponse> => {
  const order = sortField !== null ? [{
    column: sortField,
    dir: sortOrder
  }] : [];

  const payload = {
    draw: page,
    order: order,
    start: (page - 1) * limit,
    length: limit,
    filters: filters
  };

  // Remove empty filter values
  Object.keys(filters).forEach(key => {
    if (filters[key] === "" || filters[key] === null) {
      delete filters[key];
    }
  });

  // Determine endpoint based on type
  const endpoint = type === "deputado-federal" ? "/deputado/remuneracao" : "/senador/remuneracao";
  
  const result = await apiClient.post<any>(endpoint, payload);
  return result;
};

export const fetchRemuneracaoDetalhe = async (id: string): Promise<RemuneracaoDetalhe> => {
  return await apiClient.get<RemuneracaoDetalhe>(`/senador/remuneracao/${id}`);
};

export const fetchVinculos = async (): Promise<DropDownOptions[]> => {
  return await apiClient.get<DropDownOptions[]>("/senador/vinculo");
};

export const fetchCategorias = async (): Promise<DropDownOptions[]> => {
  return await apiClient.get<DropDownOptions[]>("/senador/categoria");
};

export const fetchCargos = async (): Promise<DropDownOptions[]> => {
  return await apiClient.get<DropDownOptions[]>("/senador/cargo");
};

export const fetchLotacoes = async (): Promise<DropDownOptions[]> => {
  return await apiClient.get<DropDownOptions[]>("/senador/lotacao");
};

export const fetchGruposFuncionais = async (): Promise<DropDownOptions[]> => {
  return await apiClient.get<DropDownOptions[]>("/deputado/grupofuncional");
};

export const fetchFuncionarios = async (ano: string): Promise<DropDownOptions[]> => {
  return await apiClient.post<DropDownOptions[]>("/deputado/funcionariopesquisa", { ano });
};

export interface ImportacaoData {
  id: number;
  sigla: string;
  nome: string;
  url: string;
  info: string;
  ultima_despesa: string;
  ultima_importacao: string;
}

export const fetchImportacao = async (): Promise<ImportacaoData[]> => {
  return await apiClient.get<ImportacaoData[]>("/inicio/importacao");
};