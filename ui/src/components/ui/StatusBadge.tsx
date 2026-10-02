import { Badge } from "./Badge";
import type { BadgeVariantProps } from "./badgeVariants";

type StatusBadgeVariant = BadgeVariantProps["variant"];

export interface StatusConfig {
    label: string;
    variant: StatusBadgeVariant;
}

interface StatusBadgeProps {
    status: string;
    config: Record<string, StatusConfig>;
    className?: string;
    fallbackLabel?: string;
}

/**
 * StatusBadge — A config-driven badge for displaying status with consistent styling.
 * Eliminates duplicated status→variant mapping logic across 34+ files.
 *
 * @example
 * const VOUCHER_STATUS = {
 *   "مسودة": { label: "مسودة", variant: "ghost" },
 *   "مرحّل": { label: "مرحّل", variant: "success" },
 *   "ملغي": { label: "ملغي", variant: "danger" },
 * };
 * <StatusBadge status={voucher.status} config={VOUCHER_STATUS} />
 */
export function StatusBadge({ status, config, className, fallbackLabel }: StatusBadgeProps) {
    const match = config[status];
    const label = match?.label ?? fallbackLabel ?? status;
    const variant = match?.variant ?? "ghost";

    return (
        <Badge variant={variant} className={className}>
            {label}
        </Badge>
    );
}

export type { StatusBadgeProps };
