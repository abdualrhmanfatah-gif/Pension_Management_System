import { useQuery } from "@tanstack/react-query";
import {
Ban,
ClipboardList,
FileCheck2, FileSignature, FileText,     FileWarning, Filter,
    History, Pencil, Plus,     RefreshCw, ShieldCheck,     StickyNote, Trash2,
UserCheck, Users, Wallet} from "lucide-react";
import { useCallback, useDeferredValue,useMemo, useState } from "react";

import { retirementApi } from "@/api/retirement";
<<<<<<< HEAD
import { Button, Card, Input, LoadingIndicator, TabHeader } from "@/components/ui";
=======
import { Button, Card, Heading, Input, LoadingIndicator, Text } from "@/components/ui";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { AuditTimeline } from "@/components/ui/AuditTimeline";
import { translateFieldName } from "@/lib/fieldTranslations";
import type { ChangeLogItem } from "@/types/retirement";

// ─── Types ─────────────────────────────────────────────

type FilterType = "all" | "StatusChange" | "Create" | "Update" | "Delete";

interface StatusTransitionsTabProps {
    fileId: number;
}

// ─── Constants ─────────────────────────────────────────

const FILTER_BUTTONS = [
    { key: "all" as FilterType, label: "الكل", icon: Filter },
    { key: "StatusChange" as FilterType, label: "تغيير الحالة", icon: RefreshCw },
    { key: "Create" as FilterType, label: "إنشاء", icon: Plus },
    { key: "Update" as FilterType, label: "تعديل", icon: Pencil },
    { key: "Delete" as FilterType, label: "حذف", icon: Trash2 },
] as const;

// Map أكثر موثوقية من includes()
const ENTITY_ICON_MAP = new Map<string, React.FC<{ className?: string }>>([
    ["المستحق", Users],
    ["الوكلاء", UserCheck],
    ["الأوصياء", ShieldCheck],
    ["الوثائق الناقصة", FileWarning],
    ["الوثائق", FileCheck2],
    ["التعهدات", FileSignature],
    ["إعفاء", ClipboardList],
    ["ملاحظات", StickyNote],
    ["إيقاف", Ban],
    ["مستلم", Wallet],
]);

// ─── Helpers ───────────────────────────────────────────

const getEntityIcon = (entityName?: string | null) => {
    if (!entityName) return <FileText className="w-4 h-4" />;
    
    const name = entityName.toLowerCase();
    for (const [key, Icon] of ENTITY_ICON_MAP) {
        if (name.includes(key)) return <Icon className="w-4 h-4" />;
    }
    return <FileText className="w-4 h-4" />;
};

const formatAuditLog = (c: ChangeLogItem, idx: number) => ({
    id: c.id ?? idx, // ✅ استخدام id الحقيقي إن وُجد
    action: c.changeType,
    recordId: String(c.entityKey ?? 0),
    changedAt: c.changeDate,
    changedByUser: c.changedByUserName || "النظام",
    title: c.changeType || "",
    description: c.description || "",
    changeType: c.changeType || "",
    entityName: c.entityName || "",
    entityDisplayName: c.entityDisplayName || "",
    oldStatusName: c.oldStatusName || "",
    newStatusName: c.newStatusName || "",
    reason: c.reason || "",
    fieldChanges: c.fieldChanges?.map(fc => ({
        fieldName: fc.fieldName,
        displayName: translateFieldName(fc.fieldName),
        oldValue: fc.oldValue ?? null,
        newValue: fc.newValue ?? null,
    })),
    date: c.changeDate || "",
    userName: c.changedByUserName || "",
    ipAddress: c.ipAddress || "",
    userAgent: c.userAgent || "",
    correlationId: c.correlationId || "",
});

// ─── Component ─────────────────────────────────────────

