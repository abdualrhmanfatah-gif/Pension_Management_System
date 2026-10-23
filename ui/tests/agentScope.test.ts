import assert from "node:assert/strict";
import test from "node:test";

import { agentScopeKind, agentScopeLabel, agentScopePayload } from "../src/lib/agentScope.ts";

test("each agency scope has a distinct saved value", () => {
    assert.deepEqual(agentScopePayload("retiree", "7"), { agencyScope: 1, entitledID: "" });
    assert.deepEqual(agentScopePayload("allEntitled", "7"), { agencyScope: 2, entitledID: "" });
    assert.deepEqual(agentScopePayload("specificEntitled", "7"), { agencyScope: 3, entitledID: "7" });
});

test("explicit scope is restored and labelled on reopening", () => {
    assert.equal(agentScopeKind({ agencyScope: 1 }), "retiree");
    assert.equal(agentScopeLabel({ agencyScope: 2 }), "عن جميع المستحقين");
    assert.equal(agentScopeLabel({ agencyScope: 3, entitledID: 7, entitledPersonName: "أحمد" }), "عن المستحق: أحمد");
});

test("legacy agencies are not silently reclassified", () => {
    assert.equal(agentScopeKind({}), "legacy");
    assert.equal(agentScopeLabel({}), "وكالة عامة للملف");
    assert.equal(agentScopeKind({ entitledID: 7 }), "specificEntitled");
    assert.deepEqual(agentScopePayload("legacy", ""), { agencyScope: undefined, entitledID: "" });
});
