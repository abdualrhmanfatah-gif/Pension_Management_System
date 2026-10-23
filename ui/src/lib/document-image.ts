export interface ImageSettings {
    brightness: number;
    contrast: number;
    sharpness: number;
    rotation: 0 | 90 | 180 | 270;
}

export function rotateCorners(
    corners: { x: number; y: number }[],
    rotation: 0 | 90 | 180 | 270,
    naturalW: number,
    naturalH: number,
): { x: number; y: number }[] {
    switch (rotation) {
        case 90:  return corners.map(c => ({ x: naturalH - c.y, y: c.x }));
        case 180: return corners.map(c => ({ x: naturalW - c.x, y: naturalH - c.y }));
        case 270: return corners.map(c => ({ x: c.y, y: naturalW - c.x }));
        default:  return corners;
    }
}

export function canvasToFile(canvas: Pick<HTMLCanvasElement, "toBlob">, originalName: string): Promise<File> {
    return new Promise((resolve, reject) => {
        canvas.toBlob((blob) => {
            if (!blob) {
                reject(new Error("فشل تحويل Canvas"));
                return;
            }
            // Strip GUID prefix added by LocalFileStorageService (e.g. "guid_original.jpg")
            const cleaned = originalName.replace(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}_/i, "");
            const baseName = cleaned.replace(/\.[^.]+$/, "");
            const name = baseName.endsWith("_scanned") ? `${baseName}.jpg` : `${baseName}_scanned.jpg`;
            resolve(new File([blob], name, { type: "image/jpeg" }));
        }, "image/jpeg", 0.85);
    });
}

export function applySharpen(
    ctx: CanvasRenderingContext2D,
    width: number,
    height: number,
    amount: number,
) {
    const imageData = ctx.getImageData(0, 0, width, height);
    const data = imageData.data;
    const original = new Uint8ClampedArray(data);

    const kernel = [
        0, -1, 0,
        -1, 5, -1,
        0, -1, 0,
    ];

    for (let y = 1; y < height - 1; y++) {
        for (let x = 1; x < width - 1; x++) {
            for (let c = 0; c < 3; c++) {
                let sum = 0;
                for (let ky = -1; ky <= 1; ky++) {
                    for (let kx = -1; kx <= 1; kx++) {
                        const idx = ((y + ky) * width + (x + kx)) * 4 + c;
                        sum += original[idx] * kernel[(ky + 1) * 3 + (kx + 1)];
                    }
                }
                const idx = (y * width + x) * 4 + c;
                data[idx] = Math.min(255, Math.max(0, original[idx] + (sum - original[idx]) * amount));
            }
        }
    }

    ctx.putImageData(imageData, 0, 0);
}

export function renderSettingsImage(
    ctx: CanvasRenderingContext2D,
    img: HTMLImageElement,
    settings: ImageSettings,
    scale: number,
): void {
    const canvas = ctx.canvas;
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.save();

    ctx.translate(canvas.width / 2, canvas.height / 2);
    ctx.rotate((settings.rotation * Math.PI) / 180);
    ctx.scale(scale, scale);
    ctx.drawImage(img, -img.width / 2, -img.height / 2);
    ctx.restore();

    const brightness = settings.brightness / 100;
    const contrast = settings.contrast / 100;
    ctx.filter = `brightness(${brightness}) contrast(${contrast})`;
    ctx.drawImage(canvas, 0, 0);
    ctx.filter = "none";

    if (settings.sharpness > 0) {
        applySharpen(ctx, canvas.width, canvas.height, settings.sharpness / 100);
    }
}
