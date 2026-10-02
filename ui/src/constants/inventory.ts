/**
 * ثوابت وحدة المخزون
 */

export const INVENTORY_TRANSACTION_TYPE: Record<number, { label: string; variant: "success" | "destructive" | "warning" | "info" | "outline" }> = {
  0: { label: "إدخال", variant: "success" },
  1: { label: "إخراج", variant: "destructive" },
  2: { label: "نقل", variant: "info" },
  3: { label: "إتلاف", variant: "warning" },
  4: { label: "تسوية جرد", variant: "outline" },
};

export const TRANSACTION_TYPE_GRADIENTS: Record<number, string> = {
  0: "from-success/10 via-success/5 to-background border-success/30",
  1: "from-danger/10 via-danger/5 to-background border-danger/30",
  2: "from-info/10 via-info/5 to-background border-info/30",
  3: "from-warning/10 via-warning/5 to-background border-warning/30",
  4: "from-muted/10 via-muted/5 to-background border-border",
};

export const TRANSACTION_TYPE_ACCENT_COLORS: Record<number, string> = {
  0: "bg-success/10 text-success",
  1: "bg-danger/10 text-danger",
  2: "bg-info/10 text-info",
  3: "bg-warning/10 text-warning",
  4: "bg-muted text-muted-foreground",
};

export const INVENTORY_ITEM_STATUS: Record<number, { label: string; variant: "success" | "outline" | "warning" | "destructive" }> = {
  0: { label: "نشط", variant: "success" },
  1: { label: "غير نشط", variant: "outline" },
  2: { label: "تحت الصيانة", variant: "warning" },
  3: { label: "تم التخلص", variant: "destructive" },
};

export const INVENTORY_ITEM_CONDITION: Record<number, { label: string; variant: "info" | "secondary" }> = {
  0: { label: "جديد", variant: "info" },
  1: { label: "مستعمل", variant: "secondary" },
};

export const INVENTORY_CUSTODY_STATUS: Record<number, { label: string; variant: "success" | "outline" | "warning" | "destructive" }> = {
  0: { label: "نشطة", variant: "success" },
  1: { label: "مُرجعَة", variant: "outline" },
  2: { label: "تالفة", variant: "warning" },
  3: { label: "مفقودة", variant: "destructive" },
};
