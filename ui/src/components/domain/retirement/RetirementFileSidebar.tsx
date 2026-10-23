import { useQueryClient } from "@tanstack/react-query";
import {          Activity, AlertCircle,BadgeCheck, Calendar, ChevronDown, ChevronUp, HeartPulse,  
MapPin, Shield, User, UserCircle2 } from "lucide-react";
import { useState } from "react";

import api from "@/api/axios";
import { Button, Card, EditableImage, Heading } from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { cn, formatDate, getDefaultDeceasedAvatar, getPhotoUrl, getStatusColorClass, resolvePersonalPhotoPath } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

// ─── Types ─────────────────────────────────────────────
interface FileData {
    retireeFullName: string;
    retireeID: number;
    nationalID: string | null;
    gender?: number;
    birthDate?: string | null;
    isAlive?: boolean;
    rankName: string | null;
    personalPhotoPath?: string | null;
    statusID?: number | null;
    fileStatusName?: string | null;
}

interface RetireeData {
    retireeID: number;
    nationalID: string;
    socialStatusName: string | null;
    healthStatusName: string | null;
    governorate: string | null;
    city: string | null;
    militaryNumber: string | null;
    personalPhotoPath?: string | null;
}

interface RetirementFileSidebarProps {
    file: FileData;
    retiree: RetireeData;
    isCompact: boolean;
}

export function RetirementFileSidebar({ file, retiree, isCompact }: RetirementFileSidebarProps) {
    const queryClient = useQueryClient();
    const canEditRetiree = useHasPermission("Retiree.Edit");
    const [detailsExpanded, setDetailsExpanded] = useState(false);

    const photoMutation = useApiMutation(
        async (uploadFile: File) => {
            const formData = new FormData();
            formData.append("photo", uploadFile);
            return api.post(`/retirees/${retiree.retireeID}/photo`, formData, {
                headers: { "Content-Type": "multipart/form-data" }
            });
        },
        {
            successMessage: "تم تحديث الصورة الشخصية بنجاح",
            onSuccess: () => {
                queryClient.invalidateQueries({ queryKey: ["retirement-file"] });
            }
        }
    );

    return (
        <div className="space-y-3 md:space-y-5">
            {/* ─── بطاقة الهوية الرئيسية ─── */}
            <div className="relative overflow-hidden rounded-2xl border bg-card ">
                {/* خلفية علوية متدرجة */}
                <div className="h-16 sm:h-28 bg-gradient-to-br from-primary/15 via-primary/5 to-background relative">
                    <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,var(--primary)_0%,transparent_60%)] opacity-100" />
                    {/* نمط زخرفي خفيف */}
                    <div className="absolute bottom-0 inset-x-0 h-px bg-gradient-to-r from-transparent via-primary/20 to-transparent" />
                </div>

                {/* محتوى البطاقة */}
                <div className="px-3 pb-3 sm:px-5 sm:pb-5 -mt-10 sm:-mt-14 relative">
                    <div className="flex flex-col items-center">
                        {/* الصورة الشخصية مع حلقة مميزة */}
                        <div className="relative group">
                            <div className="absolute -inset-0.5 bg-gradient-to-br from-primary/30 to-primary/10 rounded-2xl blur opacity-100 group-hover:opacity-100 transition duration-500" />
                            <EditableImage
                                src={resolvePersonalPhotoPath(retiree) ? getPhotoUrl(resolvePersonalPhotoPath(retiree)) : resolvePersonalPhotoPath(file) ? getPhotoUrl(resolvePersonalPhotoPath(file)) : !file.isAlive ? getDefaultDeceasedAvatar(file.gender) : null}
                                alt={file.retireeFullName}
                                className="relative w-20 h-28 sm:w-28 sm:h-36 rounded-2xl border-[4px] sm:border-[5px] border-card object-cover bg-muted transition-all duration-300 group-hover:scale-[1.02]"
                                fallbackIcon={<User className="w-8 h-8 sm:w-12 sm:h-12 text-muted-foreground" />}
                                canEdit={canEditRetiree}
                                imgWidth={112}
                                imgHeight={144}
                                onUpload={async (newFile) => {
                                    const res = await photoMutation.mutateAsync(newFile);
                                    if (res?.data?.data) return getPhotoUrl(res.data.data) ?? undefined;
                                }}
                            />
                            {/* شارة الحالة على الصورة */}
                            <div className={cn(
                                "absolute -bottom-1 -right-1 sm:-bottom-1.5 sm:-right-1.5 w-4 h-4 sm:w-5 sm:h-5 rounded-full border-[2px] sm:border-[3px] border-card ",
                                file.isAlive ? "bg-emerald-500" : "bg-red-500"
                            )}>
                                {file.isAlive && (
                                    <div className="absolute inset-0 rounded-full bg-emerald-500 animate-ping opacity-100" />
                                )}
                            </div>
                        </div>

                        {/* الاسم والرتبة */}
                        <div className="text-center mt-2.5 sm:mt-4 space-y-1 sm:space-y-1.5">
                            <Heading level="h2" className={cn(
                                "text-sm sm:text-lg leading-tight",
                                getStatusColorClass(file.statusID)
                            )}>
                                {file.retireeFullName}
                            </Heading>
                            <div className="inline-flex items-center gap-1 sm:gap-1.5 px-2 py-0.5 sm:px-3 sm:py-1 rounded-full bg-primary/8 text-primary text-[10px] sm:text-xs font-semibold">
                                <Shield className="w-2.5 h-2.5 sm:w-3 sm:h-3" />
                                {file.rankName || "رتبة غير محددة"}
                            </div>
                        </div>
                    </div>

                    {/* معلومات سريعة (غير مضغوط) */}
                    {!isCompact && (
                        <div className="mt-3 sm:mt-4 flex flex-col gap-2">
                            <InfoPill 
                                icon={<UserCircle2 className="w-3.5 h-3.5" />}
                                label="الرقم الوطني"
                                value={file.nationalID || "—"}
                                dir="ltr"
                            />
                            <InfoPill 
                                icon={<MapPin className="w-3.5 h-3.5" />}
                                label="العنوان"
                                value={retiree.governorate && retiree.city ? `${retiree.governorate}، ${retiree.city}` : retiree.governorate || retiree.city || "—"}
                            />
                        </div>
                    )}
                </div>
            </div>

            {/* ─── بطاقة الحالة والتفاصيل ─── */}
            <Card className="overflow-hidden">
                {/* شريط الحالة الرئيسي */}
                <Button
                    type="button"
                    variant="ghost"
                    className="w-full justify-between p-2.5 sm:p-3 h-auto lg:cursor-default"
                    onClick={() => setDetailsExpanded(!detailsExpanded)}
                >
                    <div className="flex items-center gap-2 sm:gap-2.5">
                        <div className={cn(
                            "w-7 h-7 sm:w-8 sm:h-8 rounded-lg flex items-center justify-center",
                            file.isAlive ? "bg-emerald-50 text-emerald-600" : "bg-red-50 text-red-600"
                        )}>
                            {file.isAlive ? <BadgeCheck className="w-3.5 h-3.5 sm:w-4 sm:h-4" /> : <AlertCircle className="w-3.5 h-3.5 sm:w-4 sm:h-4" />}
                        </div>
                        <div className="text-start">
                            <p className="text-[9px] sm:text-[11px] text-muted-foreground font-medium">حالة المتقاعد</p>
                            <p className="text-xs sm:text-sm font-bold text-foreground">
                                {file.isAlive ? "نشط (على قيد الحياة)" : "متوفي"}
                            </p>
                        </div>
                    </div>
                    <div className="flex items-center gap-1.5 sm:gap-2">
                        <div className={cn(
                            "px-2 py-0.5 sm:px-2.5 sm:py-1 rounded-full text-[9px] sm:text-[10px] sm:text-xs font-bold",
                            file.isAlive 
                                ? "bg-emerald-100 text-emerald-700" 
                                : "bg-red-100 text-red-700"
                        )}>
                            {file.isAlive ? "ACTIVE" : "INACTIVE"}
                        </div>
                        <span className="lg:hidden text-muted-foreground">
                            {detailsExpanded ? <ChevronUp className="w-3.5 h-3.5 sm:w-4 sm:h-4" /> : <ChevronDown className="w-3.5 h-3.5 sm:w-4 sm:h-4" />}
                        </span>
                    </div>
                </Button>

                {/* تفاصيل منظمة - قابل للطي على الهاتف */}
                <div className={cn(
                    "overflow-hidden transition-all duration-300",
                    detailsExpanded ? "max-h-96 opacity-100" : "max-h-0 opacity-0 lg:max-h-96 lg:opacity-100"
                )}>
                    <div className="px-2.5 pb-2.5 sm:px-3 sm:pb-3 space-y-0.5 border-t border-border pt-2 sm:pt-3">
                        <DetailRow 
                            icon={<Shield className="w-3 h-3 sm:w-3.5 sm:h-3.5" />}
                            label="الرقم العسكري" 
                            value={retiree.militaryNumber || "—"} 
                        />
                        <DetailRow 
                            icon={<Calendar className="w-3 h-3 sm:w-3.5 sm:h-3.5" />}
                            label="تاريخ الميلاد" 
                            value={formatDate(file.birthDate)} 
                        />
                        <DetailRow 
                            icon={<HeartPulse className="w-3 h-3 sm:w-3.5 sm:h-3.5" />}
                            label="الحالة الاجتماعية" 
                            value={retiree.socialStatusName || "—"} 
                        />
                        <DetailRow 
                            icon={<Activity className="w-3 h-3 sm:w-3.5 sm:h-3.5" />}
                            label="الحالة الصحية" 
                            value={retiree.healthStatusName || "—"} 
                        />
                    </div>
                </div>
            </Card>
        </div>
    );
}

