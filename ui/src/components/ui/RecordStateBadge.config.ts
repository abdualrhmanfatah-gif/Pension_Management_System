import {
    AlertTriangle,
    Archive,
    CheckCircle2,
    FileEdit,
    Lock,
    Search,
    XCircle,
} from "lucide-react";

export type RecordState =
    | "draft"
    | "under_review"
    | "approved"
    | "archived"
    | "locked"
    | "suspended"
    | "rejected";

export const stateConfig: Record<RecordState, {
    label: string;
    icon: React.ElementType;
    className: string;
}> = {
    draft: {
        label: "مسودة",
        icon: FileEdit,
        className: "bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-950/30 dark:text-amber-400 dark:border-amber-800",
    },
    under_review: {
        label: "قيد المراجعة",
        icon: Search,
        className: "bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-950/30 dark:text-blue-400 dark:border-blue-800",
    },
    approved: {
        label: "معتمد",
        icon: CheckCircle2,
        className: "bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/30 dark:text-emerald-400 dark:border-emerald-800",
    },
    archived: {
        label: "مؤرشف",
        icon: Archive,
        className: "bg-slate-50 text-slate-500 border-slate-200 dark:bg-slate-950/30 dark:text-slate-400 dark:border-slate-700",
    },
    locked: {
        label: "مقفل",
        icon: Lock,
        className: "bg-slate-100 text-slate-600 border-slate-300 dark:bg-slate-900 dark:text-slate-400 dark:border-slate-700",
    },
    suspended: {
        label: "موقوف",
        icon: AlertTriangle,
        className: "bg-red-50 text-red-700 border-red-200 dark:bg-red-950/30 dark:text-red-400 dark:border-red-800",
    },
    rejected: {
        label: "مرفوض",
        icon: XCircle,
        className: "bg-red-50 text-red-600 border-red-200 dark:bg-red-950/30 dark:text-red-400 dark:border-red-800",
    },
};

