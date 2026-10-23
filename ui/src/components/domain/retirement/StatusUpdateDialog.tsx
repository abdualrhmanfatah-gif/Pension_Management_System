import { zodResolver } from "@hookform/resolvers/zod";
import { ArrowDown, Flag,RefreshCw, ShieldBan, ShieldCheck } from "lucide-react";
import { Controller, type SubmitHandler, useForm, useWatch } from "react-hook-form";
import { z } from "zod";

import { 
    Badge,
    BaseFormDialog, 
    type BaseOverlayProps,
    FormField, 
    FormSection,
    NativeSelect, 
    NoteBox,
    Textarea} from "@/components/ui";
import { cn } from "@/lib/utils";

// ─── Validation Schema ─────────────────────────────────

const statusUpdateSchema = z.object({
    newStatusID: z.number().min(1, "يجب اختيار الحالة الجديدة"),
    reason: z.string().trim().optional(),
}).refine((data) => {
    // Business Rule: Mandatory reason for sensitive statuses
    // Suspended (3), Closed (4), Rejected (8) are sensitive
    const sensitiveStatusIDs = [4]; 
    if (sensitiveStatusIDs.includes(data.newStatusID)) {
        return data.reason && data.reason.length >= 10;
    }
    return true;
}, {
    message: "يجب إدخال سبب التغيير (10 أحرف على الأقل) لهذه الحالة",
    path: ["reason"],
});

type StatusUpdateValues = z.infer<typeof statusUpdateSchema>;

interface StatusOption {
    statusID: number;
    statusName: string;
}

interface Props extends BaseOverlayProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    currentStatusId?: number | null;
    currentStatusName?: string;
    allowedStatuses: StatusOption[];
    onStatusUpdated: SubmitHandler<StatusUpdateValues>;
    isLoading?: boolean;
}

const statusWarnings: Record<number, { title: string; message: string; icon: React.ReactNode }> = {
    4: {
        title: "انتهى استحقاقه",
        message: "انتهت جميع الاستحقاقات المرتبطة بهذا الملف، مع الاحتفاظ ببياناته وسجله. إعادة الاستحقاق تتم على الملف نفسه عبر المراجعة والاعتماد.",
        icon: <ShieldBan className="w-5 h-5" />,
    },
    6: {
        title: "إرسال للمراجعة",
        message: "يصبح الملف معلقًا ويتوقف صرف المعاش حتى اعتماد المراجعة، مع استمرار تسجيل المبالغ المستحقة.",
        icon: <ShieldCheck className="w-5 h-5" />,
    },
};

// ─── Component ─────────────────────────────────────────

/**
 * Standardized StatusUpdateDialog.
 * Refactored for Batch 4: Handles workflow state changes with conditional mandatory reasoning.
 */
export function StatusUpdateDialog({ 
    open, 
    onOpenChange,
    currentStatusName,
    allowedStatuses, 
    onStatusUpdated, 
    isLoading 
}: Props) {
    const form = useForm<StatusUpdateValues>({
        resolver: zodResolver(statusUpdateSchema),
        defaultValues: {
            newStatusID: 0,
            reason: "",
        }
    });

    const { control, register, formState: { errors, isDirty } } = form;
    const selectedStatusID = useWatch({ control, name: "newStatusID" });
    const selectedStatus = allowedStatuses.find(s => s.statusID === selectedStatusID);

    const isSensitiveStatus = [4].includes(selectedStatusID);
    const statusWarning = selectedStatusID ? statusWarnings[selectedStatusID] : null;

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="تحديث حالة الملفات"
            description="قم بتغيير حالة الملفات للانتقال إلى المرحلة التالية من سير العمل"
            size="full"
            isLoading={isLoading}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(onStatusUpdated)}
            resetOnOpen={true}
            reset={form.reset}
            showGlobalError
            formErrors={errors}
            submitLabel="تحديث الحالة الآن"
        >
            <div className="space-y-4">
                {/* بطاقة الحالة الحالية → الحالة الجديدة */}
                <div className="flex items-center justify-center gap-4 p-5 bg-muted/30 rounded-xl border">
                    <div className="text-center">
                        <p className="text-xs text-muted-foreground mb-2 font-medium">الحالة الحالية</p>
                        <Badge variant="outline" className="text-sm px-4 py-1.5">
                            {currentStatusName || "غير معروفة"}
                        </Badge>
                    </div>
                    <ArrowDown className="w-5 h-5 text-muted-foreground shrink-0" />
                    <div className="text-center">
                        <p className="text-xs text-muted-foreground mb-2 font-medium">الحالة الجديدة</p>
                        <Badge 
                            variant={isSensitiveStatus ? "destructive" : "success"} 
                            className={cn(
                                "text-sm px-4 py-1.5 transition-all",
                                !selectedStatusID && "opacity-100"
                            )}
                        >
                            {selectedStatus?.statusName || "— اختر حالة —"}
                        </Badge>
                    </div>
                </div>

                {/* تحذير خاص بالحالة المحددة */}
                {statusWarning && (
                    <NoteBox type="warning" title={statusWarning.title}>
                        {statusWarning.message}
                    </NoteBox>
                )}

                <FormSection title="تحديد الحالة الجديدة" icon={Flag}>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <FormField label="الحالة الجديدة" required error={errors.newStatusID?.message}>
                            <Controller
                                name="newStatusID"
                                control={control}
                                render={({ field }) => (
                                    <NativeSelect 
                                        options={allowedStatuses.map(s => ({ value: s.statusID.toString(), label: s.statusName }))}
                                        value={field.value.toString()}
                                        onChange={(val) => field.onChange(Number(val))}
                                        placeholder="اختر الحالة الجديدة..."
                                        className="h-12 text-base font-medium"
                                    />
                                )}
                            />
                        </FormField>

                        <FormField 
                            label="سبب التغيير / الملاحظات" 
                            required={isSensitiveStatus} 
                            error={errors.reason?.message}
                            hint={isSensitiveStatus ? "مطلوب (10 أحرف على الأقل)" : "اختياري"}
                        >
                            <Textarea 
                                {...register("reason")} 
                                placeholder="أدخل مبررات التغيير هنا..." 
                                className={cn(
                                    "min-h-[120px] transition-all",
                                    isSensitiveStatus ? "border-amber-300 bg-amber-50/30" : "bg-muted/10"
                                )}
                            />
                        </FormField>
                    </div>
                </FormSection>
                
                <NoteBox type="warning" title="تنبيه سير العمل">
                    تأكد من مراجعة كافة الوثائق المطلوبة قبل اعتماد تغيير الحالة. بعض الحالات تتطلب موافقة المدير المباشر.
                </NoteBox>

                <div className="flex items-center gap-2 text-[10px] sm:text-xs text-muted-foreground bg-muted/30 p-2 rounded-lg justify-center border border-dashed">
                    <RefreshCw className="w-3 h-3 animate-spin-slow" />
                    <span>سيتم أرشفة هذا التحديث في سجل الحركات الخاص بهذا الملف</span>
                </div>
            </div>
        </BaseFormDialog>
    );
}
