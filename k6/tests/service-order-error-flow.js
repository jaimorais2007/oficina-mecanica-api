import http from 'k6/http';
import { sleep, group, check } from 'k6';
import { Trend, Counter, Rate } from 'k6/metrics';
import { CONFIG } from '../config/environment.js';
import { login } from '../helpers/auth.js';
import { setupSupportData } from '../helpers/setup-data.js';
import { createServiceOrder } from '../helpers/api-client.js';

const errorFlowDuration = new Trend('error_flow_duration');
const simulatedErrorsCounter = new Counter('simulated_errors_total');
const errorAssertionsRate = new Rate('error_assertions_success_rate');

const NON_EXISTENT_ID = '00000000-0000-0000-0000-000000000000';

export const options = {
  scenarios: {
    service_order_errors: {
      executor: 'ramping-vus',
      startVUs: 1,
      stages: [
        { duration: '10s', target: 2 },
        { duration: '20s', target: 3 },
        { duration: '10s', target: 0 },
      ],
      gracefulRampDown: '10s',
    },
  },
  thresholds: {
    error_assertions_success_rate: ['rate>0.99'],
    simulated_errors_total: ['count>0'],
  },
};

/**
 * Executado 1 única vez antes dos VUs.
 * Faz login como administrador e cria dados de suporte válidos para os testes de transição.
 */
export function setup() {
  console.log(`[SETUP] Conectando a ${CONFIG.BASE_URL} com ${CONFIG.ADMIN_EMAIL}...`);
  const auth = login(CONFIG.BASE_URL, CONFIG.ADMIN_EMAIL, CONFIG.ADMIN_PASSWORD);
  console.log('[SETUP] Login realizado com sucesso. Criando dados de suporte...');

  const supportOptions = setupSupportData(CONFIG.BASE_URL, auth.headers);
  console.log('[SETUP] Dados de suporte criados com sucesso.');

  return {
    baseUrl: CONFIG.BASE_URL,
    headers: auth.headers,
    supportOptions: supportOptions,
    delaySeconds: 1,
  };
}

/**
 * Função principal executada pelos VUs.
 * Simula todos os cenários de erro da API de Ordem de Serviço.
 */
