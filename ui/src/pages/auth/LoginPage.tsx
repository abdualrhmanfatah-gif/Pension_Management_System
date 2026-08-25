import { zodResolver } from "@hookform/resolvers/zod";
import { Eye, EyeOff, ShieldCheck, TimerReset } from "lucide-react";
import { useEffect,useRef,useState } from "react";
import { useForm } from "react-hook-form";
import { useLocation, useNavigate, useSearchParams } from "react-router-dom";
import { z } from "zod";

import api from "@/api/axios";
import { AlertBanner } from "@/components/ui/AlertBanner";
import { Button } from "@/components/ui/Button";
import { Heading, Text } from "@/components/ui";
import { Input } from "@/components/ui/Input";
import { useAuthStore } from "@/stores/authStore";

// ─── Validation Schema ────────────────────────────────

const loginSchema = z.object({
    username: z.string().min(1, "اسم المستخدم مطلوب"),
    password: z.string().min(1, "كلمة المرور مطلوبة"),
});

type LoginFormData = z.infer<typeof loginSchema>;

// ─── Logo with fallback ─────────────────────────────

function LogoWithFallback() {
    const [failed, setFailed] = useState(false);

    if (failed) {
        return (
            <div className="inline-flex items-center justify-center w-28 h-28 rounded-3xl bg-white/5 backdrop-blur-xl mb-4 border border-white/10 ">
                <ShieldCheck className="w-16 h-16 text-primary-300" strokeWidth={1.5} />
            </div>
        );
    }

    return (
        <div className="inline-flex items-center justify-center w-28 h-28 rounded-3xl bg-white/5 backdrop-blur-xl mb-4 overflow-hidden border border-white/10 ">
            <picture>
              <source srcSet="/pension-logo.webp" type="image/webp" />
              <img src="/pension-logo.png" alt="الشعار" fetchPriority="high" className="w-full h-full object-contain p-2" onError={() => setFailed(true)} />
            </picture>
        </div>
    );
}

// ─── LoginPage Component ──────────────────────────────

