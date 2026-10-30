import { 
    ArrowDownRight,
    ArrowLeftRight,
    Clock, 
    Inbox,
    Layers,
    PlusCircle, 
    Trash2, 
    User} from "lucide-react";
import React, { useMemo } from "react";

import { 
    Badge, 
    Card,
    EmptyState,
    Skeleton
} from "@/components/ui";
import { AUDIT_CHANGE_TYPE, BOOLEAN_STRING } from "@/constants";

// ─── Constants ──────────────────────────────────────────

const CHANGE_TYPE_CONFIG = {
    [AUDIT_CHANGE_TYPE.CREATE]: {
        label: "إضافة جديد",
        icon: <PlusCircle className="w-4 h-4" />,
        variant: "success" as const,
        bgColor: "bg-emerald-500/10",
        textColor: "text-emerald-600",
        borderColor: "border-emerald-200"
    },
    [AUDIT_CHANGE_TYPE.DELETE]: {
        label: "حذف سجل",
        icon: <Trash2 className="w-4 h-4" />,
        variant: "danger" as const,
        bgColor: "bg-rose-500/10",
        textColor: "text-rose-600",
        borderColor: "border-rose-200"
    },
    [AUDIT_CHANGE_TYPE.STATUS_CHANGE]: {
        label: "تغيير حالة",
        icon: <ArrowLeftRight className="w-4 h-4" />,
        variant: "info" as const,
        bgColor: "bg-blue-500/10",
        textColor: "text-blue-600",
        borderColor: "border-blue-200"
    },
    "Update": {
        label: "تحديث بيانات",
        icon: <Layers className="w-4 h-4" />,
        variant: "warning" as const,
        bgColor: "bg-amber-500/10",
        textColor: "text-amber-600",
        borderColor: "border-amber-200"
    }
} as const;

// ─── Types ─────────────────────────────────────────────

export interface AuditFieldChange {
    fieldName: string;
    displayName: string;
    oldValue: string | null;
    newValue: string | null;
}

export interface AuditLogItem {
    id: number;
    action: string;
    changeType: string;
    entityName: string;
    recordId: string;
    oldStatusName?: string | null;
    newStatusName?: string | null;
    fieldChanges?: AuditFieldChange[];
    changedByUser: string;
    changedAt: string;
    ipAddress?: string | null;
    correlationId?: string | null;
}

interface AuditTimelineProps {
    logs: AuditLogItem[];
    isLoading?: boolean;
    hasCorrelation?: boolean;
}

/**
 * AuditTimeline — Reusable audit-trail timeline component.
 * Groups logs by date and correlationId.
 */
