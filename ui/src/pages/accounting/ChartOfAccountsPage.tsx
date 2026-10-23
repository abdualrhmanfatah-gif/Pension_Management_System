import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { BookOpen, FileText, FolderTree, GitFork, Hash, Layers, Pencil, PlusCircle, RefreshCw, RotateCcw, Scale, Search, Tag, ToggleLeft,Trash2 } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

import { accountsApi } from "@/api/accounting";
import { AccountFormDialog, AccountTreeView } from "@/components/domain/accounting";
import { Badge, Button, Card, ErrorState, Heading, Input, PageLayout, Skeleton, StickyHeader,Text, useConfirm } from "@/components/ui";
import { BADGE_VARIANT } from "@/constants";
import { useHasPermission } from "@/stores/authStore";
import type { AccountTreeNode, ChartOfAccountDto } from "@/types/accounting";
import { ACCOUNT_CATEGORY_AR } from "@/types/accounting";

export function ChartOfAccountsPage() {
  const queryClient = useQueryClient();
  const { confirm, ConfirmDialogElement } = useConfirm();
  const canManage = useHasPermission("Accounting.Manage", "System.Admin");
  const [search, setSearch] = useState("");
  const [selectedAccount, setSelectedAccount] = useState<ChartOfAccountDto | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editingAccount, setEditingAccount] = useState<ChartOfAccountDto | null>(null);
  const [parentAccount, setParentAccount] = useState<ChartOfAccountDto | null>(null);

  const { data: tree, isLoading, error, refetch } = useQuery({
    queryKey: ["accounts", "tree"],
    queryFn: () => accountsApi.getTree(),
  });

  const deactivateMutation = useMutation({
    mutationFn: async (id: number) => {
      // Collect all descendant IDs from the tree
      const collectDescendants = (node: AccountTreeNode): number[] => {
        const ids: number[] = [];
        if (node.children) {
          for (const child of node.children) {
            ids.push(child.accountID);
            ids.push(...collectDescendants(child));
          }
        }
        return ids;
      };

      // Find the node in the tree
      const findNode = (nodes: AccountTreeNode[]): AccountTreeNode | null => {
        for (const n of nodes) {
          if (n.accountID === id) return n;
          const found = n.children ? findNode(n.children) : null;
          if (found) return found;
        }
        return null;
      };

      const currentNode = tree ? findNode(tree) : null;
      const descendantIds = currentNode ? collectDescendants(currentNode) : [];

      // Deactivate parent first, then all descendants
      await accountsApi.deactivate(id);
      for (const childId of descendantIds) {
        await accountsApi.deactivate(childId);
      }
      return descendantIds.length;
    },
    onSuccess: (childCount) => {
      queryClient.invalidateQueries({ queryKey: ["accounts"] });
      if (childCount > 0) {
        toast.success(`تم إلغاء تنشيط الحساب و${childCount} حساب فرعي`);
      } else {
        toast.success("تم إلغاء تنشيط الحساب بنجاح");
      }
    },
    onError: (err: Error) => toast.error(err.message || "فشل في إلغاء تنشيط الحساب"),
  });

  const activateMutation = useMutation({
    mutationFn: (id: number) => accountsApi.activate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["accounts"] });
      toast.success("تم تنشيط الحساب بنجاح");
    },
    onError: (err: Error) => toast.error(err.message || "فشل في تنشيط الحساب"),
  });

  const resetMutation = useMutation({
    mutationFn: () => accountsApi.reset(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["accounts"] });
      toast.success("تم إعادة تعيين الدليل المحاسبي بنجاح");
    },
    onError: (err: Error) => toast.error(err.message || "فشل في إعادة تعيين الدليل المحاسبي"),
  });

  const disableUnusedMutation = useMutation({
    mutationFn: () => accountsApi.disableUnused(),
    onSuccess: (res) => {
      queryClient.invalidateQueries({ queryKey: ["accounts"] });
      toast.success(res.message || "تم تعطيل الحسابات غير المستخدمة");
    },
    onError: (err: Error) => toast.error(err.message || "فشل في تعطيل الحسابات غير المستخدمة"),
  });

  const handleSelect = (node: AccountTreeNode) => {
    setSelectedAccount(node);
  };

  const handleAddChild = () => {
    if (selectedAccount) {
      setParentAccount(selectedAccount);
      setEditingAccount(null);
      setShowForm(true);
    }
  };

  const handleEdit = () => {
    if (selectedAccount) {
      setEditingAccount(selectedAccount);
      setParentAccount(null);
      setShowForm(true);
    }
  };

  const handleDeactivate = async () => {
    if (!selectedAccount) return;
    const verified = await confirm({
      title: "إلغاء تنشيط الحساب",
      description: `هل أنت متأكد من إلغاء تنشيط الحساب "${selectedAccount.accountName}"؟`,
      confirmLabel: "إلغاء التنشيط",
      variant: "danger",
    });
    if (verified) {
      deactivateMutation.mutate(selectedAccount.accountID);
    }
  };

  const handleActivate = () => {
    if (selectedAccount) {
      activateMutation.mutate(selectedAccount.accountID);
    }
  };

  const handleCloseForm = () => {
    setShowForm(false);
    setEditingAccount(null);
    setParentAccount(null);
  };

  // Filter tree by search
  const filterTree = (nodes: AccountTreeNode[]): AccountTreeNode[] => {
    if (!search.trim()) return nodes;
    return nodes
      .map((node) => {
        const matches = node.accountName.includes(search) || node.accountCode.includes(search);
        const filteredChildren = node.children ? filterTree(node.children) : [];
        if (matches || filteredChildren.length > 0) {
          return { ...node, children: filteredChildren };
        }
        return null;
      })
      .filter(Boolean) as AccountTreeNode[];
  };

  if (isLoading) {
    return (
      <div className="p-8 space-y-8" dir="rtl">
        <Skeleton className="h-12 w-64 rounded-xl" />
        <Skeleton className="h-[600px] rounded-3xl" />
      </div>
    );
  }

  if (error) {
    return <ErrorState message="تعذر تحميل دليل الحسابات" />;
  }

  const filteredTree = tree ? filterTree(tree) : [];

  return (
    <PageLayout>
      {/* Header */}
      <StickyHeader>
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-primary/10 rounded-2xl">
              <BookOpen className="w-8 h-8 text-primary" />
            </div>
            <div>
              <Heading level="h1" className="text-xl md:text-3xl font-bold tracking-tight">دليل الحسابات</Heading>
              <p className="text-muted-foreground mt-1">الهيكل المحاسبي الموحد للصندوق</p>
            </div>
          </div>

          <div className="flex items-center gap-3 flex-wrap">
            <Button variant="outline" onClick={() => { refetch(); toast.success("تم تحديث البيانات"); }} className="gap-2">
              <RefreshCw className="w-4 h-4" />
              تحديث
            </Button>
            {canManage && (
              <>
                <Button 
                  variant="destructive" 
                  onClick={async () => {
                    const verified = await confirm({
                      title: "إعادة تعيين الدليل المحاسبي",
                      description: "هل أنت متأكد من إعادة تعيين الدليل المحاسبي؟ سيتم حذف جميع الحسابات الحالية وإعادة تعبئة الدليل الموحد من الصفر.",
                      confirmLabel: "إعادة التعيين",
                      variant: "danger",
                    });
                    if (verified) {
                      resetMutation.mutate();
                    }
                  }} 
                  className="gap-2"
                  disabled={resetMutation.isPending}
                >
                  <RotateCcw className="w-4 h-4" />
                  {resetMutation.isPending ? "جاري إعادة التعيين..." : "إعادة تعيين الدليل"}
                </Button>
                <Button 
                  variant="outline"
                  onClick={async () => {
                    const verified = await confirm({
                      title: "تعطيل الحسابات غير المستخدمة",
                      description: "هل تريد تعطيل الحسابات غير المستخدمة في صندوق التقاعد؟ سيتم إلغاء تنشيط الحسابات المتعلقة بالزراعة والثروات والتصنيع وغير المستخدمة.",
                      confirmLabel: "تعطيل",
                      variant: "danger",
                    });
                    if (verified) {
                      disableUnusedMutation.mutate();
                    }
                  }} 
                  className="gap-2"
                  disabled={disableUnusedMutation.isPending}
                >
                  <ToggleLeft className="w-4 h-4" />
                  {disableUnusedMutation.isPending ? "جاري التعطيل..." : "تعطيل حسابات غير مستخدمة"}
                </Button>
                <Button onClick={() => { setParentAccount(null); setEditingAccount(null); setShowForm(true); }} className="gap-2">
                  <PlusCircle className="w-4 h-4" />
                  إضافة حساب
                </Button>
              </>
            )}
          </div>
        </div>
      </StickyHeader>

      {/* Search */}
      <div className="relative max-w-sm">
        <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
        <Input
          placeholder="بحث في دليل الحسابات..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="pr-10 rounded-xl"
        />
      </div>

      {/* Main content */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-3">
        {/* Tree */}
        <Card className="lg:col-span-2 border-none rounded-3xl p-4 max-h-[70dvh] overflow-y-auto">
          <AccountTreeView accounts={filteredTree} selectedId={selectedAccount?.accountID} onSelect={handleSelect} />
        </Card>

        {/* Details panel */}
        <Card className="border-none rounded-3xl p-0 overflow-hidden">
          {selectedAccount ? (
            <div className="flex flex-col h-full">
              {/* Header with gradient accent */}
              <div className="bg-gradient-to-l from-primary/5 to-primary/10 p-5 border-b border-primary">
                <div className="flex items-start justify-between gap-3">
                  <div className="flex items-center gap-3 min-w-0">
                    <div className="p-2.5 bg-primary/10 rounded-xl shrink-0">
                      <BookOpen className="w-5 h-5 text-primary" />
                    </div>
                    <div className="min-w-0">
                      <Heading level="h3" className="font-semibold text-lg truncate">تفاصيل الحساب</Heading>
                      <Text variant="caption" className="font-mono ltr" dir="ltr">{selectedAccount.accountCode}</Text>
                    </div>
                  </div>
                  <Badge
                    variant={selectedAccount.isActive ? BADGE_VARIANT.SUCCESS : BADGE_VARIANT.DANGER}
                    dot
                    className="shrink-0"
                  >
                    {selectedAccount.isActive ? "نشط" : "غير نشط"}
                  </Badge>
                </div>
              </div>

              {/* Info fields */}
              <div className="p-4 divide-y divide-muted">
                <div className="flex items-center gap-2 py-2 text-sm">
                  <Tag className="w-3.5 h-3.5 text-muted-foreground" />
                  <span className="text-muted-foreground font-medium">الاسم</span>
                  <span className="font-medium mr-auto">{selectedAccount.accountName}</span>
                </div>
                <div className="flex items-center gap-2 py-2 text-sm">
                  <Scale className="w-3.5 h-3.5 text-muted-foreground" />
                  <span className="text-muted-foreground font-medium">الطبيعة</span>
                  <span className="font-medium mr-auto">{selectedAccount.accountNature === "Debit" ? "مدين" : "دائن"}</span>
                </div>
                <div className="flex items-center gap-2 py-2 text-sm">
                  <Layers className="w-3.5 h-3.5 text-muted-foreground" />
                  <span className="text-muted-foreground font-medium">التصنيف</span>
                  <span className="font-medium mr-auto">{ACCOUNT_CATEGORY_AR[selectedAccount.accountCategory as keyof typeof ACCOUNT_CATEGORY_AR] || selectedAccount.accountCategory}</span>
                </div>
                <div className="flex items-center gap-2 py-2 text-sm">
                  <Layers className="w-3.5 h-3.5 text-muted-foreground" />
                  <span className="text-muted-foreground font-medium">المستوى</span>
                  <span className="font-medium mr-auto">{selectedAccount.accountLevel}</span>
                </div>
                <div className="flex items-center gap-2 py-2 text-sm">
                  <Hash className="w-3.5 h-3.5 text-muted-foreground" />
                  <span className="text-muted-foreground font-medium">قابل للترحيل</span>
                  <span className="font-medium mr-auto">{selectedAccount.isPostable ? "نعم" : "لا"}</span>
                </div>
                {selectedAccount.parentAccountName && (
                  <div className="flex items-center gap-2 py-2 text-sm">
                    <FolderTree className="w-3.5 h-3.5 text-muted-foreground" />
                    <span className="text-muted-foreground font-medium">الحساب الأب</span>
                    <span className="font-medium mr-auto">{selectedAccount.parentAccountName}</span>
                  </div>
                )}
                {selectedAccount.description && (
                  <div className="flex items-center gap-2 py-2 text-sm">
                    <FileText className="w-3.5 h-3.5 text-muted-foreground" />
                    <span className="text-muted-foreground font-medium">الوصف</span>
                    <span className="font-medium mr-auto">{selectedAccount.description}</span>
                  </div>
                )}
                <div className="flex items-center gap-2 py-2 text-sm">
                  <GitFork className="w-3.5 h-3.5 text-muted-foreground" />
                  <span className="text-muted-foreground font-medium">الحسابات الفرعية</span>
                  <span className="font-medium mr-auto">{selectedAccount.subAccountsCount}</span>
                </div>
              </div>

              {/* Actions */}
              {canManage && (
                <div className="flex gap-2 p-4 border-t bg-muted/10">
                  <Button size="sm" variant="outline" onClick={handleEdit} className="gap-1.5">
                    <Pencil className="w-3.5 h-3.5" />
                    تعديل
                  </Button>
                  {selectedAccount.isPostable && (
                    <Button size="sm" variant="outline" onClick={handleAddChild} className="gap-1.5">
                      <PlusCircle className="w-3.5 h-3.5" />
                      إضافة حساب فرعي
                    </Button>
                  )}
                  {selectedAccount.isActive ? (
                    <Button size="sm" variant="destructive" onClick={handleDeactivate} className="gap-1.5 mr-auto">
                      <Trash2 className="w-3.5 h-3.5" />
                      إلغاء تنشيط
                    </Button>
                  ) : (
                    <Button size="sm" variant="primary" onClick={handleActivate} className="gap-1.5 mr-auto">
                      <RotateCcw className="w-3.5 h-3.5" />
                      تنشيط
                    </Button>
                  )}
                </div>
              )}
            </div>
          ) : (
            <div className="flex flex-col items-center justify-center h-48 text-muted-foreground">
              <BookOpen className="w-12 h-12 mb-3 opacity-100" />
              <Text variant="body">اختر حساباً من القائمة لعرض التفاصيل</Text>
            </div>
          )}
        </Card>
      </div>

      {/* Dialogs */}
      <AccountFormDialog
        open={showForm}
        onOpenChange={handleCloseForm}
        account={editingAccount}
        parentAccount={parentAccount}
      />
      {ConfirmDialogElement}
    </PageLayout>
  );
}
