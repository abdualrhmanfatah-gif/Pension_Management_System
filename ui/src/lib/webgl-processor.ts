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

export class WebGLProcessor {
    private gl: WebGLRenderingContext | WebGL2RenderingContext | null = null;
    private canvas: HTMLCanvasElement | null = null;
    private warpProgram: WebGLProgram | null = null;
    private enhanceProgram: WebGLProgram | null = null;
    private renderProgram: WebGLProgram | null = null;
    private positionBuffer: WebGLBuffer | null = null;
    private texture: WebGLTexture | null = null;
    private framebuffer: WebGLFramebuffer | null = null;
    private tempTexture: WebGLTexture | null = null;
    private options: WebGLProcessorOptions;

    constructor(options: WebGLProcessorOptions = {}) {
        this.options = {
            maxTextureSize: 4096,
            enhanceAmount: 0.3,
            ...options,
        };
    }

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

        if (!vertexShader || !fragmentShader) return null;

        const program = this.gl.createProgram();
        if (!program) return null;

        this.gl.attachShader(program, vertexShader);
        this.gl.attachShader(program, fragmentShader);
        this.gl.linkProgram(program);

        if (!this.gl.getProgramParameter(program, this.gl.LINK_STATUS)) {
            console.error('Program link error:', this.gl.getProgramInfoLog(program));
            this.gl.deleteProgram(program);
            return null;
        }

        // Clean up shaders after linking
        this.gl.deleteShader(vertexShader);
        this.gl.deleteShader(fragmentShader);

        return program;
    }

    private initGL(): boolean {
        if (this.gl) return true;

        this.canvas = document.createElement('canvas');
        this.canvas.width = 1;
        this.canvas.height = 1;

        const gl = this.canvas.getContext('webgl2') || this.canvas.getContext('webgl');
        if (!gl) return false;

        this.gl = gl;

        // Create shaders
        this.warpProgram = this.createProgram(WARP_VERTEX_SHADER, WARP_FRAGMENT_SHADER);
        this.enhanceProgram = this.createProgram(WARP_VERTEX_SHADER, ENHANCE_FRAGMENT_SHADER);
        this.renderProgram = this.createProgram(WARP_VERTEX_SHADER, RENDER_TO_OUTPUT_FRAGMENT);

        if (!this.warpProgram || !this.enhanceProgram || !this.renderProgram) {
            this.destroy();
            return false;
        }

        // Create position buffer (fullscreen quad)
        this.positionBuffer = this.gl.createBuffer();
        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.positionBuffer);
        this.gl.bufferData(
            this.gl.ARRAY_BUFFER,
            new Float32Array([-1, -1, 1, -1, -1, 1, 1, 1]),
            this.gl.STATIC_DRAW,
        );

        return true;
    }

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
    }

    private bindTexture(texture: WebGLTexture, unit: number): void {
        if (!this.gl) return;
        this.gl.activeTexture(this.gl.TEXTURE0 + unit);
        this.gl.bindTexture(this.gl.TEXTURE_2D, texture);
    }

    private setupVertexAttribs(program: WebGLProgram): void {
        if (!this.gl || !this.positionBuffer) return;

        this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.positionBuffer);
        const posLoc = this.gl.getAttribLocation(program, 'aPosition');
        this.gl.enableVertexAttribArray(posLoc);
        this.gl.vertexAttribPointer(posLoc, 2, this.gl.FLOAT, false, 0, 0);
    }

    private renderToTexture(
        program: WebGLProgram,
        inputTexture: WebGLTexture,
        outputTexture: WebGLTexture,
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
            outputTexture,
            0,
        );

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
                }
            }
        }

        this.gl.drawArrays(this.gl.TRIANGLE_STRIP, 0, 4);

        return this.gl.checkFramebufferStatus(this.gl.FRAMEBUFFER) === this.gl.FRAMEBUFFER_COMPLETE;
    }

    private readPixels(width: number, height: number): Uint8ClampedArray | null {
        if (!this.gl) return null;

        const pixels = new Uint8ClampedArray(width * height * 4);
        this.gl.readPixels(0, 0, width, height, this.gl.RGBA, this.gl.UNSIGNED_BYTE, pixels);
        return pixels;
    }

    process(
        srcData: Uint8ClampedArray,
        srcWidth: number,
        srcHeight: number,
        corners: { x: number; y: number }[],
    ): { data: Uint8ClampedArray; width: number; height: number } | null {
        if (!this.initGL() || !this.gl) return null;

        const [tl, tr, br, bl] = corners;

        // Calculate output dimensions
        const wTop = Math.hypot(tr.x - tl.x, tr.y - tl.y);
        const wBot = Math.hypot(br.x - bl.x, br.y - bl.y);
        const outW = Math.round((wTop + wBot) / 2);
        const hL = Math.hypot(bl.x - tl.x, bl.y - tl.y);
        const hR = Math.hypot(br.x - tr.x, br.y - tr.y);
        const outH = Math.round((hL + hR) / 2);

        if (outW < 1 || outH < 1) {
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
        }
    }

    destroy(): void {
        if (this.gl) {
            if (this.warpProgram) this.gl.deleteProgram(this.warpProgram);
            if (this.enhanceProgram) this.gl.deleteProgram(this.enhanceProgram);
            if (this.renderProgram) this.gl.deleteProgram(this.renderProgram);
            if (this.positionBuffer) this.gl.deleteBuffer(this.positionBuffer);
            if (this.texture) this.gl.deleteTexture(this.texture);
            if (this.tempTexture) this.gl.deleteTexture(this.tempTexture);
            if (this.framebuffer) this.gl.deleteFramebuffer(this.framebuffer);

            const loseContext = this.gl.getExtension('WEBGL_lose_context');
            if (loseContext) {
                loseContext.loseContext();
            }
        }

        this.gl = null;
        this.canvas = null;
        this.warpProgram = null;
        this.enhanceProgram = null;
        this.renderProgram = null;
        this.positionBuffer = null;
        this.texture = null;
        this.tempTexture = null;
        this.framebuffer = null;
    }
}
