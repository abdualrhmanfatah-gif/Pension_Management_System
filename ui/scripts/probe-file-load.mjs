/**
 * [DEBUG-HARNESS] حلقة تشخيص لتحميل صور الوثائق في المتصفح الحقيقي.
 *
 * تقود Edge بلا واجهة عبر CDP (بلا اعتماديات — WebSocket مدمج في Node).
 * تثبت أي مسار تحميل يفشل على أصل الصفحة http://localhost:5173:
 *   - "crossOrigin" : new Image() مع crossOrigin="anonymous" على رابط API المطلق
 *                     (الاستراتيجية القديمة في DocumentProcessPage.tsx)
 *   - "sameOrigin"  : fetch بمسار نسبي عبر Vite proxy
 *                     (الاستراتيجية الموحّدة في useDocumentBlob)
 *
 * البيانات الحسّاسة من البيئة، ولا تُطبع أبدًا:
 *   PROBE_USER, PROBE_PASSWORD
 *
 * التشغيل: node scripts/probe-file-load.mjs "<مسار ملف نسبي>"
 * الخروج: 0 = كلا المسارين يعملان، 1 = فشل أحدهما.
 */

import { setTimeout as delay } from "node:timers/promises";

const CDP_PORT = process.env.PROBE_CDP_PORT || "9222";
const ORIGIN = process.env.PROBE_ORIGIN || "http://localhost:5173";
const USER = process.env.PROBE_USER;
const PASSWORD = process.env.PROBE_PASSWORD;
const FILE_PATH = process.argv[2];

if (!FILE_PATH) {
    console.error("usage: node scripts/probe-file-load.mjs <relative-file-path>");
    process.exit(2);
}
if (!USER || !PASSWORD) {
    console.error("PROBE_USER / PROBE_PASSWORD must be set in the environment");
    process.exit(2);
}

const API_ORIGIN = new URL(ORIGIN).port === "5173" ? "http://localhost:5001" : ORIGIN;

// ── CDP minimal client ──────────────────────────────────────────────
let nextId = 1;

async function connect(url) {
    const res = await fetch(`http://127.0.0.1:${CDP_PORT}/json/new?${encodeURIComponent(url)}`, {
        method: "PUT",
    });
    if (!res.ok) throw new Error(`cannot open target: ${res.status}`);
    const target = await res.json();

    const ws = new WebSocket(target.webSocketDebuggerUrl);
    const pending = new Map();
    const events = [];

    await new Promise((resolve, reject) => {
        ws.addEventListener("open", resolve, { once: true });
        ws.addEventListener("error", () => reject(new Error("ws error")), { once: true });
    });

    ws.addEventListener("message", (event) => {
        const msg = JSON.parse(event.data);
        if (msg.id && pending.has(msg.id)) {
            const { resolve, reject } = pending.get(msg.id);
            pending.delete(msg.id);
            msg.error ? reject(new Error(JSON.stringify(msg.error))) : resolve(msg.result);
        } else if (msg.method) {
            events.push(msg);
        }
    });

    const send = (method, params = {}) =>
        new Promise((resolve, reject) => {
            const id = nextId++;
            pending.set(id, { resolve, reject });
            ws.send(JSON.stringify({ id, method, params }));
        });

    return { send, events, close: () => ws.close() };
}

async function waitForLoad(events, timeoutMs = 20000) {
    const deadline = Date.now() + timeoutMs;
    while (Date.now() < deadline) {
        if (events.some((e) => e.method === "Page.loadEventFired")) return true;
        await delay(100);
    }
    return false;
}

