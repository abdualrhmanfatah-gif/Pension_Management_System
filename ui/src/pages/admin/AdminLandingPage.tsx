import {
    BellRing,
    Building2,
    ChevronLeft,
    FileSearch,
    FileText,
    Landmark,
    LayoutDashboard,
    LogIn,
    Medal,
    Settings,
    ShieldAlert,
    ShieldCheck,
    Users,
} from "lucide-react";
import { Link } from "react-router-dom";

import { Card, CardContent, CardHeader, Heading, PageLayout, Text } from "@/components/ui";
import { cn } from "@/lib/utils";
import { useAuthStore } from "@/stores/authStore";

const adminSections: {
    title: string;
    links: { path: string; label: string; icon: React.ReactNode; permission: string | string[] }[];
}[] = [
        {
            title: "المستخدمون والصلاحيات",
            links: [
                { path: "/system-admin/users", label: "المستخدمون", icon: <Users className="h-5 w-5" />, permission: "System.Admin" },
            ],
        },
        {
            title: "البيانات الأساسية",
            links: [
                { path: "/branches", label: "الفروع", icon: <Building2 className="h-5 w-5" />, permission: "System.Admin" },
                { path: "/banks", label: "البنوك", icon: <Landmark className="h-5 w-5" />, permission: "System.Admin" },
                { path: "/ranks", label: "الرتب", icon: <Medal className="h-5 w-5" />, permission: "System.Admin" },
                { path: "/document-types", label: "أنواع الوثائق", icon: <FileText className="h-5 w-5" />, permission: "System.Admin" },
            ],
        },
        {
            title: "التشغيل والإعدادات",
            links: [
                { path: "/admin/notifications", label: "إدارة الإشعارات", icon: <BellRing className="h-5 w-5" />, permission: "System.Admin" },
                { path: "/pension/ministry-batches", label: "كشوفات الإحالة إلى التقاعد", icon: <ShieldCheck className="h-5 w-5" />, permission: "Ministry.View" },
                { path: "/admin/settings", label: "إعدادات النظام", icon: <Settings className="h-5 w-5" />, permission: "System.Admin" },
            ],
        },
        {
            title: "التدقيق والأمان",
            links: [
                { path: "/admin/audit-logs", label: "سجلات التدقيق", icon: <FileSearch className="h-5 w-5" />, permission: ["System.Admin", "Audit.ViewAll"] },
                { path: "/admin/login-history", label: "سجلات الدخول", icon: <LogIn className="h-5 w-5" />, permission: ["System.Admin", "Audit.ViewAll"] },
                { path: "/compliance", label: "لوحة الامتثال", icon: <ShieldAlert className="h-5 w-5" />, permission: ["System.Admin", "Audit.ViewAll"] },
            ],
        },
    ];

export function AdminLandingPage() {
    const permissions = useAuthStore((s) => s.permissions);

    const hasAccess = (perm: string | string[]) =>
        Array.isArray(perm) ? perm.some((p) => permissions.includes(p)) : permissions.includes(perm);

    return (
        <PageLayout>
            <div className="flex items-center gap-3">
                <div className="flex items-center justify-center w-10 h-10 rounded-xl bg-primary/10">
                    <LayoutDashboard className="w-5 h-5 text-primary" />
                </div>
                <div>
                    <Heading level="h1">لوحة الإدارة</Heading>
                    <Text variant="description">بوابة إدارة النظام والإعدادات والتدقيق</Text>
                </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {adminSections.map((section) => {
                    const visibleLinks = section.links.filter((link) => hasAccess(link.permission));
                    if (visibleLinks.length === 0) return null;

                    return (
                        <Card key={section.title} className="overflow-hidden">
                            <CardHeader className="pb-2">
                                <Heading level="h2" className="text-base font-semibold text-foreground">{section.title}</Heading>
                            </CardHeader>
                            <CardContent className="pt-0">
                                <ul className="space-y-1">
                                    {visibleLinks.map((link) => (
                                        <li key={link.path}>
                                            <Link
                                                to={link.path}
                                                className={cn(
                                                    "flex items-center gap-3 rounded-lg px-3 py-3 text-sm font-medium transition-colors min-h-[44px]",
                                                    "text-muted-foreground hover:bg-muted/50 hover:text-foreground"
                                                )}
                                            >
<<<<<<< HEAD
                                                <span className="text-muted-foreground">{link.icon}</span>
                                                <span className="flex-1">{link.label}</span>
                                                <ChevronLeft className="h-4 w-4 rotate-180 text-muted-foreground" />
=======
                                                <span className="text-muted-foreground/80">{link.icon}</span>
                                                <span className="flex-1">{link.label}</span>
                                                <ChevronLeft className="h-4 w-4 rotate-180 text-muted-foreground/60" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                            </Link>
                                        </li>
                                    ))}
                                </ul>
                            </CardContent>
                        </Card>
                    );
                })}
            </div>
        </PageLayout>
    );
}
