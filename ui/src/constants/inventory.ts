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
  0: "from-emerald-500/10 via-emerald-50/80 to-background border-emerald-200/60",
  1: "from-red-500/10 via-red-50/80 to-background border-red-200/60",
  2: "from-blue-500/10 via-blue-50/80 to-background border-blue-200/60",
  3: "from-amber-500/10 via-amber-50/80 to-background border-amber-200/60",
  4: "from-slate-500/10 via-slate-50/80 to-background border-slate-200/60",
};

export const TRANSACTION_TYPE_ACCENT_COLORS: Record<number, string> = {
  0: "bg-emerald-500/10 text-emerald-700",
  1: "bg-red-500/10 text-red-700",
  2: "bg-blue-500/10 text-blue-700",
  3: "bg-amber-500/10 text-amber-700",
  4: "bg-slate-500/10 text-slate-700",
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
