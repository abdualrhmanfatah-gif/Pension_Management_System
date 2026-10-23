import { ChevronDown } from "lucide-react";
import * as React from "react";

import { handleNativeSelectEnter } from "@/lib/enterFocus";
import { cn } from "@/lib/utils";

// ─── Types ─────────────────────────────────────────────

interface NativeSelectOption {
    value: string;
    label: string;
    disabled?: boolean;
}

interface NativeSelectProps extends React.AriaAttributes {
    label?: string;
    value: string | number;
    onChange: (value: string) => void;
    options: NativeSelectOption[];
    placeholder?: string;
    required?: boolean;
    error?: string;
    hint?: string;
    disabled?: boolean;
    className?: string;
    dir?: string;
    icon?: React.ElementType;
    defaultValue?: string | number;
    autoFocus?: boolean;
    id?: string;
}

// ─── Component ─────────────────────────────────────────

const NativeSelect = React.forwardRef<HTMLSelectElement, NativeSelectProps>(
    ({ label, value, onChange, options, placeholder = "اختر...", required, error, hint, disabled, className, dir, icon: Icon, defaultValue, autoFocus, id, ...ariaProps }, ref) => {
        const generatedId = React.useId();
        const selectId = id || generatedId;
        const errorId = `${selectId}-error`;
        const hintId = `${selectId}-hint`;
        const hasError = !!error;

        return (
            <div className="space-y-1.5">
                {label && (
                    <label
                        htmlFor={selectId}
                        className="label-field flex items-center gap-1.5"
                    >
                        {Icon && <Icon className="w-4 h-4 text-muted-foreground/60" />}
                        {label}
                        {required && <span className="text-destructive">*</span>}
                    </label>
                )}
                <div className="relative group">
                    <select
                        ref={ref}
                        id={selectId}
                        value={value}
                        defaultValue={defaultValue}
                        onChange={(e) => onChange(e.target.value)}
                        onKeyDown={(e) => handleNativeSelectEnter(e.currentTarget, e.nativeEvent)}
                        disabled={disabled}
                        dir={dir}
                        autoFocus={autoFocus}
                        aria-label={!label ? placeholder : undefined}
                        aria-invalid={hasError || undefined}
                        aria-describedby={
                            hasError ? errorId : hint ? hintId : undefined
                        }
                        {...ariaProps}
                        className={cn(
                            "flex h-11 sm:h-10 w-full items-center rounded-md border border-input bg-background ps-3 pe-10 py-2 text-base transition-all",
                            // 👇 تم التعديل هنا لإخفاء السهم الافتراضي
                            "appearance-none bg-none [&::-ms-expand]:hidden", 
                            "focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary",
                            "disabled:cursor-not-allowed disabled:opacity-50",
                            hasError && "border-destructive focus:ring-destructive/20",
                            className
                        )}
                    >
                        {placeholder && <option value="">{placeholder}</option>}
                        {options.map((opt) => (
                            <option key={opt.value} value={opt.value} disabled={opt.disabled}>
                                {opt.label}
                            </option>
                        ))}
                    </select>
                    <div className="absolute end-3 top-1/2 -translate-y-1/2 flex items-center gap-1 pointer-events-none text-muted-foreground/60 group-focus-within:text-primary transition-colors">
                        <ChevronDown className="w-4 h-4" />
                    </div>
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
);
NativeSelect.displayName = "NativeSelect";

export { NativeSelect };
export type { NativeSelectOption, NativeSelectProps };
