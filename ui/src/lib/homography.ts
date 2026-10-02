export interface Point {
    x: number;
    y: number;
}

/**
 * التحقق من صلاحية زوايا القص قبل المعالجة.
 * ترجع سبب الرفض بالعربية أو null إذا كانت الزوايا صالحة.
 *
 * topLeftV: 0 عندما تكون v=0 أعلى المخرج (CPU)، 1 عندما v=1 أعلى المخرج (WebGL)
 */
export function validateCorners(
    corners: Point[],
    minSide = 10,
    minArea = 100,
): string | null {
    if (!corners || corners.length !== 4) {
        return "زوايا القص يجب أن تكون 4 نقاط";
    }

    if (corners.some((c) => !Number.isFinite(c.x) || !Number.isFinite(c.y))) {
        return "زوايا القص غير صالحة";
    }

    const [tl, tr, br, bl] = corners;
    const sides = [
        Math.hypot(tr.x - tl.x, tr.y - tl.y),
        Math.hypot(br.x - tr.x, br.y - tr.y),
        Math.hypot(bl.x - br.x, bl.y - br.y),
        Math.hypot(tl.x - bl.x, tl.y - bl.y),
    ];

    if (sides.some((s) => s < minSide)) {
        return "أحد أضلاع منطقة القص أصغر من الحد الأدنى";
    }

    // مساحة شولز
    const area2 =
        tl.x * tr.y - tr.x * tl.y +
        tr.x * br.y - br.x * tr.y +
        br.x * bl.y - bl.x * br.y +
        bl.x * tl.y - tl.x * bl.y;
    if (Math.abs(area2) < minArea * 2) {
        return "منطقة القص فارغة أو مساحتها صغيرة جداً";
    }

    // التحدب: جميع الضربات الاتجاهية للحواف المتتالية بنفس الإشارة
    // (تبديل الإشارة يكشف التقاطع بشكل متعرج bow-tie وغير المحدب)
    const cross = (ax: number, ay: number, bx: number, by: number, cx: number, cy: number) =>
        (bx - ax) * (cy - ay) - (by - ay) * (cx - ax);

    const signs = [
        cross(tl.x, tl.y, tr.x, tr.y, br.x, br.y),
        cross(tr.x, tr.y, br.x, br.y, bl.x, bl.y),
        cross(br.x, br.y, bl.x, bl.y, tl.x, tl.y),
        cross(bl.x, bl.y, tl.x, tl.y, tr.x, tr.y),
    ].filter((s) => Math.abs(s) > 1e-6);

    if (signs.length === 4 && !(signs.every((s) => s > 0) || signs.every((s) => s < 0))) {
        return "زوايا القص متقطعة أو غير محدبة";
    }

    return null;
}

/**
 * حساب مصفوفة المنظور 3×3 التي تنقل مربع الوحدة إلى زوايا المصدر،
 * مع قسمة منظورية (homography) بدل المزج الثنائي.
 *
 * الإرجاع: مصفوفة صف-كأصل [a b c; d e f; g h 1] أو null عند التدهور.
 * topLeftV: 0 إذا كانت v=0 أعلى المخرج (CPU)، 1 إذا كانت v=1 أعلى المخرج (WebGL).
 */
export function computeHomography(corners: Point[], topLeftV: 0 | 1): Float32Array | null {
    if (!corners || corners.length !== 4) return null;
    // فحص تدهور رقعي فقط — الإحداثيات قد تكون معيارية (0..1) فلا تُطبَّق عتبات البكسل هنا
    if (validateCorners(corners, 1e-6, 1e-6) !== null) return null;

    const [tl, tr, br, bl] = corners;

    // نقاط المربع الوحدة المقابلة لكل زاوية حسب اتجاه v في المخرج
    const square: Point[] =
        topLeftV === 0
            ? [
                  { x: 0, y: 0 }, // tl
                  { x: 1, y: 0 }, // tr
                  { x: 1, y: 1 }, // br
                  { x: 0, y: 1 }, // bl
              ]
            : [
                  { x: 0, y: 1 }, // tl
                  { x: 1, y: 1 }, // tr
                  { x: 1, y: 0 }, // br
                  { x: 0, y: 0 }, // bl
              ];

    const src = [tl, tr, br, bl];

    // نظام 8 معادلات خطي للمجهولات a..h (الحافة i = 1)
    // xi*(g*ui + h*vi + 1) = a*ui + b*vi + c
    // yi*(g*ui + h*vi + 1) = d*ui + e*vi + f
    const rows: number[][] = [];
    for (let i = 0; i < 4; i++) {
        const { x: u, y: v } = square[i];
        const { x: tx, y: ty } = src[i];
        rows.push([u, v, 1, 0, 0, 0, -tx * u, -tx * v, tx]);
        rows.push([0, 0, 0, u, v, 1, -ty * u, -ty * v, ty]);
    }

    // إلغاء غاوس مع اختيار القمة الجزئية
    const n = 8;
    for (let col = 0; col < n; col++) {
        let pivot = col;
        for (let r = col + 1; r < n; r++) {
            if (Math.abs(rows[r][col]) > Math.abs(rows[pivot][col])) pivot = r;
        }
        if (Math.abs(rows[pivot][col]) < 1e-10) return null; // تدهور
        [rows[col], rows[pivot]] = [rows[pivot], rows[col]];

        const pv = rows[col][col];
        for (let c = col; c <= n; c++) rows[col][c] /= pv;

        for (let r = 0; r < n; r++) {
            if (r === col) continue;
            const factor = rows[r][col];
            if (factor === 0) continue;
            for (let c = col; c <= n; c++) rows[r][c] -= factor * rows[col][c];
        }
    }

    const h = new Float32Array(9);
    for (let i = 0; i < 8; i++) h[i] = rows[i][n];
    h[8] = 1;
    return h;
}

/** تطبيق المنظور على نقطة (مع قسمة منظورية) بإحداثيات مصدر غير معيارية. */
export function applyHomography(
    h: Float32Array,
    u: number,
    v: number,
): { x: number; y: number } | null {
    const w = h[6] * u + h[7] * v + h[8];
    if (Math.abs(w) < 1e-10) return null;
    return {
        x: (h[0] * u + h[1] * v + h[2]) / w,
        y: (h[3] * u + h[4] * v + h[5]) / w,
    };
}
