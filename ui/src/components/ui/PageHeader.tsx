<<<<<<< HEAD
import * as React from "react";

import { cn } from "@/lib/utils";
=======
import { cn } from "@/lib/utils";
import * as React from "react";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface PageHeaderProps {
    title: string;
    description?: string;
    icon?: React.ReactNode;
    actions?: React.ReactNode;
    children?: React.ReactNode;
    className?: string;
}

export function PageHeader({
    title,
    description,
    icon,
    actions,
    children,
    className
}: PageHeaderProps) {
    return (
        <div className={cn(
            "flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between",
            className
        )}>
            <div className="flex items-center gap-3 min-w-0">
                {icon && (
                    <div className="p-3 bg-primary/10 rounded-2xl shrink-0">
                        {icon}
                    </div>
                )}
                <div className="min-w-0">
                    <h1 className="text-lg md:text-xl lg:text-2xl font-bold tracking-tight truncate">
                        {title}
                    </h1>
                    {description && (
                        <p className="text-sm text-muted-foreground mt-1 truncate">
                            {description}
                        </p>
                    )}
                </div>
            </div>
            {(actions || children) && (
                <div className="flex items-center gap-2 shrink-0 flex-wrap">
                    {actions}
                    {children}
                </div>
            )}
        </div>
    );
}
