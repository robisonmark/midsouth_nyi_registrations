// API base URL config
// Uses Vite env vars if available, falls back to localhost

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

export default API_BASE_URL;
