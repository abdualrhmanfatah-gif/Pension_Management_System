import {
  Activity,
  AlertTriangle,
  Archive,
  BadgeCheck,
  BadgeDollarSign,
  Ban,
  Banknote,
  BarChart3,
  BellRing,
  BookOpen,
  Boxes,
  Briefcase,
  Building,
  Building2,
  Calculator,
  Calendar,
  CalendarCheck,
  CalendarRange,
  CircleCheck,
  ClipboardCheck,
  ClipboardList,
  Coins,
  CreditCard,
  Crown,
  DollarSign,
  DoorOpen,
  FileCheck,
  FileClock,
  FileQuestion,
  Files,
  FileSearch,
  FileSpreadsheet,
  FileType,
  FileWarning,
  Fingerprint,
  FolderOpen,
  Folders,
  FolderSearch,
  GitCompare,
  Globe,
  HandCoins,
  Handshake,
  HeartHandshake,
  History,
  IdCard,
  Inbox,
  Landmark,
  Layers,
  LayoutDashboard,
  LayoutGrid,
  LineChart,
  ListChecks,
  ListOrdered,
  ListTree,
  Medal,
  Network,
  Package,
  Palmtree,
  PauseCircle,
  Percent,
  PieChart,
  Radar,
  Receipt,
  Rocket,
  Route,
  Scale,
  ScrollText,
  Search,
  Send,
  Settings,
  Shield,
  ShieldAlert,
  Sigma,
  SlidersHorizontal,
  Tags,
  Target,
  TrendingUp,
  Upload,
  UserCheck,
  UserCog,
  Users,
  UserSearch,
  UsersRound,
  Vault,
  Wallet,
  Wrench,
} from "lucide-react";
import type { ReactNode } from "react";

const ICON = "h-[18px] w-[18px]";

// ─── Types ───────────────────────────────────────────────

export interface NavItem {
  label: string;
  path: string;
  icon: ReactNode;
  requiredPermission?: string;
  anyPermission?: string[];
  group: NavGroup;
  subGroup?: string;
  nestedIn?: string;
}

// ─── Groups (حسب قرار وزير الداخلية رقم 42 لسنة 1997م) ───

export const groups = [
  "director",      // المدير العام ونائبه ومكتبه
  "pension",       // إدارة المعاشات 
  "finance",       // إدارة الشئون المالية
  "hr",            // إدارة الشئون الإدارية
  "investment",    // إدارة الاستثمار
  "config",        // الإعدادات العامة
] as const;

export type NavGroup = (typeof groups)[number];

// ─── Labels & Icons ──────────────────────────────────────

export const groupLabels: Record<NavGroup, string> = {
  director: "المدير العام",
  pension: "إدارة المعاشات",
  finance: "الإدارة المالية",
  hr: "إدارة الشئون الإدارية",
  investment: "إدارة الاستثمار",
  config: "الإعدادات",
};

export const groupIcons: Record<NavGroup, ReactNode> = {
  director: <Crown className="h-4 w-4" />,
  pension: <Landmark className="h-4 w-4" />,
  finance: <Coins className="h-4 w-4" />,
  hr: <Briefcase className="h-4 w-4" />,
  investment: <TrendingUp className="h-4 w-4" />,
  config: <Settings className="h-4 w-4" />,
};

// ─── Sub-Group Labels (حسب القرار الوزاري) ───────────────

export const subGroupLabels: Record<string, string> = {
  // ── إدارة المعاشات  ──
  "social-research": "قسم البحث الاجتماعي والفحص",
  "settlement-link": "قسم التسوية والربط",
  entitlements: "قسم الاستحقاق",
  "records-stats": "قسم السجلات والإحصاء",
  documentation: "قسم التوثيق",
  reports: "التقارير",

  // ── إدارة الشئون المالية ──
  "budget-accounts": "قسم الميزانية والحسابات",
  "expenses-payments": "قسم المصروفات والمدفوعات",
  "audit-control": "قسم المراجعة والرقابة المالية",
  "procurement-stores": "قسم المشتريات والمخازن",
  treasury: "قسم الخزينة",

  // ── إدارة الشئون الإدارية ──
  "admin-affairs": "قسم الشئون المالية والإدارية",

  // ── إدارة الاستثمار ──
  studies: "قسم الدراسات والجدوى الاقتصادية",
  designs: "قسم التصاميم",
  "commercial-relations": "قسم العلاقات التجارية",
  "execution-followup": "قسم التنفيذ والمتابعة",

  // ── الإعدادات ──
  audit: "الإعدادات والرقابة",
};

