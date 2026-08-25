import { useQuery } from '@tanstack/react-query';
import { ArrowRight, ChevronUp, Download, FilePieChart, FileText, Monitor, Printer, RefreshCw, Users } from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import { retirementApi } from '@/api/retirement';
import { PrintSignatureBlock } from '@/components/common/PrintSignatureBlock';
import { RetirementFileReportViewer } from '@/components/common/RetirementFileReportViewer';
import { ReportChartSection, ReportSummaryCards } from '@/components/common/charts';
import { ChartToggle } from '@/components/common/charts';
import { type CustomColumnDef, DataTable, Badge, Heading } from '@/components/ui';
import { Breadcrumb } from '@/components/ui/Breadcrumb';
import { Button } from '@/components/ui/Button';
import { COLUMN_TYPE } from '@/constants';
import { cn } from '@/lib/utils';
import type { EntitledPersonSummary } from '@/types/retirement';

import { REPORT_CATALOG, type ReportDataRow, type ReportFilter } from '../../api/reports';
import { PrintLayout } from '@/components/ui/print/PrintLayout';
import { ReportFilters } from '@/components/common/ReportFilters';
import { useReportExport } from '@/hooks/useReportExport';

/* ═══════════════════════════════════════════════════════════════
   Helpers & Constants
   ═══════════════════════════════════════════════════════════════ */
const formatValue = (val: string | number | boolean | null | undefined, type?: string) => {
    if (val === null || val === undefined) return '—';
    if (type === COLUMN_TYPE.CURRENCY && typeof val === 'number') return `${val.toLocaleString('ar-YE')} ر.ي`;
    if (type === COLUMN_TYPE.PERCENT && typeof val === 'number') return `${Number(val).toFixed(1)}%`;
    if (type === COLUMN_TYPE.DATE && (typeof val === 'string' || typeof val === 'number')) return new Date(val).toLocaleDateString('ar-YE');
    if (type === COLUMN_TYPE.NUMBER && typeof val === 'number') return val.toLocaleString('ar-YE');
    if (typeof val === 'boolean') return val ? 'نعم' : 'لا';
    return String(val);
};

const SIGNATURES = [
    { title: 'أعده', sub: 'الاســــم والتوقيـــع' },
    { title: 'راجعه', sub: 'الاســــم والتوقيـــع' },
    { title: 'اعتمد', sub: 'الاســــم والتوقيـــع' },
];

const DEFAULT_PAGE_SIZE = 50;

