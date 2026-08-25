import { cn } from "@/lib/utils";

interface PageLayoutProps {
    children: React.ReactNode;
    className?: string;
    maxWidth?: string;
}

export function PageLayout({
    children,
    className,
    maxWidth = "max-w-[1600px]",
}: PageLayoutProps) {
    return (
        <div
            className={cn(
                "w-full space-y-6 animate-in fade-in duration-500 mx-auto",
                maxWidth,
                className
            )}
            dir="rtl"
        >
            {children}
        </div>
    );
}

interface StickyHeaderProps {
    children: React.ReactNode;
    className?: string;
}

export function StickyHeader({ children, className }: StickyHeaderProps) {
    return (
        <div
            className={cn(
                "sticky z-30 bg-background/95 backdrop-blur-sm border-b -mx-4 px-4 sm:-mx-6 sm:px-6 py-4 print:hidden",
                className
            )}
        >
            {children}
        </div>
    );
}
