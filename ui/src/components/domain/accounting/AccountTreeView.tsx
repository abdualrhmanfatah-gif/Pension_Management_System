import { ChevronDown, ChevronLeft, FileText, FolderOpen } from "lucide-react";
import { useState, type MouseEvent } from "react";

import type { AccountTreeNode } from "@/types/accounting";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui";

interface AccountTreeViewProps {
  accounts: AccountTreeNode[];
  selectedId?: number | null;
  onSelect?: (account: AccountTreeNode) => void;
  className?: string;
}

function TreeNode({
  node,
  depth,
  selectedId,
  onSelect,
}: {
  node: AccountTreeNode;
  depth: number;
  selectedId?: number | null;
  onSelect?: (account: AccountTreeNode) => void;
}) {
  const [expanded, setExpanded] = useState(depth < 2);
  const hasChildren = node.children && node.children.length > 0;
  const isSelected = selectedId === node.accountID;

  return (
    <div>
      <div
        className={cn(
          "flex items-center gap-2 px-3 py-1.5 cursor-pointer rounded-lg transition-colors hover:bg-accent/50 text-sm",
          depth > 0 && "mr-4",
          isSelected && "bg-primary/10 text-primary font-medium"
        )}
        style={{ marginRight: `${depth * 1.25}rem` }}
        onClick={() => onSelect?.(node)}
      >
        {hasChildren ? (
          <Button
            variant="ghost"
            size="icon"
            className="h-5 w-5"
            onClick={(e: MouseEvent) => { e.stopPropagation(); setExpanded(!expanded); }}
          >
            {expanded ? <ChevronDown className="w-3.5 h-3.5" /> : <ChevronLeft className="w-3.5 h-3.5" />}
          </Button>
        ) : (
          <span className="w-5" />
        )}
        {hasChildren ? (
          <FolderOpen className={cn("w-4 h-4 shrink-0", expanded ? "text-amber-500" : "text-muted-foreground")} />
        ) : (
          <FileText className="w-4 h-4 shrink-0 text-muted-foreground" />
        )}
        <span className="text-xs text-muted-foreground font-mono ltr" dir="ltr">{node.accountCode}</span>
        <span className="text-sm">{node.accountName}</span>
        <span className={cn(
          "text-xs px-1.5 py-0.5 rounded-full",
          node.accountNature === "Debit" ? "bg-blue-100 text-blue-700" : "bg-green-100 text-green-700"
        )}>
          {node.accountNature === "Debit" ? "مدين" : "دائن"}
        </span>
        {!node.isActive && (
          <span className="text-xs px-1.5 py-0.5 rounded-full bg-destructive/10 text-destructive">غير نشط</span>
        )}
      </div>
      {hasChildren && expanded && (
        <div>
          {node.children.map((child) => (
            <TreeNode key={child.accountID} node={child} depth={depth + 1} selectedId={selectedId} onSelect={onSelect} />
          ))}
        </div>
      )}
    </div>
  );
}

export function AccountTreeView({ accounts, selectedId, onSelect, className }: AccountTreeViewProps) {
  if (!accounts || accounts.length === 0) {
    return (
      <div className="flex items-center justify-center h-32 text-muted-foreground text-sm">
        لا توجد حسابات
      </div>
    );
  }

  return (
    <div className={cn("space-y-0.5", className)} dir="rtl">
      {accounts.map((account) => (
        <TreeNode key={account.accountID} node={account} depth={0} selectedId={selectedId} onSelect={onSelect} />
      ))}
    </div>
  );
}
