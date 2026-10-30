<<<<<<< HEAD
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { DollarSign,Eye, Receipt } from "lucide-react";
import { useRef, useState } from "react";
import { toast } from "sonner";

import { type FinancialEntitlementDto,financialEntitlementsApi } from "@/api/financial-entitlements";
import {
    Badge,
    BaseFormDialog,
    Button,
    CrudPageLayout,
    Heading,
    InfoPair,
    StatCard,
} from "@/components/ui";
=======
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Receipt, Eye, DollarSign } from "lucide-react";
import { useRef, useState } from "react";

import { financialEntitlementsApi, type FinancialEntitlementDto } from "@/api/financial-entitlements";
import {
    Badge,
    CrudPageLayout,
    Heading,
    StatCard,
    Button,
    InfoPair,
    BaseFormDialog,
} from "@/components/ui";
import { toast } from "sonner";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

export function FinancialEntitlementsPage() {
    const queryClient = useQueryClient();
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [searchQuery, setSearchQuery] = useState("");
    const [activeSearch, setActiveSearch] = useState("");
    const [selected, setSelected] = useState<FinancialEntitlementDto | null>(null);

    const { data: entitlements = [], isLoading } = useQuery({
        queryKey: ["financial-entitlements-search", activeSearch],
        queryFn: () => financialEntitlementsApi.search(activeSearch).then(r => r.data || []),
        enabled: !!activeSearch,
    });

    const endMutation = useMutation({
        mutationFn: ({ id, endDate }: { id: number; endDate: string }) =>
            financialEntitlementsApi.end(id, { endDate }),
        onSuccess: () => {
            toast.success("تم إنهاء الاستحقاق بنجاح");
            queryClient.invalidateQueries({ queryKey: ["financial-entitlements-search"] });
        },
        onError: () => {
            toast.error("خطأ في إنهاء الاستحقاق");
        },
    });

    const handleSearch = () => {
        const q = searchQuery.trim();
        if (q) {
            setActiveSearch(q);
        } else {
            toast.error("أدخل كلمة للبحث");
        }
    };

    const activeCount = entitlements.filter((e) => e.isActive).length;
    const totalAmount = entitlements.reduce((acc, e) => acc + (e.amount || 0), 0);

    const formatCurrency = (amount: number) => {
        return new Intl.NumberFormat("ar-YE", {
            style: "currency",
            currency: "YER",
        }).format(amount);
    };

    const columns = [
        {
            key: "fileNumber",
            header: "رقم الملف",
            render: (row: FinancialEntitlementDto) => (
                <span className="text-sm font-mono">{row.fileNumber || "—"}</span>
            ),
        },
        {
            key: "retireeFullName",
            header: "اسم المتتقاعد",
            render: (row: FinancialEntitlementDto) => (
                <span className="font-bold text-foreground">{row.retireeFullName || "—"}</span>
            ),
        },
        {
            key: "militaryNumber",
            header: "الرقم العسكري",
            render: (row: FinancialEntitlementDto) => (
                <span className="text-sm font-mono">{row.militaryNumber || "—"}</span>
            ),
        },
        {
            key: "phone",
            header: "هاتف المتتقاعد",
            render: (row: FinancialEntitlementDto) => (
<<<<<<< HEAD
                <span className="text-sm" dir="ltr">{row.phone || "—"}</span>
=======
                <span className="text-sm font-mono" dir="ltr">{row.phone || "—"}</span>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            ),
        },
        {
            key: "agentName",
            header: "اسم الوكيل",
            render: (row: FinancialEntitlementDto) => (
                <span className="text-sm">{row.agentName || "—"}</span>
            ),
        },
        {
            key: "amount",
            header: "المبلغ",
            render: (row: FinancialEntitlementDto) => (
                <span className="text-sm font-bold font-mono text-primary">
                    {formatCurrency(row.amount)}
                </span>
            ),
        },
        {
            key: "isActive",
            header: "الحالة",
            render: (row: FinancialEntitlementDto) => (
                <Badge variant={row.isActive ? "success" : "destructive"}>
                    {row.isActive ? "نشط" : "منتهي"}
                </Badge>
            ),
        },
        {
            key: "actions",
            header: "",
            render: (row: FinancialEntitlementDto) => (
                <div className="flex gap-1">
                    <Button
                        variant="ghost-action"
                        size="sm"
                        title="عرض التفاصيل"
                        onClick={(e) => {
                            e.stopPropagation();
                            setSelected(row);
                        }}
                    >
                        <Eye className="w-4 h-4" />
                    </Button>
                </div>
            ),
        },
    ];

    return (
        <CrudPageLayout
            title="الاستحقاقات المالية"
            description="عرض وإدارة الاستحقاقات المالية للمتقاعدين"
            icon={<Receipt className="w-6 h-6" />}
            columns={columns}
            data={entitlements}
            loading={isLoading}
            searchable
            searchInHeader
            searchPlaceholder="رقم الملف، الرقم العسكري، الهاتف، اسم المتتقاعد، اسم الوكيل..."
            searchValue={searchQuery}
            onSearchChange={setSearchQuery}
            onSearchKeyDown={(e) => {
                if (e.key === "Enter") handleSearch();
            }}
            searchInputRef={searchInputRef}
            rowKey={(row) => row.entitlementID}
            pagination={{
                page: 1,
                pageSize: 50,
                total: entitlements.length,
                onPageChange: () => {},
            }}
        >
            {/* Stats */}
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-2 sm:gap-4 mb-6">
                <StatCard
                    title="إجمالي الاستحقاقات"
                    value={entitlements.length}
                    icon={<Receipt className="w-5 h-5" />}
                    variant="blue"
                    loading={isLoading}
                />
                <StatCard
                    title="النشطة"
                    value={activeCount}
                    icon={<DollarSign className="w-5 h-5" />}
                    variant="success"
                    loading={isLoading}
                />
                <StatCard
                    title="المبلغ الإجمالي"
                    value={formatCurrency(totalAmount)}
                    icon={<DollarSign className="w-5 h-5" />}
                    variant="warning"
                    loading={isLoading}
                />
            </div>

            {/* No search message */}
            {!activeSearch && (
                <div className="text-center py-12 text-muted-foreground">
<<<<<<< HEAD
                    <Receipt className="w-12 h-12 mx-auto mb-4 opacity-100" />
                    <p className="text-sm">أدخل كلمة للبحث في الاستحقاقات المالية</p>
                    <p className="text-xs mt-1 opacity-100">رقم الملف، الرقم العسكري، الهاتف، اسم المتتقاعد، اسم الوكيل</p>
=======
                    <Receipt className="w-12 h-12 mx-auto mb-4 opacity-30" />
                    <p className="text-sm">أدخل كلمة للبحث في الاستحقاقات المالية</p>
                    <p className="text-xs mt-1 opacity-60">رقم الملف، الرقم العسكري، الهاتف، اسم المتتقاعد، اسم الوكيل</p>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                </div>
            )}

            {/* View Dialog */}
            <BaseFormDialog
                title="تفاصيل الاستحقاق"
                isOpen={!!selected}
                onClose={() => setSelected(null)}
                size="md"
                submitLabel="إغلاق"
                onSubmit={() => setSelected(null)}
            >
                {selected && (
                    <div className="space-y-4">
                        <div className="flex items-center gap-4">
                            <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center text-primary">
                                <DollarSign className="w-8 h-8" />
                            </div>
                            <div>
                                <Heading level="h3" className="text-lg font-bold">{selected.retireeFullName || "—"}</Heading>
                                <p className="text-sm text-muted-foreground">رقم الملف: {selected.fileNumber || "—"}</p>
                            </div>
                        </div>
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                            <InfoPair label="رقم الملف" value={selected.fileNumber || "—"} />
                            <InfoPair label="الرقم العسكري" value={selected.militaryNumber || "—"} />
                            <InfoPair label="هاتف المتتقاعد" value={selected.phone || "—"} />
                            <InfoPair label="اسم الوكيل" value={selected.agentName || "—"} />
                            <InfoPair label="المبلغ" value={formatCurrency(selected.amount)} />
                            <InfoPair
                                label="الحالة"
                                value={
                                    <Badge variant={selected.isActive ? "success" : "destructive"}>
                                        {selected.isActive ? "نشط" : "منتهي"}
                                    </Badge>
                                }
                            />
                            <InfoPair
                                label="تاريخ البدء"
                                value={selected.effectiveDate ? new Date(selected.effectiveDate).toLocaleDateString("ar-SA") : "—"}
                            />
                            <InfoPair
                                label="تاريخ الانتهاء"
                                value={selected.endDate ? new Date(selected.endDate).toLocaleDateString("ar-SA") : "—"}
                            />
                        </div>
                        {selected.notes && (
                            <div>
                                <span className="text-sm text-muted-foreground">ملاحظات:</span>
                                <p className="text-sm mt-1">{selected.notes}</p>
                            </div>
                        )}
                        <div className="flex gap-2 pt-4">
                            {selected.isActive && (
                                <Button
                                    variant="destructive"
                                    size="sm"
                                    onClick={() => {
                                        const endDate = prompt("أدخل تاريخ الانتهاء (YYYY-MM-DD):");
                                        if (endDate) {
                                            endMutation.mutate({ id: selected.entitlementID, endDate });
                                            setSelected(null);
                                        }
                                    }}
                                >
                                    إنهاء الاستحقاق
                                </Button>
                            )}
                        </div>
                    </div>
                )}
            </BaseFormDialog>
        </CrudPageLayout>
    );
}
