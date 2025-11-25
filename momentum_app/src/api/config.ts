// API base URL config
// Uses Vite env vars if available, falls back to localhost

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
const REGISTRANTS_API_URL = import.meta.env.REGISTRANTS_API_URL || 'http://localhost:5001';
const EVENTS_API_URL = import.meta.env.EVENTS_API_URL || 'http://localhost:5002';

export default {API_BASE_URL, REGISTRANTS_API_URL, EVENTS_API_URL};
