<<<<<<< HEAD
import { useQuery } from "@tanstack/react-query";
import {
AlertTriangle,   ArrowUpDown, Building2, Calculator,
ClipboardList, Edit,   ListTree,   Package, Plus,
Search, Trash2,
} from "lucide-react";
import { useMemo, useState } from "react";
import { Link } from "react-router-dom";

import { fixedAssetApi, type FixedAssetDto } from "@/api/fixedAssets";
import { itemApi, type ItemDto } from "@/api/inventory/items";
import { stocktakeApi } from "@/api/inventory/stocktake";
import { FixedAssetFormDialog } from "@/components/domain/assets/FixedAssetFormDialog";
import { InventoryItemFormDialog } from "@/components/domain/inventory/InventoryItemFormDialog";
=======
import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  Package, Building2, AlertTriangle, ClipboardList, Edit, Trash2,
  ArrowUpDown, Search, Plus,
  ListTree, Calculator,
} from "lucide-react";
import { Link } from "react-router-dom";

import { itemApi, type ItemDto } from "@/api/inventory/items";
import { fixedAssetApi, type FixedAssetDto } from "@/api/fixedAssets";
import { stocktakeApi } from "@/api/inventory/stocktake";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
  Badge, Button, Card, CardContent, Heading, StatCard,
} from "@/components/ui";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
<<<<<<< HEAD
import { type CustomColumnDef,DataTable } from "@/components/ui/DataTable";
import { Input } from "@/components/ui/Input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/Tabs";
import { ASSET_STATUS_CONFIG } from "@/constants/assets";
import { INVENTORY_ITEM_STATUS } from "@/constants/inventory";
import { useApiMutation } from "@/hooks";
import { formatCurrency,formatDate } from "@/lib/utils";
=======
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/Tabs";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select";
import { Input } from "@/components/ui/Input";
import { DataTable, type CustomColumnDef } from "@/components/ui/DataTable";
import { formatDate, formatCurrency } from "@/lib/utils";
import { useApiMutation } from "@/hooks";
import { INVENTORY_ITEM_STATUS } from "@/constants/inventory";
import { ASSET_STATUS_CONFIG } from "@/constants/assets";
import { InventoryItemFormDialog } from "@/components/domain/inventory/InventoryItemFormDialog";
import { FixedAssetFormDialog } from "@/components/domain/assets/FixedAssetFormDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

// ═══════════════════════════════════════
// تبويب الأصناف (with CRUD)
// ═══════════════════════════════════════

