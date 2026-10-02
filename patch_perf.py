import os

file_path = r'f:\Projects\Pension_Management_System\ui\src\components\ui\FileUploader.tsx'
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix 1: Optimize compressImage
old_compress = '''// ─── Helper: Compress image if it exceeds max size ─────

async function compressImage(file: File, maxSizeMB: number): Promise<File> {
    // Only compress images
    if (!file.type.startsWith('image/') || file.type === 'image/gif') return file;

    const maxBytes = maxSizeMB * 1024 * 1024;

    // If already under limit, return as-is
    if (file.size <= maxBytes) return file;

    // Determine target format (keep original if supported by canvas toBlob, else fallback to jpeg)
    const targetType = (file.type === 'image/png' || file.type === 'image/webp') ? file.type : 'image/jpeg';
    const targetExt = targetType === 'image/png' ? '.png' : targetType === 'image/webp' ? '.webp' : '.jpg';

    return new Promise<File>((resolve) => {
        const img = new Image();
        const url = URL.createObjectURL(file);

        img.onload = () => {
            URL.revokeObjectURL(url);

            // Dimension tiers to try (progressively smaller)
            const dimensionTiers = [2048, 1600, 1280, 1024, 800];
            // Quality levels to try at each dimension tier
            const qualityLevels = [0.8, 0.65, 0.5, 0.35, 0.2];

            const tryCompress = (dimIdx: number, qualIdx: number) => {
                const maxDim = dimensionTiers[dimIdx];
                let { width, height } = img;

                // Scale down to fit within maxDim
                if (width > maxDim || height > maxDim) {
                    const ratio = Math.min(maxDim / width, maxDim / height);
                    width = Math.round(width * ratio);
                    height = Math.round(height * ratio);
                }

                const canvas = document.createElement('canvas');
                canvas.width = width;
                canvas.height = height;
                const ctx = canvas.getContext('2d')!;
                ctx.drawImage(img, 0, 0, width, height);

                const quality = qualityLevels[qualIdx];

                canvas.toBlob(
                    (blob) => {
                        if (!blob) { resolve(file); return; }

                        if (blob.size <= maxBytes) {
                            // Success — fits under the limit
                            const compressed = new File(
                                [blob],
                                file.name.replace(/\.[^.]+$/, targetExt),
                                { type: targetType }
                            );
                            resolve(compressed);
                        } else if (qualIdx < qualityLevels.length - 1) {
                            // Try lower quality at same dimension
                            tryCompress(dimIdx, qualIdx + 1);
                        } else if (dimIdx < dimensionTiers.length - 1) {
                            // Move to smaller dimension, reset quality
                            tryCompress(dimIdx + 1, 0);
                        } else {
                            // All options exhausted, return best effort
                            const compressed = new File(
                                [blob],
                                file.name.replace(/\.[^.]+$/, targetExt),
                                { type: targetType }
                            );
                            resolve(compressed);
                        }
                    },
                    targetType,
                    quality
                );
            };

            // Start with first tier that's smaller than the original
            const startDimIdx = dimensionTiers.findIndex(
                d => d < Math.max(img.width, img.height)
            );
            tryCompress(Math.max(0, startDimIdx), 0);
        };

        img.onerror = () => {
            URL.revokeObjectURL(url);
            console.error("Failed to compress image", { fileName: file.name, fileType: file.type, fileSize: file.size });
            resolve(file); // fallback: return original
        };

        img.src = url;
    });
}'''

