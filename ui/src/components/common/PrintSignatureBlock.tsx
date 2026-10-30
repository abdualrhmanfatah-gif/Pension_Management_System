<<<<<<< HEAD
import type { SignatureSlot } from "@/components/ui/print/PrintSignatureBlock";
import { PrintSignatureBlock as NewPrintSignatureBlock } from "@/components/ui/print/PrintSignatureBlock";
=======
import { PrintSignatureBlock as NewPrintSignatureBlock } from "@/components/ui/print/PrintSignatureBlock";
import type { SignatureSlot } from "@/components/ui/print/PrintSignatureBlock";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { cn } from "@/lib/utils";

export interface Signature {
    title: string;
    sub?: string;
    className?: string;
}

interface PrintSignatureBlockProps {
    signatures: Signature[];
    variant?: 'default' | 'boxes';
    className?: string;
}

export function PrintSignatureBlock({
    signatures,
    variant = 'default',
    className
}: PrintSignatureBlockProps) {
    const slots: SignatureSlot[] = signatures.map((sig) => ({
        title: sig.title,
        name: sig.sub || "........................",
    }));

    if (variant === 'boxes') {
        return (
            <section className={cn("mt-4 mb-2 px-2 print:page-break-inside-avoid", className)}>
                <div className={cn(
                    "grid gap-4 sm:gap-8 text-center font-bold",
                    signatures.length === 2 ? 'grid-cols-1 sm:grid-cols-2' :
                    signatures.length === 3 ? 'grid-cols-1 sm:grid-cols-3' :
                    signatures.length === 4 ? 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-4' : 'grid-cols-1 sm:grid-cols-3'
                )}>
                    {signatures.map((sig, idx) => (
                        <div key={idx} className={cn(
                            "flex flex-col items-center border border-black p-3 rounded-lg bg-muted/50",
                            sig.className
                        )}>
                            <p className="text-[12px] font-black mb-8 border-b border-black pb-1 w-full">
                                {sig.title}
                            </p>
                            <div className="w-[85%]">
                                <p className="text-[10px] text-slate-700 font-bold">
                                    {sig.sub || "التوقيع والختم الرسمي"}
                                </p>
                            </div>
                        </div>
                    ))}
                </div>
            </section>
        );
    }

    return (
        <section className={cn("mt-0 mb-2 px-2 print:page-break-inside-avoid", className)}>
            <NewPrintSignatureBlock slots={slots} />
        </section>
    );
}
