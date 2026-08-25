export interface DeviceCapabilities {
    tier: 'low' | 'medium' | 'high';
    maxTextureSize: number;
    isMobile: boolean;
    gpu: string;
    hasWebGL2: boolean;
    hasWebGL: boolean;
    supportsFloatTextures: boolean;
    maxVertexUniforms: number;
    maxFragmentUniforms: number;
}

let cachedCapabilities: DeviceCapabilities | null = null;

export function getDeviceCapabilities(): DeviceCapabilities {
    if (cachedCapabilities) return cachedCapabilities;

    const canvas = document.createElement('canvas');
    const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');

    if (!gl) {
        cachedCapabilities = {
            tier: 'low',
            maxTextureSize: 4096,
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

    // Cleanup
    const loseContext = gl.getExtension('WEBGL_lose_context');
    if (loseContext) {
        loseContext.loseContext();
    }

    cachedCapabilities = {
        tier,
        maxTextureSize: gl.getParameter(gl.MAX_TEXTURE_SIZE),
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
