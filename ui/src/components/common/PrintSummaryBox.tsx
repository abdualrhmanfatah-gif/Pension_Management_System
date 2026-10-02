import { Heading } from "@/components/ui";
import { cn } from "@/lib/utils";

interface SummaryItem {
    label: string;
    value: string | number;
    color?: 'default' | 'success' | 'warning' | 'danger';
    className?: string;
}

interface PrintSummaryBoxProps {
    items: SummaryItem[];
    title?: string;
    columns?: 2 | 3 | 4;
    variant?: 'modern' | 'classic';
    className?: string;
}

/**
 * مكون موحد لعرض ملخصات الأرقام والإحصائيات في تقارير الطباعة
 */
export function PrintSummaryBox({ 
    items, 
    title, 
    columns = 3,
    variant = 'modern',
    className 
}: PrintSummaryBoxProps) {
    const gridCols = {
        2: 'grid-cols-1 sm:grid-cols-2',
        3: 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3',
        4: 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-4',
    }[columns];

    if (variant === 'classic') {
        return (
            <div className={cn("border border-black mt-5 overflow-hidden", className)}>
                {title && (
                    <div className="bg-slate-200 text-center font-extrabold text-[13px] py-1.5 border-b border-black">
                        {title}
                    </div>
                )}
                <div className={cn("grid gap-4 text-center p-4", gridCols)}>
                    {items.map((item, idx) => (
                        <div key={idx} className={item.className}>
                            <p className="text-[12px] text-black font-bold">{item.label}</p>
                            <p className="font-extrabold text-base">{item.value}</p>
                        </div>
                    ))}
                </div>
            </div>
        );
    }

    return (
        <section className={cn("mb-6", className)}>
            {title && (
                <Heading level="h3" className="text-sm font-bold bg-muted border-e-4 border-black px-2 py-1 mb-2 rounded-s">
                    {title}
                </Heading>
            )}
            <div className={cn("grid gap-4", gridCols)}>
                {items.map((item, idx) => (
                    <div 
                        key={idx} 
                        className={cn(
                            "border border-black rounded-lg p-2 text-center",
                            item.color === 'success' && "bg-green-50/50",
                            item.color === 'warning' && "bg-amber-50/50",
                            item.color === 'danger' && "bg-red-50/50",
                            item.color === 'default' && "bg-muted/50",
                            !item.color && "bg-muted/50",
                            item.className
                        )}
                    >
                        <p className={cn(
                            "text-[10px] font-bold text-muted-foreground print:text-black mb-1",
                            item.color === 'success' && "text-green-600",
                            item.color === 'warning' && "text-amber-600",
                            item.color === 'danger' && "text-red-600"
                        )}>
                            {item.label}
                        </p>
                        <p className="text-lg font-bold font-mono">
                            {item.value}
                        </p>
                    </div>
                ))}
            </div>
        </section>
    );
}
