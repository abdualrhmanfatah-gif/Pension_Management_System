import { Building2, User, UserCircle } from "lucide-react";

import { type CustomColumnDef } from "@/components/ui";
import { Badge } from "@/components/ui";
import { formatDate } from "@/lib/utils";
import { type CorrespondenceListItemDto } from "@/types/correspondence";

export const correspondenceColumns: CustomColumnDef<CorrespondenceListItemDto>[] = [
  {
    key: "sequentialNumber",
    header: "م",
    render: (row) => <span className="font-mono text-xs font-bold text-primary">{row.sequentialNumber}</span>,
  },
  {
    key: "correspondenceDate",
    header: "التاريخ",
    render: (row) => <span className="font-mono text-xs text-muted-foreground">{formatDate(row.correspondenceDate)}</span>,
  },
  {
    key: "referenceNumber",
    header: "المرجع",
    render: (row) => <Badge variant="outline" className="font-mono text-[10px] sm:text-xs">{row.referenceNumber}</Badge>,
  },
  {
    key: "subject",
    header: "الموضوع",
    render: (row) => <span className="font-bold text-sm">{row.subject}</span>,
  },
  {
    key: "party",
    header: "الجهة / الموظف",
    render: (row) => (
      <div className="flex flex-col gap-1">
        {row.externalPartyName && (
          <div className="flex items-center gap-1.5 text-xs text-blue-600 font-medium">
            <Building2 className="h-3 w-3" />
            <span>{row.externalPartyName}</span>
          </div>
        )}
        {row.employeeName && (
          <div className="flex items-center gap-1.5 text-xs text-emerald-600 font-medium">
            <UserCircle className="h-3 w-3" />
            <span>{row.employeeName}</span>
          </div>
        )}
        {!row.externalPartyName && !row.employeeName && (
          <span className="text-muted-foreground text-[10px] sm:text-xs italic">غير محدد</span>
        )}
      </div>
    ),
  },
  {
    key: "typeName",
    header: "النوع",
    accessor: (row) => row.typeName,
  },
  {
    key: "createdByUserName",
    header: "بواسطة",
    render: (row) => (
      <div className="flex items-center gap-1.5 text-[10px] sm:text-xs text-muted-foreground">
        <User className="h-3 w-3" />
        <span>{row.createdByUserName}</span>
      </div>
    ),
  },
];
