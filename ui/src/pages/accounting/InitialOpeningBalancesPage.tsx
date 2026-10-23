import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  AlertCircle,
  BookOpen,
  Calculator,
  CheckCircle2,
  Package,
  Plus,
  RotateCcw,
  ScrollText,
  Trash2,
  Wrench,
} from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

import { accountsApi, closingApi,fiscalYearsApi } from "@/api/accounting";
import { fixedAssetApi } from "@/api/fixedAssets";
import { FixedAssetFormDialog } from "@/components/domain/assets/FixedAssetFormDialog";
import { StickyHeader } from "@/components/layout/PageLayout";
import {
  Badge,
  Button,
  Card,
  ConfirmDialog,
  DateInput,
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  Heading,
  Input,
  PageLayout,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
  Text,
} from "@/components/ui";
import { normalizeApiError } from "@/lib/errorUtils";
import type { InitialOpeningBalanceLineDto } from "@/types/accounting";
import type { ChartOfAccountDto, FiscalYearSummaryDto, InitialOpeningBalanceResponseDto,SeedInitialDefaultsResultDto } from "@/types/accounting";

// ═══════════════════════════════════════════════
// Types
// ═══════════════════════════════════════════════

interface AssetLine {
  assetID: number;
  serverAssetID?: number;
  assetNumber: string;
  assetName: string;
  acquiredCost: number;
  accountID: number;
  accountCode: string;
  accountName: string;
}

interface InventoryLine {
  id: string;
  itemName: string;
  value: number;
  accountID: number;
  accountCode: string;
  accountName: string;
}

// ═══════════════════════════════════════════════
// Component
// ═══════════════════════════════════════════════

