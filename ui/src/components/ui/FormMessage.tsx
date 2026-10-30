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
<<<<<<< HEAD
                className={cn("text-sm font-medium text-destructive mt-1", className)}
=======
                className={cn("text-[0.8rem] font-medium text-destructive mt-1", className)}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                {...props}
            >
                {children}
            </p>
        );
    }
);
FormMessage.displayName = "FormMessage";

export { FormMessage };
