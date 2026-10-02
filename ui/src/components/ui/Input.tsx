import { Calendar, Clock } from "lucide-react";
import * as React from "react";

import { cn } from "@/lib/utils";

// ============================================
// FormField Wrapper (مشترك لتقليل التكرار)
// ============================================

interface FormFieldWrapperProps {
    id: string;
    label?: string;
    error?: string;
    hint?: string;
    children: React.ReactNode;
}

const FormFieldWrapper: React.FC<FormFieldWrapperProps> = ({
    id,
    label,
    error,
    hint,
    children,
}) => {
    const hasError = !!error;
    const hasHint = !!hint && !hasError;

    return (
        <div className="space-y-1">
            {label && (
                <label htmlFor={id} className="label-field">
                    {label}
                </label>
            )}
            {children}
            {hasError && (
                <p
                    id={`${id}-error`}
                    role="alert"
                    className="text-xs text-destructive font-medium animate-in fade-in slide-in-from-top-1"
                >
                    {error}
                </p>
            )}
            {hasHint && (
                <p id={`${id}-hint`} className="text-xs text-muted-foreground">
                    {hint}
                </p>
            )}
        </div>
    );
};

// ============================================
// 1. Input (محسّن)
// ============================================

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
    label?: string;
    error?: string;
    hint?: string;
    icon?: React.ElementType;
}

const Input = React.forwardRef<HTMLInputElement, InputProps>(
    ({ className, label, error, hint, id, type = "text", icon: Icon, ...props }, ref) => {
        const generatedId = React.useId();
        const inputId = id || generatedId;
        const hasError = !!error;

        const EffectiveIcon = Icon || (type === "time" ? Clock : undefined);

        return (
            <FormFieldWrapper id={inputId} label={label} error={error} hint={hint}>
                <div className="relative w-full group">
                    {EffectiveIcon && (
                        <div
                            className={cn(
                                "absolute end-[1px] top-[1px] bottom-[1px] px-4 flex items-center justify-center border-s transition-colors pointer-events-none rounded-e-md",
                                "text-muted-foreground/60 group-focus-within:text-primary group-focus-within:border-primary/30",
                                type === "time" && "bg-muted/30 group-hover:bg-muted/50"
                            )}
                        >
                            <EffectiveIcon className="h-4 w-4" />
                        </div>
                    )}
                    <input
                        type={type}
                        id={inputId}
                        ref={ref}
                        aria-label={!label && props.placeholder ? props.placeholder : undefined}
                        className={cn(
                            "block h-11 sm:h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm font-sans transition-all",
                            "placeholder:text-muted-foreground",
                            "focus:outline-none focus:ring-2 focus:ring-ring/20 focus:border-primary focus:ring-offset-0",
                            "disabled:cursor-not-allowed disabled:opacity-50",
                            "file:border-0 file:bg-transparent file:text-sm file:font-medium",
                            EffectiveIcon && "pe-12",
                            type === "time" && "font-mono text-start cursor-pointer hover:bg-muted/5",
                            hasError && "border-destructive focus:ring-destructive/20",
                            className
                        )}
                        aria-invalid={hasError}
                        aria-describedby={
                            hasError
                                ? `${inputId}-error`
                                : hint
                                    ? `${inputId}-hint`
                                    : undefined
                        }
                        {...props}
                    />
                </div>
            </FormFieldWrapper>
        );
    }
);
Input.displayName = "Input";

interface DateInputProps
    extends Omit<
        React.InputHTMLAttributes<HTMLInputElement>,
        "type" | "onChange" | "value" | "defaultValue"
    > {
    value?: string; // ISO: YYYY-MM-DD
    defaultValue?: string; // ISO: YYYY-MM-DD
    onChange?: (
        value: string | undefined,
        event?: React.ChangeEvent<HTMLInputElement> | React.ClipboardEvent<HTMLInputElement>
    ) => void;
    placeholder?: string;
    pickerLabel?: string;
    label?: string;
    error?: string;
    hint?: string;
}

