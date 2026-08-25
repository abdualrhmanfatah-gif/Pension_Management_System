import type { VariantProps } from "class-variance-authority";
import * as React from "react";

import { BADGE_VARIANT } from "@/constants";
import { cn } from "@/lib/utils";

import { badgeVariants } from "./badgeVariants";

interface BadgeProps
    extends React.HTMLAttributes<HTMLSpanElement>,
    VariantProps<typeof badgeVariants> {
    dot?: boolean;
}

export const Badge = React.forwardRef<HTMLSpanElement, BadgeProps>(
    ({ className, variant, dot, children, ...props }, ref) => (
        <span
            ref={ref}
            className={cn(badgeVariants({ variant }), className)}
            {...props}
        >
            {dot && (
                <span
                    className={cn(
                        "h-1.5 w-1.5 rounded-full",
                        variant === BADGE_VARIANT.SUCCESS && "bg-emerald-500",
                        variant === BADGE_VARIANT.WARNING && "bg-amber-500",
                        variant === BADGE_VARIANT.DANGER && "bg-red-500",
                        variant === BADGE_VARIANT.INFO && "bg-blue-500",
                      
                        (!variant || variant === BADGE_VARIANT.GHOST) && "bg-muted-foreground",
                        variant === BADGE_VARIANT.OUTLINE && "bg-current"
                    )}
                />
            )}
            {children}
        </span>
    )
);
Badge.displayName = "Badge";

export type { BadgeProps };
