<<<<<<< HEAD
import { fitToBudget } from "./crop-region.ts";
import { MAX_ALLOWED_PIXELS } from "./device-capabilities.ts";
import { computeHomography, validateCorners } from "./homography.ts";
import { createPixelBuffer, type PixelBuffer, type ProcessResult } from "./pixel-buffer.ts";

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
const WARP_VERTEX_SHADER = `
    attribute vec2 aPosition;
    varying vec2 vUv;
    
    void main() {
        vUv = aPosition * 0.5 + 0.5;
        gl_Position = vec4(aPosition, 0.0, 1.0);
    }
`;

const WARP_FRAGMENT_SHADER = `
    precision highp float;
    uniform sampler2D uTexture;
<<<<<<< HEAD
    uniform mat3 uHomography;
    varying vec2 vUv;
    
    void main() {
        // منظور: مربع الوحدة (v=1 أعلى المخرج) إلى إحداثيات المصدر
        vec3 p = uHomography * vec3(vUv, 1.0);
        vec2 srcCoord = p.xy / p.z;
=======
    uniform vec2 uCorners[4];
    uniform vec2 uResolution;
    varying vec2 vUv;
    
    vec2 bilinearTransform(vec2 uv) {
        vec2 tl = uCorners[0];
        vec2 tr = uCorners[1];
        vec2 br = uCorners[2];
        vec2 bl = uCorners[3];
        
        float u = uv.x;
        float v = uv.y;
        
        vec2 top = mix(tl, tr, u);
        vec2 bottom = mix(bl, br, u);
        return mix(top, bottom, v);
    }
    
    void main() {
        vec2 srcCoord = bilinearTransform(vUv);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        
        // Clamp to texture bounds
        srcCoord = clamp(srcCoord, vec2(0.0), vec2(1.0));
        
        gl_FragColor = texture2D(uTexture, srcCoord);
    }
`;

const ENHANCE_FRAGMENT_SHADER = `
    precision highp float;
    uniform sampler2D uTexture;
    uniform vec2 uResolution;
    uniform float uAmount;
    varying vec2 vUv;
    
    void main() {
        vec2 texel = 1.0 / uResolution;
        
        // Gaussian blur 3x3
        vec4 sum = vec4(0.0);
        sum += texture2D(uTexture, vUv + vec2(-1.0, -1.0) * texel) * 0.25;
        sum += texture2D(uTexture, vUv + vec2( 0.0, -1.0) * texel) * 0.5;
        sum += texture2D(uTexture, vUv + vec2( 1.0, -1.0) * texel) * 0.25;
        sum += texture2D(uTexture, vUv + vec2(-1.0,  0.0) * texel) * 0.5;
        sum += texture2D(uTexture, vUv + vec2( 0.0,  0.0) * texel) * 1.0;
        sum += texture2D(uTexture, vUv + vec2( 1.0,  0.0) * texel) * 0.5;
        sum += texture2D(uTexture, vUv + vec2(-1.0,  1.0) * texel) * 0.25;
        sum += texture2D(uTexture, vUv + vec2( 0.0,  1.0) * texel) * 0.5;
        sum += texture2D(uTexture, vUv + vec2( 1.0,  1.0) * texel) * 0.25;
        sum /= 4.0;
        
        // Unsharp mask
        vec4 original = texture2D(uTexture, vUv);
        vec4 diff = original - sum;
        gl_FragColor = original + diff * uAmount;
    }
`;

<<<<<<< HEAD
export interface WebGLProcessorOptions {
    maxTextureSize?: number;
    /** سقف البكسلات — ميزانية ذاكرة فوق سقف البعد/النص */
    maxPixels?: number;
    enhanceAmount?: number;
}

/** عدد الأعطال الحقيقية المتتالية قبل تعطيل WebGL للجلسة */
const MAX_CONSECUTIVE_GPU_FAILURES = 2;

=======
const RENDER_TO_OUTPUT_FRAGMENT = `
    precision highp float;
    uniform sampler2D uTexture;
    varying vec2 vUv;
    
    void main() {
        gl_FragColor = texture2D(uTexture, vUv);
    }
`;

