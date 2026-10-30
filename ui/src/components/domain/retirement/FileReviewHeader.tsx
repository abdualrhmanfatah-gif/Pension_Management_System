<<<<<<< HEAD
import { ArrowRight, CheckCircle2, Eye,Printer, XCircle } from "lucide-react";
import { useNavigate } from "react-router-dom";

import { PermissionGate } from "@/components/auth/PermissionGate";
import { Button, Text } from "@/components/ui";
import { Badge } from "@/components/ui";
import { retirementFileStatusName } from "@/constants/retirement";
=======
import { useNavigate } from "react-router-dom";
import { ArrowRight, CheckCircle2, XCircle, Printer, Eye } from "lucide-react";

import { Button, Text } from "@/components/ui";
import { Badge } from "@/components/ui";
import { RecordStateBadge } from "@/components/ui";
import { PermissionGate } from "@/components/auth/PermissionGate";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface FileReviewHeaderProps {
    fileId: number;
    fileNumber: string;
    retireeName: string;
    statusID: number | null;
    statusName: string | null;
    pendingReviewCount: number;
    missingDocsCount: number;
    onToggleReviewPanel: () => void;
    onApprove: () => void;
    isApproving: boolean;
    onReject: () => void;
    isRejecting: boolean;
}

export function FileReviewHeader({
    fileId,
    fileNumber,
    retireeName,
    statusID,
    pendingReviewCount,
    onToggleReviewPanel,
    onApprove,
    isApproving,
    onReject,
    isRejecting,
}: FileReviewHeaderProps) {
    const navigate = useNavigate();

    return (
        <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
                <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => navigate("/files")}
                    aria-label="العودة لقائمة الملفات"
                >
                    <ArrowRight className="w-4 h-4" />
                </Button>
                <div className="w-px h-8 bg-border" />
                <div>
                    <div className="flex items-center gap-2">
                        <span className="text-lg font-bold">ملف رقم {fileNumber}</span>
<<<<<<< HEAD
                        {statusID && <Badge variant="outline">{retirementFileStatusName(statusID)}</Badge>}
=======
                        {statusID && <RecordStateBadge state={statusID as any} />}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    </div>
                    <Text variant="description">{retireeName}</Text>
                </div>
            </div>

            <div className="flex items-center gap-2">
                <Button variant="outline" size="sm" onClick={onToggleReviewPanel}>
                    <Eye className="w-4 h-4 ms-1" />
                    مراجعة التعديلات
                    {pendingReviewCount > 0 && (
                        <Badge variant="danger" className="me-2 text-[10px] px-1.5 py-0.5">
                            {pendingReviewCount}
                        </Badge>
                    )}
                </Button>

                <PermissionGate permission="File.Review.Approve">
                    <Button
                        variant="success"
                        size="sm"
                        onClick={onApprove}
                        loading={isApproving}
                    >
                        <CheckCircle2 className="w-4 h-4 ms-1" />
                        اعتماد
                    </Button>
                </PermissionGate>

                <PermissionGate permission="File.Review.Reject">
                    <Button
                        variant="destructive"
                        size="sm"
                        onClick={onReject}
                        loading={isRejecting}
                    >
                        <XCircle className="w-4 h-4 ms-1" />
                        رفض
                    </Button>
                </PermissionGate>

                <Button
                    variant="outline"
                    size="sm"
                    onClick={() => navigate(`/files/${fileId}/print`)}
                >
                    <Printer className="w-4 h-4 ms-1" />
                    طباعة
                </Button>
            </div>
        </div>
    );
}
