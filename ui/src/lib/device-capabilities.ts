export interface DeviceCapabilities {
    tier: 'low' | 'medium' | 'high';
    maxTextureSize: number;
<<<<<<< HEAD
    /** سقف عدد بكسلات الصورة الناتجة عن المعالجة (ميزانية ذاكرة، لا بعد فقط) */
    maxPixels: number;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    isMobile: boolean;
    gpu: string;
    hasWebGL2: boolean;
    hasWebGL: boolean;
    supportsFloatTextures: boolean;
    maxVertexUniforms: number;
    maxFragmentUniforms: number;
}

let cachedCapabilities: DeviceCapabilities | null = null;

<<<<<<< HEAD
/** ميزانية البكسلات حسب الذاكرة المعلنة (deviceMemory بالجيغابايت) */
const MAX_PIXELS_BY_DEVICE_MEMORY: ReadonlyArray<readonly [number, number]> = [
    [8, 6_000_000],
    [4, 5_000_000],
    [2, 2_500_000],
    [1, 1_000_000],
];

/** بديل ثابت للأجهزة التي لا تعلن عن ذاكرتها (Safari/iOS) */
const MAX_PIXELS_BY_TIER: Record<DeviceCapabilities['tier'], number> = {
    high: 4_000_000,
    medium: 2_250_000,
    low: 1_000_000,
};

export const MIN_MAX_PIXELS = 1_000_000;

/** أعلى ميزانية مسموحة — تُستخدم كقيمة افتراضية عند غياب maxPixels */
export const MAX_ALLOWED_PIXELS = 6_000_000;

/**
 * ميزانية البكسلات النهائية: قيمة صالحة وموجبة دائماً.
 * الحد الأبعاد وحده لا يمنع استهلاك مئات الميغابايت عبر عدة مخازن متزامنة،
 * لأن صورة 3072×3072 تعني 9.4 مليون بكسل ≈ 37 MB لكل مخزن.
 */
export function resolveMaxPixels(
    tier: DeviceCapabilities['tier'],
    deviceMemory?: number,
): number {
    if (typeof deviceMemory === 'number' && Number.isFinite(deviceMemory) && deviceMemory > 0) {
        for (const [gb, pixels] of MAX_PIXELS_BY_DEVICE_MEMORY) {
            if (deviceMemory >= gb) return pixels;
        }
        return MIN_MAX_PIXELS;
    }
    return MAX_PIXELS_BY_TIER[tier];
}

function readDeviceMemory(): number | undefined {
    if (typeof navigator === 'undefined') return undefined;
    const value = (navigator as Navigator & { deviceMemory?: number }).deviceMemory;
    return typeof value === 'number' && Number.isFinite(value) && value > 0 ? value : undefined;
}

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
export function getDeviceCapabilities(): DeviceCapabilities {
    if (cachedCapabilities) return cachedCapabilities;

    const canvas = document.createElement('canvas');
    const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');

    if (!gl) {
<<<<<<< HEAD
        const tier = 'low' as const;
        cachedCapabilities = {
            tier,
            maxTextureSize: 4096,
            maxPixels: resolveMaxPixels(tier, readDeviceMemory()),
=======
        cachedCapabilities = {
            tier: 'low',
            maxTextureSize: 4096,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            isMobile: /Mobi|Android/i.test(navigator.userAgent),
            gpu: 'none',
            hasWebGL2: false,
            hasWebGL: false,
            supportsFloatTextures: false,
            maxVertexUniforms: 256,
            maxFragmentUniforms: 256,
        };
        return cachedCapabilities;
    }

    const debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
    const gpu = debugInfo
        ? gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL)
        : 'unknown';

    const isMobile = /Mobi|Android/i.test(navigator.userAgent);

    // Detect low-end GPUs
    const lowEndPatterns = /Adreno [3-5]|PowerVR SGX|VideoCore IV|Mali-4|Mali-T6|Mali-G3|Intel HD Graphics 3000|Intel HD Graphics 4000/i;
    const isLowEnd = isMobile && lowEndPatterns.test(gpu);

    // Detect high-end GPUs
    const highEndPatterns = /Adreno 6|Adreno 7|Mali-G7|Mali-G9|Mali-G1|Apple GPU|NVIDIA|AMD|Radeon|GeForce/i;
    const isHighEnd = !isMobile || highEndPatterns.test(gpu);

    let tier: 'low' | 'medium' | 'high';
    if (isLowEnd) {
        tier = 'low';
    } else if (isHighEnd) {
        tier = 'high';
    } else {
        tier = 'medium';
    }

    const hasWebGL2 = gl instanceof WebGL2RenderingContext;
    const hasWebGL = true;

    // Check float texture support
    const floatExt = gl.getExtension('OES_texture_float');
    const supportsFloatTextures = !!floatExt;

    const maxVertexUniforms = gl.getParameter(gl.MAX_VERTEX_UNIFORM_VECTORS);
    const maxFragmentUniforms = gl.getParameter(gl.MAX_FRAGMENT_UNIFORM_VECTORS);
<<<<<<< HEAD
    const maxTextureSize = gl.getParameter(gl.MAX_TEXTURE_SIZE);
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    // Cleanup
    const loseContext = gl.getExtension('WEBGL_lose_context');
    if (loseContext) {
        loseContext.loseContext();
    }

    cachedCapabilities = {
        tier,
<<<<<<< HEAD
        maxTextureSize: maxTextureSize || 4096,
        maxPixels: resolveMaxPixels(tier, readDeviceMemory()),
=======
        maxTextureSize: gl.getParameter(gl.MAX_TEXTURE_SIZE),
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        isMobile,
        gpu,
        hasWebGL2,
        hasWebGL,
        supportsFloatTextures,
        maxVertexUniforms,
        maxFragmentUniforms,
    };

    return cachedCapabilities;
}

export function getOptimalProcessingSize(capabilities: DeviceCapabilities): number {
    switch (capabilities.tier) {
        case 'high':
            return 1536;
        case 'medium':
            return 1024;
        case 'low':
            return 768;
        default:
            return 1024;
    }
}

export function shouldUseWebGL(capabilities: DeviceCapabilities): boolean {
    return capabilities.hasWebGL && !capabilities.isMobile;
}

export function shouldUseWebGLWithFallback(capabilities: DeviceCapabilities): boolean {
    return capabilities.hasWebGL;
}

export function resetCapabilitiesCache(): void {
    cachedCapabilities = null;
}
