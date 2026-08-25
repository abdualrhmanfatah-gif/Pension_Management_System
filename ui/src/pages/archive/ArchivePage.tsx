import { useQuery } from "@tanstack/react-query";
import { Archive, CheckCircle, Clock, FileText, Search, AlertTriangle, Eye, Download, FileX, FileWarning, RotateCcw } from "lucide-react";
import { useEffect, useState } from "react";


import { archiveApi } from "@/api/archive";
import { Badge, Button, Card, DataTable, DateInput, Input, NativeSelect, StatCard, type CustomColumnDef, Heading, PageLayout } from "@/components/ui";
import { SearchDialog } from "@/components/common/SearchDialog";

import { formatDate, getDocumentUrl } from "@/lib/utils";
import { PENDING_REVIEW_STATUS } from "@/types/archive";
import type { ArchiveDocument, ArchiveFilters, ArchiveStats, DocumentReviewFilters } from "@/types/archive";

export function ArchivePage() {
    const [page, setPage] = useState(1);
    const pageSize = 20;
    const [search, setSearch] = useState("");
    const [debouncedSearch, setDebouncedSearch] = useState("");
    const [ownerType, setOwnerType] = useState<number | undefined>(undefined);
    const [verified, setVerified] = useState<boolean | undefined>(undefined);
    const [dateFrom, setDateFrom] = useState<string | undefined>(undefined);
    const [dateTo, setDateTo] = useState<string | undefined>(undefined);

    useEffect(() => {
        const timer = setTimeout(() => {
            setDebouncedSearch(search);
            setPage(1);
        }, 500);
        return () => clearTimeout(timer);
    }, [search]);

    const filters: ArchiveFilters = {
        ownerType,
        verified,
        dateFrom,
        dateTo,
        searchTerm: debouncedSearch || undefined,
        page,
        pageSize,
    };

    const { data, isLoading } = useQuery({
        queryKey: ["archive", "documents", filters],
        queryFn: () => archiveApi.getDocuments(filters),
    });

    const { data: stats } = useQuery<ArchiveStats>({
        queryKey: ["archive", "stats"],
        queryFn: async () => {
            const result = await archiveApi.getStats();
            return result as unknown as ArchiveStats;
        },
    });

    const documents = data?.items || [];
    const totalCount = data?.totalCount || 0;

    const getStatusBadge = (doc: ArchiveDocument) => {
        if (!doc.verified) {
            return <Badge variant="warning">غير موثقة</Badge>;
        }
        if (doc.pendingReviewStatus === PENDING_REVIEW_STATUS.PENDING) {
            return <Badge variant="info">بانتظار المراجعة</Badge>;
        }
        if (doc.pendingReviewStatus === PENDING_REVIEW_STATUS.APPROVED) {
            return <Badge variant="success">موثقة</Badge>;
        }
        return <Badge variant="secondary">—</Badge>;
    };

    const columns: CustomColumnDef<ArchiveDocument>[] = [
        {
            key: "documentId",
            header: "الرقم",
            render: (row) => (
                <span className="font-mono font-bold text-primary">{row.documentId}</span>
            ),
        },
        {
            key: "fileNumber",
            header: "رقم الملف",
            render: (row) => (
                <span className="text-sm font-mono text-muted-foreground">{row.fileNumber || "—"}</span>
            ),
        },
        {
            key: "fileName",
            header: "اسم الملف",
            render: (row) => (
                <div className="flex items-center gap-2">
                    <FileText className="w-4 h-4 text-muted-foreground" />
                    <span className="text-sm font-medium truncate max-w-[200px]">{row.fileName}</span>
                </div>
            ),
        },
        {
            key: "documentTypeName",
            header: "نوع الوثيقة",
            render: (row) => (
                <span className="text-sm">{row.documentTypeName}</span>
            ),
        },
        {
            key: "ownerType",
            header: "القسم",
            render: (row) => (
                <Badge variant="outline">{row.ownerTypeName}</Badge>
            ),
        },
        {
            key: "uploadedBy",
            header: "رفع بواسطة",
            render: (row) => (
                <span className="text-sm text-muted-foreground">{row.uploadedBy}</span>
            ),
        },
        {
            key: "createdAt",
            header: "تاريخ الرفع",
            render: (row) => (
                <span className="text-sm">{formatDate(row.createdAt)}</span>
            ),
        },
        {
            key: "expiryDate",
            header: "انتهاء الصلاحية",
            render: (row) => (
                <span className="text-sm text-muted-foreground">
                    {row.expiryDate ? formatDate(row.expiryDate) : "—"}
                </span>
            ),
        },
        {
            key: "verified",
            header: "الحالة",
            render: (row) => getStatusBadge(row),
        },
        {
            key: "actions",
            header: "",
            render: (row) => (
                <div className="flex items-center gap-1">
                    <Button
                        variant="ghost"
                        size="sm"
                        onClick={(e) => {
                            e.stopPropagation();
                            window.open(getDocumentUrl(row.filePath) ?? "", "_blank");
                        }}
                    >
                        <Eye className="w-4 h-4" />
                    </Button>
                    <Button
                        variant="ghost"
                        size="sm"
                        onClick={(e) => {
                            e.stopPropagation();
                            const link = document.createElement("a");
                            link.href = getDocumentUrl(row.filePath) ?? "";
                            link.download = row.fileName;
                            link.click();
                        }}
                    >
                        <Download className="w-4 h-4" />
                    </Button>
                </div>
            ),
        },
    ];

    const clearFilters = () => {
        setSearch("");
        setDebouncedSearch("");
        setOwnerType(undefined);
        setVerified(undefined);
        setDateFrom(undefined);
        setDateTo(undefined);
        setPage(1);
    };

    return (
        <PageLayout>
            <Heading title="إدارة الأرشيف" />

            {/* Stats Cards */}
            <div className="grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-8 gap-2 sm:gap-3 mb-6">
                <StatCard
                    title="إجمالي الوثائق"
                    value={stats?.totalDocuments ?? 0}
                    icon={<Archive className="w-6 h-6" />}
                    variant="blue"
                    isLoading={isLoading}
                />
                <StatCard
                    title="الوثائق الموثقة"
                    value={stats?.verifiedDocuments ?? 0}
                    icon={<CheckCircle className="w-6 h-6" />}
                    variant="success"
                    isLoading={isLoading}
                />
                <StatCard
                    title="بانتظار المراجعة"
                    value={stats?.pendingReview ?? 0}
                    icon={<Clock className="w-6 h-6" />}
                    variant="warning"
                    isLoading={isLoading}
                />
                <StatCard
                    title="منتهية الصلاحية"
                    value={stats?.expiredDocuments ?? 0}
                    icon={<AlertTriangle className="w-6 h-6" />}
                    variant="destructive"
                    isLoading={isLoading}
                />
                <StatCard
                    title="الملفات الناقصة"
                    value={stats?.filesWithMissingDocuments ?? 0}
                    icon={<FileX className="w-6 h-6" />}
                    variant="destructive"
                    isLoading={isLoading}
                />
                <StatCard
                    title="إجمالي الناقصات"
                    value={stats?.totalMissingDocuments ?? 0}
                    icon={<FileWarning className="w-6 h-6" />}
                    variant="warning"
                    isLoading={isLoading}
                />
                <StatCard
                    title="بانتظار التسليم"
                    value={stats?.pendingMissingDocuments ?? 0}
                    icon={<Clock className="w-6 h-6" />}
                    variant="warning"
                    isLoading={isLoading}
                />
                <StatCard
                    title="متعهد بها"
                    value={stats?.pledgedMissingDocuments ?? 0}
                    icon={<CheckCircle className="w-6 h-6" />}
                    variant="success"
                    isLoading={isLoading}
                />
            </div>

            {/* Filters */}
            <Card className="p-4 mb-6">
                <div className="flex flex-col sm:flex-row items-end gap-2 flex-nowrap">
                    <SearchDialog
                        value={search}
                        onSelect={(val) => { setSearch(val); setPage(1); }}
                        queryKey={["archive", "dialog-search"]}
                        fetchFn={(term) => archiveApi.getReviewDocuments({ searchTerm: term || undefined, page: 1, pageSize: 100 } as DocumentReviewFilters)}
                        getName={(doc) => doc.retireeName || doc.entityName || ""}
                        getSubtitle={(doc) => doc.fileNumber || doc.fileName}
                        title="بحث بالاسم"
                        placeholder="اكتب اسم المتقاعد أو المستحق..."
                    />

                    <div className="relative flex-1 min-w-0">
                        <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                        <Input
                            placeholder="بحث بالاسم..."
                            value={search}
                            onChange={(e) => setSearch(e.target.value)}
                            className="pr-10"
                        />
                    </div>

                    <NativeSelect
                        className="w-36 shrink-0"
                        value={ownerType ?? ""}
                        placeholder=""
                        onChange={(val) => {
                            setOwnerType(val ? Number(val) : undefined);
                            setPage(1);
                        }}
                        options={[
                            { value: "", label: "جميع الأقسام" },
                            { value: "1", label: "ملفات التقاعد" },
                            { value: "2", label: "المتقاعدون" },
                            { value: "3", label: "المستحقون" },
                            { value: "4", label: "الوكلاء" },
                            { value: "5", label: "الأوصياء" },
                            { value: "15", label: "مشاريع الاستثمار" },
                            { value: "40", label: "الموظفون" },
                            { value: "41", label: "المراسلات" },
                        ]}
                    />

                    <NativeSelect
                        className="w-36 shrink-0"
                        value={verified === undefined ? "" : verified.toString()}
                        placeholder=""
                        onChange={(val) => {
                            setVerified(val === "" ? undefined : val === "true");
                            setPage(1);
                        }}
                        options={[
                            { value: "", label: "جميع الحالات" },
                            { value: "true", label: "موثقة" },
                            { value: "false", label: "غير موثقة" },
                        ]}
                    />

                    <DateInput
                        label="من تاريخ"
                        className="w-36 shrink-0"
                        value={dateFrom || ""}
                        onChange={(val) => {
                            setDateFrom(val || undefined);
                            setPage(1);
                        }}
                    />

                    <DateInput
                        label="إلى تاريخ"
                        className="w-36 shrink-0"
                        value={dateTo || ""}
                        onChange={(val) => {
                            setDateTo(val || undefined);
                            setPage(1);
                        }}
                    />

                    <Button variant="ghost" size="icon" className="shrink-0" onClick={clearFilters} title="إعادة تعيين">
                        <RotateCcw className="w-4 h-4" />
                    </Button>
                </div>
            </Card>

            {/* Data Table */}
            <Card>
                <DataTable
                    columns={columns}
                    data={documents}
                    loading={isLoading}
                    emptyMessage="لا توجد وثائق"
                    rowKey={(row, index) => row.documentId ?? index}
                    pagination={{
                        page,
                        pageSize,
                        total: totalCount,
                        onPageChange: setPage,
                    }}
                />
            </Card>
        </PageLayout>
    );
}
