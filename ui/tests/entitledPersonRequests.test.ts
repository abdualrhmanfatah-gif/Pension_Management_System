import assert from "node:assert/strict";
import { test } from "node:test";

import axios from "axios";

import {
    entitledPersonMultipart,
    reactivateEntitledPersonForm,
    recordEntitledPersonDeathForm,
    stopEntitledPersonForm,
} from "../src/lib/entitledPersonRequests.ts";

test("multipart options prevent Axios JSON defaults from discarding uploaded files", async () => {
    const document = new File(["proof"], "proof.pdf", { type: "application/pdf" });
    const forms = [stopEntitledPersonForm({ stopCode: "DAUGHTER_MARRIED", document }),
        reactivateEntitledPersonForm({ reactivationCode: "DIVORCED", document }),
        recordEntitledPersonDeathForm({ deathDate: "2026-10-24", deathCertificate: document })];
    const client = axios.create({ headers: { "Content-Type": "application/json" },
        adapter: async (config) => {
            assert.ok(config.data instanceof FormData);
            assert.equal(config.headers.getContentType(), "multipart/form-data");
            return { data: null, status: 200, statusText: "OK", headers: {}, config };
        } });
    for (const form of forms) await client.put("/test", form, entitledPersonMultipart);
});

test("stop posts the backend fields and preserves binary evidence", () => {
    const document = new File(["proof"], "proof.pdf", { type: "application/pdf" });
    const form = stopEntitledPersonForm({ stopCode: "DAUGHTER_MARRIED", notes: "سبب", document });
    assert.equal(form.get("stopCode"), "DAUGHTER_MARRIED");
    assert.equal(form.get("notes"), "سبب");
    assert.equal(form.get("document"), document);
    assert.equal(form.has("stopDate"), false);
});

test("reactivation uses the selected reason and both attachment field names", () => {
    const document = new File(["death"], "death.pdf");
    const civilServiceProof = new File(["civil"], "civil.pdf");
    const form = reactivateEntitledPersonForm({ reactivationCode: "HUSBAND_DECEASED", document, civilServiceProof });
    assert.equal(form.get("reactivationCode"), "HUSBAND_DECEASED");
    assert.equal(form.get("document"), document);
    assert.equal(form.get("civilServiceProof"), civilServiceProof);
    assert.equal(form.has("reactivateCode"), false);
});

test("death preserves its calendar date and the mandatory certificate", () => {
    const deathCertificate = new File(["proof"], "death.pdf");
    const form = recordEntitledPersonDeathForm({ deathDate: "2026-10-24", deathCertificate });
    assert.equal(form.get("deathDate"), "2026-10-24");
    assert.equal(form.get("deathCertificate"), deathCertificate);
});

test("optional empty attachments are omitted instead of becoming strings", () => {
    const form = reactivateEntitledPersonForm({ reactivationCode: "ADMIN_DECISION", document: null });
    assert.deepEqual([...form.keys()], ["reactivationCode"]);
});
