import { cn } from "@/lib/utils";

import { type RecordState,stateConfig } from "./RecordStateBadge.config";

interface RecordStateBadgeProps {
    state: RecordState;
    className?: string;
    showIcon?: boolean;
}
// ─── Component ─────────────────────────────────────────

export function RecordStateBadge({ state, className, showIcon = true }: RecordStateBadgeProps) {
    const config = stateConfig[state];
    if (!config) return null;

    const Icon = config.icon;

    return (
        <span
            className={cn(
                "inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md border text-xs font-semibold",
                config.className,
                className
            )}
        >
            {showIcon && <Icon className="w-3.5 h-3.5" />}
            {config.label}
        </span>
    );
}

export type { RecordState, RecordStateBadgeProps };
