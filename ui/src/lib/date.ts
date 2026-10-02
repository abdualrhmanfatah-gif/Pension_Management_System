export const YEMEN_TIME_ZONE = "Asia/Aden";

/** Explicit offsets are instants; old API wall times belong to Yemen. */
function parseDisplayDate(value: string | Date | number): Date {
    if (typeof value !== "string") return new Date(value);
    if (/^\d{4}-\d{2}-\d{2}$/.test(value)) return new Date(`${value}T00:00:00+03:00`);
    return new Date(/(?:Z|[+-]\d{2}:?\d{2})$/i.test(value) ? value : `${value}+03:00`);
}

export function formatYemenDate(value: string | Date | number | null | undefined, options: Intl.DateTimeFormatOptions = {}): string {
    if (value == null || value === "") return "";
    const date = parseDisplayDate(value);
    return Number.isNaN(date.getTime()) ? "" : new Intl.DateTimeFormat("ar-YE", {
        year: "numeric", month: "2-digit", day: "2-digit", ...options,
        calendar: "gregory", timeZone: YEMEN_TIME_ZONE,
    }).format(date);
}

export function formatYemenDateTime(value: string | Date | number | null | undefined, options: Intl.DateTimeFormatOptions = {}): string {
    if (value == null || value === "") return "";
    const date = parseDisplayDate(value);
    return Number.isNaN(date.getTime()) ? "" : new Intl.DateTimeFormat("ar-YE", {
        ...(Object.keys(options).length ? options : { dateStyle: "short", timeStyle: "short" }),
        calendar: "gregory", timeZone: YEMEN_TIME_ZONE,
    }).format(date);
}

export function formatDateOnly(value?: string | null): string {
    return formatDateInputDisplay(value);
}

export function daysBetweenDateOnly(start: string, end: string): number {
    const from = parseDateOnlyToUtc(normalizeDateInputValue(start));
    const to = parseDateOnlyToUtc(normalizeDateInputValue(end));
    return from && to ? Math.round((to.getTime() - from.getTime()) / 86400000) : Number.NaN;
}

export function ageOnDate(birthDate: string, onDate = todayYemen()): number {
    const birth = normalizeDateInputValue(birthDate);
    const today = normalizeDateInputValue(onDate);
    if (!birth || !today) return Number.NaN;
    const years = Number(today.slice(0, 4)) - Number(birth.slice(0, 4));
    return addYearsToDateOnly(birth, years) > today ? years - 1 : years;
}

const DATE_ONLY_PATTERN = /^(\d{4})-(\d{2})-(\d{2})$/;
const OFFSET_SUFFIX_PATTERN = /(?:Z|[+-]\d{2}:?\d{2})$/i;

const yemenDateFormatter = new Intl.DateTimeFormat("en-US", {
    timeZone: YEMEN_TIME_ZONE,
    calendar: "gregory",
    numberingSystem: "latn",
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
});

function daysInMonth(year: number, month: number): number {
    if (month === 2) {
        const isLeap = (year % 4 === 0 && year % 100 !== 0) || year % 400 === 0;
        return isLeap ? 29 : 28;
    }
    return [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31][month - 1];
}

function isValidDateOnly(value: string): boolean {
    const match = DATE_ONLY_PATTERN.exec(value);
    if (!match) return false;
    const year = Number(match[1]);
    const month = Number(match[2]);
    const day = Number(match[3]);
    if (month < 1 || month > 12) return false;
    return day >= 1 && day <= daysInMonth(year, month);
}

/**
 * يحوّل لحظة زمنية (Instant) إلى تاريخ تقويمي بتوقيت اليمن.
 * لا تمرّر نصاً بلا إزاحة (وقت جدار اليمن) — استخدم normalizeDateInputValue.
 */
export function toDateOnlyYemen(instant: Date | number): string {
    const date = instant instanceof Date ? instant : new Date(instant);
    if (Number.isNaN(date.getTime())) return "";

    const parts = yemenDateFormatter.formatToParts(date);
    const year = parts.find((part) => part.type === "year")?.value;
    const month = parts.find((part) => part.type === "month")?.value;
    const day = parts.find((part) => part.type === "day")?.value;
    if (!year || !month || !day) return "";

    return `${year}-${month}-${day}`;
}

