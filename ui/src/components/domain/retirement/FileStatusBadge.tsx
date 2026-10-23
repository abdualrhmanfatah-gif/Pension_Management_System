import { Badge, type BadgeProps } from "@/components/ui/Badge";
import { FILE_STATUS, retirementFileStatusName } from "@/constants/retirement";

const STATUS_VARIANTS: Readonly<Record<number, BadgeProps["variant"]>> = {
    [FILE_STATUS.New]: "outline",
    [FILE_STATUS.Active]: "success",
    [FILE_STATUS.EntitlementEnded]: "secondary",
    [FILE_STATUS.Pending]: "warning",
};

interface FileStatusBadgeProps {
    statusId: number | null | undefined;
    statusName?: string | null;
    className?: string;
}

/** The files API returns statusID and fileStatusName, without a color field. */
export function FileStatusBadge({ statusId, className }: FileStatusBadgeProps) {
    return (
        <Badge variant={STATUS_VARIANTS[statusId ?? 0] ?? "outline"} className={className}>
            {retirementFileStatusName(statusId)}
        </Badge>
    );
}
