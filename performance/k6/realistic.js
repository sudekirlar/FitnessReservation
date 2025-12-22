import http from "k6/http";
import { check, sleep, fail } from "k6";

export const options = {
    stages: [
        { duration: "1m", target: 20 },
        { duration: "5m", target: 20 },
        { duration: "1m", target: 50 },
        { duration: "5m", target: 50 },
        { duration: "1m", target: 0 },
    ],
    thresholds: {
        http_req_failed: ["rate<0.01"],
        http_req_duration: ["p(95)<800"],
    },
};

const BASE_URL = __ENV.BASE_URL || "http://127.0.0.1:7001";
const SPORT = __ENV.SPORT || "Yoga";
const SESSION_ID =
    __ENV.SESSION_ID || "11111111-1111-1111-1111-111111111111";
const PASSWORD = "P@ssw0rd-1";
const USER_COUNT = parseInt(__ENV.USER_COUNT || "60", 10);

function uid() {
    return `${Date.now()}_${Math.random().toString(16).slice(2)}`;
}

export function setup() {
    const users = [];
    for (let i = 0; i < USER_COUNT; i++) {
        const username = `u_${i}_${uid()}`;
        const reg = http.post(
            `${BASE_URL}/auth/register`,
            JSON.stringify({
                username,
                password: PASSWORD,
                membershipType: "Standard",
                membershipCode: null,
            }),
            { headers: { "Content-Type": "application/json" } }
        );

        if (!check(reg, { "register 201": (r) => r.status === 201 })) {
            fail(`register failed: status=${reg.status} body=${reg.body}`);
        }

        users.push({ username });
    }
    return { users };
}

// VU-scoped auth cookie
let frSid = null;
let printedOnce = false;

function loginAndGetCookie(username) {
    const login = http.post(
        `${BASE_URL}/auth/login`,
        JSON.stringify({ username, password: PASSWORD }),
        { headers: { "Content-Type": "application/json" } }
    );

    if (!check(login, { "login 200": (r) => r.status === 200 })) {
        fail(`login failed: status=${login.status} body=${login.body}`);
    }

    // First try parsed cookies
    const parsed =
        login.cookies && login.cookies["FR_SID"] && login.cookies["FR_SID"][0];

    if (parsed && parsed.value) return parsed.value;

    // Fallback: try Set-Cookie header
    const sc = login.headers["Set-Cookie"] || login.headers["set-cookie"];
    if (sc) {
        const m = /FR_SID=([^;]+)/.exec(sc);
        if (m) return m[1];
    }

    fail(
        `login returned 200 but FR_SID not found. headers=${JSON.stringify(
            login.headers
        )} cookies=${JSON.stringify(login.cookies)}`
    );
}

export default function (data) {
    const users = data.users;
    const me = users[(__VU - 1) % users.length];

    if (!frSid) {
        frSid = loginAndGetCookie(me.username);

        // sanity: /me should be 200 with cookie
        const meRes = http.get(`${BASE_URL}/me`, {
            headers: { Cookie: `FR_SID=${frSid}` },
        });

        if (!check(meRes, { "me 200 after login": (r) => r.status === 200 })) {
            fail(`/me failed after login: status=${meRes.status} body=${meRes.body}`);
        }
    }

    const fromUtc = new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString();
    const toUtc = new Date(Date.now() + 3 * 60 * 60 * 1000).toISOString();

    const sessionsUrl =
        `${BASE_URL}/sessions` +
        `?sport=${encodeURIComponent(SPORT)}` +
        `&from=${encodeURIComponent(fromUtc)}` +
        `&to=${encodeURIComponent(toUtc)}`;

    const sessions = http.get(sessionsUrl, {
        headers: { Cookie: `FR_SID=${frSid}` },
    });

    const sOk = check(sessions, { "sessions 200": (r) => r.status === 200 });
    if (!sOk && !printedOnce) {
        printedOnce = true;
        console.error(
            `SAMPLE sessions failure: status=${sessions.status} body=${(sessions.body || "").slice(
                0,
                200
            )}`
        );
    }

    if (Math.random() < 0.15) {
        const reserve = http.post(
            `${BASE_URL}/reservations`,
            JSON.stringify({ sessionId: SESSION_ID }),
            {
                headers: {
                    "Content-Type": "application/json",
                    Cookie: `FR_SID=${frSid}`,
                },
            }
        );

        const rOk = check(reserve, {
            "reserve 201/409": (r) => r.status === 201 || r.status === 409,
        });

        if (!rOk && !printedOnce) {
            printedOnce = true;
            console.error(
                `SAMPLE reserve failure: status=${reserve.status} body=${(reserve.body || "").slice(
                    0,
                    200
                )}`
            );
        }
    }

    sleep(Math.random() * 1.5);
}
