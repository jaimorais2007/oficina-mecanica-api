import http from 'k6/http';
import { check } from 'k6';

/**
 * Cliente HTTP encapsulando as chamadas da API Oficina Mecânica
 */

function handleResponse(res, stepName) {
  if (res.status >= 400) {
    console.error(`[ERRO] ${stepName} falhou com status ${res.status}: ${res.body}`);
  }
  return res;
}

export function createCustomer(baseUrl, headers, data) {
  const url = `${baseUrl}/api/Customer`;
  const res = http.post(url, JSON.stringify(data), { headers });
  
  check(res, {
    'POST /api/Customer: status 200/201': (r) => r.status === 200 || r.status === 201,
  });

  handleResponse(res, 'POST /api/Customer');
  return { res, body: res.json() };
}

export function createVehicle(baseUrl, headers, data) {
  const url = `${baseUrl}/api/Vehicle`;
  const res = http.post(url, JSON.stringify(data), { headers });

  check(res, {
    'POST /api/Vehicle: status 200/201': (r) => r.status === 200 || r.status === 201,
  });

  handleResponse(res, 'POST /api/Vehicle');
  return { res, body: res.json() };
}

export function createService(baseUrl, headers, data) {
  const url = `${baseUrl}/api/Service`;
  const res = http.post(url, JSON.stringify(data), { headers });

  check(res, {
    'POST /api/Service: status 200/201': (r) => r.status === 200 || r.status === 201,
  });

  handleResponse(res, 'POST /api/Service');
  return { res, body: res.json() };
}

export function createPart(baseUrl, headers, data) {
  const url = `${baseUrl}/api/Parts`;
  const res = http.post(url, JSON.stringify(data), { headers });

  check(res, {
    'POST /api/Parts: status 200/201': (r) => r.status === 200 || r.status === 201,
  });

  handleResponse(res, 'POST /api/Parts');
  return { res, body: res.json() };
}

export function addStockToPart(baseUrl, headers, partId, quantity = 10) {
  const url = `${baseUrl}/api/Parts/${partId}/add-stock`;
  const payload = JSON.stringify({ quantity });
  const res = http.post(url, payload, { headers });

  check(res, {
    'POST /api/Parts/{id}/add-stock (Adicionar Estoque): status 200/204': (r) => r.status === 200 || r.status === 204,
  });

  handleResponse(res, `POST /api/Parts/${partId}/add-stock`);
  return { res };
}

export function createServiceOrder(baseUrl, headers, data) {
  const url = `${baseUrl}/api/ServiceOrders`;
  const res = http.post(url, JSON.stringify(data), { headers });

  check(res, {
    'POST /api/ServiceOrders (Criar OS): status 200/201': (r) => r.status === 200 || r.status === 201,
    'Criar OS: ID retornado': (r) => {
      const b = r.json();
      return b && b.id !== undefined;
    },
  });

  handleResponse(res, 'POST /api/ServiceOrders');
  return { res, body: res.json() };
}

export function startDiagnostics(baseUrl, headers, serviceOrderId) {
  const url = `${baseUrl}/api/ServiceOrders/${serviceOrderId}/start-analysis`;
  const res = http.post(url, null, { headers });

  check(res, {
    'POST /start-analysis: status 200': (r) => r.status === 200,
  });

  handleResponse(res, `POST /api/ServiceOrders/${serviceOrderId}/start-analysis`);
  return { res, body: res.json() };
}

export function addPartToServiceOrder(baseUrl, headers, serviceOrderId, partId, quantity = 1) {
  const url = `${baseUrl}/api/ServiceOrders/${serviceOrderId}/parts`;
  const payload = JSON.stringify({ partId, quantity });
  const res = http.post(url, payload, { headers });

  check(res, {
    'POST /parts (Adicionar Peça): status 200': (r) => r.status === 200,
  });

  handleResponse(res, `POST /api/ServiceOrders/${serviceOrderId}/parts`);
  return { res, body: res.json() };
}

export function addServiceToServiceOrder(baseUrl, headers, serviceOrderId, serviceId) {
  const url = `${baseUrl}/api/ServiceOrders/${serviceOrderId}/services`;
  const payload = JSON.stringify({ serviceId });
  const res = http.post(url, payload, { headers });

  check(res, {
    'POST /services (Adicionar Serviço): status 200': (r) => r.status === 200,
  });

  handleResponse(res, `POST /api/ServiceOrders/${serviceOrderId}/services`);
  return { res, body: res.json() };
}

export function finishAnalysis(baseUrl, headers, serviceOrderId) {
  const url = `${baseUrl}/api/ServiceOrders/${serviceOrderId}/finish-analysis`;
  const res = http.post(url, null, { headers });

  check(res, {
    'POST /finish-analysis: status 200': (r) => r.status === 200,
  });

  handleResponse(res, `POST /api/ServiceOrders/${serviceOrderId}/finish-analysis`);
  return { res, body: res.json() };
}

export function approveServiceOrder(baseUrl, headers, serviceOrderId) {
  const url = `${baseUrl}/api/ServiceOrders/${serviceOrderId}/approve`;
  const res = http.post(url, null, { headers });

  check(res, {
    'POST /approve (Aprovar OS): status 200': (r) => r.status === 200,
  });

  handleResponse(res, `POST /api/ServiceOrders/${serviceOrderId}/approve`);
  return { res, body: res.json() };
}

export function finishExecution(baseUrl, headers, serviceOrderId) {
  const url = `${baseUrl}/api/ServiceOrders/${serviceOrderId}/finish-execution`;
  const res = http.post(url, null, { headers });

  check(res, {
    'POST /finish-execution (Finalizar Execução): status 200': (r) => r.status === 200,
  });

  handleResponse(res, `POST /api/ServiceOrders/${serviceOrderId}/finish-execution`);
  return { res, body: res.json() };
}

export function deliverServiceOrder(baseUrl, headers, serviceOrderId) {
  const url = `${baseUrl}/api/ServiceOrders/${serviceOrderId}/deliver`;
  const res = http.post(url, null, { headers });

  check(res, {
    'POST /deliver (Entregar OS): status 200': (r) => r.status === 200,
  });

  handleResponse(res, `POST /api/ServiceOrders/${serviceOrderId}/deliver`);
  return { res, body: res.json() };
}

export function refuseServiceOrder(baseUrl, headers, serviceOrderId) {
  const url = `${baseUrl}/api/ServiceOrders/${serviceOrderId}/refuse`;
  const res = http.post(url, null, { headers });

  check(res, {
    'POST /refuse (Recusar OS): status 200': (r) => r.status === 200,
  });

  handleResponse(res, `POST /api/ServiceOrders/${serviceOrderId}/refuse`);
  return { res, body: res.json() };
}

export function getServiceOrderStatus(baseUrl, headers, serviceOrderId) {
  const url = `${baseUrl}/api/ServiceOrders/${serviceOrderId}/status`;
  const res = http.get(url, { headers });

  check(res, {
    'GET /status: status 200': (r) => r.status === 200,
  });

  handleResponse(res, `GET /api/ServiceOrders/${serviceOrderId}/status`);
  return { res, body: res.json() };
}

export function getServiceOrderById(baseUrl, headers, serviceOrderId) {
  const url = `${baseUrl}/api/ServiceOrders/${serviceOrderId}`;
  const res = http.get(url, { headers });

  check(res, {
    'GET /ServiceOrders/{id}: status 200': (r) => r.status === 200,
  });

  handleResponse(res, `GET /api/ServiceOrders/${serviceOrderId}`);
  return { res, body: res.json() };
}
