import * as React from "react";

import { cn } from "@/lib/utils";

// ─── Types ─────────────────────────────────────────────

interface FormFieldProps {
    label?: string;
    required?: boolean;
    error?: string;
    hint?: string;
    children: React.ReactNode;
    className?: string;
    htmlFor?: string;
    icon?: React.ElementType;
}

// ─── Component ─────────────────────────────────────────

function FormField({ label, required, error, hint, children, className, htmlFor, icon: Icon }: FormFieldProps) {
    const autoId = React.useId();
    const fieldId = htmlFor || autoId;
    const errorId = `${fieldId}-error`;
    const hintId = `${fieldId}-hint`;

    const hasError = !!error;

    const childWithAria = React.Children.map(children, (child) => {
        if (!React.isValidElement(child)) return child;
        const existingProps = child.props as Record<string, unknown>;
        return React.cloneElement(child as React.ReactElement<Record<string, unknown>>, {
            id: existingProps.id || fieldId,
            "aria-invalid": existingProps["aria-invalid"] !== undefined ? existingProps["aria-invalid"] : hasError || undefined,
            "aria-describedby":
                existingProps["aria-describedby"] ||
                (hasError ? errorId : hint ? hintId : undefined),
        });
    });

    return (
        <div className={cn("space-y-1", className)}>
            {label && (
                <label
                    id={`${fieldId}-label`}
                    htmlFor={fieldId}
                    className="label-field flex items-center gap-1.5"
                >
                    {Icon && <Icon className="w-4 h-4 text-muted-foreground" />}
                    {label}
                    {required && <span className="text-destructive">*</span>}
                </label>
            )}
            <div className="relative group">
                {childWithAria}
            </div>
            {error && (
                <p
                    id={errorId}
                    role="alert"
                    className="text-xs text-destructive font-medium animate-in fade-in slide-in-from-top-1"
                >
                    {error}
                </p>
            )}
            {hint && !error && (
                <p id={hintId} className="text-xs text-muted-foreground">
                    {hint}
                </p>
            )}
        </div>
    );
}

export { FormField };
export type { FormFieldProps };
