export const API_CONFIG = {
  // In production (GitHub Pages), use direct API calls
  // In development, use proxy
  baseURL: import.meta.env.PROD 
    ? 'https://api.ops.org.br' 
    : '/api',
  
  // For GitHub Pages, ensure we're using the correct base path
  getFullURL: (endpoint: string) => {
    const base = import.meta.env.PROD 
      ? 'https://api.ops.org.br' 
      : '/api';
    
    // Remove leading slash from endpoint if present
    const cleanEndpoint = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
    
    return `${base}/${cleanEndpoint}`;
  }
};
