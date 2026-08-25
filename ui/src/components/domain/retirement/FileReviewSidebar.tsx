import { Users, UserCheck, FileText, DollarSign, History, FileWarning, Shield, MapPin, UserCircle2, User } from "lucide-react";

import { Badge, Button } from "@/components/ui";
import { Heading } from "@/components/ui";
import { Text } from "@/components/ui";
import { RecordStateBadge } from "@/components/ui";
import { cn, getDefaultDeceasedAvatar, getStatusColorClass, getPhotoUrl, resolvePersonalPhotoPath } from "@/lib/utils";
import type { RetirementFileDetail, RetireeDetail, ReviewSection } from "@/types/retirement";
import { REVIEW_SECTIONS, REVIEW_SECTIONS_AR } from "@/constants";

interface FileReviewSidebarProps {
    file: RetirementFileDetail;
    retiree: RetireeDetail;
    activeSection: ReviewSection;
    onSectionChange: (section: ReviewSection) => void;
    missingDocsCount: number;
    missingDataCount: number;
    pendingReviewCount: number;
}

const navItems: { key: ReviewSection; icon: typeof Users; permission?: string }[] = [
    { key: REVIEW_SECTIONS.BASIC, icon: Users },
    { key: REVIEW_SECTIONS.PEOPLE, icon: UserCheck },
    { key: REVIEW_SECTIONS.DOCUMENTS, icon: FileText },
    { key: REVIEW_SECTIONS.FINANCIAL, icon: DollarSign },
    { key: REVIEW_SECTIONS.HISTORY, icon: History },
];

export function FileReviewSidebar({
    file,
    retiree,
    activeSection,
    onSectionChange,
    missingDocsCount,
    pendingReviewCount: _pendingReviewCount,
}: FileReviewSidebarProps) {
    const photoUrl = getPhotoUrl(resolvePersonalPhotoPath(retiree)) ?? (getPhotoUrl(resolvePersonalPhotoPath(file)) ?? (!file.isAlive ? getDefaultDeceasedAvatar(file.gender) : null));

    return (
        <div className="flex flex-col h-full min-w-0">
            {/* ─── بطاقة الهوية الرئيسية ─── */}
            <div className="relative overflow-hidden border-b border-border">
                {/* خلفية علوية متدرجة */}
                <div className="h-16 bg-gradient-to-br from-primary/15 via-primary/5 to-background relative">
                    <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,var(--primary)_0%,transparent_60%)] opacity-[0.07]" />
                    <div className="absolute bottom-0 inset-x-0 h-px bg-gradient-to-r from-transparent via-primary/20 to-transparent" />
                </div>

                {/* محتوى البطاقة */}
                <div className="px-3 pb-4 -mt-10 relative">
                    <div className="flex flex-col items-center">
                        {/* الصورة الشخصية */}
                        <div className="relative group">
                            <div className="absolute -inset-0.5 bg-gradient-to-br from-primary/30 to-primary/10 rounded-2xl blur opacity-50 group-hover:opacity-75 transition duration-500" />
                            {photoUrl ? (
                                <img
                                    src={photoUrl}
                                    alt={file.retireeFullName}
                                    className="relative w-20 h-28 rounded-2xl border-[4px] border-card object-cover bg-muted transition-all duration-300 group-hover:scale-[1.02]"
                                />
                            ) : (
                                <div className="relative w-20 h-28 rounded-2xl border-[4px] border-card bg-muted flex items-center justify-center">
                                    <User className="w-8 h-8 text-muted-foreground/60" />
                                </div>
                            )}
                            {/* شارة الحالة على الصورة */}
                            <div className={cn(
                                "absolute -bottom-1 -right-1 w-4 h-4 rounded-full border-[2px] border-card",
                                file.isAlive ? "bg-emerald-500" : "bg-red-500"
                            )}>
                                {file.isAlive && (
                                    <div className="absolute inset-0 rounded-full bg-emerald-500 animate-ping opacity-40" />
                                )}
                            </div>
                        </div>

                        {/* الاسم والرتبة */}
                        <div className="text-center mt-2.5 space-y-1 min-w-0 w-full">
                            <Heading level="h4" className={cn(
                                "text-sm leading-tight truncate",
                                getStatusColorClass(file.statusID)
                            )}>
                                {file.retireeFullName}
                            </Heading>
                            <div className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-primary/8 text-primary text-[10px] font-semibold">
                                <Shield className="w-2.5 h-2.5" />
                                <span className="truncate">{file.rankName || "رتبة غير محددة"}</span>
                            </div>
                        </div>
                    </div>

                    {/* معلومات سريعة */}
                    <div className="mt-3 space-y-1.5">
                        <InfoPill
                            icon={<UserCircle2 className="w-3 h-3" />}
                            label="الرقم الوطني"
                            value={file.nationalID || "—"}
                            dir="ltr"
                        />
                        <InfoPill
                            icon={<MapPin className="w-3 h-3" />}
                            label="العنوان"
                            value={`${retiree.governorate || "—"} — ${retiree.city || "—"}`}
                        />
                    </div>

                    {/* شارة الحالة */}
                    <div className="flex justify-center mt-3">
                        <RecordStateBadge state={(file.statusID as any) || "active"} />
                    </div>
                </div>
            </div>

            {/* ─── التنقل بين الأقسام ─── */}
            <nav className="flex-1 py-2 px-2 space-y-1">
                {navItems.map((item) => {
                    const Icon = item.icon;
                    const isActive = activeSection === item.key;
                    return (
                        <Button
                            key={item.key}
                            variant={isActive ? "default" : "ghost"}
                            className="w-full justify-start gap-2 px-3 py-2.5 h-auto truncate"
                            onClick={() => onSectionChange(item.key)}
                        >
                            <Icon className="w-4 h-4 shrink-0" />
                            <span className="truncate">{REVIEW_SECTIONS_AR[item.key]}</span>
                            {item.key === REVIEW_SECTIONS.DOCUMENTS && missingDocsCount > 0 && (
                                <Badge variant="danger" className="me-auto text-[10px] px-1.5 py-0.5 shrink-0">
                                    {missingDocsCount}
                                </Badge>
                            )}
                        </Button>
                    );
                })}
            </nav>

            {missingDocsCount > 0 && (
                <div className="px-3 pb-3">
                    <div className="bg-red-50 dark:bg-red-950/20 border border-red-200 dark:border-red-800 rounded-lg px-2 py-2 flex items-center gap-2">
                        <FileWarning className="w-4 h-4 text-red-500 shrink-0" />
                        <Text variant="small" className="text-red-700 dark:text-red-400 truncate">
                            {missingDocsCount} وثائق ناقصة
                        </Text>
                    </div>
                </div>
            )}
        </div>
    );
}

/* ─── مكون مساعد ─── */
function InfoPill({ icon, label, value, dir }: { icon: React.ReactNode; label: string; value: string; dir?: string }) {
    return (
        <div className="flex items-center justify-between p-2 rounded-xl bg-muted/40 border border-border/40 hover:bg-muted/60 transition-colors group min-w-0">
            <div className="flex items-center gap-1.5 text-muted-foreground shrink-0">
                {icon}
                <span className="text-[9px] font-bold uppercase tracking-wider">{label}</span>
            </div>
            <p className="text-[10px] font-mono font-semibold text-foreground truncate min-w-0 ms-2" title={value} dir={dir}>
                {value}
            </p>
        </div>
    );
}
