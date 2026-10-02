import { lazy, Suspense, type SVGProps } from "react";

const QRCodeSVGInner = lazy(() =>
    import("qrcode.react").then((m) => ({ default: m.QRCodeSVG }))
);

type QRCodeSVGProps = SVGProps<SVGSVGElement> & {
    value: string;
    size?: number;
    level?: "L" | "M" | "Q" | "H";
    bgColor?: string;
    fgColor?: string;
};

export function LazyQRCode(props: QRCodeSVGProps) {
    return (
        <Suspense fallback={<div className="h-[52px] w-[52px] animate-pulse rounded bg-muted" />}>
            <QRCodeSVGInner {...props} />
        </Suspense>
    );
}
