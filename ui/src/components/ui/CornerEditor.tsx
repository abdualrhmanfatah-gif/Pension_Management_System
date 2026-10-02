import { useCallback, useEffect, useRef, useState } from "react";



import { validateCorners } from "../../lib/homography";



interface Point {

    x: number;

    y: number;

}



export interface CornerEditorProps {

    imageUrl: string;

    rotation?: 0 | 90 | 180 | 270;

    initialCorners?: Point[] | null;

    corners?: Point[] | null; // controlled mode

    onCornersChange?: (corners: Point[]) => void;

}



const CORNER_RADIUS_DESKTOP = 12;

const CORNER_RADIUS_MOBILE = 16;

const CORNER_HIT_RADIUS_DESKTOP = 24;

const CORNER_HIT_RADIUS_MOBILE = 40;

const MAGNIFIER_SIZE_DESKTOP = 132;

const MAGNIFIER_SIZE_MOBILE = 108;

const MAGNIFIER_ZOOM = 2;

const MAGNIFIER_MARGIN = 8;



export function CornerEditor({ imageUrl, rotation = 0, initialCorners, corners: controlledCorners, onCornersChange }: CornerEditorProps) {

    const containerRef = useRef<HTMLDivElement>(null);

    const canvasRef = useRef<HTMLCanvasElement>(null);

    const [img, setImg] = useState<HTMLImageElement | null>(null);

    const [imgError, setImgError] = useState<string | null>(null);

    const [internalCorners, setInternalCorners] = useState<Point[]>([]);

    const [scale, setScale] = useState(1);

    const [offset, setOffset] = useState({ x: 0, y: 0 });

    const [isTouch, setIsTouch] = useState(false);



    const scaleRef = useRef(1);

    const offsetRef = useRef({ x: 0, y: 0 });

    const imgRef = useRef<HTMLImageElement | null>(null);

    const cornersRef = useRef<Point[]>([]);

    const dragIndexRef = useRef<number | null>(null);

    const magnifierVisibleRef = useRef(false);

    const magnifierPointRef = useRef<Point | null>(null);

    const rafRef = useRef<number>(0);

    const canvasSizeRef = useRef({ w: 0, h: 0 });

    const rotationRef = useRef(rotation);

    // الكتابة في effect لا أثناء الرسم (react-hooks/refs) — مطابقةً لبقية الـrefs

    useEffect(() => { rotationRef.current = rotation; }, [rotation]);



    const isControlled = controlledCorners !== undefined;

    const effectiveCorners: Point[] = isControlled

        ? (controlledCorners ?? [])

        : internalCorners;



    const cornerRadius = isTouch ? CORNER_RADIUS_MOBILE : CORNER_RADIUS_DESKTOP;

    const cornerHitRadius = isTouch ? CORNER_HIT_RADIUS_MOBILE : CORNER_HIT_RADIUS_DESKTOP;

    const cornerRadiusRef = useRef(cornerRadius);

    useEffect(() => { cornerRadiusRef.current = cornerRadius; }, [cornerRadius]);



    useEffect(() => { scaleRef.current = scale; }, [scale]);

    useEffect(() => { offsetRef.current = offset; }, [offset]);

    useEffect(() => { imgRef.current = img; }, [img]);

    useEffect(() => { cornersRef.current = effectiveCorners; }, [effectiveCorners]);



    // Detect touch device

    useEffect(() => {

        const checkTouch = () => {

            setIsTouch(window.matchMedia("(pointer: coarse)").matches || "ontouchstart" in window);

        };

        checkTouch();

        const mql = window.matchMedia("(pointer: coarse)");

        mql.addEventListener("change", checkTouch);

        return () => mql.removeEventListener("change", checkTouch);

    }, []);



    // Load image with race-condition guard and cleanup

    useEffect(() => {

        let cancelled = false;

        dragIndexRef.current = null;

        magnifierVisibleRef.current = false;

        magnifierPointRef.current = null;

        setImgError(null);

        setImg(null);

        imgRef.current = null;



        const image = new Image();

        // لا crossOrigin: أي طلب صورة بوضع CORS يفشل على رابط API المطلق

        // (قيس: crossOrigin="anonymous" و"use-credentials" كلاهما يفشل).

        // يمرّر الاستدعاءون رابط blob: من useDocumentBlob — نفس الأصل.

        image.onload = () => {

            if (cancelled) {

                image.src = "";

                return;

            }

            setImg(image);

        };

        image.onerror = () => {

            if (!cancelled) {

                console.error("CornerEditor: Failed to load image", { imageUrl, src: image.src });

                setImgError("فشل تحميل الصورة");

            }

        };

        image.src = imageUrl;



        return () => {

            cancelled = true;

            image.src = "";

        };

    }, [imageUrl]);



    // Visible dimensions accounting for rotation

    const getVisibleDimensions = useCallback(() => {

        if (!img) return { visW: 0, visH: 0 };

        const isVertical = rotationRef.current === 90 || rotationRef.current === 270;

        return {

            visW: isVertical ? img.naturalHeight : img.naturalWidth,

            visH: isVertical ? img.naturalWidth : img.naturalHeight,

        };

    }, [img]);



    const calculateLayout = useCallback(() => {

        if (!img || !containerRef.current) return null;

        const container = containerRef.current;

        const paddingX = 4;

        const paddingY = 16;

        const availW = container.clientWidth - paddingX * 2;

        const availH = container.clientHeight - paddingY * 2;

        const { visW, visH } = getVisibleDimensions();

        const scaleX = availW / visW;

        const scaleY = availH / visH;

        const s = Math.min(scaleX, scaleY, 1);

        const drawW = visW * s;

        const drawH = visH * s;

        const offX = (container.clientWidth - drawW) / 2;

        const offY = (container.clientHeight - drawH) / 2;

        return { s, offX, offY, drawW, drawH };

    }, [img, getVisibleDimensions]);



    const getDefaultCorners = useCallback((offX: number, offY: number, drawW: number, drawH: number): Point[] => {

        const inset = 0.1;

        return [

            { x: offX + drawW * inset, y: offY + drawH * inset },

            { x: offX + drawW * (1 - inset), y: offY + drawH * inset },

            { x: offX + drawW * (1 - inset), y: offY + drawH * (1 - inset) },

            { x: offX + drawW * inset, y: offY + drawH * (1 - inset) },

        ];

    }, []);



    // Forward rotation: original image coords → display (visible) coords

    const forwardRotatePoint = useCallback((ox: number, oy: number): Point => {

        const rot = rotationRef.current;

        const naturalW = imgRef.current?.naturalWidth ?? 0;

        const naturalH = imgRef.current?.naturalHeight ?? 0;

        switch (rot) {

            case 90:  return { x: naturalH - oy, y: ox };

            case 180: return { x: naturalW - ox, y: naturalH - oy };

            case 270: return { x: oy, y: naturalW - ox };

            default:  return { x: ox, y: oy };

        }

    }, []);



    // Initialize corners when image or rotation changes

    useEffect(() => {

        const layout = calculateLayout();

        if (!layout) return;

        setScale(layout.s);

        scaleRef.current = layout.s;

        setOffset({ x: layout.offX, y: layout.offY });

        offsetRef.current = { x: layout.offX, y: layout.offY };



        if (isControlled && controlledCorners && controlledCorners.length === 4) {

            // controlledCorners are in original image coords — apply forward rotation to display coords

            const next = controlledCorners.map((p) => {

                const dp = forwardRotatePoint(p.x, p.y);

                return {

                    x: layout.offX + dp.x * layout.s,

                    y: layout.offY + dp.y * layout.s,

                };

            });

            cornersRef.current = next;

        } else if (initialCorners && initialCorners.length === 4) {

            // initialCorners are in original image coords — apply forward rotation

            const next = initialCorners.map((p) => {

                const dp = forwardRotatePoint(p.x, p.y);

                return {

                    x: layout.offX + dp.x * layout.s,

                    y: layout.offY + dp.y * layout.s,

                };

            });

            if (!isControlled) setInternalCorners(next);

            cornersRef.current = next;

        } else {

            const next = getDefaultCorners(layout.offX, layout.offY, layout.drawW, layout.drawH);

            if (!isControlled) setInternalCorners(next);

            cornersRef.current = next;

        }

    }, [img, rotation, initialCorners, controlledCorners, calculateLayout, getDefaultCorners, isControlled, forwardRotatePoint]);



    // Draw canvas — applies rotation transform to image

    const drawCanvas = useCallback(() => {

        if (!canvasRef.current || !img || !containerRef.current) return;

        const canvas = canvasRef.current;

        const container = containerRef.current;

        const dpr = window.devicePixelRatio || 1;

        const cssW = container.clientWidth;

        const cssH = container.clientHeight;



        const needResize = canvasSizeRef.current.w !== cssW || canvasSizeRef.current.h !== cssH;

        if (needResize) {

            canvasSizeRef.current = { w: cssW, h: cssH };

            canvas.width = cssW * dpr;

            canvas.height = cssH * dpr;

            canvas.style.width = cssW + "px";

            canvas.style.height = cssH + "px";

        }



        const ctx = canvas.getContext("2d")!;

        ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

        ctx.clearRect(0, 0, cssW, cssH);



        const s = scaleRef.current;

        const off = offsetRef.current;

        const crn = cornersRef.current;

        const rot = rotationRef.current;



        // Draw image with rotation

        const drawW = img.naturalWidth * s;

        const drawH = img.naturalHeight * s;

        const { visW, visH } = getVisibleDimensions();

        const visDrawW = visW * s;

        const visDrawH = visH * s;

        const drawSourceImage = (targetCtx: CanvasRenderingContext2D) => {

            if (rot !== 0) {

                targetCtx.translate(off.x + visDrawW / 2, off.y + visDrawH / 2);

                targetCtx.rotate((rot * Math.PI) / 180);

                targetCtx.drawImage(img, -drawW / 2, -drawH / 2, drawW, drawH);

            } else {

                targetCtx.drawImage(img, off.x, off.y, drawW, drawH);

            }

        };



        ctx.save();

        drawSourceImage(ctx);

        ctx.restore();



        // Overlay

        ctx.save();

        ctx.beginPath();

        ctx.moveTo(crn[0].x, crn[0].y);

        for (let i = 1; i < 4; i++) ctx.lineTo(crn[i].x, crn[i].y);

        ctx.closePath();

        ctx.rect(0, 0, cssW, cssH);

        ctx.fillStyle = "rgba(0, 0, 0, 0.5)";

        ctx.fill("evenodd");

        ctx.restore();



        // Edges

        ctx.strokeStyle = "#3b82f6";

        ctx.lineWidth = 2;

        ctx.setLineDash([6, 4]);

        ctx.beginPath();

        ctx.moveTo(crn[0].x, crn[0].y);

        for (let i = 1; i < 4; i++) ctx.lineTo(crn[i].x, crn[i].y);

        ctx.closePath();

        ctx.stroke();

        ctx.setLineDash([]);



        // Corner handles

        const labels = ["TL", "TR", "BR", "BL"];

        const colors = ["#ef4444", "#22c55e", "#3b82f6", "#eab308"];

        crn.forEach((p, i) => {

            ctx.beginPath();

            ctx.arc(p.x, p.y, cornerRadiusRef.current, 0, Math.PI * 2);

            ctx.fillStyle = colors[i];

            ctx.fill();

            ctx.strokeStyle = "white";

            ctx.lineWidth = 2;

            ctx.stroke();

            ctx.fillStyle = "white";

            ctx.font = "bold 10px sans-serif";

            ctx.textAlign = "center";

            ctx.textBaseline = "middle";

            ctx.fillText(labels[i], p.x, p.y);

        });

        // Magnifier / loupe while dragging a corner

        const dragIndex = dragIndexRef.current;

        const magnifierPoint = magnifierPointRef.current;

        if (magnifierVisibleRef.current && magnifierPoint && dragIndex !== null) {

            const magnifierSize = isTouch ? MAGNIFIER_SIZE_MOBILE : MAGNIFIER_SIZE_DESKTOP;

            const radius = magnifierSize / 2;

            const horizontalOffset = radius + 36;

            const verticalOffset = radius + 52;

            const clamp = (value: number, min: number, max: number) => Math.max(min, Math.min(max, value));

            let loupeX = magnifierPoint.x < cssW / 2

                ? magnifierPoint.x + horizontalOffset

                : magnifierPoint.x - horizontalOffset;

            let loupeY = magnifierPoint.y > cssH / 2

                ? magnifierPoint.y - verticalOffset

                : magnifierPoint.y + verticalOffset;

            loupeX = clamp(loupeX, radius + MAGNIFIER_MARGIN, cssW - radius - MAGNIFIER_MARGIN);

            loupeY = clamp(loupeY, radius + MAGNIFIER_MARGIN, cssH - radius - MAGNIFIER_MARGIN);

            ctx.save();

            ctx.beginPath();

            ctx.arc(loupeX, loupeY, radius, 0, Math.PI * 2);

            ctx.clip();

            ctx.fillStyle = "#111";

            ctx.fillRect(loupeX - radius, loupeY - radius, magnifierSize, magnifierSize);

            ctx.save();

            ctx.translate(loupeX, loupeY);

            ctx.scale(MAGNIFIER_ZOOM, MAGNIFIER_ZOOM);

            ctx.translate(-magnifierPoint.x, -magnifierPoint.y);

            ctx.imageSmoothingEnabled = true;

            ctx.imageSmoothingQuality = "high";

            ctx.save();

            drawSourceImage(ctx);

            ctx.restore();

            // Draw the two crop edges connected to the active corner.

            const previousIndex = (dragIndex + crn.length - 1) % crn.length;

            const nextIndex = (dragIndex + 1) % crn.length;

            ctx.lineCap = "round";

            ctx.lineJoin = "round";

            ctx.beginPath();

            ctx.moveTo(crn[previousIndex].x, crn[previousIndex].y);

            ctx.lineTo(crn[dragIndex].x, crn[dragIndex].y);

            ctx.lineTo(crn[nextIndex].x, crn[nextIndex].y);

            ctx.strokeStyle = "rgba(0, 0, 0, 0.78)";

            ctx.lineWidth = 4 / MAGNIFIER_ZOOM;

            ctx.stroke();

            ctx.strokeStyle = "#3b82f6";

            ctx.lineWidth = 2 / MAGNIFIER_ZOOM;

            ctx.stroke();

            ctx.restore();

            // Crosshair at the exact active-corner coordinate.

            const crosshairHalf = 16;

            const drawCrosshair = (strokeStyle: string, lineWidth: number) => {

                ctx.beginPath();

                ctx.moveTo(loupeX - crosshairHalf, loupeY);

                ctx.lineTo(loupeX + crosshairHalf, loupeY);

                ctx.moveTo(loupeX, loupeY - crosshairHalf);

                ctx.lineTo(loupeX, loupeY + crosshairHalf);

                ctx.strokeStyle = strokeStyle;

                ctx.lineWidth = lineWidth;

                ctx.stroke();

            };

            drawCrosshair("rgba(0, 0, 0, 0.9)", 4);

            drawCrosshair("rgba(255, 255, 255, 0.98)", 2);

            ctx.restore();

            // Border + shadow outside the clipped area.

            ctx.save();

            ctx.beginPath();

            ctx.arc(loupeX, loupeY, radius, 0, Math.PI * 2);

            ctx.shadowColor = "rgba(0, 0, 0, 0.45)";

            ctx.shadowBlur = 10;

            ctx.shadowOffsetY = 3;

            ctx.strokeStyle = "rgba(0, 0, 0, 0.72)";

            ctx.lineWidth = 6;

            ctx.stroke();

            ctx.shadowColor = "transparent";

            ctx.strokeStyle = "rgba(255, 255, 255, 0.98)";

            ctx.lineWidth = 3;

            ctx.stroke();

            ctx.restore();

        }

    }, [img, getVisibleDimensions, isTouch]);



    // ResizeObserver

    useEffect(() => {

        if (!img || !containerRef.current) return;

        const container = containerRef.current;

        const observer = new ResizeObserver(() => {

            const layout = calculateLayout();

            if (!layout) return;

            const oldScale = scaleRef.current;

            const oldOffset = offsetRef.current;

            setScale(layout.s);

            scaleRef.current = layout.s;

            setOffset({ x: layout.offX, y: layout.offY });

            offsetRef.current = { x: layout.offX, y: layout.offY };



            const next = cornersRef.current.map((p) => ({

                x: layout.offX + ((p.x - oldOffset.x) / oldScale) * layout.s,

                y: layout.offY + ((p.y - oldOffset.y) / oldScale) * layout.s,

            }));

            if (!isControlled) setInternalCorners(next);

            cornersRef.current = next;

            const activeDragIndex = dragIndexRef.current;

            if (activeDragIndex !== null && next[activeDragIndex]) {

                magnifierPointRef.current = { ...next[activeDragIndex] };

            }

            // Force redraw

            requestAnimationFrame(() => drawCanvas());

        });

        observer.observe(container);

        return () => observer.disconnect();

    }, [img, rotation, calculateLayout, isControlled, drawCanvas]);



    // Redraw on state changes

    useEffect(() => {

        if (!img || effectiveCorners.length !== 4) return;

        drawCanvas();

    }, [img, effectiveCorners, scale, offset, rotation, drawCanvas]);



    // Convert displayed corners back to original image coordinates

    const getImageCorners = useCallback((): Point[] => {

        const rot = rotationRef.current;

        const naturalW = imgRef.current?.naturalWidth ?? 0;

        const naturalH = imgRef.current?.naturalHeight ?? 0;

        return cornersRef.current.map((p) => {

            // Canvas pixel → display image coords (undo scale + offset)

            const dx = (p.x - offsetRef.current.x) / scaleRef.current;

            const dy = (p.y - offsetRef.current.y) / scaleRef.current;

            // Display coords → original image coords (undo rotation)

            switch (rot) {

                case 90:  return { x: dy, y: naturalH - dx };

                case 180: return { x: naturalW - dx, y: naturalH - dy };

                case 270: return { x: naturalW - dy, y: dx };

                default:  return { x: dx, y: dy };

            }

        });

    }, []);



    // Notify parent ONLY when uncontrolled state changes

    useEffect(() => {

        if (!isControlled && onCornersChange && internalCorners.length === 4) {

            onCornersChange(getImageCorners());

        }

    }, [internalCorners, isControlled, onCornersChange, getImageCorners]);



    // Cleanup RAF on unmount

    useEffect(() => {

        return () => {

            if (rafRef.current) cancelAnimationFrame(rafRef.current);

        };

    }, []);



    // Handle pointer down

    const handlePointerDown = useCallback((e: React.PointerEvent<HTMLCanvasElement>) => {

        const canvas = canvasRef.current;

        if (!canvas) return;

        const rect = canvas.getBoundingClientRect();

        const x = e.clientX - rect.left;

        const y = e.clientY - rect.top;

        let closestIdx = -1;

        let closestDist = Infinity;

        cornersRef.current.forEach((p, i) => {

            const dist = Math.hypot(p.x - x, p.y - y);

            if (dist < cornerHitRadius && dist < closestDist) {

                closestDist = dist;

                closestIdx = i;

            }

        });

        if (closestIdx !== -1) {

            dragIndexRef.current = closestIdx;

            magnifierVisibleRef.current = true;

            magnifierPointRef.current = { ...cornersRef.current[closestIdx] };

            canvas.setPointerCapture(e.pointerId);

            if (rafRef.current) cancelAnimationFrame(rafRef.current);

            rafRef.current = requestAnimationFrame(drawCanvas);

        }

    }, [cornerHitRadius, drawCanvas]);



    // Handle pointer move — constrain to visible image bounds

    const handlePointerMove = useCallback((e: React.PointerEvent<HTMLCanvasElement>) => {

        const di = dragIndexRef.current;

        const canvas = canvasRef.current;

        if (di === null || !canvas) return;

        const rect = canvas.getBoundingClientRect();

        const x = e.clientX - rect.left;

        const y = e.clientY - rect.top;

        const off = offsetRef.current;

        const s = scaleRef.current;

        const imgEl = imgRef.current;

        if (!imgEl) return;

        // Visible bounds (accounting for rotation)

        const isVertical = rotationRef.current === 90 || rotationRef.current === 270;

        const visW = isVertical ? imgEl.naturalHeight : imgEl.naturalWidth;

        const visH = isVertical ? imgEl.naturalWidth : imgEl.naturalHeight;

        const minX = off.x;

        const maxX = off.x + visW * s;

        const minY = off.y;

        const maxY = off.y + visH * s;

        const nx = Math.max(minX, Math.min(maxX, x));

        const ny = Math.max(minY, Math.min(maxY, y));



        const previous = cornersRef.current;

        const candidate = previous.map((p, i) =>

            i === di ? { x: nx, y: ny } : p

        );

        cornersRef.current = candidate;



        // منع تكوين زوايا متقاطعة أو غير محدبة أو منعدمة المساحة أثناء السحب

        if (validateCorners(getImageCorners()) !== null) {

            cornersRef.current = previous;

            return;

        }

        magnifierPointRef.current = { x: nx, y: ny };



        if (rafRef.current) cancelAnimationFrame(rafRef.current);

        rafRef.current = requestAnimationFrame(drawCanvas);

    }, [drawCanvas, getImageCorners]);



    // Handle pointer up — sync ref to state and release capture

    const handlePointerUp = useCallback((e: React.PointerEvent<HTMLCanvasElement>) => {

        dragIndexRef.current = null;

        magnifierVisibleRef.current = false;

        magnifierPointRef.current = null;

        const canvas = canvasRef.current;

        if (canvas) {

            try { canvas.releasePointerCapture(e.pointerId); } catch { /* ignore */ }

        }

        if (!isControlled) {

            setInternalCorners([...cornersRef.current]);

        } else if (onCornersChange) {

            onCornersChange(getImageCorners());

        }

        if (rafRef.current) cancelAnimationFrame(rafRef.current);

        rafRef.current = requestAnimationFrame(drawCanvas);

    }, [isControlled, onCornersChange, getImageCorners, drawCanvas]);



    if (imgError) {

        return (

            <div className="flex flex-col items-center justify-center h-[40vh] sm:h-[60vh] text-destructive gap-2">

                <p>{imgError}</p>

                <button type="button" onClick={() => window.location.reload()} className="text-xs underline">

                    إعادة المحاولة

                </button>

            </div>

        );

    }



    if (!img) {

        return (

            <div className="flex items-center justify-center h-[40vh] sm:h-[60vh] text-muted-foreground">

                جاري تحميل الصورة...

            </div>

        );

    }



    return (

        <div className="relative">

            <div

                ref={containerRef}

                className="relative w-full h-[40vh] sm:h-[60vh] bg-black/5 rounded-lg overflow-hidden select-none"

            >

                <canvas

                    ref={canvasRef}

                    className="absolute inset-0 w-full h-full touch-none"

                    onPointerDown={handlePointerDown}

                    onPointerMove={handlePointerMove}

                    onPointerUp={handlePointerUp}

                    onPointerCancel={handlePointerUp}

                    style={{ touchAction: "none" }}

                />

                <div className="absolute bottom-2 left-2 bg-black/60 text-white text-xs px-2 py-1 rounded hidden sm:block">

                    اسحب النقاط الملونة لتعديل حواف المستند

                </div>

            </div>

        </div>

    );

}
