import { useQuery } from "@tanstack/react-query";
import { Briefcase, Eye, Trash2, UserCheck } from "lucide-react";
import { useRef, useState } from "react";
import { useNavigate } from "react-router-dom";

import { retirementApi } from "@/api/retirement";
import type { AgentSearchResult } from "@/types/retirement";
import {
    Badge,
    CrudPageLayout,
    StatCard,
    Button,
} from "@/components/ui";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { getPhotoUrl, resolvePersonalPhotoPath } from "@/lib/utils";
import { ViewAgentDialog } from "@/components/domain/retirement/ViewAgentDialog";

export function AgentsPage() {
    const navigate = useNavigate();
    const { searchTerm, setSearchTerm, debouncedValue } = useDebouncedSearch();
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [viewAgent, setViewAgent] = useState<any>(null);
    const [viewFileId, setViewFileId] = useState<number>(0);

    const { data: agents = [], isLoading } = useQuery({
        queryKey: ["agents-search", debouncedValue],
        queryFn: () => retirementApi.searchAgents(debouncedValue, 50),
        enabled: debouncedValue.length > 1,
    });

    const activeCount = agents.filter((a) => !a.agencyEndDate).length;

    const columns = [
        {
            key: "fullName",
            header: "الاسم",
            render: (row: AgentSearchResult) => (
                <div className="flex items-center gap-3">
                    <div className="w-9 h-9 rounded-full bg-primary/10 flex items-center justify-center text-primary overflow-hidden">
                        {getPhotoUrl(resolvePersonalPhotoPath(row)) ? (
                            <img src={getPhotoUrl(resolvePersonalPhotoPath(row))!} alt="" className="w-full h-full object-cover" loading="lazy" />
                        ) : (
                            <Briefcase className="w-4 h-4" />
                        )}
                    </div>
                    <span className="font-bold text-foreground">{row.fullName}</span>
                </div>
            ),
        },
        {
            key: "nationalID",
            header: "الرقم الوطني",
            render: (row: AgentSearchResult) => <span className="text-sm font-mono">{row.nationalID || "—"}</span>,
        },
        {
            key: "agencyNumber",
            header: "رقم الوكالة",
            render: (row: AgentSearchResult) => <span className="text-sm">{row.agencyNumber || "—"}</span>,
        },
        {
            key: "agencyDate",
            header: "تاريخ الوكالة",
            render: (row: AgentSearchResult) => (
                <span className="text-sm">
                    {row.agencyDate ? new Date(row.agencyDate).toLocaleDateString("ar-SA") : "—"}
                </span>
            ),
        },
        {
            key: "fileNumber",
            header: "رقم الملف",
            render: (row: AgentSearchResult) => (
                <Button
                    variant="link"
                    size="sm"
                    className="h-auto p-0 text-sm font-bold"
                    onClick={(e) => {
                        e.stopPropagation();
                        navigate(`/files/${row.fileID}`);
                    }}
                >
                    {row.fileNumber || "—"}
                </Button>
            ),
        },
        {
            key: "retireeMilitaryNumber",
            header: "الرقم العسكري",
            render: (row: AgentSearchResult) => <span className="text-sm font-mono">{row.retireeMilitaryNumber || "—"}</span>,
        },
        {
            key: "isActive",
            header: "الحالة",
            render: (row: AgentSearchResult) => (
                <Badge variant={!row.agencyEndDate ? "success" : "destructive"}>
                    {!row.agencyEndDate ? "نشط" : "غير نشط"}
                </Badge>
            ),
        },
        {
            key: "actions",
            header: "",
            render: (row: AgentSearchResult) => (
                <div className="flex gap-1">
                    <Button
                        variant="ghost-action"
                        size="sm"
                        title="عرض التفاصيل"
                        onClick={(e) => {
                            e.stopPropagation();
                            setViewAgent(row);
                            setViewFileId(row.fileID);
                        }}
                    >
                        <Eye className="w-4 h-4" />
                    </Button>
                    <Button
                        variant="ghost-action"
                        size="sm"
                        title="انتقال للملف"
                        onClick={(e) => {
                            e.stopPropagation();
                            navigate(`/files/${row.fileID}`);
                        }}
                    >
                        <UserCheck className="w-4 h-4" />
                    </Button>
                </div>
            ),
        },
    ];

    return (
        <CrudPageLayout
            title="إدارة الوكلاء"
            description="عرض وإدارة جميع الوكلاء عبر ملفات التقاعد"
            icon={<Briefcase className="w-6 h-6" />}
            columns={columns}
            data={agents}
            loading={isLoading}
            searchable
            searchInHeader
            searchPlaceholder="ابحث بالاسم أو الرقم الوطني أو رقم الملف أو الرقم العسكري..."
            searchValue={searchTerm}
            onSearchChange={setSearchTerm}
            searchInputRef={searchInputRef}
            rowKey={(row) => row.agentID}
            pagination={{
                page: 1,
                pageSize: 50,
                total: agents.length,
                onPageChange: () => {},
            }}
        >
            {/* Stats */}
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-2 sm:gap-4 mb-6">
                <StatCard
                    title="إجمالي الوكلاء"
                    value={agents.length}
                    icon={<Briefcase className="w-5 h-5" />}
                    variant="blue"
                    loading={isLoading}
                />
                <StatCard
                    title="النشطون"
                    value={activeCount}
                    icon={<UserCheck className="w-5 h-5" />}
                    variant="success"
                    loading={isLoading}
                />
                <StatCard
                    title="غير النشطين"
                    value={agents.length - activeCount}
                    icon={<Trash2 className="w-5 h-5" />}
                    variant="destructive"
                    loading={isLoading}
                />
            </div>

            {/* View Dialog */}
            <ViewAgentDialog
                open={!!viewAgent}
                onClose={() => setViewAgent(null)}
                agent={viewAgent}
                fileId={viewFileId}
            />
        </CrudPageLayout>
    );
}
