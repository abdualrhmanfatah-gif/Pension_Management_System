import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Calendar, PlayCircle, TrendingDown, Wallet } from "lucide-react";
import { useMemo, useState } from "react";
import { Link } from "react-router-dom";

import { fixedAssetApi, type FixedAssetDto } from "@/api/fixedAssets";
import { DepreciationRunDialog } from "@/components/domain/assets/DepreciationRunDialog";
import { Breadcrumb, Button, Card, CardContent, DateInput, Heading, PageLayout, StatCard, Text } from "@/components/ui";
import { type CustomColumnDef,DataTable } from "@/components/ui/DataTable";
import { formatCurrency, formatDate } from "@/lib/utils";

export function DepreciationRunPage() {
    const [runDialogOpen, setRunDialogOpen] = useState(false);
    const [fromDate, setFromDate] = useState(() => {
        const now = new Date();
        return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}-01`;
    });
    const [toDate, setToDate] = useState(() => {
        const now = new Date();
        return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}-${String(now.getDate()).padStart(2, "0")}`;
    });

    const { data: assets = [], isLoading } = useQuery({
        queryKey: ["fixed-assets"],
        queryFn: () => fixedAssetApi.getAll()
    });

    const activeAssets = useMemo(
        () => assets.filter((a: FixedAssetDto) => a.status === 0 && a.depreciationRate > 0),
        [assets]
    );

    const filteredAssets = useMemo(() => {
        if (!fromDate || !toDate) return activeAssets;
        const from = new Date(fromDate);
        const to = new Date(toDate);
        return activeAssets.filter((a: FixedAssetDto) => {
            const serviceDate = new Date(a.inServiceDate);
            // الأصل مؤهل للإهلاك خلال الفترة إذا:
            // 1. تاريخ التشغيل قبل نهاية الفترة (بدأ العمل قبل نهاية الفترة)
            // 2. لم يتم التخلص منه قبل بداية الفترة
            const notDisposedBeforePeriod = !a.disposalDate || new Date(a.disposalDate) >= from;
            return serviceDate <= to && notDisposedBeforePeriod;
        });
    }, [activeAssets, fromDate, toDate]);

    const totalCost = useMemo(
        () => filteredAssets.reduce((sum: number, a: FixedAssetDto) => sum + a.acquiredCost, 0),
        [filteredAssets]
    );
    const totalNetBookValue = useMemo(
        () => filteredAssets.reduce((sum: number, a: FixedAssetDto) => sum + a.netBookValue, 0),
        [filteredAssets]
    );
    const totalAccumulatedDepreciation = useMemo(
        () => filteredAssets.reduce((sum: number, a: FixedAssetDto) => sum + a.accumulatedDepreciation, 0),
        [filteredAssets]
    );

    const columns: CustomColumnDef<FixedAssetDto>[] = useMemo(() => [
        {
            key: "assetNumber",
            header: "رقم الأصل",
            className: "text-center",
            render: (row) => (
                <div className="font-semibold text-foreground font-mono text-center" dir="ltr">{row.assetNumber}</div>
            ),
        },
        {
            key: "assetName",
            header: "اسم الأصل",
            render: (row) => (
                <Link
                    to={`/assets/fixed/${row.assetID}`}
                    className="text-sm font-medium text-primary hover:underline"
                >
                    {row.assetName}
                </Link>
            ),
        },
        {
            key: "categoryName",
            header: "التصنيف",
            render: (row) => (
                <span className="text-sm text-muted-foreground">{row.categoryName || "—"}</span>
            ),
        },
        {
            key: "quantity",
            header: "الكمية",
            className: "text-center",
            render: (row) => (
                <span className="font-mono text-sm">{row.quantity}</span>
            ),
        },
        {
            key: "depreciationRate",
            header: "نسبة الإهلاك",
            className: "text-center",
            render: (row) => (
                <span className="font-mono text-sm text-amber-600 font-semibold">{row.depreciationRate}%</span>
            ),
        },
        {
            key: "inServiceDate",
            header: "تاريخ التشغيل",
            className: "text-center",
            render: (row) => (
                <span className="text-sm text-muted-foreground" dir="ltr">{formatDate(row.inServiceDate)}</span>
            ),
        },
        {
            key: "acquiredCost",
            header: "التكلفة",
            className: "text-center",
            render: (row) => (
                <span className="font-mono text-sm" dir="ltr">{formatCurrency(row.acquiredCost)}</span>
            ),
        },
        {
            key: "accumulatedDepreciation",
            header: "الإهلاك المتراكم",
            className: "text-center",
            render: (row) => (
                <span className="font-mono text-sm text-red-500" dir="ltr">{formatCurrency(row.accumulatedDepreciation)}</span>
            ),
        },
        {
            key: "netBookValue",
            header: "القيمة الدفترية",
            className: "text-center",
            render: (row) => (
                <span className="font-mono text-sm text-emerald-600 font-semibold" dir="ltr">{formatCurrency(row.netBookValue)}</span>
            ),
        },
    ], []);

    return (
        <PageLayout className="text-start">
            <Breadcrumb items={[
                { label: "الأصول الثابتة", href: "/assets/fixed" },
                { label: "الإهلاك" }
            ]} />

            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex items-center gap-4">
                    <Link to="/assets/fixed" className="p-2 hover:bg-muted rounded-full transition-colors border " aria-label="رجوع">
                        <ArrowRight className="w-5 h-5" />
                    </Link>
                    <div>
                        <Heading level="h1" className="font-black">الإهلاك</Heading>
                        <Text variant="description" className="mt-1">حساب وترحيل إهلاك الأصول الثابتة</Text>
                    </div>
                </div>

                <div className="flex items-center gap-2">
                    <Button onClick={() => setRunDialogOpen(true)} className="gap-2">
                        <PlayCircle className="w-4 h-4" />
                        تشغيل الإهلاك السنوي
                    </Button>
                </div>
            </div>

            {/* فلتر الفترة */}
            <Card>
                <CardContent className="pt-4">
                    <div className="flex items-center gap-4">
                        <div className="flex items-center gap-2">
                            <Calendar className="w-4 h-4 text-muted-foreground" />
                            <span className="text-sm font-medium">فترة الإهلاك:</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <Text variant="caption">من</Text>
                            <DateInput
                                value={fromDate}
                                onChange={(v) => setFromDate(v || "")}
                                placeholder="من"
                                className="w-40"
                            />
                        </div>
                        <div className="flex items-center gap-2">
                            <Text variant="caption">إلى</Text>
                            <DateInput
                                value={toDate}
                                onChange={(v) => setToDate(v || "")}
                                placeholder="إلى"
                                className="w-40"
                            />
                        </div>
                        <span className="text-xs text-muted-foreground">
                            ({filteredAssets.length} أصل مؤهل)
                        </span>
                    </div>
                </CardContent>
            </Card>

            {/* البطاقات الإحصائية */}
            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-2 sm:gap-4">
                <StatCard
                    title="عدد الأصول النشطة"
                    value={filteredAssets.length}
                    icon={<TrendingDown className="w-4 h-4 text-primary" />}
                />
                <StatCard
                    title="إجمالي التكلفة"
                    value={formatCurrency(totalCost)}
                    icon={<Wallet className="w-4 h-4 text-emerald-500" />}
                    variant="emerald"
                />
                <StatCard
                    title="إجمالي الإهلاك المتراكم"
                    value={formatCurrency(totalAccumulatedDepreciation)}
                    icon={<TrendingDown className="w-4 h-4 text-red-500" />}
                    variant="destructive"
                />
                <StatCard
                    title="إجمالي القيمة الدفترية"
                    value={formatCurrency(totalNetBookValue)}
                    icon={<Calendar className="w-4 h-4 text-amber-500" />}
                    variant="amber"
                />
            </div>

            {/* جدول الأصول */}
            <DataTable
                columns={columns}
                data={filteredAssets}
                loading={isLoading}
                emptyMessage="لا توجد أصول نشطة مؤهلة للإهلاك"
                rowKey={(row) => row.assetID}
            />

            <DepreciationRunDialog
                open={runDialogOpen}
                onOpenChange={setRunDialogOpen}
            />
        </PageLayout>
    );
}
