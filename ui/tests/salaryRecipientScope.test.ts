import assert from "node:assert/strict";
import test from "node:test";

import { createSalaryRecipientSchema } from "../src/types/salary-recipients.ts";

test("a guardian recipient must identify the beneficiary represented", () => {
    const result = createSalaryRecipientSchema.safeParse({ recipientType: "وصي", guardianID: 7, startDate: "2026-10-16" });
    assert.equal(result.success, false);
    if (!result.success) assert.ok(result.error.issues.some(issue => issue.path[0] === "entitledID"));
});

test("a guardian may receive for a specified beneficiary", () => {
    assert.equal(createSalaryRecipientSchema.safeParse({ recipientType: "وصي", guardianID: 7, entitledID: 4, startDate: "2026-10-16" }).success, true);
});

test("an agent recipient derives its beneficiary on the server", () => {
    const result = createSalaryRecipientSchema.safeParse({ recipientType: "وكيل", agentID: 9, startDate: "2026-10-16" });
    assert.equal(result.success, true);
    if (result.success) assert.equal(result.data.entitledID, undefined);
});
