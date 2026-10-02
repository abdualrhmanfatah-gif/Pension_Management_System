import { useMutation,useQuery } from "@tanstack/react-query";
import {
AlertTriangle,   ArrowRight,   Building2, Calculator,
CheckCircle2, ClipboardCheck,
Package, Save, } from "lucide-react";
import { useEffect,useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { type AssetStocktakeLineDto,stocktakeApi } from "@/api/inventory/stocktake";
import { Heading, StatusBadge, Text } from "@/components/ui";
import {
  AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent,
  AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle,
} from "@/components/ui/AlertDialog";
import { Badge } from "@/components/ui/Badge";
import { Button } from "@/components/ui/Button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/Card";
import { Checkbox } from "@/components/ui/Checkbox";
import { Input } from "@/components/ui/Input";
import { Loader } from "@/components/ui/Loader";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select";
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
} from "@/components/ui/Table";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/Tabs";
import { STOCKTAKE_STATUS } from "@/constants/statusConfigs";
import { cn } from "@/lib/utils";

// ─── Shared Constants ───────────────────────────────────



const CONDITION_OPTIONS = [
  { value: "Good", label: "جيدة" },
  { value: "Fair", label: "مقبولة" },
  { value: "Poor", label: "ضعيفة" },
  { value: "Missing", label: "مفقودة" },
];

const CONDITION_MAP: Record<string, string> = Object.fromEntries(
  CONDITION_OPTIONS.map((o) => [o.value, o.label])
);

const formatCurrency = (amount: number) =>
  new Intl.NumberFormat("ar-YE", { style: "decimal", minimumFractionDigits: 0 }).format(amount) + " ريال";

// ─── Shared Hook: Fiscal Years ──────────────────────────

function useFiscalYears() {
  return useQuery({
    queryKey: ["fiscal-years"],
    queryFn: async () => {
      const res = await stocktakeApi.getFiscalYears();
      return res.data;
    },
  });
}

// ─── Loading ────────────────────────────────────────────

function LoadingSpinner() {
  return (
    <div className="flex items-center justify-center h-64">
      <Loader variant="page" text="جاري تحميل بيانات الجرد..." />
    </div>
  );
}

// ─── Empty State ────────────────────────────────────────

function EmptyStocktake({ onCreate, isPending }: { onCreate: () => void; isPending: boolean }) {
  return (
    <Card>
      <CardContent className="pt-6 text-center">
        <Package className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
        <p className="text-muted-foreground">لم يتم إنشاء جرد لهذه السنة المالية بعد</p>
        <Button onClick={onCreate} className="mt-4" disabled={isPending}>
          إنشاء جرد جديد
        </Button>
      </CardContent>
    </Card>
  );
}

// ─── Inventory Stocktake Tab ────────────────────────────

