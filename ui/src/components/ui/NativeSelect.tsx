import { ChevronDown } from "lucide-react";
import * as React from "react";

<<<<<<< HEAD
import { handleNativeSelectEnter } from "@/lib/enterFocus";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { cn } from "@/lib/utils";

// ─── Types ─────────────────────────────────────────────

interface NativeSelectOption {
    value: string;
    label: string;
    disabled?: boolean;
}

<<<<<<< HEAD
interface NativeSelectProps extends React.AriaAttributes {
=======
interface NativeSelectProps {
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
    ({ label, value, onChange, options, placeholder = "اختر...", required, error, hint, disabled, className, dir, icon: Icon, defaultValue, autoFocus, id, ...ariaProps }, ref) => {
=======
    ({ label, value, onChange, options, placeholder = "اختر...", required, error, hint, disabled, className, dir, icon: Icon, defaultValue, autoFocus, id }, ref) => {
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        const generatedId = React.useId();
        const selectId = id || generatedId;
        const errorId = `${selectId}-error`;
        const hintId = `${selectId}-hint`;
        const hasError = !!error;
<<<<<<< HEAD
        const emptyOption = options.find((option) => option.value === "");

        return (
            <div className="min-w-0 space-y-1.5">
                {label && (
                    <label
                        htmlFor={selectId}
                        className="label-field flex items-center gap-1.5"
                    >
                        {Icon && <Icon className="w-4 h-4 text-muted-foreground" />}
=======

        return (
            <div className="space-y-1.5">
                {label && (
                    <label
                        htmlFor={selectId}
                        className="text-sm font-medium text-muted-foreground flex items-center gap-1.5"
                    >
                        {Icon && <Icon className="w-4 h-4 text-muted-foreground/60" />}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
                        onKeyDown={(e) => handleNativeSelectEnter(e.currentTarget, e.nativeEvent)}
                        disabled={disabled}
                        dir={dir}
                        autoFocus={autoFocus}
                        aria-label={!label ? emptyOption?.label || placeholder : undefined}
=======
                        disabled={disabled}
                        dir={dir}
                        autoFocus={autoFocus}
                        aria-label={!label ? placeholder : undefined}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        aria-invalid={hasError || undefined}
                        aria-describedby={
                            hasError ? errorId : hint ? hintId : undefined
                        }
<<<<<<< HEAD
                        {...ariaProps}
                        className={cn(
                            "flex h-11 sm:h-10 w-full min-w-0 items-center rounded-md border border-input bg-background ps-3 pe-10 py-2 text-base text-[hsl(var(--select-foreground))] transition-all",
                            // 👇 تم التعديل هنا لإخفاء السهم الافتراضي
                            "appearance-none bg-none [&::-ms-expand]:hidden", 
                            "focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary",
                            "disabled:cursor-not-allowed disabled:bg-muted disabled:opacity-100",
=======
                        className={cn(
                            "flex h-11 sm:h-10 w-full items-center rounded-md border border-input bg-background ps-3 pe-10 py-2 text-sm transition-all",
                            // 👇 تم التعديل هنا لإخفاء السهم الافتراضي
                            "appearance-none bg-none [&::-ms-expand]:hidden", 
                            "focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary",
                            "disabled:cursor-not-allowed disabled:opacity-50",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            hasError && "border-destructive focus:ring-destructive/20",
                            className
                        )}
                    >
<<<<<<< HEAD
                        {placeholder && !emptyOption && <option value="">{placeholder}</option>}
=======
                        {placeholder && <option value="">{placeholder}</option>}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        {options.map((opt) => (
                            <option key={opt.value} value={opt.value} disabled={opt.disabled}>
                                {opt.label}
                            </option>
                        ))}
                    </select>
<<<<<<< HEAD
                    <div className="absolute end-3 top-1/2 -translate-y-1/2 flex items-center gap-1 pointer-events-none text-[hsl(var(--select-foreground))] group-focus-within:text-primary transition-colors">
=======
                    <div className="absolute end-3 top-1/2 -translate-y-1/2 flex items-center gap-1 pointer-events-none text-muted-foreground/60 group-focus-within:text-primary transition-colors">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
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
<<<<<<< HEAD
export type { NativeSelectOption, NativeSelectProps };
=======
export type { NativeSelectOption, NativeSelectProps };
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
