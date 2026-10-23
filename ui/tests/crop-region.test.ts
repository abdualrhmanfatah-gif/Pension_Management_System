import assert from "node:assert/strict";
import { test } from "node:test";

import {
    computeCropRegion,
    estimateOutputSize,
    fitToBudget,
    getCropBudget,
    type PixelBudget,
} from "../src/lib/crop-region.ts";
import type { DeviceCapabilities } from "../src/lib/device-capabilities.ts";

const budget = (maxDimension: number, maxPixels: number): PixelBudget => ({
    maxDimension,
    maxPixels,
});

test("fitToBudget يحترم سقف البعد", () => {
    const { width, height } = fitToBudget(4000, 2000, budget(1536, 6_000_000));
    assert.ok(width <= 1536, `width=${width}`);
    assert.ok(height <= 1536, `height=${height}`);
    assert.equal(width, 1536);
    assert.equal(height, 768);
});

test("fitToBudget يحترم سقف البكسلات حتى مع التقريب", () => {
    const cases: Array<[number, number, number]> = [
        [3072, 3072, 2_500_000],
        [4000, 3000, 1_000_000],
        [1920, 1080, 250_000],
        [517, 517, 8],
        [1000, 3, 100],
    ];
    for (const [w, h, maxPixels] of cases) {
        const size = fitToBudget(w, h, budget(8192, maxPixels));
        assert.ok(
            size.width * size.height <= maxPixels,
            `${w}x${h} @${maxPixels} => ${size.width}x${size.height}`,
        );
        assert.ok(size.width >= 1 && size.height >= 1);
    }
});

test("fitToBudget لا يكبّر الصور الصغيرة", () => {
    assert.deepEqual(fitToBudget(120, 80, budget(1536, 6_000_000)), {
        width: 120,
        height: 80,
    });
});

test("fitToBudget يحافظ على نسبة الأبعاد ضمن بكسل واحد", () => {
    const source = 1600;
    const target = 317;
    const { width, height } = fitToBudget(source, source, budget(target, 6_000_000));
    const expected = Math.floor((target / source) * source);
    assert.ok(Math.abs(width - expected) <= 1, `width=${width}`);
    assert.ok(Math.abs(height - expected) <= 1, `height=${height}`);
});

test("fitToBudget يعيد قيماً موجبة finite عند مدخلات معطوبة", () => {
    const invalid = [
        fitToBudget(0, 0, budget(0, 0)),
        fitToBudget(Number.NaN, -10, budget(-5, Number.NaN)),
        fitToBudget(Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY, {
            maxDimension: Number.POSITIVE_INFINITY,
            maxPixels: Number.POSITIVE_INFINITY,
        }),
    ];
    for (const size of invalid) {
        assert.ok(Number.isFinite(size.width) && size.width >= 1, JSON.stringify(size));
        assert.ok(Number.isFinite(size.height) && size.height >= 1, JSON.stringify(size));
    }
});

test("fitToBudget يحترم ميزانية غير صالحة عبر قيمة بديلة موجبة", () => {
    const { width, height } = fitToBudget(4000, 4000, {
        maxDimension: Number.NaN,
        maxPixels: 0,
    });
    assert.ok(Number.isFinite(width) && Number.isFinite(height));
    assert.ok(width >= 1 && height >= 1);
    assert.ok(width * height <= 1024 * 1024 * 1024);
});

test("estimateOutputSize يحسب متوسط أضلاع المربع المنظور", () => {
    const size = estimateOutputSize(
        [
            { x: 0, y: 0 },
            { x: 200, y: 20 },
            { x: 190, y: 320 },
            { x: 10, y: 300 },
        ],
        budget(4096, 6_000_000),
    );
    assert.equal(size.width, 191);
    assert.equal(size.height, 300);
});

test("estimateOutputSize يعيد 1x1 عند تدهور الزوايا", () => {
    assert.deepEqual(estimateOutputSize([], budget(1024, 1_000_000)), { width: 1, height: 1 });
    assert.deepEqual(
        estimateOutputSize(
            [
                { x: 50, y: 50 },
                { x: 50, y: 50 },
                { x: 50, y: 50 },
                { x: 50, y: 50 },
            ],
            budget(1024, 1_000_000),
        ),
        { width: 1, height: 1 },
    );
});

test("estimateOutputSize لا يتجاوز ميزانية البكسلات", () => {
    const corners = [
        { x: 0, y: 0 },
        { x: 4000, y: 0 },
        { x: 4000, y: 4000 },
        { x: 0, y: 4000 },
    ];
    const size = estimateOutputSize(corners, budget(4096, 2_500_000));
    assert.ok(size.width * size.height <= 2_500_000, `${size.width}x${size.height}`);
});

const capabilities = (maxPixels: number, maxTextureSize = 4096): DeviceCapabilities => ({
    tier: "high",
    maxTextureSize,
    maxPixels,
    isMobile: false,
    gpu: "test",
    hasWebGL2: false,
    hasWebGL: true,
    supportsFloatTextures: false,
    maxVertexUniforms: 256,
    maxFragmentUniforms: 256,
});

test("getCropBudget يشتق ميزانية البكسلات من قدرات الجهاز", () => {
    const resolved = getCropBudget(capabilities(2_250_000));
    assert.equal(resolved.maxPixels, 2_250_000);
    assert.ok(resolved.maxDim <= 4096);

    const missing = getCropBudget(capabilities(Number.NaN));
    assert.ok(Number.isFinite(missing.maxPixels) && missing.maxPixels > 0);
});

test("computeCropRegion يبقي أبعاد العمل داخل الميزانية", () => {
    const caps = capabilities(1_000_000);
    const corners = [
        { x: 120, y: 240 },
        { x: 3900, y: 300 },
        { x: 3800, y: 2100 },
        { x: 200, y: 2000 },
    ];
    const region = computeCropRegion(corners, 4000, 2400, caps);

    assert.equal(region.sx, 120);
    assert.equal(region.sy, 240);
    assert.ok(region.workWidth >= 1 && region.workHeight >= 1);
    assert.ok(region.workWidth * region.workHeight <= 1_000_000, `${region.workWidth}x${region.workHeight}`);
    assert.ok(region.workWidth <= region.sw);
    assert.ok(region.workHeight <= region.sh);
});

test("computeCropRegion يبقى ضمن حدود المصدر", () => {
    const caps = capabilities(6_000_000);
    const region = computeCropRegion(
        [
            { x: -50, y: -50 },
            { x: 900, y: -50 },
            { x: 900, y: 700 },
            { x: -50, y: 700 },
        ],
        800,
        600,
        caps,
    );
    assert.equal(region.sx, 0);
    assert.equal(region.sy, 0);
    assert.equal(region.sw, 800);
    assert.equal(region.sh, 600);
});