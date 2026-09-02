import http from 'k6/http';
import { check } from 'k6';

/**
 * Realiza autenticação com a API e retorna o token JWT e os headers de autorização
 * @param {string} baseUrl
 * @param {string} email
 * @param {string} password
 * @returns {{ token: string, headers: object }}
 */
export function login(baseUrl, email, password) {
  const url = `${baseUrl}/api/auth/login`;
  const payload = JSON.stringify({
    email: email,
    password: password,
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const res = http.post(url, payload, params);

  const loginSuccess = check(res, {
    'Login: status é 200': (r) => r.status === 200,
    'Login: token JWT retornado': (r) => {
      try {
        const body = JSON.parse(r.body);
        return body && body.token !== undefined && body.token.length > 0;
      } catch (e) {
        return false;
      }
    },
  });

  if (!loginSuccess) {
    throw new Error(`Falha no login com ${email}. Status: ${res.status}, Resposta: ${res.body}`);
  }

  const responseBody = JSON.parse(res.body);
  const token = responseBody.token;

  return {
    token: token,
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
  };
}