// ── Probe evaluated inside the page (origin = 5173) ────────────────
function probeScript() {
    const apiOrigin = JSON.stringify(API_ORIGIN);
    const path = JSON.stringify(FILE_PATH);
    return `(async () => {
        const relPath = ${path};
        const absUrl = ${apiOrigin} + '/api/files' + relPath;
        const relUrl = '/api/files' + relPath;

        // login through the same-origin proxy so the jwt cookie exists
        let login = 'skipped';
        try {
            const lr = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username: ${JSON.stringify(USER)}, password: ${JSON.stringify(PASSWORD)} }),
            });
            login = String(lr.status);
        } catch (e) { login = 'ERR ' + e; }

        // strategy A: same-origin relative fetch (useDocumentBlob)
        let sameOrigin;
        try {
            const r = await fetch(relUrl, { credentials: 'include' });
            sameOrigin = { status: r.status, type: r.headers.get('content-type'),
                           acao: r.headers.get('access-control-allow-origin') };
        } catch (e) { sameOrigin = { error: String(e) }; }

        // strategy B: new Image() + crossOrigin='anonymous' on the absolute API URL
        const crossOrigin = await new Promise((resolve) => {
            const img = new Image();
            img.crossOrigin = 'anonymous';
            img.onload = () => resolve({ ok: true, w: img.naturalWidth, h: img.naturalHeight });
            img.onerror = () => resolve({ ok: false });
            img.src = absUrl;
            setTimeout(() => resolve({ ok: false, timeout: true }), 10000);
        });

        // strategy C: plain <img>, no crossOrigin
        const plain = await new Promise((resolve) => {
            const img = new Image();
            img.onload = () => resolve({ ok: true, w: img.naturalWidth, h: img.naturalHeight });
            img.onerror = () => resolve({ ok: false });
            img.src = absUrl;
            setTimeout(() => resolve({ ok: false, timeout: true }), 10000);
        });

        // strategy D: crossOrigin = 'use-credentials' (sends the cookie in CORS mode)
        const useCreds = await new Promise((resolve) => {
            const img = new Image();
            img.crossOrigin = 'use-credentials';
            img.onload = () => resolve({ ok: true, w: img.naturalWidth, h: img.naturalHeight });
            img.onerror = () => resolve({ ok: false });
            img.src = absUrl;
            setTimeout(() => resolve({ ok: false, timeout: true }), 10000);
        });

        // pipeline: the exact chain the processing dialog needs
        // same-origin fetch -> object URL -> decode -> canvas -> toBlob
        const pipeline = await (async () => {
            try {
                const r = await fetch(relUrl, { credentials: 'include' });
                if (!r.ok) return { ok: false, step: 'fetch', status: r.status };
                const objUrl = URL.createObjectURL(await r.blob());
                const img = await new Promise((resolve, reject) => {
                    const i = new Image();
                    i.onload = () => resolve(i);
                    i.onerror = () => reject(new Error('decode failed'));
                    i.src = objUrl;
                    setTimeout(() => reject(new Error('decode timeout')), 10000);
                });
                const c = document.createElement('canvas');
                c.width = img.naturalWidth;
                c.height = img.naturalHeight;
                const ctx = c.getContext('2d');
                if (!ctx) return { ok: false, step: 'getContext' };
                ctx.drawImage(img, 0, 0);
                // canvas tainted by a cross-origin image throws SecurityError here
                const exported = await new Promise((resolve) => {
                    c.toBlob((b) => resolve(b ? b.size : 0), 'image/jpeg');
                    setTimeout(() => resolve(-1), 10000);
                });
                URL.revokeObjectURL(objUrl);
                if (exported <= 0) return { ok: false, step: 'toBlob', exported };
                return { ok: true, w: img.naturalWidth, h: img.naturalHeight, exportedBytes: exported };
            } catch (e) {
                return { ok: false, step: 'throw', error: String(e) };
            }
        })();

        return JSON.stringify({ login, sameOrigin, crossOrigin, plain, useCreds, pipeline });
    })()`;
}

const result = await (async () => {
    const client = await connect(`${ORIGIN}/__probe__`);
    try {
        await client.send("Page.enable");
        await client.send("Runtime.enable");
        if (!(await waitForLoad(client.events))) {
            return { error: "page load timeout" };
        }
        const evaluated = await client.send("Runtime.evaluate", {
            expression: probeScript(),
            awaitPromise: true,
            returnByValue: true,
        });
        if (evaluated.exceptionDetails) {
            return { error: JSON.stringify(evaluated.exceptionDetails).slice(0, 400) };
        }
        return JSON.parse(evaluated.result.value);
    } finally {
        client.close();
    }
})();

console.log(JSON.stringify(result, null, 2));

const sameOriginOk = result.sameOrigin?.status === 200;
const crossOriginOk = result.crossOrigin?.ok === true;
const plainOk = result.plain?.ok === true;

console.log("");
console.log(`sameOrigin(relative) : ${sameOriginOk ? "OK" : "FAIL"}  ${result.sameOrigin?.status ?? result.sameOrigin?.error ?? ""}`);
console.log(`crossOrigin(anon)    : ${crossOriginOk ? "OK" : "FAIL"}  ${JSON.stringify(result.crossOrigin ?? result.error ?? "")}`);
console.log(`crossOrigin(creds)   : ${result.useCreds?.ok ? "OK" : "FAIL"}  ${JSON.stringify(result.useCreds ?? "")}`);
console.log(`plain img            : ${plainOk ? "OK" : "FAIL"}`);
console.log(`pipeline(blob->canvas->toBlob) : ${result.pipeline?.ok ? "OK" : "FAIL"}  ${JSON.stringify(result.pipeline ?? "")}`);

if (!sameOriginOk || !crossOriginOk || !result.pipeline?.ok) {
    console.log("");
    console.log("VERDICT: reproduces — crossOrigin path is broken while the same-origin blob path works.");
    process.exit(1);
}
console.log("");
console.log("VERDICT: crossOrigin path loads and the blob pipeline works — cannot reproduce.");
process.exit(0);
