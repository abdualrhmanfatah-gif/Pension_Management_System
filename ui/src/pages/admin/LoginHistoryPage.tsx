import { useQuery, useQueryClient } from "@tanstack/react-query";
import { CheckCircle, LogIn, MapPin, Monitor,RefreshCw, Search, ShieldAlert, XCircle } from "lucide-react";
import { useState } from "react";

import api from "@/api/axios";
<<<<<<< HEAD
import { Badge, Button, Card, type CustomColumnDef, DataTable, DateInput, Heading, Input, Loader, PageLayout, Select, SelectContent, SelectItem, SelectTrigger, SelectValue, StatCard, StickyHeader, Text } from "@/components/ui";
=======
import { Badge, Button, Card, DataTable, type CustomColumnDef, Heading, Input, DateInput, Loader, PageLayout, StatCard, Select, SelectContent, SelectItem, SelectTrigger, SelectValue, StickyHeader, Text } from "@/components/ui";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

interface LoginEntry {
    loginID: number;
    userID: number | null;
    userName: string | null;
    attemptedUsername: string;
    loginTime: string;
    ipAddress: string | null;
    userAgent: string | null;
    isSuccessful: boolean;
    failureReason: string | null;
    deviceID: string | null;
}

interface LoginStats {
    totalAttempts: number;
    failedToday: number;
    uniqueIpsToday: number;
    lockedAccounts: number;
}