// ─── Navigation Data ───────────────────────────────────────

export const navItems: NavItem[] = [
  // ═══════════════════════════════════════════════════════
  //  المدير العام ونائبه ومكتبه
  // ═══════════════════════════════════════════════════════
  {
    label: "مكتب المدير العام",
    path: "/director-office",
    icon: <LayoutDashboard className={ICON} />,
    requiredPermission: "System.Admin",
    group: "director",
  },
  {
    label: "سجل الصادر",
    path: "/correspondence/outgoing",
    icon: <Send className={ICON} />,
    anyPermission: ["Correspondence.View", "System.Admin"],
    group: "director",
  },
  {
    label: "سجل الوارد",
    path: "/correspondence/incoming",
    icon: <Inbox className={ICON} />,
    anyPermission: ["Correspondence.View", "System.Admin"],
    group: "director",
  },
  {
    label: "الإنجازات والمعوقات",
    path: "/achievements",
    icon: <Target className={ICON} />,
    anyPermission: ["AchievementObstacle.View", "System.Admin"],
    group: "director",
  },

  // ═══════════════════════════════════════════════════════
  //  إدارة المعاشات 
  // ═══════════════════════════════════════════════════════

  // قسم التوثيق
  {
    label: "استكمال النواقص",
    path: "/documents",
    icon: <ListChecks className={ICON} />,
    requiredPermission: "File.Documents.Upload",
    group: "pension",
    subGroup: "documentation",
  },
  {
    label: "الأرشيف",
    path: "/archive",
    icon: <Archive className={ICON} />,
    anyPermission: ["File.View", "System.Admin"],
    group: "pension",
    subGroup: "documentation",
  },
  {
    label: "مراجعة الوثائق الأرشيف",
    path: "/archive/document-review",
    icon: <FolderSearch className={ICON} />,
    anyPermission: ["File.View", "System.Admin"],
    group: "pension",
    subGroup: "documentation",
  },

  // قسم البحث الاجتماعي والفحص
  {
    label: "المتقاعدون",
    path: "/retirees",
    icon: <UserCheck className={ICON} />,
    anyPermission: [
      "Retiree.View",
      "File.View.Sensitive",
      "File.Create",
      "File.Edit.BasicData",
      "File.Documents.Upload",
      "File.Edit.FinancialData",
      "File.ChangeStatus.Approve",
    ],
    group: "pension",
    subGroup: "social-research",
  },
  {
    label: "البحث الاجتماعي",
    path: "/pension/social-research",
    icon: <Radar className={ICON} />,
    anyPermission: ["Retiree.View", "System.Admin"],
    group: "pension",
    subGroup: "social-research",
  },
  {
    label: "مراجعة البيانات الناقصة",
    path: "/pension/document-review",
    icon: <FileQuestion className={ICON} />,
    anyPermission: ["File.Documents.Upload", "System.Admin"],
    group: "pension",
    subGroup: "social-research",
  },
  {
    label: "مراجعة ملفات التقاعد",
    path: "/pension/file-review",
    icon: <FileSearch className={ICON} />,
    anyPermission: ["Retiree.View", "System.Admin"],
    group: "pension",
    subGroup: "social-research",
  },

  // قسم التسوية والربط
  {
    label: "إدارة كشوفات الإحاله",
    path: "/pension/ministry-batches",
    icon: <FileSpreadsheet className={ICON} />,
    anyPermission: ["Ministry.View", "System.Admin"],
    group: "pension",
    subGroup: "settlement-link",
  },
  {
    label: "فحص المحالين",
    path: "/pension/ministry-referrals",
    icon: <UserSearch className={ICON} />,
    anyPermission: ["Ministry.View", "System.Admin"],
    group: "pension",
    subGroup: "settlement-link",
  },
  {
    label: "البحث عن محال",
    path: "/pension/ministry-search",
    icon: <Search className={ICON} />,
    anyPermission: ["Ministry.View", "System.Admin"],
    group: "pension",
    subGroup: "settlement-link",
  },
  {
    label: "التسويات المالية",
    path: "/settlements",
    icon: <Scale className={ICON} />,
    anyPermission: [
      "Settlement.View",
      "Settlement.Manage",
      "File.Edit.FinancialData",
      "System.Admin",
    ],
    group: "pension",
    subGroup: "settlement-link",
  },
  {
    label: "مطابقة كشوفات المعاش",
    path: "/pension/salary-reconciliation",
    icon: <GitCompare className={ICON} />,
    anyPermission: ["SalaryBatch.View", "System.Admin"],
    group: "pension",
    subGroup: "settlement-link",
  },

  // قسم الاستحقاق
  {
    label: "ملفات التقاعد",
    path: "/files",
    icon: <FolderOpen className={ICON} />,
    anyPermission: [
      "File.View.Sensitive",
      "File.Create",
      "File.Edit.BasicData",
      "File.Documents.Upload",
      "File.Edit.FinancialData",
      "File.ChangeStatus.Approve",
    ],
    group: "pension",
    subGroup: "entitlements",
  },
  {
    label: "تحديث بيانات المستحقين",
    path: "/pension/entitled-persons",
    icon: <UsersRound className={ICON} />,
    anyPermission: ["File.View.Sensitive", "System.Admin"],
    group: "pension",
    subGroup: "entitlements",
  },
  {
    label: "إيقاف وإعادة المعاش",
    path: "/pension/suspensions",
    icon: <PauseCircle className={ICON} />,
    anyPermission: ["File.View.Sensitive", "System.Admin"],
    group: "pension",
    subGroup: "entitlements",
  },
  {
    label: "الخلاصات الشهرية",
    path: "/pension/monthly-summaries",
    icon: <CalendarCheck className={ICON} />,
    anyPermission: ["SalaryBatch.View", "System.Admin"],
    group: "pension",
    subGroup: "entitlements",
  },
  {
    label: "إدارة الوكلاء",
    path: "/pension/agents",
    icon: <Handshake className={ICON} />,
    anyPermission: ["File.View.Sensitive", "System.Admin"],
    group: "pension",
    subGroup: "entitlements",
  },
  {
    label: "إدارة الأوصياء",
    path: "/pension/guardians",
    icon: <HeartHandshake className={ICON} />,
    anyPermission: ["File.View.Sensitive", "System.Admin"],
    group: "pension",
    subGroup: "entitlements",
  },
  {
    label: "مستلمو المعاش",
    path: "/pension/salary-recipients",
    icon: <HandCoins className={ICON} />,
    anyPermission: ["SalaryRecipient.View", "System.Admin"],
    group: "pension",
    subGroup: "entitlements",
  },
  {
    label: "الاستحقاقات المالية",
    path: "/pension/financial-entitlements",
    icon: <BadgeCheck className={ICON} />,
    anyPermission: ["File.View.Sensitive", "System.Admin"],
    group: "pension",
    subGroup: "entitlements",
  },

  // قسم السجلات والإحصاء
  {
    label: "الإحصاءات والبيانات",
    path: "/pension/statistics",
    icon: <Sigma className={ICON} />,
    anyPermission: ["Retiree.View", "System.Admin"],
    group: "pension",
    subGroup: "records-stats",
  },
  {
    label: "بطاقات صرف المعاشات",
    path: "/pension/payment-cards",
    icon: <CreditCard className={ICON} />,
    anyPermission: ["File.View.Sensitive", "System.Admin"],
    group: "pension",
    subGroup: "records-stats",
  },
  {
    label: "مراجعة دفاتر الصرف",
    path: "/pension/annual-book-review",
    icon: <BookOpen className={ICON} />,
    anyPermission: ["File.View.Sensitive", "System.Admin"],
    group: "pension",
    subGroup: "records-stats",
  },

  // التقارير (وظيفة عامة)
  {
    label: "التقارير",
    path: "/reports",
    icon: <BarChart3 className={ICON} />,
    anyPermission: [
      "Report.Pension.View",
      "Report.Financial.View",
      "Report.Entitlements.View",
      "Report.Documents.View",
      "Report.Admin.View",
    ],
    group: "pension",
    subGroup: "reports",
  },
  {
    label: "تحليل كشوفات الإحالة",
    path: "/reports/kashf",
    icon: <LineChart className={ICON} />,
    requiredPermission: "Report.Admin.View",
    group: "pension",
    subGroup: "reports",
  },
  {
    label: "حالة الملفاتات",
    path: "/reports/ret-01",
    icon: <Files className={ICON} />,
    requiredPermission: "Report.Pension.View",
    group: "pension",
    subGroup: "reports",
  },
  {
    label: "تقرير المتقاعدين",
    path: "/reports/ret-02",
    icon: <Activity className={ICON} />,
    requiredPermission: "Report.Pension.View",
    group: "pension",
    subGroup: "reports",
  },
  {
    label: "سجل الإيقافات",
    path: "/reports/ret-03",
    icon: <Ban className={ICON} />,
    requiredPermission: "Report.Pension.View",
    group: "pension",
    subGroup: "reports",
  },
  {
    label: "مسيرات المعاشات",
    path: "/reports/fin-01",
    icon: <Wallet className={ICON} />,
    requiredPermission: "Report.Financial.View",
    group: "pension",
    subGroup: "reports",
  },
  {
    label: "التسويات المالية",
    path: "/reports/fin-02",
    icon: <FileCheck className={ICON} />,
    requiredPermission: "Report.Financial.View",
    group: "pension",
    subGroup: "reports",
  },
  {
    label: "إحصائيات المدخلين",
    path: "/files/uploader-stats",
    icon: <Upload className={ICON} />,
    requiredPermission: "Report.Pension.View",
    group: "pension",
    subGroup: "reports",
  },
  {
    label: "المستندات الناقصة",
    path: "/reports/doc-01",
    icon: <FileWarning className={ICON} />,
    requiredPermission: "Report.Documents.View",
    group: "pension",
    subGroup: "reports",
  },
  {
    label: "المستندات المنتهية",
    path: "/reports/doc-02",
    icon: <FileClock className={ICON} />,
    requiredPermission: "Report.Documents.View",
    group: "pension",
    subGroup: "reports",
  },
  {
    label: "المستفيدين والورثة",
    path: "/reports/dep-01",
    icon: <Users className={ICON} />,
    requiredPermission: "Report.Entitlements.View",
    group: "pension",
    subGroup: "reports",
  },
  {
    label: "كشوفات الإحاله",
    path: "/reports/com-01",
    icon: <ListOrdered className={ICON} />,
    requiredPermission: "Report.Admin.View",
    group: "pension",
    subGroup: "reports",
  },
  {
    label: "الموافقات المعلقة",
    path: "/reports/com-02",
    icon: <CircleCheck className={ICON} />,
    requiredPermission: "Report.Admin.View",
    group: "pension",
    subGroup: "reports",
  },
  {
    label: "تقارير ملفات التقاعد",
    path: "/reports/retirement-file-reports",
    icon: <BarChart3 className={ICON} />,
    requiredPermission: "Report.Pension.View",
    group: "pension",
    subGroup: "reports",
  },

  // ═══════════════════════════════════════════════════════
  //  إدارة الشئون المالية
  // ═══════════════════════════════════════════════════════

  // قسم الميزانية والحسابات
  {
    label: "مركز الالإدارة المالية",
    path: "/accounting",
    icon: <Calculator className={ICON} />,
    anyPermission: ["Accounting.View", "System.Admin"],
    group: "finance",
    subGroup: "budget-accounts",
  },
  {
    label: "دليل الحسابات",
    path: "/accounting/chart-of-accounts",
    icon: <ListTree className={ICON} />,
    anyPermission: ["Accounting.View", "System.Admin"],
    group: "finance",
    subGroup: "budget-accounts",
  },
  {
    label: "القيود اليومية",
    path: "/accounting/journal-entries",
    icon: <ScrollText className={ICON} />,
    anyPermission: ["Accounting.View", "System.Admin"],
    group: "finance",
    subGroup: "budget-accounts",
  },
  {
    label: "سندات الصرف والقبض",
    path: "/accounting/vouchers",
    icon: <Receipt className={ICON} />,
    anyPermission: ["Accounting.View", "System.Admin"],
    group: "finance",
    subGroup: "budget-accounts",
  },
  {
    label: "مراكز التكلفة",
    path: "/accounting/cost-centers",
    icon: <Boxes className={ICON} />,
    anyPermission: ["Accounting.View", "System.Admin"],
    group: "finance",
    subGroup: "budget-accounts",
  },
  {
    label: "الفترات المالية",
    path: "/accounting/periods",
    icon: <CalendarRange className={ICON} />,
    anyPermission: ["Accounting.View", "System.Admin"],
    group: "finance",
    subGroup: "budget-accounts",
  },
  {
    label: "الفصول المالية",
    path: "/accounting/quarters",
    icon: <Calendar className={ICON} />,
    anyPermission: ["Accounting.View", "System.Admin"],
    group: "finance",
    subGroup: "budget-accounts",
  },
  {
    label: "التقارير المالية",
    path: "/accounting/reports",
    icon: <TrendingUp className={ICON} />,
    anyPermission: ["Accounting.Reports", "System.Admin"],
    group: "finance",
    subGroup: "budget-accounts",
  },
  {
    label: "الأرصدة الافتتاحية",
    path: "/accounting/initial-opening-balances",
    icon: <Banknote className={ICON} />,
    anyPermission: ["Accounting.Manage", "System.Admin"],
    group: "finance",
    subGroup: "budget-accounts",
  },
  {
    label: "الموازنة",
    path: "/accounting/budget",
    icon: <PieChart className={ICON} />,
    anyPermission: ["Accounting.View", "System.Admin"],
    group: "finance",
    subGroup: "budget-accounts",
  },
  {
    label: "التدفقات النقدية",
    path: "/accounting/cash-flow",
    icon: <DollarSign className={ICON} />,
    anyPermission: ["Accounting.Reports", "System.Admin"],
    group: "finance",
    subGroup: "budget-accounts",
  },

  // قسم المصروفات والمدفوعات
  {
    label: "المعاشات والمدفوعات",
    path: "/salaries",
    icon: <BadgeDollarSign className={ICON} />,
    anyPermission: [
      "SalaryBatch.View",
      "SalaryBatch.Manage",
      "File.Edit.FinancialData",
      "System.Admin",
    ],
    group: "finance",
    subGroup: "expenses-payments",
  },

  // قسم المراجعة والرقابة المالية
  {
    label: "مباينة",
    path: "/admin/discrepancy-report",
    icon: <AlertTriangle className={ICON} />,
    anyPermission: ["System.Admin", "Audit.ViewAll"],
    group: "finance",
    subGroup: "audit-control",
  },

  // قسم المشتريات والمخازن
  {
    label: "المخزون والأصول",
    path: "/inventory",
    icon: <Package className={ICON} />,
    anyPermission: ["Inventory.Item.View", "System.Admin"],
    group: "finance",
    subGroup: "procurement-stores",
  },
  {
    label: "الجرد",
    path: "/inventory/stocktake",
    icon: <ClipboardCheck className={ICON} />,
    anyPermission: ["Inventory.Item.View", "System.Admin"],
    group: "finance",
    subGroup: "procurement-stores",
  },
  {
    label: "تصنيفات الأصول",
    path: "/assets/categories",
    icon: <Tags className={ICON} />,
    anyPermission: ["Inventory.Item.View", "System.Admin"],
    group: "finance",
    subGroup: "procurement-stores",
  },
  {
    label: "الإهلاك",
    path: "/assets/depreciation-run",
    icon: <Percent className={ICON} />,
    anyPermission: ["Inventory.Item.View", "System.Admin"],
    group: "finance",
    subGroup: "procurement-stores",
  },

  // قسم الخزينة
  {
    label: "الخزينة",
    path: "/finance/treasury",
    icon: <Vault className={ICON} />,
    anyPermission: ["Accounting.Manage", "System.Admin"],
    group: "finance",
    subGroup: "treasury",
  },

  // ═══════════════════════════════════════════════════════
  //  إدارة الشئون الإدارية
  // ═══════════════════════════════════════════════════════

  // قسم الشئون المالية والإدارية
  {
    label: "لوحة الشؤون الإدارية",
    path: "/hr",
    icon: <LayoutGrid className={ICON} />,
    anyPermission: [
      "HR.Dashboard.View",
      "HR.Employee.View",
      "HR.Reference.View",
      "HR.ServiceHistory.View",
      "System.Admin",
    ],
    group: "hr",
    subGroup: "admin-affairs",
  },
  {
    label: "الموظفون",
    path: "/hr/employees",
    icon: <IdCard className={ICON} />,
    requiredPermission: "HR.Employee.View",
    group: "hr",
    subGroup: "admin-affairs",
  },
  {
    label: "الإجازات",
    path: "/hr/leaves",
    icon: <Palmtree className={ICON} />,
    anyPermission: [
      "HR.Dashboard.View",
      "HR.Employee.View",
      "HR.Reference.View",
      "HR.ServiceHistory.View",
      "System.Admin",
    ],
    group: "hr",
    subGroup: "admin-affairs",
  },
  {
    label: "الحضور والغياب",
    path: "/hr/attendance",
    icon: <Fingerprint className={ICON} />,
    anyPermission: [
      "HR.Dashboard.View",
      "HR.Employee.View",
      "HR.Reference.View",
      "HR.ServiceHistory.View",
      "System.Admin",
    ],
    group: "hr",
    subGroup: "admin-affairs",
  },
  {
    label: "وثائق الموظفين",
    path: "/hr/documents",
    icon: <Folders className={ICON} />,
    anyPermission: [
      "HR.Dashboard.View",
      "HR.Employee.View",
      "HR.Reference.View",
      "HR.ServiceHistory.View",
      "System.Admin",
    ],
    group: "hr",
    subGroup: "admin-affairs",
  },
  {
    label: "سجل الخدمة الآلي",
    path: "/hr/service-history",
    icon: <Route className={ICON} />,
    requiredPermission: "HR.ServiceHistory.View",
    group: "hr",
    subGroup: "admin-affairs",
  },
  {
    label: "الإدارات والأقسام",
    path: "/hr/reference",
    icon: <Network className={ICON} />,
    anyPermission: ["HR.Reference.View", "System.Admin"],
    group: "hr",
    subGroup: "admin-affairs",
  },

  // ═══════════════════════════════════════════════════════
  //  إدارة الاستثمار
  // ═══════════════════════════════════════════════════════

  // قسم الدراسات والجدوى الاقتصادية
  {
    label: "المحافظ الاستثمارية",
    path: "/investment/portfolios",
    icon: <Layers className={ICON} />,
    anyPermission: ["Investment.View", "System.Admin"],
    group: "investment",
    subGroup: "studies",
  },

  // قسم التصاميم
  {
    label: "المشاريع الاستثمارية",
    path: "/investment/projects",
    icon: <Rocket className={ICON} />,
    anyPermission: ["Investment.View", "System.Admin"],
    group: "investment",
    subGroup: "designs",
  },

  // قسم العلاقات التجارية
  {
    label: "العقارات",
    path: "/investment/properties",
    icon: <Building className={ICON} />,
    anyPermission: ["Investment.View", "System.Admin"],
    group: "investment",
    subGroup: "commercial-relations",
  },
  {
    label: "التأجير",
    path: "/investment/leasing",
    icon: <DoorOpen className={ICON} />,
    anyPermission: ["Investment.View", "System.Admin"],
    group: "investment",
    subGroup: "commercial-relations",
  },

  // قسم التنفيذ والمتابعة
  {
    label: "الاستثمار",
    path: "/investment",
    icon: <Briefcase className={ICON} />,
    anyPermission: ["Investment.View", "System.Admin"],
    group: "investment",
    subGroup: "execution-followup",
  },
  {
    label: "الصيانة و الترميمات",
    path: "/investment/maintenance",
    icon: <Wrench className={ICON} />,
    anyPermission: ["Investment.View", "System.Admin"],
    group: "investment",
    subGroup: "execution-followup",
  },
  {
    label: "الحوكمة والاعتمادات",
    path: "/investment/governance",
    icon: <Shield className={ICON} />,
    anyPermission: ["Investment.View", "System.Admin"],
    group: "investment",
    subGroup: "execution-followup",
  },

  // ═══════════════════════════════════════════════════════
  //  الإعدادات
  // ═══════════════════════════════════════════════════════
  {
    label: "إدارة المستخدمين",
    path: "/system-admin/users",
    icon: <UserCog className={ICON} />,
    requiredPermission: "System.Admin",
    group: "config",
    subGroup: "audit",
  },
  {
    label: "إدارة الإشعارات",
    path: "/admin/notifications",
    icon: <BellRing className={ICON} />,
    requiredPermission: "System.Admin",
    group: "config",
    subGroup: "audit",
  },
  {
    label: "الفروع",
    path: "/branches",
    icon: <Building2 className={ICON} />,
    requiredPermission: "System.Admin",
    group: "config",
    subGroup: "audit",
  },
  {
    label: "البنوك",
    path: "/banks",
    icon: <Landmark className={ICON} />,
    requiredPermission: "System.Admin",
    group: "config",
    subGroup: "audit",
  },
  {
    label: "الجهات الخارجية",
    path: "/correspondence/external-parties",
    icon: <Globe className={ICON} />,
    anyPermission: ["Correspondence.View", "System.Admin"],
    group: "config",
    subGroup: "audit",
  },
  {
    label: "الرتب العسكرية",
    path: "/ranks",
    icon: <Medal className={ICON} />,
    requiredPermission: "System.Admin",
    group: "config",
    subGroup: "audit",
  },
  {
    label: "أنواع الوثائق",
    path: "/document-types",
    icon: <FileType className={ICON} />,
    requiredPermission: "System.Admin",
    group: "config",
    subGroup: "audit",
  },
  {
    label: "إعدادات النظام",
    path: "/admin/settings",
    icon: <Settings className={ICON} />,
    requiredPermission: "System.Admin",
    group: "config",
    subGroup: "audit",
  },
  {
    label: "إعدادات الاستثمار",
    path: "/investment/settings",
    icon: <SlidersHorizontal className={ICON} />,
    requiredPermission: "System.Admin",
    group: "config",
    subGroup: "audit",
  },
  // سجلات التدقيق والرقابة
  {
    label: "سجلات التدقيق",
    path: "/admin/audit-logs",
    icon: <ClipboardList className={ICON} />,
    anyPermission: ["System.Admin", "Audit.ViewAll"],
    group: "config",
    subGroup: "audit",
  },
  {
    label: "سجل تسجيل الدخول",
    path: "/admin/login-history",
    icon: <History className={ICON} />,
    anyPermission: ["System.Admin", "Audit.ViewAll"],
    group: "config",
    subGroup: "audit",
  },
  {
    label: "لوحة الامتثال",
    path: "/compliance",
    icon: <ShieldAlert className={ICON} />,
    anyPermission: ["System.Admin", "Audit.ViewAll"],
    group: "config",
    subGroup: "audit",
  },
];

export const dashboardItem: NavItem = {
  label: "لوحة التحكم الرئيسية",
  path: "/dashboard",
  icon: <LayoutDashboard className={ICON} />,
  group: "director",
};

// ─── Shared Permission Filtering ─────────────────────────

export function filterByPermissions(items: NavItem[], permissions: string[] | undefined): NavItem[] {
  const perms = permissions ?? [];
  return items.filter((item) => {
    if (!item.requiredPermission && !item.anyPermission) return true;
    if (item.requiredPermission) return perms.includes(item.requiredPermission);
    if (item.anyPermission?.length) return item.anyPermission.some((p) => perms.includes(p));
    return true;
  });
}
