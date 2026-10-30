import * as AccordionPrimitive from "@radix-ui/react-accordion";
import { ChevronDown } from "lucide-react";
import * as React from "react";

import { cn } from "@/lib/utils";

const Accordion = AccordionPrimitive.Root;

const AccordionItem = React.forwardRef<
    React.ComponentRef<typeof AccordionPrimitive.Item>,
    React.ComponentPropsWithoutRef<typeof AccordionPrimitive.Item>
>(({ className, ...props }, ref) => (
    <AccordionPrimitive.Item
        ref={ref}
<<<<<<< HEAD
        className={cn("border-b border-muted", className)}
=======
        className={cn("border-b border-muted/50", className)}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        {...props}
    />
));
AccordionItem.displayName = "AccordionItem";

const AccordionTrigger = React.forwardRef<
    React.ComponentRef<typeof AccordionPrimitive.Trigger>,
<<<<<<< HEAD
    React.ComponentPropsWithoutRef<typeof AccordionPrimitive.Trigger> & {
        density?: "default" | "compact";
    }
>(({ className, children, density = "default", ...props }, ref) => (
=======
    React.ComponentPropsWithoutRef<typeof AccordionPrimitive.Trigger>
>(({ className, children, ...props }, ref) => (
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    <AccordionPrimitive.Header className="flex">
        <AccordionPrimitive.Trigger
            ref={ref}
            className={cn(
<<<<<<< HEAD
                "flex flex-1 items-center justify-between text-sm font-semibold transition-all hover:underline [&[data-state=open]>svg]:rotate-180",
                density === "compact" ? "min-h-11 py-2" : "py-4",
=======
                "flex flex-1 items-center justify-between py-4 text-sm font-semibold transition-all hover:underline [&[data-state=open]>svg]:rotate-180",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                "text-start",
                className
            )}
            {...props}
        >
            {children}
            <ChevronDown className="h-4 w-4 shrink-0 text-muted-foreground transition-transform duration-200" />
        </AccordionPrimitive.Trigger>
    </AccordionPrimitive.Header>
));
AccordionTrigger.displayName = AccordionPrimitive.Trigger.displayName;

const AccordionContent = React.forwardRef<
    React.ComponentRef<typeof AccordionPrimitive.Content>,
    React.ComponentPropsWithoutRef<typeof AccordionPrimitive.Content>
>(({ className, children, ...props }, ref) => (
    <AccordionPrimitive.Content
        ref={ref}
        className="overflow-hidden text-sm data-[state=closed]:animate-accordion-up data-[state=open]:animate-accordion-down"
        {...props}
    >
        <div className={cn("pb-4 pt-0", className)}>{children}</div>
    </AccordionPrimitive.Content>
));
AccordionContent.displayName = AccordionPrimitive.Content.displayName;

export { Accordion, AccordionContent,AccordionItem, AccordionTrigger };
