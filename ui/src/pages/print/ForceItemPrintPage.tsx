import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Printer } from "lucide-react";
import { useNavigate,useParams } from "react-router-dom";

import api from "@/api/axios";
import { ministryApi } from "@/api/ministry";
<<<<<<< HEAD
import { UnifiedRetireeStatement } from "@/components/common/UnifiedRetireeStatement";
import type { ApprovalItem,Statement } from "@/components/domain/print/print.types";
import { fetchFileDetails } from "@/components/domain/print/print.utils";
import { Button } from "@/components/ui/Button";
import { PrintLayout } from "@/components/ui/print/PrintLayout";
=======
import { Button } from "@/components/ui/Button";
import { PrintLayout } from "@/components/ui/print/PrintLayout";

import { UnifiedRetireeStatement } from "@/components/common/UnifiedRetireeStatement";
import type { ApprovalItem,Statement } from "@/components/domain/print/print.types";
import { fetchFileDetails } from "@/components/domain/print/print.utils";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

/**
 * مباينة بيانات المتقاعد من كشف الإحالة
 */
export function ForceItemPrintPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  // 1. Fetch the Force Item (Approval Item)
  const { data: item, isLoading: loadingItem, isError: errorItem } = useQuery({
    queryKey: ["force-item", id],
    queryFn: async () => {
      const res = await ministryApi.getItemById(Number(id));
      return res.data as ApprovalItem & { approvalFullName?: string, fullName?: string, retireeID?: number };
    },
    enabled: !!id,
  });

  const retireeID = item?.retireeID;

  // 2. Fetch the Retiree Statement if processed
  const { data: statement, isLoading: loadingStatement, isError: errorStatement } = useQuery({
    queryKey: ["retirees", retireeID, "statement"],
    queryFn: async () => {
      if (!retireeID) return null;
      const res = await api.get<{ data: Statement }>(`/retirees/${retireeID}/statement`);
      return res.data.data;
    },
    enabled: !!retireeID
  });

  const fileId = statement?.retirementFiles?.[0]?.fileID;

  // 3. Fetch File Details for additional info
  const { data: fileDetail, isLoading: loadingFileDetail } = useQuery({
    queryKey: ["retirement-files", fileId, "details-for-statement"],
    queryFn: () => fetchFileDetails(fileId!),
    enabled: !!fileId && !!statement && statement.retirementFiles.length > 0
  });

  if (loadingItem || loadingStatement) return <div className="flex justify-center py-20 text-muted-foreground italic text-sm">جاري التحميل...</div>;
  if (errorItem || errorStatement) return <div className="flex justify-center py-20 text-red-600 font-bold">حدث خطأ أثناء تحميل بيانات القيد.</div>;
  if (!item) return <div className="flex justify-center py-20 text-muted-foreground font-bold">البيانات غير موجودة.</div>;

  // Construct a statement object if it doesn't exist (not processed yet)
  const displayStatement: Statement = statement || {
    retiree: {
      retireeID: 0,
      fullName: item.approvalFullName || item.fullName || "—",
      militaryNumber: item.militaryNumber,
      nationalID: "---", // Missing in item, provide placeholder
      gender: 1,
      phone: null,
      address: null,
      city: null,
      governorate: null,
      isAlive: true,
      deathDate: null,
      socialStatusName: null,
      healthStatusName: null,
      createdAt: "",
    },
    approvalItems: [item as unknown as ApprovalItem],
    retirementFiles: [],
    settlements: [],
    entitlements: [],
  };

  return (
    <>
      <div className="print:hidden flex items-center justify-between mb-6 p-6 max-w-7xl mx-auto">
        <Button variant="ghost" onClick={() => navigate(-1)}>
          <ArrowRight className="w-5 h-5 ms-2" />
          رجوع
        </Button>
        <div className="flex gap-2">
          <Button onClick={() => window.print()} className="flex items-center">
            <Printer className="w-4 h-4 ms-2" />
            طباعة المباينة
          </Button>
        </div>
      </div>

      <div className="w-full flex justify-center">
        <PrintLayout
          reportNumber={`RET-${id}-${new Date().getFullYear()}`}
          attachments={`بيانات القيد رقم ${id}`}
          config={{ sectionTitle: "إدارة المعاشات" }}
        >
          <UnifiedRetireeStatement 
            statement={displayStatement} 
            fileDetail={fileDetail} 
            loadingFileDetail={loadingFileDetail}
            title="مباينة بيانات المتقاعد"
          />
        </PrintLayout>
      </div>
    </>
  );
}
