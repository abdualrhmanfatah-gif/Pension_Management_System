import { useQuery, useQueryClient } from "@tanstack/react-query";
import { 
    CheckCircle2, Edit3, FileText, MapPin, ShieldAlert,
    Upload } from "lucide-react";
import { useState } from "react";

import api from "@/api/axios";
import { documentsApi } from "@/api/documents";
import { retirementApi } from "@/api/retirement";
import { EntitledPersonDetails } from "@/components/domain/retirement/EntitledPersonDetails";
import { Badge, BaseFormDialog, Button, Heading } from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { formatDate, getDocumentUrl } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { MissingDocumentInfo } from "@/types/retirement";

import { EntitledPersonFormDialog } from "./EntitledPersonFormDialog";
import { UploadDocumentDialog } from "./UploadDocumentDialog";

interface EntitledPerson {
    entitledID: number;
    fileID: number;
    fullName: string;
    nationalID: string;
    relationshipID?: number;
    relationshipName?: string;
    dateOfBirth?: string;
    ageYears?: number;
    phone?: string;
    socialStatusName?: string;
    socialStatusID?: number;
    healthStatusName?: string;
    healthStatusID?: number;
    isEligible: boolean;
    stopReason?: string;
    stopCode?: string;
    hasDisabilityException?: boolean;
    personalPhotoPath?: string | null;
    nationalIdImageUrl?: string | null;
    fileNumber?: string;
}

interface ViewEntitledPersonDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    person: EntitledPerson | null;
}

interface MissingDocItem {
    missingDocumentID: number;
    documentTypeID: number;
    documentTypeName: string;
    status: string;
    isAutoDetected: boolean;
    isRequired?: boolean;
}

