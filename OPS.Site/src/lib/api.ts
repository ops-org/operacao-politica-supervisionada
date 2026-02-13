import { PoliticianType, getApiType, POLITICIAN_TYPES, getImageUrl } from "@/types/politician";
import { Key } from "readline";

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
  // In production (GitHub Pages), use direct API calls
  // if (import.meta.env.PROD) {
  //   return 'https://api.ops.org.br';
  // }

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
  help_text: string;
  image: string;
  tokens: string[];
}

export interface ParliamentSearchRequest {
  busca?: string;
  periodo: number;
}

export const fetchParliamentMembers = async (
  type: PoliticianType,
  busca: string = "",
  periodo: string = "57"
): Promise<DropDownOptions[]> => {

  const apiType = getApiType(type);
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

export interface Fornecedor {
  id_fornecedor: string;
  cnpj_cpf: string;
  nome_fornecedor: any;
  valor_total: string;
}

export const fetchMaioresFornecedores = async (id: string, type: PoliticianType): Promise<Fornecedor[]> => {
  return await apiClient.get<Fornecedor[]>(`/api/${type}/${id}/MaioresFornecedores`);
};

export interface CustoAnual {
  ano: number;
  cota_parlamentar: number;
  verba_gabinete: number;
  salario_patronal: number;
  auxilio_moradia: number;
  auxilio_saude: number;
  diarias: number;
  valor_total_deflacionado: number;
}

export const fetchCustoAnual = async (
  id: string,
  type: PoliticianType): Promise<CustoAnual[]> => {

  const apiType = getApiType(type);
  return await apiClient.get<CustoAnual[]>(`/api/${apiType}/${id}/CustoAnual`);
};

export interface TopSpender {
  id?: number;
  nome_parlamentar: string;
  valor_total: string;
  sigla_partido_estado: string;
}

export interface TopSpendersResponse {
  senadores: TopSpender[];
  deputados_federais: TopSpender[];
  deputados_estaduais: TopSpender[];
}

export const fetchTopSpenders = async (): Promise<TopSpendersResponse> => {
  return await apiClient.get<TopSpendersResponse>("/api/Inicio/ParlamentarResumoGastos");
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

export interface ResumoAnual {
  anos: number[];
  valores: number[];
  valores_deflacionados?: number[];
}

export const fetchResumoAnual = async (type: PoliticianType): Promise<ResumoAnual> => {
  const response = await fetch(`${getApiBaseUrl()}/${type}/resumoanual`);
  if (!response.ok) {
    throw new Error(`Failed to fetch annual summary for ${type}`);
  }
  return response.json();
};

export const fetchParlamentar = async (id: string, type: PoliticianType): Promise<Parlamentar> => {
  return await apiClient.get<Parlamentar>(`/api/${type}/${id}`);
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

export const fetchDocumentoDetalhe = async (id: string, type: PoliticianType): Promise<DocumentoDetalhe> => {
  return await apiClient.get<DocumentoDetalhe>(`/api/${type}/documento/${id}`);
};

export const fetchDocumentosDoMesmoDia = async (id: string, type: PoliticianType): Promise<DocumentoDoDia[]> => {
  return await apiClient.get<DocumentoDoDia[]>(`/api/${type}/${id}/documentosdomesmodia`);
};

export const fetchDocumentosDaSubquotaMes = async (id: string, type: PoliticianType): Promise<DocumentoDaSubquota[]> => {
  return await apiClient.get<DocumentoDaSubquota[]>(`/api/${type}/${id}/documentosdasubcotames`);
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

export const fetchTiposDespesa = async (type: PoliticianType): Promise<TipoDespesa[]> => {
  const result = await apiClient.get<TipoDespesa[]>(`/api/${type}/tipodespesa`);
  return result || [];
};

export const fetchDespesasCotaParlamentar = async (
  type: PoliticianType,
  page: number,
  limit: number,
  sortField: number | null,
  sortOrder: SortOrder,
  filters?: Filters
): Promise<DespesaCotaParlamentarApiResponse> => {

  const endpoint = `/api/${type}/lancamentos`;

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
    data: result.data || []
  };
};

export interface GastoPorAno {
  categories: string[];
  series: number[];
}

export interface MaioresNotas {
  id_despesa: number;
  id_fornecedor: number;
  cnpj_cpf: string;
  nome_fornecedor: string;
  valor_liquido: number;
}

export const fetchMaioresNotas = async (id: string, type: PoliticianType): Promise<MaioresNotas[]> => {
  return await apiClient.get<MaioresNotas[]>(`/api/${type}/${id}/MaioresNotas`);
};

export const fetchMaioresFornecedoresByType = async (id: string, type: PoliticianType): Promise<Fornecedor[]> => {
  return await apiClient.get<Fornecedor[]>(`/api/${type}/${id}/MaioresFornecedores`);
};

export interface ParlamentarDetalhes {
  detalhes: Parlamentar;
  maioresFornecedores: Fornecedor[];
  custoAnual: CustoAnual[];
  maioresNotas: MaioresNotas[];
}

export interface Parlamentar {
  type: PoliticianType;

  id_parlamentar: number;
  nome_parlamentar: string;
  nome_civil: string;
  id_partido: number;
  sigla_partido: string;
  nome_partido: string;
  id_estado: number;
  sigla_estado: string;
  nome_estado: string;
  foto_url: string;
  pagina_oficial_url: string;
  condicao?: string;
  situacao?: string;
  email?: string;
  telefone?: string;
  sala?: string;
  predio?: string;
  id_gabinete: number;
  andar: string;
  perfil: string;
  foto: string;
  naturalidade?: string;
  nascimento?: string;
  falecimento?: string;
  profissao?: string;
  escolaridade?: string;
  municipio_nascimento?: string;
  estado_nascimento?: string;
  site?: string;
  sexo?: string;
  sigla_estado_nascimento?: string;
  nome_municipio_nascimento?: string;
  ativo: boolean;

  // Summary values
  valor_total: string;
  valor_total_ceap: string;
  valor_total_remuneracao?: string;
  valor_total_salario?: string;
  valor_total_auxilio_moradia?: string;
  valor_total_verbas?: string;
  valor_mensal_secretarios?: string;
  secretarios_ativos?: number;
}

export const fetchPoliticianData = async (id: string, type: PoliticianType): Promise<ParlamentarDetalhes> => {
  const [parlamentar, topFornecedores, custosAnuais, topNotas] = await Promise.all([
    fetchParlamentar(id, type),
    fetchMaioresFornecedoresByType(id, type),
    fetchCustoAnual(id, type),
    fetchMaioresNotas(id, type)
  ]);

  let foto_url = parlamentar.foto || getImageUrl(type, parlamentar.id_parlamentar);
  let pagina_oficial_url = parlamentar.perfil || "";

  if (type === POLITICIAN_TYPES.DEPUTADO_FEDERAL) {
    pagina_oficial_url = `https://www.camara.leg.br/deputados/${parlamentar.id_parlamentar}`;
  } else if (type === POLITICIAN_TYPES.SENADOR) {
    pagina_oficial_url = `https://www25.senado.leg.br/web/senadores/senador/${parlamentar.id_parlamentar}`;
  }

  return {
    detalhes: {
      ...parlamentar,
      type,
      foto_url,
      pagina_oficial_url
    },
    maioresFornecedores: topFornecedores,
    custoAnual: custosAnuais,
    maioresNotas: topNotas
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
  type: PoliticianType
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

  const result = await apiClient.post<any>(`/${type}/remuneracao`, payload);
  return result;
};

export const fetchRemuneracaoDetalhe = async (id: string, type: PoliticianType): Promise<RemuneracaoDetalhe> => {
  return await apiClient.get<RemuneracaoDetalhe>(`/${type}/remuneracao/${id}`);
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

export interface BuscaResponse {
  senadores: any[];
  deputados_federais: any[];
  deputados_estaduais: any[];
}

export const fetchBusca = async (query: string): Promise<BuscaResponse> => {
  return await apiClient.get<BuscaResponse>(`/inicio/busca?value=${encodeURIComponent(query)}`);
};

export const fetchImportacao = async (): Promise<ImportacaoData[]> => {
  return await apiClient.get<ImportacaoData[]>("/inicio/importacao");
};