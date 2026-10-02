import { useQuery } from "@tanstack/react-query";
import { Clock, History, Monitor, Tag, User } from "lucide-react";
import { useEffect, useMemo } from "react";
import { toast } from "sonner";

import api from "@/api/axios";
import { Badge, Card, type CustomColumnDef, DataTable, ErrorState, Heading, LoadingIndicator } from "@/components/ui";

// ─── Types ─────────────────────────────────────────────

export interface AuditLogItem {
    logID: number;
    userName: string;
    action: string;
    oldValues: string;
    newValues: string;
    ipAddress: string;
    createdAt: string;
}

interface PropertyActivityLogTabProps {
    propertyId: number;
}

/**
 * تبويب سجل الأنشطة والتدقيق للأصل العقاري
 */
export function PropertyActivityLogTab({ propertyId }: PropertyActivityLogTabProps) {
    // المعرف 102 هو TableID الافتراضي لجدول العقارات الاستثمارية
    const TABLE_ID_PROPERTIES = 102; 

    const { data: logsRes, isLoading, error } = useQuery({
        queryKey: ["property-audit-logs", propertyId],
        queryFn: () => api.get<{ data: AuditLogItem[] }>(`/admin/audit-logs/by-record/${TABLE_ID_PROPERTIES}/${propertyId}`).then(r => r.data)
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل سجل التدقيق", { id: "property-activity-log" });
    }, [error]);

    const logs = logsRes?.data || [];

    const getActionBadge = (action: string) => {
        const lowerAction = action.toLowerCase();
        if (lowerAction.includes("inserted") || lowerAction.includes("create")) {
            return <Badge variant="success" className="gap-1.5 rounded-lg"><Tag className="w-3 h-3" /> إضافة</Badge>;
        }
        if (lowerAction.includes("updated") || lowerAction.includes("update")) {
            return <Badge variant="info" className="gap-1.5 rounded-lg"><Tag className="w-3 h-3" /> تعديل</Badge>;
        }
        if (lowerAction.includes("deleted") || lowerAction.includes("delete")) {
            return <Badge variant="destructive" className="gap-1.5 rounded-lg"><Tag className="w-3 h-3" /> حذف</Badge>;
        }
        return <Badge variant="outline" className="rounded-lg">{action}</Badge>;
    };

    const columns = useMemo<CustomColumnDef<AuditLogItem>[]>(() => [
        { 
            key: "createdAt", 
            header: "الوقت والتاريخ", 
            render: (row) => (
                <div className="flex items-center gap-2 text-xs font-mono">
                    <Clock className="w-3.5 h-3.5 text-muted-foreground" />
                    <span>{new Date(row.createdAt).toLocaleString("ar-YE")}</span>
                </div>
            )
        },
        { 
            key: "userName", 
            header: "المستخدم", 
            render: (row) => (
                <div className="flex items-center gap-2">
                    <div className="w-7 h-7 rounded-full bg-primary/10 flex items-center justify-center text-primary">
                        <User className="w-3.5 h-3.5" />
                    </div>
                    <span className="font-bold text-sm">{row.userName}</span>
                </div>
            )
        },
        { 
            key: "action", 
            header: "نوع الإجراء", 
            render: (row) => getActionBadge(row.action)
        },
        { 
            key: "details", 
            header: "تفاصيل التغيير", 
            render: (row) => {
                if (!row.oldValues && !row.newValues) return <span className="text-muted-foreground text-xs italic">لا توجد تفاصيل</span>;
                return (
                    <div className="max-w-[min(85vw,400px)] sm:max-w-[400px] space-y-1 text-start">
                        {row.action.toLowerCase().includes("update") && (
                            <div className="text-[10px] sm:text-xs text-muted-foreground bg-muted/50 p-1 rounded border border-dashed truncate">
                                تم تحديث قيم الحقول في قاعدة البيانات
                            </div>
                        )}
                        <div className="text-[10px] sm:text-xs font-mono text-muted-foreground opacity-70">IP: {row.ipAddress}</div>
                    </div>
                );
            }
        }
    ], []);

    if (error) {
        return <ErrorState message="خطأ في تحميل سجل أنشطة العقار" />;
    }

    if (isLoading) return <LoadingIndicator message="جاري تحميل سجل الأنشطة..." />;

    return (
        <div className="space-y-4 animate-in fade-in duration-500 text-start">
            <Card className="flex items-center justify-between bg-muted/30 p-4 border border-border/50">
                <div className="space-y-1">
                    <Heading level="h3" className="flex items-center gap-2">
                        <History className="w-5 h-5 text-primary" /> سجل الأنشطة والتدقيق
                    </Heading>
                    <p className="text-xs text-muted-foreground font-medium">عرض العمليات التاريخية التي تمت على هذا الأصل</p>
                </div>
            </Card>

            {logs.length === 0 ? (
                <div className="bg-muted/10 border-2 border-dashed rounded-2xl p-4 sm:p-8 lg:p-16 text-center">
                    <div className="w-12 h-12 bg-muted rounded-full flex items-center justify-center mx-auto mb-4 text-muted-foreground opacity-30">
                        <Monitor className="w-6 h-6" />
                    </div>
                    <h4 className="font-bold text-muted-foreground">لا توجد سجلات تدقيق</h4>
                    <p className="text-sm text-muted-foreground mt-1 opacity-70">لم يتم العثور على حركات مسجلة لهذا الأصل في النظام.</p>
                </div>
            ) : (
                <DataTable
                    columns={columns}
                    data={logs}
                />
            )}
            
            <div className="text-[10px] sm:text-xs text-muted-foreground bg-blue-50/50 p-3 rounded-lg border border-blue-100 italic flex items-center gap-2">
                <div className="w-1.5 h-1.5 rounded-full bg-blue-400" />
                ملاحظة: يتم أرشفة كافة العمليات لضمان أعلى مستويات الرقابة والشفافية في إدارة الاستثمارات.
            </div>
        </div>
    );
}