function ItemsTab() {
  const [search, setSearch] = useState("");
  const [formOpen, setFormOpen] = useState(false);
  const [editItem, setEditItem] = useState<ItemDto | null>(null);
  const [deleteItem, setDeleteItem] = useState<ItemDto | null>(null);

  const { data: items = [], isLoading, refetch } = useQuery({
    queryKey: ["inventory-items"],
    queryFn: () => itemApi.getAll(),
  });

  const deleteMutation = useApiMutation(
    (id: number) => itemApi.delete(id),
    {
      successMessage: "تم حذف الصنف بنجاح",
      invalidateKeys: [["inventory-items"]],
      onSuccessCallback: () => setDeleteItem(null),
    }
  );

  const filteredItems = items.filter(item =>
    item.itemName.includes(search) || item.itemCode.includes(search)
  );

  const columns: CustomColumnDef<ItemDto>[] = useMemo(() => [
    {
      key: "itemCode",
      header: "الكود",
      className: "text-center",
      render: (row) => (
        <div className="font-semibold text-foreground font-mono text-center" dir="ltr">{row.itemCode}</div>
      ),
    },
    {
      key: "itemName",
      header: "اسم الصنف",
      render: (row) => (
        <span className="text-sm font-medium text-foreground">{row.itemName}</span>
      ),
    },
    {
      key: "unit",
      header: "الوحدة",
      className: "text-center",
      render: (row) => <span className="text-sm">{row.unit}</span>,
    },
    {
      key: "quantity",
      header: "الكمية",
      className: "text-center",
      render: (row) => (
        <span className={row.quantity <= row.minimumQuantity ? "text-amber-600 font-bold" : ""}>
          {row.quantity}
        </span>
      ),
    },
    {
      key: "status",
      header: "الحالة",
      className: "text-center",
      render: (row) => (
        <Badge variant={INVENTORY_ITEM_STATUS[row.status]?.variant || "secondary"} className="text-xs">
          {INVENTORY_ITEM_STATUS[row.status]?.label || "غير محدد"}
        </Badge>
      ),
    },
    {
      key: "createdAt",
      header: "التاريخ",
      className: "text-center",
      render: (row) => (
        <span className="text-sm text-muted-foreground">{formatDate(row.createdAt)}</span>
      ),
    },
    {
      key: "actions",
      header: "إجراءات",
      className: "text-center",
      render: (row) => (
        <div className="flex items-center justify-center gap-1">
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            onClick={() => { setEditItem(row); setFormOpen(true); }}
          >
            <Edit className="w-4 h-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 text-destructive"
            onClick={() => setDeleteItem(row)}
          >
            <Trash2 className="w-4 h-4" />
          </Button>
        </div>
      ),
    },
  ], []);

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <Input
            placeholder="بحث بالاسم أو الكود..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pr-9"
          />
        </div>
        <Button onClick={() => { setEditItem(null); setFormOpen(true); }}>
          <Plus className="w-4 h-4 ml-2" />
          إضافة صنف
        </Button>
      </div>

      <DataTable
        columns={columns}
        data={filteredItems}
        loading={isLoading}
        emptyMessage="لا توجد أصناف"
        rowKey={(row) => row.inventoryItemID}
      />

      <InventoryItemFormDialog
        open={formOpen}
        onOpenChange={setFormOpen}
        onSaved={() => refetch()}
        item={editItem}
      />

      <ConfirmDialog
        open={!!deleteItem}
        onOpenChange={() => setDeleteItem(null)}
        title="حذف الصنف"
        description={`هل أنت متأكد من حذف "${deleteItem?.itemName}"؟ لا يمكن التراجع عن هذا الإجراء.`}
        confirmLabel="حذف"
        variant="danger"
        onConfirm={() => { if (deleteItem) deleteMutation.mutate(deleteItem.inventoryItemID); }}
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}

// ═══════════════════════════════════════
// تبويب الأصول الثابتة
// ═══════════════════════════════════════

function FixedAssetsTab() {
  const [search, setSearch] = useState("");
  const [formOpen, setFormOpen] = useState(false);
  const [editAsset, setEditAsset] = useState<FixedAssetDto | null>(null);
  const [deleteAsset, setDeleteAsset] = useState<FixedAssetDto | null>(null);

  const { data: assets = [], isLoading, refetch: refetchAssets } = useQuery({
    queryKey: ["fixed-assets"],
    queryFn: () => fixedAssetApi.getAll(),
  });

  const deleteMutation = useApiMutation(
    (id: number) => fixedAssetApi.delete(id),
    {
      successMessage: "تم حذف الأصل بنجاح",
      invalidateKeys: [["fixed-assets"]],
      onSuccessCallback: () => setDeleteAsset(null),
    }
  );

  const filteredAssets = assets.filter(a =>
    a.assetName.includes(search) || a.assetNumber.includes(search)
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
      key: "assetTypeName",
      header: "النوع",
      render: (row) => (
        <Badge variant="secondary" className="text-xs">{row.assetTypeName || "—"}</Badge>
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
      key: "quantity",
      header: "الكمية",
      className: "text-center",
      render: (row) => <span className="font-semibold text-sm">{row.quantity}</span>,
    },
    {
      key: "netBookValue",
      header: "القيمة الدفترية",
      className: "text-center",
      render: (row) => (
        <span className="font-mono text-sm" dir="ltr">{formatCurrency(row.netBookValue)}</span>
      ),
    },
    {
      key: "status",
      header: "الحالة",
      className: "text-center",
      render: (row) => (
        <Badge variant={ASSET_STATUS_CONFIG[row.status]?.variant || "secondary"} className="text-xs">
          {ASSET_STATUS_CONFIG[row.status]?.label || "غير محدد"}
        </Badge>
      ),
    },
    {
      key: "location",
      header: "الموقع",
      render: (row) => (
        <span className="text-sm text-muted-foreground">{row.location || "—"}</span>
      ),
    },
    {
      key: "actions",
      header: "إجراءات",
      className: "text-center",
      render: (row) => (
        <div className="flex items-center justify-center gap-1">
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            onClick={() => { setEditAsset(row); setFormOpen(true); }}
          >
            <Edit className="w-4 h-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 text-destructive"
            onClick={() => setDeleteAsset(row)}
          >
            <Trash2 className="w-4 h-4" />
          </Button>
        </div>
      ),
    },
  ], []);

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <Input
            placeholder="بحث بالاسم أو الرقم..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pr-9"
          />
        </div>
        <Button onClick={() => { setEditAsset(null); setFormOpen(true); }}>
          <Plus className="w-4 h-4 ml-2" />
          إضافة أصل
        </Button>
      </div>

      <DataTable
        columns={columns}
        data={filteredAssets}
        loading={isLoading}
        emptyMessage="لا توجد أصول ثابتة"
        rowKey={(row) => row.assetID}
      />

      <FixedAssetFormDialog
        open={formOpen}
        onOpenChange={setFormOpen}
        onSuccess={() => refetchAssets()}
        initialData={editAsset ?? undefined}
        mode={editAsset ? "edit" : "create"}
      />

      <ConfirmDialog
        open={!!deleteAsset}
        onOpenChange={() => setDeleteAsset(null)}
        title="حذف الأصل"
        description={`هل أنت متأكد من حذف "${deleteAsset?.assetName}"؟ لا يمكن التراجع عن هذا الإجراء.`}
        confirmLabel="حذف"
        variant="danger"
        onConfirm={() => { if (deleteAsset) deleteMutation.mutate(deleteAsset.assetID); }}
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}

