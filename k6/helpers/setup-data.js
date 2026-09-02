import { generateCPF, generatePlate, generateEmail, generatePartCode } from './data-generator.js';
import {
  createCustomer,
  createVehicle,
  createService,
  createPart,
} from './api-client.js';

/**
 * Cria previamente 3 conjuntos completos de dados de suporte (Cliente, Veículo, Serviços e Peças).
 * Executado durante a fase de setup() do teste k6.
 *
 * @param {string} baseUrl
 * @param {object} headers
 * @returns {Array<object>} Array contendo as 3 opções de suporte
 */
export function setupSupportData(baseUrl, headers) {
  const supportOptions = [];

  const definitions = [
    {
      customerName: 'Cliente Carga 1 - Silva',
      vehicle: { brand: 'Toyota', model: 'Corolla', year: 2022 },
      initialService: {
        name: 'Troca de Óleo e Filtros',
        description: 'Substituição completa de óleo sintético e filtro do motor',
        defaultPrice: 250.0,
      },
      additionalService: {
        name: 'Alinhamento e Balanceamento 3D',
        description: 'Alinhamento computadorizado e balanceamento das 4 rodas',
        defaultPrice: 180.0,
      },
      part: {
        name: 'Filtro de Óleo Sintético',
        codePrefix: 'FLT-OIL',
        initialQuantity: 1000,
        price: 45.0,
      },
    },
    {
      customerName: 'Cliente Carga 2 - Santos',
      vehicle: { brand: 'Honda', model: 'Civic', year: 2021 },
      initialService: {
        name: 'Revisão do Sistema de Freios',
        description: 'Inspeção e manutenção das pastilhas e discos de freio',
        defaultPrice: 320.0,
      },
      additionalService: {
        name: 'Troca de Fluido de Freio DOT4',
        description: 'Sangria completa e reposição de fluido de freio',
        defaultPrice: 150.0,
      },
      part: {
        name: 'Jogo de Pastilhas Dianteiras',
        codePrefix: 'PST-FRN',
        initialQuantity: 1000,
        price: 180.0,
      },
    },
    {
      customerName: 'Cliente Carga 3 - Oliveira',
      vehicle: { brand: 'Volkswagen', model: 'Golf', year: 2020 },
      initialService: {
        name: 'Diagnóstico Elétrico e Bateria',
        description: 'Teste do sistema de carga/alternador e instalação',
        defaultPrice: 120.0,
      },
      additionalService: {
        name: 'Limpeza de Bicos Injetores',
        description: 'Equalização e limpeza de injetores por ultrassom',
        defaultPrice: 220.0,
      },
      part: {
        name: 'Bateria Automotiva 60Ah',
        codePrefix: 'BAT-60A',
        initialQuantity: 1000,
        price: 450.0,
      },
    },
  ];

  for (let i = 0; i < definitions.length; i++) {
    const def = definitions[i];

    const customerPayload = {
      name: def.customerName,
      personType: 1, 
      document: generateCPF(),
      dateOfBirth: '1990-06-15T00:00:00.000Z',
      email: generateEmail(`cliente_${i + 1}`),
    };
    const customerRes = createCustomer(baseUrl, headers, customerPayload);
    const customer = customerRes.body;

    const vehiclePayload = {
      customerId: customer.id,
      plate: generatePlate(),
      brand: def.vehicle.brand,
      model: def.vehicle.model,
      year: def.vehicle.year,
    };
    const vehicleRes = createVehicle(baseUrl, headers, vehiclePayload);
    const vehicle = vehicleRes.body;

    const initialServiceRes = createService(baseUrl, headers, def.initialService);
    const initialService = initialServiceRes.body;

    const additionalServiceRes = createService(baseUrl, headers, def.additionalService);
    const additionalService = additionalServiceRes.body;

    const partPayload = {
      name: def.part.name,
      code: generatePartCode(def.part.codePrefix),
      initialQuantity: def.part.initialQuantity,
      price: def.part.price,
    };
    const partRes = createPart(baseUrl, headers, partPayload);
    const part = partRes.body;

    supportOptions.push({
      optionIndex: i + 1,
      customer: customer,
      vehicle: vehicle,
      initialService: initialService,
      additionalService: additionalService,
      part: part,
    });
  }

  return supportOptions;
}