const DateInput = React.forwardRef<HTMLInputElement, DateInputProps>(
    (
        {
            className,
            label,
            error: externalError,
            hint,
            id,
            value,
            defaultValue,
            onChange,
            placeholder,
            pickerLabel = "Open date picker",
            name,
            disabled,
            readOnly,
            required,
            autoFocus,
            onBlur,
            onFocus,
            onKeyDown,
            onClick,
            ...restProps
        },
        forwardedRef
    ) => {
        const generatedId = React.useId();
        const inputId = id || generatedId;

        const isControlled = value !== undefined;
        const initialValue = isControlled ? value : defaultValue;

        const nativeRef = React.useRef<HTMLInputElement>(null);
        const displayRef = React.useRef<HTMLInputElement>(null);

        // --- تحويل ISO → DD/MM/YYYY ---
        const isoToDisplay = React.useCallback((iso: string | undefined): string => {
            if (!iso || typeof iso !== "string") return "";
            const [y, m, d] = iso.split("-");
            if (y?.length === 4 && m?.length === 2 && d?.length === 2) {
                return `${d}/${m}/${y}`;
            }
            return "";
        }, []);

        // --- التحقق من صحة التاريخ الحقيقي ---
        const isValidDate = React.useCallback(
            (day: number, month: number, year: number): boolean => {
                if (month < 1 || month > 12) return false;
                if (day < 1 || day > 31) return false;
                const date = new Date(year, month - 1, day);
                return date.getDate() === day && date.getMonth() === month - 1;
            },
            []
        );

        const [displayValue, setDisplayValue] = React.useState(() =>
            isoToDisplay(initialValue)
        );
        const [internalError, setInternalError] = React.useState<string | undefined>(undefined);

        const hasError = !!externalError || !!internalError;
        const activeError = externalError || internalError;

        // مزامنة القيمة الخارجية (Controlled فقط)
        React.useEffect(() => {
            if (isControlled) {
                setDisplayValue(isoToDisplay(value));
            }
        }, [value, isControlled, isoToDisplay]);

        // --- معالجة التغيير أثناء الكتابة ---
        const handleDisplayChange = (e: React.ChangeEvent<HTMLInputElement>) => {
            let input = e.target.value.replace(/\D/g, "");
            if (input.length > 8) input = input.substring(0, 8);

            // التحقق المنطقي أثناء الكتابة
            if (input.length >= 2) {
                const day = parseInt(input.substring(0, 2), 10);
                if (day > 31) input = "31" + input.substring(2);
                if (day === 0) input = "01" + input.substring(2);
            }
            if (input.length >= 4) {
                const month = parseInt(input.substring(2, 4), 10);
                if (month > 12) input = input.substring(0, 2) + "12" + input.substring(4);
                if (month === 0) input = input.substring(0, 2) + "01" + input.substring(4);
            }

            // التنسيق الظاهري
            let formatted = "";
            if (input.length > 0) formatted += input.substring(0, 2);
            if (input.length > 2) formatted += "/" + input.substring(2, 4);
            if (input.length > 4) formatted += "/" + input.substring(4, 8);

            setDisplayValue(formatted);
            setInternalError(undefined);

            // إذا اكتمل، نتحقق ونرسل للأب
            if (input.length === 8) {
                const day = parseInt(input.substring(0, 2), 10);
                const month = parseInt(input.substring(2, 4), 10);
                const year = parseInt(input.substring(4, 8), 10);

                if (!isValidDate(day, month, year)) {
                    setInternalError("Invalid date");
                    onChange?.(undefined, e);
                    return;
                }

                const isoValue = `${year}-${String(month).padStart(2, "0")}-${String(day).padStart(2, "0")}`;

                // تحديث الـ native input المخفي
                if (nativeRef.current) {
                    nativeRef.current.value = isoValue;
                }

                onChange?.(isoValue, e);
            } else if (input.length === 0) {
                onChange?.(undefined, e);
            }
        };

        // --- دعم اللصق من الحافظة ---
        const handlePaste = (e: React.ClipboardEvent<HTMLInputElement>) => {
            const pasted = e.clipboardData.getData("text");

            // محاولة التعرف على ISO: YYYY-MM-DD
            const isoMatch = pasted.match(/^(\d{4})-(\d{2})-(\d{2})$/);
            if (isoMatch) {
                e.preventDefault();
                const [, y, m, d] = isoMatch;
                const day = parseInt(d, 10);
                const month = parseInt(m, 10);
                const year = parseInt(y, 10);

                if (!isValidDate(day, month, year)) {
                    setInternalError("Invalid date");
                    onChange?.(undefined, e);
                    return;
                }

                const display = `${d}/${m}/${y}`;
                setDisplayValue(display);
                setInternalError(undefined);
                const isoValue = `${y}-${m}-${d}`;
                if (nativeRef.current) nativeRef.current.value = isoValue;
                onChange?.(isoValue, e);
                return;
            }

            // محاولة التعرف على DD/MM/YYYY
            const displayMatch = pasted.match(/^(\d{2})\/(\d{2})\/(\d{4})$/);
            if (displayMatch) {
                e.preventDefault();
                const [, d, m, y] = displayMatch;
                const day = parseInt(d, 10);
                const month = parseInt(m, 10);
                const year = parseInt(y, 10);

                if (!isValidDate(day, month, year)) {
                    setInternalError("Invalid date");
                    onChange?.(undefined, e);
                    return;
                }

                setDisplayValue(pasted);
                setInternalError(undefined);
                const isoValue = `${y}-${m}-${d}`;
                if (nativeRef.current) nativeRef.current.value = isoValue;
                onChange?.(isoValue, e);
                return;
            }

            // إذا لم يتطابق مع شيء، نترك السلوك الافتراضي (إزالة غير الأرقام في onChange)
        };

        // --- فتح الـ Native Picker ---
        const openNativePicker = () => {
            if (disabled || readOnly) return;
            if (typeof nativeRef.current?.showPicker === "function") {
                nativeRef.current.showPicker();
            }
        };

        // --- عند تغيير الـ Native Picker ---
        const handleNativeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
            const val = e.target.value;
            setInternalError(undefined);
            if (val) {
                setDisplayValue(isoToDisplay(val));
                onChange?.(val, e);
            } else {
                setDisplayValue("");
                onChange?.(undefined, e);
            }
        };

        // --- useImperativeHandle: الـ Ref يُشير للـ display input ---
        React.useImperativeHandle(
            forwardedRef,
            () => {
                if (!displayRef.current) {
                    return {} as HTMLInputElement;
                }
                // نُعيد الـ display input مع إضافة showPicker كـ custom method
                const el = displayRef.current;
                return Object.assign(el, {
                    showPicker: () => {
                        if (typeof nativeRef.current?.showPicker === "function") {
                            nativeRef.current.showPicker();
                        }
                    },
                });
            },
            []
        );

        return (
            <FormFieldWrapper id={inputId} label={label} error={activeError} hint={hint}>
                <div className={cn("relative group", className)}>
                    {/* Native input مخفي تماماً - يستخدم فقط لفتح الـ Picker */}
                    <input
                        type="date"
                        ref={nativeRef}
                        tabIndex={-1}
                        aria-hidden="true"
                        style={{
                            position: "absolute",
                            opacity: 0,
                            width: 0,
                            height: 0,
                            pointerEvents: "none",
                        }}
                        onChange={handleNativeChange}
                        value={isControlled ? value : undefined}
                        defaultValue={!isControlled ? defaultValue : undefined}
                        name={name}
                    />

                    {/* الـ Input الظاهري - يفرض تنسيق DD/MM/YYYY */}
                    <input
                        ref={displayRef}
                        type="text"
                        id={inputId}
                        aria-label={!label ? placeholder || "DD/MM/YYYY" : undefined}
                        inputMode="numeric"
                        autoComplete="off"
                        data-lpignore="true"
                        data-form-type="other"
                        spellCheck={false}
                        placeholder={placeholder || "DD/MM/YYYY"}
                        dir="ltr"
                        value={displayValue}
                        onChange={handleDisplayChange}
                        onPaste={handlePaste}
                        disabled={disabled}
                        readOnly={readOnly}
                        required={required}
                        autoFocus={autoFocus}
                        onBlur={onBlur}
                        onFocus={onFocus}
                        onKeyDown={onKeyDown}
                        onClick={onClick}
                        className={cn(
                            "block h-11 sm:h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm transition-all",
                            "placeholder:text-muted-foreground/40 font-mono text-left",
                            "focus:outline-none focus:ring-2 focus:ring-ring/20 focus:border-primary",
                            "disabled:cursor-not-allowed disabled:opacity-50",
                            "pl-14",
                            hasError && "border-destructive focus:ring-destructive/20",
                        )}
                        aria-invalid={hasError}
                        aria-describedby={
                            hasError
                                ? `${inputId}-error`
                                : hint
                                    ? `${inputId}-hint`
                                    : undefined
                        }
                        {...restProps}
                    />

                    {/* زر التقويم */}
                    <button
                        type="button"
                        onClick={openNativePicker}
                        aria-label={pickerLabel}
                        tabIndex={-1}
                        disabled={disabled}
                        className={cn(
                            "absolute left-3 top-1/2 -translate-y-1/2 flex items-center justify-center transition-colors min-h-[44px] min-w-[44px]",
                            "text-muted-foreground/60 hover:text-primary",
                            disabled && "opacity-50 cursor-not-allowed"
                        )}
                    >
                        <Calendar className="h-4 w-4" />
                    </button>
                </div>
            </FormFieldWrapper>
        );
    }
);
DateInput.displayName = "DateInput";

