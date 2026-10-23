import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FolderTree, PlusCircle, RefreshCw, Trash2 } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";

import { costCentersApi } from "@/api/accounting";
import { CostCenterFormDialog } from "@/components/domain/accounting";
import { Button, Card, Heading, PageLayout, Skeleton, StickyHeader,useConfirm } from "@/components/ui";
import { useHasPermission } from "@/stores/authStore";
import type { CostCenterDto } from "@/types/accounting";

export function CostCentersPage() {
  const queryClient = useQueryClient();
  const { confirm, ConfirmDialogElement } = useConfirm();
  const canManage = useHasPermission("Accounting.Manage", "System.Admin");
  const [showForm, setShowForm] = useState(false);
  const [editingCC, setEditingCC] = useState<CostCenterDto | null>(null);

  const { data: costCenters, isLoading, error, refetch } = useQuery({
    queryKey: ["cost-centers"],
    queryFn: () => costCentersApi.getAll(),
  });

  const deactivateMutation = useMutation({
    mutationFn: (id: number) => costCentersApi.deactivate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["cost-centers"] });
      toast.success("تم إلغاء تنشيط مركز التكلفة");
    },
    onError: (err: Error) => toast.error(err.message || "فشل في إلغاء التنشيط"),
  });

  const handleEdit = (cc: CostCenterDto) => {
    setEditingCC(cc);
    setShowForm(true);
  };

  const handleDeactivate = async (cc: CostCenterDto) => {
    const verified = await confirm({
      title: "إلغاء تنشيط مركز التكلفة",
      description: `هل أنت متأكد من إلغاء تنشيط "${cc.name}"؟`,
      confirmLabel: "إلغاء التنشيط",
      variant: "danger",
    });
    if (verified) {
      deactivateMutation.mutate(cc.costCenterID);
    }
  };

  // Build a simple tree from flat list
  type TreeNode = CostCenterDto & { children: TreeNode[] };
  const buildTree = (items: CostCenterDto[]): TreeNode[] => {
    const map = new Map<number, TreeNode>();
    const roots: TreeNode[] = [];

    items.forEach((item) => map.set(item.costCenterID, { ...item, children: [] }));
    items.forEach((item) => {
      const node = map.get(item.costCenterID)!;
      if (item.parentCostCenterID && map.has(item.parentCostCenterID)) {
        map.get(item.parentCostCenterID)!.children.push(node);
      } else {
        roots.push(node);
      }
    });

    return roots;
  };

  const renderTree = (nodes: TreeNode[], depth = 0) => {
    return nodes.map((node) => (
      <div key={node.costCenterID}>
        <div
          className="flex items-center justify-between px-4 py-2.5 hover:bg-accent/50 transition-colors border-b border-border"
          style={{ marginRight: `${depth * 1.5}rem` }}
        >
          <div className="flex items-center gap-2">
            <FolderTree className="w-4 h-4 text-muted-foreground shrink-0" />
            <span className="font-mono text-xs text-muted-foreground">{node.code}</span>
            <span className={node.isActive ? "" : "text-destructive line-through"}>{node.name}</span>
            {!node.isActive && (
              <span className="text-xs px-1.5 py-0.5 rounded-full bg-destructive/10 text-destructive">غير نشط</span>
            )}
          </div>
          {canManage && (
            <div className="flex items-center gap-1">
              <Button variant="ghost" size="sm" onClick={() => handleEdit(node)}>
                تعديل
              </Button>
              {node.isActive && (
                <Button variant="ghost" size="sm" onClick={() => handleDeactivate(node)} className="text-destructive">
                  <Trash2 className="w-3.5 h-3.5" />
                </Button>
              )}
            </div>
          )}
        </div>
        {node.children.length > 0 && renderTree(node.children, depth + 1)}
      </div>
    ));
  };

  return (
    <PageLayout>
      <StickyHeader>
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className="p-3 bg-primary/10 rounded-2xl">
              <FolderTree className="w-8 h-8 text-primary" />
            </div>
            <div>
              <Heading level="h1" className="text-xl md:text-3xl font-bold tracking-tight">مراكز التكلفة</Heading>
              <p className="text-muted-foreground mt-1">تصنيف المصروفات والإيرادات حسب الوحدات التنظيمية</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <Button variant="outline" onClick={() => refetch()} className="gap-2">
              <RefreshCw className="w-4 h-4" /> تحديث
            </Button>
            {canManage && (
              <Button onClick={() => { setEditingCC(null); setShowForm(true); }} className="gap-2">
                <PlusCircle className="w-4 h-4" /> إضافة مركز تكلفة
              </Button>
            )}
          </div>
        </div>
      </StickyHeader>

      {isLoading ? (
        <Skeleton className="h-64 rounded-3xl" />
      ) : error ? (
        <div className="text-destructive text-center p-8">فشل في تحميل مراكز التكلفة</div>
      ) : !costCenters || costCenters.length === 0 ? (
        <Card className="border-none rounded-3xl p-4 sm:p-8 lg:p-12 text-center text-muted-foreground">
          لا توجد مراكز تكلفة
        </Card>
      ) : (
        <Card className="border-none rounded-3xl overflow-hidden">
          {renderTree(buildTree(costCenters))}
        </Card>
      )}

      <CostCenterFormDialog open={showForm} onOpenChange={setShowForm} costCenter={editingCC} />
      {ConfirmDialogElement}
    </PageLayout>
  );
}
