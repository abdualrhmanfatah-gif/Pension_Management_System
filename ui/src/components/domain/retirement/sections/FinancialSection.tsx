import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Ban, BookOpen, CreditCard, DollarSign, Download, Loader2, Pencil, Plus, Settings,Wallet, X } from "lucide-react";
import { useState } from "react";

import api from "@/api/axios";
import { documentsApi } from "@/api/documents";
import { retirementApi } from "@/api/retirement";
import { Heading, useConfirm } from "@/components/ui";
import { Badge } from "@/components/ui";
import { Button } from "@/components/ui";
import { Tabs, TabsContent,TabsList, TabsTrigger } from "@/components/ui";
import { EmptyState } from "@/components/ui";
import { useApiMutation } from "@/hooks/useApiMutation";
import { useDocumentBlob } from "@/hooks/useDocumentBlob";
import { formatCurrency, formatDate, getDocumentUrl } from "@/lib/utils";
import { cn } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";
import type { PaymentBookItem } from "@/types/retirement";

import { DocumentProcessDialog } from "../DocumentProcessDialog";
import type { BookFormValues } from "../PaymentBookDialog";
import { PaymentBookDialog } from "../PaymentBookDialog";
import type { RecipientFormValues } from "../RecipientsDialog";
import { RecipientsDialog } from "../RecipientsDialog";

function useAttachmentBlob(filePath: string | null | undefined) {
    const result = useDocumentBlob(filePath);
    return { ...result, error: false };
}

function BookPreview({ book, docs, fileId }: { book: PaymentBookItem; docs: any[]; fileId: number }) {
    if (docs.length === 0) {
        const { blobUrl, loading, error } = useAttachmentBlob(book.attachmentPath);
        const isImage = /\.(jpg|jpeg|png|gif|webp|bmp|svg)$/i.test(book.attachmentPath || "");

        if (loading) {
            return (
                <div className="flex flex-col items-center justify-center h-full gap-3">
                    <Loader2 className="w-8 h-8 text-primary animate-spin" />
                    <p className="text-sm text-muted-foreground">جاري تحميل المرفق...</p>
                </div>
            );
        }

        if (blobUrl && isImage) {
            return (
                <div className="flex items-center justify-center h-full p-4">
                    <img src={blobUrl} alt={book.bookNumber} className="max-w-full max-h-full object-contain rounded-lg" loading="lazy" />
                </div>
            );
        }

        if (blobUrl) {
            return <iframe src={blobUrl} className="w-full h-full border-0" title={book.bookNumber} />;
        }

        return (
            <div className="flex flex-col items-center justify-center h-full gap-4 p-6">
                <CreditCard className="w-16 h-16 text-muted-foreground/40" />
                <div className="text-center">
                    <p className="font-medium">{book.bookNumber}</p>
                    <p className="text-sm text-muted-foreground mt-1">
                        {error ? "فشل تحميل المرفق" : "لا يوجد مرفق"}
                    </p>
                </div>
            </div>
        );
    }

    return (
        <div className="h-full overflow-y-auto p-4 space-y-4">
            <p className="text-xs text-muted-foreground font-medium">{docs.length} صورة دفتر صرف</p>
            {docs.map((doc: any) => (
                <BookImage key={doc.documentID} doc={doc} fileId={fileId} />
            ))}
        </div>
    );
}