export function ReportViewerPage() {
    const { reportId } = useParams<{ reportId: string }>();
    const navigate = useNavigate();
    const { exportReport, isExporting } = useReportExport();
    const [filters, setFilters] = useState<ReportFilter>({});
    const [isPrintView, setIsPrintView] = useState(false);
    const [showChart, setShowChart] = useState(true);
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
    const [expandedFileId, setExpandedFileId] = useState<number | null>(null);
    const [exportFormat, setExportFormat] = useState<"excel" | "pdf">("excel");

    /* ── جلب المستحقين عند توسيع صف ── */
    const { data: entitledPersons = [], isLoading: isLoadingEntitleds } = useQuery({
        queryKey: ['entitled-persons', expandedFileId],
        queryFn: () => retirementApi.getEntitledPersons(expandedFileId!),
        enabled: !!expandedFileId
    });

    /* ── استخراج إعداد التقرير ── */
    const reportConfig = useMemo(() => REPORT_CATALOG.find(r => r.id === reportId), [reportId]);

    /* ── إعادة التوجيه إذا لم يُعثر على التقرير ── */
    useEffect(() => {
        if (!reportConfig) {
            navigate('/reports');
        }
    }, [reportConfig, navigate]);

    /* ── إعادة تعيين الصفحة عند تغيير الفلاتر ── */
    useEffect(() => {
        setPage(1);
    }, [filters]);

    /* ── أعمدة الجدول ── */
    const tableColumns = useMemo(() => {
        if (!reportConfig) return [];
        const cols: CustomColumnDef<ReportDataRow>[] = [
            {
                key: 'index',
                header: '#',
                className: 'w-16 text-center text-muted-foreground font-mono',
                render: (_, idx) => (page - 1) * pageSize + idx + 1
            },
            ...reportConfig.columns.map(c => ({
                key: c.key,
                header: c.label,
                render: (row: ReportDataRow) => {
                    const cellValue = row[c.key as keyof ReportDataRow];

                    if (c.key === 'missingDocsList') {
                        const docs = String(cellValue ?? '');
                        const items = docs.split('، ').filter(Boolean);
                        return (
                            <div className="flex flex-wrap gap-1 max-w-full sm:max-w-[320px]">
                                {items.map((d, i) => (
                                    <Badge key={i} variant="warning" className="rounded-md px-2 py-0.5 text-xs">
                                        {d}
                                    </Badge>
                                ))}
                                {items.length === 0 && <span className="text-muted-foreground">—</span>}
                            </div>
                        );
                    }
                    if (c.key === 'maxDaysMissing') {
                        const val = Number(cellValue) || 0;
                        const variant = val >= 60 ? 'destructive' : val >= 30 ? 'warning' : 'ghost';
                        return (
                            <Badge variant={variant as any} className="rounded-md px-2 py-0.5 text-xs">
                                {val}
                            </Badge>
                        );
                    }
                    if (c.key === 'missingCount') {
                        const val = Number(cellValue) || 0;
                        return (
                            <Badge variant="outline" className="min-w-[28px] justify-center text-sm font-bold">
                                {val}
                            </Badge>
                        );
                    }
                    return (
                        <span className={cn(c.type === COLUMN_TYPE.CURRENCY && 'font-mono font-bold text-primary')}>
                            {formatValue(cellValue, c.type)}
                        </span>
                    );
                }
            }))
        ];
        return cols;
    }, [reportConfig, page, pageSize]);

    /* ── جلب البيانات ── */
    const { data: pagedResult, isLoading, refetch } = useQuery({
        queryKey: ['report-data', reportId, filters, page, pageSize],
        queryFn: async () => {
            if (!reportConfig) return null;
            const result = await reportConfig.fetchData({ ...filters, pageNumber: page, pageSize });
            return result?.data ?? null;
        },
        enabled: !!reportConfig
    });

    /* ── جلب ملخص الإحصائيات ── */
    const { data: summaryData } = useQuery({
        queryKey: ['report-summary', reportId, filters],
        queryFn: async () => {
            if (!reportConfig?.fetchSummary) return null;
            return await reportConfig.fetchSummary(filters);
        },
        enabled: !!reportConfig?.fetchSummary
    });

    const data = useMemo(() => pagedResult?.items ?? [], [pagedResult]);
    const totalCount = pagedResult?.totalCount ?? 0;

    /* ── معالجات الأحداث ── */
    const handleFilterChange = useCallback((key: string, value: string | number | boolean | null | undefined) => {
        setFilters((prev) => ({ ...prev, [key]: value }));
    }, []);

    const handleExport = useCallback((format: "excel" | "pdf" = "excel") => {
        if (reportConfig) exportReport(reportConfig.id, filters, format);
    }, [exportReport, reportConfig, filters]);

    const handlePrint = useCallback(() => window.print(), []);
    const handleTogglePrintView = useCallback(() => setIsPrintView(p => !p), []);
    const handleBack = useCallback(() => navigate('/reports'), [navigate]);

    const handleRowClick = useCallback((row: ReportDataRow) => {
        const fileId = (row.fileID || row.fileId || row.FileID || row.FileId || row.fileid) as number | undefined;
        if (fileId) {
            // For beneficiaries report, toggle expanded row
            if (reportId === 'dep-01') {
                setExpandedFileId(prev => prev === fileId ? null : fileId);
            } else {
                navigate(`/files/${fileId}`);
            }
        }
    }, [navigate, reportId]);

    const handlePageChange = useCallback((newPage: number) => {
        setPage(newPage);
    }, []);

    const handlePageSizeChange = useCallback((newSize: number) => {
        setPageSize(newSize);
        setPage(1);
    }, []);

    /* ── رقم التقرير ── */
    const reportNumber = useMemo(() =>
        reportConfig ? `REP-${reportConfig.id}-${new Date().getFullYear()}` : '',
    [reportConfig]);

    const printLayoutConfig = useMemo(() => ({
        sectionTitle:
            reportConfig?.category === 'التقاعد' || reportConfig?.category === 'المستحقون' ? 'إدارة المعاشات' :
            reportConfig?.category === 'الموارد البشرية' ? 'إدارة الشؤون الإدارية' :
            reportConfig ? `إدارة ${reportConfig.category}` : '',
        orientation: 'landscape' as const,
    }), [reportConfig]);

    /* ── عناصر ثابتة ── */
    const breadcrumbItems = useMemo(() => [
        { label: 'مركز التقارير', href: '/reports' },
        { label: reportConfig?.name ?? '' }
    ], [reportConfig]);

    const emptyIcon = useMemo(() => (
        <FileText className="w-12 h-12 opacity-20 text-muted-foreground" />
    ), []);

    /* ── حماية إضافية قبل الرسم ── */
    if (!reportConfig) return null;

    // Check if this is the special retirement file report
    if (reportId === 'ret-00') {
        return (
            <div className="space-y-6 animate-fade-in text-start">
                <div className="space-y-4 print:hidden">
                    <Breadcrumb items={breadcrumbItems} />
                    <div className="flex items-center gap-4">
                        <Button variant="outline" size="sm" onClick={handleBack} className="bg-background">
                            <ArrowRight className="w-5 h-5" />
                        </Button>
                        <div className="w-11 h-11 rounded-xl bg-primary/10 flex items-center justify-center">
                            <FilePieChart className="w-5 h-5 text-primary" />
                        </div>
                        <div>
                            <Heading level="h1" className="text-lg md:text-xl lg:text-2xl font-bold text-foreground">{reportConfig.name}</Heading>
                            <p className="text-sm text-muted-foreground mt-0.5">{reportConfig.description}</p>
                        </div>
                    </div>
                </div>
                <RetirementFileReportViewer />
            </div>
        );
    }

    return (
        <div className="space-y-6 animate-fade-in text-start">
            {/* ═══════════════════ Header (شاشة فقط) ═══════════════════ */}
            <div className="space-y-4 print:hidden">
                <Breadcrumb items={breadcrumbItems} />
                
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                    <div className="flex items-center gap-4">
                        <Button variant="outline" size="sm" onClick={handleBack} className="bg-background">
                            <ArrowRight className="w-5 h-5" />
                        </Button>
                        <div className="w-11 h-11 rounded-xl bg-primary/10 flex items-center justify-center">
                            <FilePieChart className="w-5 h-5 text-primary" />
                        </div>
                        <div>
                            <Heading level="h1" className="text-lg md:text-xl lg:text-2xl font-bold text-foreground">{reportConfig.name}</Heading>
                            <p className="text-sm text-muted-foreground mt-0.5">{reportConfig.description}</p>
                        </div>
                    </div>

                    <div className="flex items-center gap-2">
                        <Button
                            variant="secondary"
                            size="sm"
                            onClick={handleTogglePrintView}
                            className="h-9 hidden md:flex"
                        >
                            {isPrintView ? (
                                <>
                                    <Monitor className="w-4 h-4 ms-2" />
                                    عرض الشاشة
                                </>
                            ) : (
                                <>
                                    <FileText className="w-4 h-4 ms-2" />
                                    معاينة الطباعة
                                </>
                            )}
                        </Button>
                        <Button variant="outline" size="sm" onClick={handlePrint} className="h-11">
                            <Printer className="w-4 h-4 ms-2" />
                            طباعة
                        </Button>
                        <Button
                            variant={exportFormat === "excel" ? "success" : "outline"}
                            size="sm"
                            onClick={() => { setExportFormat("excel"); handleExport("excel"); }}
                            disabled={isExporting}
                            className="h-9"
                        >
                            {isExporting && exportFormat === "excel" ? <RefreshCw className="w-4 h-4 ms-2 animate-spin" /> : <Download className="w-4 h-4 ms-2" />}
                            تصدير Excel
                        </Button>
                        <Button
                            variant={exportFormat === "pdf" ? "success" : "outline"}
                            size="sm"
                            onClick={() => { setExportFormat("pdf"); handleExport("pdf"); }}
                            disabled={isExporting}
                            className="h-9"
                        >
                            {isExporting && exportFormat === "pdf" ? <RefreshCw className="w-4 h-4 ms-2 animate-spin" /> : <Download className="w-4 h-4 ms-2" />}
                            تصدير PDF
                        </Button>
                    </div>
                </div>
            </div>

            {/* ═══════════════════ المحتوى الخاص بالشاشة ═══════════════════ */}
            {!isPrintView && (
                <div className="space-y-6">
                    <div className="print:hidden">
                        <ReportFilters
                            filters={reportConfig.filters}
                            values={filters}
                            onChange={handleFilterChange}
                            onApply={() => refetch()}
                        />
                    </div>

                    {/* ── ملخص الإحصائيات ── */}
                    {reportConfig.summaryCards && reportConfig.summaryCards.length > 0 && (
                        <div className="print:hidden">
                            <ReportSummaryCards
                                cards={reportConfig.summaryCards}
                                data={data}
                                summaryData={summaryData ?? undefined}
                            />
                        </div>
                    )}

                    {/* ── الرسوم البيانية ── */}
                    {reportConfig.charts && reportConfig.charts.length > 0 && (
                        <div className="print:hidden">
                            <div className="flex items-center justify-between mb-4">
                                <Heading level="h3" className="text-lg font-bold">الرسوم البيانية</Heading>
                                <ChartToggle showChart={showChart} onToggle={() => setShowChart(!showChart)} />
                            </div>
                            {showChart && (
                                <ReportChartSection
                                    charts={reportConfig.charts}
                                    data={data}
                                />
                            )}
                        </div>
                    )}

                    <div className="print:hidden">
                        <DataTable
                            columns={tableColumns}
                            data={data}
                            loading={isLoading}
                            emptyMessage="لا توجد بيانات تطابق المعايير المختارة"
                            emptyIcon={emptyIcon}
                            onRowClick={handleRowClick}
                            pagination={{
                                page,
                                pageSize,
                                total: totalCount,
                                onPageChange: handlePageChange,
                                pageSizeOptions: [25, 50, 100],
                                onPageSizeChange: handlePageSizeChange,
                            }}
                        />
                    </div>

                    {/* ── القائمة المنسدلة للمستحقين (تقرير dep-01 فقط) ── */}
                    {reportId === 'dep-01' && expandedFileId && (
                        <div className="print:hidden border border-border rounded-xl bg-card overflow-hidden animate-in slide-in-from-top-2">
                            <div className="flex items-center justify-between px-4 py-3 bg-primary/5 border-b border-border">
                                <div className="flex items-center gap-2">
                                    <Users className="w-4 h-4 text-primary" />
                                    <span className="font-bold text-sm">المستحقون التابعون للملف</span>
                                    {isLoadingEntitleds && (
                                        <span className="text-xs text-muted-foreground">(جاري التحميل...)</span>
                                    )}
                                </div>
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    onClick={() => setExpandedFileId(null)}
                                    className="h-7 px-2"
                                >
                                    <ChevronUp className="w-4 h-4" />
                                </Button>
                            </div>
                            <div className="p-4">
                                {!isLoadingEntitleds && entitledPersons.length === 0 && (
                                    <p className="text-sm text-muted-foreground text-center py-4">لا يوجد مستحقون مسجلون لهذا الملف</p>
                                )}
                                {!isLoadingEntitleds && entitledPersons.length > 0 && (
                                    <div className="space-y-2">
                                        {entitledPersons.map((person: EntitledPersonSummary) => (
                                            <div
                                                key={person.entitledID}
                                                className="flex items-center justify-between p-3 rounded-lg bg-muted/30 border border-border/50 hover:bg-muted/50 transition-colors"
                                            >
                                                <div className="flex items-center gap-3">
                                                    <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center">
                                                        <Users className="w-4 h-4 text-primary" />
                                                    </div>
                                                    <div>
                                                        <p className="font-bold text-sm">{person.fullName}</p>
                                                        <p className="text-xs text-muted-foreground">
                                                            {person.relationshipName || '—'} • {person.nationalID || '—'}
                                                        </p>
                                                    </div>
                                                </div>
                                                <div className="flex items-center gap-2">
                                                    {person.phone && (
                                                        <Badge variant="outline" className="text-xs">{person.phone}</Badge>
                                                    )}
                                                    <Badge variant={person.isActive ? 'success' : 'destructive'} className="text-xs">
                                                        {person.isActive ? 'نشط' : 'غير نشط'}
                                                    </Badge>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </div>
                        </div>
                    )}
                </div>
            )}

            {/* ═══════════════════ منطقة الطباعة الرسمية ═══════════════════ */}
            <PrintLayout
                printOnly={!isPrintView}
                reportName={reportConfig.description}
                reportNumber={reportNumber}
                attachments=""
                config={printLayoutConfig}
            >
                {/* شريط معلومات التقرير */}
                <div className="mb-4 flex justify-between text-sm text-black border-b border-black pb-2 font-bold">
                    <span>التصنيف: {reportConfig.category}</span>
                    <span>عدد السجلات: {totalCount.toLocaleString('ar-YE')}</span>
                    {filters.fromDate && filters.toDate && (
                        <span>
                            الفترة: {new Date(filters.fromDate as string).toLocaleDateString('ar-YE')} - {new Date(filters.toDate as string).toLocaleDateString('ar-YE')}
                        </span>
                    )}
                </div>

                {/* الجدول الرسمي */}
                <div className="border border-black overflow-x-auto">
                    <table className="w-full border-collapse text-[12px] text-black" dir="rtl">
                        <colgroup>
                            <col style={{ width: '5%' }} />
                            {reportConfig.columns.map((col, idx) => {
                                const w =
                                    col.key === 'missingDocsList' ? '30%' :
                                    col.key === 'fileNumber' ? '12%' :
                                    col.key === 'retireeName' ? '18%' :
                                    col.key === 'phone' ? '12%' : '11.5%';
                                return <col key={idx} style={{ width: w }} />;
                            })}
                        </colgroup>
                        <thead>
                            <tr className="bg-slate-200 border-b-2 border-black">
                                <th className="px-2 py-2 border-e border-black text-center font-extrabold">م</th>
                                {reportConfig.columns.map((col, idx) => (
                                    <th key={idx} className="px-2 py-2 border-e border-black text-center font-extrabold last:border-e-0">
                                        {col.label}
                                    </th>
                                ))}
                            </tr>
                        </thead>
                        <tbody>
                            {data.length === 0 ? (
                                <tr>
                                    <td colSpan={reportConfig.columns.length + 1} className="p-8 text-center text-black border-t border-black">
                                        لا توجد بيانات تطابق المعايير المختارة
                                    </td>
                                </tr>
                            ) : (
                                data.map((row, rowIdx) => (
                                    <tr key={rowIdx} className="border-b border-black last:border-b-0">
                                        <td className="px-2 py-2 border-e border-black text-center font-bold">{(page - 1) * pageSize + rowIdx + 1}</td>
                                        {reportConfig.columns.map((col, colIdx) => {
                                            const cellValue = row[col.key as keyof ReportDataRow];

                                            if (col.key === 'missingDocsList') {
                                                const docs = String(cellValue ?? '');
                                                const items = docs.split('، ').filter(Boolean);
                                                return (
                                                    <td key={colIdx} className="px-2 py-2 border-e border-black text-center last:border-e-0 text-[11px] break-words">
                                                        {items.length > 0 ? (
                                                            <ul className="list-disc list-inside text-start">
                                                                {items.map((d, di) => <li key={di} className="leading-tight">{d}</li>)}
                                                            </ul>
                                                        ) : '—'}
                                                    </td>
                                                );
                                            }
                                            if (col.key === 'maxDaysMissing') {
                                                const val = Number(cellValue) || 0;
                                                const colorClass = val >= 60 ? 'text-destructive font-bold' :
                                                              val >= 30 ? 'text-[hsl(var(--gov-warning))] font-bold' : '';
                                                return (
                                                    <td
                                                        key={colIdx}
                                                        className={cn("px-2 py-2 border-e border-black text-center last:border-e-0", colorClass)}
                                                    >
                                                        {val}
                                                    </td>
                                                );
                                            }
                                            if (col.key === 'missingCount') {
                                                const val = Number(cellValue) || 0;
                                                return (
                                                    <td key={colIdx} className="px-2 py-2 border-e border-black text-center last:border-e-0 font-bold">
                                                        <Badge variant="outline" className="min-w-[24px] justify-center">
                                                            {val}
                                                        </Badge>
                                                    </td>
                                                );
                                            }
                                            return (
                                                <td
                                                    key={colIdx}
                                                    className={cn(
                                                        'px-2 py-2 border-e border-black text-center last:border-e-0',
                                                        col.type === COLUMN_TYPE.CURRENCY && 'font-bold font-mono'
                                                    )}
                                                >
                                                    {formatValue(cellValue, col.type)}
                                                </td>
                                            );
                                        })}
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>

                {/* التوقيعات */}
                <PrintSignatureBlock signatures={SIGNATURES} className="mt-14" />
            </PrintLayout>
        </div>
    );
};