export interface WebGLProcessorOptions {
    maxTextureSize?: number;
    enhanceAmount?: number;
}

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
export class WebGLProcessor {
    private gl: WebGLRenderingContext | WebGL2RenderingContext | null = null;
    private canvas: HTMLCanvasElement | null = null;
    private warpProgram: WebGLProgram | null = null;
    private enhanceProgram: WebGLProgram | null = null;
<<<<<<< HEAD
    private positionBuffer: WebGLBuffer | null = null;
    private options: WebGLProcessorOptions;
    private consecutiveGpuFailures = 0;
    private gpuDisabled = false;
    private readonly onContextLost = () => this.handleContextLost();
=======
    private renderProgram: WebGLProgram | null = null;
    private positionBuffer: WebGLBuffer | null = null;
    private texture: WebGLTexture | null = null;
    private framebuffer: WebGLFramebuffer | null = null;
    private tempTexture: WebGLTexture | null = null;
    private options: WebGLProcessorOptions;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    constructor(options: WebGLProcessorOptions = {}) {
        this.options = {
            maxTextureSize: 4096,
            enhanceAmount: 0.3,
            ...options,
        };
    }

<<<<<<< HEAD
    /** هل WebGL معطّل نهائياً لهذه الجلسة بعد أعطال متتالية */
    isPermanentlyDisabled(): boolean {
        return this.gpuDisabled;
    }

    /**
     * تسجيل نتيجة process. gpuFault=false يعني رفض مدخلات (زوايا/ميزانية)
     * وهو لا يدخل في عدّاد الأعطال.
     */
    private finish(gpuFault: boolean, reason: string): null {
        if (gpuFault) {
            this.consecutiveGpuFailures++;
            if (this.consecutiveGpuFailures >= MAX_CONSECUTIVE_GPU_FAILURES && !this.gpuDisabled) {
                this.gpuDisabled = true;
                console.warn(
                    `WebGL: ${this.consecutiveGpuFailures} أعطال متتالية — تعطيل WebGL للجلسة، التحويل إلى fallback`,
                );
            }
        }
        console.warn(`WebGL: ${reason}`);
        return null;
    }

    private handleContextLost(): void {
        this.resetGLState();
        this.consecutiveGpuFailures = 0;
    }

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    private createShader(type: number, source: string): WebGLShader | null {
        if (!this.gl) return null;
        const shader = this.gl.createShader(type);
        if (!shader) return null;

        this.gl.shaderSource(shader, source);
        this.gl.compileShader(shader);

        if (!this.gl.getShaderParameter(shader, this.gl.COMPILE_STATUS)) {
            console.error('Shader compile error:', this.gl.getShaderInfoLog(shader));
            this.gl.deleteShader(shader);
            return null;
        }

        return shader;
    }

    private createProgram(vertexSource: string, fragmentSource: string): WebGLProgram | null {
        if (!this.gl) return null;

        const vertexShader = this.createShader(this.gl.VERTEX_SHADER, vertexSource);
        const fragmentShader = this.createShader(this.gl.FRAGMENT_SHADER, fragmentSource);

<<<<<<< HEAD
        // إذا فشل أيٌّ منهما نحذف الآخر لمنع تسرب موارد GPU
        if (!vertexShader || !fragmentShader) {
            if (vertexShader) this.gl.deleteShader(vertexShader);
            if (fragmentShader) this.gl.deleteShader(fragmentShader);
            return null;
        }

        const program = this.gl.createProgram();
        if (!program) {
            this.gl.deleteShader(vertexShader);
            this.gl.deleteShader(fragmentShader);
            return null;
        }
=======
        if (!vertexShader || !fragmentShader) return null;

        const program = this.gl.createProgram();
        if (!program) return null;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

        this.gl.attachShader(program, vertexShader);
        this.gl.attachShader(program, fragmentShader);
        this.gl.linkProgram(program);

        if (!this.gl.getProgramParameter(program, this.gl.LINK_STATUS)) {
            console.error('Program link error:', this.gl.getProgramInfoLog(program));
            this.gl.deleteProgram(program);
<<<<<<< HEAD
            this.gl.deleteShader(vertexShader);
            this.gl.deleteShader(fragmentShader);
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            return null;
        }

        // Clean up shaders after linking
        this.gl.deleteShader(vertexShader);
        this.gl.deleteShader(fragmentShader);

        return program;
    }