export function StatusTransitionsTab({ fileId }: StatusTransitionsTabProps) {
    const [filter, setFilter] = useState<FilterType>("all");
    const [entityFilter, setEntityFilter] = useState<string>("all");
    const [search, setSearch] = useState("");

    // ✅ useDeferredValue: يمنع التأخير أثناء الكتابة
    const deferredSearch = useDeferredValue(search);

    // جلب البيانات مع معالجة الأخطاء
    const { 
        data: changes = [], 
        isLoading, 
        error,
        refetch,
        isFetching 
    } = useQuery({
        queryKey: ["file-change-log", fileId],
        queryFn: () => retirementApi.getChangeLog(fileId),
        staleTime: 30_000, // ✅ 30 ثانية قبل إعادة الجلب التلقائي
    });

    // ✅ useCallback للمعالجات
    const handleFilterChange = useCallback((key: FilterType) => {
        setFilter(key);
    }, []);

    const handleEntityFilterChange = useCallback((entity: string) => {
        setEntityFilter(entity);
    }, []);

    const handleSearchChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
        setSearch(e.target.value);
    }, []);

    // استخراج الكيانات الفريدة
    const uniqueEntities = useMemo(() => 
        Array.from(new Set(changes.map((c: ChangeLogItem) => c.entityName).filter(Boolean))) as string[],
    [changes]);

    // ✅ الفلترة مع البحث المؤجل
    const filtered = useMemo(() => {
        const term = deferredSearch.trim().toLowerCase();
        
        return changes.filter((c: ChangeLogItem) => {
            const matchType = filter === "all" || c.changeType === filter;
            const matchEntity = entityFilter === "all" || c.entityName === entityFilter;
            
            if (!matchType || !matchEntity) return false;
            if (!term) return true;

            return (
                c.description?.toLowerCase().includes(term) ||
                c.entityName?.toLowerCase().includes(term) ||
                c.entityDisplayName?.toLowerCase().includes(term) ||
                c.reason?.toLowerCase().includes(term) ||
                c.changedByUserName?.toLowerCase().includes(term)
            );
        });
    }, [changes, filter, entityFilter, deferredSearch]);

    // ✅ تحويل البيانات مرة واحدة فقط
    const timelineLogs = useMemo(() => 
        filtered.map(formatAuditLog),
    [filtered]);

    // ─── Loading State ─────────────────────────────────
    if (isLoading) {
        return <LoadingIndicator message="جاري تحميل سجل التحولات..." />;
    }

    // ─── Error State ─────────────────────────────────────
    if (error) {
        return (
<<<<<<< HEAD
            <div className="border-2 border-destructive rounded-2xl py-16 text-center text-destructive bg-destructive/5">
                <FileWarning className="w-12 h-12 mx-auto mb-4 opacity-100" />
=======
            <div className="border-2 border-destructive/20 rounded-2xl py-16 text-center text-destructive bg-destructive/5">
                <FileWarning className="w-12 h-12 mx-auto mb-4 opacity-50" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                <p className="font-medium mb-2">حدث خطأ أثناء تحميل السجل</p>
                <Button variant="outline" size="sm" onClick={() => refetch()} className="gap-1.5">
                    <RefreshCw className="w-4 h-4" /> إعادة المحاولة
                </Button>
            </div>
        );
    }

    // ─── Empty State ───────────────────────────────────
    if (changes.length === 0) {
        return (
            <div className="border-2 border-dashed rounded-2xl py-16 text-center text-muted-foreground bg-muted/10 animate-in fade-in duration-500">
<<<<<<< HEAD
                <History className="w-12 h-12 mx-auto mb-4 opacity-100" />
=======
                <History className="w-12 h-12 mx-auto mb-4 opacity-20" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                <p className="font-medium">لا توجد تغييرات مسجلة لهذا الملف حتى الآن</p>
            </div>
        );
    }

    return (
        <div className="space-y-3 md:space-y-6 animate-in fade-in duration-500 text-start">
            {/* Header */}
<<<<<<< HEAD
            <TabHeader
                title="سجل العمليات"
                description="تتبع الأنشطة والتعديلات التي تمت على الملف"
                icon={History}
            >
                <div className="flex items-center gap-2 flex-wrap md:flex-nowrap">
=======
            <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4 bg-muted/30 p-4 rounded-xl border border-border/50">
                <div className="space-y-1">
                    <Heading level="h3" className="text-lg font-bold flex items-center gap-2">
                        <History className="w-5 h-5 text-primary" /> 
                        سجل العمليات
                    </Heading>
                    <Text variant="caption" className="font-medium">
                        تتبع الأنشطة والتعديلات التي تمت على الملف
                    </Text>
                </div>
                <div className="flex items-center gap-2 flex-wrap lg:flex-nowrap">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    <Input
                        value={search}
                        onChange={handleSearchChange}
                        placeholder="بحث في الوصف أو المستخدم..."
<<<<<<< HEAD
                        className="h-9 w-full sm:w-64 text-xs bg-background"
                        aria-label="البحث في سجل التغييرات"
                    />
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => refetch()}
                        className="gap-1.5 h-8 md:h-10 text-xs md:text-sm"
                        disabled={isFetching}
                        aria-label="تحديث السجل"
                    >
                        <RefreshCw className={`w-3.5 h-3.5 md:w-4 md:h-4 ${isFetching ? "animate-spin" : ""}`} />
                        تحديث
                    </Button>
                </div>
            </TabHeader>
