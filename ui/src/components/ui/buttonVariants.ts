import { cva, type VariantProps } from "class-variance-authority";

export const buttonVariants = cva(
    "inline-flex items-center justify-center gap-2 rounded-md text-sm font-medium transition-all duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 active:scale-[0.98] hover:scale-[1.01]",
    {
        variants: {
            variant: {
                primary:
                    "bg-primary text-primary-foreground shadow-md shadow-primary/20 font-bold hover:bg-primary/90 hover:shadow-lg hover:shadow-primary/30 hover:scale-[1.02]",
                secondary:
                    "bg-secondary text-secondary-foreground hover:bg-secondary/80 font-medium",
                default:
                    "bg-primary text-primary-foreground shadow-sm hover:bg-primary/90 font-medium",
                destructive:
                    "bg-destructive text-destructive-foreground shadow-sm hover:bg-destructive/90 font-bold hover:scale-[1.02]",
                success:
<<<<<<< HEAD
                    "bg-[hsl(var(--gov-success))] text-white shadow-md shadow-emerald-500/20 font-bold hover:opacity-100 hover:shadow-lg hover:shadow-emerald-500/30 hover:scale-[1.02]",
                warning:
                    "bg-[hsl(var(--gov-warning))] text-white shadow-md shadow-amber-500/20 font-bold hover:opacity-100 hover:shadow-lg hover:shadow-amber-500/30 hover:scale-[1.02]",
                info:
                    "bg-[hsl(var(--gov-info))] text-white shadow-md shadow-blue-500/20 font-bold hover:opacity-100 hover:shadow-lg hover:shadow-blue-500/30 hover:scale-[1.02]",
=======
                    "bg-[hsl(var(--gov-success))] text-white shadow-md shadow-emerald-500/20 font-bold hover:opacity-90 hover:shadow-lg hover:shadow-emerald-500/30 hover:scale-[1.02]",
                warning:
                    "bg-[hsl(var(--gov-warning))] text-white shadow-md shadow-amber-500/20 font-bold hover:opacity-90 hover:shadow-lg hover:shadow-amber-500/30 hover:scale-[1.02]",
                info:
                    "bg-[hsl(var(--gov-info))] text-white shadow-md shadow-blue-500/20 font-bold hover:opacity-90 hover:shadow-lg hover:shadow-blue-500/30 hover:scale-[1.02]",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                outline:
                    "border border-input bg-background hover:bg-accent hover:text-accent-foreground font-medium",
                ghost:
                    "hover:bg-accent hover:text-accent-foreground font-medium",
                "ghost-action":
<<<<<<< HEAD
                    "text-primary border border-transparent hover:border-primary hover:bg-primary/5 hover:shadow-sm font-bold rounded-lg transition-all",
=======
                    "text-primary border border-transparent hover:border-primary/20 hover:bg-primary/5 hover:shadow-sm font-bold rounded-lg transition-all",
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                link:
                    "text-primary underline-offset-4 hover:underline font-medium",
            },
            size: {
                sm: "min-h-[44px] h-11 px-3 text-xs rounded-md",
                md: "min-h-[44px] h-11 px-4 py-2",
                lg: "h-12 px-6 text-base rounded-lg",
                icon: "min-h-[44px] min-w-[44px] h-11 w-11",
            },
        },
        defaultVariants: {
            variant: "primary",
            size: "md",
        },
    }
);

export type ButtonVariantProps = VariantProps<typeof buttonVariants>;

