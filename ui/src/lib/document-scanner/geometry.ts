/**
 * هندسة الإحداثيات (جزء م3: التحجيم فقط) — تحويل الزوايا بين دقة التحليل ودقة الحفظ.
 * م4 تضيف فحوص الجودة فوق هذه الأساسيات.
 */

import type { QuadPoint } from "./opencv-adapter.ts";

/** تحجيم نقاط من أبعاد مصدر إلى أبعاد هدف (نسبة مستقلة لكل محور) */
export function scalePoints(
    points: QuadPoint[],
    fromWidth: number,
    fromHeight: number,
    toWidth: number,
    toHeight: number,
): QuadPoint[] {
    if (fromWidth <= 0 || fromHeight <= 0) return points.map((p) => ({ ...p }));
    const sx = toWidth / fromWidth;
    const sy = toHeight / fromHeight;
    return points.map((p) => ({ x: p.x * sx, y: p.y * sy }));
}
