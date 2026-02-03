export type PoliticianType = "deputado-federal" | "deputado-estadual" | "senador";

export const POLITICIAN_TYPES = {
  DEPUTADO_FEDERAL: "deputado-federal" as const,
  DEPUTADO_ESTADUAL: "deputado-estadual" as const,
  SENADOR: "senador" as const,
} as const;

export const getApiType = (type: PoliticianType): string => {
  switch (type) {
    case POLITICIAN_TYPES.DEPUTADO_FEDERAL:
      return "deputado-federal";
    case POLITICIAN_TYPES.DEPUTADO_ESTADUAL:
      return "deputado-estadual";
    case POLITICIAN_TYPES.SENADOR:
      return "senador";
    default:
      return type;
  }
};

export const getImageUrl = (type: PoliticianType, id: number): string => {
  const baseUrl = "https://static.ops.org.br";
  
  switch (type) {
    case POLITICIAN_TYPES.DEPUTADO_FEDERAL:
      return `${baseUrl}/depfederal/${id}_120x160.jpg`;
    // case POLITICIAN_TYPES.DEPUTADO_ESTADUAL:
    //   return `${baseUrl}/depestadual/${id}_120x160.jpg`;
    case POLITICIAN_TYPES.SENADOR:
      return `${baseUrl}/senador/${id}_120x160.jpg`;
    default:
      return "";
  }
};

export const getDetailUrl = (type: PoliticianType, id: number): string => {
  return `/${type}/${id}`;
};

export const getPoliticianLabel = (type: PoliticianType): string => {
  switch (type) {
    case POLITICIAN_TYPES.DEPUTADO_FEDERAL:
      return "Deputado Federal";
    case POLITICIAN_TYPES.DEPUTADO_ESTADUAL:
      return "Deputado Estadual";
    case POLITICIAN_TYPES.SENADOR:
      return "Senador";
    default:
      return "";
  }
};
