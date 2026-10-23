import { useQuery } from "@tanstack/react-query";
import {
    AlertTriangle,
    Ban,
    BookOpen,
    CalendarDays,
    CheckCircle,
    Clock,
    Download,
    ExternalLink,
    Eye,
    FileText,
    Hash,
    MapPin,
    User,
    XCircle,
    ZoomIn,
} from "lucide-react";
import { useRef, useState } from "react";

import { paymentBookReviewApi } from "@/api/pension";
import {
    Badge,
    BaseFormDialog,
    Button,
    CrudPageLayout,
    FormSection,
    InfoPair,
    NativeSelect,
    StatCard,
} from "@/components/ui";
import { useDebouncedSearch } from "@/hooks/useDebouncedSearch";
import { getDocumentUrl } from "@/lib/utils";
import type { PaymentBookReview } from "@/types/pension";

const statusColors: Record<string, "success" | "warning" | "destructive" | "info" | "outline"> = {
    "فعال": "success",
    "منتهي": "destructive",
    "مفقود": "warning",
    "تالف": "info",
};

const statusLabels: Record<string, string> = {
    "فعال": "نشط",
    "منتهي": "منتهي",
    "مفقود": "مفقود",
    "تالف": "تالف",
};

export function AnnualBookReviewPage() {
    const { searchTerm, setSearchTerm } = useDebouncedSearch();
    const searchInputRef = useRef<HTMLInputElement>(null);
    const [statusFilter, setStatusFilter] = useState("all");
    const [typeFilter, setTypeFilter] = useState("all");
    const [expiringFilter, setExpiringFilter] = useState(false);
    const [selected, setSelected] = useState<PaymentBookReview | null>(null);
    const [previewImage, setPreviewImage] = useState<string | null>(null);

    const { data: stats, isLoading: statsLoading } = useQuery({
        queryKey: ["payment-book-stats"],
        queryFn: () => paymentBookReviewApi.getStats(),
    });

    const { data, isLoading } = useQuery({
        queryKey: ["payment-book-review", { statusFilter, typeFilter, expiringFilter }],
        queryFn: () =>
            paymentBookReviewApi.getReview({
                page: 1,
                pageSize: 100,
                status: statusFilter === "all" ? undefined : statusFilter,
                bookType: typeFilter === "all" ? undefined : typeFilter,
                expiringWithinDays: expiringFilter ? 30 : undefined,
            }),
    });

    const items: PaymentBookReview[] = data?.items || [];

    const filteredItems = items.filter((item) => {
        if (!searchTerm) return true;
        const q = searchTerm.trim().toLowerCase();
        return (
            item.retireeName?.toLowerCase().includes(q) ||
            item.bookNumber?.toLowerCase().includes(q) ||
            item.fileNumber?.toLowerCase().includes(q) ||
            item.recipientName?.toLowerCase().includes(q) ||
            item.militaryNumber?.toLowerCase().includes(q) ||
            item.nationalID?.toLowerCase().includes(q)
        );
    });

    const handleExport = () => {
        const headers = ["رقم الدفتر", "المتقاعد", "رقم الملف", "المستلم", "الرقم العسكري", "الحالة", "تاريخ الانتهاء"];
        const rows = filteredItems.map((item) => [
            item.bookNumber || "",
            item.retireeName || "",
            item.fileNumber || "",
            item.recipientName || "",
            item.militaryNumber || "",
            item.status || "",
            item.expiryDate ? new Date(item.expiryDate).toLocaleDateString("ar-SA") : "",
        ]);
        const csvContent = [headers, ...rows].map((row) => row.join(",")).join("\n");
        const blob = new Blob(["\uFEFF" + csvContent], { type: "text/csv;charset=utf-8;" });
        const url = URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.download = `payment-books-${new Date().toISOString().split("T")[0]}.csv`;
        link.click();
        URL.revokeObjectURL(url);
    };

    const columns = [
        {
            key: "bookNumber",
            header: "رقم الدفتر",
            render: (row: PaymentBookReview) => (
                <span className="font-mono font-bold text-xs text-primary">
                    {row.bookNumber}
                </span>
            ),
        },
        {
            key: "retireeName",
            header: "المتقاعد",
            render: (row: PaymentBookReview) => (
                <div>
                    <p className="font-medium text-sm">{row.retireeName || "—"}</p>
                    <div className="flex items-center gap-2 text-xs text-muted-foreground flex-wrap">
                        {row.fileNumber && <span>ملف: {row.fileNumber}</span>}
                        {row.militaryNumber && <span>عسكري: {row.militaryNumber}</span>}
                    </div>
                </div>
            ),
        },
        {
            key: "recipientName",
            header: "المستفيد",
            render: (row: PaymentBookReview) => (
                <span className="text-sm">{row.recipientName || "—"}</span>
            ),
        },
        {
            key: "bookType",
            header: "النوع",
            render: (row: PaymentBookReview) => (
                <Badge variant="outline">{row.bookType || "—"}</Badge>
            ),
        },
        {
            key: "issuingBranchName",
            header: "الفرع المُصدر",
            render: (row: PaymentBookReview) => (
                <span className="text-sm">{row.issuingBranchName || "—"}</span>
            ),
        },
        {
            key: "issueDate",
            header: "تاريخ الإصدار",
            render: (row: PaymentBookReview) => (
                <span className="text-sm">
                    {row.issueDate ? new Date(row.issueDate).toLocaleDateString("ar-SA") : "—"}
                </span>
            ),
        },
        {
            key: "expiryDate",
            header: "تاريخ الانتهاء",
            render: (row: PaymentBookReview) => {
                const isExpiring = row.isExpiringSoon;
                return (
                    <div className="flex items-center gap-1">
                        <span className="text-sm">
                            {row.expiryDate ? new Date(row.expiryDate).toLocaleDateString("ar-SA") : "—"}
                        </span>
                        {isExpiring && (
                            <span title={`باقي ${row.daysUntilExpiry} يوم`}><AlertTriangle className="w-3 h-3 text-amber-500" /></span>
                        )}
                    </div>
                );
            },
        },
        {
            key: "status",
            header: "الحالة",
            render: (row: PaymentBookReview) => (
                <Badge variant={statusColors[row.status] || "outline"}>
                    {statusLabels[row.status] || row.status}
                </Badge>
            ),
        },
        {
            key: "actions",
            header: "",
            render: (row: PaymentBookReview) => (
                <Button
                    variant="ghost-action"
                    size="sm"
                    onClick={(e) => {
                        e.stopPropagation();
                        setSelected(row);
                    }}
                >
                    <Eye className="w-4 h-4" />
                </Button>
            ),
        },
    ];

    return (
        <CrudPageLayout
            title="مراجعة دفاتر الصرف"
            description="مراجعة حالة دفاتر الصرف وتجديدها"
            icon={<CalendarDays className="w-6 h-6" />}
            columns={columns}
            data={filteredItems}
            loading={isLoading}
            searchable
            searchInHeader
            searchPlaceholder="بحث بالاسم أو رقم الدفتر..."
            searchValue={searchTerm}
            onSearchChange={setSearchTerm}
            searchInputRef={searchInputRef}
            rowKey={(row) => row.paymentBookID}
            headerActions={
                <div className="flex gap-2">
                    <NativeSelect
                        value={statusFilter}
                        onChange={setStatusFilter}
                        options={[
                            { value: "all", label: "الكل" },
                            { value: "فعال", label: "نشط" },
                            { value: "منتهي", label: "منتهي" },
                            { value: "مفقود", label: "مفقود" },
                            { value: "تالف", label: "تالف" },
                        ]}
                        placeholder="الحالة"
                    />
                    <NativeSelect
                        value={typeFilter}
                        onChange={setTypeFilter}
                        options={[
                            { value: "all", label: "الكل" },
                            { value: "pension", label: "معاش" },
                            { value: "compensation", label: "تعويض" },
                        ]}
                        placeholder="النوع"
                    />
                    <Button
                        variant={expiringFilter ? "warning" : "outline"}
                        size="sm"
                        onClick={() => setExpiringFilter(!expiringFilter)}
                    >
                        <AlertTriangle className="w-4 h-4 ml-1" />
                        تنتهي قريباً
                    </Button>
                    <Button variant="outline" size="sm" onClick={handleExport}>
                        <Download className="w-4 h-4 ml-1" />
                        تصدير
                    </Button>
                </div>
            }
        >
            {/* Stats */}
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-2 sm:gap-4 mb-6">
                <StatCard
                    title="نشطة"
                    value={stats?.totalActive ?? 0}
                    icon={<CheckCircle className="w-5 h-5" />}
                    variant="success"
                    loading={statsLoading}
                />
                <StatCard
                    title="منتهية"
                    value={stats?.totalExpired ?? 0}
                    icon={<XCircle className="w-5 h-5" />}
                    variant="destructive"
                    loading={statsLoading}
                />
                <StatCard
                    title="مفقودة"
                    value={stats?.totalLost ?? 0}
                    icon={<Ban className="w-5 h-5" />}
                    variant="warning"
                    loading={statsLoading}
                />
                <StatCard
                    title="تالفة"
                    value={stats?.totalDamaged ?? 0}
                    icon={<AlertTriangle className="w-5 h-5" />}
                    variant="orange"
                    loading={statsLoading}
                />
                <StatCard
                    title="تنتهي خلال 30 يوم"
                    value={stats?.expiringWithin30Days ?? 0}
                    icon={<Clock className="w-5 h-5" />}
                    variant="amber"
                    loading={statsLoading}
                />
            </div>

            {/* View Dialog */}
            <BaseFormDialog
                title="تفاصيل دفتر الصرف"
                isOpen={!!selected}
                onClose={() => setSelected(null)}
                size="lg"
                submitLabel="إغلاق"
                onSubmit={() => setSelected(null)}
            >
                {selected && (
                    <div className="space-y-6">
                        {/* Header Card */}
                        <div className="flex items-center justify-between p-4 bg-muted/40 rounded-xl border border-border shadow-xs">
                            <div className="flex items-center gap-3.5">
                                <div className="p-2.5 bg-primary/10 rounded-xl border border-primary text-primary">
                                    <BookOpen className="w-5 h-5" />
                                </div>
                                <div>
                                    <div className="flex items-center gap-2">
                                        <span className="text-xs text-muted-foreground font-medium">رقم الدفتر:</span>
                                        <span className="font-mono font-bold text-lg text-primary">{selected.bookNumber}</span>
                                    </div>
                                    <p className="text-xs text-muted-foreground font-medium mt-0.5">{selected.bookType || "دفتر صرف معاش"}</p>
                                </div>
                            </div>
                            <Badge variant={statusColors[selected.status] || "outline"} className="text-sm px-3.5 py-1 font-medium shadow-xs">
                                {statusLabels[selected.status] || selected.status}
                            </Badge>
                        </div>

                        {/* Retiree Info */}
                        <FormSection title="بيانات المتقاعد والمستفيد" icon={User} separator>
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                <InfoPair label="اسم المتقاعد" value={selected.retireeName} icon={<User className="w-4 h-4 text-primary" />} />
                                <InfoPair label="رقم الملف التقاعدي" value={selected.fileNumber} icon={<Hash className="w-4 h-4 text-primary" />} mono />
                                <InfoPair label="اسم المستفيد" value={selected.recipientName} icon={<User className="w-4 h-4 text-primary" />} />
                                <InfoPair label="نوع المستفيد" value={selected.recipientType} icon={<FileText className="w-4 h-4 text-primary" />} />
                            </div>
                        </FormSection>

                        {/* Book Details */}
                        <FormSection title="بيانات الدفتر والتخصيص" icon={FileText} separator>
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                <InfoPair label="نوع الدفتر" value={selected.bookType} icon={<BookOpen className="w-4 h-4 text-primary" />} />
                                <InfoPair label="لون الدفتر" value={selected.legacyColor} icon={<FileText className="w-4 h-4 text-primary" />} />
                                <InfoPair label="الفرع المُصدر" value={selected.issuingBranchName} icon={<MapPin className="w-4 h-4 text-primary" />} />
                                <InfoPair label="سبب التغيير / التعديل" value={selected.changeReason} icon={<AlertTriangle className="w-4 h-4 text-primary" />} />
                            </div>
                        </FormSection>

                        {/* Dates */}
                        <FormSection title="صلاحية الدفتر والتواريخ" icon={CalendarDays} separator={Boolean(selected.bookImagePaths && selected.bookImagePaths.length > 0)}>
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                <InfoPair
                                    label="تاريخ الإصدار"
                                    value={selected.issueDate ? new Date(selected.issueDate).toLocaleDateString("ar-SA") : null}
                                    icon={<CalendarDays className="w-4 h-4 text-primary" />}
                                />
                                <div className="space-y-2">
                                    <InfoPair
                                        label="تاريخ الانتهاء"
                                        value={selected.expiryDate ? new Date(selected.expiryDate).toLocaleDateString("ar-SA") : null}
                                        icon={<CalendarDays className="w-4 h-4 text-primary" />}
                                    />
                                    {selected.isExpiringSoon && selected.daysUntilExpiry !== null && (
                                        <div className="flex items-center gap-2 p-2.5 bg-amber-500/10 border border-amber-500 text-amber-700 dark:text-amber-400 rounded-lg text-xs font-medium">
                                            <AlertTriangle className="w-4 h-4 shrink-0 text-amber-500" />
                                            <span>متبقي <strong>{selected.daysUntilExpiry}</strong> يوم على انتهاء صلاحية هذا الدفتر</span>
                                        </div>
                                    )}
                                </div>
                            </div>
                        </FormSection>

                        {/* Book Images */}
                        {selected.bookImagePaths && selected.bookImagePaths.length > 0 && (
                            <FormSection title={`صور ومستندات الدفتر (${selected.bookImagePaths.length})`} icon={BookOpen}>
                                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                    {selected.bookImagePaths.map((imagePath, idx) => (
                                        <div
                                            key={idx}
                                            className="relative group overflow-hidden rounded-xl border border-border bg-muted/20 hover:border-primary hover:shadow-md transition-all cursor-pointer"
                                            onClick={() => setPreviewImage(getDocumentUrl(imagePath) ?? `/${imagePath}`)}
                                        >
                                            <img
                                                src={getDocumentUrl(imagePath) ?? `/${imagePath}`}
                                                alt={`صورة الدفتر ${idx + 1}`}
                                                className="w-full h-48 sm:h-56 object-contain bg-background p-2 transition-transform duration-300 group-hover:scale-105"
                                                onError={(e) => {
                                                    (e.target as HTMLImageElement).style.display = "none";
                                                }}
                                            />
                                            {/* Hover overlay with zoom icon */}
                                            <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2 text-white font-medium text-xs backdrop-blur-[1px]">
                                                <ZoomIn className="w-5 h-5" />
                                                <span>انقر لتكبير الصورة</span>
                                            </div>
                                            <span className="absolute top-2.5 end-2.5 bg-background/90 backdrop-blur-xs text-xs font-semibold px-2.5 py-1 rounded-md border border-border shadow-xs group-hover:opacity-0 transition-opacity">
                                                صورة {idx + 1}
                                            </span>
                                        </div>
                                    ))}
                                </div>
                            </FormSection>
                        )}
                    </div>
                )}
            </BaseFormDialog>

            {/* Image Preview Lightbox Dialog */}
            <BaseFormDialog
                title="معاينة صورة الدفتر المكبرة"
                isOpen={!!previewImage}
                onClose={() => setPreviewImage(null)}
                size="xl"
                submitLabel="إغلاق"
                onSubmit={() => setPreviewImage(null)}
            >
                {previewImage && (
                    <div className="space-y-4">
                        <div className="flex justify-end gap-2">
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() => window.open(previewImage, "_blank")}
                            >
                                <ExternalLink className="w-4 h-4 ms-1" />
                                فتح في تبويب جديد
                            </Button>
                        </div>
                        <div className="relative flex items-center justify-center p-4 bg-muted/20 rounded-xl min-h-[350px] max-h-[70vh] overflow-auto border border-border">
                            <img
                                src={previewImage}
                                alt="معاينة الدفتر المكبرة"
                                className="max-w-full max-h-[65vh] object-contain rounded-lg shadow-sm"
                            />
                        </div>
                    </div>
                )}
            </BaseFormDialog>
        </CrudPageLayout>
    );
}
