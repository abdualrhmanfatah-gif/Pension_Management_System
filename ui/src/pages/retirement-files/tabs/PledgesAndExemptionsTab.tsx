import { useQuery } from "@tanstack/react-query";
import { CheckCircle, Edit2, FileSignature, Plus, Printer,ShieldCheck, X } from "lucide-react";
import { useCallback,useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";

<<<<<<< HEAD
=======

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { documentsApi } from "@/api/documents";
import {
    type CreateExemptionDto,
    type CreatePledgeDto,
    retirementApi} from "@/api/retirement";
<<<<<<< HEAD
import { ExemptionDialog, type ExemptionFormValues } from "@/components/domain/retirement/ExemptionDialog";
import { FulfillPledgeDialog, type FulfillPledgeFormValues } from "@/components/domain/retirement/FulfillPledgeDialog";
import { PledgeDialog, type PledgeFormValues } from "@/components/domain/retirement/PledgeDialog";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
    Badge,
    Button,
    type CustomColumnDef,
    DataTable,
    LoadingIndicator,
<<<<<<< HEAD
    TabHeader,
    useConfirm} from "@/components/ui";
=======
    useConfirm} from "@/components/ui";
import { ExemptionDialog, type ExemptionFormValues } from "@/components/domain/retirement/ExemptionDialog";
import { FulfillPledgeDialog, type FulfillPledgeFormValues } from "@/components/domain/retirement/FulfillPledgeDialog";
import { PledgeDialog, type PledgeFormValues } from "@/components/domain/retirement/PledgeDialog";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { useApiMutation, useCreateShortcut } from "@/hooks";
import { useAgentsQuery, useEntitledPersonsQuery, useGuardiansQuery, useMissingDocumentsQuery } from "@/hooks/queries";
import { cn, formatDate } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type {
    AgentSummary,
    EntitledPersonSummary,
    ExemptionItem,
    GuardianSummary,
    MissingDocumentInfo,
    PledgeItem} from "@/types/retirement";
import { mapMissingDocsForDialog,resolveMissingDocumentIds } from "@/utils/missing-documents";

interface PledgesAndExemptionsTabProps {
    fileId: number;
    retireeName?: string;
}

type TabType = "pledges" | "exemptions";

/**
 * تبويب إدارة التعهدات والإعفاءات للملف التقاعدي
 */
