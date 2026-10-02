import { useQuery } from "@tanstack/react-query";
import { Clock, Eye, FileText, Filter, FolderOpen,RefreshCw, Search } from "lucide-react";
import { Fragment,useState } from "react";
import { Link } from "react-router-dom";

import api from "@/api/axios";
import { PageLayout } from "@/components/layout/PageLayout";
import { StickyHeader } from "@/components/layout/PageLayout";
import { Heading, Text } from "@/components/ui";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { type CustomColumnDef,DataTable } from "@/components/ui/DataTable";
import { InfoPair } from "@/components/ui/InfoPair";
import { DateInput,Input } from "@/components/ui/Input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/Select";
import { translateFieldName } from "@/lib/fieldTranslations";
import { cn } from "@/lib/utils";

interface AuditLogItem {
    logID: number;
    userID: number | null;
    userName: string | null;
    tableID: number | null;
    tableName: string | null;
    action: string;
    recordID: number | null;
    entityKey: number | null;
    entityDisplayName: string | null;
    oldValues: string | null;
    newValues: string | null;
    ipAddress: string | null;
    userAgent: string | null;
    correlationId: string | null;
    createdAt: string;
    affectedFileId: number | null;
    affectedFileNumber: string | null;
}

interface TableOption { tableID: number; tableNameAr: string; }

const actionLabels: Record<string, string> = {
    Create: "إنشاء",
    Update: "تعديل",
    Delete: "حذف",
    ChangeStatus: "تغيير حالة",
    Approve: "اعتماد",
    Override: "تجاوز",
    Login: "تسجيل دخول",
    Logout: "تسجيل خروج",
};

function JsonDataViewer({ oldJson, newJson }: { oldJson?: string | null, newJson?: string | null }) {
    let oldObj: Record<string, unknown> = {};
    let newObj: Record<string, unknown> = {};

    try { if (oldJson) oldObj = JSON.parse(oldJson); } catch { /* ignore */ }
    try { if (newJson) newObj = JSON.parse(newJson); } catch { /* ignore */ }

    const allKeys = Array.from(new Set([...Object.keys(oldObj), ...Object.keys(newObj)]));

    if (allKeys.length === 0) return null;

    const renderValue = (val: unknown) => {
        if (val === null || val === undefined) return <span className="text-muted-foreground italic">Null</span>;
        if (typeof val === 'boolean') return val ? 'نعم (True)' : 'لا (False)';
        if (typeof val === 'object') return JSON.stringify(val);
        return String(val);
    };

    const data = allKeys.map(key => {
        const oldVal = oldObj[key];
        const newVal = newObj[key];
        const hasChanged = typeof oldVal !== 'undefined' && typeof newVal !== 'undefined' && oldVal !== newVal;
        return { key, oldVal, newVal, hasChanged };
    });

    return (
        <Card className="w-full overflow-hidden mt-6">
            <DataTable
                columns={[
                    { key: "key", header: "الحقل (Property)", render: (row) => <div><div className="font-semibold text-sm text-foreground mb-0.5">{translateFieldName(row.key)}</div>{translateFieldName(row.key) !== row.key && <div className="text-xs font-mono text-muted-foreground opacity-60" dir="ltr">{row.key}</div>}</div> },
                    ...(oldJson ? [{ key: "oldVal", header: <span className="text-red-600 dark:text-red-400">القيمة القديمة</span>, render: (row: typeof data[number]) => <td className={cn("break-all font-mono text-xs", row.hasChanged ? "bg-red-50 dark:bg-red-950/20 text-red-700 dark:text-red-300" : "")} dir="auto">{typeof row.oldVal !== 'undefined' ? renderValue(row.oldVal) : <span className="text-muted-foreground opacity-50">—</span>}</td> }] : []),
                    ...(newJson ? [{ key: "newVal", header: <span className="text-emerald-600 dark:text-emerald-400">القيمة الجديدة</span>, render: (row: typeof data[number]) => <td className={cn("break-all font-mono text-xs", row.hasChanged ? "bg-emerald-50 dark:bg-emerald-950/20 text-emerald-700 dark:text-emerald-300 font-bold" : "")} dir="auto">{typeof row.newVal !== 'undefined' ? renderValue(row.newVal) : <span className="text-muted-foreground opacity-50">—</span>}</td> }] : []),
                ] as CustomColumnDef<typeof data[number]>[]}
                data={data}
                rowKey={(row) => row.key}
            />
        </Card>
    );
}

