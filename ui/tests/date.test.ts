import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import process from "node:process";
import { test } from "node:test";

import {
    addDaysToDateOnly,
    addMonthsToDateOnly,
    addYearsToDateOnly,
    ageOnDate,
    daysBetweenDateOnly,
    formatDateInputDisplay,
    formatDateOnly,
    formatYemenDate,
    formatYemenDateTime,
    normalizeDateInputValue,
    toDateOnlyYemen,
    todayYemen,
} from "../src/lib/date.ts";

test("expiry dates include their full Yemen calendar day", () => {
    assert.equal(daysBetweenDateOnly("2026-09-22", "2026-09-22"), 0);
    assert.equal(daysBetweenDateOnly("2026-09-23", "2026-09-22"), -1);
    assert.equal(daysBetweenDateOnly("2026-12-31", "2027-01-01"), 1);
});

test("age uses the birthday, including leap-day clamping", () => {
    assert.equal(ageOnDate("2008-09-22", "2026-09-21"), 17);
    assert.equal(ageOnDate("2008-09-22", "2026-09-22"), 18);
    assert.equal(ageOnDate("2000-02-29", "2026-02-28"), 26);
});

test("display preserves dates and recognizes UTC and legacy Yemen wall times", () => {
    assert.equal(formatDateOnly("1966-12-31"), "31/12/1966");
    assert.equal(formatYemenDate("2026-09-21T21:30:00Z"), formatYemenDate("2026-09-22"));
    assert.equal(formatYemenDateTime("2026-09-21T21:30:00Z"), formatYemenDateTime("2026-09-22T00:30:00"));
    assert.equal(formatYemenDate("bad input"), "");
});

test("display is identical across browser host timezones", () => {
    const moduleUrl = new URL("../src/lib/date.ts", import.meta.url).href;
    const script = `import { formatDateOnly, formatYemenDate, formatYemenDateTime } from ${JSON.stringify(moduleUrl)}; process.stdout.write(JSON.stringify([formatDateOnly("1966-12-31"), formatYemenDate("2026-09-22"), formatYemenDateTime("2026-09-21T21:30:00Z")]));`;
    const outputs = ["UTC", "Asia/Aden", "America/Los_Angeles", "Asia/Tokyo"].map(tz =>
        execFileSync(process.execPath, ["--input-type=module", "--eval", script], {
            env: { ...process.env, TZ: tz }, encoding: "utf8",
        }));
    assert.ok(outputs.every(output => output === outputs[0]));
});

test("toDateOnlyYemen: midnight boundary in Yemen", () => {
    assert.strictEqual(toDateOnlyYemen(new Date("2026-09-21T20:59:59Z")), "2026-09-21");
    assert.strictEqual(toDateOnlyYemen(new Date("2026-09-21T21:00:00Z")), "2026-09-22");
    assert.strictEqual(toDateOnlyYemen(new Date("2026-09-21T21:30:00Z")), "2026-09-22");
});

test("toDateOnlyYemen: year boundary in Yemen", () => {
    assert.strictEqual(toDateOnlyYemen(new Date("2025-12-31T20:59:59Z")), "2025-12-31");
    assert.strictEqual(toDateOnlyYemen(new Date("2025-12-31T21:00:00Z")), "2026-01-01");
});

test("toDateOnlyYemen: invalid instants", () => {
    assert.strictEqual(toDateOnlyYemen(new Date("invalid")), "");
    assert.strictEqual(toDateOnlyYemen(Number.NaN), "");
});

test("normalizeDateInputValue: DateOnly values unchanged", () => {
    assert.strictEqual(normalizeDateInputValue("2026-09-22"), "2026-09-22");
    assert.strictEqual(normalizeDateInputValue("2024-02-29"), "2024-02-29");
});

test("normalizeDateInputValue: API wall time keeps its own day", () => {
    assert.strictEqual(normalizeDateInputValue("2026-09-22T00:00:00"), "2026-09-22");
    assert.strictEqual(normalizeDateInputValue("2026-09-22T23:30:00"), "2026-09-22");
});

test("normalizeDateInputValue: DateTimeOffset and UTC converted to Yemen date", () => {
    assert.strictEqual(normalizeDateInputValue("2026-09-21T21:30:00Z"), "2026-09-22");
    assert.strictEqual(normalizeDateInputValue("2026-09-22T00:30:00+03:00"), "2026-09-22");
    assert.strictEqual(normalizeDateInputValue("2026-09-21T23:30:00-05:00"), "2026-09-22");
});

test("normalizeDateInputValue: invalid and empty values", () => {
    assert.strictEqual(normalizeDateInputValue(null), "");
    assert.strictEqual(normalizeDateInputValue(undefined), "");
    assert.strictEqual(normalizeDateInputValue(""), "");
    assert.strictEqual(normalizeDateInputValue("   "), "");
    assert.strictEqual(normalizeDateInputValue("not-a-date"), "");
    assert.strictEqual(normalizeDateInputValue("2026-02-30"), "");
    assert.strictEqual(normalizeDateInputValue("2026-13-01"), "");
    assert.strictEqual(normalizeDateInputValue("2023-02-29"), "");
});

