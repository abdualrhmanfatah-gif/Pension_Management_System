import { useQuery } from "@tanstack/react-query";
import { ChevronDown, FileText, Inbox, Layers, Package, Plus } from "lucide-react";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

<<<<<<< HEAD
import { itemApi, type ItemDto } from "@/api/inventory/items";
import { transactionApi, type TransactionDto } from "@/api/inventory/transactions";
import { InventoryBulkTransactionFormDialog } from "@/components/domain/inventory/InventoryBulkTransactionFormDialog";
import { InventoryItemFormDialog } from "@/components/domain/inventory/InventoryItemFormDialog";
import { InventoryTransactionFormDialog } from "@/components/domain/inventory/InventoryTransactionFormDialog";
import { Badge, Button, Card, Heading, Loader, PageLayout, Select, SelectContent, SelectItem, SelectTrigger, SelectValue,Text } from "@/components/ui";
import { INVENTORY_TRANSACTION_TYPE } from "@/constants/inventory";
=======
import { transactionApi, type TransactionDto } from "@/api/inventory/transactions";
import { Badge, Button, Loader, PageLayout, Card, Heading, Text, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useCrudState } from "@/hooks";
import { normalizeApiError } from "@/lib/errorUtils";
import { formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
<<<<<<< HEAD
=======
import { INVENTORY_TRANSACTION_TYPE } from "@/constants/inventory";

import { InventoryBulkTransactionFormDialog } from "@/components/domain/inventory/InventoryBulkTransactionFormDialog";
import { InventoryItemFormDialog } from "@/components/domain/inventory/InventoryItemFormDialog";
import { InventoryTransactionFormDialog } from "@/components/domain/inventory/InventoryTransactionFormDialog";
import { itemApi, type ItemDto } from "@/api/inventory/items";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

export function InventoryTransactionsPage() {
    const crud = useCrudState<TransactionDto>();
    const navigate = useNavigate();
    const canCreate = useHasPermission("Inventory.Transaction.Create");
    const [selectedItemId, setSelectedItemId] = useState<number | null>(null);
    const [items, setItems] = useState<ItemDto[]>([]);
    const [showItemForm, setShowItemForm] = useState(false);
    const [showBulkForm, setShowBulkForm] = useState(false);
    const [showWithdrawalForm, setShowWithdrawalForm] = useState(false);

    const { data: itemsData, refetch: refetchItems } = useQuery({
        queryKey: ["inventory-items"],
        queryFn: () => itemApi.getAll()
    });

    useEffect(() => {
        if (itemsData) setItems(itemsData);
    }, [itemsData]);

    const handleItemSaved = useCallback(() => {
        setShowItemForm(false);
        refetchItems();
    }, [refetchItems]);

    const queryFilter = selectedItemId ? `item-${selectedItemId}` : "all";
    const { data: transactions = [], isLoading: loading, error, refetch } = useQuery({
        queryKey: ["inventory-transactions", queryFilter],
        queryFn: () => selectedItemId ? transactionApi.getByItem(selectedItemId) : transactionApi.getAll(),
    });

    useEffect(() => {
        if (error) {
            toast.error("فشل تحميل الحركات", {
                description: normalizeApiError(error).message
            });
        }
    }, [error]);

    const handleSaved = () => {
        crud.closeForm();
        refetch();
    };

    const handleWithdrawalOrderIssued = (result: TransactionDto[]) => {
        const orderNumber = result[0]?.withdrawalOrderNumber;
        if (orderNumber) {
            navigate(`/inventory/withdrawal-orders/${encodeURIComponent(orderNumber)}/print`);
        }
    };

    const [expandedBatches, setExpandedBatches] = useState<Set<string>>(new Set());

    const groupedTransactions = useMemo(() => {
        const groups: { batchID: string; transactions: TransactionDto[] }[] = [];
        const singles: TransactionDto[] = [];
        const batchMap = new Map<string, TransactionDto[]>();

        for (const t of transactions) {
            if (t.batchID) {
                const arr = batchMap.get(t.batchID) ?? [];
                arr.push(t);
                batchMap.set(t.batchID, arr);
            } else {
                singles.push(t);
            }
        }

        for (const [batchID, txns] of batchMap) {
            groups.push({ batchID, transactions: txns });
        }

        return { groups, singles };
    }, [transactions]);

    const toggleBatch = (batchID: string) => {
        setExpandedBatches(prev => {
            const next = new Set(prev);
            if (next.has(batchID)) next.delete(batchID);
            else next.add(batchID);
            return next;
        });
    };

    const renderRow = (row: TransactionDto) => (
        <div key={row.inventoryTransactionID}
            role="button"
            tabIndex={0}
<<<<<<< HEAD
            className="flex items-center gap-4 p-3 border-b border-border last:border-b-0 hover:bg-muted/30 cursor-pointer transition-colors focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-inset"
=======
            className="flex items-center gap-4 p-3 border-b border-border/40 last:border-b-0 hover:bg-muted/30 cursor-pointer transition-colors focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-inset"
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            onClick={() => navigate(`/inventory/transactions/${row.inventoryTransactionID}`)}
            onKeyDown={(e: React.KeyboardEvent) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); navigate(`/inventory/transactions/${row.inventoryTransactionID}`); } }}>
            <div className="flex-1 grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-11 gap-2 items-center text-sm">
                <div className="col-span-2">
                    <Badge variant={(INVENTORY_TRANSACTION_TYPE[row.transactionType] ?? INVENTORY_TRANSACTION_TYPE[4]).variant as any}>
                        {(INVENTORY_TRANSACTION_TYPE[row.transactionType] ?? INVENTORY_TRANSACTION_TYPE[4]).label}
                    </Badge>
                </div>
                <div className="col-span-2 flex items-center gap-2">
                    <Package className="w-4 h-4 text-muted-foreground shrink-0" />
                    <div>
                        <div className="font-medium truncate">{row.itemName}</div>
                        <div className="text-xs text-muted-foreground font-mono">{row.itemCode}</div>
                    </div>
                </div>
                <div className="col-span-1 font-mono font-medium">{row.quantity}</div>
                <div className="col-span-1 font-mono text-muted-foreground">{row.previousQuantity}</div>
                <div className="col-span-1 font-mono font-medium">{row.newQuantity}</div>
                <div className="col-span-1 font-mono text-xs">{row.unitCost != null ? `${row.unitCost.toFixed(2)}` : "-"}</div>
                <div className="col-span-1 font-mono text-xs">{row.currency || "-"}</div>
                <div className="col-span-1 font-mono text-xs">{row.withdrawalOrderNumber || "-"}</div>
                <div className="col-span-1 text-xs text-muted-foreground">{formatDate(row.transactionDate)}</div>
            </div>
        </div>
    );

    const renderBatchGroup = (group: { batchID: string; transactions: TransactionDto[] }) => {
        const isExpanded = expandedBatches.has(group.batchID);
        const first = group.transactions[0];
        const t = INVENTORY_TRANSACTION_TYPE[first.transactionType] ?? INVENTORY_TRANSACTION_TYPE[4];
        return (
<<<<<<< HEAD
            <div key={group.batchID} className="border-b border-border last:border-b-0">
=======
            <div key={group.batchID} className="border-b border-border/40 last:border-b-0">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                <div
                    role="button"
                    tabIndex={0}
                    className="flex items-center gap-3 p-3 bg-muted/10 cursor-pointer hover:bg-muted/30 transition-colors focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-inset"
                    onClick={() => toggleBatch(group.batchID)}
                    onKeyDown={(e: React.KeyboardEvent) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); toggleBatch(group.batchID); } }}>
                    <ChevronDown className={`w-4 h-4 text-muted-foreground transition-transform ${isExpanded ? "" : "-rotate-90"}`} />
                    <Layers className="w-4 h-4 text-primary" />
                    <Badge variant={t.variant as any}>{t.label}</Badge>
                    <span className="font-medium text-sm">حركة متعددة ({group.transactions.length} أصناف)</span>
                    <span className="text-xs text-muted-foreground">{first.invoiceNumber || ""}</span>
                    <span className="text-xs text-muted-foreground ms-auto">{formatDate(first.transactionDate)}</span>
                </div>
                {isExpanded && (
<<<<<<< HEAD
                    <div className="border-t border-border bg-muted/5">
=======
                    <div className="border-t border-border/40 bg-muted/5">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        {group.transactions.map(renderRow)}
                    </div>
                )}
            </div>
        );
    };

    return (
        <PageLayout>
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="text-start">
                    <Heading level="h3">حركات المخزون</Heading>
                    <Text variant="description" className="mt-1">
                        سجل حركات الأصناف (إدخال، إخراج، نقل، إتلاف، تسوية جرد)
                    </Text>
                </div>
                <div className="flex items-center gap-2 flex-wrap">
                    <Button variant="outline" onClick={() => setShowItemForm(true)} className="gap-1.5">
                        <Plus className="w-4 h-4" /> إضافة صنف
                    </Button>
                    {canCreate && (
                        <Button variant="secondary" onClick={() => setShowBulkForm(true)} className="gap-1.5">
                            <Plus className="w-4 h-4" /> إضافة حركة متعددة
                        </Button>
                    )}
                    {canCreate && (
                        <Button onClick={() => setShowWithdrawalForm(true)} className="gap-1.5 ">
                            <FileText className="w-4 h-4" /> إصدار أمر صرف
                        </Button>
                    )}
                    {canCreate && selectedItemId && (
                        <Button onClick={() => crud.openCreate()} className="gap-1.5 ">
                            <Plus className="w-4 h-4" /> إضافة حركة
                        </Button>
                    )}
                </div>
            </div>

<<<<<<< HEAD
            <Card className="p-4 flex flex-col sm:flex-row sm:items-center gap-4 bg-muted/20 border-border">
=======
            <Card className="p-4 flex flex-col sm:flex-row sm:items-center gap-4 bg-muted/20 border-border/60">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                <div className="flex items-center gap-3">
                    <Text variant="label" className="whitespace-nowrap">تصفية حسب الصنف:</Text>
                    <Select
                        value={selectedItemId?.toString() ?? ""}
                        onValueChange={(val) => setSelectedItemId(val ? Number(val) : null)}
                    >
                        <SelectTrigger className="min-w-[250px]">
                            <SelectValue placeholder="جميع الحركات" />
                        </SelectTrigger>
                        <SelectContent>
                            {items.map((item) => (
                                <SelectItem key={item.inventoryItemID} value={item.inventoryItemID.toString()}>
                                    {item.itemName} ({item.itemCode})
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </div>
            </Card>

            <div className="space-y-4">
                {loading ? (
                    <div className="py-8"><Loader text="جاري تحميل الحركات..." size={40} /></div>
                ) : error ? (
<<<<<<< HEAD
                    <div className="text-center py-12 text-destructive bg-destructive/10 rounded-lg border border-destructive">تعذر تحميل الحركات</div>
                ) : groupedTransactions.groups.length === 0 && groupedTransactions.singles.length === 0 ? (
                    <div className="text-center py-12 text-muted-foreground bg-muted/10 rounded-lg border border-dashed">
                        <Inbox className="w-10 h-10 mx-auto mb-2 opacity-100" />
=======
                    <div className="text-center py-12 text-destructive bg-destructive/10 rounded-lg border border-destructive/20">تعذر تحميل الحركات</div>
                ) : groupedTransactions.groups.length === 0 && groupedTransactions.singles.length === 0 ? (
                    <div className="text-center py-12 text-muted-foreground bg-muted/10 rounded-lg border border-dashed">
                        <Inbox className="w-10 h-10 mx-auto mb-2 opacity-30" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        <p className="font-bold">لا توجد حركات مخزون</p>
                        <p className="text-xs mt-1">لم يتم تسجيل أي حركات مخزون بعد</p>
                    </div>
                ) : (
                    <div className="bg-card border rounded-xl overflow-hidden  flex flex-col">
                        <div className="flex items-center gap-4 p-3 border-b bg-muted/40 text-muted-foreground text-xs font-medium">
                            <div className="flex-1 grid grid-cols-2 sm:grid-cols-4 lg:grid-cols-11 gap-2 items-center">
                                <div className="col-span-2">نوع الحركة</div>
                                <div className="col-span-2">الصنف</div>
                                <div className="col-span-1">الكمية</div>
                                <div className="col-span-1">الرصيد السابق</div>
                                <div className="col-span-1">الرصيد الجديد</div>
                                <div className="col-span-1">تكلفة الوحدة</div>
                                <div className="col-span-1">العملة</div>
                                <div className="col-span-1">أمر الصرف</div>
                                <div className="col-span-1">التاريخ</div>
                            </div>
                        </div>
                        {groupedTransactions.groups.map(renderBatchGroup)}
                        {groupedTransactions.singles.map(renderRow)}
                    </div>
                )}
            </div>

            <InventoryItemFormDialog
                open={showItemForm}
                onOpenChange={setShowItemForm}
                onSaved={handleItemSaved}
                item={null}
            />

            <InventoryBulkTransactionFormDialog
                open={showBulkForm}
                onOpenChange={setShowBulkForm}
                onSaved={() => { refetch(); }}
                items={items}
            />

            <InventoryBulkTransactionFormDialog
                open={showWithdrawalForm}
                onOpenChange={setShowWithdrawalForm}
                onSaved={() => { setShowWithdrawalForm(false); refetch(); }}
                items={items}
                presetType={1}
                onSuccessWithResult={handleWithdrawalOrderIssued}
            />

            {selectedItemId != null && (
                <InventoryTransactionFormDialog
                    open={crud.isFormOpen}
                    onOpenChange={crud.onOpenChange}
                    onSaved={handleSaved}
                    transaction={null}
                    itemId={selectedItemId}
                />
            )}
        </PageLayout>
    );
}
