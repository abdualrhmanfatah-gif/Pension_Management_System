import { cn } from "@/lib/utils";

/* ═══════════════════════════════════════════════════════════
   Types
   ═══════════════════════════════════════════════════════════ */

export interface SignatureSlot {
    title: string;
    name?: string;
    isSeal?: boolean;
}

export interface PrintSignatureBlockProps {
    slots: SignatureSlot[];
    className?: string;
    /** تاريخ التوقيع (اختياري) */
    date?: string;
    /** عنوان القسم مثل "التوقيعات المعتمدة" */
    sectionTitle?: string;
}

/* ═══════════════════════════════════════════════════════════
   Component
   ═══════════════════════════════════════════════════════════ */

export function PrintSignatureBlock({ 
    slots, 
    className, 
    date, 
    sectionTitle 
}: PrintSignatureBlockProps) {
    return (
        <section 
            className={cn(
                "mt-16 pt-4 print:mt-12 print:pt-2",
                "break-inside-avoid-page",
                className
            )}
            aria-label="قسم التوقيعات"
        >
            {/* عنوان القسم */}
            {sectionTitle && (
                <div className="mb-8 border-b border-gray-300 pb-2">
                    <h3 className="text-sm font-bold text-brand">
                        {sectionTitle}
                    </h3>
                </div>
            )}

            {/* التاريخ */}
            {date && (
                <p className="text-[13px] font-semibold text-gray-700 mb-6 text-start">
                    التاريخ: <span dir="ltr">{date}</span>
                </p>
            )}

            {/* شبكة التوقيعات */}
            <div 
                className={cn(
                    "grid gap-8",
                    slots.length === 1 && "grid-cols-1 justify-items-center",
                    slots.length === 2 && "grid-cols-2",
                    slots.length === 3 && "grid-cols-3",
                    slots.length >= 4 && "grid-cols-2 md:grid-cols-4 print:grid-cols-4"
                )}
                role="list"
            >
                {slots.map((slot, index) => (
                    <SignatureItem 
                        key={index} 
                        slot={slot} 
                        index={index} 
                    />
                ))}
            </div>
        </section>
    );
}

/* ─── عنصر التوقيع الفردي ─── */

function SignatureItem({ 
    slot, 

}: { 
    slot: SignatureSlot; 
    index: number; 
}) {
    if (slot.isSeal) {
        return (
            <div 
                className="flex flex-col items-center gap-3"
                role="listitem"
            >
                <p className="text-[11px] font-bold text-gray-600 uppercase tracking-wide">
                    {slot.title}
                </p>
                <div 
                    className={cn(
                        "w-28 h-28 md:w-32 md:h-32",
                        "border-[3px] border-double border-gray-400",
                        "rounded-full flex items-center justify-center",
                        "bg-gray-50/80",
                        "print:w-28 print:h-28"
                    )}
                    aria-label="مكان الختم الرسمي"
                >
                    <span className="text-[10px] text-gray-400 font-bold text-center leading-tight px-2">
                        الختم
                        <br />
                        الرسمي
                    </span>
                </div>
            </div>
        );
    }

    return (
        <div 
            className="flex flex-col items-center text-center min-w-[200px]"
            role="listitem"
        >
            {/* المنصب/اللقب */}
            <p className="text-[13px] font-bold text-gray-800 mb-1">
                {slot.title}
            </p>

            {/* الاسم */}
            <div className="mb-6">
                {slot.name ? (
                    <p className="text-[14px] font-black text-black">
                        {slot.name}
                    </p>
                ) : (
                    <div className="h-6 flex items-end justify-center">
                        <span className="text-[14px] font-black text-gray-400 tracking-widest">
                            ..............................
                        </span>
                    </div>
                )}
            </div>

            {/* خط التوقيع */}
            <div className="w-full max-w-[180px]">
                <div className="border-b-2 border-gray-800 border-dashed pb-1 mb-1" />
                <p className="text-[11px] font-semibold text-gray-600">
                    التوقيع
                </p>
            </div>
        </div>
    );
}