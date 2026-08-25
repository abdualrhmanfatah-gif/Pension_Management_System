import { Heading, Text } from "@/components/ui";
import { cn } from "@/lib/utils";

interface DashboardHeaderProps {
    title: string;
    description?: string;
    icon?: React.ReactNode;
    actions?: React.ReactNode;
    className?: string;
}

export function DashboardHeader({
    title,
    description,
    icon,
    actions,
    className
}: DashboardHeaderProps) {
    return (
        <div className={cn("relative overflow-hidden rounded-2xl border border-primary/10 bg-gradient-to-br from-primary/5 via-background to-primary/[0.02] mb-8", className)}>
            {/* خلفية مائية */}
            <div
                className="absolute inset-0 opacity-[0.04] dark:opacity-[0.07] pointer-events-none"
                style={{ backgroundImage: "url('/pension-logo.png')", backgroundSize: "200px", backgroundRepeat: "repeat", backgroundPosition: "center" }}
            />
            <div className="relative flex flex-col md:flex-row md:items-center justify-between gap-4 p-4 md:p-6">
                <div className="flex items-center gap-3 md:gap-4">
                    {/* شعار النظام */}
                    <div className="shrink-0 p-2 rounded-xl bg-primary/10 border border-primary/20 shadow-sm">
                        <picture>
                            <source srcSet="/pension-logo.webp" type="image/webp" />
                            <img src="/pension-logo.png" alt="شعار النظام" className="w-10 h-10 md:w-12 md:h-12 object-contain" />
                        </picture>
                    </div>
                    <div className="space-y-0.5">
                        <div className="flex items-center gap-2">
                            {icon && <span className="text-primary">{icon}</span>}
                            <Heading level="h1">{title}</Heading>
                        </div>
                        <p className="text-[10px] md:text-xs text-primary/70 font-bold tracking-wide">الإدارة العامة لصندوق التقاعد الأمني</p>
                        {description && (
                            <Text variant="description" className="mt-0.5">{description}</Text>
                        )}
                    </div>
                </div>
                {actions && (
                    <div className="flex items-center gap-3 flex-wrap">
                        {actions}
                    </div>
                )}
            </div>
        </div>
    );
}
