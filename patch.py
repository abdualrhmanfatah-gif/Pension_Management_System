import os

file_path = r'f:\Projects\Pension_Management_System\ui\src\components\ui\FileUploader.tsx'
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix 1: compressImage type retention
content = content.replace('''
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
                                file.name.replace(/\.[^.]+$/, '.jpg'),
                                { type: 'image/jpeg' }
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
                                file.name.replace(/\.[^.]+$/, '.jpg'),
                                { type: 'image/jpeg' }
                            );
                            resolve(compressed);
                        }
                    },
                    'image/jpeg',
                    quality
                );
            };

            // Start with first tier that's smaller than the original
            const startDimIdx = dimensionTiers.findIndex(
                d => d < Math.max(img.width, img.height)
            );
            tryCompress(Math.max(0, startDimIdx), 0);
        };
'''.strip(), '''
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
'''.strip())

# Fix 2: scannerConfig
content = content.replace('    scannerConfig = DEFAULT_SCANNER_CONFIG,', '    scannerConfig: userScannerConfig,')
content = content.replace('''    const isMobile = useMemo(() => isMobileDevice(), []);''', '''    // Merge scanner config properties with defaults
    const scannerConfig = useMemo(() => {
        return { ...DEFAULT_SCANNER_CONFIG, ...userScannerConfig };
    }, [userScannerConfig]);

    // Detect mobile once
    const isMobile = useMemo(() => isMobileDevice(), []);''')

# Fix 3: validateFinalFile
content = content.replace('''    const validateFinalFile = useCallback(async (file: File): Promise<File> => {
        if (accept && !isFileAccepted(file, accept)) {
            throw new Error("نوع الملف الناتج غير مدعوم أو غير مسموح به");
        }
        let out = file;
        if (maxSizeMB && out.type.startsWith("image/") && out.size > maxSizeMB * 1024 * 1024) {
            out = await compressImage(out, maxSizeMB);
        }
        if (maxSizeMB && out.size > maxSizeMB * 1024 * 1024) {
            throw new Error(حجم الملف يجب أن لا يتجاوز  ميغابايت);
        }
        return out;
    }, [accept, maxSizeMB]);''', '''    const validateFinalFile = useCallback(async (file: File): Promise<File> => {
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
    }, [accept, maxSizeMB]);''')

# Fix 4: handleFileSelect validation
content = content.replace('''        if (file) {
            // Validate Extension / MIME Type
            if (accept && !isFileAccepted(file, accept)) {
                toast.error("نوع الملف غير مدعوم أو غير مسموح به");
                if (fileInputRef.current) fileInputRef.current.value = "";
                return;
            }

            // Auto-compress images that exceed the size limit
            if (maxSizeMB && file.type.startsWith('image/') && file.size > maxSizeMB * 1024 * 1024) {
                file = await compressImage(file, maxSizeMB);
            }

            // Final size check (for non-image files or if compression wasn't enough)
            if (maxSizeMB && file.size > maxSizeMB * 1024 * 1024) {
                toast.error(حجم الملف يجب أن لا يتجاوز  ميغابايت);
                if (fileInputRef.current) fileInputRef.current.value = "";
                return;
            }

            onChange?.(file);
        }''', '''        if (file) {
            try {
                const validated = await validateFinalFile(file);
                onChange?.(validated);
            } catch (err: unknown) {
                toast.error(err instanceof Error ? err.message : "حدث خطأ أثناء فحص الملف");
            }
        }''')

# Fix 5: handleMobileCameraCapture
content = content.replace('''        if (file) {
            if (accept && !isFileAccepted(file, accept)) {
                toast.error("صيغة الملف غير مدعومة");
                if (cameraInputRef.current) cameraInputRef.current.value = "";
                return;
            }

            // Auto-compress camera images (mobile cameras produce very large files)
            if (maxSizeMB && file.size > maxSizeMB * 1024 * 1024) {
                file = await compressImage(file, maxSizeMB);
            }

            // Final size check
            if (maxSizeMB && file.size > maxSizeMB * 1024 * 1024) {
                toast.error(حجم الملف يجب أن لا يتجاوز  ميغابايت حتى بعد الضغط);
                return;
            }
            // فتح crop يدوي مباشر بدون auto-scan
            setPendingCropFile(file);
            setCropOpen(true);
        }''', '''        if (file) {
            try {
                const validated = await validateFinalFile(file);
                setPendingCropFile(validated);
                setCropOpen(true);
            } catch (err: unknown) {
                toast.error(err instanceof Error ? err.message : "حدث خطأ أثناء فحص الصورة");
            }
        }''')

