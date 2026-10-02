import { useQueryClient } from "@tanstack/react-query";
import { User } from "lucide-react";

import type { SalaryRecipientDto } from "@/api/salary-recipients";
import { salaryRecipientApi } from "@/api/salary-recipients";
import { EditableImage, Heading, Text } from "@/components/ui";
import { Badge } from "@/components/ui/Badge";
import { Dialog, DialogContent } from "@/components/ui/Dialog";
import { useApiMutation } from "@/hooks";
import { formatDate, getPhotoUrl } from "@/lib/utils";
import { useHasPermission } from "@/stores/authStore";

interface SalaryRecipientViewerDialogProps {
    open: boolean;
    onClose: () => void;
    recipient: SalaryRecipientDto | null;
}

export function SalaryRecipientViewerDialog({ open, onClose, recipient }: SalaryRecipientViewerDialogProps) {
    const queryClient = useQueryClient();
    const canEdit = useHasPermission("SalaryRecipient.Edit");
    
    const updatePhotoMutation = useApiMutation(
        ({ id, photo }: { id: number; photo: File }) => salaryRecipientApi.updatePhoto(id, photo),
        {
            successMessage: "تم تحديث الصورة الشخصية بنجاح",
            onSuccessCallback: () => {
                if (recipient) {
                    queryClient.invalidateQueries({ queryKey: ["salary-recipients", recipient.fileID] });
                }
            }
        }
    );

    if (!recipient) return null;

    return (
        <>
            <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
                <DialogContent size="md" aria-labelledby="recipient-dialog-title" className="p-0 border-none bg-slate-50 overflow-visible">
                    <div className="h-32 bg-gradient-to-r from-primary/90 to-primary relative rounded-t-xl">
                        <div className="absolute inset-0 opacity-10" style={{ backgroundImage: "url('/pension-logo.png')", backgroundSize: "100px", backgroundRepeat: "repeat" }} />
                        <div className="absolute -bottom-16 start-1/2 -translate-x-1/2 rtl:translate-x-1/2 z-20">
                            <div className="relative group">
                                <EditableImage
                                    src={recipient.recipientPhotoPath ? getPhotoUrl(recipient.recipientPhotoPath) : null}
                                    alt="صورة المستلم"
                                    fallbackIcon={<User className="w-16 h-16 text-slate-200" />}
                                    className="w-32 h-32 rounded-3xl border-4 border-white bg-white"
                                    canEdit={canEdit}
                                    onUpload={async (file) => {
                                        await updatePhotoMutation.mutateAsync({ id: recipient.recipientID, photo: file });
                                    }}
                                />
                            </div>
                        </div>
                    </div>

                    <div className="pt-20 pb-8 px-6 space-y-8">
                        {/* Identity Details */}
                        <div className="mt-4 text-center space-y-3 w-full">
                            <div className="space-y-1">
                                <Heading level="h3" id="recipient-dialog-title" className="text-xl sm:text-2xl font-black text-foreground tracking-tight">{recipient.recipientName}</Heading>
                                <div className="flex items-center justify-center gap-2">
                                    <Badge variant="secondary" className="px-4 py-1 rounded-full font-black text-[10px] sm:text-xs border border-slate-200">
                                        {recipient.recipientType}
                                    </Badge>
                                    <Badge 
                                        variant={recipient.isActive ? "success" : "outline"} 
                                        className={`px-4 py-1 rounded-full font-black text-[10px] sm:text-xs ${!recipient.isActive ? "bg-slate-100 text-slate-400 border-slate-200" : ""}`}
                                    >
                                        {recipient.isActive ? "نشط حالياً" : "منتهي الصلاحية"}
                                    </Badge>
                                </div>
                            </div>

                            {/* Info Grid */}
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 pt-4">
                                <DetailCard label="رقم الملف التقاعدي" value={recipient.fileNumber} />
                                <DetailCard label="تاريخ بدء الاستحقاق" value={formatDate(recipient.startDate)} />
                                <DetailCard label="تاريخ نهاية الاستحقاق" value={recipient.endDate ? formatDate(recipient.endDate) : "ـــ ـــ ـــ"} isDanger={!recipient.isActive} />
                                <DetailCard label="تاريخ التسجيل بالنظام" value={formatDate(recipient.createdAt)} />
                            </div>

                            {/* Notes */}
                            {recipient.notes && (
                                <div className="bg-card p-4 rounded-2xl border border-slate-200/60 text-start relative overflow-hidden">
                                    <div className="absolute top-0 start-0 w-1 h-full bg-primary/20" />
                                    <span className="text-[10px] sm:text-xs font-black text-slate-400 uppercase tracking-widest block mb-2">ملاحظات إدارية:</span>
                                    <Text variant="body" className="text-slate-700 leading-relaxed font-bold italic">
                                        "{recipient.notes}"
                                    </Text>
                                </div>
                            )}
                        </div>
                    </div>
                </DialogContent>
            </Dialog>
        </>
    );
}

function DetailCard({ label, value, isDanger }: { label: string; value: string; isDanger?: boolean }) {
    return (
        <div className="bg-card p-4 rounded-2xl border border-slate-200/50 flex flex-col gap-1 transition-all hover:bg-slate-50">
            <span className="text-[9px] font-black text-slate-400 uppercase tracking-wider">
                {label}
            </span>
            <div className={`text-sm font-black ${isDanger ? "text-red-600" : "text-slate-800"}`}>
                {value}
            </div>
        </div>
    );
}