function InventoryStocktakeTab({ fiscalYearId }: { fiscalYearId: string }) {
  const [physicalCounts, setPhysicalCounts] = useState<Record<number, number>>({});
  const [showAdjustDialog, setShowAdjustDialog] = useState(false);

  const { data: stocktake, isLoading, refetch } = useQuery({
    queryKey: ["simple-stocktake", fiscalYearId],
    queryFn: async () => {
      if (!fiscalYearId || fiscalYearId === "current") {
        const res = await stocktakeApi.getSimpleStocktake();
        return res.data;
      }
      const res = await stocktakeApi.getSimpleStocktakeByFiscalYear(Number(fiscalYearId));
      return res.data;
    },
    enabled: true,
  });

  const createMutation = useMutation({
    mutationFn: () => stocktakeApi.createSimpleStocktake(),
    onSuccess: () => { toast.success("تم إنشاء جرد المخزون بنجاح"); refetch(); },
    onError: (error: any) => { toast.error(error.response?.data?.message || "فشل إنشاء الجرد"); },
  });

  const adjustMutation = useMutation({
    mutationFn: (stocktakeId: number) => stocktakeApi.adjustSimpleStocktake(stocktakeId),
    onSuccess: () => { toast.success("تم تسوية الجرد وترحيل القيد بنجاح"); setShowAdjustDialog(false); refetch(); },
    onError: (error: any) => { toast.error(error.response?.data?.message || "فشل تسوية الجرد"); },
  });

  const handleAdjust = () => {
    if (!stocktake) return;
    adjustMutation.mutate(stocktake.simpleStocktakeID);
  };

  if (isLoading) return <LoadingSpinner />;
  if (!stocktake) return <EmptyStocktake onCreate={() => createMutation.mutate()} isPending={createMutation.isPending} />;

  const isInProgress = stocktake.status === "InProgress";

  return (
    <div className="space-y-4">
      {/* Header Actions */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <StatusBadge status={stocktake.status} config={STOCKTAKE_STATUS} />
          {stocktake.status === "Adjusted" && (
            <Badge variant="success" className="bg-green-600">
              <CheckCircle2 className="w-3 h-3 ml-1" /> تمت التسوية
            </Badge>
          )}
        </div>
        {!stocktake && (
          <Button onClick={() => createMutation.mutate()} disabled={createMutation.isPending}>
            <Package className="w-4 h-4 ml-2" /> إنشاء جرد جديد
          </Button>
        )}
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">إجمالي الأصناف</p>
                <p className="text-lg md:text-xl lg:text-2xl font-bold">{stocktake.totalItems}</p>
              </div>
              <Package className="w-8 h-8 text-muted-foreground" />
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">أصناف بها فروق</p>
                <p className="text-lg md:text-xl lg:text-2xl font-bold text-yellow-600">{stocktake.itemsWithVariance}</p>
              </div>
              <AlertTriangle className="w-8 h-8 text-yellow-600" />
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">قيمة الفروق</p>
                <p className="text-lg md:text-xl lg:text-2xl font-bold text-red-600">{formatCurrency(stocktake.totalVarianceValue)}</p>
              </div>
              <Calculator className="w-8 h-8 text-red-600" />
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">الحالة</p>
          <StatusBadge status={stocktake.status} config={STOCKTAKE_STATUS} />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Table */}
      {stocktake.lines.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle>قائمة الأصناف ({stocktake.lines.length} صنف)</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-20 text-center">كود الصنف</TableHead>
                    <TableHead className="min-w-[150px]">اسم الصنف</TableHead>
                    <TableHead className="w-20 text-center">بالسجلات</TableHead>
                    <TableHead className="w-24 text-center">الكمية الفعلية</TableHead>
                    <TableHead className="w-16 text-center">الفرق</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {stocktake.lines.map((line) => {
                    const physicalQty = physicalCounts[line.inventoryItemID] ?? line.physicalQuantity;
                    const variance = physicalQty - line.systemQuantity;
                    return (
                      <TableRow key={line.lineID}>
                        <TableCell className="font-mono text-sm text-center">{line.itemCode}</TableCell>
                        <TableCell className="font-medium">{line.itemName}</TableCell>
                        <TableCell className="text-center">{line.systemQuantity}</TableCell>
                        <TableCell className="text-center">
                          {isInProgress ? (
                            <Input
                              type="number"
                              value={physicalCounts[line.inventoryItemID] ?? line.physicalQuantity}
                              onChange={(e) => {
                                const val = parseInt(e.target.value) || 0;
                                setPhysicalCounts((prev) => ({ ...prev, [line.inventoryItemID]: val }));
                              }}
                              className="w-20 text-center h-9"
                              min={0}
                            />
                          ) : (
                            <span>{physicalQty}</span>
                          )}
                        </TableCell>
                        <TableCell className="text-center">
                          <span className={variance > 0 ? "text-green-600 font-bold" : variance < 0 ? "text-red-600 font-bold" : ""}>
                            {variance > 0 ? `+${variance}` : variance}
                          </span>
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </div>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardContent className="pt-6 text-center">
            <Package className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
            <p className="text-muted-foreground">لا توجد أصناف في المخزون للجرد</p>
          </CardContent>
        </Card>
      )}

      {/* Adjust Dialog */}
      <AlertDialog open={showAdjustDialog} onOpenChange={setShowAdjustDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>تأكيد تسوية الجرد</AlertDialogTitle>
            <AlertDialogDescription>
              سيتم إنشاء قيد محاسبي للتسوية وترحيله تلقائياً. هل أنت متأكد من المتابعة؟
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>إلغاء</AlertDialogCancel>
            <AlertDialogAction onClick={handleAdjust} disabled={adjustMutation.isPending}>
              {adjustMutation.isPending ? "جاري التسوية..." : "تأكيد التسوية"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

// ─── Asset Stocktake Tab ────────────────────────────────

function AssetStocktakeTab({ fiscalYearId }: { fiscalYearId: string }) {
  const [edits, setEdits] = useState<Record<number, { isPresent: boolean; actualCondition: string; recordedQuantity: number; notes: string }>>({});
  const [showCompleteDialog, setShowCompleteDialog] = useState(false);

  const { data: stocktake, isLoading, refetch } = useQuery({
    queryKey: ["asset-stocktake", fiscalYearId],
    queryFn: async () => {
      if (!fiscalYearId || fiscalYearId === "current") {
        const res = await stocktakeApi.getAssetStocktake();
        return res.data;
      }
      const res = await stocktakeApi.getAssetStocktakeByFiscalYear(Number(fiscalYearId));
      return res.data;
    },
    enabled: true,
  });

  useEffect(() => { setEdits({}); }, [stocktake?.assetStocktakeID]);

  const createMutation = useMutation({
    mutationFn: (fiscalYearID: number) => stocktakeApi.createAssetStocktake(fiscalYearID),
    onSuccess: () => { toast.success("تم إنشاء جرد الأصول بنجاح"); refetch(); },
    onError: (error: any) => { toast.error(error.response?.data?.message || "فشل إنشاء الجرد"); },
  });

  const recordMutation = useMutation({
    mutationFn: ({ stocktakeId, lines }: { stocktakeId: number; lines: AssetStocktakeLineDto[] }) =>
      stocktakeApi.recordAssetStocktake(stocktakeId, lines),
    onSuccess: () => { toast.success("تم حفظ نتائج الجرد بنجاح"); setEdits({}); refetch(); },
    onError: (error: any) => { toast.error(error.response?.data?.message || "فشل حفظ النتائج"); },
  });

  const completeMutation = useMutation({
    mutationFn: (stocktakeId: number) => stocktakeApi.completeAssetStocktake(stocktakeId),
    onSuccess: () => { toast.success("تم إتمام الجرد بنجاح"); setShowCompleteDialog(false); setEdits({}); refetch(); },
    onError: (error: any) => { toast.error(error.response?.data?.message || "فشل إتمام الجرد"); },
  });

  const handleCreateStocktake = () => {
    const yearId = fiscalYearId ? Number(fiscalYearId) : undefined;
    if (!yearId) { toast.error("الرجاء اختيار سنة مالية"); return; }
    createMutation.mutate(yearId);
  };

  const updateEdit = (lineId: number, field: string, value: any) => {
    setEdits((prev) => {
      const existing = stocktake?.lines.find((l) => l.stocktakeLineID === lineId);
      const current = prev[lineId] || {
        isPresent: existing?.isPresent ?? false,
        actualCondition: existing?.actualCondition || "Good",
        recordedQuantity: existing?.recordedQuantity ?? 0,
        notes: existing?.notes || "",
      };
      return { ...prev, [lineId]: { ...current, [field]: value } };
    });
  };

  const getEffectiveLine = (line: AssetStocktakeLineDto) => {
    const edit = edits[line.stocktakeLineID];
    return {
      ...line,
      isPresent: edit?.isPresent ?? line.isPresent,
      actualCondition: edit?.actualCondition ?? (line.actualCondition || ""),
      recordedQuantity: edit?.recordedQuantity ?? line.recordedQuantity ?? 0,
      notes: edit?.notes ?? line.notes ?? "",
    };
  };

  const handleSaveResults = () => {
    if (!stocktake) return;
    const lines: AssetStocktakeLineDto[] = stocktake.lines.map((line) => {
      const edit = edits[line.stocktakeLineID];
      return {
        ...line,
        isPresent: edit?.isPresent ?? line.isPresent,
        actualCondition: edit?.actualCondition ?? (line.actualCondition || "Good"),
        recordedQuantity: edit?.recordedQuantity ?? line.recordedQuantity ?? 0,
        notes: edit?.notes ?? line.notes,
      };
    });
    recordMutation.mutate({ stocktakeId: stocktake.assetStocktakeID, lines });
  };

  const handleComplete = () => {
    if (!stocktake) return;
    if (Object.keys(edits).length > 0) handleSaveResults();
    completeMutation.mutate(stocktake.assetStocktakeID);
  };

  if (isLoading) return <LoadingSpinner />;
  if (!stocktake) return <EmptyStocktake onCreate={handleCreateStocktake} isPending={createMutation.isPending} />;

  const isInProgress = stocktake.status === "InProgress";
  const hasEdits = Object.keys(edits).length > 0;

  return (
    <div className="space-y-4">
      {/* Header Actions */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <StatusBadge status={stocktake.status} config={STOCKTAKE_STATUS} />
          {stocktake.status === "Completed" && (
            <Badge variant="success" className="bg-green-600">
              <CheckCircle2 className="w-3 h-3 ml-1" /> تم إتمام الجرد
            </Badge>
          )}
          {hasEdits && isInProgress && (
            <Badge variant="outline" className="border-yellow-500 text-yellow-600">
              توجد تعديلات غير محفوظة
            </Badge>
          )}
        </div>
        {isInProgress && (
          <div className="flex gap-2">
            <Button variant="outline" onClick={handleSaveResults} disabled={!hasEdits || recordMutation.isPending}>
              <Save className="w-4 h-4 ml-2" /> حفظ النتائج
            </Button>
            <Button onClick={() => setShowCompleteDialog(true)}>
              <ClipboardCheck className="w-4 h-4 ml-2" /> إتمام الجرد
            </Button>
          </div>
        )}
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">إجمالي الأصول</p>
                <p className="text-lg md:text-xl lg:text-2xl font-bold">{stocktake.summary.totalAssets}</p>
              </div>
              <Building2 className="w-8 h-8 text-muted-foreground" />
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">موجودة</p>
                <p className="text-lg md:text-xl lg:text-2xl font-bold text-green-600">{stocktake.summary.presentAssets}</p>
              </div>
              <CheckCircle2 className="w-8 h-8 text-green-600" />
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">مفقودة</p>
                <p className="text-lg md:text-xl lg:text-2xl font-bold text-red-600">{stocktake.summary.missingAssets}</p>
              </div>
              <AlertTriangle className="w-8 h-8 text-red-600" />
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-muted-foreground">قيمة الدفتر</p>
                <p className="text-lg md:text-xl lg:text-2xl font-bold">{formatCurrency(stocktake.summary.totalBookValue)}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Table */}
      {stocktake.lines.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle>قائمة الأصول ({stocktake.lines.length} أصل)</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow className="bg-muted/50 hover:bg-muted/50">
                    <TableHead className="w-[100px] text-center font-bold text-xs">رقم الأصل</TableHead>
                    <TableHead className="min-w-[160px] font-bold text-xs">اسم الأصل</TableHead>
                    <TableHead className="w-[120px] text-center font-bold text-xs">الموقع</TableHead>
                    <TableHead className="w-[130px] text-center font-bold text-xs">القيمة الدفترية</TableHead>
                    <TableHead className="w-[100px] text-center font-bold text-xs">الكمية الفعلية</TableHead>
                    <TableHead className="w-[80px] text-center font-bold text-xs">موجود</TableHead>
                    <TableHead className="w-[120px] text-center font-bold text-xs">الحالة</TableHead>
                    <TableHead className="w-[180px] text-center font-bold text-xs">ملاحظات</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {stocktake.lines.map((line) => {
                    const effective = getEffectiveLine(line);
                    const isMissing = effective.isPresent === false;
                    return (
                      <TableRow
                        key={line.stocktakeLineID}
                        className={cn(
                          "transition-colors",
                          edits[line.stocktakeLineID] ? "bg-amber-50/80 dark:bg-amber-950/30" : "hover:bg-muted/30",
                          isMissing && "bg-red-50/50 dark:bg-red-950/20"
                        )}
                      >
                        <TableCell className="text-center">
                          <span className="font-mono text-xs bg-muted/60 px-2 py-1 rounded">{line.assetNumber}</span>
                        </TableCell>
                        <TableCell>
                          <span className="font-medium text-sm">{line.assetName}</span>
                        </TableCell>
                        <TableCell className="text-center text-sm text-muted-foreground">{line.location || "-"}</TableCell>
                        <TableCell className="text-center">
                          <span className="font-mono text-sm font-semibold" dir="ltr">{formatCurrency(line.netBookValue)}</span>
                        </TableCell>
                        <TableCell className="text-center">
                          {isInProgress ? (
                            <Input
                              type="number"
                              min={0}
                              value={effective.recordedQuantity}
                              onChange={(e) => updateEdit(line.stocktakeLineID, "recordedQuantity", parseInt(e.target.value) || 0)}
                              className="w-16 text-center h-8 text-sm"
                            />
                          ) : (
                            <span className="font-semibold text-sm">{effective.recordedQuantity}</span>
                          )}
                        </TableCell>
                        <TableCell className="text-center">
                          {isInProgress ? (
                            <div className="flex justify-center">
                              <Checkbox
                                checked={effective.isPresent}
                                onCheckedChange={(checked) => updateEdit(line.stocktakeLineID, "isPresent", !!checked)}
                              />
                            </div>
                          ) : (
                            <Badge variant={effective.isPresent ? "success" : "destructive"} className="text-xs">
                              {effective.isPresent ? "موجود" : "مفقود"}
                            </Badge>
                          )}
                        </TableCell>
                        <TableCell className="text-center">
                          {isInProgress ? (
                            <Select
                              value={effective.actualCondition || "Good"}
                              onValueChange={(val) => updateEdit(line.stocktakeLineID, "actualCondition", val)}
                            >
                              <SelectTrigger className="h-8 w-full text-xs"><SelectValue /></SelectTrigger>
                              <SelectContent>
                                {CONDITION_OPTIONS.map((opt) => (
                                  <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          ) : (
                            <Badge
                              variant={effective.actualCondition === "Good" ? "success" : effective.actualCondition === "Missing" ? "destructive" : "secondary"}
                              className="text-xs"
                            >
                              {CONDITION_MAP[effective.actualCondition] || effective.actualCondition || "-"}
                            </Badge>
                          )}
                        </TableCell>
                        <TableCell className="text-center">
                          {isInProgress ? (
                            <Input
                              value={effective.notes}
                              onChange={(e) => updateEdit(line.stocktakeLineID, "notes", e.target.value)}
                              placeholder="ملاحظات..."
                              className="h-8 w-full text-xs text-center"
                            />
                          ) : (
                            <span className="text-xs text-muted-foreground">{effective.notes || "-"}</span>
                          )}
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </div>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardContent className="pt-6 text-center">
            <Package className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
            <p className="text-muted-foreground">لا توجد أصول ثابتة للجرد</p>
          </CardContent>
        </Card>
      )}

      {/* Complete Dialog */}
      <AlertDialog open={showCompleteDialog} onOpenChange={setShowCompleteDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>تأكيد إتمام الجرد</AlertDialogTitle>
            <AlertDialogDescription>
              سيتم حفظ جميع التعديلات وإتمام الجرد. لا يمكن تعديل الجرد بعد الإتمام. هل أنت متأكد؟
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>إلغاء</AlertDialogCancel>
            <AlertDialogAction onClick={handleComplete} disabled={completeMutation.isPending}>
              {completeMutation.isPending ? "جاري الإتمام..." : "تأكيد الإتمام"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

// ─── Main Page ──────────────────────────────────────────

export function StocktakePage() {
  const navigate = useNavigate();
  const [selectedFiscalYearId, setSelectedFiscalYearId] = useState<string>("");
  const { data: fiscalYears } = useFiscalYears();

  return (
    <div className="space-y-6">
      {/* Title + Fiscal Year Selector */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="icon" onClick={() => navigate("/inventory")}>
            <ArrowRight className="w-4 h-4" />
          </Button>
          <div>
            <Heading level="h1" className="text-lg md:text-xl lg:text-2xl font-bold">الجرد</Heading>
            <p className="text-sm text-muted-foreground">جرد المخزون والأصول الثابتة</p>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <Text variant="label">السنة المالية:</Text>
          <Select
            value={selectedFiscalYearId || "__current__"}
            onValueChange={(val) => setSelectedFiscalYearId(val === "__current__" ? "" : val)}
          >
            <SelectTrigger className="w-64">
              <SelectValue placeholder="السنة المالية الحالية" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="__current__">السنة المالية الحالية</SelectItem>
              {fiscalYears?.map((year) => (
                <SelectItem key={year.fiscalYearID} value={String(year.fiscalYearID)}>
                  {year.yearName} {year.isClosed ? "(مغلقة)" : "(مفتوحة)"}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="inventory">
        <TabsList>
          <TabsTrigger value="inventory">جرد المخزون (قرطاسية وأحبار)</TabsTrigger>
          <TabsTrigger value="assets">جرد الأصول الثابتة</TabsTrigger>
        </TabsList>
        <TabsContent value="inventory">
          <InventoryStocktakeTab fiscalYearId={selectedFiscalYearId} />
        </TabsContent>
        <TabsContent value="assets">
          <AssetStocktakeTab fiscalYearId={selectedFiscalYearId} />
        </TabsContent>
      </Tabs>
    </div>
  );
}
