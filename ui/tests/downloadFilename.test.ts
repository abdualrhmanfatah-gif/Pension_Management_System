import assert from "node:assert/strict";
import { test } from "node:test";

import { getDownloadFilename } from "../src/lib/downloadFilename.ts";

test("extracts the ASP.NET export filename without including the next parameter", () => {
    const header = "attachment; filename=ActiveRetirees_20261016.xlsx; filename*=UTF-8''ActiveRetirees_20261016.xlsx";
    assert.equal(getDownloadFilename(header, "report.xlsx"), "ActiveRetirees_20261016.xlsx");
});

test("prefers the UTF-8 filename and decodes Arabic with an optional language", () => {
    const name = "المتقاعدين النشطين.xlsx";
    const header = `attachment; filename="retirees.xlsx"; filename*=UTF-8'ar'${encodeURIComponent(name)}`;
    assert.equal(getDownloadFilename(header, "report.xlsx"), name);
});

test("handles quoted names containing semicolons and keeps subsequent parameters out", () => {
    assert.equal(getDownloadFilename('attachment; filename="report; final.pdf"; size=123', "report.pdf"), "report; final.pdf");
});

test("falls back to the ordinary filename when UTF-8 encoding is invalid", () => {
    assert.equal(getDownloadFilename("attachment; filename=retirees.xlsx; filename*=UTF-8''%E0%A4", "report.xlsx"), "retirees.xlsx");
});

test("uses the default filename for missing or empty filename parameters", () => {
    for (const header of [undefined, null, "attachment", 'attachment; filename=""']) {
        assert.equal(getDownloadFilename(header, "report.xlsx"), "report.xlsx");
    }
});
