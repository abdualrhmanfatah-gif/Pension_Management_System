import type { DeviceCapabilities } from "./device-capabilities.ts";
import { getOptimalProcessingSize, MIN_MAX_PIXELS } from "./device-capabilities.ts";

export interface CropRegion {
    sx: number;
    sy: number;
    sw: number;
    sh: number;
    workWidth: number;
    workHeight: number;
    scale: number;
}

export interface CropBudget {
    maxDim: number;
    previewDim: number;
    /** سقف البكسلات — ميزانية ذاكرة عبر كل المخازن المؤقتة */
    maxPixels: number;
}

export interface PixelBudget {
    maxDimension: number;
    maxPixels: number;
}

export interface PixelSize {
    width: number;
    height: number;
}

export interface Point2D {
    x: number;
    y: number;
}

/** قيمة بديلة آمنة عند تمرير ميزانية غير صالحة */
const FALLBACK_MAX_DIMENSION = 1024;
const FALLBACK_MAX_PIXELS = MIN_MAX_PIXELS;

function positiveOr(value: number, fallback: number): number {
    return Number.isFinite(value) && value > 0 ? value : fallback;
}

/**
 * تصغير الأبعاد حتى يحترم سقف البعد وسقف البكسلات معاً.
 * يحافظ على نسبة الأبعاد، ولا يكبّر (scale لا يتجاوز 1)، ويقبل أبعاداً/ميزانيات
 * غير صالحة عبر قيم بديلة موجبة بدل رمي NaN داخل WebGL أو ImageData.
 * الضبط النهائي بالقص لأسفل يضمن أن `width * height` لا يتجاوز `maxPixels` مهما كان التقريب.
 */
export function fitToBudget(
    width: number,
    height: number,
    budget: PixelBudget,
): PixelSize {
    const w = positiveOr(width, 1);
    const h = positiveOr(height, 1);
    const maxDimension = positiveOr(budget.maxDimension, FALLBACK_MAX_DIMENSION);
    const maxPixels = positiveOr(budget.maxPixels, FALLBACK_MAX_PIXELS);

    const scale = Math.min(
        1,
        maxDimension / Math.max(w, h),
        Math.sqrt(maxPixels / (w * h)),
    );

    const outW = Math.max(1, Math.floor(w * scale));
    const outH = Math.max(1, Math.floor(h * scale));

    return shrinkToPixelCeiling(outW, outH, maxPixels);
}

/** قص المحور الأكبر بكسل واحد حتى لا تتجاوز المساحة السقف (أقصى انحراف بكسل واحد) */
function shrinkToPixelCeiling(width: number, height: number, maxPixels: number): PixelSize {
    let w = width;
    let h = height;
    while (w * h > maxPixels) {
        if (w >= h && w > 1) {
            w--;
        } else if (h > 1) {
            h--;
        } else {
            break;
        }
    }
    return { width: w, height: h };
}

export function getCropBudget(capabilities: DeviceCapabilities): CropBudget {
    const previewDim = getOptimalProcessingSize(capabilities);
    const maxDim = Math.min(capabilities.maxTextureSize, previewDim * 2);
    const maxPixels = positiveOr(capabilities.maxPixels, MIN_MAX_PIXELS);
    return { maxDim, previewDim, maxPixels };
}

/**
 * حجم الناتج المتوقع لمربع من الزوايا — مصدر واحد لحساب الأبعاد
 * تشترك فيه WebGL وCPU وواجهة الـpipeline (لتحديد ما إذا كان CPU على main thread مسموحاً).
 * ترجع 1×1 عند التدهور ليكتشفه المستدعي.
 */
export function estimateOutputSize(corners: Point2D[], budget: PixelBudget): PixelSize {
    if (!corners || corners.length < 4) return { width: 1, height: 1 };
    const [tl, tr, br, bl] = corners;

    const wTop = Math.hypot(tr.x - tl.x, tr.y - tl.y);
    const wBot = Math.hypot(br.x - bl.x, br.y - bl.y);
    const outW = Math.round((wTop + wBot) / 2);
    const hL = Math.hypot(bl.x - tl.x, bl.y - tl.y);
    const hR = Math.hypot(br.x - tr.x, br.y - tr.y);
    const outH = Math.round((hL + hR) / 2);

    if (!Number.isFinite(outW) || !Number.isFinite(outH) || outW < 1 || outH < 1) {
        return { width: 1, height: 1 };
    }

    return fitToBudget(outW, outH, budget);
}

/**
 * تحديد منطقة القص على الأصل بدقتها الكاملة ضمن حدود ذاكرة الجهاز،
 * بدل تصغير الصورة كاملة ثم القص (فقدان تفاصيل الوثيقة الصغيرة).
 */
export function computeCropRegion(
    corners: { x: number; y: number }[],
    sourceWidth: number,
    sourceHeight: number,
    capabilities: DeviceCapabilities,
): CropRegion {
    const budget = getCropBudget(capabilities);
    const { maxDim, maxPixels } = budget;

    let minX = Infinity;
    let minY = Infinity;
    let maxX = -Infinity;
    let maxY = -Infinity;
    for (const c of corners) {
        minX = Math.min(minX, c.x);
        maxX = Math.max(maxX, c.x);
        minY = Math.min(minY, c.y);
        maxY = Math.max(maxY, c.y);
    }

    const sx = Math.max(0, Math.floor(minX));
    const sy = Math.max(0, Math.floor(minY));
    const sw = Math.max(1, Math.min(sourceWidth, Math.ceil(maxX)) - sx);
    const sh = Math.max(1, Math.min(sourceHeight, Math.ceil(maxY)) - sy);

    const { width: workWidth, height: workHeight } = fitToBudget(sw, sh, {
        maxDimension: maxDim,
        maxPixels,
    });

    // المقياس الفعلي بعد التقريب: يضمن أن تبقى الزوايا المُحوَّلة داخل صورة العمل
    const scale = Math.min(workWidth / sw, workHeight / sh);

    return { sx, sy, sw, sh, workWidth, workHeight, scale };
}
