import assert from "node:assert/strict";
import { test } from "node:test";

import { BULK_UPLOAD_RETENTION_MS, type BulkUploadDraftFile, bulkUploadDraftKey, prepareBulkUploadDraft } from "../src/lib/bulkUploadDraft.ts";

function document(status: BulkUploadDraftFile["status"]): BulkUploadDraftFile {
    const file = new File(["وثيقة"], "document.pdf", { type: "application/pdf" });
    return {
        id: "item", file, fileName: file.name, fileSize: file.size, documentTypeID: 1,
        ownerEntity: "agent_5", notes: "ملاحظة", status,
        needsReview: true, reviewConfirmed: false, qualityFlags: ["blur"],
    };
}

test("interrupted requests remain retryable and file contents survive a storage clone", async () => {
    const draft = prepareBulkUploadDraft(bulkUploadDraftKey(3, 7), [document("uploading")], true, 1000);
    assert.ok(draft);
    assert.equal(draft.expiresAt, 1000 + BULK_UPLOAD_RETENTION_MS);
    const cloned = structuredClone(draft);
    assert.equal(cloned.files[0].status, "pending");
    assert.equal(await cloned.files[0].file.text(), "وثيقة");
    assert.equal(cloned.files[0].ownerEntity, "agent_5");
    assert.equal(cloned.files[0].reviewConfirmed, false);
});

test("success IDs are retained until finalization while finished batches are removed", () => {
    const uploaded = { ...document("success"), documentId: 42 };
    assert.equal(prepareBulkUploadDraft("3:7", [uploaded], true)?.files[0].documentId, 42);
    assert.equal(prepareBulkUploadDraft("3:7", [uploaded], false), null);
    assert.equal(prepareBulkUploadDraft("3:7", [], false), null);
    const partial = prepareBulkUploadDraft("3:7", [uploaded, document("error")], false);
    assert.equal(partial?.files.length, 2);
    assert.equal(partial?.files[0].documentId, 42);
});
