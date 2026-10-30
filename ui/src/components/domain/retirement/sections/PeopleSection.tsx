import { useQueryClient } from "@tanstack/react-query";
import { Briefcase, CheckCircle2, ChevronDown, Clock, FileWarning, Pencil, Plus, Shield, Skull,UserPlus, Users, UserX } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef,useState } from "react";

import { documentsApi } from "@/api/documents";
import { retirementApi } from "@/api/retirement";
import { Button } from "@/components/ui";
import { Badge } from "@/components/ui";
import { Heading } from "@/components/ui";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui";
import { EmptyState } from "@/components/ui";
import { EditableImage } from "@/components/ui/EditableImage";
import { PERSON_DOC_FILTER, PERSON_DOC_FILTER_AR } from "@/constants";
import { useAgentQuery } from "@/hooks/queries/useAgentQuery";
import { useAgentsQuery } from "@/hooks/queries/useAgentsQuery";
import { useEntitledPersonsQuery } from "@/hooks/queries/useEntitledPersonsQuery";
import { useGuardianQuery } from "@/hooks/queries/useGuardianQuery";
import { useGuardiansQuery } from "@/hooks/queries/useGuardiansQuery";
import { usePersonDocumentsQuery } from "@/hooks/queries/usePersonDocumentsQuery";
import { useApiMutation } from "@/hooks/useApiMutation";
import { chartColors } from "@/lib/chartColors";
import { cn, formatDate, getPhotoUrl, resolvePersonalPhotoPath } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { AgentDto, AgentSummary, DocumentItem,EntitledPersonSummary, GuardianDto, GuardianSummary, PersonDocFilter, PersonSummary, PersonType } from "@/types/retirement";

import { AgentDialog } from "../AgentDialog";
import { DocumentViewerDialog } from "../DocumentViewerDialog";
import { EntitledPersonFormDialog } from "../EntitledPersonFormDialog";
import { GuardianDialog } from "../GuardianDialog";
import { PersonDocCard } from "../PersonDocCard";
import { RecordEntitledPersonDeathDialog } from "../RecordEntitledPersonDeathDialog";
import { UploadDocumentDialog } from "../UploadDocumentDialog";

interface PeopleSectionProps {
    fileId: number;
    fileNumber?: string;
}

