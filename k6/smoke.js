import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  vus: 1,
  iterations: 1,
};

const BASE_URL = "http://127.0.0.1:7001";

export default function () {
  const res = http.get(`${BASE_URL}/health`);

  check(res, {
    "health status is 200": (r) => r.status === 200,
  });

  sleep(1);
}
