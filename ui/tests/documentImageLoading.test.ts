import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, it } from "node:test";

/**
 * حارس انحدار: تحميل صور الوثائق يجب ألا يعود إلى وضع CORS عبر النطاقات.
 *
 * سبب الفشل: رابط FilesController مطلق (VITE_API_URL)، ووضع CORS يُسقط
 * طلب الصورة في المتصفح — قِيس على http://localhost:5173:
 *   new Image() + crossOrigin="anonymous"  -> يفشل
 *   new Image() + crossOrigin="use-credentials" -> يفشل
 *   fetch بمسار نسبي (blob:) -> 200 ثم toBlob بلا SecurityError
 *
 * لا يوجد DOM harness في المشروع، لذا الحارس يفحص المصدر: المكوّنات
 * المعنية لا تعيّن crossOrigin، والنمط الموحّد جلب نسبي.
 */
const SRC = join(import.meta.dirname, "..", "src");

function read(relativePath: string): string {
    return readFileSync(join(SRC, relativePath), "utf8");
}

/** يزيل التعليقات حتى لا يُحسب النص التوضيحي تعيينًا */
function stripComments(source: string): string {
    return source
        .replace(/\/\*[\s\S]*?\*\//g, "")
        .replace(/(^|[^:])\/\/.*$/gm, "$1");
}

const IMAGE_CONSUMERS = [
    "pages/retirement-files/DocumentProcessPage.tsx",
    "components/domain/retirement/DocumentProcessDialog.tsx",
    "components/ui/CornerEditor.tsx",
    "components/ui/ImageCropperDialog.tsx",
];

describe("تحميل صور الوثائق", () => {
    for (const file of IMAGE_CONSUMERS) {
        it(`${file}: لا يعيّن crossOrigin`, () => {
            const source = stripComments(read(file));
            const assignments = source.match(/\.\s*crossOrigin\s*=/g) ?? [];
            assert.deepEqual(
                assignments,
                [],
                `${file} يعيّن crossOrigin — أي طلب صورة بوضع CORS يفشل في المتصفح`,
            );
        });
    }

    it("DocumentProcessPage يجلب الصورة عبر useDocumentBlob", () => {
        const source = read("pages/retirement-files/DocumentProcessPage.tsx");
        assert.ok(source.includes("useDocumentBlob"), "يجب استخدام النمط الموحّد");
        assert.ok(
            !/new Image\(\)[\s\S]{0,200}img\.src\s*=\s*imageUrl/.test(source),
            "لا يُحمَّل Image من رابط API المطلق",
        );
    });

    it("DocumentProcessDialog يجلب الصورة عبر useDocumentBlob", () => {
        const source = read("components/domain/retirement/DocumentProcessDialog.tsx");
        assert.ok(source.includes("useDocumentBlob"), "يجب استخدام النمط الموحّد");
    });

    it("useDocumentBlob يجلب بمسار نسبي و baseURL فارغ", () => {
        const source = read("hooks/useDocumentBlob.ts");
        assert.ok(
            source.includes("getFileRelativeUrl"),
            "يجب استخدام الرابط النسبي لا المطلق",
        );
        assert.ok(
            /baseURL:\s*""|baseURL:\s*''/.test(source),
            "يجب تجاوز baseURL المطلق وإلا ضُمّ /api مرتين",
        );
    });

    it("getFileRelativeUrl يجرّد الأصل فيبقى same-origin", () => {
        const source = read("api/axios.ts");
        assert.ok(
            source.includes("getFileRelativeUrl"),
            "الدالة مطلوبة في axios.ts",
        );
        assert.ok(
            /replace\(\/\^https\?:\\\/\\\/\[\^\/\]\+\/i,\s*""\)/.test(source),
            "يجب تجريد أصل VITE_API_URL فيبقى المسار نسبياً",
        );
    });
});
