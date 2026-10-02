import * as React from "react";

import { cn } from "@/lib/utils";

interface FormMessageProps extends React.HTMLAttributes<HTMLParagraphElement> {
    children?: React.ReactNode;
}

const FormMessage = React.forwardRef<HTMLParagraphElement, FormMessageProps>(
    ({ className, children, ...props }, ref) => {
        if (!children) return null;

        return (
            <p
                ref={ref}
                className={cn("text-sm font-medium text-destructive mt-1", className)}
                {...props}
            >
                {children}
            </p>
        );
    }
);
FormMessage.displayName = "FormMessage";

export { FormMessage };