// ═══════════════════════════════════════
// تبويب الجرد
// ═══════════════════════════════════════

function StocktakeTab() {
  const [stocktakeType, setStocktakeType] = useState<"asset" | "simple">("asset");
  const [selectedFiscalYearId, setSelectedFiscalYearId] = useState<string>("current");

  const { data: fiscalYears } = useQuery({
    queryKey: ["fiscal-years"],
    queryFn: async () => {
      const res = await stocktakeApi.getFiscalYears();
      return res.data;
    },
  });

  const { data: stocktake, isLoading } = useQuery({
    queryKey: ["stocktake", stocktakeType, selectedFiscalYearId],
    queryFn: async () => {
      if (stocktakeType === "asset") {
        if (!selectedFiscalYearId || selectedFiscalYearId === "current") {
          const res = await stocktakeApi.getAssetStocktake();
          return res.data;
        }
        const res = await stocktakeApi.getAssetStocktakeByFiscalYear(Number(selectedFiscalYearId));
        return res.data;
      } else {
        if (!selectedFiscalYearId || selectedFiscalYearId === "current") {
          const res = await stocktakeApi.getSimpleStocktake();
          return res.data;
        }
        const res = await stocktakeApi.getSimpleStocktakeByFiscalYear(Number(selectedFiscalYearId));
        return res.data;
      }
    },
    enabled: true,
  });

  const assetColumns: CustomColumnDef<any>[] = useMemo(() => [
    {
      key: "assetNumber",
      header: "رقم الأصل",
      className: "text-center",
      render: (row) => (
        <span className="font-mono text-xs bg-muted/60 px-2 py-1 rounded">{row.assetNumber}</span>
      ),
    },
    {
      key: "assetName",
      header: "اسم الأصل",
      render: (row) => <span className="font-medium text-sm">{row.assetName}</span>,
    },
    {
      key: "location",
      header: "الموقع",
      className: "text-center",
      render: (row) => <span className="text-sm text-muted-foreground">{row.location || "-"}</span>,
    },
    {
      key: "netBookValue",
      header: "القيمة الدفترية",
      className: "text-center",
      render: (row) => (
        <span className="font-mono text-sm font-semibold" dir="ltr">{row.netBookValue?.toLocaleString()}</span>
      ),
    },
    {
      key: "systemQuantity",
      header: "بالسجلات",
      className: "text-center",
      render: (row) => <span className="font-semibold text-sm">{row.systemQuantity}</span>,
    },
    {
      key: "recordedQuantity",
      header: "الكمية الفعلية",
      className: "text-center",
      render: (row) => <span className="font-bold text-sm">{row.recordedQuantity}</span>,
    },
    {
      key: "isPresent",
      header: "موجود",
      className: "text-center",
      render: (row) => (
        <Badge variant={row.isPresent ? "success" : "destructive"} className="text-xs">
          {row.isPresent ? "موجود" : "مفقود"}
        </Badge>
      ),
    },
    {
      key: "actualCondition",
      header: "الحالة",
      className: "text-center",
      render: (row) => (
        <Badge variant={row.actualCondition === "Good" ? "success" : row.actualCondition === "Missing" ? "destructive" : "secondary"} className="text-xs">
          {row.actualCondition === "Good" ? "جيدة" : row.actualCondition === "Fair" ? "مقبولة" : row.actualCondition === "Poor" ? "ضعيفة" : row.actualCondition === "Missing" ? "مفقودة" : "غير محددة"}
        </Badge>
      ),
    },
    {
      key: "notes",
      header: "ملاحظات",
      className: "text-center",
      render: (row) => <span className="text-xs text-muted-foreground">{row.notes || "-"}</span>,
    },
  ], []);

  const simpleColumns: CustomColumnDef<any>[] = useMemo(() => [
    {
      key: "itemCode",
      header: "كود الصنف",
      className: "text-center",
      render: (row) => (
        <span className="font-mono text-xs bg-muted/60 px-2 py-1 rounded">{row.itemCode}</span>
      ),
    },
    {
      key: "itemName",
      header: "اسم الصنف",
      render: (row) => <span className="font-medium text-sm">{row.itemName}</span>,
    },
    {
      key: "systemQuantity",
      header: "في السجلات",
      className: "text-center",
      render: (row) => <span className="font-semibold text-sm">{row.systemQuantity}</span>,
    },
    {
      key: "physicalQuantity",
      header: "الكمية الفعلية",
      className: "text-center",
      render: (row) => <span className="font-bold text-sm">{row.physicalQuantity}</span>,
    },
    {
      key: "variance",
      header: "الفرق",
      className: "text-center",
      render: (row) => (
        <span className={`font-semibold text-sm ${row.variance > 0 ? "text-green-600" : row.variance < 0 ? "text-red-600" : "text-muted-foreground"}`}>
          {row.variance > 0 ? `+${row.variance}` : row.variance}
        </span>
      ),
    },
  ], []);

  return (
    <div className="space-y-4">
      {/* رأس القسم */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Select value={stocktakeType} onValueChange={(v) => setStocktakeType(v as "asset" | "simple")}>
            <SelectTrigger className="w-40">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="asset">جرد الأصول</SelectItem>
              <SelectItem value="simple">جرد المخزون</SelectItem>
            </SelectContent>
          </Select>

          <Select value={selectedFiscalYearId} onValueChange={setSelectedFiscalYearId}>
            <SelectTrigger className="w-56">
              <SelectValue placeholder="السنة المالية" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="current">السنة المالية الحالية</SelectItem>
              {fiscalYears?.map((year) => (
                <SelectItem key={year.fiscalYearID} value={String(year.fiscalYearID)}>
                  {year.yearName} {year.isClosed ? "(مغلقة)" : ""}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {stocktake && (
          <Badge variant={stocktake.status === "Completed" ? "success" : stocktake.status === "Adjusted" ? "outline" : "secondary"}>
            {stocktake.status === "InProgress" ? "قيد التنفيذ" : stocktake.status === "Completed" ? "مكتمل" : "تمت التسوية"}
          </Badge>
        )}
      </div>

      {/* ملخص */}
      {stocktake && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {stocktakeType === "asset" ? (
            <>
              <Card>
                <CardContent className="pt-4">
                  <p className="text-sm text-muted-foreground">إجمالي الأصول</p>
                  <p className="text-base md:text-lg lg:text-xl font-bold">{(stocktake as any).summary?.totalAssets || 0}</p>
                </CardContent>
              </Card>
              <Card>
                <CardContent className="pt-4">
                  <p className="text-sm text-muted-foreground">موجودة</p>
                  <p className="text-base md:text-lg lg:text-xl font-bold text-green-600">{(stocktake as any).summary?.presentAssets || 0}</p>
                </CardContent>
              </Card>
              <Card>
                <CardContent className="pt-4">
                  <p className="text-sm text-muted-foreground">مفقودة</p>
                  <p className="text-base md:text-lg lg:text-xl font-bold text-red-600">{(stocktake as any).summary?.missingAssets || 0}</p>
                </CardContent>
              </Card>
              <Card>
                <CardContent className="pt-4">
                  <p className="text-sm text-muted-foreground">قيمة الدفتر</p>
                  <p className="text-base md:text-lg lg:text-xl font-bold">{(stocktake as any).summary?.totalBookValue?.toLocaleString() || 0}</p>
                </CardContent>
              </Card>
            </>
          ) : (
            <>
              <Card>
                <CardContent className="pt-4">
                  <p className="text-sm text-muted-foreground">إجمالي الأصناف</p>
                  <p className="text-base md:text-lg lg:text-xl font-bold">{(stocktake as any).totalItems || 0}</p>
                </CardContent>
              </Card>
              <Card>
                <CardContent className="pt-4">
                  <p className="text-sm text-muted-foreground">أصناف بها فروق</p>
                  <p className="text-base md:text-lg lg:text-xl font-bold text-amber-600">{(stocktake as any).itemsWithVariance || 0}</p>
                </CardContent>
              </Card>
              <Card>
                <CardContent className="pt-4">
                  <p className="text-sm text-muted-foreground">قيمة الفروق</p>
                  <p className="text-base md:text-lg lg:text-xl font-bold text-red-600">{(stocktake as any).totalVarianceValue?.toLocaleString() || 0}</p>
                </CardContent>
              </Card>
              <Card>
                <CardContent className="pt-4">
                  <p className="text-sm text-muted-foreground">القيد المحاسبي</p>
                  <p className="text-base md:text-lg lg:text-xl font-bold">{(stocktake as any).journalEntryNumber || "-"}</p>
                </CardContent>
              </Card>
            </>
          )}
        </div>
      )}

      {/* جدول */}
      {stocktake && stocktake.lines?.length > 0 && (
        <DataTable
          columns={stocktakeType === "asset" ? assetColumns : simpleColumns}
          data={stocktake.lines}
          loading={isLoading}
          emptyMessage="لا يوجد جرد لهذه السنة المالية"
          rowKey={(row) => row.stocktakeLineID || row.lineID}
        />
      )}

      {!isLoading && (!stocktake || !stocktake.lines || stocktake.lines.length === 0) && (
        <Card>
          <CardContent className="py-12 text-center">
            <Package className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
            <p className="text-muted-foreground">لا يوجد جرد لهذه السنة المالية</p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

// ═══════════════════════════════════════
// الصفحة الرئيسية
// ═══════════════════════════════════════

export function InventoryDashboardPage() {
  const [activeTab, setActiveTab] = useState("items");

  const { data: items = [] } = useQuery({
    queryKey: ["inventory-items"],
    queryFn: () => itemApi.getAll(),
  });

  const { data: assets = [] } = useQuery({
    queryKey: ["fixed-assets"],
    queryFn: () => fixedAssetApi.getAll(),
  });

  const lowStockCount = items.filter(i => i.quantity <= i.minimumQuantity && i.minimumQuantity > 0).length;

  return (
    <div className="space-y-6" dir="rtl">
      {/* العنوان */}
      <div>
        <Heading level="h1" className="text-lg md:text-xl lg:text-2xl font-bold">إدارة المخزون والأصول</Heading>
        <p className="text-muted-foreground">إدارة أصول ومنقولات المؤسسة والجرد</p>
      </div>

      {/* البطاقات الإحصائية */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <StatCard title="إجمالي الأصناف" value={items.length} icon={<Package className="w-5 h-5" />} variant="blue" />
        <StatCard title="تحت الحد الأدنى" value={lowStockCount} icon={<AlertTriangle className="w-5 h-5" />} variant={lowStockCount > 0 ? "warning" : "default"} />
        <StatCard title="الأصول الثابتة" value={assets.length} icon={<Building2 className="w-5 h-5" />} variant="emerald" />
        <StatCard title="العهدة النشطة" value="-" icon={<ClipboardList className="w-5 h-5" />} variant="blue" />
      </div>

      {/* روابط سريعة */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
        <Link to="/inventory/items" className="flex items-center gap-3 rounded-xl border bg-background hover:bg-muted/50 px-4 py-3 transition-colors">
          <Package className="w-5 h-5 text-primary shrink-0" />
          <div>
            <div className="font-medium text-sm">إدارة الأصناف</div>
            <div className="text-xs text-muted-foreground">إضافة/تعديل/حذف</div>
          </div>
        </Link>
        <Link to="/assets/fixed" className="flex items-center gap-3 rounded-xl border bg-background hover:bg-muted/50 px-4 py-3 transition-colors">
          <Building2 className="w-5 h-5 text-primary shrink-0" />
          <div>
            <div className="font-medium text-sm">إدارة الأصول الثابتة</div>
            <div className="text-xs text-muted-foreground">قائمة الأصول</div>
          </div>
        </Link>
        <Link to="/assets/categories" className="flex items-center gap-3 rounded-xl border bg-background hover:bg-muted/50 px-4 py-3 transition-colors">
          <ListTree className="w-5 h-5 text-primary shrink-0" />
          <div>
            <div className="font-medium text-sm">تصنيفات الأصول</div>
            <div className="text-xs text-muted-foreground">إدارة التصنيفات</div>
          </div>
        </Link>
        <Link to="/assets/depreciation-run" className="flex items-center gap-3 rounded-xl border bg-background hover:bg-muted/50 px-4 py-3 transition-colors">
          <Calculator className="w-5 h-5 text-primary shrink-0" />
          <div>
            <div className="font-medium text-sm">الإهلاك</div>
            <div className="text-xs text-muted-foreground">حساب وترحيل الإهلاك</div>
          </div>
        </Link>
      </div>

      {/* التبويبات */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList className="w-full justify-start">
          <TabsTrigger value="items">الأصناف</TabsTrigger>
          <TabsTrigger value="assets">الأصول الثابتة</TabsTrigger>
          <TabsTrigger value="stocktake">الجرد</TabsTrigger>
          <TabsTrigger value="custody">العهدة</TabsTrigger>
          <TabsTrigger value="transactions">الحركات</TabsTrigger>
        </TabsList>

        <TabsContent value="items">
          <ItemsTab />
        </TabsContent>

        <TabsContent value="assets">
          <FixedAssetsTab />
        </TabsContent>

        <TabsContent value="stocktake">
          <StocktakeTab />
        </TabsContent>

        <TabsContent value="custody">
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center gap-4 mb-4">
                <div className="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center">
                  <ClipboardList className="w-6 h-6 text-primary" />
                </div>
                <div>
                  <Heading level="h3" className="font-medium">إدارة العهد الشخصية</Heading>
                  <p className="text-sm text-muted-foreground">تتبع العهد الممنوحة للموظفين</p>
                </div>
              </div>
              <div className="grid grid-cols-2 sm:grid-cols-2 gap-2 sm:gap-3">
                <Link to="/inventory/custody" className="inline-flex items-center gap-2 rounded-xl border bg-background hover:bg-muted/50 px-4 py-2.5 transition-colors text-sm font-medium">
                  <ClipboardList className="w-4 h-4" />
                  عرض العهد النشطة
                </Link>
                <Link to="/inventory/custody?action=new" className="inline-flex items-center gap-2 rounded-xl border bg-background hover:bg-muted/50 px-4 py-2.5 transition-colors text-sm font-medium">
                  <Plus className="w-4 h-4" />
                  إضافة عهد جديدة
                </Link>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="transactions">
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center gap-4 mb-4">
                <div className="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center">
                  <ArrowUpDown className="w-6 h-6 text-primary" />
                </div>
                <div>
                  <Heading level="h3" className="font-medium">سجل الحركات</Heading>
                  <p className="text-sm text-muted-foreground">حركات إدخال وإخراج المخزون</p>
                </div>
              </div>
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-2 sm:gap-3">
                <Link to="/inventory/transactions" className="inline-flex items-center gap-2 rounded-xl border bg-background hover:bg-muted/50 px-4 py-2.5 transition-colors text-sm font-medium">
                  <ArrowUpDown className="w-4 h-4" />
                  كل الحركات
                </Link>
                <Link to="/inventory/transactions?type=in" className="inline-flex items-center gap-2 rounded-xl border bg-background hover:bg-muted/50 px-4 py-2.5 transition-colors text-sm font-medium">
                  <Plus className="w-4 h-4" />
                  إدخال مخزون
                </Link>
                <Link to="/inventory/transactions?type=out" className="inline-flex items-center gap-2 rounded-xl border bg-background hover:bg-muted/50 px-4 py-2.5 transition-colors text-sm font-medium">
                  <Trash2 className="w-4 h-4" />
                  إخراج مخزون
                </Link>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
