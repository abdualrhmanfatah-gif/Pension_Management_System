/**
 * مخزن بكسلات مربوط بـArrayBuffer عادي (وليس SharedArrayBuffer).
 * هذا هو النوع الذي يقبله ImageData مباشرة، فيحفظ نسخة كاملة من البيانات
 * عند كل تغليف بـnew ImageData(...).
 */
export type PixelBuffer = Uint8ClampedArray<ArrayBuffer>;

export function createPixelBuffer(length: number): PixelBuffer {
    return new Uint8ClampedArray(length);
}

/** ناتج معالج صور مشترك بين WebGL وCPU وWorker */
export interface ProcessResult {
    data: PixelBuffer;
    width: number;
    height: number;
}
