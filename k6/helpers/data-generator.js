/**
 * Utilitários para geração de dados dinâmicos e válidos para a API
 */

export function generateCPF() {
  const randomDigits = [];
  for (let i = 0; i < 9; i++) {
    randomDigits.push(Math.floor(Math.random() * 10));
  }

  if (randomDigits.every((d) => d === randomDigits[0])) {
    randomDigits[0] = (randomDigits[0] + 1) % 10;
  }

  let sum = 0;
  for (let i = 0; i < 9; i++) {
    sum += randomDigits[i] * (10 - i);
  }
  let remainder = sum % 11;
  const digit1 = remainder < 2 ? 0 : 11 - remainder;
  randomDigits.push(digit1);

  sum = 0;
  for (let i = 0; i < 10; i++) {
    sum += randomDigits[i] * (11 - i);
  }
  remainder = sum % 11;
  const digit2 = remainder < 2 ? 0 : 11 - remainder;
  randomDigits.push(digit2);

  return randomDigits.join('');
}

export function generatePlate() {
  const letters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
  const digits = '0123456789';

  let plate = '';
  for (let i = 0; i < 3; i++) {
    plate += letters.charAt(Math.floor(Math.random() * letters.length));
  }
  
  plate += digits.charAt(Math.floor(Math.random() * digits.length));
  plate += letters.charAt(Math.floor(Math.random() * letters.length));

  for (let i = 0; i < 2; i++) {
    plate += digits.charAt(Math.floor(Math.random() * digits.length));
  }

  return plate;
}

export function generateEmail(prefix = 'user') {
  const uniqueId = `${Date.now()}_${Math.floor(Math.random() * 100000)}`;
  return `${prefix}_${uniqueId}@testload.com`;
}

export function generatePartCode(prefix = 'PART') {
  const uniqueId = `${Date.now().toString(36).toUpperCase()}_${Math.floor(Math.random() * 1000)}`;
  return `${prefix}-${uniqueId}`;
}
