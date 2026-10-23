import { useEffect, useRef, useState } from "react";

import { Button, Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui";
import {
    type AcceptedCapture,
    useCameraScanSession,
} from "@/hooks/useCameraScanSession";
import type { ScanMachineConfig } from "@/lib/document-scanner/session-machine";

export interface CameraScanDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onAccept: (capture: AcceptedCapture) => void;
    config?: ScanMachineConfig;
}

const STATE_HINT: Record<string, string> = {
    initializing: "جارٍ تجهيز المحرك والكاميرا...",
    searching: "وجّه الكاميرا نحو الوثيقة",
    stabilizing: "اثبت... جارٍ تأكيد الوثيقة",
    processing: "جارٍ معالجة اللقطة...",
    waitingRemoval: "أزل الوثيقة من الإطار وضع التالية",
    paused: "الجلسة متوقفة مؤقتًا",
    ended: "انتهت الجلسة",
};

function quadPoints(overlay: { quad: { points: { x: number; y: number }[] } }): string {
    return overlay.quad.points.map((p) => `${p.x},${p.y}`).join(" ");
}

function handleStreamError(_message: string): void {
    // الخطأ معروض أصلًا عبر streamError من الخطاف — لا تنبيه مكرر هنا
}

/**
 * واجهة جلسة المسح (م3): عرض حي + حدود المضلع + عدّاد. القرار كله في الخطاف والآلة.
 */
export function CameraScanDialog({ open, onOpenChange, onAccept, config }: CameraScanDialogProps) {
    const videoRef = useRef<HTMLVideoElement | null>(null);
    const [videoSize, setVideoSize] = useState({ width: 0, height: 0 });

    const { snapshot, overlay, streamError, manualCapture, pause, resume, end, switchCamera, capped } =
        useCameraScanSession(videoRef, open, { onAccept, onError: handleStreamError }, config);

    useEffect(() => {
        if (!open) setVideoSize({ width: 0, height: 0 });
    }, [open]);

    const busy = snapshot.state === "processing" || snapshot.state === "initializing";
    const paused = snapshot.state === "paused";
    const viewBox = overlay ? `0 0 ${overlay.width} ${overlay.height}` : undefined;

    return (
        <Dialog open={open} onOpenChange={(v) => !v && (end(), onOpenChange(false))}>
            <DialogContent size="lg" dir="rtl" className="flex max-h-[90dvh] flex-col p-0">
                <DialogHeader className="shrink-0 border-b border-border px-3 py-2 sm:px-6 sm:py-4">
                    <DialogTitle className="text-base sm:text-xl">مسح الوثائق بالكاميرا</DialogTitle>
                </DialogHeader>

                <div className="flex min-h-0 flex-1 flex-col gap-2 overflow-auto p-2 sm:p-4">
                    <div
                        className="relative w-full overflow-hidden rounded-xl bg-black"
                        style={videoSize.width > 0 ? { aspectRatio: `${videoSize.width} / ${videoSize.height}` } : undefined}
                    >
                        <video
                            ref={videoRef}
                            playsInline
                            muted
                            onLoadedMetadata={(e) => {
                                const v = e.currentTarget;
                                if (v.videoWidth > 0) setVideoSize({ width: v.videoWidth, height: v.videoHeight });
                            }}
                            className="h-full w-full object-contain"
                        />
                        {overlay && viewBox && (
                            <svg viewBox={viewBox} preserveAspectRatio="none" className="pointer-events-none absolute inset-0 h-full w-full">
                                <polygon points={quadPoints(overlay)} fill="none" strokeWidth={4} className="stroke-emerald-400" />
                            </svg>
                        )}
                    </div>

                    <div aria-live="polite" className="flex items-center justify-between gap-2 text-sm">
                        <span className="font-medium">{capped ? "اكتمل الحد الأقصى للجلسة. أنهِ التصوير لرفع الوثائق." : STATE_HINT[snapshot.state] ?? ""}</span>
                        <span className="text-muted-foreground">المقبولة: {snapshot.acceptedCount}{capped ? " (الحد)" : ""}</span>
                    </div>

                    {streamError && <p role="alert" className="text-sm font-medium text-destructive">{streamError}</p>}

                    <div className="flex flex-wrap gap-2">
                        <Button type="button" onClick={manualCapture} disabled={busy || paused || capped || !!streamError} className="min-h-[44px]">
                            التقاط يدوي
                        </Button>
                        {paused ? (
                            <Button type="button" variant="outline" onClick={resume} className="min-h-[44px]">
                                استئناف
                            </Button>
                        ) : (
                            <Button type="button" variant="outline" onClick={pause} disabled={busy} className="min-h-[44px]">
                                إيقاف مؤقت
                            </Button>
                        )}
                        <Button type="button" variant="outline" onClick={switchCamera} disabled={busy} className="min-h-[44px]">
                            تبديل الكاميرا
                        </Button>
                        <Button type="button" variant="ghost" onClick={() => { end(); onOpenChange(false); }} className="min-h-[44px]">
                            إنهاء التصوير
                        </Button>
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    );
}
