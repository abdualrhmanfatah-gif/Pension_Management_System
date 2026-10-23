import assert from "node:assert/strict";
import { test } from "node:test";

import {
    buildFormData,
    findActiveFile,
    type FormState,
    initialFormState,
    isRetireeStepBlocked,
    isStepValid,
    mapRetireeToForm,
    resolveInitialForm,
    type RetireeFullData,
    shouldConfirmDismissal,
    shouldConfirmExplicitCancel,
    shouldOpenExistingFileOnEnter,
    toDraft,
    validateStep,
} from "../src/lib/retirementFileForm.ts";

const today = "2026-09-29";

const fullData: RetireeFullData = {
    retireeID: 42,
    fullName: "محمد أحمد",
    nationalID: "123456789012",
    militaryNumber: null,
    gender: 1,
    birthDate: "1980-05-15",
    phone: "777123456",
    address: "صنعاء",
    city: "صنعاء",
    governorate: "صنعاء",
    socialStatusID: 2,
    healthStatusID: 1,
    isAlive: true,
    deathDate: null,
    personalPhotoPath: null,
};

function makeForm(overrides: Partial<FormState> = {}): FormState {
    return { ...initialFormState, ...overrides };
}

test("draft of another retiree is discarded entirely, not just its identity fields", () => {
    const leaked = toDraft(
        makeForm({
            retireeID: 7,
            nationalID: "999999999999",
            address: "عنوان المتقاعد الآخر",
            phone: "733333333",
            notes: "ملاحظات سرية للمتقاعد الآخر",
        }),
    );

    const result = resolveInitialForm({
        mode: "create",
        initialRetireeId: 42,
        rawDraft: JSON.stringify(leaked),
    });

    assert.equal(result.draftRestored, false);
    assert.deepEqual(result.form, initialFormState);
    assert.equal(result.form.nationalID, "");
    assert.equal(result.form.address, "");
    assert.equal(result.form.phone, "");
    assert.equal(result.form.notes, "");
    assert.equal(result.form.retireeID, 0);
});

test("draft is restored only for the same retiree and edit mode never reads drafts", () => {
    const draft = toDraft(makeForm({ retireeID: 42, nationalID: "123456789012", notes: "مسودة" }));

    const sameRetiree = resolveInitialForm({
        mode: "create",
        initialRetireeId: 42,
        rawDraft: JSON.stringify(draft),
    });
    assert.equal(sameRetiree.draftRestored, true);
    assert.equal(sameRetiree.form.notes, "مسودة");

    const edit = resolveInitialForm({
        mode: "edit",
        initialRetireeId: 42,
        rawDraft: JSON.stringify(draft),
    });
    assert.equal(edit.draftRestored, false);
    assert.deepEqual(edit.form, initialFormState);
});

test("draft keeps default lookups when stored values are empty", () => {
    const draft = toDraft(makeForm({ retireeID: 42, gender: "", socialStatusID: "", healthStatusID: "" }));
    const result = resolveInitialForm({ mode: "create", initialRetireeId: 42, rawDraft: JSON.stringify(draft) });
    assert.equal(result.form.gender, "1");
    assert.equal(result.form.socialStatusID, "2");
    assert.equal(result.form.healthStatusID, "1");
});

test("draft never carries File objects", () => {
    const draft = toDraft(makeForm({ retireeID: 1, personalPhoto: {} as File, idDocument: {} as File }));
    assert.equal("personalPhoto" in draft, false);
    assert.equal("idDocument" in draft, false);
    assert.equal(JSON.parse(JSON.stringify(draft)).personalPhoto, undefined);
});

test("invalid draft JSON falls back to a clean form", () => {
    const result = resolveInitialForm({ mode: "create", initialRetireeId: 42, rawDraft: "{not json" });
    assert.deepEqual(result.form, initialFormState);
    assert.equal(result.draftRestored, false);
});

