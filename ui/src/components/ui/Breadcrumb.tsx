import { ChevronLeft } from "lucide-react";
import { Link } from "react-router-dom";

import { cn } from "@/lib/utils";

// ─── Types ─────────────────────────────────────────────

interface BreadcrumbItem {
    label: string;
    href?: string;
}

interface BreadcrumbProps {
    items: BreadcrumbItem[];
    className?: string;
}

// ─── Component ─────────────────────────────────────────

function Breadcrumb({ items, className }: BreadcrumbProps) {
    return (
        <nav aria-label="التنقل التسلسلي" className={cn("flex items-center gap-1 text-sm overflow-x-auto", className)}>
            {items.map((item, index) => {
                const isLast = index === items.length - 1;

                return (
                    <span key={index} className="flex items-center gap-1">
                        {index > 0 && (
                            <ChevronLeft className="w-3.5 h-3.5 text-muted-foreground/50 shrink-0" />
                        )}
                        {isLast || !item.href ? (
                            <span className={cn(
                                "font-medium truncate max-w-[150px] sm:max-w-[200px]",
                                isLast ? "text-foreground" : "text-muted-foreground"
                            )}>
                                {item.label}
                            </span>
                        ) : (
                            <Link
                                to={item.href}
                                className="text-muted-foreground hover:text-foreground transition-colors truncate max-w-[150px] sm:max-w-[200px]"
                            >
                                {item.label}
                            </Link>
                        )}
                    </span>
                );
            })}
        </nav>
    );
}

export { Breadcrumb };
export type { BreadcrumbItem,BreadcrumbProps };