test("todayYemen returns current Yemen calendar date", () => {
    const value = todayYemen();
    assert.match(value, /^\d{4}-\d{2}-\d{2}$/);
    assert.strictEqual(value, toDateOnlyYemen(new Date()));
});

test("formatDateInputDisplay: DateOnly and API wall time", () => {
    assert.strictEqual(formatDateInputDisplay("2026-09-22"), "22/09/2026");
    assert.strictEqual(formatDateInputDisplay("2026-09-22T00:00:00"), "22/09/2026");
    assert.strictEqual(formatDateInputDisplay("2026-09-22T23:30:00"), "22/09/2026");
});

test("formatDateInputDisplay: instants converted to Yemen date", () => {
    assert.strictEqual(formatDateInputDisplay("2026-09-21T21:30:00Z"), "22/09/2026");
    assert.strictEqual(formatDateInputDisplay("2026-09-22T00:30:00+03:00"), "22/09/2026");
});

test("formatDateInputDisplay: invalid and empty values", () => {
    assert.strictEqual(formatDateInputDisplay(null), "");
    assert.strictEqual(formatDateInputDisplay(undefined), "");
    assert.strictEqual(formatDateInputDisplay("not-a-date"), "");
    assert.strictEqual(formatDateInputDisplay("2026-02-30"), "");
});

test("ExemptionDialog path: API DateTime seeds DateInput display", () => {
    const apiExemptionDate = "2026-09-22T00:00:00";
    const seeded = normalizeDateInputValue(apiExemptionDate) || todayYemen();
    assert.strictEqual(formatDateInputDisplay(seeded), "22/09/2026");
});

test("addDaysToDateOnly: calendar days across month and year", () => {
    assert.strictEqual(addDaysToDateOnly("2026-01-31", 1), "2026-02-01");
    assert.strictEqual(addDaysToDateOnly("2024-02-28", 1), "2024-02-29");
    assert.strictEqual(addDaysToDateOnly("2026-12-31", 1), "2027-01-01");
    assert.strictEqual(addDaysToDateOnly("2026-01-01", -1), "2025-12-31");
    assert.strictEqual(addDaysToDateOnly("2026-03-01", -1), "2026-02-28");
    assert.strictEqual(addDaysToDateOnly("2026-09-22", 0), "2026-09-22");
});

test("addMonthsToDateOnly: clamps to last day of month", () => {
    assert.strictEqual(addMonthsToDateOnly("2026-01-31", 1), "2026-02-28");
    assert.strictEqual(addMonthsToDateOnly("2024-01-31", 1), "2024-02-29");
    assert.strictEqual(addMonthsToDateOnly("2026-03-31", -1), "2026-02-28");
    assert.strictEqual(addMonthsToDateOnly("2026-01-31", 3), "2026-04-30");
    assert.strictEqual(addMonthsToDateOnly("2026-11-30", 3), "2027-02-28");
    assert.strictEqual(addMonthsToDateOnly("2026-12-15", 1), "2027-01-15");
    assert.strictEqual(addMonthsToDateOnly("2026-01-15", -1), "2025-12-15");
    assert.strictEqual(addMonthsToDateOnly("2026-01-31", -13), "2024-12-31");
});

test("addYearsToDateOnly: Feb 29 clamps to Feb 28 in non-leap years", () => {
    assert.strictEqual(addYearsToDateOnly("2024-02-29", 1), "2025-02-28");
    assert.strictEqual(addYearsToDateOnly("2024-02-29", 4), "2028-02-29");
    assert.strictEqual(addYearsToDateOnly("2026-09-22", 1), "2027-09-22");
    assert.strictEqual(addYearsToDateOnly("2026-09-22", -1), "2025-09-22");
});

test("date-only arithmetic: invalid values return empty", () => {
    assert.strictEqual(addDaysToDateOnly("2026-02-30", 1), "");
    assert.strictEqual(addDaysToDateOnly("not-a-date", 1), "");
    assert.strictEqual(addMonthsToDateOnly("", 1), "");
    assert.strictEqual(addMonthsToDateOnly("2026-13-01", 1), "");
    assert.strictEqual(addYearsToDateOnly("2026-13-01", 1), "");
    assert.strictEqual(addYearsToDateOnly("2023-02-29", 1), "");
});

test("result independent of host process timezone", () => {
    const moduleUrl = new URL("../src/lib/date.ts", import.meta.url).href;
    const script = `import { addDaysToDateOnly, toDateOnlyYemen } from ${JSON.stringify(moduleUrl)}; process.stdout.write(toDateOnlyYemen(new Date("2026-09-21T21:30:00Z")) + "," + addDaysToDateOnly("2026-01-31", 1));`;

    for (const tz of ["UTC", "America/New_York", "Asia/Tokyo"]) {
        const output = execFileSync(process.execPath, ["--input-type=module", "--eval", script], {
            env: { ...process.env, TZ: tz },
            encoding: "utf8",
        });
        assert.strictEqual(output.trim(), "2026-09-22,2026-02-01", `TZ=${tz}`);
    }
});