test("prefill keeps DateOnly values untouched", () => {
    const mapped = mapRetireeToForm({ ...fullData, birthDate: "1980-05-15" }, "");
    assert.equal(mapped.birthDate, "1980-05-15");

    const legacyWallTime = mapRetireeToForm({ ...fullData, deathDate: "2010-03-04T00:00:00" }, "");
    assert.equal(legacyWallTime.deathDate, "2010-03-04");

    const utc = mapRetireeToForm({ ...fullData, birthDate: "1980-05-14T21:30:00Z" }, "");
    assert.equal(utc.birthDate, "1980-05-15");

    assert.equal(mapRetireeToForm({ ...fullData, birthDate: null }, "").birthDate, "");
});

test("today is accepted as a birth date regardless of host timezone", () => {
    const form = makeForm({ retireeID: 42, nationalID: "123456789012", birthDate: today });
    const errors = validateStep(form, 1, { today });
    assert.equal(errors.birthDate, undefined);
    assert.equal(isStepValid(errors), true);
});

test("future birth and death dates are rejected by calendar comparison", () => {
    const future = validateStep(
        makeForm({ retireeID: 42, nationalID: "123456789012", birthDate: "2026-09-30" }),
        1,
        { today },
    );
    assert.equal(future.birthDate, "تاريخ الميلاد لا يمكن أن يكون في المستقبل");

    const death = validateStep(
        makeForm({ isAlive: false, deathDate: "2026-09-30" }),
        1,
        { today },
    );
    assert.equal(death.deathDate, "تاريخ الوفاة لا يمكن أن تكون في المستقبل");
});

test("death date before birth date is rejected", () => {
    const errors = validateStep(
        makeForm({ isAlive: false, birthDate: "1980-05-15", deathDate: "1980-05-14" }),
        1,
        { today },
    );
    assert.equal(errors.deathDate, "تاريخ الوفاة لا يمكن أن يكون قبل تاريخ الميلاد");
});

test("prefill failure blocks saving so defaults cannot overwrite real retiree data", () => {
    const form = makeForm({ retireeID: 42, nationalID: "123456789012", birthDate: "1980-05-15" });
    const errors = validateStep(form, 1, { today, prefillError: "فشل في تحميل بيانات المتقاعد" });
    assert.equal(isStepValid(errors), false);
    assert.equal(isRetireeStepBlocked({ prefillError: "فشل" }), true);
});

test("an active file blocks the retiree step", () => {
    const active = { fileID: 9, fileNumber: "RF-001", statusID: 2, fileStatusName: "نشط" };
    assert.equal(isRetireeStepBlocked({ existingFile: active }), true);
    const errors = validateStep(makeForm({ retireeID: 42 }), 0, { existingFile: active });
    assert.equal(errors.retireeID, "لا يمكن المتابعة، لهذا المتقاعد ملف نشط بالفعل");
});

test("enter in the retiree step opens the existing file instead of advancing", () => {
    const active = { fileID: 9, fileNumber: "RF-001", statusID: 2, fileStatusName: "نشط" };
    assert.equal(shouldOpenExistingFileOnEnter({ mode: "create", step: 0, existingFile: active }), true);
    assert.equal(shouldOpenExistingFileOnEnter({ mode: "create", step: 1, existingFile: active }), false);
    assert.equal(shouldOpenExistingFileOnEnter({ mode: "edit", step: 0, existingFile: active }), false);
    assert.equal(shouldOpenExistingFileOnEnter({ mode: "create", step: 0, existingFile: null }), false);
});

test("only a non-closed file counts as active", () => {
    const closed = { fileID: 1, statusID: 4 };
    const unknown = { fileID: 2, statusID: null };
    const active = { fileID: 3, statusID: 2 };
    assert.equal(findActiveFile([closed])?.fileID, closed.fileID);
    assert.equal(findActiveFile([unknown, active])?.fileID, 2);
    assert.equal(findActiveFile([]), undefined);
});

