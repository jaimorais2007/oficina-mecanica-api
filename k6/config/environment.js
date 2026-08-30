export const CONFIG = {
  BASE_URL: __ENV.BASE_URL || 'http://localhost:5000',
  ADMIN_EMAIL: __ENV.ADMIN_EMAIL || 'admin@gmail.com',
  ADMIN_PASSWORD: __ENV.ADMIN_PASSWORD || '123',
  DELAYS: {
    SUCCESS_FLOW: 5, // 5 segundos entre etapas
    REFUSAL_FLOW: 10, // 10 segundos entre etapas
  },
  THRESHOLDS: {
    http_req_failed: ['rate<0.01'], // taxa de falha menor que 1%
    http_req_duration: ['p(95)<2000'], // 95% das requisições abaixo de 2s
  },
};
