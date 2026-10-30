import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Eye, Plus, User,Wallet, X } from "lucide-react";
import { useState } from "react";

import { salaryRecipientApi, type SalaryRecipientDto } from "@/api/salary-recipients";
import {
    Badge,
    Button,
<<<<<<< HEAD
    Card,
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    type CustomColumnDef,
    DataTable,
    EditableImage,
    Heading,
<<<<<<< HEAD
=======
    Text,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    useConfirm} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { formatDate, getPhotoUrl } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

import { SalaryRecipientFormDialog } from "./SalaryRecipientFormDialog";
import { SalaryRecipientViewerDialog } from "./SalaryRecipientViewerDialog";

interface RecipientsTabProps {
    fileId: number;
    retireeId?: number;
    retireeName?: string;
}

/**
 * تبويب مستلمي المعاش لملف التقاعد
 */
export function RecipientsTab({ fileId, retireeId, retireeName }: RecipientsTabProps) {
    const queryClient = useQueryClient();
    const { confirm, ConfirmDialogElement } = useConfirm();
    const canAdd = useHasPermission("SalaryRecipient.Add");
    const canEdit = useHasPermission("SalaryRecipient.Edit");
    const [isFormOpen, setIsFormOpen] = useState(false);
    
    const [viewerOpen, setViewerOpen] = useState(false);
    const [selectedRecipient, setSelectedRecipient] = useState<SalaryRecipientDto | null>(null);

    // جلب المستلمين
    const { data: response, isLoading } = useQuery({
        queryKey: ["salary-recipients", fileId],
        queryFn: () => salaryRecipientApi.getByFile(fileId)
    });
    const recipients = response?.data || [];

    const updatePhotoMutation = useApiMutation(
        ({ id, photo }: { id: number; photo: File }) => salaryRecipientApi.updatePhoto(id, photo),
        {
            successMessage: "تم تحديث صورة المستلم بنجاح",
            invalidateKeys: [["salary-recipients", fileId]],
        }
    );

    // طفرة إنهاء الاستحقاق
    const endMutation = useApiMutation(
        ({ id, endDate }: { id: number, endDate: string }) => salaryRecipientApi.end(id, { endDate }),
        {
            successMessage: "تم إنهاء استحقاق المستلم بنجاح",
            onSuccessCallback: () => {
                queryClient.invalidateQueries({ queryKey: ["salary-recipients", fileId] });
            }
        }
    );

    const handleEnd = async (id: number) => {
        const ok = await confirm({
            title: "تأكيد إنهاء الاستحقاق",
            description: "هل أنت متأكد من إنهاء استحقاق هذا المستلم؟ لن يتم تضمينه في عمليات توليد المعاشات القادمة.",
            variant: "danger"
        });
        if (ok) {
            endMutation.mutate({ id, endDate: new Date().toISOString().split("T")[0] });
        }
    };

    const handleView = (recipient: SalaryRecipientDto) => {
        setSelectedRecipient(recipient);
        setViewerOpen(true);
    };

    const columns: CustomColumnDef<SalaryRecipientDto>[] = [
        {
            key: "photo",
            header: "الصورة",
            render: (row) => (
                <EditableImage
                    src={getPhotoUrl(row.recipientPhotoPath)}
                    alt={row.recipientName}
                    fallbackIcon={<User className="w-5 h-5 text-slate-400" />}
                    className="w-10 h-10 rounded-full object-cover border border-slate-200"
                    canEdit={canEdit}
                    onUpload={async (file) => {
                        await updatePhotoMutation.mutateAsync({ id: row.recipientID, photo: file });
                    }}
                />
            )
        },
        {
            key: "recipientName",
            header: "اسم المستلم",
            accessor: (row) => row.recipientName,
            className: "font-bold"
        },
        {
            key: "recipientType",
            header: "نوع المستلم",
            render: (row) => <Badge variant="outline" className="font-medium">{row.recipientType}</Badge>,
        },

        {
            key: "startDate",
            header: "تاريخ البدء",
            render: (row) => <span className="font-mono text-xs">{formatDate(row.startDate)}</span>,
        },
        {
<<<<<<< HEAD
            key: "sharePercent",
            header: "حصة المعاش",
            render: (row) => row.sharePercent == null ? "—" : `${row.sharePercent}%`,
        },
        {
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            key: "status",
            header: "الحالة",
            render: (row) => (
                <Badge variant={row.isActive ? "success" : "outline"} className="font-bold">
                    {row.isActive ? "نشط" : "منتهي"}
                </Badge>
            ),
        },
        {
            key: "actions",
            header: "",
            render: (row) => (
                <div className="flex items-center gap-1 justify-end">
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleView(row)}
                        className="min-h-[44px] min-w-[44px] text-muted-foreground hover:text-primary"
                    >
                        <Eye className="w-4 h-4" />
                    </Button>
                    {row.isActive && canEdit && (
                        <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleEnd(row.recipientID)}
                            className="text-destructive hover:text-destructive hover:bg-destructive/10 px-2"
                            loading={endMutation.isPending && endMutation.variables?.id === row.recipientID}
                        >
                            <X className="w-4 h-4 me-1" /> إنهاء
                        </Button>
                    )}
                </div>
            ),
        }
    ];

    return (
        <div className="space-y-6 animate-in fade-in duration-500">
            {ConfirmDialogElement}

<<<<<<< HEAD
            <Card className="flex flex-col md:flex-row md:items-center justify-between gap-2 md:gap-4 bg-muted/30 p-3 md:p-5">
                <div className="space-y-0.5 md:space-y-1 text-start">
                    <Heading level="h3" className="font-bold flex items-center gap-1.5 md:gap-2 text-sm md:text-base">
                        <Wallet className="w-4 h-4 md:w-5 md:h-5 text-primary" /> مستلمو المعاش
                    </Heading>
                    <p className="text-xs md:text-sm text-muted-foreground hidden sm:block">
                        تحديد المستحقين الماليين للمنافع وصرف المعاش التقاعدي
                    </p>
                </div>
                {canAdd && (
                    <Button 
                        size="sm" 
                        onClick={() => setIsFormOpen(true)}
                        className="gap-1 rounded-xl font-bold h-8 md:h-10 px-4 md:px-6 transition-all text-xs md:text-sm"
                    >
                        <Plus className="w-3.5 h-3.5 md:w-4 md:h-4 ms-1" /> إضافة مستلم
                    </Button>
                )}
            </Card>
=======
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="space-y-1">
                    <Heading level="h3" className="text-lg font-black flex items-center gap-2 text-foreground tracking-tight">
                        <Wallet className="w-5 h-5 text-primary" /> مستلمو المعاش
                    </Heading>
                    <Text variant="caption" className="font-medium">تحديد المستحقين الماليين ونسب توزيع المعاش التقاعدي</Text>
                </div>

                <div className="flex items-center gap-3">
                    {canAdd && (
                        <Button onClick={() => setIsFormOpen(true)} className="gap-2 rounded-2xl h-11 font-bold">
                            <Plus className="w-4 h-4" /> إضافة مستلم
                        </Button>
                    )}
                </div>
            </div>
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

            <div className="bg-card border rounded-2xl overflow-hidden">
                <DataTable
                    columns={columns}
                    data={recipients}
                    loading={isLoading}
                    emptyMessage="لا يوجد مستلمون مسجلون حالياً لهذا الملف."
                />
            </div>

            {isFormOpen && (
                <SalaryRecipientFormDialog
                    open={isFormOpen}
                    onClose={() => setIsFormOpen(false)}
                    fileId={fileId}
                    retireeId={retireeId}
                    retireeName={retireeName}
                    onSaved={() => queryClient.invalidateQueries({ queryKey: ["salary-recipients", fileId] })}
                />
            )}

            <SalaryRecipientViewerDialog 
                open={viewerOpen} 
                onClose={() => setViewerOpen(false)} 
                recipient={selectedRecipient} 
            />
        </div>
    );
}