test("a failed active-file check fails closed instead of letting the user through", () => {
    assert.equal(isRetireeStepBlocked({ existingCheckFailed: true }), true);
    const errors = validateStep(makeForm({ retireeID: 42 }), 0, { existingCheckFailed: true });
    assert.equal(errors.retireeID, "تعذّر التحقق من ملفات المتقاعد. أعد المحاولة قبل المتابعة.");
});

test("phone format is validated when provided and stays optional", () => {
    assert.equal(validateStep(makeForm({ phone: "" }), 2, { today }).phone, undefined);
    assert.equal(validateStep(makeForm({ phone: "777123456" }), 2, { today }).phone, undefined);
    assert.equal(validateStep(makeForm({ phone: "01234567" }), 2, { today }).phone, undefined);
    assert.equal(validateStep(makeForm({ phone: "12345" }), 2, { today }).phone, "رقم الهاتف يجب أن يكون 8 أو 9 أرقام");
    assert.equal(validateStep(makeForm({ phone: "77712345678" }), 2, { today }).phone, "رقم الهاتف يجب أن يكون 8 أو 9 أرقام");
    assert.equal(validateStep(makeForm({ phone: "612345678" }), 2, { today }).phone, "رقم الهاتف غير صحيح (موبايل: 7XXXXXXXX / أرضي: 0XXXXXXX)");
    assert.equal(validateStep(makeForm({ phone: "12345678" }), 2, { today }).phone, "رقم الهاتف غير صحيح (موبايل: 7XXXXXXXX / أرضي: 0XXXXXXX)");
    assert.equal(validateStep(makeForm({ phone: "012345678" }), 2, { today }).phone, "رقم الهاتف غير صحيح (موبايل: 7XXXXXXXX / أرضي: 0XXXXXXX)");
});

test("submitted dates are plain YYYY-MM-DD, never ISO instants", () => {
    const data = buildFormData(
        makeForm({
            retireeID: 42,
            nationalID: "123456789012",
            birthDate: "1980-05-15",
            deathDate: "2010-03-04",
            isAlive: false,
        }),
    );
    assert.equal(data.get("birthDate"), "1980-05-15");
    assert.equal(data.get("deathDate"), "2010-03-04");
    for (const [, value] of data.entries()) {
        if (typeof value !== "string") continue;
        assert.equal(/T\d{2}:\d{2}/.test(value), false, `date leaked as instant: ${value}`);
    }
});

test("creating an alive retiree omits deathDate", () => {
    const data = buildFormData(makeForm({ retireeID: 42, birthDate: "1980-05-15", deathDate: "2010-03-04", isAlive: true }));
    assert.equal(data.get("isAlive"), "true");
    assert.equal(data.has("deathDate"), false);
});

test("editing ordinary file data does not submit life status fields", () => {
    const form = makeForm({ retireeID: 42, isAlive: false, deathDate: "2024-03-04", healthStatusID: "4" });
    const data = buildFormData(form, false);
    assert.equal(data.has("isAlive"), false);
    assert.equal(data.has("deathDate"), false);
    assert.equal(data.has("healthStatusID"), false);
    assert.equal(validateStep({ ...form, deathDate: "" }, 1, { today, lifeStatusReadOnly: true }).deathDate, undefined);
});

test("explicit cancel asks when data exists, dismissal only asks for unsaved edits", () => {
    assert.equal(shouldConfirmExplicitCancel("create", makeForm(), 0, 0), false);
    assert.equal(shouldConfirmExplicitCancel("create", makeForm({ retireeID: 42 }), 0, 0), true);
    assert.equal(shouldConfirmExplicitCancel("create", makeForm(), 1, 0), true);
    assert.equal(shouldConfirmExplicitCancel("edit", makeForm(), 1, 1), false);
    assert.equal(shouldConfirmExplicitCancel("edit", makeForm(), 3, 1), true);

    assert.equal(shouldConfirmDismissal("edit", true), true);
    assert.equal(shouldConfirmDismissal("edit", false), false);
    assert.equal(shouldConfirmDismissal("create", true), false);
});
