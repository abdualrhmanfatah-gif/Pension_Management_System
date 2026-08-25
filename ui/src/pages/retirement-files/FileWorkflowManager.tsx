import {
    Ban,
    BookOpen,
    CheckCircle2,
    CreditCard,
    DollarSign,
    FileSignature,
    FileText,
    History,
    Scale,
    ShieldCheck,
    StickyNote,
    Target,
    UserCheck,
    Users,
    Wallet,
} from "lucide-react";
import React, { lazy, Suspense, useCallback, useMemo } from "react";

import { Card, type StepItem,Stepper, Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui";
import { Skeleton } from "@/components/ui/Skeleton";
import { useAuthStore } from "@/stores/authStore";
import type { RetirementFileDetail } from "@/types/retirement";

// ─── Lazy-loaded Tab Components (Named Exports) ─────────
const DocumentsTab = lazy(() => import("./tabs/DocumentsTab").then(m => ({ default: m.DocumentsTab })));
const EntitledPersonsTab = lazy(() => import("./tabs/EntitledPersonsTab").then(m => ({ default: m.EntitledPersonsTab })));
const GuardiansTab = lazy(() => import("./tabs/GuardiansTab").then(m => ({ default: m.GuardiansTab })));
const NotesTab = lazy(() => import("./tabs/NotesTab").then(m => ({ default: m.NotesTab })));
const SuspensionsTab = lazy(() => import("./tabs/SuspensionsTab").then(m => ({ default: m.SuspensionsTab })));
const PledgesAndExemptionsTab = lazy(() => import("./tabs/PledgesAndExemptionsTab").then(m => ({ default: m.PledgesAndExemptionsTab })));
const AgentsTab = lazy(() => import("./tabs/AgentsTab").then(m => ({ default: m.AgentsTab })));
const RecipientsTab = lazy(() => import("@/components/domain/salaries/RecipientsTab").then(m => ({ default: m.RecipientsTab })));
const StatusTransitionsTab = lazy(() => import("./tabs/StatusTransitionsTab").then(m => ({ default: m.StatusTransitionsTab })));
const SettlementsTab = lazy(() => import("./tabs/SettlementsTab").then(m => ({ default: m.SettlementsTab })));
const PaymentBooksTab = lazy(() => import("./tabs/PaymentBooksTab").then(m => ({ default: m.PaymentBooksTab })));
const FinancialEntitlementsTab = lazy(() => import("./tabs/FinancialEntitlementsTab").then(m => ({ default: m.FinancialEntitlementsTab })));
const SalaryPaymentsTab = lazy(() => import("./tabs/SalaryPaymentsTab").then(m => ({ default: m.SalaryPaymentsTab })));
const ReviewTab = lazy(() => import("./tabs/ReviewTab").then(m => ({ default: m.ReviewTab })));

// ─── Types ─────────────────────────────────────────────

export type TabKey =
    | "entitled"
    | "agents"
    | "guardians"
    | "notes"
    | "suspensions"
    | "pledges"
    | "recipients"
    | "documents"
    | "statusTransitions"
    | "settlements"
    | "paymentBooks"
    | "entitlements"
    | "salaryPayments"
    | "review"
    | "achievements";

interface TabConfig {
    key: TabKey;
    label: string;
    icon: React.ReactNode;
    countKey?: keyof RetirementFileDetail;
    requiredPermission?: string;
}

interface WorkflowSection {
    id: string;
    label: string;
    description: string;
    tabs: TabKey[];
}

const sections: WorkflowSection[] = [
    {
        id: "data-entry",
        label: "البيانات الأساسية",
        description: "المستحقون والوكلاء",
        tabs: ["entitled", "guardians", "agents", "notes"],
    },
    {
        id: "documents",
        label: "الوثائق والإجراءات",
        description: "الأرشفة والتعهدات",
        tabs: ["documents", "pledges", "suspensions"],
    },
    {
        id: "financial",
        label: "المالية والمعاشات",
        description: "الحسابات والدفاتر",
        tabs: ["entitlements", "salaryPayments", "paymentBooks", "settlements", "recipients"],
    },
    {
        id: "review",
        label: "المراجعة والاعتماد",
        description: "التدقيق النهائي",
        tabs: ["statusTransitions", "review"],
    },
];

const allTabsConfig: Record<TabKey, TabConfig> = {
    entitled: { key: "entitled", label: "المستحقون", icon: <Users className="w-4 h-4" />, requiredPermission: "EntitledPerson.View", countKey: "entitledPersonsCount" },
    guardians: { key: "guardians", label: "الأوصياء", icon: <ShieldCheck className="w-4 h-4" />, requiredPermission: "Guardian.View", countKey: "guardiansCount" },
    agents: { key: "agents", label: "الوكلاء", icon: <UserCheck className="w-4 h-4" />, requiredPermission: "File.Edit.BasicData", countKey: "agentsCount" },
    notes: { key: "notes", label: "الملاحظات", icon: <StickyNote className="w-4 h-4" />, requiredPermission: "File.Edit.BasicData", countKey: "notesCount" },
    documents: { key: "documents", label: "الوثائق", icon: <FileText className="w-4 h-4" />, requiredPermission: "File.View.Sensitive", countKey: "documentsCount" },
    pledges: { key: "pledges", label: "التعهدات والإعفاءات", icon: <FileSignature className="w-4 h-4" />, requiredPermission: "DocumentPledge.View", countKey: "pledgesCount" },
    suspensions: { key: "suspensions", label: "إيقاف المعاش", icon: <Ban className="w-4 h-4" />, requiredPermission: "Suspension.View", countKey: "suspensionsCount" },
    entitlements: { key: "entitlements", label: "الاستحقاقات", icon: <DollarSign className="w-4 h-4" />, requiredPermission: "File.Edit.FinancialData", countKey: "entitlementsCount" },
    salaryPayments: { key: "salaryPayments", label: "المعاشات", icon: <CreditCard className="w-4 h-4" />, requiredPermission: "File.Edit.FinancialData", countKey: "salaryPaymentsCount" },
    paymentBooks: { key: "paymentBooks", label: "دفاتر الصرف", icon: <BookOpen className="w-4 h-4" />, requiredPermission: "File.Edit.FinancialData", countKey: "paymentBooksCount" },
    settlements: { key: "settlements", label: "التسويات", icon: <Scale className="w-4 h-4" />, requiredPermission: "File.Edit.FinancialData", countKey: "settlementsCount" },
    recipients: { key: "recipients", label: "مستلم المعاش", icon: <Wallet className="w-4 h-4" />, requiredPermission: "File.Edit.BasicData", countKey: "recipientsCount" },
    statusTransitions: { key: "statusTransitions", label: "سجل التغييرات", icon: <History className="h-4 w-4" />,requiredPermission:"File.View.History", countKey: "statusTransitionsCount" },
    review: { key: "review", label: "المراجعة", icon: <CheckCircle2 className="h-4 w-4" />, requiredPermission: "File.ChangeStatus.Approve", countKey: "reviewCount" },
    achievements: { key: "achievements", label: "الإنجازات والمعوقات", icon: <Target className="w-4 h-4" />, requiredPermission: "AchievementObstacle.View", countKey: "achievementsCount" },
};

interface FileWorkflowManagerProps {
    file: RetirementFileDetail;
    activeTab: TabKey;
    onTabChange: (tab: TabKey) => void;
    isTabVisible?: (tab: TabKey) => boolean;
    isCompact?: boolean;
}

export function FileWorkflowManager({
    file,
    activeTab,
    onTabChange,
    isTabVisible: externalIsTabVisible
}: FileWorkflowManagerProps) {
    const permissions = useAuthStore((s) => s.permissions);
    const currentSectionIndex = useMemo(() => {
        return sections.findIndex(s => s.tabs.includes(activeTab));
    }, [activeTab]);

    const activeSection = sections[currentSectionIndex] || sections[0];

    const isTabVisible = useCallback((tabKey: TabKey) => {
        const config = allTabsConfig[tabKey];
        if (config.requiredPermission && !permissions.includes(config.requiredPermission)) return false;
        if (externalIsTabVisible) return externalIsTabVisible(tabKey);
        return true;
    }, [permissions, externalIsTabVisible]);

    const visibleTabsInActiveSection = useMemo(() => {
        return activeSection.tabs.filter(isTabVisible);
    }, [activeSection, isTabVisible]);

    const handleSectionClick = (index: number) => {
        const section = sections[index];
        const firstVisibleTab = section.tabs.find(isTabVisible);
        if (firstVisibleTab) {
            onTabChange(firstVisibleTab);
        }
    };

    const stepItems: StepItem[] = sections.map(s => ({
        label: s.label,
        description: s.description,
    }));

    return (
        <div className="space-y-3 md:space-y-8">
            <Card className="p-3 sm:p-6 rounded-2xl">
                <Stepper
                    steps={stepItems}
                    activeStep={currentSectionIndex}
                    onStepClick={handleSectionClick}
                    allowAllStepsClickable={true}
                />
            </Card>

            <Tabs dir="rtl" value={activeTab} onValueChange={(v) => onTabChange(v as TabKey)} className="space-y-2 md:space-y-6 text-start">
                <div className="relative border-b border-border">
                    <TabsList className="h-auto bg-transparent p-0 gap-0 w-full flex overflow-x-auto scrollbar-hide">
                        {visibleTabsInActiveSection.map((tabKey) => {
                            const config = allTabsConfig[tabKey];
                            return (
                                <TabsTrigger
                                    key={tabKey}
                                    value={tabKey}
                                    className="group relative gap-2 px-4 py-3 whitespace-nowrap flex-shrink-0 rounded-none border-0 bg-transparent font-semibold text-sm text-muted-foreground hover:text-foreground hover:bg-muted/40 transition-colors duration-200 data-[state=active]:text-primary data-[state=active]:bg-transparent after:absolute after:bottom-0 after:inset-x-0 after:h-0.5 after:bg-primary after:scale-x-0 after:transition-transform after:duration-200 data-[state=active]:after:scale-x-100"
                                >
                                    {config.icon}
                                    {config.label}
                                    {config.countKey && file[config.countKey] !== undefined && (file[config.countKey] as number) > 0 && (
                                        <span className="px-1.5 py-0.5 rounded-full bg-primary/10 text-primary group-data-[state=active]:bg-primary group-data-[state=active]:text-primary-foreground text-[10px] sm:text-xs font-bold tabular-nums">
                                            {file[config.countKey]}
                                        </span>
                                    )}
                                </TabsTrigger>
                            );
                        })}
                    </TabsList>
                </div>


                <div className="mt-6">
                    <Suspense fallback={<TabLoadingSkeleton />}>
                        <TabsContent value="entitled" className="m-0 focus-visible:outline-none">
                            <EntitledPersonsTab fileId={file.fileID} />
                        </TabsContent>
                        <TabsContent value="guardians" className="m-0 focus-visible:outline-none">
                            <GuardiansTab fileId={file.fileID} />
                        </TabsContent>
                        <TabsContent value="agents" className="m-0 focus-visible:outline-none">
                            <AgentsTab fileId={file.fileID} fileNumber={file.fileNumber} />
                        </TabsContent>
                        <TabsContent value="notes" className="m-0 focus-visible:outline-none">
                            <NotesTab fileId={file.fileID} />
                        </TabsContent>
                        <TabsContent value="documents" className="m-0 focus-visible:outline-none">
                            <DocumentsTab fileId={file.fileID} retireeName={file.retireeFullName} />
                        </TabsContent>
                        <TabsContent value="pledges" className="m-0 focus-visible:outline-none">
                            <PledgesAndExemptionsTab fileId={file.fileID} retireeName={file.retireeFullName} />
                        </TabsContent>
                        <TabsContent value="suspensions" className="m-0 focus-visible:outline-none">
                            <SuspensionsTab fileId={file.fileID} />
                        </TabsContent>
                        <TabsContent value="entitlements" className="m-0 focus-visible:outline-none">
                            <FinancialEntitlementsTab fileId={file.fileID} retireeId={file.retireeID} />
                        </TabsContent>
                        <TabsContent value="salaryPayments" className="m-0 focus-visible:outline-none">
                            <SalaryPaymentsTab fileId={file.fileID} />
                        </TabsContent>
                        <TabsContent value="paymentBooks" className="m-0 focus-visible:outline-none">
                            <PaymentBooksTab fileId={file.fileID} />
                        </TabsContent>
                        <TabsContent value="settlements" className="m-0 focus-visible:outline-none">
                            <SettlementsTab retireeId={file.retireeID} />
                        </TabsContent>
                        <TabsContent value="recipients" className="m-0 focus-visible:outline-none">
                            <RecipientsTab fileId={file.fileID} retireeId={file.retireeID} retireeName={file.retireeFullName} />
                        </TabsContent>
                        <TabsContent value="statusTransitions" className="m-0 focus-visible:outline-none">
                            <StatusTransitionsTab fileId={file.fileID} />
                        </TabsContent>
                        <TabsContent value="review" className="m-0 focus-visible:outline-none">
                            <ReviewTab fileId={file.fileID} file={file} />
                        </TabsContent>
                    </Suspense>
                </div>
            </Tabs>
        </div>
    );
}

function TabLoadingSkeleton() {
    return (
        <div className="space-y-4">
            <div className="flex items-center justify-between">
                <Skeleton className="h-8 w-full max-w-[180px]" />
                <Skeleton className="h-10 w-32" />
            </div>
            <div className="rounded-xl border border-dashed p-4 sm:p-8 lg:p-12 flex items-center justify-center">
                <div className="space-y-3 text-center">
                    <Skeleton className="h-12 w-12 rounded-full mx-auto" />
                    <Skeleton className="h-4 w-32 mx-auto" />
                    <Skeleton className="h-3 w-full max-w-[180px] mx-auto" />
                </div>
            </div>
        </div>
    );
}