export function AuditLogsPage() {
    const [page, setPage] = useState(1);
    const [filterAction, setFilterAction] = useState("");
    const [filterTable, setFilterTable] = useState("");
    const [filterSearch, setFilterSearch] = useState("");
    const [filterFrom, setFilterFrom] = useState("");
    const [filterTo, setFilterTo] = useState("");
    const [expandedLog, setExpandedLog] = useState<number | null>(null);

    // Fetch filters data
    const { data: filterOptions } = useQuery({
        queryKey: ["audit-logs", "filters"],
        queryFn: async () => {
            const [actRes, tblRes] = await Promise.all([
                api.get<{ data: string[] }>("/admin/audit-logs/actions"),
                api.get<{ data: TableOption[] }>("/admin/audit-logs/tables"),
            ]);
            return {
                actions: actRes.data.data || [],
                tables: tblRes.data.data || []
            };
        }
    });

    const actions = filterOptions?.actions || [];
    const tables = filterOptions?.tables || [];

    // Fetch paginated logs
    const { data: logsData, isLoading: loading, isError: logsError, refetch: fetchLogs } = useQuery({
        queryKey: ["audit-logs", page, filterAction, filterTable, filterSearch, filterFrom, filterTo],
        queryFn: async () => {
            const params = new URLSearchParams();
            params.set("page", String(page));
            params.set("pageSize", "30");
            if (filterAction) params.set("action", filterAction);
            if (filterTable) params.set("tableId", filterTable);
            if (filterSearch) params.set("search", filterSearch);
            if (filterFrom) params.set("from", filterFrom);
            if (filterTo) params.set("to", filterTo);

            const res = await api.get<{ data: { items: AuditLogItem[]; totalCount: number; totalPages: number } }>(`/admin/audit-logs?${params}`);
            return res.data.data;
        }
    });

    const logs = logsData?.items || [];
    const totalCount = logsData?.totalCount || 0;
    const totalPages = logsData?.totalPages || 1;

    const resetFilters = () => {
        setFilterAction("");
        setFilterTable("");
        setFilterSearch("");
        setFilterFrom("");
        setFilterTo("");
        setPage(1);
    };



    return (
        <PageLayout>
            {/* Header */}
            <StickyHeader>
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <div className="p-2.5 rounded-xl bg-gradient-to-br from-violet-500/20 to-purple-500/20">
                            <FileText className="w-6 h-6 text-violet-600 dark:text-violet-400" />
                        </div>
                        <div>
                            <Heading level="h1" className="text-lg md:text-xl lg:text-2xl font-bold">سجلات التدقيق</Heading>
                            <Text variant="description">تتبع جميع العمليات في النظام · {totalCount.toLocaleString("ar-YE")} سجل</Text>
                        </div>
                    </div>
                    <Button variant="outline" size="sm" onClick={() => fetchLogs()}>
                        <RefreshCw className="w-4 h-4 ms-1" /> تحديث
                    </Button>
                </div>
            </StickyHeader>

            {/* Filters */}
            <Card className="p-4 space-y-3">
                <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                    <Filter className="w-4 h-4" /> فلاتر البحث
                </div>
                <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-3">
                    <div className="relative">
                        <Search className="absolute end-3 top-2.5 w-4 h-4 text-muted-foreground" />
                        <Input
                            placeholder="بحث..."
                            value={filterSearch}
                            onChange={e => { setFilterSearch(e.target.value); setPage(1); }}
                            className="pe-9"
                        />
                    </div>
                    <Select
                        value={filterAction}
                        onValueChange={(v) => { setFilterAction(v === "__all__" ? "" : v); setPage(1); }}
                    >
                        <SelectTrigger className="min-h-[44px]">
                            <SelectValue placeholder="كل العمليات" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="__all__">كل العمليات</SelectItem>
                            {actions.map(a => (
                                <SelectItem key={a} value={a}>{actionLabels[a] || a}</SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                    <Select
                        value={filterTable}
                        onValueChange={(v) => { setFilterTable(v === "__all__" ? "" : v); setPage(1); }}
                    >
                        <SelectTrigger className="min-h-[44px]">
                            <SelectValue placeholder="كل الجداول" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="__all__">كل الجداول</SelectItem>
                            {tables.map(t => (
                                <SelectItem key={t.tableID} value={String(t.tableID)}>{t.tableNameAr}</SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                    <DateInput value={filterFrom} onChange={val => { setFilterFrom(val || ""); setPage(1); }} placeholder="من تاريخ" />
                    <DateInput value={filterTo} onChange={val => { setFilterTo(val || ""); setPage(1); }} placeholder="إلى تاريخ" />
                </div>
                {(filterAction || filterTable || filterSearch || filterFrom || filterTo) && (
                    <Button variant="ghost" size="sm" onClick={resetFilters} className="text-xs">مسح الفلاتر</Button>
                )}
            </Card>

            {/* Table */}
            {loading ? (
                <div className="py-16 text-center text-muted-foreground">جاري تحميل سجلات التدقيق...</div>
            ) : logsError ? (
                <div className="border-2 border-destructive/20 rounded-xl py-16 text-center text-destructive">تعذر تحميل سجلات التدقيق</div>
            ) : logs.length === 0 ? (
                <div className="border-2 border-dashed rounded-xl py-16 text-center text-muted-foreground">لا توجد سجلات مطابقة</div>
            ) : (
                <div className="border rounded-xl overflow-hidden">
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm" role="table" aria-label="سجلات التدقيق">
                            <thead className="bg-muted/50 sticky top-0 z-10">
                                <tr>
                                    <th className="text-start px-4 py-3 font-medium">
                                        التاريخ</th>
                                    <th className="text-start px-4 py-3 font-medium">
                                        المستخدم</th>
                                    <th className="text-start px-4 py-3 font-medium">
                                        العملية</th>
                                    <th className="text-start px-4 py-3 font-medium">
                                        الجدول</th>
                                    <th className="text-start px-4 py-3 font-medium">
                                        الكيان</th>
                                    <th className="text-start px-4 py-3 font-medium">
                                        IP</th>
                                    <th className="text-center px-4 py-3 font-medium">تفاصيل</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y">
                                {logs.map(log => (
                                    <Fragment key={log.logID}>
                                        <tr
                                            className="hover:bg-muted/30 transition-colors cursor-pointer"
                                            onClick={() => setExpandedLog(expandedLog === log.logID ? null : log.logID)}
                                        >
                                            <td className="px-4 py-3 whitespace-nowrap">
                                                <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
                                                    <Clock className="w-3.5 h-3.5" />
                                                    {new Date(log.createdAt).toLocaleString("ar-YE")}
                                                </div>
                                            </td>
                                            <td className="px-4 py-3 font-medium">{log.userName || "—"}</td>
                                            <td className="px-4 py-3">
                                                <Badge variant={log.action === "Create" ? "success" : log.action === "Update" ? "info" : log.action === "Delete" ? "destructive" : log.action === "ChangeStatus" ? "warning" : log.action === "Approve" ? "secondary" : "ghost"}>
                                                    {actionLabels[log.action] || log.action}
                                                </Badge>
                                            </td>
                                            <td className="px-4 py-3 text-muted-foreground">{log.tableName || "—"}</td>
                                            <td className="px-4 py-3">{log.entityDisplayName || (log.recordID ? `#${log.recordID}` : "—")}</td>
                                            <td className="px-4 py-3 text-xs text-muted-foreground font-mono">{log.ipAddress || "—"}</td>
                                            <td className="px-4 py-3 text-center" onClick={e => e.stopPropagation()}>
                                                <Button variant="ghost" size="sm" className="min-h-[44px]" onClick={() => setExpandedLog(expandedLog === log.logID ? null : log.logID)} title="عرض التفاصيل">
                                                    <Eye className="w-4 h-4" />
                                                </Button>
                                            </td>
                                        </tr>
                                        {expandedLog === log.logID && (
                                            <tr key={`${log.logID}-details`}>
                                                <td colSpan={7} className="px-6 py-5 bg-muted/20 text-start">
                                                    <div className="space-y-4">
                                                        <p className="text-xs font-semibold text-muted-foreground border-b pb-2">جميع تفاصيل السجل #{log.logID}</p>
                                                        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-x-6 gap-y-3 text-sm">
                                                            <InfoPair label="معرّف السجل" value={log.logID} mono />
                                                            <InfoPair label="التاريخ والوقت" value={new Date(log.createdAt).toLocaleString("ar-YE", { dateStyle: "medium", timeStyle: "medium" })} />
                                                            <InfoPair
                                                                label="المستخدم"
                                                                value={log.userName ? `${log.userName}${log.userID != null ? ` (معرّف: ${log.userID})` : ""}` : "—"}
                                                            />
                                                            <InfoPair
                                                                label="العملية"
                                                                value={actionLabels[log.action] || log.action}
                                                            />
                                                            <InfoPair
                                                                label="الجدول"
                                                                value={log.tableName ? `${log.tableName}${log.tableID != null ? ` (معرّف: ${log.tableID})` : ""}` : "—"}
                                                            />
                                                            <InfoPair
                                                                label="الكيان المتأثر"
                                                                value={
                                                                    log.entityDisplayName
                                                                        ? `${log.entityDisplayName}${log.recordID != null ? ` (معرّف: ${log.recordID})` : ""}`
                                                                        : log.recordID != null
                                                                            ? `معرّف السجل: ${log.recordID}`
                                                                            : "—"
                                                                }
                                                                className="sm:col-span-2"
                                                            />
                                                            <InfoPair label="عنوان IP" value={log.ipAddress ?? "—"} mono />
                                                            <InfoPair label="معرّف الارتباط" value={log.correlationId ?? "—"} mono className="sm:col-span-2" />
                                                        </div>
                                                        {log.affectedFileId != null && (
                                                            <div className="mt-4 p-3 rounded-lg bg-primary/5 border border-primary/20">
                                                                <p className="text-xs font-semibold text-muted-foreground mb-1.5 flex items-center gap-1.5">
                                                                    <FolderOpen className="w-4 h-4" /> الملف المتأثر
                                                                </p>
                                                                <Link
                                                                    to={`/files/${log.affectedFileId}`}
                                                                    className="inline-flex items-center gap-1.5 text-sm font-medium text-primary hover:underline"
                                                                    onClick={e => e.stopPropagation()}
                                                                >
                                                                    ملف التقاعد {log.affectedFileNumber ? `رقم ${log.affectedFileNumber}` : `#${log.affectedFileId}`}
                                                                    <span className="text-xs opacity-80">→</span>
                                                                </Link>
                                                            </div>
                                                        )}
                                                        {log.userAgent && (
                                                            <div>
                                                                <Text variant="caption" className="font-medium mb-1">بيانات المتصفح (User-Agent)</Text>
                                                                <Text variant="body-sm" className="bg-muted/50 p-2 rounded-lg break-all font-mono" dir="ltr">{log.userAgent}</Text>
                                                            </div>
                                                        )}
                                                        {(log.oldValues || log.newValues) && (
                                                            <JsonDataViewer oldJson={log.oldValues} newJson={log.newValues} />
                                                        )}
                                                    </div>
                                                </td>
                                            </tr>
                                        )}
                                    </Fragment>
                                ))}
                            </tbody>
                        </table>
                    </div>

                    {/* Pagination */}
                    {totalPages > 1 && (
                        <div className="flex items-center justify-between px-4 py-3 border-t bg-muted/20">
                            <Text variant="description">صفحة {page} من {totalPages} · إجمالي {totalCount.toLocaleString("ar-YE")} سجل</Text>
                            <div className="flex gap-2">
                                <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>السابق</Button>
                                <Button variant="outline" size="sm" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>التالي</Button>
                            </div>
                        </div>
                    )}
                </div>
            )}
        </PageLayout>
    );
}

