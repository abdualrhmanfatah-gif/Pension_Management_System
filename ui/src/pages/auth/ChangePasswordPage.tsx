import { zodResolver } from "@hookform/resolvers/zod";
import { Eye, EyeOff, KeyRound } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { z } from "zod";

import api from "@/api/axios";
import { Heading, Text } from "@/components/ui";
import { AlertBanner } from "@/components/ui/AlertBanner";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { useAuthStore } from "@/stores/authStore";

const changePasswordSchema = z
    .object({
        currentPassword: z.string().min(1, "كلمة المرور الحالية مطلوبة"),
        newPassword: z.string().min(6, "كلمة المرور الجديدة يجب أن تكون 6 أحرف على الأقل"),
        confirmPassword: z.string().min(1, "تأكيد كلمة المرور مطلوب"),
    })
    .refine((data) => data.newPassword === data.confirmPassword, {
        message: "كلمتا المرور غير متطابقتين",
        path: ["confirmPassword"],
    })
    .refine((data) => data.currentPassword !== data.newPassword, {
        message: "يجب أن تختلف كلمة المرور الجديدة عن الحالية",
        path: ["newPassword"],
    });

type ChangePasswordFormData = z.infer<typeof changePasswordSchema>;

export function ChangePasswordPage() {
    const navigate = useNavigate();
    const logout = useAuthStore((s) => s.logout);
    const user = useAuthStore((s) => s.user);
    const mustChangePassword = useAuthStore((s) => s.user?.mustChangePassword);
    const [showCurrent, setShowCurrent] = useState(false);
    const [showNew, setShowNew] = useState(false);
    const [showConfirm, setShowConfirm] = useState(false);
    const [apiError, setApiError] = useState<string | null>(null);

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting },
    } = useForm<ChangePasswordFormData>({
        resolver: zodResolver(changePasswordSchema),
    });

    const onSubmit = async (data: ChangePasswordFormData) => {
        setApiError(null);
        try {
            const res = await api.post<{ isSuccess: boolean; message?: string }>("/auth/change-password", {
                currentPassword: data.currentPassword,
                newPassword: data.newPassword,
            });

            if (res.data.isSuccess) {
                logout();
                navigate("/login", { replace: true });
                return;
            }

            setApiError(res.data.message || "فشل تغيير كلمة المرور");
        } catch (err: unknown) {
            if (err && typeof err === "object" && "response" in err) {
                const axiosErr = err as { response?: { data?: { message?: string } } };
                setApiError(axiosErr.response?.data?.message || "خطأ في الاتصال بالخادم");
            } else {
                setApiError("خطأ في الاتصال بالخادم");
            }
        }
    };

    const title = mustChangePassword
        ? "يجب تغيير كلمة المرور"
        : "تغيير كلمة المرور";

    const description = mustChangePassword
        ? "تم إعادة تعيين كلمة المرور بواسطة المشرف. يرجى تغييرها الآن للمتابعة."
        : "أدخل كلمة المرور الحالية والجديدة";

    return (
        <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-950 via-primary-900 to-primary-800 p-4">
            <div className="absolute inset-0 overflow-hidden">
                <div className="absolute -top-40 -start-40 w-96 h-96 bg-primary-500/10 rounded-full blur-3xl" />
                <div className="absolute -bottom-40 -end-40 w-96 h-96 bg-primary-400/10 rounded-full blur-3xl" />
            </div>

            <div className="relative w-full max-w-md animate-fade-in">
                <div className="text-center mb-8">
                    <div className="inline-flex items-center justify-center w-28 h-28 rounded-3xl bg-white/10 mb-4 overflow-hidden border border-white/10 ">
                        <picture>
                          <source srcSet="/pension-logo.webp" type="image/webp" />
                          <img src="/pension-logo.png" alt="الشعار" className="w-full h-full object-contain p-2" />
                        </picture>
                    </div>
                    <Heading level="h1" className="text-lg md:text-xl lg:text-2xl font-bold text-white">الإدارة العامة لصندوق التقاعد الأمني</Heading>
                    <Text variant="body" className="text-primary-300/80 mt-1">{title}</Text>
                </div>

                <div className="bg-white/10 border border-white/10 rounded-2xl p-8 ">
                    <div className="flex items-center gap-3 mb-6 pb-4 border-b border-white/10">
                        <div className="flex items-center justify-center w-10 h-10 rounded-full bg-amber-500/20">
                            <KeyRound className="h-5 w-5 text-amber-400" />
                        </div>
                        <div className="text-end">
                            <Text variant="label" className="text-white">{user?.fullName}</Text>
                            <Text variant="body-sm" className="text-primary-300/70">{user?.username}</Text>
                        </div>
                    </div>

                    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
                        {apiError && (
                            <AlertBanner
                                variant="destructive"
                                title={apiError}
                                className="bg-red-500/10 border-red-400/20 text-red-300"
                            />
                        )}

                        {description && (
                            <AlertBanner
                                variant="info"
                                title={description}
                                className="bg-primary-500/10 border-primary-400/20 text-primary-200"
                            />
                        )}

                        {/* Current Password */}
                        <div className="relative">
                            <Input
                                id="currentPassword"
                                type={showCurrent ? "text" : "password"}
                                autoComplete="current-password"
                                placeholder="أدخل كلمة المرور الحالية"
                                label="كلمة المرور الحالية"
                                error={errors.currentPassword?.message}
                                className="bg-white/5 border-white/10 text-white placeholder:text-primary-400/50 focus:ring-primary-400/50 focus:border-transparent pe-10 label:text-primary-200"
                                {...register("currentPassword")}
                            />
                            <Button
                                variant="ghost"
                                size="sm"
                                type="button"
                                onClick={() => setShowCurrent(!showCurrent)}
                                className="absolute end-3 top-8 text-primary-400/70 hover:text-primary-300"
                            >
                                {showCurrent ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                            </Button>
                        </div>

                        {/* New Password */}
                        <div className="relative">
                            <Input
                                id="newPassword"
                                type={showNew ? "text" : "password"}
                                autoComplete="new-password"
                                placeholder="أدخل كلمة المرور الجديدة"
                                label="كلمة المرور الجديدة"
                                error={errors.newPassword?.message}
                                className="bg-white/5 border-white/10 text-white placeholder:text-primary-400/50 focus:ring-primary-400/50 focus:border-transparent pe-10 label:text-primary-200"
                                {...register("newPassword")}
                            />
                            <Button
                                variant="ghost"
                                size="sm"
                                type="button"
                                onClick={() => setShowNew(!showNew)}
                                className="absolute end-3 top-8 text-primary-400/70 hover:text-primary-300"
                            >
                                {showNew ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                            </Button>
                        </div>

                        {/* Confirm Password */}
                        <div className="relative">
                            <Input
                                id="confirmPassword"
                                type={showConfirm ? "text" : "password"}
                                autoComplete="new-password"
                                placeholder="أعد إدخال كلمة المرور الجديدة"
                                label="تأكيد كلمة المرور الجديدة"
                                error={errors.confirmPassword?.message}
                                className="bg-white/5 border-white/10 text-white placeholder:text-primary-400/50 focus:ring-primary-400/50 focus:border-transparent pe-10 label:text-primary-200"
                                {...register("confirmPassword")}
                            />
                            <Button
                                variant="ghost"
                                size="sm"
                                type="button"
                                onClick={() => setShowConfirm(!showConfirm)}
                                className="absolute end-3 top-8 text-primary-400/70 hover:text-primary-300"
                            >
                                {showConfirm ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                            </Button>
                        </div>

                        <Button
                            type="submit"
                            loading={isSubmitting}
                            className="w-full h-11 text-base bg-primary-500 hover:bg-primary-400 text-white"
                        >
                            {!isSubmitting && "تغيير كلمة المرور"}
                        </Button>

                        {!mustChangePassword && (
                            <Button
                                type="button"
                                variant="ghost"
                                onClick={() => navigate("/dashboard")}
                                className="w-full h-11 text-base text-primary-300 hover:text-white"
                            >
                                إلغاء
                            </Button>
                        )}
                    </form>
                </div>

                <Text variant="body-sm" className="text-center text-white mt-6">
                    جميع الحقوق محفوظة © 2026 —الإدارة العامة لصندوق التقاعد الأمني
                </Text>
            </div>
        </div>
    );
}