export function PledgesAndExemptionsTab({ fileId, retireeName }: PledgesAndExemptionsTabProps) {
    const navigate = useNavigate();
    const { confirm, ConfirmDialogElement } = useConfirm();

    // الصلاحيات
    const canCreatePledge = useHasPermission("DocumentPledge.Add");
    const canEditPledge = useHasPermission("DocumentPledge.Edit");
    const canManagePledge = useHasPermission("DocumentPledge.Manage");
    const canCreateExemption = useHasPermission("DocumentExemption.Add");
    const canEditExemption = useHasPermission("DocumentExemption.Edit");
    const canManageExemption = useHasPermission("DocumentExemption.Manage");

    const [activeTab, setActiveTab] = useState<TabType>("pledges");

    const [dialogState, setDialogState] = useState({
        pledge: { open: false, mode: "create" as "create" | "edit", data: null as PledgeItem | null, shouldPrint: false },
        exemption: { open: false, mode: "create" as "create" | "edit", data: null as ExemptionItem | null },
        fulfill: { open: false, data: null as PledgeItem | null }
    });

    const handleOpenCreatePledge = useCallback(() => {
        setDialogState(s => ({ ...s, pledge: { open: true, mode: "create", data: null, shouldPrint: false } }));
    }, []);

    const handleOpenCreateExemption = useCallback(() => {
        setDialogState(s => ({ ...s, exemption: { open: true, mode: "create", data: null } }));
    }, []);

    // جلب البيانات
    const { data: peData, isLoading: isPEDataLoading, isError: peError, refetch: refetchPE } = useQuery({
        queryKey: ["pledges-exemptions", fileId],
        queryFn: async () => {
            const [pledgesRes, exemptionsRes] = await Promise.all([
                retirementApi.getPledges(fileId),
                retirementApi.getExemptions(fileId)
            ]);
            return {
                pledges: (pledgesRes as PledgeItem[]).map(p => ({ ...p, missingDocuments: p.missingDocuments || [] })),
                exemptions: (exemptionsRes as ExemptionItem[]).map(e => ({ ...e, missingDocuments: e.missingDocuments || [] }))
            };
        }
    });

    const pledges = peData?.pledges || [];
    const exemptions = peData?.exemptions || [];

    const { data: missingDocs = [], isLoading: isLoadingDocs, refetch: refetchMissing } = useMissingDocumentsQuery(fileId);
    const { data: agents = [], isLoading: isLoadingAgents } = useAgentsQuery(fileId);
    const { data: entitleds = [], isLoading: isLoadingEntitleds } = useEntitledPersonsQuery(fileId);
    const { data: guardians = [], isLoading: isLoadingGuardians } = useGuardiansQuery(fileId);

    const isLoading = isPEDataLoading || isLoadingDocs || isLoadingAgents || isLoadingEntitleds || isLoadingGuardians;

    const lookups = useMemo(() => ({
        activeAgent: agents.find((a: AgentSummary) => a.isActive) ? { id: agents.find((a: AgentSummary) => a.isActive)!.agentID, fullName: agents.find((a: AgentSummary) => a.isActive)!.fullName } : null,
        agents: agents.map((a: AgentSummary) => ({ id: a.agentID, fullName: a.fullName })),
        entitleds: entitleds.map((e: EntitledPersonSummary) => ({ id: e.entitledID, fullName: e.fullName })),
        guardians: guardians.map((g: GuardianSummary) => ({ id: g.guardianID, fullName: g.fullName })),
    }), [agents, entitleds, guardians]);

    // طفرات البيانات
    const pledgeMutation = useApiMutation(async (payload: { values: CreatePledgeDto & { pledgeFile?: File }; id?: number; shouldPrint?: boolean; keepOpen?: boolean }) => {
        if (payload.id) {
            const res = await retirementApi.updatePledge(payload.id, payload.values);
            return { ...res, keepOpen: payload.keepOpen };
        } else {
            const { pledgeFile, ...dto } = payload.values;
            const res = await retirementApi.createPledge(dto);
            if (pledgeFile && res.isSuccess) {
                const formData = new FormData();
                formData.append("File", pledgeFile);
                formData.append("DocumentTypeID", "9"); // وثيقة تعهد
                formData.append("RetirementFileID", String(fileId));
                formData.append("PledgeID", String(res.data));
                await documentsApi.upload(formData);
            }
<<<<<<< HEAD
            return { ...res, keepOpen: payload.keepOpen };
=======
            return res;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        }
    }, {
        successMessage: "تم حفظ التعهد بنجاح",
        invalidateKeys: [["pledges-exemptions", fileId], ["retirement-file"], ["documents", fileId]],
        onSuccess: (res) => {
            const result = res as { shouldPrint?: boolean; data?: { pledgeID: number }; keepOpen?: boolean };
            if (!result?.keepOpen) {
                setDialogState(s => ({ ...s, pledge: { open: false, mode: "create", data: null, shouldPrint: false } }));
            }
            if (result?.shouldPrint && result?.data?.pledgeID) {
                navigate(`/pledges/${result.data.pledgeID}/print`);
            }
        }
    });

    const exemptionMutation = useApiMutation(async (payload: { values: CreateExemptionDto & { exemptionFile?: File }; id?: number }) => {
        if (payload.id) {
            return retirementApi.updateExemption(payload.id, payload.values);
        } else {
            const { exemptionFile, ...dto } = payload.values;
            const res = await retirementApi.createExemption(dto);
            if (exemptionFile && res.isSuccess) {
                const formData = new FormData();
                formData.append("File", exemptionFile);
                formData.append("DocumentTypeID", "10"); // توجيه إعفاء
                formData.append("RetirementFileID", String(fileId));
                formData.append("ExemptionID", String(res.data));
                await documentsApi.upload(formData);
            }
            return res;
        }
    }, {
        successMessage: "تم حفظ الإعفاء بنجاح",
        invalidateKeys: [["pledges-exemptions", fileId], ["retirement-file"], ["documents", fileId]],
        onSuccess: () => setDialogState(s => ({ ...s, exemption: { open: false, mode: "create", data: null } }))
    });

    const fulfillPledgeMutation = useApiMutation(async (values: FulfillPledgeFormValues) => {
        const docsToUpload = values.documents.filter((d) => d.file);
        
        if (docsToUpload.length === 0) return { isSuccess: true };

        const results = [];
        for (const doc of docsToUpload) {
            const originalDoc = dialogState.fulfill.data?.missingDocuments?.find(m => m.missingDocumentID === doc.missingDocumentID);
            const formData = new FormData();
            if (doc.file) formData.append("File", doc.file);
            formData.append("DocumentTypeID", String(originalDoc?.documentTypeID));
            formData.append("RetirementFileID", String(fileId));
            
            if (originalDoc?.agentID) formData.append("AgentID", String(originalDoc.agentID));
            if (originalDoc?.entitledID) formData.append("EntitledPersonID", String(originalDoc.entitledID));
            if (originalDoc?.guardianID) formData.append("GuardianID", String(originalDoc.guardianID));
            
            try {
                const res = await documentsApi.upload(formData);
                results.push(res);
            } catch (err) {
                console.error(`Failed to upload document ${doc.missingDocumentID}:`, err);
                throw err; // Stop the sequence if one fails (safer for transaction integrity)
            }
        }
        
        return results;
    }, {
        successMessage: "تم رفع الوثائق وتحديث حالة التعهد بنجاح",
        invalidateKeys: [["pledges-exemptions", fileId], ["retirement-file"], ["documents", fileId], ["missing-documents", fileId]],
        onSuccess: () => {
            setDialogState(s => ({ ...s, fulfill: { open: false, data: null } }));
            // Refetch is already handled by invalidateKeys, but explicit call for immediate UI update
            void refetchPE();
            void refetchMissing();
        }
    });

    const deletePledgeMutation = useApiMutation((id: number) => retirementApi.deletePledge(id), {
        successMessage: "تم حذف التعهد بنجاح",
        invalidateKeys: [["pledges-exemptions", fileId], ["retirement-file"]]
    });

    const deleteExemptionMutation = useApiMutation((id: number) => retirementApi.deleteExemption(id), {
        successMessage: "تم حذف الإعفاء بنجاح",
        invalidateKeys: [["pledges-exemptions", fileId], ["retirement-file"]]
    });

    // المعالجات
<<<<<<< HEAD
    const handlePledgeSubmit = async (values: PledgeFormValues, options?: { keepOpen?: boolean }) => {
=======
    const handlePledgeSubmit = async (values: PledgeFormValues) => {
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        let pledgedName = "";
        let pledgedType = "";
        switch (values.pledgedByType) {
            case "retiree": pledgedName = retireeName || ""; pledgedType = "متقاعد"; break;
            case "agent": pledgedName = lookups.agents.find((a: { id: number | string; }) => String(a.id) === values.selectedID)?.fullName || ""; pledgedType = "وكيل"; break;
            case "entitled": pledgedName = lookups.entitleds.find((e: { id: number; fullName: string }) => e.id === Number(values.selectedID))?.fullName || ""; pledgedType = "مستحق"; break;
            case "guardian": pledgedName = lookups.guardians.find((g: { id: number; fullName: string }) => g.id === Number(values.selectedID))?.fullName || ""; pledgedType = "وصي"; break;
        }

        const deadlineDate = new Date();
        deadlineDate.setMonth(deadlineDate.getMonth() + Number(values.deadlineMonths));

        const materializedIds = await resolveMissingDocumentIds(
            values.missingDocumentIDs, missingDocs, fileId
        );

        const payload: CreatePledgeDto & { pledgeFile?: File } = {
            fileID: fileId,
            missingDocumentIDs: materializedIds,
            deadline: deadlineDate.toISOString().split("T")[0],
            pledgedByName: pledgedName,
            pledgedByType: pledgedType,
            reason: values.reason || undefined,
            pledgeFile: values.pledgeFile as File || undefined,
        };
        
        // We use mutateAsync to allow the dialog to handle "Save & Add Another" logic
        await pledgeMutation.mutateAsync({ 
            values: payload, 
            id: dialogState.pledge.data?.pledgeID, 
            shouldPrint: values.issueOfficialPledge,
<<<<<<< HEAD
            keepOpen: options?.keepOpen,
=======
            keepOpen: (values as unknown as { keepOpen: boolean }).keepOpen 
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        });
    };

    const handleExemptionSubmit = async (values: ExemptionFormValues) => {
        const materializedIds = await resolveMissingDocumentIds(
            values.missingDocumentIDs, missingDocs, fileId
        );

        const payload: CreateExemptionDto & { exemptionFile?: File } = {
            fileID: fileId,
            missingDocumentIDs: materializedIds,
            directiveSource: values.directiveSource,
            exemptionDate: values.exemptionDate,
            notes: values.notes || undefined,
            exemptionFile: values.exemptionFile as File || undefined,
        };
        exemptionMutation.mutate({ values: payload, id: dialogState.exemption.data?.exemptionID });
    };

    const handleMarkFulfilled = useCallback(async (p: PledgeItem) => {
        setDialogState(s => ({ ...s, fulfill: { open: true, data: p } }));
    }, []);

    const handleDeletePledge = useCallback(async (p: PledgeItem) => {
        if (!(await confirm({ title: "حذف التعهد", description: "هل أنت متأكد من حذف هذا التعهد نهائياً؟", variant: "danger" }))) return;
        deletePledgeMutation.mutate(p.pledgeID);
    }, [confirm, deletePledgeMutation]);


    const handleDeleteExemption = useCallback(async (e: ExemptionItem) => {
        if (!(await confirm({ title: "حذف الإعفاء", description: "هل أنت متأكد من حذف هذا الإعفاء نهائياً؟", variant: "danger" }))) return;
        deleteExemptionMutation.mutate(e.exemptionID);
    }, [confirm, deleteExemptionMutation]);

    // Shared render for missing documents column
    const renderDocsColumn = useCallback((row: { missingDocuments?: MissingDocumentInfo[] }) => (
        <div className="flex flex-wrap gap-1 text-start">
            {row.missingDocuments?.map((doc, idx) => {
                const isFulfilled = doc.status === "Fulfilled";
                return (
                    <Badge 
                        key={idx} 
                        variant={isFulfilled ? "success" : "outline"} 
<<<<<<< HEAD
                        className={`text-[10px] sm:text-xs font-bold ${isFulfilled ? "opacity-100 line-through" : ""}`}
=======
                        className={`text-[10px] sm:text-xs font-bold ${isFulfilled ? "opacity-70 line-through" : ""}`}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                        title={isFulfilled ? "مستلمة" : "غير مستلمة"}
                    >
                        {doc.documentTypeName}
                    </Badge>
                );
            }) || "—"}
        </div>
    ), []);

    const pledgeColumns = useMemo<CustomColumnDef<PledgeItem>[]>(() => [
        { key: "docs", header: "الوثائق", render: renderDocsColumn },
        { key: "pledgeDate", header: "تاريخ التعهد", render: (row) => <span className="font-mono text-xs">{formatDate(row.pledgeDate)}</span> },
        { key: "deadline", header: "المهلة", render: (row) => <span className="font-mono text-xs text-amber-600 font-bold">{formatDate(row.deadline)}</span> },
        { key: "pledgedByName", header: "المتعهد", accessor: (row) => row.pledgedByName || "—" },
        {
            key: "status",
            header: "الحالة",
            render: (row) => {
                const map: Record<string, { label: string; variant: "outline" | "warning" | "success" | "destructive" }> = {
                    Active: { label: "نشط", variant: "warning" },
                    Fulfilled: { label: "مستوفى", variant: "success" },
                    Expired: { label: "منتهي", variant: "destructive" },
                };
                const info = map[row.status] || { label: row.status, variant: "outline" };
                return <Badge variant={info.variant} className="font-black">{info.label}</Badge>;
            }
        },
        {
            key: "actions",
            header: "إجراءات",
            render: (row) => (
                <div className="flex items-center justify-center gap-1">
                    <Button size="icon" variant="ghost" className="min-h-[44px] min-w-[44px] text-primary hover:bg-primary/5" onClick={() => navigate(`/pledges/${row.pledgeID}/print`)} title="طباعة تعهد">
                        <Printer className="w-4 h-4" />
                    </Button>
                    {row.status === "Active" && (canEditPledge || canManagePledge) && (
                        <Button size="icon" variant="ghost" className="min-h-[44px] min-w-[44px] text-emerald-600 hover:bg-emerald-50" onClick={() => handleMarkFulfilled(row)} title="مستوفى">
                            <CheckCircle className="w-4 h-4" />
                        </Button>
                    )}
                    {canEditPledge && (
                        <Button size="icon" variant="ghost" className="min-h-[44px] min-w-[44px] text-muted-foreground hover:bg-muted" onClick={() => setDialogState(s => ({ ...s, pledge: { open: true, mode: "edit", data: row, shouldPrint: false } }))} title="تعديل">
                            <Edit2 className="w-4 h-4" />
                        </Button>
                    )}
                    {canManagePledge && (
                        <Button size="icon" variant="ghost" className="min-h-[44px] min-w-[44px] text-destructive hover:bg-destructive/5" onClick={() => handleDeletePledge(row)} title="حذف">
                            <X className="w-4 h-4" />
                        </Button>
                    )}
                </div>
            )
        }
    ], [canEditPledge, canManagePledge, handleMarkFulfilled, handleDeletePledge, navigate, renderDocsColumn]);

    const exemptionColumns = useMemo<CustomColumnDef<ExemptionItem>[]>(() => [
        { key: "docs", header: "الوثائق", render: renderDocsColumn },
        { key: "directiveSource", header: "مصدر التوجيه", render: (row) => <span className="font-bold text-sm">{row.directiveSource}</span> },
        { key: "exemptionDate", header: "تاريخ الإعفاء", render: (row) => <span className="font-mono text-xs">{formatDate(row.exemptionDate)}</span> },
        { key: "notes", header: "ملاحظات", accessor: (row) => row.notes || "—" },
        {
            key: "actions",
            header: "إجراءات",
            render: (row) => (
                <div className="flex items-center justify-center gap-1">
                    {canEditExemption && (
                        <Button size="icon" variant="ghost" className="min-h-[44px] min-w-[44px] text-muted-foreground hover:bg-muted" onClick={() => setDialogState(s => ({ ...s, exemption: { open: true, mode: "edit", data: row } }))} title="تعديل">
                            <Edit2 className="w-4 h-4" />
                        </Button>
                    )}
                    {canManageExemption && (
                        <Button size="icon" variant="ghost" className="min-h-[44px] min-w-[44px] text-destructive hover:bg-destructive/5" onClick={() => handleDeleteExemption(row)} title="حذف">
                            <X className="w-4 h-4" />
                        </Button>
                    )}
                </div>
            )
        }
    ], [canEditExemption, canManageExemption, handleDeleteExemption, renderDocsColumn]);

    useCreateShortcut(activeTab === "pledges" ? handleOpenCreatePledge : handleOpenCreateExemption, activeTab === "pledges" ? canCreatePledge : canCreateExemption);

    if (isLoading) return <LoadingIndicator message="جاري تحميل القروض والمعفيات..." />;
    if (peError) return <div className="py-12 text-center text-destructive">تعذر تحميل بيانات التعهدات والإعفاءات</div>;

    return (
        <div className="space-y-3 md:space-y-6 animate-in fade-in duration-500 text-start" dir="rtl">
<<<<<<< HEAD
            {/* Card Header */}
            <TabHeader
                title="التعهدات والإعفاءات"
                description="متابعة وثائق التعهدات المستندية وتوجيهات الإعفاء الخاصة بالملف"
                icon={FileSignature}
                action={{
                    label: activeTab === "pledges" ? "إضافة تعهد" : "إضافة إعفاء",
                    icon: Plus,
                    onClick: () => activeTab === "pledges" ? setDialogState(s => ({ ...s, pledge: { open: true, mode: "create", data: null, shouldPrint: false } })) : setDialogState(s => ({ ...s, exemption: { open: true, mode: "create", data: null } })),
                    show: activeTab === "pledges" ? canCreatePledge : canCreateExemption
                }}
            >
                <div className="flex p-1 bg-background/80 rounded-xl border border-border w-fit ms-0 sm:ms-4">
=======
            {/* Tabs Header */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-border/60 pb-4">
                <div className="flex p-1 bg-muted/50 rounded-xl border border-border/40 w-fit ">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => setActiveTab("pledges")}
<<<<<<< HEAD
                        className={`flex items-center gap-2 px-3 py-1.5 text-xs font-bold rounded-lg ${activeTab === "pledges"
                            ? "bg-primary text-primary-foreground shadow-sm"
                            : "text-muted-foreground"
                            }`}
                    >
                        <FileSignature className="w-3.5 h-3.5" />
                        التعهدات <span className="ms-1 px-1.5 py-0.5 rounded-full bg-primary/20 text-[10px] font-mono font-bold">{pledges.length}</span>
=======
                        className={`flex items-center gap-2 px-5 py-2 text-sm font-black ${activeTab === "pledges"
                            ? "bg-background text-primary "
                            : "text-muted-foreground"
                            }`}
                    >
                        <FileSignature className="w-4 h-4" />
                        التعهدات <span className="ms-1 px-1.5 py-0.5 rounded-full bg-primary/10 text-[10px] sm:text-xs font-mono font-bold">{pledges.length}</span>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                    </Button>
                    <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => setActiveTab("exemptions")}
<<<<<<< HEAD
                        className={`flex items-center gap-2 px-3 py-1.5 text-xs font-bold rounded-lg ${activeTab === "exemptions"
                            ? "bg-primary text-primary-foreground shadow-sm"
                            : "text-muted-foreground"
                            }`}
                    >
                        <ShieldCheck className="w-3.5 h-3.5" />
                        الإعفاءات <span className="ms-1 px-1.5 py-0.5 rounded-full bg-primary/20 text-[10px] font-mono font-bold">{exemptions.length}</span>
                    </Button>
                </div>
            </TabHeader>
=======
                        className={`flex items-center gap-2 px-5 py-2 text-sm font-black ${activeTab === "exemptions"
                            ? "bg-background text-primary "
                            : "text-muted-foreground"
                            }`}
                    >
                        <ShieldCheck className="w-4 h-4" />
                        الإعفاءات <span className="ms-1 px-1.5 py-0.5 rounded-full bg-primary/10 text-[10px] sm:text-xs font-mono font-bold">{exemptions.length}</span>
                    </Button>
                </div>

                {activeTab === "pledges" && canCreatePledge && (
                    <Button size="sm" onClick={() => setDialogState(s => ({ ...s, pledge: { open: true, mode: "create", data: null, shouldPrint: false } }))} className="gap-1.5 font-bold">
                        <Plus className="w-4 h-4" /> إضافة تعهد
                    </Button>
                )}
                {activeTab === "exemptions" && canCreateExemption && (
                    <Button size="sm" onClick={() => setDialogState(s => ({ ...s, exemption: { open: true, mode: "create", data: null } }))} className="gap-1.5 font-bold">
                        <Plus className="w-4 h-4" /> إضافة إعفاء
                    </Button>
                )}
            </div>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

            {/* Content */}
            <DataTable
                columns={(activeTab === "pledges" ? pledgeColumns : exemptionColumns) as CustomColumnDef<PledgeItem | ExemptionItem>[]}
                data={(activeTab === "pledges" ? pledges : exemptions) as (PledgeItem | ExemptionItem)[]}
                loading={isLoading}
                emptyMessage={activeTab === "pledges" ? "لا توجد تعهدات مسجلة لهذا الملف" : "لا توجد إعفاءات مسجلة لهذا الملف"}
                renderMobileCard={(item) => activeTab === "pledges" ? (
                    <div className="p-3 space-y-2">
                        <div className="flex items-center justify-between">
                            {(() => {
                                const pMap: Record<string, { color: string; label: string }> = {
                                    Active: { color: "bg-amber-500", label: "نشط" },
                                    Fulfilled: { color: "bg-emerald-500", label: "مستوفى" },
                                    Expired: { color: "bg-red-500", label: "منتهي" },
                                };
                                const info = pMap[(item as PledgeItem).status] || { color: "bg-muted-foreground", label: (item as PledgeItem).status };
                                return <span className={cn("w-2 h-2 rounded-full shrink-0", info.color)} title={info.label} />;
                            })()}
                            <span className="font-mono text-[10px] text-amber-600 font-bold">{formatDate((item as PledgeItem).deadline)}</span>
                        </div>
                        <div className="flex flex-wrap gap-1">
                            {(item as PledgeItem).missingDocuments?.map((doc, idx) => {
                                const isFulfilled = doc.status === "Fulfilled";
                                return (
<<<<<<< HEAD
                                    <Badge key={idx} variant={isFulfilled ? "success" : "outline"} className={`text-[9px] h-3.5 font-bold ${isFulfilled ? "opacity-100 line-through" : ""}`}>
=======
                                    <Badge key={idx} variant={isFulfilled ? "success" : "outline"} className={`text-[9px] h-3.5 font-bold ${isFulfilled ? "opacity-70 line-through" : ""}`}>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                        {doc.documentTypeName}
                                    </Badge>
                                );
                            }) || "—"}
                        </div>
                        <div className="text-[10px] text-muted-foreground space-y-0.5">
                            <div>المتعهد: {(item as PledgeItem).pledgedByName || "—"}</div>
                            <div>التاريخ: <span className="font-mono">{formatDate((item as PledgeItem).pledgeDate)}</span></div>
                        </div>
                        <div className="flex justify-end gap-0">
                            <Button size="icon" variant="ghost" className="min-h-[32px] min-w-[32px] h-8 w-8 text-primary" onClick={() => navigate(`/pledges/${(item as PledgeItem).pledgeID}/print`)}>
                                <Printer className="w-3.5 h-3.5" />
                            </Button>
                            {(item as PledgeItem).status === "Active" && (canEditPledge || canManagePledge) && (
                                <Button size="icon" variant="ghost" className="min-h-[32px] min-w-[32px] h-8 w-8 text-emerald-600" onClick={() => handleMarkFulfilled(item as PledgeItem)}>
                                    <CheckCircle className="w-3.5 h-3.5" />
                                </Button>
                            )}
                            {canEditPledge && (
                                <Button size="icon" variant="ghost" className="min-h-[32px] min-w-[32px] h-8 w-8 text-muted-foreground" onClick={() => setDialogState(s => ({ ...s, pledge: { open: true, mode: "edit", data: item as PledgeItem, shouldPrint: false } }))}>
                                    <Edit2 className="w-3.5 h-3.5" />
                                </Button>
                            )}
                            {canManagePledge && (
                                <Button size="icon" variant="ghost" className="min-h-[32px] min-w-[32px] h-8 w-8 text-destructive" onClick={() => handleDeletePledge(item as PledgeItem)}>
                                    <X className="w-3.5 h-3.5" />
                                </Button>
                            )}
                        </div>
                    </div>
                ) : (
                    <div className="p-3 space-y-2">
                        <div className="font-bold text-xs">{(item as ExemptionItem).directiveSource}</div>
                        <div className="text-[10px] text-muted-foreground space-y-0.5">
                            <div>التاريخ: <span className="font-mono">{formatDate((item as ExemptionItem).exemptionDate)}</span></div>
                            {(item as ExemptionItem).notes && <div>ملاحظات: {(item as ExemptionItem).notes}</div>}
                        </div>
                        <div className="flex flex-wrap gap-1">
                            {(item as ExemptionItem).missingDocuments?.map((doc, idx) => {
                                const isFulfilled = doc.status === "Fulfilled";
                                return (
<<<<<<< HEAD
                                    <Badge key={idx} variant={isFulfilled ? "success" : "outline"} className={`text-[9px] h-3.5 font-bold ${isFulfilled ? "opacity-100 line-through" : ""}`}>
=======
                                    <Badge key={idx} variant={isFulfilled ? "success" : "outline"} className={`text-[9px] h-3.5 font-bold ${isFulfilled ? "opacity-70 line-through" : ""}`}>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                                        {doc.documentTypeName}
                                    </Badge>
                                );
                            }) || "—"}
                        </div>
                        <div className="flex justify-end gap-0">
                            {canEditExemption && (
                                <Button size="icon" variant="ghost" className="min-h-[32px] min-w-[32px] h-8 w-8 text-muted-foreground" onClick={() => setDialogState(s => ({ ...s, exemption: { open: true, mode: "edit", data: item as ExemptionItem } }))}>
                                    <Edit2 className="w-3.5 h-3.5" />
                                </Button>
                            )}
                            {canManageExemption && (
                                <Button size="icon" variant="ghost" className="min-h-[32px] min-w-[32px] h-8 w-8 text-destructive" onClick={() => handleDeleteExemption(item as ExemptionItem)}>
                                    <X className="w-3.5 h-3.5" />
                                </Button>
                            )}
                        </div>
                    </div>
                )}
            />

            {/* Dialogs */}
            {dialogState.pledge.open && (
                <PledgeDialog
                    open={dialogState.pledge.open}
                    onClose={() => setDialogState(s => ({ ...s, pledge: { open: false, mode: "create", data: null, shouldPrint: false } }))}
                    onSubmit={handlePledgeSubmit}
                    isLoading={pledgeMutation.isPending}
                    mode={dialogState.pledge.mode}
                    missingDocs={mapMissingDocsForDialog(missingDocs)}
                    retireeId={fileId}
                    retireeName={retireeName}
                    lookups={lookups}
                    initialData={dialogState.pledge.data ? {
                        missingDocumentIDs: dialogState.pledge.data.missingDocuments?.map(m => m.missingDocumentID) || [],
                        reason: dialogState.pledge.data.reason || "",
                        deadlineMonths: 3,
                        pledgedByType: (() => {
                            const type = dialogState.pledge.data.pledgedByType;
                            if (type === "متقاعد") return "retiree";
                            if (type === "وكيل") return "agent";
                            if (type === "مستحق") return "entitled";
                            if (type === "وصي") return "guardian";
                            return "retiree";
                        })(),
                        selectedID: (() => {
                            const name = dialogState.pledge.data.pledgedByName;
                            const type = dialogState.pledge.data.pledgedByType;
                            if (type === "متقاعد") return String(fileId);
                            if (type === "وكيل") return String(lookups.agents.find((a: { fullName: string | undefined; }) => a.fullName === name)?.id || "");
                            if (type === "مستحق") return String(lookups.entitleds.find((e: { fullName: string | undefined; }) => e.fullName === name)?.id || "");
                            if (type === "وصي") return String(lookups.guardians.find((g: { fullName: string | undefined; }) => g.fullName === name)?.id || "");
                            return "";
                        })()
                    } : undefined}
                />
            )}


            {dialogState.exemption.open && (
                <ExemptionDialog
                    open={dialogState.exemption.open}
                    onClose={() => setDialogState(s => ({ ...s, exemption: { open: false, mode: "create", data: null } }))}
                    onSubmit={handleExemptionSubmit}
                    isLoading={exemptionMutation.isPending}
                    mode={dialogState.exemption.mode}
                    missingDocs={mapMissingDocsForDialog(missingDocs)}
                    initialData={dialogState.exemption.data ? {
                        missingDocumentIDs: dialogState.exemption.data.missingDocuments?.map(m => m.missingDocumentID) || [],
                        directiveSource: dialogState.exemption.data.directiveSource,
                        exemptionDate: dialogState.exemption.data.exemptionDate.split("T")[0],
                        notes: dialogState.exemption.data.notes || "",
                    } : undefined}
                />
            )}

            {dialogState.fulfill.open && dialogState.fulfill.data && (
                <FulfillPledgeDialog
                    open={dialogState.fulfill.open}
                    onOpenChange={(o) => !o && setDialogState(s => ({ ...s, fulfill: { open: false, data: null } }))}
                    onClose={() => setDialogState(s => ({ ...s, fulfill: { open: false, data: null } }))}
                    pledge={dialogState.fulfill.data}
                    isLoading={fulfillPledgeMutation.isPending}
                    onSubmit={async (values) => { await fulfillPledgeMutation.mutateAsync(values); }}
                />
            )}

            {ConfirmDialogElement}
        </div>
    );
}