export function ViewEntitledPersonDialog({ 
    open, 
    onOpenChange,
    person 
}: ViewEntitledPersonDialogProps) {
    const queryClient = useQueryClient();
    const canUpload = useHasPermission("File.Documents.Upload");
    const canEdit = useHasPermission("EntitledPerson.Edit");
    const canActivate = useHasPermission("EntitledPerson.Lifecycle");

    // حالة حوار الرفع
    const [uploadOpen, setUploadOpen] = useState(false);
    const [uploadTypeId, setUploadTypeId] = useState<number | undefined>(undefined);

    // حالة حوار التعديل
    const [editOpen, setEditOpen] = useState(false);

    // جلب الوثائق الخاصة بهذا المستحق
    const { data: entitledDocuments = [], isLoading: loadingDocuments } = useQuery({
        queryKey: ["documents", "file", person?.fileID, "entitled", person?.entitledID],
        queryFn: async () => {
            if (!person) return [];
            const allDocs = await documentsApi.getByFileId(person.fileID);
            return allDocs.filter(doc => doc.entitledID === person.entitledID);
        },
        enabled: open && !!person
    });

    // جلب النواقص الخاصة بهذا المستحق
    const { data: missingDocs = [], isLoading: isMissingLoading } = useQuery<MissingDocItem[]>({
        queryKey: ["entitled-missing-docs", person?.entitledID],
        queryFn: async () => {
            if (!person) return [];
            const [manual, auto] = await Promise.all([
                retirementApi.getMissingDocuments(person.fileID),
                retirementApi.getAutoDetectedMissing(person.fileID).catch(() => [])
            ]);

            const manualForEp = manual.filter((m: MissingDocumentInfo) => m.entitledID === person.entitledID).map((m: MissingDocumentInfo) => ({
                missingDocumentID: m.missingDocumentID,
                documentTypeID: m.documentTypeID,
                documentTypeName: m.documentTypeName,
                status: m.status,
                isAutoDetected: false,
                isRequired: true
            }));

            const autoForEp = auto.filter((a: MissingDocumentInfo) => a.entitledID === person.entitledID || (a.entityType === "EntitledPerson" && a.entityName === person.fullName))
                .map((a: MissingDocumentInfo) => ({
                    missingDocumentID: 0,
                    documentTypeID: a.documentTypeID,
                    documentTypeName: a.documentTypeName,
                    status: "Pending",
                    isAutoDetected: true,
                    isRequired: a.isRequired
                }));

            // دمج وتجنب التكرار
            const combined: MissingDocItem[] = [...manualForEp];
            autoForEp.forEach((a: MissingDocItem) => {
                if (!combined.some(c => c.documentTypeID === a.documentTypeID)) {
                    combined.push(a);
                }
            });

            return combined;
        },
        enabled: !!person && open
    });

    // طفرة التفعيل اليدوي
    const activateMutation = useApiMutation(
        () => api.post(`/entitled-persons/${person?.entitledID}/manual-activate`, {
            reason: "تفعيل يدوي بناءً على طلب الموظف مع الالتزام باستكمال النواقص",
            createPledgesForMissingDocs: true,
            pledgeDeadlineMonths: 3
        }),
        {
            successMessage: "تم تفعيل المستحق يدوياً وتحويل النواقص إلى تعهدات",
            invalidateKeys: [
                ["entitled-missing-docs", person?.entitledID],
                ["entitled-persons", person?.fileID],
                ["retirement-file"]
            ]
        }
    );

    const updatePhotoMutation = useApiMutation(
        (formData: FormData) => api.put(`/entitled-persons/${person?.entitledID}`, formData, {
            headers: { "Content-Type": undefined }
        }),
        {
            successMessage: "تم تحديث الصورة الشخصية للوريث بنجاح",
            invalidateKeys: [["entitled-persons", person?.fileID], ["retirement-file"]],
        }
    );


    // جلب أنواع الوثائق لاستخدامها في حوار الرفع
    const { data: docTypes = [] } = useQuery({
        queryKey: ["document-types"],
        queryFn: () => retirementApi.getDocumentTypes(),
        enabled: open
    });

    if (!person) return null;

    return (
        <>
            <BaseFormDialog 
                open={open} 
                onOpenChange={onOpenChange}
                title="بطاقة بيانات المستحق"
                description="عرض وتدقيق بيانات الوريث في الملف التقاعدي"
                size="full"
                footer={
                    <div className="flex flex-col sm:flex-row w-full items-stretch sm:items-center justify-between gap-3">
                        <div className="flex flex-col sm:flex-row gap-2 w-full sm:w-auto">
                            <Button 
                                variant="outline" 
                                className="rounded-xl font-bold h-11 px-6 border-muted-foreground/20 hover:bg-muted/50 transition-all w-full sm:w-auto"
                                onClick={() => onOpenChange(false)}
                            >
                                إغلاق النافذة
                            </Button>
                            {canEdit && (
                                <Button 
                                    variant="secondary" 
                                    className="gap-2 rounded-xl font-bold h-11 px-6 border border-primary/10 transition-all w-full sm:w-auto" 
                                    onClick={() => setEditOpen(true)}
                                >
                                    <Edit3 className="w-4 h-4 shrink-0" /> استكمال وتصحيح البيانات
                                </Button>
                            )}
                        </div>
                        
                        {!person.isEligible && canActivate && (
                            <Button 
                                variant="success" 
                                className="gap-2 rounded-xl font-black h-11 px-8 transition-all w-full sm:w-auto" 
                                onClick={() => activateMutation.mutate(undefined)}
                                loading={activateMutation.isPending}
                            >
                                <ShieldAlert className="w-4 h-4 shrink-0" /> تفعيل استثنائي (تعهد)
                            </Button>
                        )}
                    </div>
                }
            >
                <div className="space-y-6">
                    <div className="grid grid-cols-1 lg:grid-cols-4 gap-4 lg:gap-6 text-start">
                        {/* Main Data Column */}
                        <div className="lg:col-span-3 space-y-4 lg:space-y-6">
                            <EntitledPersonDetails 
                                person={person} 
                                onPhotoUpload={async (file) => {
                                    const formData = new FormData();
                                    formData.append("fullName", person.fullName);
                                    formData.append("nationalID", person.nationalID);
                                    formData.append("relationshipID", (person.relationshipID || 0).toString());
                                    formData.append("personalPhoto", file);
                                    await updatePhotoMutation.mutateAsync(formData);
                                }}
                            />

                            {/* Documents Grid Section */}
                            <div className="bg-card border rounded-2xl lg:rounded-[2rem] p-4 lg:p-6 space-y-4 lg:space-y-5">
                                <Heading level="h3" className="text-base lg:text-xl font-black flex items-center gap-2 lg:gap-3 text-primary border-b border-primary/5 pb-3">
                                    <FileText className="w-5 h-5 lg:w-6 lg:h-6 shrink-0" /> 
                                    الوثائق المرفقة 
                                    <Badge variant="secondary" className="ms-auto rounded-full px-3 lg:px-4 text-xs">{entitledDocuments.length}</Badge>
                                </Heading>
                                
                                {loadingDocuments ? (
                                    <div className="text-center py-12 flex flex-col items-center gap-3">
                                        <div className="w-10 h-10 border-4 border-primary/10 border-t-primary rounded-full animate-spin" />
                                        <p className="text-sm font-bold text-muted-foreground">جاري تحميل الأرشيف...</p>
                                    </div>
                                ) : entitledDocuments.length === 0 ? (
                                    <div className="text-center py-12 bg-muted/20 rounded-2xl border-2 border-dashed">
                                        <FileText className="w-12 h-12 text-muted-foreground/30 mx-auto mb-3" />
                                        <p className="text-sm text-muted-foreground font-bold italic">لا توجد وثائق مؤرشفة لهذا المستحق</p>
                                    </div>
                                ) : (
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                                        {entitledDocuments.map(doc => {
                                            const docUrl = getDocumentUrl(doc.filePath);
                                            const isPdf = doc.fileName?.toLowerCase().endsWith('.pdf');
                                            return (
                                                <div key={doc.documentID} className="border-2 border-transparent bg-muted/30 hover:bg-white hover:border-primary/20 rounded-3xl p-5 transition-all group overflow-hidden relative">
                                                    <div className="flex items-center justify-between mb-4">
                                                        <div className="flex items-center gap-3">
                                                            <div className="p-2 bg-primary/10 rounded-xl text-primary">
                                                                <FileText className="w-5 h-5" />
                                                            </div>
                                                            <h4 className="font-black text-sm text-foreground/90 truncate max-w-[160px] sm:max-w-[250px]">{doc.documentTypeName}</h4>
                                                        </div>
                                                        <p className="text-[10px] sm:text-xs font-black text-muted-foreground bg-white px-2 py-1 rounded-full">
                                                            {formatDate(doc.createdAt)}
                                                        </p>
                                                    </div>
                                                    
                                                    {docUrl ? (
                                                        <div className="space-y-3">
                                                            {isPdf ? (
                                                                <div className="w-full h-52 border-2 border-white rounded-2xl overflow-hidden bg-white transition-shadow">
                                                                    <iframe src={docUrl} className="w-full h-full border-0" title={doc.fileName} />
                                                                </div>
                                                            ) : (
                                                                <div className="relative overflow-hidden rounded-2xl border-2 border-white transition-all">
                                                                    <img 
                                                                        src={docUrl} 
                                                                        alt={doc.documentTypeName} 
                                                                        className="w-full h-52 object-cover cursor-pointer hover:scale-105 transition-transform duration-500" 
                                                                        loading="lazy"
                                                                        onClick={() => window.open(docUrl, '_blank')} 
                                                                    />
                                                                    <div className="absolute inset-0 bg-black/20 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center pointer-events-none">
                                                                        <p className="text-white text-xs font-black bg-black/40 px-4 py-2 rounded-full backdrop-blur-md">عرض بالحجم الكامل</p>
                                                                    </div>
                                                                </div>
                                                            )}
                                                            <p className="text-[10px] sm:text-xs text-muted-foreground font-medium truncate px-1" title={doc.fileName}>{doc.fileName}</p>
                                                        </div>
                                                    ) : <p className="text-sm text-muted-foreground text-center py-4 italic">رابط الوثيقة تالف أو غير متوفر</p>}
                                                </div>
                                            );
                                        })}
                                    </div>
                                )}
                            </div>
                        </div>

                        {/* Sidebar: Missing Documents */}
                        <div className="lg:col-span-1 h-full">
                            <div className="bg-card border-2 border-muted/30 rounded-2xl lg:rounded-[2.5rem] flex flex-col h-full overflow-hidden lg:sticky lg:top-4">
                                <div className="p-4 lg:p-6 border-b border-muted/30 space-y-1.5 bg-primary/[0.02]">
                                    <div className="flex items-center justify-between">
                                        <h4 className="text-xs lg:text-sm font-black text-foreground flex items-center gap-2">
                                            <Upload className="w-4 h-4 text-primary shrink-0" /> قائمة النواقص
                                        </h4>
                                        {missingDocs.length > 0 && (
                                            <Badge variant="destructive" className="h-6 px-2.5 rounded-full text-[10px] sm:text-xs font-black animate-pulse">
                                                {missingDocs.length} ناقصة
                                            </Badge>
                                        )}
                                    </div>
                                    <p className="text-[10px] sm:text-xs text-muted-foreground font-bold tracking-tight opacity-70">التدقيق والمطابقة القانونية للمستندات</p>
                                </div>
                                
                                <div className="p-3 lg:p-4 flex-1 space-y-2 lg:space-y-3 overflow-y-auto max-h-[40vh] lg:max-h-[600px] custom-scrollbar">
                                    {isMissingLoading ? (
                                        <div className="text-center py-12"><div className="w-8 h-8 border-4 border-primary/10 border-t-primary rounded-full animate-spin mx-auto mb-3" /><p className="text-xs font-black text-muted-foreground">جاري جرد الملف...</p></div>
                                    ) : missingDocs.length === 0 ? (
                                        <div className="text-center py-16 opacity-80 group">
                                            <div className="w-16 h-16 bg-emerald-50 text-emerald-500 rounded-[2rem] flex items-center justify-center border-2 border-emerald-100 mx-auto mb-4 group-hover:scale-110 transition-transform duration-500"><CheckCircle2 className="w-8 h-8" /></div>
                                            <p className="text-sm font-black text-emerald-800">الملف مكتمل قانونياً</p>
                                            <p className="text-[10px] sm:text-xs text-emerald-600/60 mt-1 font-bold">تم استيفاء جميع المتطلبات</p>
                                        </div>
                                    ) : (
                                        missingDocs.map((md, idx) => (
                                            <div key={idx} className="group relative bg-muted/20 border-2 border-transparent hover:border-primary/20 hover:bg-white p-3 lg:p-4 rounded-2xl lg:rounded-3xl transition-all">
                                                <div className="flex justify-between items-center gap-3">
                                                    <div className="space-y-1.5 min-w-0">
                                                        <p className="text-xs font-black text-foreground/90 truncate" title={md.documentTypeName}>{md.documentTypeName}</p>
                                                        <div className="flex items-center gap-2">
                                                            <span className={`text-[9px] font-black px-2 py-0.5 rounded-full ${md.isRequired !== false ? 'bg-destructive/10 text-destructive' : 'bg-muted text-muted-foreground'}`}>
                                                                {md.isRequired !== false ? 'مطلب إلزامي' : 'مطلب تكميلي'}
                                                            </span>
                                                        </div>
                                                    </div>
                                                    {canUpload && (
                                                        <Button 
                                                            size="icon" 
                                                            variant="secondary" 
                                                            aria-label="رفع وثيقة"
                                                            className="h-9 w-9 rounded-2xl shrink-0 text-primary bg-white border border-primary/10 hover:bg-primary hover:text-white transition-all scale-90 group-hover:scale-100" 
                                                            onClick={() => { setUploadTypeId(md.documentTypeID); setUploadOpen(true); }}
                                                        >
                                                            <Upload className="w-4 h-4" />
                                                        </Button>
                                                    )}
                                                </div>
                                            </div>
                                        ))
                                    )}
                                </div>

                                <div className="p-3 lg:p-5 bg-primary/[0.03] border-t border-muted/50 mt-auto">
                                    <div className="flex items-start gap-3">
                                        <MapPin className="w-4 h-4 text-primary/40 shrink-0 mt-0.5" />
                                        <p className="text-[10px] sm:text-xs text-muted-foreground leading-relaxed font-bold tracking-tight">
                                            ترفع الوثائق إلى الأرشيف المركزي وتخضع للمراجعة والتدقيق قبل الاعتماد النهائي لحالة الصرف.
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </BaseFormDialog>

            {/* حوار رفع الوثائق */}
            <UploadDocumentDialog
                open={uploadOpen}
                onOpenChange={setUploadOpen}
                onSaved={() => {
                    queryClient.invalidateQueries({ queryKey: ["entitled-missing-docs", person.entitledID] });
                    queryClient.invalidateQueries({ queryKey: ["documents", person.fileID] });
                    queryClient.invalidateQueries({ queryKey: ["entitled-persons", person.fileID] });
                    queryClient.invalidateQueries({ queryKey: ["retirement-file", person.fileID] });
                }}
                fileId={person.fileID}
                docTypes={docTypes}
                defaultTypeId={uploadTypeId}
                missingEntityIds={{ entitledID: person.entitledID }}
            />

            {/* حوار استكمال البيانات */}
            {editOpen && (
                <EntitledPersonFormDialog
                    open={editOpen}
                    onOpenChange={setEditOpen}
                    mode="edit"
                    fileId={person.fileID}
                    initialData={person}
                    onSuccess={() => {
                        queryClient.invalidateQueries({ queryKey: ["entitled-persons", person.fileID] });
                        queryClient.invalidateQueries({ queryKey: ["entitled-missing-docs", person.entitledID] });
                    }}
                />
            )}
        </>
    );
}