function BookImage({ doc, fileId }: { doc: any; fileId: number }) {
    const { blobUrl, loading, error } = useAttachmentBlob(doc.filePath);
    const isImage = /\.(jpg|jpeg|png|gif|webp|bmp|svg)$/i.test(doc.filePath || "");
    const canEdit = useHasPermission("File.Documents.Update");
    const queryClient = useQueryClient();
    const [processOpen, setProcessOpen] = useState(false);

    if (loading) {
        return (
            <div className="flex items-center justify-center h-48 rounded-lg border bg-muted/20">
                <Loader2 className="w-6 h-6 text-primary animate-spin" />
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex items-center justify-center h-32 rounded-lg border bg-muted/20">
                <p className="text-xs text-muted-foreground">فشل التحميل</p>
            </div>
        );
    }

    return (
        <div className="relative group rounded-lg border overflow-hidden">
            {isImage ? (
                <img src={blobUrl!} alt={doc.documentTypeName} className="w-full object-contain max-h-[600px] bg-muted/10" loading="lazy" />
            ) : (
                <iframe src={blobUrl!} className="w-full h-96 border-0" title={doc.documentTypeName} />
            )}
            <div className="absolute top-2 left-2 flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                {canEdit && (
                    <Button
                        variant="ghost"
                        size="icon"
                        className="h-7 w-7 bg-background/80 backdrop-blur-sm"
                        onClick={() => setProcessOpen(true)}
                        title="الإعدادات"
                    >
                        <Settings className="w-3.5 h-3.5" />
                    </Button>
                )}
                {canEdit && (
                    <Button
                        variant="ghost"
                        size="icon"
                        className="h-7 w-7 bg-background/80 backdrop-blur-sm"
                        onClick={() => {
                            const path = doc.filePath;
                            const url = path.startsWith("http") ? path : getDocumentUrl(path) ?? path;
                            api.get(url, { responseType: "blob" }).then((res) => {
                                const blob = new Blob([res.data], { type: res.headers["content-type"] });
                                const blobUrl = URL.createObjectURL(blob);
                                const a = document.createElement("a");
                                a.href = blobUrl;
                                a.download = doc.documentTypeName || "document";
                                document.body.appendChild(a);
                                a.click();
                                document.body.removeChild(a);
                                URL.revokeObjectURL(blobUrl);
                            });
                        }}
                        title="تحميل"
                    >
                        <Download className="w-3.5 h-3.5" />
                    </Button>
                )}
            </div>
            <DocumentProcessDialog
                open={processOpen}
                onOpenChange={setProcessOpen}
                document={{
                    documentID: doc.documentID,
                    documentTypeID: doc.documentTypeID || 0,
                    documentTypeName: doc.documentTypeName || "",
                    fileName: doc.fileName || "",
                    filePath: doc.filePath,
                    notes: doc.notes || undefined,
                    uploadedByUser: doc.uploadedByUser || undefined,
                    createdAt: doc.createdAt || new Date().toISOString(),
                }}
                fileId={fileId}
                onSaved={() => {
                    queryClient.invalidateQueries({ queryKey: ["documents", fileId] });
                    setProcessOpen(false);
                }}
            />
        </div>
    );
}

interface RecipientOption { id: string | number; name: string; }

const CHANGE_REASONS = [
    "إصدار أول",
    "تجديد سنوي",
    "تغيير وكيل",
    "تعيين وكيل",
    "تعديل وصي",
    "تعديل مستحقين",
    "بدل فاقد",
    "بدل تالف",
];
const RECIPIENT_TYPES = ["المتقاعد", "الوكيل", "الوصي", "المستحق"];

interface FinancialSectionProps {
    fileId: number;
    retireeId: number;
}