/* ─── مكونات مساعدة ─── */

function InfoPill({ icon, label, value, dir }: { icon: React.ReactNode; label: string; value: string; dir?: string }) {
    return (
        <div className="flex items-center justify-between gap-2 p-2 sm:p-2.5 rounded-xl bg-muted/40 border border-border hover:bg-muted/60 transition-colors group min-w-0">
            <div className="flex items-center gap-1.5 text-muted-foreground shrink-0">
                {icon}
                <span className="text-xs font-bold">{label}</span>
            </div>
            <p className="text-xs font-semibold text-foreground truncate min-w-0 text-end" title={value} dir={dir}>
                {value}
            </p>
        </div>
    );
}

function DetailRow({ icon, label, value }: { icon: React.ReactNode; label: string; value: string }) {
    return (
        <div className="flex items-center justify-between py-2 sm:py-2.5 px-1.5 sm:px-2 rounded-lg hover:bg-muted/30 transition-colors group">
            <div className="flex items-center gap-1.5 sm:gap-2 text-muted-foreground">
                <span className="text-muted-foreground group-hover:text-primary transition-colors">
                    {icon}
                </span>
                <span className="text-[10px] sm:text-xs font-medium">{label}</span>
            </div>
            <span className="text-[11px] sm:text-sm font-semibold text-foreground">{value}</span>
        </div>
    );
}