export function AuditTimeline({ logs, isLoading, hasCorrelation = true }: AuditTimelineProps) {
    // 1. Group logs by Day
    const groupedByDate = useMemo(() => {
        if (!logs) return [];
        
        const groups: { key: string, items: AuditLogItem[] }[] = [];
        logs.forEach(log => {
            const dateKey = new Date(log.changedAt).toLocaleDateString("ar-YE", { 
                weekday: 'long', 
                year: 'numeric', 
                month: 'long', 
                day: 'numeric' 
            });
            
            let group = groups.find(g => g.key === dateKey);
            if (!group) {
                group = { key: dateKey, items: [] };
                groups.push(group);
            }
            group.items.push(log);
        });
        return groups;
    }, [logs]);

    const formatValue = (val: string | null) => {
<<<<<<< HEAD
        if (val === null || val === "null" || val === "") return <span className="text-muted-foreground italic">فارغ</span>;
=======
        if (val === null || val === "null" || val === "") return <span className="text-muted-foreground/40 italic">فارغ</span>;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        
        // Arabic translation for booleans
        const v = val.toLowerCase();
        if (v === BOOLEAN_STRING.TRUE) return "نعم";
        if (v === BOOLEAN_STRING.FALSE) return "لا";
        
        return val;
    };

    const filterFieldChanges = (changes: AuditFieldChange[]) => {
        // Hide standard internal fields
        const hidden = ["RowVersion", "UpdatedAt", "UpdatedByUserID", "IsDeleted"];
        return changes.filter(c => !hidden.includes(c.fieldName));
    };

    if (isLoading) return <AuditTimelineSkeleton />;
    if (!logs || logs.length === 0) return (
        <div className="py-20" dir="rtl">
            <EmptyState icon={Inbox} title="لا توجد سجلات تدقيق حالياً." />
        </div>
    );

    return (
        <div className="relative space-y-12 pb-10 text-start" dir="rtl">
            {/* Vertical Line */}
            <div className="absolute top-0 bottom-0 start-[19px] w-0.5 bg-gradient-to-b from-primary/20 via-border/50 to-transparent" />

            {groupedByDate.map((group) => (
                <div key={group.key} className="relative space-y-6">
                    {/* Date Header */}
                    <div className="relative flex items-center gap-4">
<<<<<<< HEAD
                        <div className="z-10 w-10 h-10 rounded-full bg-background border-2 border-primary flex items-center justify-center">
                            <Clock className="w-4 h-4 text-primary" />
                        </div>
                        <div className="bg-primary/5 px-4 py-1.5 rounded-full border border-primary">
=======
                        <div className="z-10 w-10 h-10 rounded-full bg-background border-2 border-primary/30 flex items-center justify-center">
                            <Clock className="w-4 h-4 text-primary" />
                        </div>
                        <div className="bg-primary/5 px-4 py-1.5 rounded-full border border-primary/10">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                            <span className="text-xs font-black text-primary tracking-tight">{group.key}</span>
                        </div>
                    </div>

                    {/* Events in this day */}
                    <div className="space-y-8">
                        {group.items.map((item, idx) => {
                            const visibleChanges = item.fieldChanges ? filterFieldChanges(item.fieldChanges) : [];
                            const isCreate = item.changeType === AUDIT_CHANGE_TYPE.CREATE;
                            const isDelete = item.changeType === AUDIT_CHANGE_TYPE.DELETE;
                            const config = CHANGE_TYPE_CONFIG[item.changeType as keyof typeof CHANGE_TYPE_CONFIG] || CHANGE_TYPE_CONFIG["Update"];

                            return (
                            <div 
                                key={`${group.key}-${idx}`} 
                                className={`relative flex items-start gap-4 pe-1 ${hasCorrelation && item.correlationId ? "ms-4" : ""}`}
                            >
                                {/* Bullet */}
                                <div className={`z-10 w-2 h-2 rounded-full mt-5 shrink-0 ${isCreate ? "bg-emerald-500" : isDelete ? "bg-rose-500" : "bg-primary"}`} />

<<<<<<< HEAD
                                <Card className="flex-1 overflow-hidden border-border group">
=======
                                <Card className="flex-1 overflow-hidden border-border/60 group">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                    <div className="p-4 sm:p-5 text-start">
                                        {/* Header */}
                                        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mb-4">
                                            <div className="flex items-center gap-3">
                                                <div className={`p-2 rounded-xl border ${config.borderColor} ${config.bgColor}`}>
                                                    {React.cloneElement(config.icon, { 
                                                        className: `w-4 h-4 ${config.textColor}` 
                                                    })}
                                                </div>
                                                <div>
                                                    <h4 className="font-bold text-sm text-foreground flex items-center gap-1.5">
                                                        {config.label}
<<<<<<< HEAD
                                                        <span className="text-xs font-normal text-muted-foreground opacity-100">/</span>
                                                        <span className="text-xs font-bold text-primary">{item.entityName}</span>
=======
                                                        <span className="text-xs font-normal text-muted-foreground opacity-40">/</span>
                                                        <span className="text-xs font-bold text-primary/80">{item.entityName}</span>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                    </h4>
                                                    <div className="flex items-center gap-2 mt-0.5">
                                                        <div className="flex items-center gap-1 text-caption sm:text-xs text-muted-foreground font-medium">
                                                            <User className="w-3 h-3" />
                                                            {item.changedByUser}
                                                        </div>
<<<<<<< HEAD
                                                        <span className="text-caption sm:text-xs text-muted-foreground">•</span>
=======
                                                        <span className="text-caption sm:text-xs text-muted-foreground/30">•</span>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                        <span className="text-caption sm:text-xs font-mono text-muted-foreground">{new Date(item.changedAt).toLocaleTimeString("ar-YE", { hour: '2-digit', minute: '2-digit' })}</span>
                                                    </div>
                                                </div>
                                            </div>

                                            <div className="flex items-center gap-2 self-end sm:self-center">
<<<<<<< HEAD
<Badge variant="outline" className="font-mono text-xsmall h-5 opacity-100">ID: {item.recordId}</Badge>
                                                 {item.ipAddress && <Badge variant="ghost" className="font-mono text-xsmall h-5 opacity-100">IP: {item.ipAddress}</Badge>}
=======
<Badge variant="outline" className="font-mono text-xsmall h-5 opacity-60">ID: {item.recordId}</Badge>
                                                 {item.ipAddress && <Badge variant="ghost" className="font-mono text-xsmall h-5 opacity-40">IP: {item.ipAddress}</Badge>}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            </div>
                                        </div>

                                        {/* Status Transition */}
                                        {item.changeType === AUDIT_CHANGE_TYPE.STATUS_CHANGE && (item.oldStatusName || item.newStatusName) && (
<<<<<<< HEAD
                                            <div className="bg-primary/[0.03] border border-primary rounded-2xl p-4 mb-3 flex items-center justify-center gap-6">
                                                <div className="text-center space-y-1">
                                                    <p className="text-xsmall uppercase font-black tracking-widest text-muted-foreground">الحالة السابقة</p>
=======
                                            <div className="bg-primary/[0.03] border border-primary/10 rounded-2xl p-4 mb-3 flex items-center justify-center gap-6">
                                                <div className="text-center space-y-1">
                                                    <p className="text-xsmall uppercase font-black tracking-widest text-muted-foreground/60">الحالة السابقة</p>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                    <Badge variant="outline" className="h-7 px-3">{item.oldStatusName || "—"}</Badge>
                                                </div>
                                                <div className="p-2 rounded-full bg-primary/10 text-primary">
                                                    <ArrowLeftRight className="w-4 h-4" />
                                                </div>
                                                <div className="text-center space-y-1">
                                                    <p className="text-xsmall uppercase font-black text-muted-primary">الحالة الجديدة</p>
                                                    <Badge className="h-7 px-3">{item.newStatusName || "—"}</Badge>
                                                </div>
                                            </div>
                                        )}

                                        {/* Field Changes Table */}
                                        {visibleChanges.length > 0 && (
<<<<<<< HEAD
                                            <div className="mt-4 border border-border rounded-xl overflow-x-auto bg-muted/20">
                                                 <table className="w-full text-xs text-start" role="table" aria-label="مقارنة القيم">
                                                    <thead>
                                                        <tr className="bg-muted/40 border-b border-border">
=======
                                            <div className="mt-4 border border-border/50 rounded-xl overflow-x-auto bg-muted/20">
                                                 <table className="w-full text-xs text-start" role="table" aria-label="مقارنة القيم">
                                                    <thead>
                                                        <tr className="bg-muted/40 border-b border-border/50">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                            <th className="px-3 py-2 font-bold text-muted-foreground w-1/3 text-start">الحقل</th>
                                                            <th className="px-3 py-2 font-bold text-muted-foreground text-start">القيمة السابقة</th>
                                                            <th className="px-3 py-2 font-bold text-primary text-start">القيمة الجديدة</th>
                                                        </tr>
                                                    </thead>
<<<<<<< HEAD
                                                    <tbody className="divide-y divide-border">
=======
                                                    <tbody className="divide-y divide-border/40">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                        {visibleChanges.map((change, cIdx) => (
                                                            <tr key={cIdx} className="hover:bg-background/50 transition-colors">
                                                                <td className="px-3 py-2 font-medium text-foreground">{change.displayName}</td>
                                                                <td className="px-3 py-2 text-muted-foreground line-through decoration-muted-foreground/30">{formatValue(change.oldValue)}</td>
                                                                <td className="px-3 py-2 font-bold text-foreground">
                                                                    <div className="flex items-center gap-1.5">
<<<<<<< HEAD
                                                                        <ArrowDownRight className="w-3 h-3 text-emerald-500 opacity-100" />
=======
                                                                        <ArrowDownRight className="w-3 h-3 text-emerald-500 opacity-50" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                                        {formatValue(change.newValue)}
                                                                    </div>
                                                                </td>
                                                            </tr>
                                                        ))}
                                                    </tbody>
                                                </table>
                                            </div>
                                        )}
                                        
                                        {/* No visible changes placeholder */}
                                        {!visibleChanges.length && item.changeType === "Update" && (
<<<<<<< HEAD
                                            <div className="text-caption sm:text-xs text-muted-foreground italic text-center py-2 opacity-100">
=======
                                            <div className="text-caption sm:text-xs text-muted-foreground italic text-center py-2 opacity-60">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                                تحديث سجل (لم تتغير أي حقول عرض)
                                            </div>
                                        )}
                                    </div>
                                </Card>
                            </div>
                            );
                        })}
                    </div>
                </div>
            ))}
        </div>
    );
}

function AuditTimelineSkeleton() {
    return (
<<<<<<< HEAD
        <div className="space-y-12 pb-10 opacity-100" dir="rtl">
=======
        <div className="space-y-12 pb-10 opacity-50" dir="rtl">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            {[1, 2].map(g => (
                <div key={g} className="space-y-6 text-start">
                    <div className="flex items-center gap-4">
                        <Skeleton className="w-10 h-10 rounded-full" />
                        <Skeleton className="h-6 w-48 rounded-full" />
                    </div>
                    <div className="space-y-8 ms-4 ps-1">
                        {[1, 2].map(i => (
                            <Skeleton key={i} className="h-32 w-full rounded-3xl" />
                        ))}
                    </div>
                </div>
            ))}
        </div>
    );
}