export function LoginPage() {
    const navigate = useNavigate();
    const location = useLocation();
    const [searchParams] = useSearchParams();
    const login = useAuthStore((s) => s.login);
    const [showPassword, setShowPassword] = useState(false);
    const [apiError, setApiError] = useState<string | null>(null);
    const failCountRef = useRef(0);
    const intervalRef = useRef<ReturnType<typeof setInterval>>(0);
    const [cooldown, setCooldown] = useState(0);

    useEffect(() => () => clearInterval(intervalRef.current), []);

    // Determine where to go after login:
    // 1. React Router state (from ProtectedRoute / PermissionRoute)
    // 2. Query param ?returnTo= (from axios 401 interceptor)
    // 3. sessionStorage saved path (from page refresh / session expiry)
    // 4. Default: /dashboard
    const fromState = (location.state as { from?: { pathname: string; search?: string } })?.from;
    const savedPath = sessionStorage.getItem("postLoginRedirect");
    const returnTo = fromState
        ? `${fromState.pathname}${fromState.search || ""}`
        : searchParams.get("returnTo")
        || savedPath
        || "/dashboard";

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting },
    } = useForm<LoginFormData>({
        resolver: zodResolver(loginSchema),
    });

    const onSubmit = async (data: LoginFormData) => {
        if (cooldown > 0) return;
        setApiError(null);
        try {
            // Generate persistent device ID for session tracking
            let deviceId = localStorage.getItem("deviceId");
            if (!deviceId) {
                deviceId = crypto.randomUUID?.() ?? crypto.getRandomValues(new Uint32Array(4)).join("-");
                localStorage.setItem("deviceId", deviceId);
            }

            const loginPayload = { ...data, deviceID: deviceId };

            const res = await api.post<{
                isSuccess: boolean;
                message?: string;
                token?: string;
                refreshToken?: string;
                userID?: number;
                fullName?: string;
                username?: string;
                roleName?: string;
                roleID?: number;
                branchID?: number;
                branchName?: string;
                mustChangePassword?: boolean;
            }>("/auth/login", loginPayload);

            if (res.data.isSuccess && res.data.token) {
                failCountRef.current = 0;
                await login({
                    token: res.data.token,
                    refreshToken: res.data.refreshToken,
                    user: {
                        id: res.data.userID!,
                        fullName: res.data.fullName!,
                        username: res.data.username!,
                        role: res.data.roleName!,
                        roleId: res.data.roleID,
                        branchId: res.data.branchID,
                        branchName: res.data.branchName,
                        mustChangePassword: res.data.mustChangePassword,
                    },
                });
                navigate(returnTo, { replace: true });
                sessionStorage.removeItem("postLoginRedirect");
                return;
            }
            setApiError(res.data.message || "فشل تسجيل الدخول");
        } catch (err: unknown) {
            if (err && typeof err === "object" && "response" in err) {
                const axiosErr = err as { response?: { data?: { message?: string } } };
                setApiError(axiosErr.response?.data?.message || "خطأ في الاتصال بالخادم");
            } else {
                setApiError("خطأ في الاتصال بالخادم");
            }
        }

        failCountRef.current += 1;
        clearInterval(intervalRef.current);
        const delay = Math.min(1000 * 2 ** (failCountRef.current - 1), 60000);
        setCooldown(delay);
        const end = performance.now() + delay;
        intervalRef.current = setInterval(() => {
            const remaining = Math.max(0, end - performance.now());
            setCooldown(remaining);
            if (remaining <= 0) {
                clearInterval(intervalRef.current);
                failCountRef.current = 0;
            }
        }, 1000);
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary-950 via-primary-900 to-primary-800 p-4" dir="rtl">
            {/* Decorative background */}
            <div className="absolute inset-0 overflow-hidden">
                <div className="absolute -top-40 -start-40 w-96 h-96 bg-primary-500/10 rounded-full" />
                <div className="absolute -bottom-40 -end-40 w-96 h-96 bg-primary-400/10 rounded-full" />
            </div>

            <div className="relative w-full max-w-md animate-fade-in">
                {/* Logo */}
                <div className="text-center mb-8">
                    <LogoWithFallback />
                    <Heading level="h1" className="text-lg md:text-xl lg:text-2xl font-bold text-white">الإدارة العامة لصندوق التقاعد الأمني</Heading>
                    <Text variant="body" className="text-primary-300 mt-1">تسجيل الدخول </Text>
                </div>

                {/* Card */}
                <div className="bg-white/10 border border-white/10 rounded-2xl p-8 ">
                    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5 [&_label]:text-white">
                        {/* API Error + Cooldown */}
                        {apiError && (
                            <AlertBanner
                                variant="destructive"
                                title={cooldown > 0 ? `انتظر ${Math.ceil(cooldown / 1000)} ثانية قبل المحاولة التالية` : apiError}
                                icon={cooldown > 0 ? <TimerReset className="h-4 w-4" /> : undefined}
                                className="bg-red-500/10 border-red-400/20 text-red-300"
                            />
                        )}

                        {/* Username */}
                        <Input
                            id="username"
                            type="text"
                            autoComplete="username"
                            placeholder="أدخل اسم المستخدم"
                            label="اسم المستخدم"
                            error={errors.username?.message}
                            className="dark-autofill-fix bg-white/5 border-white/10 text-foreground placeholderr:text-primary-400/50 focus:ring-primary-400/50 focus:border-transparent"
                            {...register("username")}
                        />

                        {/* Password */}
                        <div className="relative">
                            <Input
                                id="password"
                                type={showPassword ? "text" : "password"}
                                autoComplete="current-password"
                                placeholder="أدخل كلمة المرور"
                                label="كلمة المرور"
                                error={errors.password?.message}
                                className="dark-autofill-fix bg-white/5 border-white/10 text-foreground placeholderr:text-primary-400/50 focus:ring-primary-400/50 focus:border-transparent pe-10"
                                {...register("password")}
                            />
                            <Button
                                variant="ghost"
                                size="sm"
                                type="button"
                                onClick={() => setShowPassword(!showPassword)}
                                className="absolute end-3 top-8 text-primary-400/70 hover:text-primary-300"
                            >
                                {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                            </Button>
                        </div>

                        {/* Submit */}
                        <Button
                            type="submit"
                            loading={isSubmitting}
                            disabled={cooldown > 0}
                            className="w-full h-11 text-base bg-primary-500 hover:bg-primary-400 text-white"
                        >
                            {!isSubmitting && "تسجيل الدخول"}
                        </Button>
                    </form>
                </div>

                {/* Footer */}
                <Text variant="body-sm" className="text-center text-white mt-6">
                    جميع الحقوق محفوظة © 2026 —الإدارة العامة لصندوق التقاعد الأمني
                </Text>
            </div>
        </div>
    );
}
