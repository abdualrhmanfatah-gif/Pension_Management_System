import { useQuery } from "@tanstack/react-query";
<<<<<<< HEAD
import { Clock, DollarSign, Eye,GitCompare, Users } from "lucide-react";
import { useRef, useState } from "react";

import { salariesApi } from "@/api/salaries";
=======
import { GitCompare, DollarSign, Users, Clock, Eye } from "lucide-react";
import { useRef, useState } from "react";

import { salariesApi } from "@/api/salaries";
import type { SalaryBatch } from "@/types/salaries";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    Badge,
    BaseFormDialog,
    Button,
    CrudPageLayout,
    NativeSelect,
    StatCard,
} from "@/components/ui";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
<<<<<<< HEAD
import type { SalaryBatch } from "@/types/salaries";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface BatchItem {
    batchItemID: number;
    retireeName: string;
    nationalID: string;
    amount: number;
    bankName: string;
    status: string;
}

export function SalaryReconciliationPage() {
    const { searchTerm, setSearchTerm } = useDebouncedSearch();
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [statusFilter, setStatusFilter] = useState("all");
    const [selectedBatch, setSelectedBatch] = useState<SalaryBatch | null>(null);

    const { data, isLoading } = useQuery({
        queryKey: ["salary-batches"],
        queryFn: () => salariesApi.getAll({ page: 1, pageSize: 100 }),
    });

    const items: SalaryBatch[] = data?.items || [];

    const { data: batchItems } = useQuery({
        queryKey: ["salary-batch-items", selectedBatch?.batchID],
        queryFn: () => salariesApi.getItems(selectedBatch!.batchID, { page: 1, pageSize: 100 }),
        enabled: !!selectedBatch,
    });

    const itemsList: BatchItem[] = Array.isArray(batchItems?.items) ? batchItems.items as unknown as BatchItem[] : Array.isArray(batchItems) ? batchItems as unknown as BatchItem[] : [];

    const filteredItems = items.filter((item) => {
        const matchesSearch = !searchTerm || item.batchNumber?.toLowerCase().includes(searchTerm.toLowerCase());
        const matchesStatus = statusFilter === "all" || item.status === statusFilter;
        return matchesSearch && matchesStatus;
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
            render: (row: SalaryBatch) => <span className="text-sm">{row.batchMonth}</span>,
        },
        {
            key: "totalCount",
            header: "العدد",
            render: (row: SalaryBatch) => <span className="text-sm font-medium">{row.totalCount}</span>,
        },
        {
            key: "totalAmount",
            header: "المبلغ الإجمالي",
            render: (row: SalaryBatch) => (
                <span className="text-sm font-bold">{(row.totalAmount || 0).toLocaleString("ar-SA")} ر.ي</span>
            ),
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
                <Button variant="ghost-action" size="sm" onClick={(e) => { e.stopPropagation(); setSelectedBatch(row); }}>
                    <Eye className="w-4 h-4" />
                </Button>
            ),
        },
    ];

    return (
        <CrudPageLayout
            title="مطابقة كشوفات المعاش"
            description="إدارة ومطابقة كشوفات صرف المعاشات"
            icon={<GitCompare className="w-6 h-6" />}
            columns={columns}
            data={filteredItems}
            loading={isLoading}
            searchable
            searchInHeader
            searchPlaceholder="بحث برقم الكشف..."
            searchValue={searchTerm}
            onSearchChange={setSearchTerm}
            searchInputRef={searchInputRef}
            rowKey={(row) => row.batchID}
            headerActions={
                <div className="flex gap-2">
                    <NativeSelect
                        value={statusFilter}
                        onChange={setStatusFilter}
                        options={[
                            { value: "all", label: "الكل" },
                            { value: "Approved", label: "معتمد" },
                            { value: "Pending", label: "معلق" },
                        ]}
                        placeholder="الحالة"
                    />
                </div>
            }
        >
            {/* Stats */}
            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4 mb-6">
                <StatCard title="إجمالي الكشوفات" value={items.length} icon={<GitCompare className="w-5 h-5" />} variant="blue" loading={isLoading} />
                <StatCard title="إجمالي المبالغ" value={`${(totalAmount / 1000).toFixed(0)} ألف`} icon={<DollarSign className="w-5 h-5" />} variant="emerald" loading={isLoading} />
                <StatCard title="إجمالي المستفيدين" value={totalCount.toLocaleString("ar-SA")} icon={<Users className="w-5 h-5" />} variant="amber" loading={isLoading} />
                <StatCard title="بانتظار المطابقة" value={items.filter((i) => i.status === "Pending").length} icon={<Clock className="w-5 h-5" />} variant="warning" loading={isLoading} />
            </div>

            {/* Batch Detail Dialog */}
            <BaseFormDialog
                title={`تفاصيل الكشف - ${selectedBatch?.batchNumber || ""}`}
                isOpen={!!selectedBatch}
                onClose={() => setSelectedBatch(null)}
                size="lg"
                submitLabel="إغلاق"
                onSubmit={() => setSelectedBatch(null)}
            >
                {selectedBatch && (
                    <div className="space-y-4">
                        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                            <div className="bg-muted/30 rounded-lg p-3 text-center">
                                <p className="text-lg font-bold">{selectedBatch.batchMonth}</p>
                                <p className="text-xs text-muted-foreground">الشهر</p>
                            </div>
                            <div className="bg-muted/30 rounded-lg p-3 text-center">
                                <p className="text-lg font-bold">{selectedBatch.totalCount}</p>
                                <p className="text-xs text-muted-foreground">العدد</p>
                            </div>
                            <div className="bg-muted/30 rounded-lg p-3 text-center">
                                <p className="text-lg font-bold">{(selectedBatch.totalAmount || 0).toLocaleString("ar-SA")}</p>
                                <p className="text-xs text-muted-foreground">المبلغ</p>
                            </div>
                            <div className="bg-muted/30 rounded-lg p-3 text-center">
                                <Badge variant={selectedBatch.status === "Approved" ? "success" : "warning"}>
                                    {selectedBatch.status}
                                </Badge>
                            </div>
                        </div>
                        {itemsList.length > 0 && (
                            <div className="overflow-x-auto">
                                <table className="w-full text-sm" role="table" aria-label="عناصر الدفعة">
                                    <thead className="sticky top-0 z-10 bg-background">
                                        <tr className="border-b">
                                            <th className="text-end p-2 font-bold">#</th>
                                            <th className="text-end p-2 font-bold">الاسم</th>
                                            <th className="text-end p-2 font-bold">المبلغ</th>
                                            <th className="text-end p-2 font-bold">البنك</th>
                                            <th className="text-end p-2 font-bold">الحالة</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {itemsList.map((item, idx) => (
                                            <tr key={item.batchItemID || idx} className="border-b hover:bg-muted/30">
                                                <td className="p-2">{idx + 1}</td>
                                                <td className="p-2 font-medium">{item.retireeName}</td>
                                                <td className="p-2 font-bold">{item.amount?.toLocaleString("ar-SA")} ر.ي</td>
                                                <td className="p-2">{item.bankName || "—"}</td>
                                                <td className="p-2">
                                                    <Badge variant={item.status === "Paid" ? "success" : "warning"}>
                                                        {item.status}
                                                    </Badge>
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </div>
                )}
            </BaseFormDialog>
        </CrudPageLayout>
    );
}