export function InitialOpeningBalancesPage() {
  const queryClient = useQueryClient();
  const [fiscalYearID, setFiscalYearID] = useState<number | null>(null);
  const [asOfDate, setAsOfDate] = useState(new Date().toISOString().split("T")[0]);
  const [lines, setLines] = useState<InitialOpeningBalanceLineDto[]>([]);
  const [assets, setAssets] = useState<AssetLine[]>([]);
  const [inventoryItems, setInventoryItems] = useState<InventoryLine[]>([]);
  const [showAssetForm, setShowAssetForm] = useState(false);
  const [showResult, setShowResult] = useState<{ accountsCarriedForward: number } | null>(null);
  const [activeTab, setActiveTab] = useState("assets");

  // Inventory form state
  const [invName, setInvName] = useState("");

  const [invQty, setInvQty] = useState<number>(1);
  const [invUnitPrice, setInvUnitPrice] = useState<number>(0);

  // Other account form state
  const [otherAccountID, setOtherAccountID] = useState<number | null>(null);
  const [otherBalance, setOtherBalance] = useState<number>(0);

  // Reset all data state
  const [showResetConfirm, setShowResetConfirm] = useState(false);

  // Seed initial defaults (الأصول ورصيد البنك المركزي) dialog state
  const [showSeedDialog, setShowSeedDialog] = useState(false);
  const [seedFiscalYearID, setSeedFiscalYearID] = useState<number | null>(null);

  // ─── Queries ─────────────────────────────────
  const { data: fiscalYears } = useQuery<FiscalYearSummaryDto[]>({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });

  const { data: allAccounts } = useQuery<ChartOfAccountDto[]>({
    queryKey: ["accounts", "postable"],
    queryFn: () => accountsApi.getPostable(),
  });

  // ─── تحميل الأرصدة الافتتاحية المحفوظة ─────────
  const { data: existingBalances } = useQuery<InitialOpeningBalanceResponseDto>({
    queryKey: ["initial-opening-balances", fiscalYearID],
    queryFn: () => closingApi.getInitialOpeningBalances(fiscalYearID!),
    enabled: !!fiscalYearID,
  });

  // تحميل البيانات عند وجود أرصدة محفوظة
  useEffect(() => {
    if (existingBalances?.lines && existingBalances.lines.length > 0) {
      setLines(existingBalances.lines);
      if (existingBalances.asOfDate) {
        setAsOfDate(existingBalances.asOfDate.split("T")[0]);
      }
    }
  }, [existingBalances]);

  // ─── Helpers ─────────────────────────────────
  const usedAccountIds = useMemo(() => {
    const ids = new Set<number>();
    lines.forEach(l => { if (l.accountID > 0) ids.add(l.accountID); });
    assets.forEach(a => { if (a.accountID > 0) ids.add(a.accountID); });
    inventoryItems.forEach(i => { if (i.accountID > 0) ids.add(i.accountID); });
    return ids;
  }, [lines, assets, inventoryItems]);

  const addOrUpdateLine = (accountID: number, amount: number, label?: string) => {
    setLines(prev => {
      const account = allAccounts?.find(a => a.accountID === accountID);
      if (!account) return prev;

      const existing = prev.find(l => l.accountID === accountID);
      if (existing) {
        return prev.map(l =>
          l.accountID === accountID
            ? { ...l, balance: (l.balance ?? 0) + (amount ?? 0) }
            : l
        );
      }
      return [...prev, {
        accountID: account.accountID,
        accountCode: account.accountCode,
        accountName: label || account.accountName,
        accountNature: account.accountNature,
        balance: amount ?? 0,
      }];
    });
  };

  const removeLineByAccountId = (accountID: number) => {
    setLines(prev => prev.filter(l => l.accountID !== accountID));
  };

  // ─── Offset Credit (رأس المال) ─────────────
  const EQUITY_ACCOUNT_CODE = "2111"; // مساهمة الحكومة - حقوق ملكية

  const addOffsetCredit = (amount: number) => {
    const equityAccount = allAccounts?.find(a => a.accountCode === EQUITY_ACCOUNT_CODE && a.isPostable);
    if (!equityAccount) {
      toast.error("لا يوجد حساب حقوق الملكية (2111)");
      return;
    }
    setLines(prev => {
      const existing = prev.find(l => l.accountID === equityAccount.accountID);
      if (existing) {
        return prev.map(l =>
          l.accountID === equityAccount.accountID
            ? { ...l, balance: (l.balance ?? 0) + (amount ?? 0) }
            : l
        );
      }
      return [...prev, {
        accountID: equityAccount.accountID,
        accountCode: equityAccount.accountCode,
        accountName: equityAccount.accountName,
        accountNature: equityAccount.accountNature,
        balance: amount ?? 0,
      }];
    });
  };

  const removeOffsetCredit = (amount: number) => {
    const equityAccount = allAccounts?.find(a => a.accountCode === EQUITY_ACCOUNT_CODE && a.isPostable);
    if (!equityAccount) return;
    setLines(prev => {
      const existing = prev.find(l => l.accountID === equityAccount.accountID);
      if (!existing) return prev;
      const newBalance = (existing.balance ?? 0) - (amount ?? 0);
      if (Math.abs(newBalance) < 0.01) {
        return prev.filter(l => l.accountID !== equityAccount.accountID);
      }
      return prev.map(l =>
        l.accountID === equityAccount.accountID
          ? { ...l, balance: newBalance }
          : l
      );
    });
  };

  // ─── Asset Handlers ──────────────────────────
  const handleAddAsset = (assetData: { assetID?: number; assetAccountID: number; unitPrice: number; quantity: number; assetName: string; assetNumber?: string }) => {
    // حساب التكلفة الإجمالية من السعر والكمية
    const acquiredCost = (assetData.unitPrice || 0) * (assetData.quantity || 1);

    // Find account - if assetAccountID is 0 or not found, use default fixed asset account
    let account = allAccounts?.find(a => a.accountID === assetData.assetAccountID);
    if (!account && assetData.assetAccountID === 0) {
      // Try to find a default fixed asset account (code starts with "11")
      account = allAccounts?.find(a => a.accountCode.startsWith("11") && a.isPostable);
    }
    if (!account || !(acquiredCost > 0)) {
      toast.error("لم يتم العثور على حساب مناسب للأصل");
      return;
    }

    // تحقق من عدم التكرار
    const duplicate = assets.find(a => a.assetName === assetData.assetName);
    if (duplicate) {
      toast.error("يوجد أصل مماثل بالفعل");
      return;
    }

    const newAsset: AssetLine = {
      assetID: Date.now(),
      serverAssetID: assetData.assetID,
      assetNumber: assetData.assetNumber || `AS-${Date.now()}`,
      assetName: assetData.assetName,
      acquiredCost,
      accountID: account.accountID,
      accountCode: account.accountCode,
      accountName: account.accountName,
    };
    setAssets(prev => [...prev, newAsset]);
    addOrUpdateLine(account.accountID, acquiredCost, account.accountName);
    addOffsetCredit(acquiredCost);
    toast.success(`تمت إضافة الأصل "${assetData.assetName}" بقيمة ${acquiredCost.toLocaleString()}`);
  };

  const handleRemoveAsset = async (index: number) => {
    const asset = assets[index];
    if (asset.serverAssetID) {
      try {
        await fixedAssetApi.delete(asset.serverAssetID);
        toast.success(`تم حذف الأصل "${asset.assetName}" من قاعدة البيانات`);
      } catch {
        toast.warning("تم حذف الأصل من القيد فقط");
      }
    }
    removeLineByAccountId(asset.accountID);
    removeOffsetCredit(asset.acquiredCost ?? 0);
    setAssets(prev => prev.filter((_, i) => i !== index));
  };

  // ─── Inventory Handlers ──────────────────────
  const handleAddInventory = () => {
    const totalValue = invQty * invUnitPrice;
    if (!invName.trim() || totalValue <= 0) {
      toast.error("أدخل اسم وقيمة المخزون (الكمية × سعر الوحدة)");
      return;
    }
    const inventoryAccount = allAccounts?.find(a => a.accountCode.startsWith("13") && a.isPostable);
    if (!inventoryAccount) {
      toast.error("لا يوجد حساب مخزون متاح");
      return;
    }

    // تحقق من عدم التكرار
    const duplicate = inventoryItems.find(i => i.itemName === invName.trim());
    if (duplicate) {
      toast.error("يوجد صنف مخزون مماثل بالفعل");
      return;
    }

    const newInv: InventoryLine = {
      id: crypto.randomUUID(),
      itemName: invName.trim(),
      value: totalValue,
      accountID: inventoryAccount.accountID,
      accountCode: inventoryAccount.accountCode,
      accountName: inventoryAccount.accountName,
    };
    setInventoryItems(prev => [...prev, newInv]);
    addOrUpdateLine(inventoryAccount.accountID, totalValue, `${inventoryAccount.accountName} - ${invName.trim()}`);
    addOffsetCredit(totalValue);
    toast.success(`تمت إضافة المخزون "${invName}" بقيمة ${totalValue.toLocaleString()}`);
    setInvName("");

    setInvQty(1);
    setInvUnitPrice(0);
  };

  const handleRemoveInventory = (index: number) => {
    const item = inventoryItems[index];
    removeLineByAccountId(item.accountID);
    removeOffsetCredit(item.value ?? 0);
    setInventoryItems(prev => prev.filter((_, i) => i !== index));
  };

  // ─── Other Account Handlers ──────────────────
  const handleAddOtherAccount = () => {
    if (!otherAccountID || otherBalance <= 0) {
      toast.error("اختر حساب وأدخل الرصيد");
      return;
    }
    addOrUpdateLine(otherAccountID, otherBalance);
    setOtherAccountID(null);
    setOtherBalance(0);
    toast.success("تمت إضافة الحساب");
  };

  const handleRemoveOtherAccount = (accountID: number) => {
    removeLineByAccountId(accountID);
  };

  // ─── Summary ─────────────────────────────────
  const { totalDebit, totalCredit, isBalanced } = useMemo(() => {
    let debit = 0;
    let credit = 0;
    for (const line of lines) {
      const bal = line.balance ?? 0;
      if (line.accountID === 0 || bal === 0) continue;
      if (line.accountNature === "Debit") {
        if (bal > 0) debit += bal;
        else credit += Math.abs(bal);
      } else {
        if (bal > 0) credit += bal;
        else debit += Math.abs(bal);
      }
    }
    return { totalDebit: debit, totalCredit: credit, isBalanced: Math.abs(debit - credit) < 0.01 };
  }, [lines]);

  // ─── Mutation ────────────────────────────────
  const mutation = useMutation({
    mutationFn: () => closingApi.createInitialOpeningBalances({
      fiscalYearID: fiscalYearID!,
      asOfDate,
      description: "إثبات الأرصدة الافتتاحية الأولية",
      lines: lines.filter(l => l.accountID > 0 && (l.balance ?? 0) !== 0),
    }),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ["accounting"] });
      setShowResult({ accountsCarriedForward: data.accountsCarriedForward });
      toast.success(`تم إدخال الأرصدة الافتتاحية: ${data.accountsCarriedForward} حساب`);
    },
    onError: (err: Error) => {
      const { message } = normalizeApiError(err);
      toast.error(message || "فشل في إدخال الأرصدة الافتتاحية");
    },
  });

  // ─── Reset All Mutation ──────────────────────
  const resetAllMutation = useMutation({
    mutationFn: () => closingApi.resetAll(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["accounting"] });
      queryClient.invalidateQueries({ queryKey: ["accounts"] });
      queryClient.invalidateQueries({ queryKey: ["fiscal-years"] });
      setLines([]);
      setAssets([]);
      setInventoryItems([]);
      setFiscalYearID(null);
      toast.success("تم تصفير جميع البيانات المحاسبية والأصول والمخزون");
    },
    onError: (err: Error) => {
      const { message } = normalizeApiError(err);
      toast.error(message || "فشل في تصفير البيانات");
    },
  });

  // ─── Seed Initial Defaults Mutation ──────────
  const seedDefaultsMutation = useMutation<SeedInitialDefaultsResultDto>({
    mutationFn: () => closingApi.seedInitialDefaults(seedFiscalYearID!),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ["accounting"] });
      queryClient.invalidateQueries({ queryKey: ["initial-opening-balances"] });

      // إضافة الأصول لقائمة الأصول الثابتة (نفس منطق الأصل اليدوي)
      data.assets?.forEach((a) => {
        handleAddAsset({
          assetAccountID: a.accountID,
          unitPrice: a.acquiredCost,
          quantity: 1,
          assetName: a.assetName,
        });
      });

      // إضافة أرصدة البنك المركزي لقائمة الحسابات (تُرحّل عند ترحيل الأرصدة)
      data.balances?.forEach((b) => {
        addOrUpdateLine(b.accountID, b.balance, `${b.accountCode} - ${b.accountName}`);
      });

      setShowSeedDialog(false);
      setSeedFiscalYearID(null);
      toast.success(
        `تم إدراج ${data.assetCount} أصل. اضغط "ترحيل الأرصدة الافتتاحية" لإثبات الأرصدة.`
      );
    },
    onError: (err: Error) => {
      const { message } = normalizeApiError(err);
      toast.error(message || "فشل في إدراج البيانات الأولية");
    },
  });

  // ═══════════════════════════════════════════════
  // Result Screen
  // ═══════════════════════════════════════════════
  if (showResult) {
    return (
      <PageLayout>
        <div className="flex flex-col items-center justify-center min-h-[60vh] gap-6">
          <div className="w-20 h-20 rounded-full bg-green-100 flex items-center justify-center">
            <CheckCircle2 className="w-10 h-10 text-green-600" />
          </div>
          <Heading level="h2" className="text-lg md:text-xl lg:text-2xl font-bold">تم إدخال الأرصدة بنجاح</Heading>
          <Card className="p-6 w-full max-w-md">
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-muted-foreground">عدد الحسابات</span>
                <span className="font-bold">{showResult.accountsCarriedForward}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">الأصول الثابتة</span>
                <span className="font-bold">{assets.length}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">المخزون</span>
                <span className="font-bold">{inventoryItems.length}</span>
              </div>
            </div>
          </Card>
          <Button onClick={() => window.location.reload()}>إدخال أرصدة جديدة</Button>
        </div>
      </PageLayout>
    );
  }

  // ═══════════════════════════════════════════════
  // Main Form
  // ═══════════════════════════════════════════════
  return (
    <>
    <PageLayout>
      {/* Header */}
      <StickyHeader>
        <div className="flex items-center gap-3">
          <div className="p-3 bg-primary/10 rounded-2xl">
            <Calculator className="w-8 h-8 text-primary" />
          </div>
          <div>
                          <Heading level="h1" className="text-xl md:text-3xl font-bold tracking-tight">الأرصدة الافتتاحية الأولية</Heading>
            <Text variant="description" className="mt-1">إدخال أرصدة الافتتاح عند بدء تشغيل النظام</Text>
          </div>
        </div>
      </StickyHeader>

      {/* Settings */}
      <Card className="p-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-1.5">
            <Text variant="label">السنة المالية</Text>
            <Select value={String(fiscalYearID ?? "")} onValueChange={(v) => setFiscalYearID(Number(v))}>
              <SelectTrigger>
                <SelectValue placeholder="اختر السنة" />
              </SelectTrigger>
              <SelectContent>
                {fiscalYears?.filter(y => !y.isClosed).map((fy) => (
                  <SelectItem key={fy.fiscalYearID} value={String(fy.fiscalYearID)}>
                    {fy.yearName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <DateInput
            label="التاريخ"
            value=""
            onChange={(v) => setAsOfDate(v || "")}
          />
        </div>
      </Card>

      {/* Warning */}
      <div className="flex items-start gap-3 p-4 bg-amber-500/5 rounded-xl">
        <AlertCircle className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />
        <div className="text-sm text-amber-800">
          <p className="font-medium">تنبيه</p>
          <Text variant="body" className="mt-1">يُستخدم هذا الإجراء فقط عند بدء تشغيل النظام لأول مرة. الأرصدة المُدخلة ستنشئ قيداً افتتاحياً واحداً فقط.</Text>
        </div>
      </div>

      {/* Reset All Button */}
      <Card className="p-4 border-destructive bg-destructive/5">
        <div className="flex items-center justify-between">
          <div>
            <Heading level="h3" className="text-sm font-semibold text-destructive">تصفير جميع البيانات</Heading>
            <Text variant="caption" className="mt-1">سيحذف جميع الأرصدة والأصول والمخزون والقيود المحاسبية ويُعيد تهيئة الدليل</Text>
          </div>
          <Button variant="destructive" size="sm" onClick={() => setShowResetConfirm(true)}>
            <RotateCcw className="w-4 h-4 ml-1" /> تصفير الكل
          </Button>
        </div>
      </Card>

      {/* Seed Initial Defaults Button */}
      <Card className="p-4 border-primary bg-primary/5">
        <div className="flex items-center justify-between">
          <div>
            <Heading level="h3" className="text-sm font-semibold text-primary">إدراج بيانات أولية افتراضية</Heading>
            <Text variant="caption" className="mt-1">إدراج الأصول الثابتة ورصيد البنك المركزي للسنة المالية المختارة (18211 أصل و256 التزام)</Text>
          </div>
          <Button variant="primary" size="sm" onClick={() => setShowSeedDialog(true)}>
            <BookOpen className="w-4 h-4 ml-1" /> إدراج بيانات أولية
          </Button>
        </div>
      </Card>

      {/* ═══ Tabs Section ═══ */}
      <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
        <TabsList className="w-full justify-start">
          <TabsTrigger value="assets" className="gap-2">
            <Wrench className="w-4 h-4" /> الأصول الثابتة
            <Badge variant="outline" className="px-1.5">{assets.length}</Badge>
          </TabsTrigger>
          <TabsTrigger value="inventory" className="gap-2">
            <Package className="w-4 h-4" /> المخزون
            <Badge variant="outline" className="px-1.5">{inventoryItems.length}</Badge>
          </TabsTrigger>
          <TabsTrigger value="other" className="gap-2">
            <BookOpen className="w-4 h-4" /> حسابات أخرى
            <Badge variant="outline" className="px-1.5">{lines.filter(l => l.accountID > 0 && (l.balance ?? 0) !== 0 && !assets.some(a => a.accountID === l.accountID) && !inventoryItems.some(i => i.accountID === l.accountID)).length}</Badge>
          </TabsTrigger>
          <TabsTrigger value="compound" className="gap-2">
            <ScrollText className="w-4 h-4" /> القيد المركب
            <Badge variant="outline" className="px-1.5">{lines.filter(l => l.accountID > 0 && (l.balance ?? 0) !== 0).length}</Badge>
          </TabsTrigger>
        </TabsList>

        {/* ═══ Assets Tab ═══ */}
        <TabsContent value="assets">
          <Card className="p-4">
            <div className="flex items-center justify-between mb-4">
              <Heading level="h3" className="text-lg font-semibold">الأصول الثابتة</Heading>
              <Button variant="outline" size="sm" onClick={() => setShowAssetForm(true)}>
                <Plus className="w-4 h-4 ml-1" /> إضافة أصل
              </Button>
            </div>

            {assets.length === 0 ? (
              <div className="text-center py-8 text-muted-foreground text-sm">
                لم تُضف أي أصول ثابتة بعد. اضغط "إضافة أصل" لبدء إدخال الأصول.
              </div>
            ) : (
              <div className="space-y-3">
                {assets.map((asset, index) => (
                  <div key={asset.assetID} className="flex items-center justify-between p-4 bg-muted/30 rounded-lg border border-primary">
                    <div className="flex items-center gap-4">
                      <div className="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
                        <Wrench className="w-5 h-5 text-primary" />
                      </div>
                      <div>
                        <div className="font-semibold">{asset.assetName}</div>
                        <div className="text-sm text-muted-foreground flex items-center gap-2 mt-1">
                          <span className="font-mono bg-primary/10 px-2 py-0.5 rounded">{asset.accountCode}</span>
                          <span>{asset.accountName}</span>
                        </div>
                      </div>
                    </div>
                    <div className="flex items-center gap-4">
                      <div className="text-left">
                        <div className="text-lg font-bold font-mono" dir="ltr">{(asset.acquiredCost ?? 0).toLocaleString()}</div>
                        <div className="text-xs text-muted-foreground">ر.ي</div>
                      </div>
                      <Button variant="ghost" size="icon" className="text-destructive" onClick={() => handleRemoveAsset(index)}>
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>
        </TabsContent>

        {/* ═══ Inventory Tab ═══ */}
        <TabsContent value="inventory">
          <Card className="p-4">
            <Heading level="h3" className="text-lg font-semibold mb-4">المخزون</Heading>

            {/* Add Inventory Form */}
            <div className="grid grid-cols-1 md:grid-cols-[1fr_100px_120px_auto] gap-3 mb-4 p-3 bg-muted/30 rounded-lg">
              <Input
                placeholder="اسم المخزون"
                value={invName}
                onChange={(e) => setInvName(e.target.value)}
              />
              <Input
                type="number"
                placeholder="الكمية"
                value={invQty || ""}
                onChange={(e) => setInvQty(parseInt(e.target.value) || 1)}
                className="font-mono"
              />
              <Input
                type="number"
                step="0.01"
                placeholder="سعر الوحدة"
                value={invUnitPrice || ""}
                onChange={(e) => setInvUnitPrice(parseFloat(e.target.value) || 0)}
                className="font-mono"
              />
              <Button variant="outline" onClick={handleAddInventory}>
                <Plus className="w-4 h-4 ml-1" /> إضافة
              </Button>
            </div>

            {inventoryItems.length === 0 ? (
              <div className="text-center py-6 text-muted-foreground text-sm">
                لم يُضف أي مخزون بعد. أدخل اسم وقيمة المخزون ثم اضغط "إضافة".
              </div>
            ) : (
              <div className="space-y-3">
                {inventoryItems.map((item, index) => (
                  <div key={item.id} className="flex items-center justify-between p-4 bg-muted/30 rounded-lg border border-primary">
                    <div className="flex items-center gap-4">
                      <div className="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
                        <Package className="w-5 h-5 text-primary" />
                      </div>
                        <div>
                          <div className="font-semibold">{item.itemName}</div>
                          <div className="text-sm text-muted-foreground flex items-center gap-2 mt-1">
                            <span className="font-mono bg-primary/10 px-2 py-0.5 rounded">{item.accountCode}</span>
                            <span>{item.accountName}</span>
                          </div>
                        </div>
                    </div>
                    <div className="flex items-center gap-4">
                      <div className="text-left">
                        <div className="text-lg font-bold font-mono" dir="ltr">{(item.value || 0).toLocaleString()}</div>
                        <div className="text-xs text-muted-foreground">ر.ي</div>
                      </div>
                      <Button variant="ghost" size="icon" className="text-destructive" onClick={() => handleRemoveInventory(index)}>
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>
        </TabsContent>

        {/* ═══ Other Accounts Tab ═══ */}
        <TabsContent value="other">
          <Card className="p-4">
            <Heading level="h3" className="text-lg font-semibold mb-4">حسابات أخرى</Heading>

            {/* Add Other Account Form */}
            <div className="grid grid-cols-1 md:grid-cols-[1fr_150px_auto] gap-3 mb-4 p-3 bg-muted/30 rounded-lg">
              <Select value={String(otherAccountID ?? "")} onValueChange={(v) => setOtherAccountID(Number(v))}>
                <SelectTrigger>
                  <SelectValue placeholder="اختر الحساب" />
                </SelectTrigger>
                <SelectContent>
                  {allAccounts?.filter(a => a.isPostable && !usedAccountIds.has(a.accountID)).map((acc) => (
                    <SelectItem key={acc.accountID} value={String(acc.accountID)}>
                      {acc.accountCode} - {acc.accountName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <Input
                type="number"
                step="0.01"
                placeholder="الرصيد"
                value={otherBalance || ""}
                onChange={(e) => setOtherBalance(parseFloat(e.target.value) || 0)}
                className="font-mono"
              />
              <Button variant="outline" onClick={handleAddOtherAccount}>
                <Plus className="w-4 h-4 ml-1" /> إضافة
              </Button>
            </div>

            {lines.filter(l => l.accountID > 0 && (l.balance ?? 0) !== 0 && !assets.some(a => a.accountID === l.accountID) && !inventoryItems.some(i => i.accountID === l.accountID)).length === 0 ? (
              <div className="text-center py-6 text-muted-foreground text-sm">
                لم تُضف أي حسابات أخرى بعد. اختر حساباً وأدخل الرصيد ثم اضغط "إضافة".
              </div>
            ) : (
              <div className="space-y-3">
                {lines.filter(l => l.accountID > 0 && (l.balance ?? 0) !== 0 && !assets.some(a => a.accountID === l.accountID) && !inventoryItems.some(i => i.accountID === l.accountID)).map((line) => (
                  <div key={line.accountID} className="flex items-center justify-between p-4 bg-muted/30 rounded-lg border border-primary">
                    <div className="flex items-center gap-4">
                      <div className="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
                        <BookOpen className="w-5 h-5 text-primary" />
                      </div>
                      <div>
                        <div className="font-semibold flex items-center gap-2">
                          <span className="text-xs font-mono text-muted-foreground bg-primary/10 px-2 py-0.5 rounded">{line.accountCode}</span>
                          <span>{line.accountName}</span>
                        </div>
                      </div>
                    </div>
                    <div className="flex items-center gap-4">
                      <div className="text-left">
                        <div className="text-lg font-bold font-mono" dir="ltr">{(line.balance || 0).toLocaleString()}</div>
                        <div className="text-xs text-muted-foreground">ر.ي</div>
                      </div>
                      <Button variant="ghost" size="icon" className="text-destructive" onClick={() => handleRemoveOtherAccount(line.accountID)}>
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>
        </TabsContent>

        {/* ═══ Compound Entry Tab ═══ */}
        <TabsContent value="compound">
          <Card className="p-4">
            <Heading level="h3" className="text-lg font-semibold mb-4">القيد المركب</Heading>
            <Text variant="description" className="mb-4">عرض شامل لجميع الحسابات المتأثرة: الأصول الثابتة والمخزون والحسابات الأخرى</Text>

            {lines.filter(l => l.accountID > 0 && (l.balance ?? 0) !== 0).length === 0 ? (
              <div className="text-center py-6 text-muted-foreground text-sm">
                لا توجد حسابات في القيد بعد. أضف أصولاً أو مخزوناً أو حسابات أخرى.
              </div>
            ) : (
              <div className="space-y-2">
                {/* Header */}
                <div className="grid grid-cols-[1fr_100px_120px_120px] gap-4 px-4 py-2 text-xs font-medium text-muted-foreground border-b">
                  <span>الحساب</span>
                  <span className="text-center">الطبيعة</span>
                  <span className="text-left">مدين</span>
                  <span className="text-left">دائن</span>
                </div>

                {/* Asset Lines */}
                {assets.length > 0 && (
                  <>
                    <div className="flex items-center gap-2 px-4 pt-2 text-xs font-semibold text-primary">
                      <Wrench className="w-3 h-3" /> الأصول الثابتة
                    </div>
                    {lines.filter(l => l.accountID > 0 && (l.balance ?? 0) !== 0 && assets.some(a => a.accountID === l.accountID)).map((line) => {
                      const asset = assets.find(a => a.accountID === line.accountID);
                      return (
                        <div key={`asset-${line.accountID}`} className="grid grid-cols-[1fr_100px_120px_120px] gap-4 px-4 py-3 bg-muted/20 rounded-lg items-center">
                          <div>
                            <div className="font-medium text-sm flex items-center gap-2">
                              <span className="font-mono text-xs text-muted-foreground">{line.accountCode}</span>
                              <span>{line.accountName}</span>
                              {asset?.assetName && <span className="text-xs text-muted-foreground">— {asset.assetName}</span>}
                            </div>
                          </div>
                          <span className="text-center text-xs">{line.accountNature === "Debit" ? "مدين" : "دائن"}</span>
                          <div className="font-mono text-sm font-bold" dir="ltr">
                            {line.accountNature === "Debit" ? (line.balance ?? 0).toLocaleString() : ""}
                          </div>
                          <div className="font-mono text-sm font-bold" dir="ltr">
                            {line.accountNature === "Credit" ? (line.balance ?? 0).toLocaleString() : ""}
                          </div>
                        </div>
                      );
                    })}
                  </>
                )}

                {/* Inventory Lines */}
                {inventoryItems.length > 0 && (
                  <>
                    <div className="flex items-center gap-2 px-4 pt-2 text-xs font-semibold text-primary">
                      <Package className="w-3 h-3" /> المخزون
                    </div>
                    {lines.filter(l => l.accountID > 0 && (l.balance ?? 0) !== 0 && inventoryItems.some(i => i.accountID === l.accountID)).map((line) => {
                      const inv = inventoryItems.find(i => i.accountID === line.accountID);
                      return (
                        <div key={`inv-${line.accountID}`} className="grid grid-cols-[1fr_100px_120px_120px] gap-4 px-4 py-3 bg-muted/20 rounded-lg items-center">
                          <div>
                            <div className="font-medium text-sm flex items-center gap-2">
                              <span className="font-mono text-xs text-muted-foreground">{line.accountCode}</span>
                              <span>{line.accountName}</span>
                              {inv?.itemName && <span className="text-xs text-muted-foreground">— {inv.itemName}</span>}
                            </div>
                          </div>
                          <span className="text-center text-xs">{line.accountNature === "Debit" ? "مدين" : "دائن"}</span>
                          <div className="font-mono text-sm font-bold" dir="ltr">
                            {line.accountNature === "Debit" ? (line.balance ?? 0).toLocaleString() : ""}
                          </div>
                          <div className="font-mono text-sm font-bold" dir="ltr">
                            {line.accountNature === "Credit" ? (line.balance ?? 0).toLocaleString() : ""}
                          </div>
                        </div>
                      );
                    })}
                  </>
                )}

                {/* Other Lines */}
                {(() => {
                  const otherLines = lines.filter(l => l.accountID > 0 && (l.balance ?? 0) !== 0 && !assets.some(a => a.accountID === l.accountID) && !inventoryItems.some(i => i.accountID === l.accountID));
                  if (otherLines.length === 0) return null;
                  return (
                    <>
                      <div className="flex items-center gap-2 px-4 pt-2 text-xs font-semibold text-primary">
                        <BookOpen className="w-3 h-3" /> حسابات أخرى
                      </div>
                      {otherLines.map((line) => (
                        <div key={`other-${line.accountID}`} className="grid grid-cols-[1fr_100px_120px_120px] gap-4 px-4 py-3 bg-muted/20 rounded-lg items-center">
                          <div>
                            <div className="font-medium text-sm flex items-center gap-2">
                              <span className="font-mono text-xs text-muted-foreground">{line.accountCode}</span>
                              <span>{line.accountName}</span>
                            </div>
                          </div>
                          <span className="text-center text-xs">{line.accountNature === "Debit" ? "مدين" : "دائن"}</span>
                          <div className="font-mono text-sm font-bold" dir="ltr">
                            {line.accountNature === "Debit" ? (line.balance ?? 0).toLocaleString() : ""}
                          </div>
                          <div className="font-mono text-sm font-bold" dir="ltr">
                            {line.accountNature === "Credit" ? (line.balance ?? 0).toLocaleString() : ""}
                          </div>
                        </div>
                      ))}
                    </>
                  );
                })()}

                {/* Totals */}
                <div className="grid grid-cols-[1fr_100px_120px_120px] gap-4 px-4 py-3 bg-primary/5 rounded-lg items-center border-t-2 border-primary mt-2">
                  <div className="font-bold text-sm">الإجمالي</div>
                  <span></span>
                  <div className="font-mono text-sm font-bold" dir="ltr">{totalDebit.toLocaleString()}</div>
                  <div className="font-mono text-sm font-bold" dir="ltr">{totalCredit.toLocaleString()}</div>
                </div>
              </div>
            )}
          </Card>
        </TabsContent>
      </Tabs>

      {/* ═══ Summary ═══ */}
      <Card className={`p-4 ${isBalanced ? "bg-green-500/5" : "bg-red-500/5"}`}>
        <div className="flex flex-col md:flex-row items-center justify-between gap-4">
          <div className="flex flex-wrap gap-6 text-sm">
            <span>إجمالي المدين: <strong className="font-mono text-lg">{totalDebit.toLocaleString()}</strong></span>
            <span>إجمالي الدائن: <strong className="font-mono text-lg">{totalCredit.toLocaleString()}</strong></span>
          </div>
          <span className={`text-lg font-bold ${isBalanced ? "text-green-600" : "text-red-600"}`}>
            {isBalanced ? "متوازن" : "غير متوازن"}
          </span>
        </div>
      </Card>

      {/* ═══ Actions ═══ */}
      <div className="flex justify-end gap-3">
        <Button
          onClick={() => mutation.mutate()}
          disabled={!fiscalYearID || lines.length === 0 || !isBalanced || mutation.isPending}
          className="px-8"
        >
          {mutation.isPending ? "جاري الترحيل..." : "ترحيل الأرصدة الافتتاحية"}
        </Button>
      </div>

      {/* ═══ Asset Form Dialog ═══ */}
      <FixedAssetFormDialog
        open={showAssetForm}
        onOpenChange={setShowAssetForm}
        mode="create"
        skipJournalEntry
        onSuccess={(asset) => {
          if (asset) {
            handleAddAsset({
              assetID: asset.assetID,
              assetAccountID: asset.assetAccountID,
              unitPrice: asset.unitPrice,
              quantity: asset.quantity,
              assetName: asset.assetName,
            });
          }
        }}
      />
    </PageLayout>

    {/* ═══ Reset All Confirm Dialog ═══ */}
    <ConfirmDialog
      open={showResetConfirm}
      onOpenChange={setShowResetConfirm}
      title="تصفير جميع البيانات"
      description="هل أنت متأكد من تصفير جميع البيانات المحاسبية والأصول والمخزون؟ سيُحذف كل شيء ولن يمكن التراجع."
      confirmLabel="نعم، تصفير الكل"
      cancelLabel="إلغاء"
      variant="danger"
      isLoading={resetAllMutation.isPending}
      onConfirm={() => resetAllMutation.mutate()}
    />

    {/* ═══ Seed Initial Defaults Dialog ═══ */}
    <Dialog open={showSeedDialog} onOpenChange={(o) => { setShowSeedDialog(o); if (!o) setSeedFiscalYearID(null); }}>
      <DialogContent className="sm:max-w-[500px]" dir="rtl">
        <DialogHeader>
          <DialogTitle>إدراج بيانات أولية افتراضية</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 py-2">
          <Text variant="description">
            سيتم إدراج الأصول الثابتة (مبنى، مركبة، أجهزة) ورصيد البنك المركزي (حساب 18211 أصل و256 التزام) للسنة المالية المختارة. هذا الإجراء idempotent ولا يُكرر البيانات الموجودة.
          </Text>
          <div className="space-y-1.5">
            <Text variant="label">السنة المالية</Text>
            <Select value={String(seedFiscalYearID ?? "")} onValueChange={(v) => setSeedFiscalYearID(Number(v))}>
              <SelectTrigger>
                <SelectValue placeholder="اختر السنة المالية" />
              </SelectTrigger>
              <SelectContent>
                {fiscalYears?.filter((y) => !y.isClosed).map((fy) => (
                  <SelectItem key={fy.fiscalYearID} value={String(fy.fiscalYearID)}>
                    {fy.yearName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
        <DialogFooter className="gap-2">
          <Button variant="outline" onClick={() => { setShowSeedDialog(false); setSeedFiscalYearID(null); }}>إلغاء</Button>
          <Button onClick={() => seedDefaultsMutation.mutate()} disabled={!seedFiscalYearID || seedDefaultsMutation.isPending}>
            {seedDefaultsMutation.isPending ? "جاري الإدراج..." : "إدراج البيانات"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
    </>
  );
}