# Fix 6: capturePhoto
content = content.replace('''                canvas.toBlob((blob) => {
                    if (blob) {
                        const file = new File(
                            [blob],
                            capture_.jpg,
                            { type: "image/jpeg" }
                        );
                        handleCloseCamera();
                        // فتح crop يدوي مباشر بدون auto-scan
                        setPendingCropFile(file);
                        setCropOpen(true);
                    }
                }, "image/jpeg", 0.9);''', '''                canvas.toBlob(async (blob) => {
                    if (blob) {
                        const file = new File(
                            [blob],
                            capture_.jpg,
                            { type: "image/jpeg" }
                        );
                        handleCloseCamera();
                        try {
                            const validated = await validateFinalFile(file);
                            setPendingCropFile(validated);
                            setCropOpen(true);
                        } catch (err: unknown) {
                            toast.error(err instanceof Error ? err.message : "الصورة الملتقطة غير مقبولة");
                        }
                    }
                }, "image/jpeg", 0.9);''')
content = content.replace('}, [handleCloseCamera, videoReady]);', '}, [handleCloseCamera, videoReady, validateFinalFile]);')

# Fix 7: handleScan
content = content.replace('''    const handleScan = useCallback(async (options: { useAdf?: boolean, isPassport?: boolean } = {}) => {
        const { useAdf = false, isPassport = false } = options;''', '''    const handleScan = useCallback(async (options: { useAdf?: boolean, isPassport?: boolean } = {}) => {
        const { isPassport = false } = options;''')
content = content.replace('const adfParam = useAdf || scannerConfig.adf ? "&adf=true" : "";', '''const isAdf = options.useAdf ?? scannerConfig.adf ?? false;
            const adfParam = isAdf ? "&adf=true" : "";''')
content = content.replace('const r = await fetch(url, { method: "POST" });', 'const r = await fetch(url, { method: "POST", signal: AbortSignal.timeout(60000) });')

# Fix 8: useMemo -> useEffect
content = content.replace('''    const previewUrlRef = useRef("");

    const previewUrl = useMemo(() => {
        if (!hasFile) return "";
        if (isString) return value as string;
        return URL.createObjectURL(value as File);
    }, [hasFile, isString, value]);

    // Revoke old blob URL when a new one is created; revoke current on unmount
    useEffect(() => {
        if (!isString && previewUrl && previewUrl !== previewUrlRef.current) {
            if (previewUrlRef.current) URL.revokeObjectURL(previewUrlRef.current);
            previewUrlRef.current = previewUrl;
        }
        return () => {
            if (previewUrlRef.current) {
                URL.revokeObjectURL(previewUrlRef.current);
                previewUrlRef.current = "";
            }
        };
    }, [previewUrl, isString]);''', '''    const [previewUrl, setPreviewUrl] = useState("");

    // Setup preview URL and manage Blob URL lifecycle
    useEffect(() => {
        if (!hasFile) {
            setPreviewUrl("");
            return;
        }
        if (isString) {
            setPreviewUrl(value as string);
            return;
        }
        const url = URL.createObjectURL(value as File);
        setPreviewUrl(url);
        
        return () => URL.revokeObjectURL(url);
    }, [hasFile, isString, value]);''')

# Fix 9: ImageCropperDialog
content = content.replace('''                onClose={() => {
                    // Only use the original file if crop wasn't already confirmed
                    if (pendingCropFile && !cropCompletedRef.current) {
                        onChange?.(pendingCropFile);
                    }
                    setPendingCropFile(null);
                    cropCompletedRef.current = false;
                    setCropOpen(false);
                }}
                onCrop={async (croppedFile) => {
                    // م40: إعادة فحص accept/size على ناتج القص قبل التسليم
                    const validated = await validateFinalFile(croppedFile);
                    onChange?.(validated);
                    setPendingCropFile(null);
                    cropCompletedRef.current = true;
                }}''', '''                onClose={() => {
                    // إذا تم إغلاق نافذة القص بدون إتمام وكان هذا ملفاً قيد الانتظار (من الكاميرا مثلا)، فلا نمرره لـ onChange بل نلغيه
                    setPendingCropFile(null);
                    cropCompletedRef.current = false;
                    setCropOpen(false);
                }}
                onCrop={async (croppedFile) => {
                    // م40: إعادة فحص accept/size على ناتج القص قبل التسليم
                    try {
                        const validated = await validateFinalFile(croppedFile);
                        onChange?.(validated);
                    } catch (err: unknown) {
                        toast.error(err instanceof Error ? err.message : "خطأ في نتيجة القص");
                    }
                    setPendingCropFile(null);
                    cropCompletedRef.current = true;
                    setCropOpen(false);
                }}''')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Patch applied")