export function FinancialSection({ fileId, retireeId }: FinancialSectionProps) {
    const queryClient = useQueryClient();
    const [activeTab, setActiveTab] = useState("entitlements");
    const [editDialogOpen, setEditDialogOpen] = useState(false);
    const [editingBook, setEditingBook] = useState<PaymentBookItem | null>(null);
    const [selectedBook, setSelectedBook] = useState<PaymentBookItem | null>(null);
    const [selectedBookDocs, setSelectedBookDocs] = useState<any[]>([]);
    const [createBookOpen, setCreateBookOpen] = useState(false);
    const [createRecipientOpen, setCreateRecipientOpen] = useState(false);
    const canEditBook = useHasPermission("PaymentBook.Edit");
    const canCreateBook = useHasPermission("PaymentBook.Add");
    const canEndRecipient = useHasPermission("SalaryRecipient.Edit");
    const canCreateRecipient = useHasPermission("SalaryRecipient.Add");
    const { confirm, ConfirmDialogElement } = useConfirm();

    const { data: entitlements = [] } = useQuery({
        queryKey: ["financial-entitlements", retireeId],
        queryFn: () => retirementApi.getFinancialEntitlements(retireeId),
        enabled: !!retireeId,
    });

    const { data: paymentBooks = [] } = useQuery({
        queryKey: ["payment-books", fileId],
        queryFn: () => retirementApi.getPaymentBooks(fileId),
        enabled: !!fileId,
    });

    const { data: documents = [] } = useQuery({
        queryKey: ["documents", fileId],
        queryFn: () => documentsApi.getByFileId(fileId),
        enabled: !!fileId,
    });

    const getBookDocuments = () => {
        return documents.filter((d: any) =>
            d.documentTypeName?.includes("دفتر الصرف") || d.documentTypeName?.includes("صورة دفتر")
        );
    };

    const { data: recipients = [] } = useQuery({
        queryKey: ["salary-recipients", fileId],
        queryFn: () => retirementApi.getRecipients(fileId),
        enabled: !!fileId,
    });

    const saveMutation = useApiMutation(
        async (values: BookFormValues) => {
            if (editingBook) {
                const updateDto = {
                    bookNumber: values.bookNumber,
                    issueDate: values.issueDate ? new Date(values.issueDate).toISOString() : undefined,
                    expiryDate: values.expiryDate ? new Date(values.expiryDate).toISOString() : undefined,
                    status: values.status,
                    legacyColor: values.legacyColor,
                    changeReason: values.changeReason,
                    notes: values.notes,
                    issuingBranchID: values.issuingBranchID,
                };
                return retirementApi.updatePaymentBook(editingBook.paymentBookID, updateDto);
            }
        },
        {
            successMessage: "تم تحديث الدفتر بنجاح",
            invalidateKeys: [["payment-books", fileId], ["retirement-file", fileId]],
            onSuccess: () => {
                setEditDialogOpen(false);
                setEditingBook(null);
            }
        }
    );

    const endRecipientMutation = useApiMutation(
        async ({ id, reason }: { id: number; reason: string }) => {
            return retirementApi.endRecipient(id, { endDate: new Date().toISOString(), reason });
        },
        {
            successMessage: "تم إنهاء مستلم المعاش بنجاح",
            invalidateKeys: [["salary-recipients", fileId], ["retirement-file", fileId]],
        }
    );

    const getRecipientOptions = async (type: string): Promise<RecipientOption[]> => {
        if (type === "الوكيل") {
            const res = await retirementApi.getAgents(fileId);
            return (res || []).map((a: any) => ({ id: a.agentID, name: a.fullName }));
        }
        if (type === "الوصي") {
            const res = await retirementApi.getGuardians(fileId);
            return (res || []).map((g: any) => ({ id: g.guardianID, name: g.fullName }));
        }
        if (type === "المستحق") {
            const res = await retirementApi.getEntitledPersons(fileId);
            return (res || []).map((e: any) => ({ id: e.entitledID, name: e.fullName }));
        }
        return [];
    };

    const handleEditBook = (book: PaymentBookItem) => {
        setEditingBook(book);
        setEditDialogOpen(true);
    };

    // جلب بيانات الأوصياء والوكيل والمستحقين لإنشاء مستلم المعاش
    const { data: agentsData = [] } = useQuery({
        queryKey: ["agents", fileId],
        queryFn: () => retirementApi.getAgents(fileId),
        enabled: !!fileId && createRecipientOpen,
    });

    const { data: guardiansData = [] } = useQuery({
        queryKey: ["guardians", fileId],
        queryFn: () => retirementApi.getGuardians(fileId),
        enabled: !!fileId && createRecipientOpen,
    });

    const { data: entitledsData = [] } = useQuery({
        queryKey: ["entitled-persons", fileId],
        queryFn: () => retirementApi.getEntitledPersons(fileId),
        enabled: !!fileId && createRecipientOpen,
    });

    const createRecipientMutation = useApiMutation(
        async (values: RecipientFormValues) => {
            const formData = new FormData();
            formData.append("fileID", fileId.toString());
            formData.append("recipientType", values.recipientType);
            formData.append("targetID", values.targetID);
            formData.append("startDate", values.startDate + "T00:00:00");
            if (values.notes) formData.append("notes", values.notes);
            return retirementApi.createRecipient(formData as any);
        },
        {
            successMessage: "تم إضافة مستلم المعاش بنجاح",
            invalidateKeys: [["salary-recipients", fileId], ["retirement-file", fileId]],
            onSuccess: () => {
                setCreateRecipientOpen(false);
            }
        }
    );

    return (
        <div className="space-y-4 fade-in">
            <Heading level="h2">البيانات المالية</Heading>

            <Tabs value={activeTab} onValueChange={setActiveTab}>
                <TabsList>
                    <TabsTrigger value="entitlements">
                        الاستحقاقات ({entitlements.length})
                    </TabsTrigger>
                    <TabsTrigger value="books" className="gap-1">
                        دفاتر الصرف ({paymentBooks.length})
                        {canCreateBook && (
                            <span
                                role="button"
                                tabIndex={0}
                                className="inline-flex items-center justify-center h-5 w-5 rounded hover:bg-primary/10 cursor-pointer"
                                onClick={(e) => { e.stopPropagation(); setCreateBookOpen(true); }}
                            >
                                <Plus className="h-3.5 w-3.5" />
                            </span>
                        )}
                    </TabsTrigger>
                    <TabsTrigger value="recipients" className="gap-1">
                        مستلمو المعاش ({recipients.length})
                        {canCreateRecipient && (
                            <span
                                role="button"
                                tabIndex={0}
                                className="inline-flex items-center justify-center h-5 w-5 rounded hover:bg-primary/10 cursor-pointer"
                                onClick={(e) => { e.stopPropagation(); setCreateRecipientOpen(true); }}
                            >
                                <Plus className="h-3.5 w-3.5" />
                            </span>
                        )}
                    </TabsTrigger>
                </TabsList>

                <TabsContent value="entitlements">
                    {entitlements.length === 0 ? (
                        <EmptyState icon={DollarSign} title="لا توجد استحقاقات مالية" />
                    ) : (
                        <div className="space-y-2">
                            {entitlements.map((ent) => (
                                <div key={ent.entitlementID} className="flex items-center justify-between p-3 rounded-lg border border-border bg-card">
                                    <div>
                                        <p className="font-bold text-green-600">{formatCurrency(ent.amount)}</p>
                                        <p className="text-xs text-muted-foreground">
                                            من: {new Date(ent.effectiveDate).toLocaleDateString("ar-YE")}
                                            {ent.endDate && ` — إلى: ${new Date(ent.endDate).toLocaleDateString("ar-YE")}`}
                                        </p>
                                    </div>
                                    <Badge variant={ent.isActive ? "success" : "secondary"}>
                                        {ent.isActive ? "ساري" : "منتهي"}
                                    </Badge>
                                </div>
                            ))}
                        </div>
                    )}
                </TabsContent>

                <TabsContent value="books">
                    <div className={cn("flex gap-4", selectedBook && "min-h-[500px]")}>
                        <div className={cn(selectedBook ? "w-1/2" : "w-full")}>
                            {paymentBooks.length === 0 ? (
                                <EmptyState icon={BookOpen} title="لا توجد دفاتر صرف" />
                            ) : (
                                <div className="space-y-2">
                                    {paymentBooks.map((book) => (
                                        <div
                                            key={book.paymentBookID}
                                            onClick={() => {
                                                if (selectedBook?.paymentBookID === book.paymentBookID) {
                                                    setSelectedBook(null);
                                                    setSelectedBookDocs([]);
                                                } else {
                                                    setSelectedBook(book);
                                                    setSelectedBookDocs(getBookDocuments());
                                                }
                                            }}
                                            className={cn(
                                                "flex items-center justify-between p-3 rounded-lg border cursor-pointer transition-colors",
                                                selectedBook?.paymentBookID === book.paymentBookID
                                                    ? "border-primary bg-primary/5"
                                                    : "border-border bg-card hover:border-muted-foreground/30"
                                            )}
                                        >
                                            <div className="flex items-center gap-3">
                                                <CreditCard className={cn(
                                                    "w-5 h-5",
                                                    selectedBook?.paymentBookID === book.paymentBookID ? "text-primary" : "text-blue-500"
                                                )} />
                                                <div>
                                                    <p className="font-medium text-sm">{book.bookNumber}</p>
                                                    <p className="text-xs text-muted-foreground">
                                                        {book.bookType} — {book.recipientName || "المتقاعد"}
                                                    </p>
                                                </div>
                                            </div>
                                            <div className="flex items-center gap-2" onClick={(e) => e.stopPropagation()}>
                                                <Badge variant={book.status === "فعال" ? "success" : book.status === "منتهي" ? "secondary" : "danger"}>
                                                    {book.status}
                                                </Badge>
                                                {canEditBook && (
                                                    <Button
                                                        variant="ghost"
                                                        size="sm"
                                                        onClick={() => handleEditBook(book)}
                                                        className="h-8 w-8 p-0 rounded-lg hover:bg-blue-50 hover:text-blue-600 dark:hover:bg-blue-950/30"
                                                        title="تعديل"
                                                    >
                                                        <Pencil className="w-4 h-4" />
                                                    </Button>
                                                )}
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                        {selectedBook && (
                            <div className="w-1/2 flex flex-col border rounded-xl bg-card overflow-hidden h-[500px]">
                                <div className="flex items-center justify-between px-4 py-3 border-b bg-muted/30 shrink-0">
                                    <div className="flex items-center gap-2 min-w-0">
                                        <CreditCard className="w-5 h-5 text-primary shrink-0" />
                                        <span className="font-medium truncate">{selectedBook.bookNumber}</span>
                                        <Badge variant={selectedBook.status === "فعال" ? "success" : selectedBook.status === "منتهي" ? "secondary" : "danger"}>
                                            {selectedBook.status}
                                        </Badge>
                                    </div>
                                    <div className="flex items-center gap-1">
                                        {canEditBook && (
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                className="h-8 w-8 shrink-0"
                                                onClick={() => { handleEditBook(selectedBook); }}
                                                title="تعديل"
                                            >
                                                <Pencil className="w-4 h-4" />
                                            </Button>
                                        )}
                                        {selectedBookDocs.length > 0 && (
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                className="h-8 w-8 shrink-0"
                                                onClick={async () => {
                                                    for (const doc of selectedBookDocs) {
                                                        if (!doc.filePath) continue;
                                                        const path = doc.filePath;
                                                        const url = path.startsWith("http")
                                                            ? path
                                                            : getDocumentUrl(path) ?? path;
                                                        try {
                                                            const res = await api.get(url, { responseType: "blob" });
                                                            const blob = new Blob([res.data], { type: res.headers["content-type"] });
                                                            const blobUrl = URL.createObjectURL(blob);
                                                            const a = document.createElement("a");
                                                            a.href = blobUrl;
                                                            a.download = doc.documentTypeName || "document";
                                                            document.body.appendChild(a);
                                                            a.click();
                                                            document.body.removeChild(a);
                                                            URL.revokeObjectURL(blobUrl);
                                                        } catch {}
                                                    }
                                                }}
                                                title="تحميل الكل"
                                            >
                                                <Download className="w-4 h-4" />
                                            </Button>
                                        )}
                                        <Button variant="ghost" size="icon" className="h-8 w-8 shrink-0" onClick={() => { setSelectedBook(null); setSelectedBookDocs([]); }}>
                                            <X className="w-4 h-4" />
                                        </Button>
                                    </div>
                                </div>
                                <div className="flex-1 overflow-hidden">
                                    <BookPreview book={selectedBook} docs={selectedBookDocs} fileId={fileId} />
                                </div>
                            </div>
                        )}
                    </div>
                </TabsContent>

                <TabsContent value="recipients">
                    {recipients.length === 0 ? (
                        <EmptyState icon={Wallet} title="لا يوجد مستلمون" />
                    ) : (
                        <div className="space-y-2">
                            {recipients.map((rec) => (
                                <div key={rec.recipientID} className="flex items-center justify-between p-3 rounded-lg border border-border bg-card">
                                    <div>
                                        <p className="font-medium text-sm">
                                            {rec.retireeFullName || rec.agentFullName || rec.entitledFullName || rec.guardianFullName || "—"}
                                        </p>
                                        <p className="text-xs text-muted-foreground">
                                            من: {formatDate(rec.startDate)}
                                            {rec.endDate && ` — إلى: ${formatDate(rec.endDate)}`}
                                        </p>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <Badge variant={rec.isActive ? "success" : "secondary"}>
                                            {rec.isActive ? "نشط" : "منتهي"}
                                        </Badge>
                                        {rec.isActive && canEndRecipient && (
                                            <Button
                                                variant="ghost"
                                                size="sm"
                                                onClick={async () => {
                                                    const ok = await confirm({
                                                        title: "هل أنت متأكد من إنهاء هذا المستلم؟",
                                                        variant: "danger",
                                                    });
                                                    if (ok) {
                                                        endRecipientMutation.mutate({
                                                            id: rec.recipientID,
                                                            reason: "إنهاء من القائمة المالية"
                                                        });
                                                    }
                                                }}
                                                className="h-8 w-8 p-0 rounded-lg hover:bg-red-50 hover:text-red-600 dark:hover:bg-red-950/30"
                                                title="إنهاء"
                                            >
                                                <Ban className="w-4 h-4" />
                                            </Button>
                                        )}
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </TabsContent>
            </Tabs>

            {/* حوار تعديل دفتر الصرف */}
            <PaymentBookDialog
                open={editDialogOpen}
                onOpenChange={setEditDialogOpen}
                mode="edit"
                onSubmit={(v) => saveMutation.mutate(v)}
                isLoading={saveMutation.isPending}
                existingImages={documents
                    .filter((d: any) => d.documentTypeName?.includes("دفتر الصرف") || d.documentTypeName?.includes("صورة دفتر"))
                    .map((d: any) => ({
                        documentID: d.documentID,
                        filePath: d.filePath
                    }))}
                onDeleteImage={async (documentID) => {
                    try {
                        await documentsApi.delete(documentID);
                        queryClient.invalidateQueries({ queryKey: ["documents", fileId] });
                    } catch {}
                }}
                initialData={editingBook ? {
                    bookNumber: editingBook.bookNumber,
                    issueDate: editingBook.issueDate,
                    expiryDate: editingBook.expiryDate,
                    bookType: editingBook.bookType,
                    legacyColor: editingBook.legacyColor || undefined,
                    changeReason: editingBook.changeReason || undefined,
                    recipientType: editingBook.recipientType || undefined,
                    recipientName: editingBook.recipientName || undefined,
                    status: editingBook.status,
                    notes: editingBook.notes || undefined,
                    issuingBranchID: editingBook.issuingBranchID || undefined,
                } : undefined}
                lookups={{
                    changeReasons: CHANGE_REASONS,
                    recipientTypes: RECIPIENT_TYPES,
                    getRecipientOptions: getRecipientOptions
                }}
            />

            {/* حوار إنشاء دفتر صرف جديد */}
            <PaymentBookDialog
                open={createBookOpen}
                onOpenChange={setCreateBookOpen}
                mode="create"
                onSubmit={(v) => saveMutation.mutate(v)}
                isLoading={saveMutation.isPending}
                lookups={{
                    changeReasons: CHANGE_REASONS,
                    recipientTypes: RECIPIENT_TYPES,
                    getRecipientOptions: getRecipientOptions
                }}
            />

            {/* حوار إضافة مستلم معاش */}
            <RecipientsDialog
                open={createRecipientOpen}
                onOpenChange={setCreateRecipientOpen}
                onSubmit={(v) => createRecipientMutation.mutate(v)}
                isLoading={createRecipientMutation.isPending}
                lookups={{
                    agents: agentsData.map((a: any) => ({ id: a.agentID, fullName: a.fullName })),
                    guardians: guardiansData.map((g: any) => ({ id: g.guardianID, fullName: g.fullName })),
                    entitleds: entitledsData.map((e: any) => ({ id: e.entitledID, fullName: e.fullName })),
                }}
                retireeName={""}
            />
            {ConfirmDialogElement}
        </div>
    );
}