    private initGL(): boolean {
<<<<<<< HEAD
        if (this.gl) {
            // فحص context loss: وجود gl لا يعني أن context لا يزال حيًّا
            if (!this.gl.isContextLost()) return true;
            // Context ضاع — نعيد تعيين المراجع ثم نُعيد التهيئة كاملاً
            this.resetGLState();
        }
        if (typeof document === 'undefined') return false;
=======
        if (this.gl) return true;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

        this.canvas = document.createElement('canvas');
        this.canvas.width = 1;
        this.canvas.height = 1;

        const gl = this.canvas.getContext('webgl2') || this.canvas.getContext('webgl');
        if (!gl) return false;

        this.gl = gl;
<<<<<<< HEAD
        this.canvas.addEventListener('webglcontextlost', this.onContextLost);

        // Create shaders (2 passes فقط — لا يوجد render-to-canvas pass)
        this.warpProgram = this.createProgram(WARP_VERTEX_SHADER, WARP_FRAGMENT_SHADER);
        this.enhanceProgram = this.createProgram(WARP_VERTEX_SHADER, ENHANCE_FRAGMENT_SHADER);

        if (!this.warpProgram || !this.enhanceProgram) {
=======

        // Create shaders
        this.warpProgram = this.createProgram(WARP_VERTEX_SHADER, WARP_FRAGMENT_SHADER);
        this.enhanceProgram = this.createProgram(WARP_VERTEX_SHADER, ENHANCE_FRAGMENT_SHADER);
        this.renderProgram = this.createProgram(WARP_VERTEX_SHADER, RENDER_TO_OUTPUT_FRAGMENT);

        if (!this.warpProgram || !this.enhanceProgram || !this.renderProgram) {
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            this.destroy();
            return false;
        }

        // Create position buffer (fullscreen quad)
<<<<<<< HEAD
        this.positionBuffer = gl.createBuffer();
        if (!this.positionBuffer) {
            this.destroy();
            return false;
        }
        gl.bindBuffer(gl.ARRAY_BUFFER, this.positionBuffer);
        gl.bufferData(
            gl.ARRAY_BUFFER,
            new Float32Array([-1, -1, 1, -1, -1, 1, 1, 1]),
            gl.STATIC_DRAW,
=======
        this.positionBuffer = this.gl.createBuffer();
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.positionBuffer);
        this.gl.bufferData(
            this.gl.ARRAY_BUFFER,
            new Float32Array([-1, -1, 1, -1, -1, 1, 1, 1]),
            this.gl.STATIC_DRAW,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        );

        return true;
    }

<<<<<<< HEAD
    /** إعادة تعيين جميع مراجع GL بدون GL calls — للاستخدام بعد context loss أو destroy. */
    private resetGLState(): void {
        if (this.canvas) {
            this.canvas.removeEventListener('webglcontextlost', this.onContextLost);
        }
        this.gl = null;
        this.canvas = null;
        this.warpProgram = null;
        this.enhanceProgram = null;
        this.positionBuffer = null;
=======
    private createTextureFromImageData(imageData: ImageData): WebGLTexture | null {
        if (!this.gl) return null;

        const texture = this.gl.createTexture();
        if (!texture) return null;

        this.gl.bindTexture(this.gl.TEXTURE_2D, texture);
        this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_WRAP_S, this.gl.CLAMP_TO_EDGE);
        this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_WRAP_T, this.gl.CLAMP_TO_EDGE);
        this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_MIN_FILTER, this.gl.LINEAR);
        this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_MAG_FILTER, this.gl.LINEAR);
        this.gl.texImage2D(
            this.gl.TEXTURE_2D,
            0,
            this.gl.RGBA,
            this.gl.RGBA,
            this.gl.UNSIGNED_BYTE,
            imageData,
        );

        return texture;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    }

    private bindTexture(texture: WebGLTexture, unit: number): void {
        if (!this.gl) return;
        this.gl.activeTexture(this.gl.TEXTURE0 + unit);
        this.gl.bindTexture(this.gl.TEXTURE_2D, texture);
    }

<<<<<<< HEAD
    /** false إذا لم يُعثر على aPosition — الرسم عندها ينتج صورة فارغة بلا خطأ GL ظاهر */
    private setupVertexAttribs(program: WebGLProgram): boolean {
        if (!this.gl || !this.positionBuffer) return false;

        const posLoc = this.gl.getAttribLocation(program, 'aPosition');
        if (posLoc < 0) return false;

        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.positionBuffer);
        this.gl.enableVertexAttribArray(posLoc);
        this.gl.vertexAttribPointer(posLoc, 2, this.gl.FLOAT, false, 0, 0);
        return true;
    }

    /**
     * يُرسم من inputTexture إلى outputTexture عبر framebuffer.
     * يتحقق من اكتمال الـframebuffer قبل الرسم — لا نرسم على target غير صالح.
     */
