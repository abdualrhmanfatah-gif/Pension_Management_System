import { useState } from 'react';
import { toast } from 'sonner';

import { REPORT_CATALOG, type ReportFilter } from '@/api/reports';

export function useReportExport() {
    const [isExporting, setIsExporting] = useState(false);

    const exportReport = async (reportId: string, filters: ReportFilter, format: "excel" | "pdf" = "excel") => {
        setIsExporting(true);
        try {
            const report = REPORT_CATALOG.find(r => r.id === reportId);
            if (!report) throw new Error('Report not found');
            await report.exportData(filters, format);
        } catch (error) {
            console.error('Failed to export report:', error);
            toast.error('حدث خطأ أثناء تصدير التقرير');
        } finally {
            setIsExporting(false);
        }
    };

    return { exportReport, isExporting };
}