new_compress = '''// ─── Helper: Compress image if it exceeds max size ─────

async function compressImage(file: File, maxSizeMB: number): Promise<File> {
    // Only compress images
    if (!file.type.startsWith('image/') || file.type === 'image/gif') return file;

    const maxBytes = maxSizeMB * 1024 * 1024;

    // If already under limit, return as-is
    if (file.size <= maxBytes) return file;

    // Determine target format (keep original if supported by canvas toBlob, else fallback to jpeg)
    const targetType = (file.type === 'image/png' || file.type === 'image/webp') ? file.type : 'image/jpeg';
    const targetExt = targetType === 'image/png' ? '.png' : targetType === 'image/webp' ? '.webp' : '.jpg';

    return new Promise<File>((resolve) => {
        const img = new Image();
        const url = URL.createObjectURL(file);

        img.onload = () => {
            URL.revokeObjectURL(url);

            // Dimension tiers to try (progressively smaller)
            const dimensionTiers = [2048, 1600, 1280, 1024, 800];
            
            // Reduced quality levels for jpeg/webp. PNG ignores quality so it only gets 1 level.
            const qualityLevels = targetType === 'image/png' ? [1.0] : [0.8, 0.5, 0.2];

            const tryCompress = (dimIdx: number, qualIdx: number, cachedCanvas?: HTMLCanvasElement) => {
                let currentCanvas = cachedCanvas;
                
                // Create and draw canvas only once per dimension
                if (!currentCanvas) {
                    const maxDim = dimensionTiers[dimIdx];
                    let { width, height } = img;

                    // Scale down to fit within maxDim
                    if (width > maxDim || height > maxDim) {
                        const ratio = Math.min(maxDim / width, maxDim / height);
                        width = Math.round(width * ratio);
                        height = Math.round(height * ratio);
                    }

                    currentCanvas = document.createElement('canvas');
                    currentCanvas.width = width;
                    currentCanvas.height = height;
                    const ctx = currentCanvas.getContext('2d')!;
                    ctx.drawImage(img, 0, 0, width, height);
                }

                const quality = qualityLevels[qualIdx];

                currentCanvas.toBlob(
                    (blob) => {
                        if (!blob) { resolve(file); return; }

                        if (blob.size <= maxBytes) {
                            // Success — fits under the limit
                            const compressed = new File(
                                [blob],
                                file.name.replace(/\.[^.]+$/, targetExt),
                                { type: targetType }
                            );
                            resolve(compressed);
                        } else if (qualIdx < qualityLevels.length - 1) {
                            // Try lower quality with SAME canvas
                            tryCompress(dimIdx, qualIdx + 1, currentCanvas);
                        } else if (dimIdx < dimensionTiers.length - 1) {
                            // Move to smaller dimension, reset quality and clear canvas
                            tryCompress(dimIdx + 1, 0);
                        } else {
                            // All options exhausted, return best effort
                            const compressed = new File(
                                [blob],
                                file.name.replace(/\.[^.]+$/, targetExt),
                                { type: targetType }
                            );
                            resolve(compressed);
                        }
                    },
                    targetType,
                    quality
                );
            };

            // Start with first tier that's smaller than or equal to the original
            const startDimIdx = Math.max(0, dimensionTiers.findIndex(d => d <= Math.max(img.width, img.height)));
            tryCompress(startDimIdx, 0);
        };

        img.onerror = () => {
            URL.revokeObjectURL(url);
            console.error("Failed to compress image", { fileName: file.name, fileType: file.type, fileSize: file.size });
            resolve(file); // fallback: return original
        };

        img.src = url;
    });
}'''

content = content.replace(old_compress, new_compress)

old_val = '''    const validateFinalFile = useCallback(async (file: File): Promise<File> => {
        if (accept && !isFileAccepted(file, accept)) {
            throw new Error("نوع الملف غير مدعوم أو غير مسموح به");
        }
        let out = file;
        if (maxSizeMB && out.type.startsWith("image/") && out.size > maxSizeMB * 1024 * 1024) {
            out = await compressImage(out, maxSizeMB);
            if (accept && !isFileAccepted(out, accept)) {
                throw new Error("نوع الملف الناتج بعد الضغط غير مدعوم");
            }
        }
        if (maxSizeMB && out.size > maxSizeMB * 1024 * 1024) {
            throw new Error(حجم الملف يجب أن لا يتجاوز  ميغابايت حتى بعد محاولة الضغط);
        }
        return out;
    }, [accept, maxSizeMB]);'''

new_val = '''    const validateFinalFile = useCallback(async (file: File): Promise<File> => {
        if (accept && !isFileAccepted(file, accept)) {
            throw new Error("نوع الملف غير مدعوم أو غير مسموح به");
        }
        let out = file;
        if (maxSizeMB && out.type.startsWith("image/") && out.size > maxSizeMB * 1024 * 1024) {
            const start = performance.now();
            out = await compressImage(out, maxSizeMB);
            const end = performance.now();
            
            if (import.meta.env.DEV) {
                console.log([FileUploader] compressImage took ms | Before: MB | After: MB);
            }

            if (accept && !isFileAccepted(out, accept)) {
                throw new Error("نوع الملف الناتج بعد الضغط غير مدعوم");
            }
        }
        if (maxSizeMB && out.size > maxSizeMB * 1024 * 1024) {
            throw new Error(حجم الملف يجب أن لا يتجاوز  ميغابايت حتى بعد محاولة الضغط);
        }
        return out;
    }, [accept, maxSizeMB]);'''

content = content.replace(old_val, new_val)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Patch applied for performance")