=======
    private setupVertexAttribs(program: WebGLProgram): void {
        if (!this.gl || !this.positionBuffer) return;

        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.positionBuffer);
        const posLoc = this.gl.getAttribLocation(program, 'aPosition');
        this.gl.enableVertexAttribArray(posLoc);
        this.gl.vertexAttribPointer(posLoc, 2, this.gl.FLOAT, false, 0, 0);
    }

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    private renderToTexture(
        program: WebGLProgram,
        inputTexture: WebGLTexture,
        outputTexture: WebGLTexture,
<<<<<<< HEAD
        framebuffer: WebGLFramebuffer,
        width: number,
        height: number,
        uniforms?: Record<string, { type: string; value: number | number[] | Float32Array }>,
    ): boolean {
        if (!this.gl) return false;
        const gl = this.gl;

        gl.bindFramebuffer(gl.FRAMEBUFFER, framebuffer);
        gl.framebufferTexture2D(
            gl.FRAMEBUFFER,
            gl.COLOR_ATTACHMENT0,
            gl.TEXTURE_2D,
=======
        width: number,
        height: number,
        uniforms?: Record<string, { type: string; value: number | number[] }>,
    ): boolean {
        if (!this.gl) return false;

        this.gl.bindFramebuffer(this.gl.FRAMEBUFFER, this.framebuffer);
        this.gl.framebufferTexture2D(
            this.gl.FRAMEBUFFER,
            this.gl.COLOR_ATTACHMENT0,
            this.gl.TEXTURE_2D,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            outputTexture,
            0,
        );

<<<<<<< HEAD
        // فحص اكتمال الـframebuffer قبل الرسم
        const status = gl.checkFramebufferStatus(gl.FRAMEBUFFER);
        if (status !== gl.FRAMEBUFFER_COMPLETE) {
            console.warn('WebGL: incomplete framebuffer:', status);
            return false;
        }

        gl.viewport(0, 0, width, height);
        gl.clear(gl.COLOR_BUFFER_BIT);

        gl.useProgram(program);
        if (!this.setupVertexAttribs(program)) {
            console.warn('WebGL: aPosition attribute missing');
            return false;
        }

        this.bindTexture(inputTexture, 0);
        const texLoc = gl.getUniformLocation(program, 'uTexture');
        if (texLoc) gl.uniform1i(texLoc, 0);

        if (uniforms) {
            for (const [name, uniform] of Object.entries(uniforms)) {
                const loc = gl.getUniformLocation(program, name);
                if (!loc) continue;

                if (uniform.type === '1f') {
                    gl.uniform1f(loc, uniform.value as number);
                } else if (uniform.type === '2f') {
                    const [x, y] = uniform.value as number[];
                    gl.uniform2f(loc, x, y);
                } else if (uniform.type === 'Matrix3fv' && uniform.value instanceof Float32Array) {
                    gl.uniformMatrix3fv(loc, false, uniform.value);
=======
        this.gl.viewport(0, 0, width, height);
        this.gl.clear(this.gl.COLOR_BUFFER_BIT);

        this.gl.useProgram(program);
        this.setupVertexAttribs(program);

        this.bindTexture(inputTexture, 0);
        const texLoc = this.gl.getUniformLocation(program, 'uTexture');
        if (texLoc) this.gl.uniform1i(texLoc, 0);

        if (uniforms) {
            for (const [name, uniform] of Object.entries(uniforms)) {
                const loc = this.gl.getUniformLocation(program, name);
                if (!loc) continue;

                if (uniform.type === '1f') {
                    this.gl.uniform1f(loc, uniform.value as number);
                } else if (uniform.type === '2f') {
                    const [x, y] = uniform.value as number[];
                    this.gl.uniform2f(loc, x, y);
                } else if (uniform.type === '2fv' && Array.isArray(uniform.value)) {
                    this.gl.uniform2fv(loc, uniform.value);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                }
            }
        }

<<<<<<< HEAD
        gl.drawArrays(gl.TRIANGLE_STRIP, 0, 4);

        // الرسم قد يفشل بصمت (فشل سائق أو attrib غير مربوط) وتبقى الـframebuffer
        // بمحتوى clear الشفاف — تُقرأ عندها صورة سوداء وتُعتبر نجاحاً. لا بد من الفحص.
        const error = gl.getError();
        if (error !== gl.NO_ERROR) {
            console.warn('WebGL: drawArrays failed, error code:', error);
            return false;
        }

        return true;
    }

    /**
     * قراءة بكسلات الـframebuffer الحالي مع فحص خطأ GL.
     * ترجع null إذا فشل readPixels — المستدعي ينتقل إلى CPU fallback.
     */
    private readPixels(width: number, height: number): PixelBuffer | null {
        if (!this.gl) return null;

        const pixels = createPixelBuffer(width * height * 4);
        this.gl.readPixels(0, 0, width, height, this.gl.RGBA, this.gl.UNSIGNED_BYTE, pixels);

        const error = this.gl.getError();
        if (error !== this.gl.NO_ERROR) {
            console.warn('WebGL readPixels failed, error code:', error);
            return null;
        }

        return pixels;
    }

    /**
     * قلب الصفوف in-place بدل تخصيص buffer ثانٍ بحجم كامل (~64 MB).
     * المؤقت هو سطر واحد فقط (width × 4 bytes ~ 16 KB لـ4096px).
     * readPixels يعيد السطر السفلي أولاً (WebGL convention) — نحوّل إلى top-down.
     */
    private static flipRowsInPlace(data: Uint8ClampedArray, width: number, height: number): void {
        const rowSize = width * 4;
        const tmp = new Uint8ClampedArray(rowSize);
        for (let y = 0; y < Math.floor(height / 2); y++) {
            const topOffset = y * rowSize;
            const bottomOffset = (height - 1 - y) * rowSize;
            tmp.set(data.subarray(topOffset, topOffset + rowSize));
            data.copyWithin(topOffset, bottomOffset, bottomOffset + rowSize);
            data.set(tmp, bottomOffset);
        }
    }

=======
        this.gl.drawArrays(this.gl.TRIANGLE_STRIP, 0, 4);

        return this.gl.checkFramebufferStatus(this.gl.FRAMEBUFFER) === this.gl.FRAMEBUFFER_COMPLETE;
    }

    private readPixels(width: number, height: number): Uint8ClampedArray | null {
        if (!this.gl) return null;

        const pixels = new Uint8ClampedArray(width * height * 4);
        this.gl.readPixels(0, 0, width, height, this.gl.RGBA, this.gl.UNSIGNED_BYTE, pixels);
        return pixels;
    }

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    process(
        srcData: Uint8ClampedArray,
        srcWidth: number,
        srcHeight: number,
        corners: { x: number; y: number }[],
<<<<<<< HEAD
    ): ProcessResult | null {
        if (this.gpuDisabled) {
            console.warn('WebGL: معطّل لهذه الجلسة بعد أعطال متتالية');
            return null;
        }

        if (!this.initGL() || !this.gl) {
            return this.finish(true, 'init failed');
        }

        const [tl, tr, br, bl] = corners;

        // رفض الزوايا المعيبة → null يُشغّل CPU fallback بدل صورة سوداء/تالفة
        if (validateCorners(corners) !== null) {
            return this.finish(false, 'invalid corners');
        }

        if (
            !Number.isFinite(srcWidth) || !Number.isFinite(srcHeight) ||
            srcWidth < 1 || srcHeight < 1 ||
            srcData.length < srcWidth * srcHeight * 4
        ) {
            return this.finish(false, 'source buffer does not match given dimensions');
        }

        // حد GPU الفعلي — يُعلي على قيمة options إذا كانت أكبر مما يدعمه الـcontext
        const actualMaxTexture = this.gl.getParameter(this.gl.MAX_TEXTURE_SIZE) as number;
        const effectiveMaxTexture = Math.min(
            this.options.maxTextureSize ?? actualMaxTexture,
            actualMaxTexture,
        );

        // فحص أبعاد المصدر قبل رفع texture — texImage2D يفشل بصمت إذا تجاوز GPU limit
        if (srcWidth > effectiveMaxTexture || srcHeight > effectiveMaxTexture) {
            return this.finish(false, `source exceeds GPU texture limit (${srcWidth}x${srcHeight} > ${effectiveMaxTexture})`);
        }

=======
    ): { data: Uint8ClampedArray; width: number; height: number } | null {
        if (!this.initGL() || !this.gl) return null;

        const [tl, tr, br, bl] = corners;

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        // Calculate output dimensions
        const wTop = Math.hypot(tr.x - tl.x, tr.y - tl.y);
        const wBot = Math.hypot(br.x - bl.x, br.y - bl.y);
        const outW = Math.round((wTop + wBot) / 2);
        const hL = Math.hypot(bl.x - tl.x, bl.y - tl.y);
        const hR = Math.hypot(br.x - tr.x, br.y - tr.y);
        const outH = Math.round((hL + hR) / 2);

        if (outW < 1 || outH < 1) {
<<<<<<< HEAD
            return this.finish(false, 'degenerate output size');
        }

        // Scale to fit effective texture limit + memory budget (no upscale)
        const { width: w, height: h } = fitToBudget(outW, outH, {
            maxDimension: effectiveMaxTexture,
            maxPixels: this.options.maxPixels ?? MAX_ALLOWED_PIXELS,
        });

        if ((srcWidth > 1 || srcHeight > 1) && (w <= 1 || h <= 1)) {
            return this.finish(false, "produced 1x1 output for large source image");
        }

        // منظور بالإحداثيات المعيارية: الصورة تُرفع بترتيب الصفوف (t=0 = أعلى صف) بدون UNPACK_FLIP_Y،
        // وv=1 أعلى المخرج framebuffer ثم يقلب صفوف readPixels — زاوية tl هي الأولى عند v=1 (topLeftV=1)
        const normalizedCorners = [
            { x: tl.x / srcWidth, y: tl.y / srcHeight },
            { x: tr.x / srcWidth, y: tr.y / srcHeight },
            { x: br.x / srcWidth, y: br.y / srcHeight },
            { x: bl.x / srcWidth, y: bl.y / srcHeight },
        ];
        const homo = computeHomography(normalizedCorners, 1);
        if (!homo) {
            return this.finish(false, 'homography degenerate');
        }

        // uniformMatrix3fv تتوقع ترتيب أعمدة (column-major) والمصفوفة صف-كأصل
        const homoGL = new Float32Array([
            homo[0], homo[3], homo[6],
            homo[1], homo[4], homo[7],
            homo[2], homo[5], homo[8],
        ]);

        // موارد GPU محلية — تُنظَّف دائماً في finally حتى عند الاستثناءات
        let srcTexture: WebGLTexture | null = null;
        let tempTexture: WebGLTexture | null = null;
        let enhancedTexture: WebGLTexture | null = null;
        let framebuffer: WebGLFramebuffer | null = null;

        try {
            const gl = this.gl;

            // Upload source texture
            // upload مباشر من المخزن بلا ImageData: نسخة كاملة (37 MB عند 3072²) بلا فائدة
            srcTexture = gl.createTexture();
            if (!srcTexture) return this.finish(true, 'createTexture failed for source');
            gl.bindTexture(gl.TEXTURE_2D, srcTexture);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
            gl.texImage2D(
                gl.TEXTURE_2D,
                0,
                gl.RGBA,
                srcWidth,
                srcHeight,
                0,
                gl.RGBA,
                gl.UNSIGNED_BYTE,
                srcData,
            );

            // فحص خطأ رفع texture المصدر — OOM أو INVALID_VALUE يفشل بصمت بدون هذا الفحص
            if (gl.getError() !== gl.NO_ERROR) {
                return this.finish(true, 'failed to upload source texture (OOM or invalid size?)');
            }

            // Create warp output texture
            tempTexture = gl.createTexture();
            if (!tempTexture) return this.finish(true, 'createTexture failed for warp output');
            gl.bindTexture(gl.TEXTURE_2D, tempTexture);
            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, w, h, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
            if (gl.getError() !== gl.NO_ERROR) {
                return this.finish(true, 'failed to allocate warp output texture');
            }

            // Create enhance output texture
            enhancedTexture = gl.createTexture();
            if (!enhancedTexture) return this.finish(true, 'createTexture failed for enhance output');
            gl.bindTexture(gl.TEXTURE_2D, enhancedTexture);
            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, w, h, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
            gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
            if (gl.getError() !== gl.NO_ERROR) {
                return this.finish(true, 'failed to allocate enhance output texture');
            }

            // Create framebuffer — createFramebuffer يمكن أن يعيد null؛ ربط null = default framebuffer
            framebuffer = gl.createFramebuffer();
            if (!framebuffer) {
                return this.finish(true, 'createFramebuffer failed');
            }

            // Step 1: Warp (srcTexture → tempTexture)
            if (!this.renderToTexture(this.warpProgram!, srcTexture, tempTexture, framebuffer, w, h, {
                uHomography: { type: 'Matrix3fv', value: homoGL },
            })) return this.finish(true, 'warp pass failed');

            // Step 2: Enhance (tempTexture → enhancedTexture)
            // الـframebuffer يبقى مربوطاً بـenhancedTexture بعد هذه الخطوة
            if (!this.renderToTexture(this.enhanceProgram!, tempTexture, enhancedTexture, framebuffer, w, h, {
                uResolution: { type: '2f', value: [w, h] },
                uAmount: { type: '1f', value: this.options.enhanceAmount ?? 0.3 },
            })) return this.finish(true, 'enhance pass failed');

            // Step 3: قراءة النتيجة مباشرة من FBO — لا يوجد render-to-canvas pass
            // الـFBO لا يزال مربوطاً بـenhancedTexture بعد Step 2
            const resultData = this.readPixels(w, h);
            if (!resultData) return this.finish(true, 'readPixels failed');

            // قلب الصفوف in-place: readPixels = bottom-up → top-down
            // يستخدم مؤقت سطر واحد (~16 KB) بدل نسخة كاملة (~64 MB)
            WebGLProcessor.flipRowsInPlace(resultData, w, h);

            this.consecutiveGpuFailures = 0;
            return { data: resultData, width: w, height: h };
        } catch (error) {
            console.error('WebGL processing error:', error);
            return this.finish(true, 'unexpected exception');
        } finally {
            // تنظيف موارد GPU دائماً — حتى عند الاستثناءات أو المسارات المبكرة
            const gl = this.gl;
            if (gl) {
                if (srcTexture) gl.deleteTexture(srcTexture);
                if (tempTexture) gl.deleteTexture(tempTexture);
                if (enhancedTexture) gl.deleteTexture(enhancedTexture);
                if (framebuffer) gl.deleteFramebuffer(framebuffer);
                gl.bindFramebuffer(gl.FRAMEBUFFER, null);
            }
=======
            return { data: new Uint8ClampedArray(srcData), width: srcWidth, height: srcHeight };
        }

        // Scale to fit max texture size
        const scale = Math.min(1, this.options.maxTextureSize! / Math.max(outW, outH));
        const w = Math.max(1, Math.round(outW * scale));
        const h = Math.max(1, Math.round(outH * scale));

        // Normalize corners to 0-1 range
        const normalizedCorners = [
            tl.x / srcWidth, 1 - tl.y / srcHeight,
            tr.x / srcWidth, 1 - tr.y / srcHeight,
            br.x / srcWidth, 1 - br.y / srcHeight,
            bl.x / srcWidth, 1 - bl.y / srcHeight,
        ];

        try {
            // Create source texture - copy data to ensure proper ArrayBuffer type
            const srcDataCopy = new Uint8ClampedArray(srcData);
            const srcImageData = new ImageData(srcDataCopy, srcWidth, srcHeight);
            const srcTexture = this.createTextureFromImageData(srcImageData);
            if (!srcTexture) return null;

            // Create output texture for warp
            this.tempTexture = this.gl.createTexture();
            if (!this.tempTexture) {
                this.gl.deleteTexture(srcTexture);
                return null;
            }
            this.gl.bindTexture(this.gl.TEXTURE_2D, this.tempTexture);
            this.gl.texImage2D(
                this.gl.TEXTURE_2D,
                0,
                this.gl.RGBA,
                w,
                h,
                0,
                this.gl.RGBA,
                this.gl.UNSIGNED_BYTE,
                null,
            );
            this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_WRAP_S, this.gl.CLAMP_TO_EDGE);
            this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_WRAP_T, this.gl.CLAMP_TO_EDGE);
            this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_MIN_FILTER, this.gl.LINEAR);
            this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_MAG_FILTER, this.gl.LINEAR);

            // Create framebuffer
            this.framebuffer = this.gl.createFramebuffer();

            // Step 1: Warp
            if (
                !this.renderToTexture(this.warpProgram!, srcTexture, this.tempTexture, w, h, {
                    uCorners: { type: '2fv', value: normalizedCorners },
                    uResolution: { type: '2f', value: [srcWidth, srcHeight] },
                })
            ) {
                this.gl.deleteTexture(srcTexture);
                this.gl.deleteTexture(this.tempTexture);
                this.gl.deleteFramebuffer(this.framebuffer);
                return null;
            }

            // Step 2: Enhance
            const enhancedTexture = this.gl.createTexture();
            if (!enhancedTexture) {
                this.gl.deleteTexture(srcTexture);
                this.gl.deleteTexture(this.tempTexture);
                this.gl.deleteFramebuffer(this.framebuffer);
                return null;
            }
            this.gl.bindTexture(this.gl.TEXTURE_2D, enhancedTexture);
            this.gl.texImage2D(
                this.gl.TEXTURE_2D,
                0,
                this.gl.RGBA,
                w,
                h,
                0,
                this.gl.RGBA,
                this.gl.UNSIGNED_BYTE,
                null,
            );
            this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_WRAP_S, this.gl.CLAMP_TO_EDGE);
            this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_WRAP_T, this.gl.CLAMP_TO_EDGE);
            this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_MIN_FILTER, this.gl.LINEAR);
            this.gl.texParameteri(this.gl.TEXTURE_2D, this.gl.TEXTURE_MAG_FILTER, this.gl.LINEAR);

            if (
                !this.renderToTexture(this.enhanceProgram!, this.tempTexture, enhancedTexture, w, h, {
                    uResolution: { type: '2f', value: [w, h] },
                    uAmount: { type: '1f', value: this.options.enhanceAmount! },
                })
            ) {
                this.gl.deleteTexture(srcTexture);
                this.gl.deleteTexture(this.tempTexture);
                this.gl.deleteTexture(enhancedTexture);
                this.gl.deleteFramebuffer(this.framebuffer);
                return null;
            }

            // Step 3: Render to screen (to read pixels)
            this.gl.bindFramebuffer(this.gl.FRAMEBUFFER, null);
            this.gl.viewport(0, 0, w, h);
            this.gl.clear(this.gl.COLOR_BUFFER_BIT);

            this.gl.useProgram(this.renderProgram);
            this.setupVertexAttribs(this.renderProgram!);

            this.bindTexture(enhancedTexture, 0);
            const texLoc = this.gl.getUniformLocation(this.renderProgram!, 'uTexture');
            if (texLoc) this.gl.uniform1i(texLoc, 0);

            this.gl.drawArrays(this.gl.TRIANGLE_STRIP, 0, 4);

            // Read result
            const resultData = this.readPixels(w, h);

            // Cleanup
            this.gl.deleteTexture(srcTexture);
            this.gl.deleteTexture(this.tempTexture);
            this.gl.deleteTexture(enhancedTexture);
            this.gl.deleteFramebuffer(this.framebuffer);
            this.tempTexture = null;
            this.framebuffer = null;

            if (!resultData) return null;

            // Flip Y (WebGL reads bottom-up)
            const flippedData = new Uint8ClampedArray(resultData.length);
            const rowSize = w * 4;
            for (let y = 0; y < h; y++) {
                const srcRow = (h - 1 - y) * rowSize;
                const dstRow = y * rowSize;
                flippedData.set(resultData.subarray(srcRow, srcRow + rowSize), dstRow);
            }

            return { data: flippedData, width: w, height: h };
        } catch (error) {
            console.error('WebGL processing error:', error);
            return null;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        }
    }

    destroy(): void {
        if (this.gl) {
            if (this.warpProgram) this.gl.deleteProgram(this.warpProgram);
            if (this.enhanceProgram) this.gl.deleteProgram(this.enhanceProgram);
<<<<<<< HEAD
            if (this.positionBuffer) this.gl.deleteBuffer(this.positionBuffer);
=======
            if (this.renderProgram) this.gl.deleteProgram(this.renderProgram);
            if (this.positionBuffer) this.gl.deleteBuffer(this.positionBuffer);
            if (this.texture) this.gl.deleteTexture(this.texture);
            if (this.tempTexture) this.gl.deleteTexture(this.tempTexture);
            if (this.framebuffer) this.gl.deleteFramebuffer(this.framebuffer);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

            const loseContext = this.gl.getExtension('WEBGL_lose_context');
            if (loseContext) {
                loseContext.loseContext();
            }
        }

<<<<<<< HEAD
        this.gpuDisabled = false;
        this.consecutiveGpuFailures = 0;
        this.resetGLState();
=======
        this.gl = null;
        this.canvas = null;
        this.warpProgram = null;
        this.enhanceProgram = null;
        this.renderProgram = null;
        this.positionBuffer = null;
        this.texture = null;
        this.tempTexture = null;
        this.framebuffer = null;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    }
}
