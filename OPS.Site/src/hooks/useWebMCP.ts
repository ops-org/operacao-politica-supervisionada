import { useEffect, useState } from 'react';
import { API_CONFIG } from '@/config/api';

interface MCPTool {
  name: string;
  description: string;
  inputSchema: {
    type: 'object';
    properties: Record<string, any>;
    required?: string[];
  };
}

interface MCPContext {
  tools: MCPTool[];
  provideContext: (tools: MCPTool[]) => void;
}

declare global {
  interface Navigator {
    modelContext?: {
      provideContext: (tools: MCPTool[]) => void;
    };
  }
}

export const useWebMCP = () => {
  const [isSupported, setIsSupported] = useState(false);
  const [isRegistered, setIsRegistered] = useState(false);

  useEffect(() => {
    // Check if WebMCP is supported
    if (typeof navigator !== 'undefined' && navigator.modelContext) {
      setIsSupported(true);
      
      // Define the tools available for AI agents
      const tools: MCPTool[] = [
        {
          name: 'search_parliamentarians',
          description: 'Buscar parlamentares brasileiros (deputados e senadores)',
          inputSchema: {
            type: 'object',
            properties: {
              type: {
                type: 'string',
                enum: ['deputado-federal', 'deputado-estadual', 'senador'],
                description: 'Tipo de parlamentar para buscar'
              },
              name: {
                type: 'string',
                description: 'Nome ou nome parcial para buscar'
              },
              party: {
                type: 'string',
                description: 'Sigla do partido político'
              },
              state: {
                type: 'string',
                description: 'Sigla do estado (UF)'
              },
              limit: {
                type: 'number',
                default: 20,
                description: 'Número máximo de resultados a retornar'
              }
            }
          }
        },
        {
          name: 'get_parliamentarian_details',
          description: 'Obter informações detalhadas sobre um parlamentar específico',
          inputSchema: {
            type: 'object',
            properties: {
              id: {
                type: 'number',
                description: 'ID do Parlamentar'
              },
              type: {
                type: 'string',
                enum: ['deputado-federal', 'deputado-estadual', 'senador'],
                description: 'Tipo de parlamentar'
              }
            },
            required: ['id', 'type']
          }
        },
        {
          name: 'get_parliamentarian_expenses',
          description: 'Obter informações de despesas de um parlamentar',
          inputSchema: {
            type: 'object',
            properties: {
              id: {
                type: 'number',
                description: 'ID do Parlamentar'
              },
              type: {
                type: 'string',
                enum: ['deputado-federal', 'deputado-estadual', 'senador'],
                description: 'Tipo de parlamentar'
              },
              year: {
                type: 'number',
                description: 'Ano para filtrar despesas'
              },
              category: {
                type: 'string',
                description: 'Categoria de despesa para filtrar'
              }
            },
            required: ['id', 'type']
          }
        },
        {
          name: 'search_suppliers',
          description: 'Buscar fornecedores que prestaram serviços aos parlamentares',
          inputSchema: {
            type: 'object',
            properties: {
              name: {
                type: 'string',
                description: 'Nome do fornecedor ou nome parcial'
              },
              cnpj: {
                type: 'string',
                description: 'CNPJ do fornecedor'
              },
              limit: {
                type: 'number',
                default: 20,
                description: 'Número máximo de resultados a retornar'
              }
            }
          }
        },
        {
          name: 'get_supplier_details',
          description: 'Obter informações detalhadas sobre um fornecedor específico',
          inputSchema: {
            type: 'object',
            properties: {
              id: {
                type: 'number',
                description: 'ID do Fornecedor'
              }
            },
            required: ['id']
          }
        },
        {
          name: 'get_political_parties',
          description: 'Obter lista de partidos políticos',
          inputSchema: {
            type: 'object',
            properties: {
              include_stats: {
                type: 'boolean',
                default: false,
                description: 'Incluir informações estatísticas'
              }
            }
          }
        },
        {
          name: 'get_states',
          description: 'Obter lista de estados brasileiros',
          inputSchema: {
            type: 'object',
            properties: {}
          }
        }
      ];

      // Register tools with WebMCP
      try {
        navigator.modelContext.provideContext(tools);
        setIsRegistered(true);
        console.log('WebMCP tools registered successfully');
      } catch (error) {
        console.error('Failed to register WebMCP tools:', error);
      }
    }
  }, []);

  const executeTool = async (toolName: string, parameters: any) => {
    const apiUrl = API_CONFIG.baseURL;
    
    try {
      const response = await buildApiResponse(toolName, parameters, apiUrl);
      
      if (!response.ok) {
        throw new Error(`API request failed: ${response.statusText}`);
      }
      
      return await response.json();
    } catch (error) {
      console.error(`Error executing tool ${toolName}:`, error);
      throw error;
    }
  };

  const buildApiResponse = async (toolName: string, parameters: any, apiUrl: string): Promise<Response> => {
    switch (toolName) {
      case 'search_parliamentarians':
        return await searchParliamentarians(parameters, apiUrl);
      
      case 'get_parliamentarian_details':
        return fetch(`${apiUrl}/${parameters.type}/${parameters.id}`);
      
      case 'get_parliamentarian_expenses':
        return await getParliamentarianExpenses(parameters, apiUrl);
      
      case 'search_suppliers':
        return await searchSuppliers(parameters, apiUrl);
      
      case 'get_supplier_details':
        return fetch(`${apiUrl}/fornecedores/${parameters.id}`);
      
      case 'get_political_parties':
        return fetch(`${apiUrl}/partidos`);
      
      case 'get_states':
        return fetch(`${apiUrl}/estados`);
      
      default:
        throw new Error(`Unknown tool: ${toolName}`);
    }
  };

  const searchParliamentarians = async (parameters: any, apiUrl: string): Promise<Response> => {
    const { type, name, party, state, limit = 20 } = parameters;
    const searchParams = new URLSearchParams();
    if (name) searchParams.append('nome', name);
    if (party) searchParams.append('partido', party);
    if (state) searchParams.append('uf', state);
    searchParams.append('limit', limit.toString());
    
    return fetch(`${apiUrl}/${type}?${searchParams}`);
  };

  const getParliamentarianExpenses = async (parameters: any, apiUrl: string): Promise<Response> => {
    const expenseParams = new URLSearchParams();
    if (parameters.year) expenseParams.append('ano', parameters.year.toString());
    if (parameters.category) expenseParams.append('categoria', parameters.category);
    
    return fetch(`${apiUrl}/${parameters.type}/${parameters.id}/despesas?${expenseParams}`);
  };

  const searchSuppliers = async (parameters: any, apiUrl: string): Promise<Response> => {
    const supplierParams = new URLSearchParams();
    if (parameters.name) supplierParams.append('nome', parameters.name);
    if (parameters.cnpj) supplierParams.append('cnpj', parameters.cnpj);
    supplierParams.append('limit', (parameters.limit || 20).toString());
    
    return fetch(`${apiUrl}/fornecedores?${supplierParams}`);
  };

  return {
    isSupported,
    isRegistered,
    executeTool
  };
};
