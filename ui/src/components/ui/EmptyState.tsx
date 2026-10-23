import type { LucideIcon } from "lucide-react";
import React from "react";

import { cn } from "@/lib/utils";

import { Button } from "./Button";

interface EmptyStateProps {
    title: string;
    description?: string;
    icon: LucideIcon | React.ReactNode;
    action?: {
        label: string;
        onClick: () => void;
        icon?: LucideIcon;
    };
    className?: string;
}

/**
 * EmptyState — A reusable component for empty lists/tabs.
 * Encourages user action rather than just stating "no data".
 */
export function EmptyState({
    title,
    description,
    icon: Icon,
    action,
    className
}: EmptyStateProps) {
    return (
        <div className={cn(
            "flex flex-col items-center justify-center p-6 sm:p-12 text-center animate-fade-in",
            "border-2 border-dashed border-muted rounded-2xl bg-muted/5",
            className
        )}>
            <div className="flex items-center justify-center w-16 h-16 rounded-full bg-primary/5 mb-4 group hover:bg-primary/10 transition-colors">
                {typeof Icon === 'function' ? (
                    <Icon className="w-8 h-8 text-primary group-hover:text-primary transition-colors" aria-hidden="true" />
                ) : React.isValidElement(Icon) ? (
                    Icon
                ) : (
                    null
                )}
            </div>
            
            <h3 className="text-lg font-bold text-foreground mb-2">{title}</h3>
            
            {description && (
                <p className="text-sm text-muted-foreground max-w-xs mb-6 mx-auto leading-relaxed">
                    {description}
                </p>
            )}

            {action && (
                <Button onClick={action.onClick} className="gap-2">
                    {action.icon && <action.icon className="w-4 h-4" aria-hidden="true" />}
                    {action.label}
                </Button>
            )}
        </div>
    );
}
