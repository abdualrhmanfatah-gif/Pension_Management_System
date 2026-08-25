import { useQuery } from "@tanstack/react-query";
import { CalendarDays, DollarSign, Users, TrendingUp, Eye, ExternalLink } from "lucide-react";
import { useState, useRef } from "react";
import { useNavigate } from "react-router-dom";

import { salariesApi } from "@/api/salaries";
import type { SalaryBatch } from "@/types/salaries";
import {
    Badge,
    BaseFormDialog,
    Button,
    CrudPageLayout,
    StatCard,
} from "@/components/ui";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";

export function MonthlySummariesPage() {
    const navigate = useNavigate();
    const { searchTerm, setSearchTerm } = useDebouncedSearch();
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [selected, setSelected] = useState<SalaryBatch | null>(null);

    const { data, isLoading } = useQuery({
        queryKey: ["salary-batches"],
        queryFn: () => salariesApi.getAll({ page: 1, pageSize: 100 }),
    });

    const items: SalaryBatch[] = data?.items || [];

    const filteredItems = items.filter((item) => {
        return !searchTerm || item.batchNumber?.toLowerCase().includes(searchTerm.toLowerCase()) || item.batchMonth?.includes(searchTerm);
    });

    const totalAmount = items.reduce((acc, i) => acc + (i.totalAmount || 0), 0);
    const totalCount = items.reduce((acc, i) => acc + (i.totalCount || 0), 0);

    const columns = [
        {
            key: "batchNumber",
            header: "رقم الكشف",
            render: (row: SalaryBatch) => <span className="font-mono font-bold text-xs">{row.batchNumber}</span>,
        },
        {
            key: "batchMonth",
            header: "الشهر",
            render: (row: SalaryBatch) => <span className="text-sm font-medium">{row.batchMonth}</span>,
        },
        {
            key: "totalCount",
            header: "عدد المستفيدين",
            render: (row: SalaryBatch) => <span className="text-sm font-medium">{row.totalCount}</span>,
        },
        {
            key: "totalAmount",
            header: "المبلغ الإجمالي",
            render: (row: SalaryBatch) => <span className="text-sm font-bold">{(row.totalAmount || 0).toLocaleString("ar-SA")} ر.ي</span>,
        },
        {
            key: "status",
            header: "الحالة",
            render: (row: SalaryBatch) => (
                <Badge variant={row.status === "Approved" ? "success" : row.status === "Pending" ? "warning" : "outline"}>
                    {row.status === "Approved" ? "معتمد" : row.status === "Pending" ? "معلق" : row.status}
                </Badge>
            ),
        },
        {
            key: "actions",
            header: "",
            render: (row: SalaryBatch) => (
                <div className="flex gap-1">
                    <Button variant="ghost-action" size="sm" onClick={(e) => { e.stopPropagation(); setSelected(row); }}>
                        <Eye className="w-4 h-4" />
                    </Button>
                    <Button
                        variant="ghost-action"
                        size="sm"
                        title="عرض التفاصيل"
                        onClick={(e) => {
                            e.stopPropagation();
                            navigate(`/salaries/${row.batchID}`);
                        }}
                    >
                        <ExternalLink className="w-4 h-4" />
                    </Button>
                </div>
            ),
        },
    ];

    return (
        <CrudPageLayout
            title="الخلاصات الشهرية"
            description="ملخصات كشوفات الصرف الشهرية"
            icon={<CalendarDays className="w-6 h-6" />}
            columns={columns}
            data={filteredItems}
            loading={isLoading}
            searchable
            searchInHeader
            searchPlaceholder="بحث برقم الكشف أو الشهر..."
            searchValue={searchTerm}
            onSearchChange={setSearchTerm}
            searchInputRef={searchInputRef}
            rowKey={(row) => row.batchID}
        >
            {/* Stats */}
            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4 mb-6">
                <StatCard title="إجمالي الكشوفات" value={items.length} icon={<CalendarDays className="w-5 h-5" />} variant="blue" loading={isLoading} />
                <StatCard title="إجمالي المبالغ" value={`${(totalAmount / 1000000).toFixed(1)}M`} icon={<DollarSign className="w-5 h-5" />} variant="emerald" loading={isLoading} />
                <StatCard title="إجمالي المستفيدين" value={totalCount.toLocaleString("ar-SA")} icon={<Users className="w-5 h-5" />} variant="amber" loading={isLoading} />
                <StatCard title="معدل الصرف" value={`${(totalAmount / Math.max(items.length, 1) / 1000).toFixed(0)} ألف`} icon={<TrendingUp className="w-5 h-5" />} variant="info" loading={isLoading} />
            </div>

            {/* View Dialog */}
            <BaseFormDialog
                title="تفاصيل الكشف"
                isOpen={!!selected}
                onClose={() => setSelected(null)}
                size="md"
                submitLabel="إغلاق"
                onSubmit={() => setSelected(null)}
            >
                {selected && (
                    <div className="space-y-3 text-sm">
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">رقم الكشف:</span>
                            <span className="font-mono font-bold">{selected.batchNumber}</span>
                        </div>
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">الشهر:</span>
                            <span className="font-medium">{selected.batchMonth}</span>
                        </div>
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">عدد المستفيدين:</span>
                            <span className="font-medium">{selected.totalCount}</span>
                        </div>
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">المبلغ الإجمالي:</span>
                            <span className="font-bold text-lg">{(selected.totalAmount || 0).toLocaleString("ar-SA")} ر.ي</span>
                        </div>
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">المتوسط للفرد:</span>
                            <span className="font-medium">
                                {selected.totalCount ? ((selected.totalAmount || 0) / selected.totalCount).toLocaleString("ar-SA") : "—"} ر.ي
                            </span>
                        </div>
                        <div className="flex justify-between">
                            <span className="text-muted-foreground">الحالة:</span>
                            <Badge variant={selected.status === "Approved" ? "success" : "warning"}>{selected.status}</Badge>
                        </div>
                    </div>
                )}
            </BaseFormDialog>
        </CrudPageLayout>
    );
}
