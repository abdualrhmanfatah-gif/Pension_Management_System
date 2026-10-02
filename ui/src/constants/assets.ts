/**
 * ثوابت وحدة الأصول الثابتة
 */

export const ASSET_TYPE_ICONS: Record<number, string> = {
  0: "Laptop",
  1: "Truck",
  2: "Building2",
  3: "LandPlot",
  4: "Sofa",
};

export const ASSET_STATUS_CONFIG: Record<number, { label: string; variant: "success" | "outline" | "warning" | "destructive" }> = {
  0: { label: "نشط", variant: "success" },
  1: { label: "متوقف", variant: "outline" },
  2: { label: "متخلص", variant: "destructive" },
  3: { label: "تحت الصيانة", variant: "warning" },
};

export const ASSET_STATUS_LABELS: Record<number, string> = {
  0: "نشط",
  1: "متوقف",
  2: "متخلص",
  3: "تحت الصيانة",
};

export const DEPRECIATION_METHOD_LABELS: Record<number, string> = {
  0: "الخط المستقيم",
  1: "الرصيد المتناقص",
  2: "وحدات الإنتاج",
};
