import { Camera } from "lucide-react";
import { useCallback, useEffect, useRef,useState } from "react";

import { BaseFormDialog } from "@/components/ui/BaseFormDialog";
import { Button } from "@/components/ui/Button";
import { FileUploader } from "@/components/ui/FileUploader";
import { cn } from "@/lib/utils";

export interface EditableImageProps {
    /** The image URL (or null if empty) */
    src: string | null;
    /** Alt text for the image */
    alt?: string;
    /** Container class name (for sizing, borders, etc.) */
    className?: string;
    /** Fallback icon rendered when no image exists */
    fallbackIcon?: React.ReactNode;
    /** Tooltip for the camera button */
    title?: string;
    /** Whether the user has permission to edit the image */
    canEdit?: boolean;
    /** The actual async upload function. Returns the new image URL for instant update. Required when canEdit is true. */
    onUpload?: (file: File) => Promise<string | void>;
    /** Max file size in MB */
    maxSizeMB?: number;
    /** Size variant for the camera icon. Use 'sm' for small avatars (e.g. tables). */
    size?: "sm" | "md" | "lg";
    /** Explicit image dimensions for CLS prevention */
    imgWidth?: number;
    imgHeight?: number;
}

const KEY_PREFIX = "editable-image-src-override";

const getStoredSrc = (propsSrc: string | null): string | null => {
    if (!propsSrc) return null;
    try {
        const stored = sessionStorage.getItem(`${KEY_PREFIX}-${propsSrc}`);
        return stored || null;
    } catch {
        return null;
    }
};

const setStoredSrc = (propsSrc: string | null, value: string | null) => {
    if (!propsSrc) return;
    try {
        if (value) {
            sessionStorage.setItem(`${KEY_PREFIX}-${propsSrc}`, value);
        } else {
            sessionStorage.removeItem(`${KEY_PREFIX}-${propsSrc}`);
        }
    } catch {}
};

export function EditableImage({
    src,
    alt = "صورة",
    className,
    fallbackIcon,
    title = "تحديث الصورة",
    canEdit = true,
    onUpload,
    maxSizeMB = 2,
    size = "md",
    imgWidth,
    imgHeight,
}: EditableImageProps) {
    const [uploadDialogOpen, setUploadDialogOpen] = useState(false);
    const [uploadFile, setUploadFile] = useState<File | string | null>(null);
    const [uploadError, setUploadError] = useState<string | null>(null);
    const [imageHash, setImageHash] = useState<number>(Date.now());
    const [isUploading, setIsUploading] = useState(false);
    const [srcOverride, setSrcOverride] = useState<string | null>(null);
    const propsSrcRef = useRef(src);
    useEffect(() => { propsSrcRef.current = src; }, [src]);

    const isValidFile = (file: File) => {
        const allowedTypes = ["image/jpeg", "image/png", "image/jpg"];
        const maxSizeBytes = maxSizeMB * 1024 * 1024;

        if (!allowedTypes.includes(file.type)) {
            setUploadError("صيغة الملف غير مدعومة. يرجى اختيار صورة بجودة عالية.");
            return false;
        }
        if (file.size > maxSizeBytes) {
            setUploadError(`حجم الصورة كبير جداً. الحد الأقصى هو ${maxSizeMB} ميجابايت.`);
            return false;
        }
        setUploadError(null);
        return true;
    };

    const handleUpload = useCallback(async () => {
        if (!uploadFile) return;
        if (typeof uploadFile === "string") {
            setUploadDialogOpen(false);
            return;
        }

        try {
            setIsUploading(true);
            const result = await onUpload?.(uploadFile);
            if (typeof result === "string" && result) {
                setSrcOverride(result);
            } else {
                setImageHash(Date.now());
            }
            setUploadDialogOpen(false);
            setUploadFile(null);
        } catch {
            console.error("Upload failed");
        } finally {
            setIsUploading(false);
        }
    }, [uploadFile, onUpload]);

    // Load srcOverride from sessionStorage on mount
    useEffect(() => {
        if (src) {
            const stored = getStoredSrc(src);
            if (stored) {
                setSrcOverride(stored);
            }
        }
    }, [src]);

    // Persist srcOverride to sessionStorage
    useEffect(() => {
        if (propsSrcRef.current && srcOverride) {
            setStoredSrc(propsSrcRef.current, srcOverride);
        } else if (propsSrcRef.current) {
            setStoredSrc(propsSrcRef.current, null);
        }
    }, [srcOverride, src]);

    // Cleanup on unmount
    useEffect(() => {
        return () => {
            if (propsSrcRef.current) {
                setStoredSrc(propsSrcRef.current, null);
            }
        };
    }, []);

    const activeSrc = srcOverride || (src ? `${src}?h=${imageHash}` : null);

    return (
        <>
            <div className="group relative inline-flex flex-col items-center">
                {activeSrc ? (
                    <img
                        src={activeSrc}
                        alt={alt}
                        width={imgWidth}
                        height={imgHeight}
                        className={cn("object-cover transition-all duration-300", canEdit && "cursor-pointer", className)}
                        onClick={canEdit ? () => { setUploadFile(src); setUploadDialogOpen(true); } : undefined}
                    />
                ) : (
                    <div 
                        className={cn("flex items-center justify-center bg-muted/50 text-muted-foreground transition-all duration-300", className, canEdit && "cursor-pointer")}
                        onClick={canEdit ? () => { setUploadFile(null); setUploadDialogOpen(true); } : undefined}
                    >
                        {fallbackIcon}
                    </div>
                )}
                
                {canEdit && (
                    <Button
                        variant="primary"
                        onClick={() => { setUploadFile(src); setUploadDialogOpen(true); }}
                        className={cn(
                            "absolute rounded-full border-2 border-card transition-all active:scale-95",
                            "hover:bg-primary/90 hover:scale-110",
                            size === "sm" ? "-bottom-1 -end-1 w-6 h-6 p-1" : 
                            size === "lg" ? "-bottom-2 -end-2 w-12 h-12 min-h-[44px] min-w-[44px] p-3" : 
                            "-bottom-1 -end-1 w-9 h-9 min-h-[44px] min-w-[44px] p-2.5"
                        )}
                        title={title}
                    >
                        <Camera className={cn(
                            size === "sm" ? "w-3 h-3" :
                            size === "lg" ? "w-5 h-5" :
                            "w-4 h-4"
                        )} />
                    </Button>
                )}
            </div>

            <BaseFormDialog
                isOpen={uploadDialogOpen}
                onClose={() => {
                    if (!isUploading) {
                        setUploadDialogOpen(false);
                        setUploadFile(null);
                        setUploadError(null);
                    }
                }}
                title={title}
                description={`يرجى اختيار صورة واضحة (JPG, PNG). الحد الأقصى ${maxSizeMB} ميجابايت.`}
                onSubmit={handleUpload}
                isLoading={isUploading}
                submitLabel="حفظ والتحديث"
            >
                <div className="space-y-4">
                    <FileUploader
                        label={alt}
                        accept="image/*"
                        value={uploadFile}
                        onChange={(f) => {
                            if (f && isValidFile(f)) {
                                setUploadFile(f);
                            } else if (!f) {
                                setUploadFile(null);
                            }
                        }}
                        error={uploadError || undefined}
                    />
                </div>
            </BaseFormDialog>
        </>
    );
}