export default function (data) {
  const startTime = new Date();
  const baseUrl = data.baseUrl;
  const headers = data.headers;
  const selectedOption = data.supportOptions[Math.floor(Math.random() * data.supportOptions.length)];

  group('Fluxo de Teste de Erros - Ordem de Serviço', function () {
    group('1. Erro de Autenticação (Sem Token Bearer)', function () {
      const res = http.get(`${baseUrl}/api/ServiceOrders`);
      const ok = check(res, {
        'GET /api/ServiceOrders sem token retorna 401 Unauthorized': (r) => r.status === 401,
      });
      errorAssertionsRate.add(ok);
      simulatedErrorsCounter.add(1);
    });

    sleep(data.delaySeconds);

    group('2. Erros de Validação na Criação de OS', function () {
      const payloadEmptyVehicle = JSON.stringify({
        customerId: selectedOption.customer.id,
        vehicleId: NON_EXISTENT_ID,
        servicesUsed: [selectedOption.initialService.id],
      });

      const resEmptyVehicle = http.post(`${baseUrl}/api/ServiceOrders`, payloadEmptyVehicle, { headers });
      const ok1 = check(resEmptyVehicle, {
        'Criar OS com VehicleId vazio retorna 400 Bad Request': (r) => r.status === 400,
        'Mensagem: Veículo da Ordem de Serviço não informado': (r) => {
          const b = r.json();
          return b && b.message && b.message.includes('Veículo da Ordem de Serviço não informado');
        },
      });
      errorAssertionsRate.add(ok1);
      simulatedErrorsCounter.add(1);

      const payloadEmptyServices = JSON.stringify({
        customerId: selectedOption.customer.id,
        vehicleId: selectedOption.vehicle.id,
        servicesUsed: [],
      });

      const resEmptyServices = http.post(`${baseUrl}/api/ServiceOrders`, payloadEmptyServices, { headers });
      const ok2 = check(resEmptyServices, {
        'Criar OS com serviços vazios retorna 400 Bad Request': (r) => r.status === 400,
        'Mensagem: Serviços que serão feitos não foram informados': (r) => {
          const b = r.json();
          return b && b.message && b.message.includes('Serviços que serão feitos não foram informados');
        },
      });
      errorAssertionsRate.add(ok2);
      simulatedErrorsCounter.add(1);
    });

    sleep(data.delaySeconds);

    group('3. Erros de Entidade Não Encontrada (ID Inexistente)', function () {
      const resStart = http.post(`${baseUrl}/api/ServiceOrders/${NON_EXISTENT_ID}/start-analysis`, null, { headers });
      const okStart = check(resStart, {
        'POST /start-analysis com ID inexistente retorna 404': (r) => r.status === 404,
      });
      errorAssertionsRate.add(okStart);
      simulatedErrorsCounter.add(1);

      const resFinishAnalysis = http.post(`${baseUrl}/api/ServiceOrders/${NON_EXISTENT_ID}/finish-analysis`, null, { headers });
      const okFinishAnalysis = check(resFinishAnalysis, {
        'POST /finish-analysis com ID inexistente retorna 400': (r) => r.status === 400,
      });
      errorAssertionsRate.add(okFinishAnalysis);
      simulatedErrorsCounter.add(1);

      const resApprove = http.post(`${baseUrl}/api/ServiceOrders/${NON_EXISTENT_ID}/approve`, null, { headers });
      const okApprove = check(resApprove, {
        'POST /approve com ID inexistente retorna 400': (r) => r.status === 400,
      });
      errorAssertionsRate.add(okApprove);
      simulatedErrorsCounter.add(1);

      const resFinishExecution = http.post(`${baseUrl}/api/ServiceOrders/${NON_EXISTENT_ID}/finish-execution`, null, { headers });
      const okFinishExecution = check(resFinishExecution, {
        'POST /finish-execution com ID inexistente retorna 400': (r) => r.status === 400,
      });
      errorAssertionsRate.add(okFinishExecution);
      simulatedErrorsCounter.add(1);

      const resDeliver = http.post(`${baseUrl}/api/ServiceOrders/${NON_EXISTENT_ID}/deliver`, null, { headers });
      const okDeliver = check(resDeliver, {
        'POST /deliver com ID inexistente retorna 400': (r) => r.status === 400,
      });
      errorAssertionsRate.add(okDeliver);
      simulatedErrorsCounter.add(1);

      const resRefuse = http.post(`${baseUrl}/api/ServiceOrders/${NON_EXISTENT_ID}/refuse`, null, { headers });
      const okRefuse = check(resRefuse, {
        'POST /refuse com ID inexistente retorna 400': (r) => r.status === 400,
      });
      errorAssertionsRate.add(okRefuse);
      simulatedErrorsCounter.add(1);
    });

    sleep(data.delaySeconds);

    group('4. Erros de Peças / Quantidade Inválida', function () {
      const payloadInvalidQty = JSON.stringify({
        partId: selectedOption.part.id,
        quantity: 0,
      });

      const resInvalidQty = http.post(`${baseUrl}/api/ServiceOrders/${NON_EXISTENT_ID}/parts`, payloadInvalidQty, { headers });
      const okQty = check(resInvalidQty, {
        'Adicionar peça com quantidade 0 retorna 400 Bad Request': (r) => r.status === 400,
        'Mensagem: A quantidade deve ser maior que zero': (r) => {
          const b = r.json();
          return b && b.message && b.message.includes('A quantidade deve ser maior que zero');
        },
      });
      errorAssertionsRate.add(okQty);
      simulatedErrorsCounter.add(1);
    });

    sleep(data.delaySeconds);

    group('5. Erros de Transição Inválida de Status', function () {
      const validPayload = {
        customerId: selectedOption.customer.id,
        vehicleId: selectedOption.vehicle.id,
        servicesUsed: [selectedOption.initialService.id],
      };

      const osRes = createServiceOrder(baseUrl, headers, validPayload);
      const serviceOrderId = osRes.body ? osRes.body.id : null;

      check(osRes.res, {
        'OS criada com sucesso no status Received': (r) => r.status === 200 || r.status === 201,
      });

      if (serviceOrderId) {
        const resInvalidApprove = http.post(`${baseUrl}/api/ServiceOrders/${serviceOrderId}/approve`, null, { headers });
        const okApprove = check(resInvalidApprove, {
          'Aprovar OS em status Received retorna 400': (r) => r.status === 400,
          'Mensagem de transição inválida para aprovação': (r) => {
            const b = r.json();
            return b && b.message && b.message.includes('Aguardando Aprovação');
          },
        });
        errorAssertionsRate.add(okApprove);
        simulatedErrorsCounter.add(1);

        const resInvalidFinishExec = http.post(`${baseUrl}/api/ServiceOrders/${serviceOrderId}/finish-execution`, null, { headers });
        const okFinishExec = check(resInvalidFinishExec, {
          'Finalizar execução de OS em status Received retorna 400': (r) => r.status === 400,
          'Mensagem de transição inválida para finalização': (r) => {
            const b = r.json();
            return b && b.message && b.message.includes('Em Execução');
          },
        });
        errorAssertionsRate.add(okFinishExec);
        simulatedErrorsCounter.add(1);

        const resInvalidDeliver = http.post(`${baseUrl}/api/ServiceOrders/${serviceOrderId}/deliver`, null, { headers });
        const okDeliver = check(resInvalidDeliver, {
          'Entregar OS em status Received retorna 400': (r) => r.status === 400,
          'Mensagem de transição inválida para entrega': (r) => {
            const b = r.json();
            return b && b.message && b.message.includes('Finalizada');
          },
        });
        errorAssertionsRate.add(okDeliver);
        simulatedErrorsCounter.add(1);

        const resInvalidRefuse = http.post(`${baseUrl}/api/ServiceOrders/${serviceOrderId}/refuse`, null, { headers });
        const okRefuse = check(resInvalidRefuse, {
          'Recusar OS em status Received retorna 400': (r) => r.status === 400,
          'Mensagem de transição inválida para recusa': (r) => {
            const b = r.json();
            return b && b.message && b.message.includes('Aguardando Aprovação');
          },
        });
        errorAssertionsRate.add(okRefuse);
        simulatedErrorsCounter.add(1);
      }
    });
  });

  const duration = (new Date() - startTime) / 1000;
  errorFlowDuration.add(duration);
}