/** تاريخ اليوم التقويمي بتوقيت اليمن بصيغة YYYY-MM-DD. */
export function todayYemen(): string {
    return toDateOnlyYemen(new Date());
}

/**
 * يطبّع قيمة تاريخ قادمة من API أو من نموذج إلى YYYY-MM-DD:
 * - YYYY-MM-DD: تُعاد كما هي.
 * - DateTime بلا إزاحة (عقد الـAPI = وقت جدار اليمن): يُؤخذ جزء التاريخ كما هو.
 * - DateTimeOffset/UTC (Z أو ±HH:mm): تُحوَّل إلى تاريخ اليمن.
 * - غير الصالح و null: "".
 */
export function normalizeDateInputValue(value?: string | null): string {
    if (typeof value !== "string") return "";
    const trimmed = value.trim();
    if (!trimmed) return "";

    if (DATE_ONLY_PATTERN.test(trimmed)) {
        return isValidDateOnly(trimmed) ? trimmed : "";
    }

    if (OFFSET_SUFFIX_PATTERN.test(trimmed)) {
        const parsed = new Date(trimmed);
        return Number.isNaN(parsed.getTime()) ? "" : toDateOnlyYemen(parsed);
    }

    const wallTime = /^(\d{4}-\d{2}-\d{2})T/.exec(trimmed);
    if (!wallTime) return "";
    return isValidDateOnly(wallTime[1]) ? wallTime[1] : "";
}

/** يحوّل قيمة تاريخ (API/نموذج) إلى صيغة عرض DateInput: DD/MM/YYYY أو "". */
export function formatDateInputDisplay(value?: string | null): string {
    const normalized = normalizeDateInputValue(value);
    if (!normalized) return "";
    const [year, month, day] = normalized.split("-");
    return `${day}/${month}/${year}`;
}

function parseDateOnlyToUtc(value: string): Date | null {
    if (!isValidDateOnly(value)) return null;
    const [year, month, day] = value.split("-").map(Number);
    const date = new Date(0);
    date.setUTCHours(0, 0, 0, 0);
    date.setUTCFullYear(year, month - 1, day);
    return date;
}

function formatUtcDate(date: Date): string {
    const year = date.getUTCFullYear();
    const month = String(date.getUTCMonth() + 1).padStart(2, "0");
    const day = String(date.getUTCDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
}

/** جمع أيام تقويمية على تاريخ YYYY-MM-DD دون أي اعتماد على منطقة الجهاز. */
export function addDaysToDateOnly(value: string, days: number): string {
    const date = parseDateOnlyToUtc(value);
    if (!date) return "";
    date.setUTCDate(date.getUTCDate() + days);
    return formatUtcDate(date);
}

/** جمع أشهر تقويمية مع ضبط اليوم إلى آخر يوم في الشهر عند الحاجة (2026-01-31 + 1 = 2026-02-28). */
export function addMonthsToDateOnly(value: string, months: number): string {
    const date = parseDateOnlyToUtc(value);
    if (!date) return "";

    const targetIndex = date.getUTCMonth() + months;
    const targetYear = date.getUTCFullYear() + Math.floor(targetIndex / 12);
    const targetMonth = ((targetIndex % 12) + 12) % 12;
    const targetDay = Math.min(date.getUTCDate(), daysInMonth(targetYear, targetMonth + 1));

    const result = new Date(0);
    result.setUTCHours(0, 0, 0, 0);
    result.setUTCFullYear(targetYear, targetMonth, targetDay);
    return formatUtcDate(result);
}

/** جمع سنوات تقويمية؛ 29 فبراير يُضبط إلى 28 فبراير في السنة غير الكبيسة (2024-02-29 + 1 = 2025-02-28). */
export function addYearsToDateOnly(value: string, years: number): string {
    const date = parseDateOnlyToUtc(value);
    if (!date) return "";

    const targetYear = date.getUTCFullYear() + years;
    const targetMonth = date.getUTCMonth() + 1;
    const targetDay = Math.min(date.getUTCDate(), daysInMonth(targetYear, targetMonth));

    const result = new Date(0);
    result.setUTCHours(0, 0, 0, 0);
    result.setUTCFullYear(targetYear, targetMonth - 1, targetDay);
    return formatUtcDate(result);
}
