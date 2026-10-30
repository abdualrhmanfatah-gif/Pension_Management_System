<<<<<<< HEAD
import { ExternalLink, Eye, FileIcon, X } from "lucide-react";
import { useState } from "react";

=======
import { FileIcon, Eye, ExternalLink, X } from "lucide-react";
import { useState } from "react";
import { getDocumentUrl } from "@/lib/utils";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { 
    Button, 
    Dialog, 
    DialogContent, 
    DialogHeader, 
    DialogTitle,
    Heading,
    ScrollArea
} from "@/components/ui";
<<<<<<< HEAD
import { getDocumentUrl } from "@/lib/utils";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface ReviewFilePreviewProps {
    path: string;
    label?: string;
}

/**
 * Modern file preview component for the review system.
 * Robust URL construction and inline preview.
 */
export function ReviewFilePreview({ path, label }: ReviewFilePreviewProps) {
    const [isOpen, setIsOpen] = useState(false);

    if (!path || path === "—" || path.length < 3) return null;
    
    // Check if it's a valid path (contains extension or slashes)
    if (!path.includes('.') && !path.includes('/') && !path.includes('\\')) return null;

    const isImage = /\.(jpg|jpeg|png|webp|gif|bmp)$/i.test(path);
    const isPdf = /\.pdf$/i.test(path);
    
    // Standardize URL construction (Ensure root-relative paths start with /)
    const fullUrl = path.startsWith('http') 
        ? path 
        : getDocumentUrl(path) ?? `/${path}`;

    return (
        <>
            <div className="flex flex-col gap-2 p-2 bg-muted/30 rounded-xl border border-border transition-colors">
                {label && <span className="text-[10px] sm:text-xs font-bold text-muted-foreground mb-1 px-1">{label}</span>}
                <div className="flex items-center gap-3 flex-row-reverse">
                    {isImage ? (
                        <div 
                            className="relative group overflow-hidden rounded-lg border border-border w-12 h-12 shrink-0 bg-background cursor-pointer" 
                            onClick={() => setIsOpen(true)}
                        >
                            <img 
                                src={fullUrl} 
                                className="w-full h-full object-cover transition-transform group-hover:scale-105" 
                                alt="preview"
                                loading="lazy"
                                onError={(e) => {
                                    // Fallback if image fails to load
                                    const target = e.target as HTMLImageElement;
                                    target.style.display = 'none';
                                    target.parentElement!.classList.add('bg-red-50');
                                }}
                            />
                            <div className="absolute inset-0 bg-black/40 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                                <Eye className="w-4 h-4 text-white" />
                            </div>
                        </div>
                    ) : (
                        <div 
<<<<<<< HEAD
                            className="w-12 h-12 rounded-lg bg-primary/5 border border-primary flex items-center justify-center shrink-0 cursor-pointer" 
                            onClick={() => setIsOpen(true)}
                        >
                            <FileIcon className="w-5 h-5 text-primary" />
                        </div>
                    )}
                    <div className="min-w-0 flex-1 text-start">
                        <p className="text-[10px] sm:text-xs text-muted-foreground truncate font-mono opacity-100" dir="ltr">
=======
                            className="w-12 h-12 rounded-lg bg-primary/5 border border-primary/10 flex items-center justify-center shrink-0 cursor-pointer" 
                            onClick={() => setIsOpen(true)}
                        >
                            <FileIcon className="w-5 h-5 text-primary/40" />
                        </div>
                    )}
                    <div className="min-w-0 flex-1 text-start">
                        <p className="text-[10px] sm:text-xs text-muted-foreground truncate font-mono opacity-60" dir="ltr">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            {path.split(/[/\\]/).pop()}
                        </p>
                        <Button 
                            variant="link"
                            size="sm"
                            className="h-auto p-0 text-[10px] sm:text-xs font-bold text-primary gap-1 mt-0.5 justify-start"
                            onClick={() => setIsOpen(true)}
                        >
                            {isImage ? 'عرض الصورة' : 'معاينة الملف'} <ExternalLink className="w-3 h-3 ms-1" />
                        </Button>
                    </div>
                </div>
            </div>

            {/* Inline Preview Dialog */}
            <Dialog open={isOpen} onOpenChange={setIsOpen}>
                <DialogContent className="max-w-[95vw] sm:max-w-4xl h-[85dvh] p-0 overflow-hidden flex flex-col gap-0 rounded-2xl border-2">
                    <DialogHeader className="p-4 border-b bg-muted/20 flex flex-row items-center justify-between space-y-0">
                        <DialogTitle className="text-base font-bold flex items-center gap-2">
                            {isImage ? <Eye className="w-5 h-5 text-primary" /> : <FileIcon className="w-5 h-5 text-primary" />}
                            معاينة الوثيقة
                        </DialogTitle>
                        <Button variant="ghost" size="sm" className="min-h-[44px] min-w-[44px] p-0 rounded-full" onClick={() => setIsOpen(false)}>
                            <X className="w-4 h-4" />
                        </Button>
                    </DialogHeader>
                    
                    <div className="flex-1 bg-zinc-900/5 dark:bg-black/20 relative overflow-hidden">
                        <ScrollArea className="h-full w-full">
                            <div className="flex items-center justify-center min-h-full p-4 md:p-8 text-center">
                                {isImage ? (
                                    <img 
                                        src={fullUrl} 
                                        className="max-w-full h-auto rounded-lg border bg-background" 
                                        alt="full preview"
                                        loading="lazy"
                                    />
                                ) : isPdf ? (
                                    <iframe 
                                        src={`${fullUrl}#toolbar=0`} 
                                        className="w-full h-[70dvh] rounded-lg border bg-background "
                                        title="pdf-preview"
                                    />
                                ) : (
                                    <div className="p-12 bg-background rounded-3xl border  space-y-4 max-w-sm mx-auto">
                                        <div className="w-20 h-20 bg-primary/10 rounded-full flex items-center justify-center mx-auto">
                                            <FileIcon className="w-10 h-10 text-primary" />
                                        </div>
                                        <Heading level="h3" className="font-bold text-lg">لا تتوفر معاينة مباشرة</Heading>
                                        <p className="text-sm text-muted-foreground">يمكنك فتح الملف في نافذة جديدة أو تحميله.</p>
                                        <Button className="rounded-xl w-full font-bold" onClick={() => window.open(fullUrl, '_blank')}>
                                            فتح في نافذة جديدة <ExternalLink className="w-4 h-4 ms-2" />
                                        </Button>
                                    </div>
                                )}
                            </div>
                        </ScrollArea>
                    </div>

                    <div className="p-3 border-t bg-muted/20 flex flex-wrap items-center justify-between gap-3">
                        <div className="flex flex-col">
                            <span className="text-[10px] sm:text-xs font-bold text-muted-foreground uppercase">المسار الفني</span>
<<<<<<< HEAD
                            <code className="text-[10px] sm:text-xs bg-background/50 px-2 py-0.5 rounded border border-border max-w-[300px] sm:max-w-none truncate">{path}</code>
=======
                            <code className="text-[10px] sm:text-xs bg-background/50 px-2 py-0.5 rounded border border-border/50 max-w-[300px] sm:max-w-none truncate">{path}</code>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        </div>
                        <div className="flex items-center gap-2">
                            <Button variant="outline" size="sm" className="rounded-lg h-11 font-bold" onClick={() => window.open(fullUrl, '_blank')}>
                                فتح في نافذة مستقلة <ExternalLink className="w-3.5 h-3.5 ms-1.5" />
                            </Button>
                        </div>
                    </div>
                </DialogContent>
            </Dialog>
        </>
    );
}
