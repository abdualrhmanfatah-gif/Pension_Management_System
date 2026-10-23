import React from "react";
import { type LucideIcon } from "lucide-react";

import { Button } from "./Button";
import { Card } from "./Card";
import { Heading } from "./Typography";

export interface TabHeaderProps {
    title: string;
    description?: string;
    icon?: LucideIcon;
    action?: {
        label: string;
        icon?: LucideIcon;
        onClick: () => void;
        disabled?: boolean;
        show?: boolean;
        title?: string;
    };
    children?: React.ReactNode;
}

export function TabHeader({
    title,
    description,
    icon: Icon,
    action,
    children
}: TabHeaderProps) {
    return (
        <Card className="flex flex-col md:flex-row md:items-center justify-between gap-2 md:gap-4 bg-card shadow-sm border-2 border-primary px-3.5 py-2.5 md:px-4 md:py-3 rounded-xl transition-all">
            <div className="space-y-0.5 text-start">
                <Heading level="h3" className="font-bold flex items-center gap-1.5 md:gap-2 text-sm md:text-base text-foreground">
                    {Icon && <Icon className="w-4 h-4 md:w-5 md:h-5 text-primary" />} {title}
                </Heading>
                {description && (
                    <p className="text-xs md:text-sm font-medium text-muted-foreground hidden sm:block">
                        {description}
                    </p>
                )}
            </div>
            <div className="flex items-center gap-2">
                {children}
                {action && (action.show !== false) && (
                    <Button 
                        size="sm" 
                        onClick={action.onClick} 
                        disabled={action.disabled}
                        title={action.title}
                        className="gap-1 rounded-xl font-bold h-8 md:h-9 px-3.5 md:px-5 transition-all text-xs md:text-sm"
                    >
                        {action.icon && React.createElement(action.icon, { className: "w-3.5 h-3.5 md:w-4 md:h-4 ms-1" })} 
                        {action.label}
                    </Button>
                )}
            </div>
        </Card>
    );
}
