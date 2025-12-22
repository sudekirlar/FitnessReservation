import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
    stages: [
        { duration: "1m", target: 10 },
        { duration: "3m", target: 10 },
        { duration: "1m", target: 10 },
        { duration: "1m", target: 0 },
    ],
    thresholds: {
        http_req_failed: ["rate<0.01"],
        http_req_duration: ["p(95)<800"],
    },
};

const BASE_URL = __ENV.BASE_URL || "http://127.0.0.1:7001";
const SPORT = "Yoga";

function uid() {
    return `${Date.now()}_${Math.random().toString(16).slice(2)}`;
}

export default function () {
    // health
    const h = http.get(`${BASE_URL}/health`);
    check(h, { "health 200": (r) => r.status === 200 });

    // register (unique user)
    const username = `u_${uid()}`;
    const password = "P@ssw0rd-1";

    const reg = http.post(
        `${BASE_URL}/auth/register`,
        JSON.stringify({
            username,
            password,
            membershipType: "Standard",
            membershipCode: null,
        }),
        { headers: { "Content-Type": "application/json" } }
    );
    check(reg, { "register 201/409": (r) => r.status === 201 || r.status === 409 });

    // login (cookie session)
    const login = http.post(
        `${BASE_URL}/auth/login`,
        JSON.stringify({ username, password }),
        { headers: { "Content-Type": "application/json" } }
    );
    check(login, { "login 200": (r) => r.status === 200 });

    // sessions list (auth required)
    const fromUtc = new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString();
    const toUtc = new Date(Date.now() + 3 * 60 * 60 * 1000).toISOString();

    const url =
        `${BASE_URL}/sessions` +
        `?sport=${encodeURIComponent(SPORT)}` +
        `&from=${encodeURIComponent(fromUtc)}` +
        `&to=${encodeURIComponent(toUtc)}`;

    const sessions = http.get(url);

    check(sessions, {
        "sessions 200": (r) => r.status === 200,
        "sessions json array": (r) => {
            if (r.status !== 200) return false;
            try {
                const data = r.json();
                return Array.isArray(data);
            } catch {
                return false;
            }
        },
    });

    sleep(1);
}
