import type { ReactNode } from "react";

import { FileStatusBadge } from "./FileStatusBadge";

interface FileSummary {
    fileNumber: string;
    retireeFullName: string;
    statusID?: number | null;
    fileStatusName: string | null;
    hasDocuments?: boolean;
    missingDocumentsCount?: number;
    lastUploadedByUserName?: string | null;
}

export function RetirementFileCard({ file, children }: { file: FileSummary; children?: ReactNode }) {
    const documentStatus = file.hasDocuments === false
        ? "لم تُرفع"
        : (file.missingDocumentsCount ?? 0) > 0
            ? `${file.missingDocumentsCount} ناقصة`
            : file.hasDocuments === true ? "مكتملة" : undefined;

    return (
        <div className="space-y-2 p-2.5">
            <div className="space-y-1">
                <div className="flex items-start gap-2">
                    <h2 className="min-w-0 flex-1 break-words text-base font-bold leading-relaxed text-foreground">
                        {file.retireeFullName}
                    </h2>
                    <FileStatusBadge statusId={file.statusID} statusName={file.fileStatusName} className="shrink-0" />
                </div>
                <div className="flex min-w-0 items-center gap-2">
                    <p className="shrink-0 whitespace-nowrap text-xs text-muted-foreground">
                        رقم الملف: <bdi dir="ltr" className="ms-1 font-mono font-semibold text-foreground">{file.fileNumber}</bdi>
                    </p>
                    {file.hasDocuments && file.lastUploadedByUserName && (
                        <p className="min-w-0 flex-1 truncate text-xs text-muted-foreground" title={`المدخل: ${file.lastUploadedByUserName}`}>
                            المدخل: {file.lastUploadedByUserName}
                        </p>
                    )}
                </div>
            </div>
            {documentStatus && <span className="sr-only">حالة الوثائق: {documentStatus}</span>}
            {children}
        </div>
    );
}