export function PeopleSection({ fileId, fileNumber }: PeopleSectionProps) {
    const [selectedPersonId, setSelectedPersonId] = useState<number | null>(null);
    const [selectedPersonType, setSelectedPersonType] = useState<PersonType | null>(null);
    const [activeTab, setActiveTab] = useState("all");
    const [docFilter, setDocFilter] = useState<PersonDocFilter>(PERSON_DOC_FILTER.ALL);
    const [viewingDoc, setViewingDoc] = useState<DocumentItem | null>(null);
    const [viewerOpen, setViewerOpen] = useState(false);
    const [uploadOpen, setUploadOpen] = useState(false);
    const [uploadDocTypeId, setUploadDocTypeId] = useState<number | undefined>(undefined);
    const [editDialogOpen, setEditDialogOpen] = useState(false);
    const [editingPersonId, setEditingPersonId] = useState<number | null>(null);
    const [editingPersonType, setEditingPersonType] = useState<PersonType | null>(null);
    const [agentDialogOpen, setAgentDialogOpen] = useState(false);
    const [editingAgentData, setEditingAgentData] = useState<AgentDto | null>(null);
    const [guardianDialogOpen, setGuardianDialogOpen] = useState(false);
    const [editingGuardianData, setEditingGuardianData] = useState<GuardianDto | null>(null);
    const [addMenuOpen, setAddMenuOpen] = useState(false);
    const [createMode, setCreateMode] = useState<PersonType | null>(null);
    const [deathDialogOpen, setDeathDialogOpen] = useState(false);
    const addMenuRef = useRef<HTMLDivElement>(null);
    const queryClient = useQueryClient();

    // إغلاق القائمة عند النقر خارجها
    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (addMenuRef.current && !addMenuRef.current.contains(event.target as Node)) {
                setAddMenuOpen(false);
            }
        };
        if (addMenuOpen) {
            document.addEventListener("mousedown", handleClickOutside);
        }
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, [addMenuOpen]);

    const { data: entitledPersons = [] } = useEntitledPersonsQuery(fileId);
    const { data: agents = [] } = useAgentsQuery(fileId);
    const { data: guardians = [] } = useGuardiansQuery(fileId);

    const { data: personDocs } = usePersonDocumentsQuery(fileId, selectedPersonId, selectedPersonType);

    // جلب بيانات الوكيل عند التعديل
    const editAgentId = editingPersonType === "agent" ? editingPersonId : undefined;
    const { data: agentData } = useAgentQuery(editAgentId ?? undefined, !!editAgentId && !agentDialogOpen);

    // جلب بيانات الوصي عند التعديل
    const editGuardianId = editingPersonType === "guardian" ? editingPersonId : undefined;
    const { data: guardianData } = useGuardianQuery(editGuardianId ?? undefined, !!editGuardianId && !guardianDialogOpen);

    const allPersons: PersonSummary[] = useMemo(() => {
        const entitled: PersonSummary[] = entitledPersons.map((p: EntitledPersonSummary) => ({
            id: p.entitledID,
            type: "entitled" as PersonType,
            name: p.fullName,
            nationalID: p.nationalID,
            relationship: p.relationshipName,
            age: p.ageYears,
            dateOfBirth: p.dateOfBirth,
            socialStatus: p.socialStatusName,
            phone: p.phone,
            status: p.isDeceased ? "متوفى" : p.isEligible ? "مستحق" : "موقوف",
            isActive: p.isActive !== false && !p.isDeleted,
            pendingReviewStatus: p.pendingReviewStatus,
            photoPath: p.personalPhotoPath,
            isEligible: p.isEligible,
            isDeceased: p.isDeceased,
        }));

        const agentList: PersonSummary[] = agents.map((a: AgentSummary) => ({
            id: a.agentID,
            type: "agent" as PersonType,
            name: a.fullName,
            nationalID: a.nationalID,
            relationship: "وكيل",
            phone: a.phone,
            status: a.isActive ? "نشط" : "معطل",
            isActive: a.isActive !== false && !a.isDeleted,
            pendingReviewStatus: a.pendingReviewStatus,
            photoPath: a.personalPhotoPath,
            agencyNumber: a.agencyNumber,
            agencyDate: a.agencyDate,
        }));

        const guardianList: PersonSummary[] = guardians.map((g: GuardianSummary) => ({
            id: g.guardianID,
            type: "guardian" as PersonType,
            name: g.fullName,
            nationalID: g.nationalID,
            relationship: "وصي",
            phone: g.phone,
            status: g.isActive ? "نشط" : "منتهي",
            isActive: g.isActive !== false && !g.isDeleted,
            pendingReviewStatus: g.pendingReviewStatus,
            photoPath: g.personalPhotoPath,
            guardianshipDocNumber: g.guardianshipDocNumber,
        }));

        return [...entitled, ...agentList, ...guardianList];
    }, [entitledPersons, agents, guardians]);

    const filteredPersons = useMemo(() => {
        if (activeTab === "all") return allPersons;
        if (activeTab === "entitled") return allPersons.filter((p) => p.type === "entitled");
        if (activeTab === "agents") return allPersons.filter((p) => p.type === "agent");
        if (activeTab === "guardians") return allPersons.filter((p) => p.type === "guardian");
        return allPersons;
    }, [allPersons, activeTab]);

    const selectedPerson = selectedPersonId ? allPersons.find((p) => p.id === selectedPersonId && p.type === selectedPersonType) : null;

    const filteredDocs = useMemo(() => {
        if (!personDocs) return { uploaded: [], missing: [], pledges: [] };
        if (docFilter === PERSON_DOC_FILTER.UPLOADED) return { ...personDocs, missing: [], pledges: [] };
        if (docFilter === PERSON_DOC_FILTER.MISSING) return { uploaded: [], missing: personDocs.missing, pledges: personDocs.pledges };
        return personDocs;
    }, [personDocs, docFilter]);

    const handleSelectPerson = (person: PersonSummary) => {
        setSelectedPersonId(person.id);
        setSelectedPersonType(person.type);
        setDocFilter(PERSON_DOC_FILTER.ALL);
    };

    const canCreate = useHasPermission("EntitledPerson.Create") || useHasPermission("Agent.Add") || useHasPermission("Guardian.Add");
    const canEditPhoto = useHasPermission("EntitledPerson.Edit") || useHasPermission("Agent.Edit") || useHasPermission("Guardian.Edit");
    const canEditEntitled = useHasPermission("EntitledPerson.Edit");
    const canEditAgent = useHasPermission("Agent.Edit");
    const canEditGuardian = useHasPermission("Guardian.Edit");

    const handleEditPerson = useCallback((person: PersonSummary) => {
        if (person.type === "entitled") {
            setEditingPersonId(person.id);
            setEditingPersonType("entitled");
            setEditDialogOpen(true);
        } else if (person.type === "agent") {
            // تأخير فتح النافذة حتى تصل البيانات
            setEditingPersonId(person.id);
            setEditingPersonType("agent");
        } else if (person.type === "guardian") {
            // تأخير فتح النافذة حتى تصل البيانات
            setEditingPersonId(person.id);
            setEditingPersonType("guardian");
        }
    }, []);

    // فتح نافذة الوكيل بعد وصول البيانات
    useEffect(() => {
        if (editingPersonType === "agent" && editingPersonId && agentData && !agentDialogOpen) {
            setEditingAgentData(agentData);
            setAgentDialogOpen(true);
        }
    }, [editingPersonType, editingPersonId, agentData, agentDialogOpen]);

    // فتح نافذة الوصي بعد وصول البيانات
    useEffect(() => {
        if (editingPersonType === "guardian" && editingPersonId && guardianData && !guardianDialogOpen) {
            setEditingGuardianData(guardianData);
            setGuardianDialogOpen(true);
        }
    }, [editingPersonType, editingPersonId, guardianData, guardianDialogOpen]);

    const updatePhotoMutation = useApiMutation(
        async ({ id, type, file }: { id: number; type: PersonType; file: File }) => {
            const formData = new FormData();
            formData.append("personalPhoto", file);
            
            if (type === "entitled") {
                return retirementApi.updateEntitledPerson(id, formData);
            } else if (type === "agent") {
                return retirementApi.updateAgent(id, formData);
            } else if (type === "guardian") {
                return retirementApi.updateGuardian(id, formData);
            }
        },
        {
            successMessage: "تم تحديث الصورة الشخصية بنجاح",
            invalidateKeys: [["entitled-persons", fileId], ["agents", fileId], ["guardians", fileId], ["retirement-file", fileId]],
        }
    );

    const handlePhotoUpload = useCallback(async (file: File) => {
        if (!selectedPerson) return "";
        await updatePhotoMutation.mutateAsync({ id: selectedPerson.id, type: selectedPerson.type, file });
        return "";
    }, [selectedPerson, updatePhotoMutation]);

    const canUploadDoc = useHasPermission("File.Documents.Upload");
    const canDeleteDoc = useHasPermission("File.Delete.Soft");
    const canCreateEntitled = useHasPermission("EntitledPerson.Create");
    const canCreateAgent = useHasPermission("Agent.Add");
    const canCreateGuardian = useHasPermission("Guardian.Add");

    const deleteMutation = useApiMutation(
        async (docId: number) => documentsApi.delete(docId),
        {
            successMessage: "تم حذف الوثيقة بنجاح",
            invalidateKeys: [["person-documents", fileId], ["retirement-file", fileId]],
        }
    );

    const handleViewDocument = useCallback((doc: DocumentItem) => {
        setViewingDoc(doc);
        setViewerOpen(true);
    }, []);

    const handleDeleteDocument = useCallback(async (doc: DocumentItem) => {
        await deleteMutation.mutateAsync(doc.documentID);
    }, [deleteMutation]);

    const handleUploadDocument = useCallback((docTypeId?: number) => {
        setUploadDocTypeId(docTypeId);
        setUploadOpen(true);
    }, []);

    const getEntityIds = useCallback(() => {
        if (!selectedPerson) return undefined;
        if (selectedPerson.type === "entitled") return { entitledID: selectedPerson.id };
        if (selectedPerson.type === "agent") return { agentID: selectedPerson.id };
        if (selectedPerson.type === "guardian") return { guardianID: selectedPerson.id };
        return undefined;
    }, [selectedPerson]);

    return (
        <div className="space-y-4 fade-in">
            <div className="flex items-center justify-between">
                <Heading level="h2">الأشخاص المرتبطون بالملف</Heading>
                {canCreate && (
                    <div className="relative" ref={addMenuRef}>
                        <Button
                            size="sm"
                            onClick={() => setAddMenuOpen((p) => !p)}
                        >
                            <Plus className="w-4 h-4 ml-1" />
                            إضافة شخص
                            <ChevronDown className={cn("w-3 h-3 mr-1 transition-transform", addMenuOpen && "rotate-180")} />
                        </Button>
                        {addMenuOpen && (
                            <div className="absolute left-0 top-full mt-1 w-56 bg-card border border-border rounded-xl shadow-lg z-50 py-1 animate-in fade-in slide-in-from-top-2">
                                {canCreateEntitled && (
                                    <Button
                                        variant="ghost"
                                        className="w-full justify-start gap-3 px-4 py-3 h-auto text-right"
                                        onClick={() => {
                                            setCreateMode("entitled");
                                            setEditDialogOpen(true);
                                            setAddMenuOpen(false);
                                        }}
                                    >
                                        <UserPlus className="w-4 h-4 text-blue-500" />
                                        <div>
                                            <p className="text-sm font-medium">إضافة مستحق (وريث)</p>
                                            <p className="text-[10px] text-muted-foreground">إضافة شخص جديد كمستحق في الملف</p>
                                        </div>
                                    </Button>
                                )}
                                {canCreateAgent && (
                                    <Button
                                        variant="ghost"
                                        className="w-full justify-start gap-3 px-4 py-3 h-auto text-right"
                                        onClick={() => {
                                            setCreateMode("agent");
                                            setAgentDialogOpen(true);
                                            setAddMenuOpen(false);
                                        }}
                                    >
                                        <Briefcase className="w-4 h-4 text-amber-500" />
                                        <div>
                                            <p className="text-sm font-medium">إضافة وكيل</p>
                                            <p className="text-[10px] text-muted-foreground">تعيين وكيل لاستلام المستحقات</p>
                                        </div>
                                    </Button>
                                )}
                                {canCreateGuardian && (
                                    <Button
                                        variant="ghost"
                                        className="w-full justify-start gap-3 px-4 py-3 h-auto text-right"
                                        onClick={() => {
                                            setCreateMode("guardian");
                                            setGuardianDialogOpen(true);
                                            setAddMenuOpen(false);
                                        }}
                                    >
                                        <Shield className="w-4 h-4 text-cyan-500" />
                                        <div>
                                            <p className="text-sm font-medium">إضافة وصي</p>
                                            <p className="text-[10px] text-muted-foreground">تعيين وصي على قاصرين</p>
                                        </div>
                                    </Button>
                                )}
                            </div>
                        )}
                    </div>
                )}
            </div>

            <Tabs value={activeTab} onValueChange={setActiveTab}>
                <TabsList>
                    <TabsTrigger value="all">الكل ({allPersons.length})</TabsTrigger>
                    <TabsTrigger value="entitled">المستحقون ({entitledPersons.length})</TabsTrigger>
                    <TabsTrigger value="agents">الوكلاء ({agents.length})</TabsTrigger>
                    <TabsTrigger value="guardians">الأوصياء ({guardians.length})</TabsTrigger>
                </TabsList>
            </Tabs>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 min-h-[500px]">
                {/* القائمة الجانبية: الأشخاص */}
                <div className="space-y-2 overflow-y-auto pr-2 border-l border-border">
                    {filteredPersons.length === 0 ? (
                        <EmptyState
                            icon={UserX}
                            title="لا يوجد أشخاص"
                            description="لم يتم تسجيل أي أشخاص في هذا الملف بعد"
                        />
                    ) : (
                        filteredPersons.map((person) => {
                            const isSelected = selectedPersonId === person.id && selectedPersonType === person.type;
                            return (
                            <div
                                key={`${person.type}-${person.id}`}
                                className={cn(
                                    "rounded-lg border bg-card transition-all",
                                    isSelected
                                        ? "border-primary ring-2 ring-primary/10 bg-primary/5"
                                        : "border-border hover:border-muted-foreground"
                                )}
                            >
                                <div
                                    onClick={() => handleSelectPerson(person)}
                                    className="flex items-center gap-3 p-3 cursor-pointer"
                                >
                                    <div
                                        className="w-10 h-10 rounded-full flex items-center justify-center text-white text-sm font-bold shrink-0 overflow-hidden"
                                        style={{
                                            background: person.photoPath ? "transparent" : 
                                                        person.type === "entitled" ? chartColors.primary : 
                                                        person.type === "agent" ? chartColors.warning : chartColors.teal,
                                        }}
                                    >
                                        {person.photoPath ? (
                                            <img 
                                                src={getPhotoUrl(resolvePersonalPhotoPath(person)) ?? undefined} 
                                                alt="" 
                                                className="w-full h-full object-cover"
                                                loading="lazy"
                                            />
                                        ) : (
                                            person.name?.charAt(0) || "؟"
                                        )}
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <div className="flex items-center gap-2">
                                            <p className="text-sm font-bold truncate">{person.name}</p>
                                            <Badge
                                                variant={
                                                    person.status === "مستحق" || person.status === "نشط"
                                                        ? "success"
                                                        : person.status === "موقوف" || person.status === "معطل"
                                                        ? "danger"
                                                        : "secondary"
                                                }
                                                className="text-[10px]"
                                            >
                                                {person.status}
                                            </Badge>
                                            {person.pendingReviewStatus === 1 && (
                                                <Badge variant="warning" className="text-[10px]">قيد المراجعة</Badge>
                                            )}
                                        </div>
                                        <p className="text-xs text-muted-foreground">
                                            {person.relationship}
                                            {person.age ? ` — ${person.age} سنة` : ""}
                                        </p>
                                    </div>
                                    {person.nationalID && (
                                        <div className="text-left shrink-0">
                                            <p className="text-[10px] text-muted-foreground">الرقم الوطني</p>
                                            <p className="text-xs font-medium" dir="ltr">{person.nationalID}</p>
                                        </div>
                                    )}
                                    {((person.type === "entitled" && canEditEntitled) || (person.type === "agent" && canEditAgent) || (person.type === "guardian" && canEditGuardian)) && (
                                        <Button
                                            variant="ghost"
                                            size="sm"
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                handleEditPerson(person);
                                            }}
                                            className="shrink-0 h-8 w-8 p-0 rounded-lg hover:bg-blue-50 hover:text-blue-600 dark:hover:bg-blue-950/30 transition-all duration-200"
                                            title="تعديل"
                                        >
                                            <Pencil className="w-4 h-4" />
                                        </Button>
                                    )}
                                </div>

                                {/* المعلومات الأساسية - تظهر عند التحديد */}
                                {isSelected && (
                                    <div className="px-3 pb-3 pt-0 border-t border-border mt-0">
                                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs pt-2">
                                            <div className="flex flex-col">
                                                <span className="text-muted-foreground">العمر</span>
                                                <span className="font-medium">{person.age ? `${person.age} سنة` : "—"}</span>
                                            </div>
                                            <div className="flex flex-col">
                                                <span className="text-muted-foreground">تاريخ الميلاد</span>
                                                <span className="font-medium">{formatDate(person.dateOfBirth) || "—"}</span>
                                            </div>
                                            <div className="flex flex-col">
                                                <span className="text-muted-foreground">الحالة الاجتماعية</span>
                                                <span className="font-medium">{person.socialStatus || "—"}</span>
                                            </div>
                                            {person.phone && (
                                                <div className="flex flex-col">
                                                    <span className="text-muted-foreground">رقم الهاتف</span>
                                                    <span className="font-medium" dir="ltr">{person.phone}</span>
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                )}
                            </div>
                            );
                        })
                    )}
                </div>

                {/* المحتوى: تفاصيل الشخص + وثائقه */}
                <div className="overflow-y-auto pl-2 border-r border-border">
                    {!selectedPerson ? (
                        <div className="flex flex-col items-center justify-center h-full text-muted-foreground">
                            <Users className="w-12 h-12 mb-3 opacity-100" />
                            <p className="font-bold text-sm">اختر شخصاً من القائمة</p>
                            <p className="text-xs">سيتم عرض بياناته ووثائقه المرتبطة هنا</p>
                        </div>
                    ) : (
                        <div className="space-y-4">
                            {/* بطاقة الملف الشخصي */}
                            <div className="bg-gradient-to-br from-primary/5 to-primary/10 rounded-xl p-4 border border-primary">
                                <div className="flex items-start gap-4">
                                    <EditableImage
                                        src={getPhotoUrl(resolvePersonalPhotoPath(selectedPerson))}
                                        alt={selectedPerson.name}
                                        fallbackIcon={
                                            <span className="text-white text-xl font-bold">
                                                {selectedPerson.name?.charAt(0) || "؟"}
                                            </span>
                                        }
                                        className="w-16 h-16 rounded-full border-4 border-white shadow-lg"
                                        onUpload={canEditPhoto ? handlePhotoUpload : undefined}
                                        canEdit={canEditPhoto}
                                        size="lg"
                                    />
                                    <div className="flex-1 min-w-0">
                                        <div className="flex items-center gap-2 flex-wrap">
                                            <Heading level="h3" className="font-bold text-lg">{selectedPerson.name}</Heading>
                                            <Badge
                                                variant={
                                                    selectedPerson.status === "مستحق" || selectedPerson.status === "نشط"
                                                        ? "success"
                                                        : selectedPerson.status === "موقوف" || selectedPerson.status === "معطل" || selectedPerson.status === "متوفى"
                                                        ? "danger"
                                                        : "secondary"
                                                }
                                                className="text-[10px]"
                                            >
                                                {selectedPerson.status}
                                            </Badge>
                                            {selectedPerson.pendingReviewStatus === 1 && (
                                                <Badge variant="warning" className="text-[10px]">قيد المراجعة</Badge>
                                            )}
                                        </div>
                                        <p className="text-sm text-muted-foreground">{selectedPerson.relationship}</p>
                                    </div>
                                </div>
                            </div>

                            {/* زر تسجيل الوفاة للمستحق النشط */}
                            {selectedPerson.type === "entitled" && selectedPerson.isActive && !selectedPerson.isDeceased && (
                                <Button
                                    variant="destructive"
                                    size="sm"
                                    className="w-full gap-2"
                                    onClick={() => setDeathDialogOpen(true)}
                                >
                                    <Skull className="w-4 h-4" />
                                    تسجيل وفاة المستحق
                                </Button>
                            )}

                            {/* معلومات إضافية حسب النوع */}
                            {selectedPerson.type === "agent" && (selectedPerson.agencyNumber || selectedPerson.agencyDate) && (
                                <div className="bg-card rounded-xl border border-border p-4">
                                    <h4 className="font-bold text-sm mb-3 flex items-center gap-2">
                                        <span className="w-1 h-4 bg-amber-500 rounded-full"></span>
                                        معلومات الوكالة
                                    </h4>
                                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 text-sm">
                                        {selectedPerson.agencyNumber && (
                                            <div className="flex flex-col">
                                                <span className="text-muted-foreground text-xs">رقم الوكالة</span>
                                                <span className="font-medium">{selectedPerson.agencyNumber}</span>
                                            </div>
                                        )}
                                        {selectedPerson.agencyDate && (
                                            <div className="flex flex-col">
                                                <span className="text-muted-foreground text-xs">تاريخ الوكالة</span>
                                                <span className="font-medium">{selectedPerson.agencyDate}</span>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            )}

                            {selectedPerson.type === "guardian" && selectedPerson.guardianshipDocNumber && (
                                <div className="bg-card rounded-xl border border-border p-4">
                                    <h4 className="font-bold text-sm mb-3 flex items-center gap-2">
                                        <span className="w-1 h-4 bg-cyan-500 rounded-full"></span>
                                        معلومات الوصاية
                                    </h4>
                                    <div className="text-sm">
                                        <div className="flex flex-col">
                                            <span className="text-muted-foreground text-xs">رقم وثيقة الوصاية</span>
                                            <span className="font-medium">{selectedPerson.guardianshipDocNumber}</span>
                                        </div>
                                    </div>
                                </div>
                            )}

                            {/* فلتر الوثائق */}
                            <div className="flex gap-1 bg-muted/50 p-1 rounded-lg">
                                {Object.entries(PERSON_DOC_FILTER_AR).map(([key, label]) => (
                                    <Button
                                        key={key}
                                        variant={docFilter === key ? "default" : "ghost"}
                                        size="sm"
                                        className="flex-1 h-auto py-2"
                                        onClick={() => setDocFilter(key as PersonDocFilter)}
                                    >
                                        {label}
                                        {key === PERSON_DOC_FILTER.UPLOADED && personDocs && ` (${personDocs.uploaded.length})`}
                                        {key === PERSON_DOC_FILTER.MISSING && personDocs && ` (${personDocs.missing.length + personDocs.pledges.length})`}
                                    </Button>
                                ))}
                            </div>

                            {/* قائمة الوثائق */}
                            <div className="space-y-2">
                                {filteredDocs.uploaded.length === 0 && filteredDocs.missing.length === 0 && filteredDocs.pledges.length === 0 ? (
                                    <EmptyState
                                        icon={UserX}
                                        title="لا توجد وثائق"
                                        description="لا توجد وثائق مسجلة لهذا الشخص"
                                    />
                                ) : (
                                    <>
                                        {filteredDocs.uploaded.length > 0 && (
                                            <div className="space-y-2">
                                                <p className="text-xs font-bold text-green-600 flex items-center gap-1">
                                                    <CheckCircle2 className="w-3 h-3" />
                                                    وثائق مرفوقة ({filteredDocs.uploaded.length})
                                                </p>
                                                {filteredDocs.uploaded.map((doc) => (
                                                    <PersonDocCard
                                                        key={doc.documentID}
                                                        type="uploaded"
                                                        document={doc}
                                                        onView={() => handleViewDocument(doc)}
                                                        onDelete={canDeleteDoc ? () => handleDeleteDocument(doc) : undefined}
                                                    />
                                                ))}
                                            </div>
                                        )}
                                        {filteredDocs.missing.length > 0 && (
                                            <div className="space-y-2">
                                                <p className="text-xs font-bold text-red-600 flex items-center gap-1">
                                                    <FileWarning className="w-3 h-3" />
                                                    وثائق ناقصة ({filteredDocs.missing.length})
                                                </p>
                                                {filteredDocs.missing.map((doc) => (
                                                    <PersonDocCard
                                                        key={doc.missingDocumentID}
                                                        type="missing"
                                                        document={doc}
                                                        onUpload={canUploadDoc ? () => handleUploadDocument(doc.documentTypeID) : undefined}
                                                    />
                                                ))}
                                            </div>
                                        )}
                                        {filteredDocs.pledges.length > 0 && (
                                            <div className="space-y-2">
                                                <p className="text-xs font-bold text-amber-600 flex items-center gap-1">
                                                    <Clock className="w-3 h-3" />
                                                    تعهدات ({filteredDocs.pledges.length})
                                                </p>
                                                {filteredDocs.pledges.map((pledge) => (
                                                    <PersonDocCard
                                                        key={pledge.pledgeID}
                                                        type="pledge"
                                                        pledge={pledge}
                                                        onUpload={canUploadDoc ? () => handleUploadDocument() : undefined}
                                                    />
                                                ))}
                                            </div>
                                        )}
                                    </>
                                )}
                            </div>
                        </div>
                    )}
                </div>
            </div>

            {/* حوار عرض الوثيقة */}
            <DocumentViewerDialog
                open={viewerOpen}
                onOpenChange={setViewerOpen}
                document={viewingDoc}
                fileId={fileId}
            />

            {/* حوار رفع الوثيقة */}
            <UploadDocumentDialog
                open={uploadOpen}
                onOpenChange={setUploadOpen}
                fileId={fileId}
                defaultTypeId={uploadDocTypeId}
                missingEntityIds={getEntityIds()}
                onSaved={() => {
                    queryClient.invalidateQueries({ queryKey: ["person-documents", fileId] });
                    setUploadOpen(false);
                }}
            />

            {/* حوار تعديل/إضافة المستحق */}
            <EntitledPersonFormDialog
                open={editDialogOpen}
                onOpenChange={(open) => {
                    setEditDialogOpen(open);
                    if (!open) {
                        setEditingPersonId(null);
                        setCreateMode(null);
                    }
                }}
                mode={createMode === "entitled" && !editingPersonId ? "create" : "edit"}
                fileId={fileId}
                initialData={editingPersonId ? { entitledID: editingPersonId } as any : undefined}
                onSuccess={() => {
                    queryClient.invalidateQueries({ queryKey: ["entitled-persons", fileId] });
                    queryClient.invalidateQueries({ queryKey: ["person-documents", fileId] });
                    queryClient.invalidateQueries({ queryKey: ["retirement-file", fileId] });
                }}
            />

            {/* حوار تعديل/إضافة الوكيل */}
            <AgentDialog
                open={agentDialogOpen}
                onClose={() => {
                setAgentDialogOpen(false);
                setEditingPersonId(null);
                setEditingAgentData(null);
                setCreateMode(null);
            }}
                onSaved={() => {
                    queryClient.invalidateQueries({ queryKey: ["agents", fileId] });
                    queryClient.invalidateQueries({ queryKey: ["person-documents", fileId] });
                    queryClient.invalidateQueries({ queryKey: ["retirement-file", fileId] });
                }}
                fileId={fileId}
                fileNumber={fileNumber}
                mode={createMode === "agent" && !editingPersonId ? "create" : "edit"}
                initialData={editingAgentData ?? undefined}
            />

            {/* حوار تعديل/إضافة الوصي */}
            <GuardianDialog
                open={guardianDialogOpen}
                onClose={() => {
                setGuardianDialogOpen(false);
                setEditingPersonId(null);
                setEditingGuardianData(null);
                setCreateMode(null);
            }}
                onSaved={() => {
                    queryClient.invalidateQueries({ queryKey: ["guardians", fileId] });
                    queryClient.invalidateQueries({ queryKey: ["person-documents", fileId] });
                    queryClient.invalidateQueries({ queryKey: ["retirement-file", fileId] });
                }}
                fileId={fileId}
                availableEntitleds={entitledPersons.map((ep: EntitledPersonSummary) => ({
                    entitledID: ep.entitledID,
                    fullName: ep.fullName
                }))}
                mode={createMode === "guardian" && !editingPersonId ? "create" : "edit"}
                initialData={editingGuardianData ?? undefined}
            />

            {/* حوار تسجيل وفاة المستحق */}
            {deathDialogOpen && selectedPerson && (
                <RecordEntitledPersonDeathDialog
                    open={deathDialogOpen}
                    onOpenChange={setDeathDialogOpen}
                    personId={selectedPerson.id}
                    personName={selectedPerson.name}
                    fileId={fileId}
                />
            )}
        </div>
    );
}