export function LoginHistoryPage() {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);

    const [filterSearch, setFilterSearch] = useState("");
    const [filterSuccess, setFilterSuccess] = useState<string>("");
    const [filterFrom, setFilterFrom] = useState("");
    const [filterTo, setFilterTo] = useState("");

    const { data, isLoading: loading, isError } = useQuery({
        queryKey: ["login-history", page, filterSearch, filterSuccess, filterFrom, filterTo],
        queryFn: async () => {
            const params = new URLSearchParams();
            params.set("page", String(page));
            params.set("pageSize", "30");
            if (filterSearch) params.set("search", filterSearch);
            if (filterSuccess === "true") params.set("isSuccessful", "true");
            if (filterSuccess === "false") params.set("isSuccessful", "false");
            if (filterFrom) params.set("from", filterFrom);
            if (filterTo) params.set("to", filterTo);

            const res = await api.get<{ data: { items: LoginEntry[]; totalCount: number; totalPages: number; stats: LoginStats } }>(`/admin/login-history?${params}`);
            return res.data.data;
        }
    });

    const entries = data?.items || [];
    const stats = data?.stats || null;
    const totalPages = data?.totalPages || 1;
    void totalPages;

    const handleRefresh = () => {
        queryClient.invalidateQueries({ queryKey: ["login-history"] });
    };

    return (
        <PageLayout>
            {/* Header */}
            <StickyHeader>
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <div className="p-2.5 rounded-xl bg-gradient-to-br from-blue-500/20 to-cyan-500/20">
                            <LogIn className="w-6 h-6 text-blue-600 dark:text-blue-400" />
                        </div>
                        <div>
                            <Heading level="h1" className="text-lg md:text-xl lg:text-2xl font-bold">سجلات تسجيل الدخول</Heading>
                            <Text variant="description">مراقبة محاولات الدخول والأنشطة الأمنية</Text>
                        </div>
                    </div>
                    <Button variant="outline" size="sm" onClick={handleRefresh} loading={loading}>
                        <RefreshCw className="w-4 h-4 ms-1" /> تحديث
                    </Button>
                </div>
            </StickyHeader>

            {/* Stats Cards */}
            {stats && (
                <div className="grid grid-cols-2 sm:grid-cols-2 md:grid-cols-4 gap-2 sm:gap-4">
                    <StatCard title="إجمالي المحاولات" value={stats.totalAttempts} icon={<LogIn className="w-5 h-5" />} variant="blue" />
                    <StatCard title="محاولات فاشلة اليوم" value={stats.failedToday} icon={<XCircle className="w-5 h-5" />} variant="destructive" />
                    <StatCard title="عناوين IP اليوم" value={stats.uniqueIpsToday} icon={<MapPin className="w-5 h-5" />} variant="amber" />
                    <StatCard title="حسابات مقفلة" value={stats.lockedAccounts} icon={<ShieldAlert className="w-5 h-5" />} variant="rose" />
                </div>
            )}

            {/* Filters */}
            <Card className="p-4">
                <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
                    <div className="relative">
                        <Search className="absolute end-3 top-2.5 w-4 h-4 text-muted-foreground" />
                        <Input
                            placeholder="بحث في اسم المستخدم..."
                            className="pe-9 h-9"
                            value={filterSearch}
                            onChange={(e) => { setFilterSearch(e.target.value); setPage(1); }}
                        />
                    </div>
                    <Select
                        value={filterSuccess}
                        onValueChange={(v) => { setFilterSuccess(v === "__all__" ? "" : v); setPage(1); }}
                    >
                        <SelectTrigger className="min-h-[44px]">
                            <SelectValue placeholder="جميع النتائج" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="__all__">جميع النتائج</SelectItem>
                            <SelectItem value="true">عمليات ناجحة</SelectItem>
                            <SelectItem value="false">عمليات فاشلة</SelectItem>
                        </SelectContent>
                    </Select>
                    <DateInput
                        className="h-9"
                        value={filterFrom}
                        onChange={(val) => { val !== undefined && setFilterFrom(val); setPage(1); }}
                    />
                    <DateInput
                        className="h-9"
                        value={filterTo}
                        onChange={(val) => { val !== undefined && setFilterTo(val); setPage(1); }}
                    />
                </div>
            </Card>

            {/* Content */}
            {loading ? (
                <div className="py-20 flex justify-center"><Loader variant="page" /></div>
            ) : isError ? (
                <Card className="py-20 text-center text-destructive"><p>تعذر تحميل سجل الدخول</p></Card>
            ) : entries.length === 0 ? (
                <Card className="py-20 text-center text-muted-foreground">
<<<<<<< HEAD
                    <Search className="w-12 h-12 mx-auto mb-3 opacity-100" />
=======
                    <Search className="w-12 h-12 mx-auto mb-3 opacity-20" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    <p>لا توجد سجلات مطابقة للبحث</p>
                </Card>
            ) : (
                <Card className="overflow-hidden ">
                    <DataTable
                        columns={[
                            { key: "userName", header: "اسم المستخدم", render: (row) => <div><div className="font-medium">{row.userName || row.attemptedUsername}</div>{row.userName && row.userName !== row.attemptedUsername && <div className="text-[10px] sm:text-xs text-muted-foreground">حاول بـ: {row.attemptedUsername}</div>}</div> },
                            { key: "loginTime", header: "وقت المحاولة", render: (row) => <span className="text-xs font-mono">{new Date(row.loginTime).toLocaleString("ar-YE")}</span> },
                            { key: "isSuccessful", header: "الحالة", render: (row) => row.isSuccessful ? <Badge variant="success" className="gap-1"><CheckCircle className="w-3 h-3" /> ناجح</Badge> : <Badge variant="destructive" className="gap-1"><XCircle className="w-3 h-3" /> فشل</Badge> },
                            { key: "ipAddress", header: "عنوان IP", render: (row) => <span className="text-xs font-mono text-muted-foreground">{row.ipAddress}</span> },
                            { key: "userAgent", header: "معلومات الجهاز", render: (row) => <div className="flex items-center gap-2 text-xs text-muted-foreground max-w-[300px] truncate" title={row.userAgent || ""}><Monitor className="w-3 h-3 flex-shrink-0" /><span className="truncate">{row.userAgent || "—"}</span></div> },
                            { key: "failureReason", header: "السبب (عند الفشل)", render: (row) => <span className="text-xs text-rose-600 font-medium">{row.failureReason || "—"}</span> },
                        ] as CustomColumnDef<LoginEntry>[]}
                        data={entries}
                        rowKey={(row) => row.loginID}
                        pagination={{ page, pageSize: 30, total: data?.totalCount || 0, onPageChange: setPage }}
                    />
                </Card>
            )}
        </PageLayout>
    );
}


