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
  approveServiceOrder,
  finishExecution,
  deliverServiceOrder,
  getServiceOrderStatus,
} from '../helpers/api-client.js';

const successfulFlowDuration = new Trend('successful_flow_duration');
const completedOrdersCounter = new Counter('completed_service_orders');

export const options = {
  scenarios: {
    service_order_success: {
      executor: 'ramping-vus',
      startVUs: 1,
      stages: [
        { duration: '20s', target: 2 },
        { duration: '1m', target: 3 },
        { duration: '20s', target: 0 },
      ],
      gracefulRampDown: '30s',
    },
  },
  thresholds: {
    ...CONFIG.THRESHOLDS,
    completed_service_orders: ['count>0'],
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
    delaySeconds: CONFIG.DELAYS.SUCCESS_FLOW,
  };
}

/**
 * Função principal executada pelos VUs.
 * Simula o fluxo completo de sucesso de uma Ordem de Serviço com delay de 5 segundos.
 */
export default function (data) {
  const startTime = new Date();
  const delay = data.delaySeconds;

  // Seleciona aleatoriamente 1 das 3 opções de dados de suporte criadas no setup
  const selectedOption = data.supportOptions[Math.floor(Math.random() * data.supportOptions.length)];
  let serviceOrderId = null;

  group(`Fluxo Sucesso OS (Opção ${selectedOption.optionIndex} - ${selectedOption.customer.name})`, function () {
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

    group('Etapa 5 - Aprovação da Ordem de Serviço', function () {
      const res = approveServiceOrder(data.baseUrl, data.headers, serviceOrderId);
      check(res.res, {
        'OS aprovada e movida para Em Execução (Executing)': (r) => r.status === 200,
      });

      const stockRes = addStockToPart(
        data.baseUrl,
        data.headers,
        selectedOption.part.id,
        10
      );
      check(stockRes.res, {
        'Garantia de estoque acionada para OS em Execução': (r) => r.status === 200 || r.status === 204,
      });
    });

    sleep(delay);

    group('Etapa 6 - Finalizar Execução', function () {
      const res = finishExecution(data.baseUrl, data.headers, serviceOrderId);
      check(res.res, {
        'Execução concluída e OS movida para Finalizada (Finished)': (r) => r.status === 200,
      });
    });

    sleep(delay);

    group('Etapa 7 - Entregar Veículo', function () {
      const res = deliverServiceOrder(data.baseUrl, data.headers, serviceOrderId);
      check(res.res, {
        'OS entregue ao cliente (Delivered)': (r) => r.status === 200,
      });
    });

    sleep(delay);

    group('Etapa 8 - Validação Final do Status', function () {
      const res = getServiceOrderStatus(data.baseUrl, data.headers, serviceOrderId);
      const isDelivered = check(res.res, {
        'Consulta de status retornou 200': (r) => r.status === 200,
        'Status final da OS é Delivered': (r) => {
          try {
            const body = JSON.parse(r.body);
            return body && (body.status === 'Delivered' || body.status === 5);
          } catch (e) {
            return false;
          }
        },
      });

      if (isDelivered) {
        completedOrdersCounter.add(1);
        successfulFlowDuration.add(new Date() - startTime);
      }
    });
  });
}
