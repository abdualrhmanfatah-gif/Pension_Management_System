import {
    Building2,
    Calendar,
    Clock,
    KeyRound,
    LogIn,
    Mail,
    Shield,
    User as UserIcon,
} from "lucide-react";

import { Badge, Card, CardContent, CardHeader, CardTitle, Heading, InfoPair, PageLayout } from "@/components/ui";
import { useNavigate } from "react-router-dom";

import { Button } from "@/components/ui/Button";
import { useAuthStore } from "@/stores/authStore";
import { BADGE_VARIANT } from "@/constants";

const formatDate = (dateStr: string | null | undefined) => {
    if (!dateStr) return "—";
    try {
        return new Intl.DateTimeFormat("ar-YE", {
            year: "numeric",
            month: "long",
            day: "numeric",
            hour: "2-digit",
            minute: "2-digit",
        }).format(new Date(dateStr));
    } catch {
        return dateStr;
    }
};

export function AccountPage() {
    const navigate = useNavigate();
    const user = useAuthStore((s) => s.user);

    if (!user) return null;

    return (
        <PageLayout maxWidth="max-w-4xl">
            {/* Header Card — Avatar + Name + Role */}
            <Card className="overflow-hidden">
                <div className="h-24 bg-gradient-to-l from-primary-600 to-primary-800" />
                <div className="px-6 pb-6 -mt-12">
                    <div className="flex items-end gap-4">
                        <div className="flex items-center justify-center w-24 h-24 rounded-2xl bg-primary-500 text-white text-3xl font-bold border-4 border-background  shrink-0">
                            {user.fullName?.charAt(0) || "م"}
                        </div>
                        <div className="flex-1 pt-14">
                            <Heading level="h1" className="text-xl md:text-2xl lg:text-3xl font-bold">{user.fullName}</Heading>
                            <div className="flex items-center gap-2 mt-1">
                                <Shield className="h-4 w-4 text-muted-foreground" />
                                <span className="text-muted-foreground">{user.role}</span>
                                <span className="text-muted-foreground mx-1">·</span>
                                <Badge variant={user.isActive ? BADGE_VARIANT.SUCCESS : BADGE_VARIANT.DANGER} dot>
                                    {user.isActive ? "نشط" : "غير نشط"}
                                </Badge>
                            </div>
                        </div>
                    </div>
                </div>
            </Card>

            {/* Main Info Grid */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Personal Information */}
                <Card>
                    <CardHeader>
                        <CardTitle className="flex items-center gap-2">
                            <UserIcon className="h-5 w-5 text-primary" />
                            المعلومات الشخصية
                        </CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-1">
                        <InfoPair
                            label="الاسم الكامل"
                            value={user.fullName}
                            icon={<UserIcon className="h-4 w-4" />}
                            layout="row"
                        />
                        <InfoPair
                            label="اسم المستخدم"
                            value={user.username}
                            icon={<UserIcon className="h-4 w-4" />}
                            layout="row"
                        />
                        <InfoPair
                            label="البريد الإلكتروني"
                            value={user.email || "غير محدد"}
                            icon={<Mail className="h-4 w-4" />}
                            layout="row"
                        />
                    </CardContent>
                </Card>

                {/* Account Information */}
                <Card>
                    <CardHeader>
                        <CardTitle className="flex items-center gap-2">
                            <Calendar className="h-5 w-5 text-primary" />
                            معلومات الحساب
                        </CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-1">
                        <InfoPair
                            label="تاريخ إنشاء الحساب"
                            value={formatDate(user.createdAt)}
                            icon={<Calendar className="h-4 w-4" />}
                            layout="row"
                        />
                        <InfoPair
                            label="آخر تسجيل دخول"
                            value={formatDate(user.lastLogin)}
                            icon={<LogIn className="h-4 w-4" />}
                            layout="row"
                        />
                        <InfoPair
                            label="حالة الحساب"
                            value={user.isActive ? "نشط" : "غير نشط"}
                            icon={<Clock className="h-4 w-4" />}
                            layout="row"
                        />
                    </CardContent>
                </Card>

                {/* Organization Information */}
                <Card>
                    <CardHeader>
                        <CardTitle className="flex items-center gap-2">
                            <Building2 className="h-5 w-5 text-primary" />
                            معلومات التنظيم
                        </CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-1">
                        <InfoPair
                            label="الدور / الصلاحية"
                            value={user.role}
                            icon={<Shield className="h-4 w-4" />}
                            layout="row"
                        />
                        <InfoPair
                            label="الفرع"
                            value={user.branchName || "غير محدد"}
                            icon={<Building2 className="h-4 w-4" />}
                            layout="row"
                        />
                    </CardContent>
                </Card>

                {/* Security */}
                <Card>
                    <CardHeader>
                        <CardTitle className="flex items-center gap-2">
                            <KeyRound className="h-5 w-5 text-primary" />
                            الأمان
                        </CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <InfoPair
                            label="تغيير كلمة المرور"
                            value={user.mustChangePassword ? "مطلوب التغيير" : "—"}
                            icon={<KeyRound className="h-4 w-4" />}
                            layout="row"
                        />
                        <Button
                            variant="outline"
                            onClick={() => navigate("/change-password")}
                            className="w-full mt-2"
                        >
                            <KeyRound className="h-4 w-4" />
                            تغيير كلمة المرور
                        </Button>
                    </CardContent>
                </Card>
            </div>
        </PageLayout>
    );
}
