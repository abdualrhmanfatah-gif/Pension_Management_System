import { useQuery } from "@tanstack/react-query";
<<<<<<< HEAD
import { useEffect,useMemo, useState } from "react";
=======
import { useMemo, useState, useEffect } from "react";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { salariesApi } from "@/api/salaries";
<<<<<<< HEAD
import { CreateSalaryBatchDialog } from "@/components/domain/salaries/CreateSalaryBatchDialog";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    Badge,
    CrudPageLayout,
    type CustomColumnDef,
    NativeSelect} from "@/components/ui";
<<<<<<< HEAD
import { BATCH_STATUS } from "@/constants";
import { queryKeys } from "@/constants/queryKeys";
import { useCrudState } from "@/hooks";
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
=======
import { CreateSalaryBatchDialog } from "@/components/domain/salaries/CreateSalaryBatchDialog";
import { BATCH_STATUS } from "@/constants";
import { useCrudState } from "@/hooks";
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import { queryKeys } from "@/constants/queryKeys";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import type { SalaryBatch } from "@/types/salaries";

/**
 * صفحة كشوفات صرف المعاشات
 * تعرض قائمة بكافة الكشوفات مع إمكانية الفلترة والبحث
 */
export function SalaryBatchesPage() {
    const navigate = useNavigate();
    const canManageBatches = useHasPermission("SalaryBatch.Manage");

    // إدارة حالة الحوارات (Dialogs)
    const { isFormOpen, openCreate, closeForm } = useCrudState<SalaryBatch>();

    // الفلاتر والصفحات
    const [page, setPage] = useState(1);
    const [statusFilter, setStatusFilter] = useState("");
    const pageSize = 10;

    // جلب البيانات
    const { data, isLoading, error: queryError } = useQuery({
        queryKey: queryKeys.salaries.batches.list(page, statusFilter as unknown as number),
        queryFn: () => salariesApi.getAll({
            page,
            pageSize,
            status: statusFilter || undefined
        }),
    });

    useEffect(() => {
        if (queryError) {
            toast.error("فشل تحميل كشوفات الصرف", {
                description: queryError instanceof Error ? queryError.message : "حدث خطأ غير متوقع"
            });
        }
    }, [queryError]);

    // تعريف الأعمدة
    const columns = useMemo<CustomColumnDef<SalaryBatch>[]>(() => [
        {
            key: "batchNumber",
            header: "رقم الكشف",
            sortable: true,
            render: (row) => (
                <span className="font-mono font-medium text-primary">
                    {row.batchNumber || `#${row.batchID}`}
                </span>
            )
        },
        {
            key: "bankName",
            header: "البنك",
            accessor: (row) => row.bankName || "—"
        },
        {
            key: "batchMonth",
            header: "شهر الصرف",
            render: (row) => new Date(row.batchMonth).toLocaleDateString("ar-YE", {
                year: "numeric",
                month: "long"
            })
        },
        {
            key: "totalCount",
            header: "عدد المستفيدين",
            render: (row) => <span className="font-mono">{row.totalCount}</span>
        },
        {
            key: "totalAmount",
            header: "الإجمالي",
            render: (row) => (
                <span className="font-mono font-semibold">
                    {row.totalAmount.toLocaleString()} ر.ي
                </span>
            )
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => {
                const variant = row.status === BATCH_STATUS.APPROVED ? "success" :
                    row.status === BATCH_STATUS.DRAFT ? "warning" : "outline";
                return (
                    <Badge variant={variant}>
                        {row.status}
                    </Badge>
                );
            }
        },
        {
            key: "createdAt",
            header: "تاريخ الإنشاء",
            render: (row) => (
                <span className="text-muted-foreground">
                    {formatDate(row.createdAt)}
                </span>
            )
        }
    ], []);

    return (
        <CrudPageLayout
            title="كشوفات صرف المعاشات"
            description={`${data?.totalCount || 0} كشف صرف موجود في النظام`}
            createLabel="كشف جديد"
            onCreateClick={canManageBatches ? openCreate : undefined}
            canCreate={canManageBatches}
            columns={columns}
            data={data?.items || []}
            loading={isLoading}
            pagination={{
                page: page,
                pageSize: pageSize,
                total: data?.totalCount || 0,
                onPageChange: setPage
            }}
            onRowClick={(row) => navigate(`/salaries/${row.batchID}`)}
            emptyMessage="لا توجد كشوفات صرف"
            headerActions={
                <div className="flex gap-3 items-end me-4">
                    <NativeSelect
                        value={statusFilter}

                        onChange={setStatusFilter}
                        options={[
                            { value: "", label: "الكل" },
                            { value: "مسودة", label: "مسودة" },
                            { value: "معتمد", label: "معتمد" },
                        ]}
                        placeholder="فلترة بالحالة"
                        className="w-40"
                    />
                </div>
            }
        >
            <CreateSalaryBatchDialog
                open={isFormOpen}
                onOpenChange={closeForm}
            />
        </CrudPageLayout>
    );
}
