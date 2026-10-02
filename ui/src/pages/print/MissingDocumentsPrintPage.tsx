import { useQuery } from "@tanstack/react-query";
import { Printer } from "lucide-react";
import { useParams } from "react-router-dom";

import { documentsApi } from "@/api/documents";
import { retirementApi } from "@/api/retirement";
import { Button } from "@/components/ui/Button";
import { PrintableReportTable, type TableCell } from "@/components/ui/print/PrintableReportTable";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
import { chartColors } from "@/lib/chartColors";

export function MissingDocumentsPrintPage() {
    const { id } = useParams<{ id: string }>();
    const fileId = parseInt(id || "");

    const { data: fileData, isLoading: loadingFile } = useQuery({
        queryKey: ["retirement-files", fileId],
        queryFn: () => retirementApi.getFile(fileId),
        enabled: !!fileId,
    });

    const { data: uploaded = [], isLoading: loadingUploaded } = useQuery({
        queryKey: ["documents-by-file", fileId],
        queryFn: () => documentsApi.getByFileId(fileId).then(docs => docs.map(d => ({ documentID: d.documentID, documentTypeName: d.documentTypeName }))),
        enabled: !!fileId,
    });

    const { data: missing = [], isLoading: loadingMissing } = useQuery({
        queryKey: ["missing-documents-auto", fileId],
        queryFn: () => documentsApi.getAutoDetectedMissing(fileId) as Promise<any>,
        enabled: !!fileId,
    });

    const isLoading = loadingFile || loadingUploaded || loadingMissing;

    const uploadedNames = new Set(uploaded.map((d) => d.documentTypeName));

    const missingByName = new Map<string, { entityType: string | null; entityName: string | null }>();
    missing.forEach((d: { documentTypeName: string; entityType?: string | null; entityName?: string | null }) => {
        if (!missingByName.has(d.documentTypeName)) {
            missingByName.set(d.documentTypeName, { entityType: d.entityType ?? null, entityName: d.entityName ?? null });
        }
    });

    const allDocNames = [...new Set([...uploadedNames, ...missingByName.keys()])].sort();

    const uploadedCount = uploadedNames.size;
    const missingCount = missingByName.size;
    const total = allDocNames.length;

    const tableHeaders = ["#", "نوع الوثيقة", "صاحب الوثيقة", "الحالة"];

    const tableRows: TableCell[][] = allDocNames.map((name, i) => {
        const isUploaded = uploadedNames.has(name);
        const owner = missingByName.get(name);
        const ownerLabel = owner?.entityName
            ? `${owner.entityName}${owner.entityType ? ` (${owner.entityType})` : ""}`
            : "—";
        return [
            { value: i + 1, align: "center" },
            { value: name, bold: true, align: "start" },
            { value: ownerLabel, align: "start" },
            {
                value: isUploaded ? "✓ مرفوعة" : "✗ ناقصة",
                bold: true,
                color: isUploaded ? chartColors.success : chartColors.danger,
                align: "center",
            },
        ];
    });

    const tableTotals: TableCell[] = [
        { value: "الإجمالي", bold: true, colSpan: 2 },
        { value: `${total} نوع`, bold: true, align: "center" },
        {
            value: `${uploadedCount} مرفوعة / ${missingCount} ناقصة`,
            bold: true,
            align: "center",
            color: missingCount === 0 ? chartColors.success : chartColors.warning,
        },
    ];

    if (isLoading) {
        return <div className="flex items-center justify-center min-h-screen italic text-muted-foreground text-sm">جاري تجهيز التقرير للطباعة...</div>;
    }

    return (
        <>
            <div className="print:hidden flex items-center justify-between mb-6 p-6 max-w-7xl mx-auto">
                <Button variant="ghost" onClick={() => window.history.back()}>
                    رجوع
                </Button>
                <Button onClick={() => window.print()} className="flex items-center">
                    <Printer className="w-4 h-4 ms-2" />
                    طباعة التقرير
                </Button>
            </div>

            <PrintLayout
                reportName="تقرير حالة الوثائق"
                reportNumber={fileData?.fileNumber || ""}
                attachments={fileData?.retireeFullName || ""}
                config={{
                    pageSize: "A4",
                    orientation: "portrait",
                    sectionTitle: "إدارة المعاشات",
                }}
            >
                <PrintableReportTable
                    headers={tableHeaders}
                    rows={tableRows}
                    totals={tableTotals}
                    caption="قائمة الوثائق"
                    captionNumber={1}
                    hoverable
                />

                {missingCount > 0 && (
                    <section className="mt-6 print:page-break-inside-avoid">
                        <h3 className="text-xs font-black mb-2">ملاحظات:</h3>
                        <ul className="text-[11px] space-y-1 list-disc list-inside">
                            <li>الوثائق الناقصة: {[...missingByName.keys()].join("، ")}</li>
                        </ul>
                    </section>
                )}
            </PrintLayout>
        </>
    );
}
