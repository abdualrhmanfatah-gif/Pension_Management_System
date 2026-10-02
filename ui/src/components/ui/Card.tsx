import * as React from "react";

import { cn } from "@/lib/utils";

// ─── Card ──────────────────────────────────────────────

export type CardVariant = "default" | "flat" | "dashboard";

interface CardProps extends React.HTMLAttributes<HTMLDivElement> {
    variant?: CardVariant;
}

const variantStyles: Record<CardVariant, string> = {
    default: "rounded-xl border border-border bg-card text-card-foreground",
    flat: "rounded-3xl border-none bg-card text-card-foreground",
    dashboard: "rounded-3xl border-none bg-muted/30 text-card-foreground",
};

const Card = React.forwardRef<HTMLDivElement, CardProps>(
    ({ className, variant = "default", ...props }, ref) => (
        <div
            ref={ref}
            className={cn(variantStyles[variant], "w-full", className)}
            {...props}
        />
    )
);
Card.displayName = "Card";

// ─── CardHeader ────────────────────────────────────────

type CardHeaderProps = React.HTMLAttributes<HTMLDivElement>;

const CardHeader = React.forwardRef<HTMLDivElement, CardHeaderProps>(
    ({ className, ...props }, ref) => (
        <div
            ref={ref}
            className={cn("flex flex-col space-y-1.5 p-4 sm:p-6 pb-4", className)}
            {...props}
        />
    )
);
CardHeader.displayName = "CardHeader";

// ─── CardTitle ─────────────────────────────────────────

type CardTitleProps = React.HTMLAttributes<HTMLHeadingElement>;

const CardTitle = React.forwardRef<HTMLHeadingElement, CardTitleProps>(
    ({ className, ...props }, ref) => (
        <h3
            ref={ref}
            className={cn("text-lg font-semibold leading-none tracking-tight", className)}
            {...props}
        />
    )
);
CardTitle.displayName = "CardTitle";

// ─── CardDescription ──────────────────────────────────

type CardDescriptionProps = React.HTMLAttributes<HTMLParagraphElement>;

const CardDescription = React.forwardRef<HTMLParagraphElement, CardDescriptionProps>(
    ({ className, ...props }, ref) => (
        <p
            ref={ref}
            className={cn("text-sm text-muted-foreground", className)}
            {...props}
        />
    )
);
CardDescription.displayName = "CardDescription";

// ─── CardContent ───────────────────────────────────────

type CardContentProps = React.HTMLAttributes<HTMLDivElement>;

const CardContent = React.forwardRef<HTMLDivElement, CardContentProps>(
    ({ className, ...props }, ref) => (
        <div
            ref={ref}
            className={cn("p-4 sm:p-6 pt-0", className)}
            {...props}
        />
    )
);
CardContent.displayName = "CardContent";

// ─── CardFooter ────────────────────────────────────────

type CardFooterProps = React.HTMLAttributes<HTMLDivElement>;

const CardFooter = React.forwardRef<HTMLDivElement, CardFooterProps>(
    ({ className, ...props }, ref) => (
        <div
            ref={ref}
            className={cn("flex items-center flex-wrap gap-2 p-4 sm:p-6 pt-0", className)}
            {...props}
        />
    )
);
CardFooter.displayName = "CardFooter";

export { Card, CardContent, CardDescription, CardFooter,CardHeader, CardTitle };
export type { CardContentProps, CardDescriptionProps, CardFooterProps,CardHeaderProps, CardProps, CardTitleProps };
