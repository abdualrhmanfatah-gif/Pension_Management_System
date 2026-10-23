import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { join } from "node:path";
import { test } from "node:test";

test("الرفع الجماعي يحافظ على المرفقات وحالة التحديث النهائي عند الإلغاء وإعادة المحاولة", () => {
    const result = spawnSync(process.execPath, [join(import.meta.dirname, "helpers", "bulkUploadDialogHarness.cjs")], {
        encoding: "utf8",
        timeout: 60000,
    });
    assert.equal(result.status, 0, `${result.error?.message ?? ""}\n${result.stdout}\n${result.stderr}`);
});
