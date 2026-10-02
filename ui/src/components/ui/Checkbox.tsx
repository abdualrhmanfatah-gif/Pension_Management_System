import { forwardRef, type InputHTMLAttributes } from "react";

import { cn } from "@/lib/utils";

export interface CheckboxProps extends Omit<InputHTMLAttributes<HTMLInputElement>, "type"> {
    label?: string;
    onCheckedChange?: (checked: boolean) => void;
}

const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(
    ({ className, label, onCheckedChange, onChange, ...props }, ref) => {
        const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
            onChange?.(e);
            onCheckedChange?.(e.target.checked);
        };

        return (
            <label className={cn("flex items-center gap-2 cursor-pointer", className)}>
                <input
                    ref={ref}
                    type="checkbox"
                    className={cn(
                        "h-4 w-4 rounded border-border text-primary focus:ring-primary/30",
                        "disabled:cursor-not-allowed disabled:opacity-50"
                    )}
                    onChange={handleChange}
                    {...props}
                />
                {label && <span className="text-sm font-medium text-foreground">{label}</span>}
            </label>
        );
    }
);

Checkbox.displayName = "Checkbox";

export { Checkbox };
