import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  vus: 1,
  iterations: 1,
  thresholds: {
    http_req_failed: ["rate<0.01"],         // < %1 gerçek hata
    http_req_duration: ["p(95)<300"],       // p95 < 300ms
  },
};

http.setResponseCallback(http.expectedStatuses({ min: 200, max: 399 }, 409));

const BASE_URL = "http://127.0.0.1:7001";

export default function () {
  const username = `u_${__VU}_${Date.now()}`;
  const password = "P@ssw0rd-1";

  const regRes = http.post(
    `${BASE_URL}/auth/register`,
    JSON.stringify({
      username,
      password,
      membershipType: "Standard",
      membershipCode: null,
    }),
    { headers: { "Content-Type": "application/json" } }
  );

  check(regRes, { "register 201": (r) => r.status === 201 });

  const loginRes = http.post(
    `${BASE_URL}/auth/login`,
    JSON.stringify({ username, password }),
    { headers: { "Content-Type": "application/json" } }
  );

  check(loginRes, { "login 200": (r) => r.status === 200 });

  const fromUtc = new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString();
  const toUtc = new Date(Date.now() + 3 * 60 * 60 * 1000).toISOString();

  const sessionsRes = http.get(
    `${BASE_URL}/sessions?sport=Yoga&from=${encodeURIComponent(fromUtc)}&to=${encodeURIComponent(toUtc)}`
  );

  check(sessionsRes, {
    "sessions 200": (r) => r.status === 200,
    "sessions non-empty array": (r) => {
      try {
        const arr = r.json();
        return Array.isArray(arr) && arr.length > 0;
      } catch {
        return false;
      }
    },
  });

  const sessionId = "11111111-1111-1111-1111-111111111111";

  const reserveRes = http.post(
    `${BASE_URL}/reservations`,
    JSON.stringify({ sessionId }),
    { headers: { "Content-Type": "application/json" } }
  );

  check(reserveRes, {
    "reserve 201 or 409": (r) => r.status === 201 || r.status === 409,
  });

  sleep(1);
}