=======
                        className="h-9 w-full sm:w-64 text-xs bg-background "
                        aria-label="البحث في سجل التغييرات"
                    />
                    <Button 
                        variant="outline" 
                        size="sm" 
                        onClick={() => refetch()} 
                        className="h-11 gap-1.5 "
                        disabled={isFetching}
                        aria-label="تحديث السجل"
                    >
                        <RefreshCw className={`w-4 h-4 ${isFetching ? "animate-spin" : ""}`} /> 
                        تحديث
                    </Button>
                </div>
            </div>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

            {/* Filters */}
            <div className="space-y-3">
                {/* نوع التغيير */}
                <div 
<<<<<<< HEAD
                    className="flex gap-2 flex-wrap p-1 bg-muted/50 rounded-lg border border-border w-fit"
=======
                    className="flex gap-2 flex-wrap p-1 bg-muted/50 rounded-lg border border-border/40 w-fit"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    role="group"
                    aria-label="فلترة حسب نوع التغيير"
                >
                    {FILTER_BUTTONS.map(({ key, label, icon: Icon }) => (
                        <Button
                            key={key}
                            variant="ghost"
                            size="sm"
                            onClick={() => handleFilterChange(key)}
                            aria-pressed={filter === key}
                            className={`flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold
                                ${filter === key
                                    ? "bg-background text-primary  ring-1 ring-border"
                                    : "text-muted-foreground"
                                }`}
                        >
                            <Icon className="w-4 h-4" /> {label}
                            {key === "all" && (
                                <span className="ms-1 px-1.5 py-0.5 rounded-full bg-primary/10 text-primary text-[10px] sm:text-xs font-mono">
                                    {changes.length}
                                </span>
                            )}
                        </Button>
                    ))}
                </div>

                {/* فلترة الكيانات */}
                {uniqueEntities.length > 1 && (
                    <div 
<<<<<<< HEAD
                        className="flex gap-2 flex-wrap p-1 bg-muted/50 rounded-lg border border-border w-fit"
=======
                        className="flex gap-2 flex-wrap p-1 bg-muted/50 rounded-lg border border-border/40 w-fit"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        role="group"
                        aria-label="فلترة حسب الكيان"
                    >
                        <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleEntityFilterChange("all")}
                            aria-pressed={entityFilter === "all"}
                            className={`flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold
                                ${entityFilter === "all"
                                    ? "bg-background text-primary  ring-1 ring-border"
                                    : "text-muted-foreground"
                                }`}
                        >
                            <Filter className="w-4 h-4" /> الكل
                        </Button>
                        {uniqueEntities.map(entity => (
                            <Button
                                key={entity}
                                variant="ghost"
                                size="sm"
                                onClick={() => handleEntityFilterChange(entity)}
                                aria-pressed={entityFilter === entity}
                                className={`flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold
                                    ${entityFilter === entity
                                        ? "bg-background text-primary  ring-1 ring-border"
                                        : "text-muted-foreground"
                                    }`}
                            >
                                {getEntityIcon(entity)} {entity}
                            </Button>
                        ))}
                    </div>
                )}
            </div>

            {/* Timeline */}
            <Card className="p-6  rounded-2xl overflow-hidden">
                <AuditTimeline logs={timelineLogs} />
            </Card>
        </div>
    );
<<<<<<< HEAD
}
=======
}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
