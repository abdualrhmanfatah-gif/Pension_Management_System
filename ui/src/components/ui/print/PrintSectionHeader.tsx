
import { cn } from "@/lib/utils";

export interface PrintSectionHeaderProps {
    title: string;
    number?: number | string;
    variant?: 'box' | 'line';
    className?: string;
}

export function PrintSectionHeader({ title, number, variant = 'box', className }: PrintSectionHeaderProps) {
    if (variant === 'line') {
        return (
            <h3 className={cn("text-sm font-bold bg-gray-100 print:bg-gray-100 border-r-4 border-primary-600 print:border-black px-2 py-1 mb-2 rounded-l", className)}>
                {number ? `${number}. ` : ''}{title}
            </h3>
        );
    }

    return (
        <div className={cn("flex items-center gap-2 mb-3 border-b-2 border-black pb-1", className)}>
            {number && (
                <div className="w-6 h-6 bg-black text-white flex items-center justify-center text-xs font-bold rounded-sm shrink-0">
                    {number}
                </div>
            )}
            <h3 className="text-sm font-bold uppercase tracking-wide">{title}</h3>
        </div>
    );
}
