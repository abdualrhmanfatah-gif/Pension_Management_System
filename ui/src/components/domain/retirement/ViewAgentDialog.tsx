import { useQuery } from "@tanstack/react-query";
import { Calendar, CheckCircle, FileText, Hash, Phone, UserCheck, XCircle } from "lucide-react";

import { documentsApi } from "@/api/documents";
import { retirementApi } from "@/api/retirement";
import { Badge, BaseFormDialog, Button, EditableImage, Heading } from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { formatDate, getDocumentUrl, getPhotoUrl, resolvePersonalPhotoPath } from "@/lib/utils";

interface Agent {
    agentID: number;
    fileID: number;
    fileNumber: string;
    fullName: string;
    nationalID: string;
    personalPhotoPath?: string;
    phone?: string;
    agencyNumber: string;
    agencyDate: string;
    agencyEndDate?: string;
    notes?: string;
    isActive: boolean;
    createdAt: string;
    updatedAt?: string;
}

interface Props {
    open: boolean;
    onClose: () => void;
    agent: Agent | null;
    fileId: number;
}

export function ViewAgentDialog({ open, onClose, agent, fileId }: Props) {
    const { data: agentDocuments = [], isLoading: loadingDocuments } = useQuery({
        queryKey: ["documents", "file", fileId, "agent", agent?.agentID],
        queryFn: async () => {
            const allDocs = await documentsApi.getByFileId(fileId);
            return allDocs.filter(doc => doc.agentID === agent?.agentID);
        },
        enabled: open && !!agent
    });

    const updatePhotoMutation = useApiMutation(
        (formData: FormData) => retirementApi.updateAgent(agent!.agentID, formData),
        {
            successMessage: "تم تحديث الصورة الشخصية للوكيل بنجاح",
            invalidateKeys: [["agents", fileId], ["retirement-file"]],
        }
    );

    const getAgentPhotoUrl = (agent: any) => getPhotoUrl(resolvePersonalPhotoPath(agent));

    if (!agent) return null;

    return (
        <>
            <BaseFormDialog
                isOpen={open}
                onClose={onClose}
                title="استعراض بيانات الوكيل"
                description="معلومات تفصيلية عن الوكيل والوثائق المرفقة"
                size="full"
                footer={null}
            >
                <div className="space-y-4">
                    <div className="bg-gradient-to-l from-primary/5 to-primary/10 rounded-xl p-4 lg:p-6 border border-primary/20">
                        <div className="flex flex-col sm:flex-row gap-3 lg:gap-4 items-start">
                            <div className="flex-shrink-0">
                                <EditableImage
                                    src={getAgentPhotoUrl(agent)}
                                    alt={agent.fullName}
                                    fallbackIcon={<UserCheck className="w-12 lg:w-16 h-12 lg:h-16 text-muted-foreground" />}
                                    className="w-20 lg:w-32 h-20 lg:h-32 rounded-2xl border-4 border-white"
                                    onUpload={async (file) => {
                                        const formData = new FormData();
                                        formData.append("FullName", agent.fullName);
                                        formData.append("NationalID", agent.nationalID);
                                        formData.append("Phone", agent.phone || "");
                                        formData.append("AgencyNumber", agent.agencyNumber || "");
                                        
                                        const agencyDate = agent.agencyDate ? agent.agencyDate.split('T')[0] : "";
                                        formData.append("AgencyDate", agencyDate);
                                        
                                        if (agent.agencyEndDate && agent.agencyEndDate !== "0001-01-01T00:00:00") {
                                             formData.append("AgencyEndDate", agent.agencyEndDate.split('T')[0]);
                                        }
                                        
                                        formData.append("Notes", agent.notes || "");
                                        formData.append("PersonalPhoto", file);

                                        await updatePhotoMutation.mutateAsync(formData);
                                    }}
                                />
                            </div>

                            <div className="flex-1 space-y-2 lg:space-y-3 min-w-0">
                                <div>
                                    <Heading level="h2" className="text-base lg:text-2xl font-bold text-foreground break-words">{agent.fullName}</Heading>
                                    <p className="text-xs lg:text-sm text-muted-foreground mt-1">وكيل مفوض</p>
                                </div>
                                <div className="flex flex-wrap gap-2 lg:gap-4">
                                    <Badge variant={agent.isActive ? "success" : "outline"} className="text-xs lg:text-sm px-2.5 lg:px-3 py-0.5 lg:py-1">
                                        {agent.isActive ? <><CheckCircle className="w-3.5 h-3.5 lg:w-4 lg:h-4 ms-1 inline" /> نشط</> : <><XCircle className="w-3.5 h-3.5 lg:w-4 lg:h-4 ms-1 inline" /> منتهي</>}
                                    </Badge>
                                    <span className="text-xs lg:text-sm text-muted-foreground font-bold">رقم الملف: {agent.fileNumber}</span>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 lg:gap-4">
                        <div className="bg-card border rounded-xl lg:rounded-lg p-3 lg:p-4 space-y-2 lg:space-y-3">
                            <Heading level="h3" className="text-sm lg:text-lg font-semibold flex items-center gap-2 text-primary border-b pb-2"><UserCheck className="w-4 h-4 lg:w-5 lg:h-5 shrink-0" /> المعلومات الشخصية</Heading>
                            <div className="space-y-2">
                                <div className="flex items-start gap-2 lg:gap-3">
                                    <Hash className="w-3.5 h-3.5 lg:w-4 lg:h-4 text-muted-foreground mt-1 flex-shrink-0" />
                                    <div className="flex-1"><p className="text-[10px] lg:text-xs text-muted-foreground">رقم الهوية الوطنية</p><p className="text-xs lg:text-sm font-mono font-medium">{agent.nationalID}</p></div>
                                </div>
                                {agent.phone && (
                                    <div className="flex items-start gap-2 lg:gap-3">
                                        <Phone className="w-3.5 h-3.5 lg:w-4 lg:h-4 text-muted-foreground mt-1 flex-shrink-0" />
                                        <div className="flex-1"><p className="text-[10px] lg:text-xs text-muted-foreground">رقم الهاتف</p><p className="text-xs lg:text-sm font-medium">{agent.phone}</p></div>
                                    </div>
                                )}
                            </div>
                        </div>

                        {agent.agencyNumber && (
                        <div className="bg-card border rounded-xl lg:rounded-lg p-3 lg:p-4 space-y-2 lg:space-y-3">
                            <Heading level="h3" className="text-sm lg:text-lg font-semibold flex items-center gap-2 text-primary border-b pb-2"><FileText className="w-4 h-4 lg:w-5 lg:h-5 shrink-0" /> معلومات الوكالة</Heading>
                            <div className="space-y-2">
                                <div className="flex items-start gap-2 lg:gap-3">
                                    <Hash className="w-3.5 h-3.5 lg:w-4 lg:h-4 text-muted-foreground mt-1 flex-shrink-0" />
                                    <div className="flex-1"><p className="text-[10px] lg:text-xs text-muted-foreground">رقم الوكالة</p><p className="text-xs lg:text-sm font-medium">{agent.agencyNumber}</p></div>
                                </div>
                                {agent.agencyDate && (
                                <div className="flex items-start gap-2 lg:gap-3">
                                    <Calendar className="w-3.5 h-3.5 lg:w-4 lg:h-4 text-muted-foreground mt-1 flex-shrink-0" />
                                    <div className="flex-1"><p className="text-[10px] lg:text-xs text-muted-foreground">تاريخ الوكالة</p><p className="text-xs lg:text-sm font-medium">{formatDate(agent.agencyDate)}</p></div>
                                </div>
                                )}
                                {agent.agencyEndDate && agent.agencyEndDate !== "0001-01-01T00:00:00" && (
                                    <div className="flex items-start gap-2 lg:gap-3">
                                        <Calendar className="w-3.5 h-3.5 lg:w-4 lg:h-4 text-muted-foreground mt-1 flex-shrink-0" />
                                        <div className="flex-1"><p className="text-[10px] lg:text-xs text-muted-foreground">تاريخ انتهاء الوكالة</p><p className="text-xs lg:text-sm font-medium">{formatDate(agent.agencyEndDate)}</p></div>
                                    </div>
                                )}
                            </div>
                        </div>
                        )}
                    </div>

                    <div className="bg-card border rounded-xl lg:rounded-lg p-3 lg:p-4 space-y-3 lg:space-y-4">
                        <Heading level="h3" className="text-sm lg:text-lg font-semibold flex items-center gap-2 text-primary border-b pb-2"><FileText className="w-4 h-4 lg:w-5 lg:h-5 shrink-0" /> الوثائق المرفقة ({agentDocuments.length})</Heading>
                        {loadingDocuments ? (
                            <div className="text-center py-8 text-muted-foreground">جاري تحميل الوثائق...</div>
                        ) : agentDocuments.length === 0 ? (
                            <p className="text-sm text-muted-foreground text-center py-6">لا توجد وثائق مرفقة لهذا الوكيل</p>
                        ) : (
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                {agentDocuments.map(doc => {
                                    const docUrl = getDocumentUrl(doc.filePath);
                                    const isPdf = doc.fileName?.toLowerCase().endsWith('.pdf');
                                    return (
                                        <div key={doc.documentID} className="border rounded-lg p-4 bg-muted/30">
                                            <div className="flex items-center gap-2 mb-3">
                                                <FileText className="w-5 h-5 text-primary" />
                                                <h4 className="font-medium">{doc.documentTypeName}</h4>
                                            </div>
                                            {docUrl ? (
                                                <div className="space-y-2">
                                                    {isPdf ? (
                                                        <div className="w-full h-48 border rounded-lg overflow-hidden bg-muted/50"><iframe src={docUrl} className="w-full h-full border-0" title={doc.fileName} /></div>
                                                    ) : (
                                                        <img src={docUrl} alt={doc.documentTypeName} className="w-full rounded-lg border cursor-pointer hover:opacity-90 transition-opacity max-h-64 object-contain bg-muted/30" loading="lazy" onClick={() => window.open(docUrl, '_blank')} />
                                                    )}
                                                    <p className="text-xs text-muted-foreground truncate" title={doc.fileName}>{doc.fileName}</p>
                                                </div>
                                            ) : <p className="text-sm text-muted-foreground text-center py-4">مسار الوثيقة غير متوفر</p>}
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </div>

                    {agent.notes && (
                        <div className="bg-card border rounded-xl lg:rounded-lg p-3 lg:p-4">
                            <Heading level="h3" className="text-sm lg:text-lg font-semibold flex items-center gap-2 text-primary border-b pb-2 mb-2 lg:mb-3"><FileText className="w-4 h-4 lg:w-5 lg:h-5 shrink-0" /> ملاحظات</Heading>
                            <p className="text-xs lg:text-sm text-foreground whitespace-pre-wrap">{agent.notes}</p>
                        </div>
                    )}
                </div>
                <div className="flex justify-end pt-4">
                    <Button variant="outline" onClick={onClose} className="rounded-xl font-bold h-10 px-6">إغلاق</Button>
                </div>
            </BaseFormDialog>
        </>
    );
}
