import { sleep, group, check } from 'k6';
import { Trend, Counter } from 'k6/metrics';
import { CONFIG } from '../config/environment.js';
import { login } from '../helpers/auth.js';
import { setupSupportData } from '../helpers/setup-data.js';
import {
  createServiceOrder,
  startDiagnostics,
  addPartToServiceOrder,
  addStockToPart,
  addServiceToServiceOrder,
  finishAnalysis,
  refuseServiceOrder,
  getServiceOrderStatus,
} from '../helpers/api-client.js';

const refusedFlowDuration = new Trend('refused_flow_duration');
const refusedOrdersCounter = new Counter('refused_service_orders');

export const options = {
  scenarios: {
    service_order_refusal: {
      executor: 'ramping-vus',
      startVUs: 1,
      stages: [
        { duration: '30s', target: 2 },
        { duration: '1m', target: 3 },
        { duration: '30s', target: 0 },
      ],
      gracefulRampDown: '30s',
    },
  },
  thresholds: {
    ...CONFIG.THRESHOLDS,
    refused_service_orders: ['count>0'],
  },
};

/**
 * Executado 1 única vez antes dos VUs.
 * Faz login como administrador e cria as 3 opções de dados de suporte.
 */
export function setup() {
  console.log(`[SETUP] Conectando a ${CONFIG.BASE_URL} com ${CONFIG.ADMIN_EMAIL}...`);
  const auth = login(CONFIG.BASE_URL, CONFIG.ADMIN_EMAIL, CONFIG.ADMIN_PASSWORD);
  console.log('[SETUP] Login realizado com sucesso. Criando 3 opções de suporte...');

  const supportOptions = setupSupportData(CONFIG.BASE_URL, auth.headers);
  console.log(`[SETUP] 3 Opções de suporte criadas com sucesso.`);

  return {
    baseUrl: CONFIG.BASE_URL,
    headers: auth.headers,
    supportOptions: supportOptions,
    delaySeconds: CONFIG.DELAYS.REFUSAL_FLOW,
  };
}

/**
 * Função principal executada pelos VUs.
 * Simula o fluxo de recusa de uma Ordem de Serviço com delay de 10 segundos.
 */
export default function (data) {
  const startTime = new Date();
  const delay = data.delaySeconds;

  const selectedOption = data.supportOptions[Math.floor(Math.random() * data.supportOptions.length)];
  let serviceOrderId = null;

  group(`Fluxo Recusa OS (Opção ${selectedOption.optionIndex} - ${selectedOption.customer.name})`, function () {
    group('Etapa 1 - Criar Ordem de Serviço', function () {
      const payload = {
        customerId: selectedOption.customer.id,
        vehicleId: selectedOption.vehicle.id,
        servicesUsed: [selectedOption.initialService.id],
      };

      const res = createServiceOrder(data.baseUrl, data.headers, payload);
      serviceOrderId = res.body ? res.body.id : null;

      check(res.res, {
        'OS criada com sucesso': (r) => r.status === 201 || r.status === 200,
        'OS possui ID válido': () => serviceOrderId !== undefined && serviceOrderId !== null,
      });
    });

    sleep(delay);

    group('Etapa 2 - Iniciar Análise Técnica', function () {
      const res = startDiagnostics(data.baseUrl, data.headers, serviceOrderId);
      check(res.res, {
        'Status alterado para Em Análise (InDiagnostics)': (r) => r.status === 200,
      });
    });

    sleep(delay);

    group('Etapa 3 - Adicionar Peça, Estoque e Serviço Complementar', function () {
      const partRes = addPartToServiceOrder(
        data.baseUrl,
        data.headers,
        serviceOrderId,
        selectedOption.part.id,
        1
      );
      check(partRes.res, {
        'Peça adicionada à OS com sucesso': (r) => r.status === 200,
      });

      const stockRes = addStockToPart(
        data.baseUrl,
        data.headers,
        selectedOption.part.id,
        10
      );
      check(stockRes.res, {
        'Estoque da peça adicionado com sucesso': (r) => r.status === 200 || r.status === 204,
      });

      const serviceRes = addServiceToServiceOrder(
        data.baseUrl,
        data.headers,
        serviceOrderId,
        selectedOption.additionalService.id
      );
      check(serviceRes.res, {
        'Serviço complementar adicionado com sucesso': (r) => r.status === 200,
      });
    });

    sleep(delay);

    group('Etapa 4 - Finalizar Análise Técnica', function () {
      const res = finishAnalysis(data.baseUrl, data.headers, serviceOrderId);
      check(res.res, {
        'OS movida para Aguardando Aprovação (WaitingApproval)': (r) => r.status === 200,
      });
    });

    sleep(delay);

    group('Etapa 5 - Recusa da Ordem de Serviço pelo Cliente', function () {
      const res = refuseServiceOrder(data.baseUrl, data.headers, serviceOrderId);
      check(res.res, {
        'OS recusada com sucesso (Refused)': (r) => r.status === 200,
      });
    });

    sleep(delay);

    group('Etapa 6 - Validação Final do Status Recusado', function () {
      const res = getServiceOrderStatus(data.baseUrl, data.headers, serviceOrderId);
      const isRefused = check(res.res, {
        'Consulta de status retornou 200': (r) => r.status === 200,
        'Status final da OS é Refused': (r) => {
          try {
            const body = JSON.parse(r.body);
            return body && (body.status === 'Refused' || body.status === 6);
          } catch (e) {
            return false;
          }
        },
      });

      if (isRefused) {
        refusedOrdersCounter.add(1);
        refusedFlowDuration.add(new Date() - startTime);
      }
    });
  });
}
