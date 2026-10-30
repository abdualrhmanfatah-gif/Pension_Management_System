<<<<<<< HEAD
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
=======
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    Activity,
    ChevronDown,
    ChevronUp,
    Eye,
    FolderOpen,
    Heart,
    MapPin,
    Phone,
    Printer,
    Search,
    Users,
} from "lucide-react";
import { useMemo, useRef, useState } from "react";
<<<<<<< HEAD
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { z } from "zod";

import { lookupApi,socialResearchApi } from "@/api/pension";
import { SearchDialog } from "@/components/common/SearchDialog";
=======
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";

import { socialResearchApi, lookupApi } from "@/api/pension";
import type {
    SocialResearchFilters,
    SocialResearchResult,
} from "@/types/pension";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    Badge,
    BaseFormDialog,
    Button,
    CrudPageLayout,
    DateInput,
    FormField,
    Input,
    NativeSelect,
    StatCard,
<<<<<<< HEAD
    yemenPhoneSchema,
} from "@/components/ui";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import type {
    SocialResearchFilters,
    SocialResearchResult,
} from "@/types/pension";

const updateSchema = z.object({
    phone: yemenPhoneSchema.optional().or(z.literal("")),
=======
} from "@/components/ui";
import { SearchDialog } from "@/components/common/SearchDialog";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { toast } from "sonner";

const updateSchema = z.object({
    phone: z.string().optional(),
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    governorate: z.string().optional(),
    city: z.string().optional(),
});

type UpdateForm = z.infer<typeof updateSchema>;

export function SocialResearchPage() {
    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const { searchTerm, setSearchTerm, debouncedValue } =
        useDebouncedSearch();
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [page, setPage] = useState(1);

    const [showFilters, setShowFilters] = useState(false);
    const [filters, setFilters] = useState<SocialResearchFilters>({});
    const [updateTarget, setUpdateTarget] =
        useState<SocialResearchResult | null>(null);

    const queryFilters = useMemo(
        () => ({
            ...filters,
            hasFiles: true,
            search: debouncedValue || undefined,
            page,
            pageSize: 20,
        }),
        [filters, debouncedValue, page]
    );

    const { data, isLoading } = useQuery({
        queryKey: ["social-research", queryFilters],
        queryFn: () => socialResearchApi.search(queryFilters),
    });

    const { data: socialStatuses = [] } = useQuery({
        queryKey: ["lookup", "socialStatuses"],
        queryFn: () => lookupApi.getSocialStatuses(),
    });

    const { data: healthStatuses = [] } = useQuery({
        queryKey: ["lookup", "healthStatuses"],
        queryFn: () => lookupApi.getHealthStatuses(),
    });

    const { data: governorates = [] } = useQuery({
        queryKey: ["lookup", "governorates"],
        queryFn: () => lookupApi.getGovernorates(),
    });

    const { data: categories = [] } = useQuery({
        queryKey: ["lookup", "categories"],
        queryFn: () => lookupApi.getCategories(),
    });

    const items = data?.items || [];
    const totalCount = data?.totalCount || 0;

    const stats = useMemo(() => {
        const total = totalCount;
        const alive = items.filter((r) => r.isAlive).length;
        const deceased = items.filter((r) => !r.isAlive).length;
        return { total, alive, deceased };
    }, [items, totalCount]);

    const updateForm = useForm<UpdateForm>({
        resolver: zodResolver(updateSchema),
        values: updateTarget
            ? {
                phone: updateTarget.phone || "",
                governorate: updateTarget.governorate || "",
                city: updateTarget.city || "",
            }
            : undefined,
    });

    const updateMutation = useMutation({
        mutationFn: async (data: UpdateForm) => {
            const { default: api } = await import("@/api/axios");
            return api.put(`/social-research/${updateTarget!.retireeID}`, data);
        },
        onSuccess: () => {
            toast.success("تم التحديث بنجاح");
            queryClient.invalidateQueries({ queryKey: ["social-research"] });
            setUpdateTarget(null);
        },
        onError: () => {
            toast.error("خطأ في التحديث");
        },
    });

    const columns = [
        {
            key: "fullName",
            header: "الاسم",
            render: (row: SocialResearchResult) => (
                <span className="font-bold">{row.fullName}</span>
            ),
        },
        {
            key: "nationalID",
            header: "الرقم الوطني",
            render: (row: SocialResearchResult) => (
                <span className="text-sm font-mono">{row.nationalID || "—"}</span>
            ),
        },
        {
            key: "militaryNumber",
            header: "الرقم العسكري",
            render: (row: SocialResearchResult) => (
                <span className="text-sm font-mono text-primary font-bold">
                    {row.militaryNumber || "—"}
                </span>
            ),
        },
        {
            key: "linkNumber",
            header: "رقم الربط",
            render: (row: SocialResearchResult) => (
                <span className="text-sm font-mono">{row.linkNumber || "—"}</span>
            ),
        },
        {
            key: "rankName",
            header: "الرتبة",
            render: (row: SocialResearchResult) => (
                <span className="text-sm">{row.rankName || "—"}</span>
            ),
        },
        {
            key: "unitName",
            header: "الوحدة",
            render: (row: SocialResearchResult) => (
                <span className="text-sm">{row.unitName || "—"}</span>
            ),
        },
        {
            key: "socialStatusName",
            header: "الحالة الاجتماعية",
            render: (row: SocialResearchResult) => (
                <Badge variant="outline">{row.socialStatusName || "—"}</Badge>
            ),
        },
        {
            key: "phone",
            header: "الهاتف",
            render: (row: SocialResearchResult) => (
                <span className="text-sm font-mono flex items-center gap-1">
                    <Phone className="w-3 h-3 text-muted-foreground" />
                    {row.phone || "—"}
                </span>
            ),
        },
        {
            key: "governorate",
            header: "المحافظة",
            render: (row: SocialResearchResult) => (
                <span className="text-sm flex items-center gap-1">
                    <MapPin className="w-3 h-3 text-muted-foreground" />
                    {row.governorate || "—"}
                </span>
            ),
        },
        {
            key: "isAlive",
            header: "الحالة",
            render: (row: SocialResearchResult) => (
                <Badge variant={row.isAlive ? "success" : "danger"}>
                    {row.isAlive ? "على قيد الحياة" : "متوفى"}
                </Badge>
            ),
        },
        {
            key: "fileCount",
            header: "عدد الملفات",
            render: (row: SocialResearchResult) => (
                <span className="text-sm font-bold">{row.fileCount}</span>
            ),
        },
        {
            key: "actions",
            header: "إجراءات",
            render: (row: SocialResearchResult) => (
                <div className="flex gap-1 items-center">
                    <Button
                        variant="ghost"
                        size="sm"
                        className="text-blue-600 dark:text-blue-400 hover:bg-blue-500/15 hover:text-blue-700 dark:hover:text-blue-300"
                        title="عرض التفاصيل"
                        onClick={() => navigate(`/retirees/${row.retireeID}`)}
                    >
                        <Eye className="w-4 h-4 text-blue-600 dark:text-blue-400 shrink-0" />
                    </Button>
                    {row.fileID && (
                        <Button
                            variant="ghost"
                            size="sm"
                            className="text-emerald-600 dark:text-emerald-400 hover:bg-emerald-500/15 hover:text-emerald-700 dark:hover:text-emerald-300"
                            title="عرض الملف التقاعدي"
                            onClick={() => navigate(`/files/${row.fileID}`)}
                        >
                            <FolderOpen className="w-4 h-4 text-emerald-600 dark:text-emerald-400 shrink-0" />
                        </Button>
                    )}
                    <Button
                        variant="ghost"
                        size="sm"
                        className="text-amber-600 dark:text-amber-400 hover:bg-amber-500/15 hover:text-amber-700 dark:hover:text-amber-300"
                        title="طباعة المباينة"
                        onClick={() =>
                            navigate(`/retirees/${row.retireeID}/statement`)
                        }
                    >
                        <Printer className="w-4 h-4 text-amber-600 dark:text-amber-400 shrink-0" />
                    </Button>
                    <Button
                        variant="ghost"
                        size="sm"
                        className="text-indigo-600 dark:text-indigo-400 hover:bg-indigo-500/15 hover:text-indigo-700 dark:hover:text-indigo-300"
                        title="تحديث البيانات"
                        onClick={() => setUpdateTarget(row)}
                    >
                        <Activity className="w-4 h-4 text-indigo-600 dark:text-indigo-400 shrink-0" />
                    </Button>
                </div>
            ),
        },
    ];

    return (
        <CrudPageLayout
            title="البحث الاجتماعي"
            description="بحث شامل عن المتقاعدين وبياناتهم الاجتماعية والصحية والعائلية"
            icon={<Search className="w-6 h-6" />}
            columns={columns}
            data={items}
            loading={isLoading}
            searchable
            searchInHeader
            searchPlaceholder="بحث بالاسم / الرقم الوطني / الرقم العسكري..."
            searchValue={searchTerm}
            onSearchChange={setSearchTerm}
            onSearchKeyDown={(e) => {
                if (e.key === "ArrowDown" || e.key === "ArrowUp")
                    e.preventDefault();
            }}
            searchInputRef={searchInputRef}
            rowKey={(row) => row.retireeID}
            onRowClick={(row) => { if (row.fileID) navigate(`/files/${row.fileID}`); }}
            pagination={{
                page,
                pageSize: 20,
                total: totalCount,
                onPageChange: setPage,
            }}
            headerActions={
                <div className="flex gap-2">
                    <SearchDialog
                        value={searchTerm}
                        onSelect={(val) => { setSearchTerm(val); setPage(1); }}
                        queryKey={["social-research", "dialog-search"]}
                        fetchFn={(term) => socialResearchApi.search({ search: term || undefined, page: 1, pageSize: 100, hasFiles: true } as SocialResearchFilters)}
                        getName={(r) => r.fullName}
                        getSubtitle={(r) => r.nationalID || r.militaryNumber || ""}
                        title="بحث بالاسم"
                        placeholder="اكتب اسم المتقاعد..."
                        hideTrigger
                    />
                    <Button
                        variant={showFilters ? "primary" : "outline"}
                        size="sm"
                        onClick={() => setShowFilters(!showFilters)}
                    >
                        فلاتر متقدمة
                        {showFilters ? (
                            <ChevronUp className="w-5 h-5 mr-1" />
                        ) : (
                            <ChevronDown className="w-5 h-5 mr-1" />
                        )}
                    </Button>
                </div>
            }
        >
            {/* Stats Cards */}
            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4 mb-6">
                <StatCard
                    title="إجمالي المتقاعدين"
                    value={stats.total}
                    icon={<Users className="w-5 h-5" />}
                    variant="blue"
                    loading={isLoading}
                />
                <StatCard
                    title="على قيد الحياة"
                    value={stats.alive}
                    icon={<Heart className="w-5 h-5" />}
                    variant="emerald"
                    loading={isLoading}
                />
                <StatCard
                    title="المتوفين"
                    value={stats.deceased}
                    icon={<Activity className="w-5 h-5" />}
                    variant="rose"
                    loading={isLoading}
                />
                <StatCard
                    title="عدد الملفات"
                    value={items.reduce((sum, r) => sum + r.fileCount, 0)}
                    icon={<FolderOpen className="w-5 h-5" />}
                    variant="amber"
                    loading={isLoading}
                />
            </div>

            {/* Advanced Filters */}
            {showFilters && (
                <div className="bg-muted/30 rounded-lg p-4 mb-6 border border-border">
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                        <NativeSelect
                            label="الحالة الاجتماعية"
                            value={filters.socialStatusId?.toString() || ""}
                            onChange={(val) =>
                                setFilters((f) => ({
                                    ...f,
                                    socialStatusId: val
                                        ? Number(val)
                                        : undefined,
                                }))
                            }
                            options={socialStatuses.map((s) => ({
                                value: String(s.id),
                                label: s.name,
                            }))}
                            placeholder="الكل"
                        />
                        <NativeSelect
                            label="الحالة الصحية"
                            value={filters.healthStatusId?.toString() || ""}
                            onChange={(val) =>
                                setFilters((f) => ({
                                    ...f,
                                    healthStatusId: val
                                        ? Number(val)
                                        : undefined,
                                }))
                            }
                            options={healthStatuses.map((s) => ({
                                value: String(s.id),
                                label: s.name,
                            }))}
                            placeholder="الكل"
                        />
                        <NativeSelect
                            label="الحالة"
                            value={
                                filters.isAlive === undefined
                                    ? ""
                                    : filters.isAlive
                                        ? "true"
                                        : "false"
                            }
                            onChange={(val) =>
                                setFilters((f) => ({
                                    ...f,
                                    isAlive:
                                        val === ""
                                            ? undefined
                                            : val === "true",
                                }))
                            }
                            options={[
                                { value: "true", label: "على قيد الحياة" },
                                { value: "false", label: "متوفى" },
                            ]}
                            placeholder="الكل"
                        />
                        <NativeSelect
                            label="المحافظة"
                            value={filters.governorate || ""}
                            onChange={(val) =>
                                setFilters((f) => ({
                                    ...f,
                                    governorate: val || undefined,
                                }))
                            }
                            options={governorates.map((g) => ({
                                value: g.name,
                                label: g.name,
                            }))}
                            placeholder="الكل"
                        />
                        <NativeSelect
                            label="الفئة"
                            value={filters.categoryId?.toString() || ""}
                            onChange={(val) =>
                                setFilters((f) => ({
                                    ...f,
                                    categoryId: val
                                        ? Number(val)
                                        : undefined,
                                }))
                            }
                            options={categories.map((c) => ({
                                value: String(c.id),
                                label: c.name,
                            }))}
                            placeholder="الكل"
                        />
                        <DateInput
                            label="من تاريخ التقاعد"
                            value={filters.retirementDateFrom || ""}
                            onChange={(val) =>
                                setFilters((f) => ({
                                    ...f,
                                    retirementDateFrom: val || undefined,
                                }))
                            }
                            placeholder="DD/MM/YYYY"
                        />
                        <DateInput
                            label="إلى تاريخ التقاعد"
                            value={filters.retirementDateTo || ""}
                            onChange={(val) =>
                                setFilters((f) => ({
                                    ...f,
                                    retirementDateTo: val || undefined,
                                }))
                            }
                            placeholder="DD/MM/YYYY"
                        />
                        <div className="flex items-end">
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() =>
                                    setFilters({
                                        socialStatusId: undefined,
                                        healthStatusId: undefined,
                                        isAlive: undefined,
                                        governorate: undefined,
                                        categoryId: undefined,
                                        hasFiles: true,
                                        retirementDateFrom: undefined,
                                        retirementDateTo: undefined,
                                    })
                                }
                            >
                                مسح الفلاتر
                            </Button>
                        </div>
                    </div>
                </div>
            )}

            {/* Update Dialog */}
            <BaseFormDialog
                title="تحديث بيانات المتقاعد"
                description={`تحديث بيانات: ${updateTarget?.fullName || ""}`}
                isOpen={!!updateTarget}
                onClose={() => setUpdateTarget(null)}
                onSubmit={updateForm.handleSubmit((data) =>
                    updateMutation.mutate(data)
                )}
                isLoading={updateMutation.isPending}
                size="md"
            >
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <FormField
                        label="الهاتف"
                        error={updateForm.formState.errors.phone?.message}
                    >
                        <Input
                            {...updateForm.register("phone")}
<<<<<<< HEAD
                            placeholder="7xxxxxxx"
=======
                            placeholder="رقم الهاتف"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        />
                    </FormField>
                    <FormField
                        label="المحافظة"
                        error={
                            updateForm.formState.errors.governorate?.message
                        }
                    >
                        <Input
                            {...updateForm.register("governorate")}
                            placeholder="المحافظة"
                        />
                    </FormField>
                    <FormField
                        label="المدينة"
                        error={updateForm.formState.errors.city?.message}
                    >
                        <Input
                            {...updateForm.register("city")}
                            placeholder="المدينة"
                        />
                    </FormField>
                </div>
            </BaseFormDialog>
        </CrudPageLayout>
    );
}
