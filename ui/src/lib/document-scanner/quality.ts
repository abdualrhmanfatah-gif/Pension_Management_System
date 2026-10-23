/**
 * مؤشرات جودة تنبيهية فقط (م4) — لا تمنع التقاطًا ولا إضافة.
 * تحسب على منطقة الوثيقة المقصوصة قبل أي تحسين تجميلي.
 * العتبات مبدئية قبل المعايرة على عينات عربية وخط يدوي — configurable.
 */

export type QualityFlag = "blur" | "dark" | "bright" | "lowContrast" | "glare";

export interface QualityThresholds {
    /** تباين لابلاسيان الأدنى للحدة المقبولة */
    minSharpness: number;
    /** متوسط إضاءة أدنى */
    minBrightness: number;
    /** متوسط إضاءة أقصى */
    maxBrightness: number;
    /** انحراف معياري أدنى للتباين */
    minContrast: number;
    /** نسبة البكسلات الساطعة (>245) القصوى قبل تنبيه اللمعان */
    maxBrightFraction: number;
}

/** عتبات مبدئية — تعاير ميدانيًا في م5، ليست نهائية */
export const DEFAULT_QUALITY_THRESHOLDS: QualityThresholds = {
    minSharpness: 60,
    minBrightness: 55,
    maxBrightness: 205,
    minContrast: 22,
    maxBrightFraction: 0.08,
};

export interface QualityAssessment {
    sharpness: number;
    brightness: number;
    contrast: number;
    brightFraction: number;
    flags: QualityFlag[];
    needsReview: boolean;
}

function luminance(data: Uint8ClampedArray, i: number): number {
    return 0.299 * data[i] + 0.587 * data[i + 1] + 0.114 * data[i + 2];
}

/**
 * تقييم جودة صورة منطقة الوثيقة. يعمل على نسخة مصغرة داخليًا للأداء.
 * خالص تمامًا — يعمل في الاختبارات بلا DOM.
 */
export function assessQuality(
    imageData: ImageData,
    thresholds: QualityThresholds = DEFAULT_QUALITY_THRESHOLDS,
): QualityAssessment {
    const { data, width, height } = imageData;
    const pixels = width * height;
    if (pixels <= 0) {
        return { sharpness: 0, brightness: 0, contrast: 0, brightFraction: 1, flags: ["blur", "dark"], needsReview: true };
    }

    let sum = 0;
    let bright = 0;
    const gray = new Float64Array(pixels);
    for (let p = 0; p < pixels; p++) {
        const lum = luminance(data, p * 4);
        gray[p] = lum;
        sum += lum;
        if (lum > 245) bright++;
    }
    const brightness = sum / pixels;
    const brightFraction = bright / pixels;

    let varianceSum = 0;
    for (let p = 0; p < pixels; p++) {
        const diff = gray[p] - brightness;
        varianceSum += diff * diff;
    }
    const contrast = Math.sqrt(varianceSum / pixels);

    // حدة: متوسط القيمة المطلقة للابلاسيان 3×3 (مبسط: فروق الجيران الأربع)
    let lapSum = 0;
    let lapCount = 0;
    for (let y = 1; y < height - 1; y++) {
        for (let x = 1; x < width - 1; x++) {
            const c = gray[y * width + x];
            const lap = Math.abs(4 * c - gray[(y - 1) * width + x] - gray[(y + 1) * width + x] - gray[y * width + x - 1] - gray[y * width + x + 1]);
            lapSum += lap;
            lapCount++;
        }
    }
    const sharpness = lapCount > 0 ? lapSum / lapCount : 0;

    const flags: QualityFlag[] = [];
    if (sharpness < thresholds.minSharpness) flags.push("blur");
    if (brightness < thresholds.minBrightness) flags.push("dark");
    if (brightness > thresholds.maxBrightness) flags.push("bright");
    if (contrast < thresholds.minContrast) flags.push("lowContrast");
    if (brightFraction > thresholds.maxBrightFraction) flags.push("glare");

    return { sharpness, brightness, contrast, brightFraction, flags, needsReview: flags.length > 0 };
}

export const QUALITY_FLAG_LABELS: Record<QualityFlag, string> = {
    blur: "ضبابية — أعد التصوير بثبات أكبر",
    dark: "إضاءة خافتة",
    bright: "إضاءة زائدة",
    lowContrast: "تباين منخفض",
    glare: "لمعان قد يغطي بيانات",
};
