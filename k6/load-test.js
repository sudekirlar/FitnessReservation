import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/2.4.0/dist/bundle.js";

export const options = {
    stages: [
        { duration: '10s', target: 10 },
        { duration: '30s', target: 10 },
        { duration: '10s', target: 0 },
    ],
    thresholds: {
        'http_req_failed': ['rate<0.60'],
        'http_req_duration': ['p(95)<500'],
    },
};

http.setResponseCallback(http.expectedStatuses({ min: 200, max: 399 }, 409));

const BASE_URL = 'http://localhost:7001';

export default function () {
    const username = `user_${__VU}_${Date.now()}`;
    const password = "P@ssw0rd-1";
    const params = { headers: { 'Content-Type': 'application/json' } };

    http.post(`${BASE_URL}/auth/register`, JSON.stringify({ username, password, membershipType: "Standard" }), params);
    http.post(`${BASE_URL}/auth/login`, JSON.stringify({ username, password }), params);

    http.get(`${BASE_URL}/sessions?sport=Yoga&from=${new Date().toISOString()}&to=${new Date(Date.now() + 86400000).toISOString()}`);

    const sessionId = "33333333-3333-3333-3333-333333333333";
    const res = http.post(`${BASE_URL}/reservations`, JSON.stringify({ sessionId }), params);

    check(res, {
        "is success or capacity full": (r) => r.status === 201 || r.status === 409,
    });

    sleep(1);
}

export function handleSummary(data) {
    return {
        "k6-results/summary.html": htmlReport(data),
    };
}