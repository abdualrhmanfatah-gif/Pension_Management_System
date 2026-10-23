import { useQuery } from "@tanstack/react-query";
import { ArrowLeft, ArrowRight, Calendar, Clock, Coins, FileText, Hash, Network, Package, Printer, Receipt, StickyNote, User } from "lucide-react";
import { useNavigate, useParams } from "react-router-dom";

import { transactionApi } from "@/api/inventory/transactions";
import { Badge, Button, Card, CardContent, CardHeader, CardTitle, ErrorState, Heading, InfoPair, PageLayout, Skeleton } from "@/components/ui";
import { INVENTORY_TRANSACTION_TYPE, TRANSACTION_TYPE_ACCENT_COLORS,TRANSACTION_TYPE_GRADIENTS } from "@/constants/inventory";
import { formatDate } from "@/lib/utils";

function TransactionDetailsSkeleton() {
    return (
        <PageLayout>
            <div className="max-w-4xl mx-auto space-y-6">
                <div className="flex items-center gap-4">
                    <Skeleton className="h-10 w-10 rounded-full" />
                    <div className="space-y-2">
                        <Skeleton className="h-7 w-56" />
                        <Skeleton className="h-4 w-36" />
                    </div>
                </div>
                <Skeleton className="h-48 w-full rounded-2xl" />
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <Skeleton className="h-52 w-full rounded-xl" />
                    <Skeleton className="h-52 w-full rounded-xl" />
                </div>
                <Skeleton className="h-28 w-full rounded-xl" />
            </div>
        </PageLayout>
    );
}

export function InventoryTransactionDetailsPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const transactionId = Number(id);

    const { data: transaction, isLoading, isError } = useQuery({
        queryKey: ["inventory-transaction", transactionId],
        queryFn: () => transactionApi.getById(transactionId),
        enabled: !!transactionId,
    });

    if (isLoading) return <TransactionDetailsSkeleton />;

    if (isError) {
        return (
            <PageLayout>
                <ErrorState message="تعذر تحميل بيانات الحركة" />
                <div className="flex justify-center mt-4">
                    <Button variant="outline" onClick={() => navigate("/inventory/transactions")}>
                        العودة إلى قائمة الحركات
                    </Button>
                </div>
            </PageLayout>
        );
    }

    if (!transaction) {
        return (
            <PageLayout>
                <ErrorState
                    message="الحركة غير موجودة"
                    onRetry={() => navigate("/inventory/transactions")}
                />
            </PageLayout>
        );
    }

    const t = INVENTORY_TRANSACTION_TYPE[transaction.transactionType] ?? INVENTORY_TRANSACTION_TYPE[4];
    const gradient = TRANSACTION_TYPE_GRADIENTS[transaction.transactionType] ?? TRANSACTION_TYPE_GRADIENTS[4];
    const accentColor = TRANSACTION_TYPE_ACCENT_COLORS[transaction.transactionType] ?? TRANSACTION_TYPE_ACCENT_COLORS[4];
    const totalCost = transaction.unitCost != null ? (transaction.unitCost * transaction.quantity).toFixed(2) : null;

    return (
        <PageLayout>
            <div className="max-w-4xl mx-auto space-y-6 animate-in fade-in duration-500 text-start" dir="rtl">

                {/* ─── Back + Breadcrumb ─── */}
                <div className="flex items-center gap-3">
                    <Button variant="ghost" size="icon" onClick={() => navigate(-1)} className="rounded-full bg-muted/40 hover:bg-muted/70 size-10" aria-label="رجوع">
                        <ArrowRight className="w-5 h-5" />
                    </Button>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                        <Button variant="ghost" size="sm" onClick={() => navigate("/inventory/transactions")} className="font-medium min-h-[44px]">حركات المخزون</Button>
                        <ArrowLeft className="w-3 h-3" />
                        <span className="text-foreground font-semibold">حركة #{transaction.inventoryTransactionID}</span>
                    </div>
                </div>

                {/* ─── Color Banner ─── */}
                <div className={`relative overflow-hidden rounded-2xl bg-gradient-to-br ${gradient} p-6 sm:p-8 border`}>
                    <div className={`absolute top-0 end-0 w-40 h-40 rounded-full blur-3xl opacity-100 ${accentColor}`} />
                    <div className="relative flex flex-col sm:flex-row sm:items-center justify-between gap-6">
                        <div className="flex items-start gap-4">
                            <div className={`p-3 rounded-xl ${accentColor} hidden sm:block`}>
                                <Package className="w-6 h-6" />
                            </div>
                            <div className="space-y-2">
                                <div className="flex items-center gap-2 flex-wrap">
                                    <Badge variant={t.variant} className="text-sm px-3 py-1">{t.label}</Badge>
                                    <span className="text-xs font-mono text-muted-foreground">ID: {transaction.inventoryTransactionID}</span>
                                </div>
                                <Heading level="h1" className="text-lg md:text-xl lg:text-2xl font-black tracking-tight">{transaction.itemName}</Heading>
                                <div className="flex items-center gap-3 text-sm text-muted-foreground font-medium">
                                    <span className="font-mono">{transaction.itemCode}</span>
                                    {transaction.batchID && <span className="font-mono">دفعة: {transaction.batchID.slice(0, 8)}...</span>}
                                </div>
                            </div>
                        </div>
                        <div className="flex items-center gap-2 shrink-0">
                            {transaction.withdrawalOrderNumber && (
                                <Button onClick={() => navigate(`/inventory/withdrawal-orders/${encodeURIComponent(transaction.withdrawalOrderNumber!)}/print`)}
                                    className="gap-2 rounded-xl h-11 px-5">
                                    <Printer className="w-4 h-4" /> أمر الصرف
                                </Button>
                            )}
                        </div>
                    </div>
                </div>

                {/* ─── Quantity Stepper ─── */}
                <Card className="overflow-hidden">
                    <CardHeader className="bg-muted/20 border-b border-border pb-3">
                        <CardTitle className="flex items-center gap-2 text-sm font-bold text-muted-foreground">
                            <Package className="w-4 h-4" /> أثر الحركة على الكمية
                        </CardTitle>
                    </CardHeader>
                    <CardContent className="p-6">
                        <div className="flex items-center justify-center gap-3 sm:gap-6">
                            <div className="flex flex-col items-center gap-1.5">
                                <span className="text-xs font-bold text-muted-foreground uppercase tracking-wider">قبل</span>
                                <div className="w-20 h-16 sm:w-28 sm:h-20 rounded-xl bg-muted/40 border-2 border-border flex items-center justify-center">
                                    <span className="text-xl sm:text-3xl font-black text-muted-foreground">{transaction.previousQuantity}</span>
                                </div>
                            </div>
                            <div className="flex flex-col items-center gap-1">
                                <ArrowLeft className="w-5 h-5 sm:w-6 sm:h-6 text-muted-foreground" />
                                <span className="text-xs text-muted-foreground">—</span>
                            </div>
                            <div className="flex flex-col items-center gap-1.5">
                                <span className="text-xs font-bold text-muted-foreground uppercase tracking-wider">التغيير</span>
                                <div className={`w-20 h-16 sm:w-28 sm:h-20 rounded-xl border-2 flex items-center justify-center ${
                                    transaction.transactionType === 0
                                        ? "bg-emerald-500/5 border-emerald-300"
                                        : transaction.transactionType === 1
                                        ? "bg-red-500/5 border-red-300"
                                        : "bg-blue-500/5 border-blue-300"
                                }`}>
                                    <span className={`text-xl sm:text-3xl font-black ${
                                        transaction.transactionType === 0 ? "text-emerald-600" :
                                        transaction.transactionType === 1 ? "text-red-600" :
                                        "text-blue-600"
                                    }`}>
                                        {transaction.transactionType === 0 ? "+" : ""}{transaction.quantity}
                                    </span>
                                </div>
                            </div>
                            <div className="flex flex-col items-center gap-1">
                                <ArrowLeft className="w-5 h-5 sm:w-6 sm:h-6 text-muted-foreground" />
                                <span className="text-xs text-muted-foreground">—</span>
                            </div>
                            <div className="flex flex-col items-center gap-1.5">
                                <span className="text-xs font-bold text-muted-foreground uppercase tracking-wider">بعد</span>
                                <div className="w-20 h-16 sm:w-28 sm:h-20 rounded-xl bg-primary/5 border-2 border-primary flex items-center justify-center">
                                    <span className="text-xl sm:text-3xl font-black text-primary">{transaction.newQuantity}</span>
                                </div>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                {/* ─── Two-Column Cards ─── */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

                    {/* Financial Details */}
                    <Card>
                        <CardHeader className="border-b border-border pb-3">
                            <CardTitle className="flex items-center gap-2 text-sm font-bold">
                                <Receipt className="w-4 h-4 text-primary" /> المالية والمستندات
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="p-5 space-y-4">
                            <InfoPair label="تاريخ الحركة" value={formatDate(transaction.transactionDate)} icon={<Calendar className="w-3.5 h-3.5" />} layout="row" />
                            {transaction.unitCost != null && (
                                <InfoPair label="تكلفة الوحدة" value={`${transaction.unitCost.toFixed(2)} ${transaction.currency}`} icon={<Coins className="w-3.5 h-3.5" />} layout="row" />
                            )}
                            {totalCost && (
                                <InfoPair label="الإجمالي" value={`${totalCost} ${transaction.currency}`} icon={<Coins className="w-3.5 h-3.5" />} layout="row" />
                            )}
                            <InfoPair label="العملة" value={transaction.currency} icon={<Network className="w-3.5 h-3.5" />} layout="row" />
                            {transaction.invoiceNumber && (
                                <InfoPair label="رقم الفاتورة" value={transaction.invoiceNumber} icon={<Hash className="w-3.5 h-3.5" />} layout="row" />
                            )}
                            <InfoPair label="الرقم المرجعي" value={transaction.referenceNumber || "—"} icon={<FileText className="w-3.5 h-3.5" />} layout="row" />
                            {transaction.withdrawalOrderNumber && (
                                <InfoPair label="رقم أمر الصرف" value={transaction.withdrawalOrderNumber} icon={<FileText className="w-3.5 h-3.5" />} layout="row" />
                            )}
                        </CardContent>
                    </Card>

                    {/* Transaction Metadata */}
                    <Card>
                        <CardHeader className="border-b border-border pb-3">
                            <CardTitle className="flex items-center gap-2 text-sm font-bold">
                                <User className="w-4 h-4 text-primary" /> معلومات الحركة
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="p-5 space-y-4">
                            <InfoPair label="تم بواسطة" value={transaction.performedByName || "—"} icon={<User className="w-3.5 h-3.5" />} layout="row" />
                            <InfoPair label="تاريخ التسجيل" value={formatDate(transaction.createdAt)} icon={<Clock className="w-3.5 h-3.5" />} layout="row" />
                            <InfoPair label="حالة الرصيد" value={transaction.newQuantity > 0 ? "متوفر" : "نفذ"} icon={<Package className="w-3.5 h-3.5" />} layout="row" />
                        </CardContent>
                    </Card>

                </div>

                {/* ─── Reason Card ─── */}
                {transaction.reason && (
                    <Card>
                        <CardHeader className="border-b border-border pb-3">
                            <CardTitle className="flex items-center gap-2 text-sm font-bold">
                                <StickyNote className="w-4 h-4 text-primary" /> سبب الحركة
                            </CardTitle>
                        </CardHeader>
                        <CardContent className="p-5">
                            <div className="p-4 rounded-xl bg-muted/20 border border-border text-sm leading-relaxed">
                                {transaction.reason}
                            </div>
                        </CardContent>
                    </Card>
                )}

            </div>
        </PageLayout>
    );
}
