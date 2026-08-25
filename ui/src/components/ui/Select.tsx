import * as SelectPrimitive from "@radix-ui/react-select";
import { Check, ChevronDown, ChevronUp } from "lucide-react";
import * as React from "react";

import { cn } from "@/lib/utils";

const Select: React.FC<React.ComponentProps<typeof SelectPrimitive.Root>> = (props) => (
    <SelectPrimitive.Root dir={props.dir || "rtl"} {...props} />
);
const SelectGroup = SelectPrimitive.Group;
const SelectValue = SelectPrimitive.Value;

// ─── Trigger ───────────────────────────────────────────

interface SelectTriggerProps
    extends React.ComponentPropsWithoutRef<typeof SelectPrimitive.Trigger> {
    label?: string;
    error?: string;
}

const SelectTrigger = React.forwardRef<
    React.ComponentRef<typeof SelectPrimitive.Trigger>,
    SelectTriggerProps
>(({ className, children, label, error, ...props }, ref) => (
    <div className="space-y-1.5">
        {label && (
            <label className="block text-sm font-medium text-foreground">{label}</label>
        )}
        <SelectPrimitive.Trigger
            ref={ref}
            dir="rtl"
            aria-label={!label ? (typeof children === 'string' ? children : undefined) : undefined}
            className={cn(
                "flex h-11 sm:h-10 w-full items-center justify-between rounded-md border border-input bg-background px-3 py-2 text-sm",
                "placeholder:text-muted-foreground",
                "focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-1",
                "disabled:cursor-not-allowed disabled:opacity-50",
                "[&>span]:line-clamp-1",
                error && "border-destructive focus:ring-destructive",
                className
            )}
            {...props}
        >
            {children}
            <SelectPrimitive.Icon asChild>
                <ChevronDown className="h-4 w-4 opacity-50" />
            </SelectPrimitive.Icon>
        </SelectPrimitive.Trigger>
        {error && (
            <p className="text-xs text-destructive font-medium">{error}</p>
        )}
    </div>
));
SelectTrigger.displayName = "SelectTrigger";

// ─── ScrollUpButton ────────────────────────────────────

const SelectScrollUpButton = React.forwardRef<
    React.ComponentRef<typeof SelectPrimitive.ScrollUpButton>,
    React.ComponentPropsWithoutRef<typeof SelectPrimitive.ScrollUpButton>
>(({ className, ...props }, ref) => (
    <SelectPrimitive.ScrollUpButton
        ref={ref}
        className={cn("flex cursor-default items-center justify-center py-1 min-h-[44px]", className)}
        {...props}
    >
        <ChevronUp className="h-4 w-4" />
    </SelectPrimitive.ScrollUpButton>
));
SelectScrollUpButton.displayName = "SelectScrollUpButton";

// ─── ScrollDownButton ──────────────────────────────────

const SelectScrollDownButton = React.forwardRef<
    React.ComponentRef<typeof SelectPrimitive.ScrollDownButton>,
    React.ComponentPropsWithoutRef<typeof SelectPrimitive.ScrollDownButton>
>(({ className, ...props }, ref) => (
    <SelectPrimitive.ScrollDownButton
        ref={ref}
        className={cn("flex cursor-default items-center justify-center py-1 min-h-[44px]", className)}
        {...props}
    >
        <ChevronDown className="h-4 w-4" />
    </SelectPrimitive.ScrollDownButton>
));
SelectScrollDownButton.displayName = "SelectScrollDownButton";

// ─── Content ───────────────────────────────────────────

const SelectContent = React.forwardRef<
    React.ComponentRef<typeof SelectPrimitive.Content>,
    React.ComponentPropsWithoutRef<typeof SelectPrimitive.Content>
>(({ className, children, position = "popper", dir = "rtl", ...props }, ref) => (
    <SelectPrimitive.Portal>
        <SelectPrimitive.Content
            ref={ref}
            dir={dir}
            className={cn(
                "relative z-dropdown max-h-72 min-w-[8rem] overflow-hidden rounded-lg border bg-popover text-popover-foreground",
                "data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95",
                "data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95",
                "data-[side=bottom]:slide-in-from-top-2 data-[side=top]:slide-in-from-bottom-2",
                position === "popper" &&
                "data-[side=bottom]:translate-y-1 data-[side=top]:-translate-y-1",
                className
            )}
            position={position}
            {...props}
        >
            <SelectScrollUpButton />
            <SelectPrimitive.Viewport
                className={cn(
                    "p-1",
                    position === "popper" &&
                    "h-[var(--radix-select-trigger-height)] w-full min-w-[var(--radix-select-trigger-width)]"
                )}
            >
                {children}
            </SelectPrimitive.Viewport>
            <SelectScrollDownButton />
        </SelectPrimitive.Content>
    </SelectPrimitive.Portal>
));
SelectContent.displayName = "SelectContent";

// ─── Label ─────────────────────────────────────────────

const SelectLabel = React.forwardRef<
    React.ComponentRef<typeof SelectPrimitive.Label>,
    React.ComponentPropsWithoutRef<typeof SelectPrimitive.Label>
>(({ className, ...props }, ref) => (
    <SelectPrimitive.Label
        ref={ref}
        className={cn("py-1.5 ps-8 pe-2 text-sm font-semibold text-muted-foreground", className)}
        {...props}
    />
));
SelectLabel.displayName = "SelectLabel";

// ─── Item ──────────────────────────────────────────────

const SelectItem = React.forwardRef<
    React.ComponentRef<typeof SelectPrimitive.Item>,
    React.ComponentPropsWithoutRef<typeof SelectPrimitive.Item>
>(({ className, children, ...props }, ref) => (
    <SelectPrimitive.Item
        ref={ref}
        className={cn(
            "relative flex w-full cursor-default select-none items-center rounded-md py-2 ps-8 pe-2 text-sm outline-none",
            "focus:bg-accent focus:text-accent-foreground",
            "data-[disabled]:pointer-events-none data-[disabled]:opacity-50",
            className
        )}
        {...props}
    >
        <span className="absolute start-2 flex h-3.5 w-3.5 items-center justify-center">
            <SelectPrimitive.ItemIndicator>
                <Check className="h-4 w-4" />
            </SelectPrimitive.ItemIndicator>
        </span>
        <SelectPrimitive.ItemText>{children}</SelectPrimitive.ItemText>
    </SelectPrimitive.Item>
));
SelectItem.displayName = "SelectItem";

// ─── Separator ─────────────────────────────────────────

const SelectSeparator = React.forwardRef<
    React.ComponentRef<typeof SelectPrimitive.Separator>,
    React.ComponentPropsWithoutRef<typeof SelectPrimitive.Separator>
>(({ className, ...props }, ref) => (
    <SelectPrimitive.Separator
        ref={ref}
        className={cn("-mx-1 my-1 h-px bg-border", className)}
        {...props}
    />
));
SelectSeparator.displayName = "SelectSeparator";

export {
    Select,
    SelectContent,
    SelectGroup,
    SelectItem,
    SelectLabel,
    SelectScrollDownButton,
    SelectScrollUpButton,
    SelectSeparator,
    SelectTrigger,
    SelectValue,
};
export type { SelectTriggerProps };