// ============================================
// 3. Textarea (محسّن)
// ============================================

interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
    label?: string;
    error?: string;
    hint?: string;
}

const Textarea = React.forwardRef<HTMLTextAreaElement, TextareaProps>(
    ({ className, label, error, hint, id, ...props }, ref) => {
        const generatedId = React.useId();
        const textareaId = id || generatedId;
        const hasError = !!error;

        return (
            <FormFieldWrapper id={textareaId} label={label} error={error} hint={hint}>
                <textarea
                    id={textareaId}
                    ref={ref}
                    aria-label={!label && props.placeholder ? props.placeholder : undefined}
                    className={cn(
                        "flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm transition-colors",
                        "placeholder:text-muted-foreground",
                        "focus:outline-none focus:ring-2 focus:ring-ring/20 focus:border-primary focus:ring-offset-0",
                        "disabled:cursor-not-allowed disabled:opacity-50",
                        hasError && "border-destructive focus:ring-destructive/20",
                        className
                    )}
                    aria-invalid={hasError}
                    aria-describedby={
                        hasError
                            ? `${textareaId}-error`
                            : hint
                                ? `${textareaId}-hint`
                                : undefined
                    }
                    {...props}
                />
            </FormFieldWrapper>
        );
    }
);
Textarea.displayName = "Textarea";

export { DateInput, Input, Textarea };
export type { DateInputProps, InputProps, TextareaProps };
