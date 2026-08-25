import { cva, type VariantProps } from "class-variance-authority";

export const badgeVariants = cva(
    "inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-medium transition-colors",
    {
        variants: {
            variant: {
                success:
                    "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-400",
                warning:
                    "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400",
                danger:
                    "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400",
                destructive:
                    "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400",
                info:
                    "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400",
                ghost:
                    "bg-muted text-muted-foreground",
                secondary:
                    "bg-slate-100 text-slate-900 dark:bg-slate-800 dark:text-slate-100",
                outline:
                    "border border-current bg-transparent",
                default:
                    "bg-primary/10 text-primary font-medium",
            },
        },
        defaultVariants: {
            variant: "ghost",
        },
    }
);

export type BadgeVariantProps = VariantProps<typeof badgeVariants>;

