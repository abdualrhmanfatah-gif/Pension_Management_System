import { useQuery } from "@tanstack/react-query";
import {
    DollarSign,
    FileText,
    FolderPlus,
    IdCard,
    Printer,
    Receipt,
    Shield,
    FolderOpen} from "lucide-react";
import { useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";

import { retireeApi } from "@/api/retirees";
import { RetirementFileDialog } from "@/components/domain/retirement/RetirementFileDialog";
import { DocumentsSection } from "@/components/domain/retirement/sections/DocumentsSection";
import {
    Badge,
    Button,
    Card,
    EditableImage,
    Heading,
    Skeleton,
    PageLayout,
    NativeSelect} from "@/components/ui";
import { BADGE_VARIANT,RETIREE_TAB } from "@/constants";
import { useApiMutation } from "@/hooks";
import { formatDate, getDefaultDeceasedAvatar, getPhotoUrl, resolvePersonalPhotoPath } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

// ─── Types ─────────────────────────────────────────────

interface Retiree {
    retireeID: number;
    fullName: string;
    nationalID: string;
    militaryNumber: string;
    rankName: string;
    unitName?: string;
    categoryName?: string;
    linkNumber?: string;
    retirementDate?: string;
    isAlive: boolean;
    birthDate: string;
    gender: number;
    governorate: string;
    city: string;
    socialStatusName: string;
    healthStatusName: string;
    photoPath: string | null;
    personalPhotoPath?: string | null;
}

interface Statement {
    retiree: Retiree;
    approvalItems: ApprovalItem[];
    retirementFiles: RetirementFile[];
    settlements: Settlement[];
    entitlements: Entitlement[];
}

interface ApprovalItem {
    approvalItemID: number;
    batchNumber: string;
    militaryNumber: string;
    fullName: string;
    gender: number;
    rankName: string;
    unitName: string;
    retirementDate: string;
    salary: number | null;
    categoryName: string;
    notes: string;
    isProcessed: boolean;
}

interface RetirementFile {
    createdAt(createdAt: unknown): import("react").ReactNode;
    fileID: number;
    fileNumber: string;
    statusName: string;
    rankName: string;
    branchName: string;
    fileOpenDate: string;
}

interface Settlement {
    settlementItemID: number;
    batchNumber: string;
    decisionNumber: string;
    decisionDate: string;
    decisionType: string;
    currentRankName: string;
    processedRankName: string;
    notes: string;
    createdAt: string;
}

interface Entitlement {
    entitlementTypeName: React.ReactNode;
    isActive: boolean;
    entitlementID: number;
    rankName: string;
    amount: number;
    effectiveDate: string;
    endDate: string | null;
}

/**
 * صفحة الملف الشامل للمتقاعد (البيانات التاريخية)
 */
export function RetireeDetailsPage() {
    const { id: idStr } = useParams<{ id: string }>();
    const id = Number(idStr);
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();
    const tabFromUrl = searchParams.get("tab");
    const validTabs = Object.values(RETIREE_TAB) as string[];
    const [activeTab, setActiveTab] = useState<typeof RETIREE_TAB[keyof typeof RETIREE_TAB]>(
        (tabFromUrl && validTabs.includes(tabFromUrl) ? tabFromUrl : RETIREE_TAB.APPROVAL) as typeof RETIREE_TAB[keyof typeof RETIREE_TAB]
    );
    const [createFileOpen, setCreateFileOpen] = useState(false);
    const [selectedFileId, setSelectedFileId] = useState<number | null>(null);

    const canEdit = useHasPermission("Retiree.Edit");
    const canCreateFile = useHasPermission("File.Create");
    const canViewStatement = useHasPermission("Ministry.View", "Ministry.Manage");
    const canViewFiles = useHasPermission("File.View.Sensitive", "File.Create", "File.Edit.BasicData");
    const canViewDocuments = useHasPermission("File.View.Sensitive", "File.Documents.Upload");
    const canViewSettlements = useHasPermission("Settlement.View", "Settlement.Manage");
    const canViewEntitlements = useHasPermission("File.Edit.FinancialData", "SalaryBatch.View");

    const { data: statement, isLoading: loading, refetch, error } = useQuery<Statement>({
        queryKey: ["retiree-statement", id],
        queryFn: () => retireeApi.getStatement(id),
        enabled: !!id
    });

    const updatePhotoMutation = useApiMutation(
        (file: File) => retireeApi.uploadPhoto(id, file),
        {
            successMessage: "تم تحديث الصورة الشخصية بنجاح",
            onSuccess: () => refetch()
        }
    );

    if (loading) return <RetireeDetailsSkeleton />;
    if (error || !statement) return <div className="p-6 sm:p-12 text-center text-destructive font-bold">فشل في تحميل بيانات المتقاعد.</div>;

    const { retiree } = statement;

    return (
        <PageLayout>
            {/* Header / Banner */}
            <div className="relative overflow-hidden rounded-3xl bg-primary/5 border border-primary/10 p-8 ">
                <div className="absolute top-0 end-0 -mt-10 -me-10 w-40 h-40 bg-primary/5 rounded-full blur-3xl" />
                <div className="relative flex flex-col md:flex-row items-center gap-8">
                    {/* Photo with Overlay */}
                    <div className="relative group">
                        <EditableImage
                            src={getPhotoUrl(resolvePersonalPhotoPath(retiree)) || (!retiree.isAlive ? getDefaultDeceasedAvatar(retiree.gender) : null)}
                            alt={retiree.fullName}
                            fallbackIcon={<IdCard className="w-16 h-16 text-muted-foreground/30" />}
                            className="w-32 h-32 md:w-40 md:h-40 rounded-3xl border-4 border-white"
                            canEdit={canEdit}
                            onUpload={async (file) => {
                                await updatePhotoMutation.mutateAsync(file);
                            }}
                        />
                    </div>

                    <div className="flex-1 space-y-2 text-center md:text-start">
                        <div className="flex flex-wrap items-center justify-center md:justify-start gap-3">
                            <Heading level="h1" className="text-xl md:text-3xl font-black tracking-tight text-foreground">{retiree.fullName}</Heading>
                            <div className="flex gap-2">
                                <Badge variant={BADGE_VARIANT.INFO} className="h-6 px-3 rounded-lg font-bold">
                                    {retiree.rankName}
                                </Badge>
                                <Badge
                                    variant={retiree.isAlive ? BADGE_VARIANT.SUCCESS : BADGE_VARIANT.OUTLINE}
                                    className="h-6 px-3 rounded-lg font-bold"
                                >
                                    {retiree.isAlive ? "على قيد الحياة" : "متوفى"}
                                </Badge>
                            </div>
                        </div>
                        <div className="flex flex-wrap items-center justify-center md:justify-start gap-x-8 gap-y-3 text-muted-foreground font-medium">
                            <div className="flex items-center gap-2">
                                <span className="font-label text-label uppercase text-muted-primary">رقم الربط</span>
                                <span className="font-mono text-primary-900 font-bold">{retiree.linkNumber || "—"}</span>
                            </div>
                            <div className="flex items-center gap-2">
                                <span className="font-label text-label uppercase text-muted-primary">الرقم العسكري</span>
                                <span className="font-mono text-foreground font-bold">{retiree.militaryNumber}</span>
                            </div>
                            <div className="flex items-center gap-2">
                                <span className="font-label text-label uppercase text-muted-primary">الرقم الوطني</span>
                                <span className="font-mono text-foreground font-bold">{retiree.nationalID}</span>
                            </div>
                            {retiree.unitName && (
                                <div className="flex items-center gap-2">
                                    <span className="font-label text-label uppercase text-muted-primary">الوحدة</span>
                                    <span className="text-foreground font-bold">{retiree.unitName}</span>
                                </div>
                            )}
                            {retiree.categoryName && (
                                <div className="flex items-center gap-2">
                                    <span className="font-label text-label uppercase text-muted-primary">الفئة</span>
                                    <span className="text-foreground font-bold">{retiree.categoryName}</span>
                                </div>
                            )}
                            {retiree.retirementDate && (
                                <div className="flex items-center gap-2">
                                    <span className="font-label text-label uppercase text-muted-primary">تاريخ التقاعد</span>
                                    <span className="font-mono text-foreground font-bold">{formatDate(retiree.retirementDate)}</span>
                                </div>
                            )}
                        </div>
                    </div>

                    <div className="flex flex-col gap-2 w-full md:w-auto">
                        {canViewStatement && (
                            <Button
                                variant="outline"
                                className="rounded-xl h-11 px-6 gap-2 border-primary/20 bg-white hover:bg-primary/5 transition-all w-full justify-start"
                                onClick={() => navigate(`/retirees/${id}/statement`)}
                            >
                                <Printer className="w-4 h-4" />
                                طباعة بيانات المتقاعد
                            </Button>
                        )}

                        {statement.retirementFiles && statement.retirementFiles.length > 0 ? (
                            canViewFiles && (
                                <Button
                                    className="rounded-xl h-11 px-6 gap-2 font-bold w-full justify-start bg-primary hover:bg-primary/90"
                                    onClick={() => navigate(`/files/${statement.retirementFiles[0].fileID}`)}
                                >
                                    <FileText className="w-4 h-4" />
                                    عرض الملف التقاعدي
                                </Button>
                            )
                        ) : (
                            canCreateFile && (
                                <Button
                                    className="rounded-xl h-11 px-6 gap-2 font-bold w-full justify-start bg-emerald-600 hover:bg-emerald-700 border-none"
                                    onClick={() => setCreateFileOpen(true)}
                                >
                                    <FolderPlus className="w-4 h-4" />
                                    فتح ملف تقاعدي جديد
                                </Button>
                            )
                        )}
                    </div>
                </div>
            </div>

            <div className="space-y-6">
                {/* Tabs / History Section */}
                <div className="w-full space-y-6">
                    <Card className="rounded-3xl p-2  inline-flex w-full">
                        {(Object.entries(RETIREE_TAB) as [keyof typeof RETIREE_TAB, typeof RETIREE_TAB[keyof typeof RETIREE_TAB]][])
                            .filter(([, value]) => {
                                if (value === RETIREE_TAB.FILES) return canViewFiles;
                                if (value === RETIREE_TAB.DOCUMENTS) return canViewDocuments;
                                if (value === RETIREE_TAB.SETTLEMENTS) return canViewSettlements;
                                if (value === RETIREE_TAB.ENTITLEMENTS) return canViewEntitlements;
                                return true; // APPROVAL is default
                            })
                            .map(([, value]) => {
                                const labels = {
                                    [RETIREE_TAB.APPROVAL]: { label: "بيانات الاعتماد", icon: Shield },
                                    [RETIREE_TAB.FILES]: { label: "الملفات التقاعدية", icon: FileText },
                                    [RETIREE_TAB.DOCUMENTS]: { label: "المستندات", icon: FolderOpen },
                                    [RETIREE_TAB.SETTLEMENTS]: { label: "التسويات", icon: Receipt },
                                    [RETIREE_TAB.ENTITLEMENTS]: { label: "الاستحقاقات", icon: DollarSign },
                                };
                                const tab = labels[value];
                                return (
                                    <Button
                                        key={value}
                                        variant="ghost"
                                        onClick={() => { setActiveTab(value); setSearchParams((p) => { p.set("tab", value); return p; }, { replace: true }); }}
                                        className={`flex-1 flex items-center justify-center gap-2 h-12 rounded-2xl text-sm font-bold transition-all ${activeTab === value
                                                ? "bg-primary text-white scale-[1.02] hover:bg-primary/90"
                                                : "text-muted-foreground hover:bg-muted"
                                            }`}
                                    >
                                        <tab.icon className={`w-4 h-4 ${activeTab === value ? "animate-bounce" : ""}`} />
                                        {tab.label}
                                    </Button>
                                );
                            })}
                    </Card>

                    {/* Tab Content Area */}
                    <Card className="rounded-3xl overflow-hidden  min-h-[250px] sm:min-h-[400px]">
                        {activeTab === RETIREE_TAB.APPROVAL && (
                            <div className="animate-in fade-in slide-in-from-bottom-4 duration-500 text-start">
                                <div className="overflow-x-auto">
                                    <table className="w-full text-sm" role="table" aria-label="عناصر الاعتماد">
                                        <thead className="bg-muted/30 border-b border-border/50 sticky top-0 z-10 bg-background">
                                            <tr>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">#</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">رقم الكشف</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">الرتبة</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">الوحدة</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">الفئة</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">تاريخ التقاعد</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">المعاش</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">الحالة</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">ملاحظات</th>
                                            </tr>
                                        </thead>
                                        <tbody className="divide-y divide-border/50">
                                            {statement.approvalItems?.map((item: ApprovalItem, idx: number) => (
                                                <tr key={item.approvalItemID} className="hover:bg-muted/20 transition-colors text-start">
                                                    <td className="px-4 py-3 text-muted-foreground">{idx + 1}</td>
                                                    <td className="px-4 py-3 font-mono font-bold text-primary">{item.batchNumber || "—"}</td>
                                                    <td className="px-4 py-3 font-medium">{item.rankName || "—"}</td>
                                                    <td className="px-4 py-3">{item.unitName || "—"}</td>
                                                    <td className="px-4 py-3">{item.categoryName || "—"}</td>
                                                    <td className="px-4 py-3 text-muted-foreground font-mono">{item.retirementDate ? formatDate(item.retirementDate) : "—"}</td>
                                                    <td className="px-4 py-3 font-mono font-bold">{item.salary != null ? `${item.salary.toLocaleString()} ر.ي` : "—"}</td>
                                                    <td className="px-4 py-3">
                                                        <Badge variant={item.isProcessed ? BADGE_VARIANT.SUCCESS : BADGE_VARIANT.WARNING} className="rounded-lg px-3">
                                                            {item.isProcessed ? "معالج" : "قيد الانتظار"}
                                                        </Badge>
                                                    </td>
                                                    <td className="px-4 py-3 text-muted-foreground text-xs max-w-[30vw] sm:max-w-[200px] truncate" title={item.notes || ""}>{item.notes || "—"}</td>
                                                </tr>
                                            ))}
                                            {(!statement.approvalItems || statement.approvalItems.length === 0) && <EmptyState text="لا توجد بيانات اعتماد لهذا المتقاعد." />}
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        )}

                        {activeTab === RETIREE_TAB.FILES && (
                            <div className="animate-in fade-in slide-in-from-bottom-4 duration-500 text-start">
                                <div className="overflow-x-auto">
                                    <table className="w-full text-sm" role="table" aria-label="ملفات التقاعد">
                                        <thead className="bg-muted/30 border-b border-border/50 sticky top-0 z-10 bg-background">
                                            <tr>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">رقم الملف</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">الرتبة</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">الفرع</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">تاريخ فتح الملف</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">الحالة</th>
                                            </tr>
                                        </thead>
                                        <tbody className="divide-y divide-border/50">
                                            {statement.retirementFiles?.map((f: RetirementFile) => (
                                                <tr key={f.fileID} className="hover:bg-muted/20 transition-colors text-start">
                                                    <td className="px-4 py-3 text-start">
                                                        <Link to={`/files/${f.fileID}`} className="text-primary font-black hover:underline underline-offset-4">
                                                            {f.fileNumber}
                                                        </Link>
                                                    </td>
                                                    <td className="px-4 py-3 font-medium">{f.rankName || "—"}</td>
                                                    <td className="px-4 py-3">{f.branchName}</td>
                                                    <td className="px-4 py-3 text-muted-foreground font-mono">{f.fileOpenDate ? formatDate(f.fileOpenDate) : "—"}</td>
                                                    <td className="px-4 py-3 text-start">
                                                        <Badge variant={f.statusName?.includes("نشط") ? BADGE_VARIANT.SUCCESS : BADGE_VARIANT.WARNING} className="rounded-lg px-3">
                                                            {f.statusName}
                                                        </Badge>
                                                    </td>
                                                </tr>
                                            ))}
                                            {(!statement.retirementFiles || statement.retirementFiles.length === 0) && <EmptyState text="لم يتم إنشاء أي ملف تقاعدي لهذا الشخص بعد." />}
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        )}

                        {activeTab === RETIREE_TAB.DOCUMENTS && (
                            <div className="animate-in fade-in slide-in-from-bottom-4 duration-500 text-start p-4">
                                {statement.retirementFiles && statement.retirementFiles.length > 0 ? (
                                    <div className="space-y-4">
                                        {statement.retirementFiles.length > 1 && (
                                            <NativeSelect
                                                value={selectedFileId?.toString() || statement.retirementFiles[0].fileID.toString()}
                                                onChange={(val) => setSelectedFileId(Number(val))}
                                                options={statement.retirementFiles.map((f) => ({
                                                    value: f.fileID.toString(),
                                                    label: `ملف رقم ${f.fileNumber} - ${f.statusName}`,
                                                }))}
                                            />
                                        )}
                                        <DocumentsSection
                                            fileId={selectedFileId || statement.retirementFiles[0].fileID}
                                        />
                                    </div>
                                ) : (
                                    <div className="text-center py-12 text-muted-foreground">
                                        <FolderOpen className="w-12 h-12 mx-auto mb-4 opacity-50" />
                                        <p>لا توجد ملفات تقاعدية لعرض مستنداتها</p>
                                    </div>
                                )}
                            </div>
                        )}

                        {activeTab === RETIREE_TAB.SETTLEMENTS && (
                            <div className="animate-in fade-in slide-in-from-bottom-4 duration-500 text-start">
                                <div className="overflow-x-auto">
                                    <table className="w-full text-sm" role="table" aria-label="التسويات">
                                        <thead className="bg-muted/30 border-b border-border/50 sticky top-0 z-10 bg-background">
                                            <tr>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">رقم الكشف</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">رقم القرار</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">نوع القرار</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">الرتبة السابقة</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">الرتبة الجديدة</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">تاريخ القرار</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">ملاحظات</th>
                                            </tr>
                                        </thead>
                                        <tbody className="divide-y divide-border/50">
                                            {statement.settlements?.map((s: Settlement) => (
                                                <tr key={s.settlementItemID} className="hover:bg-muted/20 transition-colors text-start">
                                                    <td className="px-4 py-3 font-mono font-bold">{s.batchNumber || "—"}</td>
                                                    <td className="px-4 py-3 font-mono">{s.decisionNumber || "—"}</td>
                                                    <td className="px-4 py-3">{s.decisionType || "—"}</td>
                                                    <td className="px-4 py-3">{s.currentRankName || "—"}</td>
                                                    <td className="px-4 py-3 font-bold text-emerald-600">{s.processedRankName || "—"}</td>
                                                    <td className="px-4 py-3 text-muted-foreground font-mono">{s.decisionDate ? formatDate(s.decisionDate) : "—"}</td>
                                                    <td className="px-4 py-3 text-muted-foreground text-xs max-w-[30vw] sm:max-w-[200px] truncate" title={s.notes || ""}>{s.notes || "—"}</td>
                                                </tr>
                                            ))}
                                            {(!statement.settlements || statement.settlements.length === 0) && <EmptyState text="لا توجد تسويات مسجلة." />}
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        )}

                        {activeTab === RETIREE_TAB.ENTITLEMENTS && (
                            <div className="animate-in fade-in slide-in-from-bottom-4 duration-500 text-start">
                                <div className="overflow-x-auto">
                                    <table className="w-full text-sm" role="table" aria-label="الاستحقاقات">
                                        <thead className="bg-muted/30 border-b border-border/50 sticky top-0 z-10 bg-background">
                                            <tr>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">الرتبة</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">المبلغ</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">تاريخ البدء</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">تاريخ الانتهاء</th>
                                                <th className="px-4 py-3 text-start font-bold text-muted-foreground text-xs">الحالة</th>
                                            </tr>
                                        </thead>
                                        <tbody className="divide-y divide-border/50">
                                            {statement.entitlements?.map((e: Entitlement) => (
                                                <tr key={e.entitlementID} className="hover:bg-muted/20 transition-colors text-start">
                                                    <td className="px-4 py-3 font-bold">{e.rankName || "—"}</td>
                                                    <td className="px-4 py-3 font-mono font-black text-primary">{e.amount.toLocaleString()} ر.ي</td>
                                                    <td className="px-4 py-3 text-muted-foreground font-mono">{formatDate(e.effectiveDate)}</td>
                                                    <td className="px-4 py-3 text-muted-foreground font-mono">{e.endDate ? formatDate(e.endDate) : "—"}</td>
                                                    <td className="px-4 py-3 text-start">
                                                        <Badge variant={!e.endDate ? BADGE_VARIANT.SUCCESS : BADGE_VARIANT.GHOST} className="rounded-lg">
                                                            {!e.endDate ? "نشط" : "منتهي"}
                                                        </Badge>
                                                    </td>
                                                </tr>
                                            ))}
                                            {(!statement.entitlements || statement.entitlements.length === 0) && <EmptyState text="لا توجد استحقاقات مالية مسجلة." />}
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        )}
                    </Card>
                </div>
            </div>

            <RetirementFileDialog
                open={createFileOpen}
                onOpenChange={setCreateFileOpen}
                retireeId={id}
                retireeName={retiree.fullName}
                onCreated={(fileId) => navigate(`/files/${fileId}`)}
                onSuccess={async () => { await refetch(); }}
            />
        </PageLayout>
    );
}

function EmptyState({ text }: { text: string }) {
    return (
        <tr>
            <td colSpan={5} className="px-6 py-12 text-center text-muted-foreground italic font-medium bg-muted/5">
                {text}
            </td>
        </tr>
    );
}

function RetireeDetailsSkeleton() {
    return (
        <div className="space-y-8 text-start" dir="rtl">
            <Skeleton className="h-48 w-full rounded-3xl" />
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                <Skeleton className="h-48 sm:h-96 rounded-3xl" />
                <div className="lg:col-span-2 space-y-6">
                    <Skeleton className="h-16 rounded-2xl" />
                    <Skeleton className="h-[200px] sm:h-[400px] rounded-3xl" />
                </div>
            </div>
        </div>
    );
}
