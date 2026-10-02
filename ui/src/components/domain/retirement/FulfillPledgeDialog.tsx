"use no memo";

import { zodResolver } from "@hookform/resolvers/zod";
import { FileCheck } from "lucide-react";
import { useMemo } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";

import { 
  Badge,
  BaseFormDialog, 
  FileField, 
  NoteBox} from "@/components/ui";
import type { PledgeItem } from "@/types/retirement";

// ─── Validation Schema ─────────────────────────────────

const fulfillPledgeSchema = z.object({
    documents: z.array(z.object({
        missingDocumentID: z.number(),
        documentTypeName: z.string(),
        file: z.instanceof(File).optional().nullable(),
        alreadyFulfilled: z.boolean().optional(),
    })).refine(docs => docs.every(d => d.alreadyFulfilled) || docs.some(d => d.file), {
        message: "يجب رفع وثيقة واحدة على الأقل",
    }),
});

export type FulfillPledgeFormValues = z.infer<typeof fulfillPledgeSchema>;

interface FulfillPledgeDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onClose: () => void;
    onSubmit: (values: FulfillPledgeFormValues) => Promise<void>;
    isLoading?: boolean;
    pledge: PledgeItem;
}

/**
 * حوار استيفاء التعهد عن طريق رفع الوثائق
 */
export function FulfillPledgeDialog({ 
    open, 
    onOpenChange,
    onClose, 
    onSubmit, 
    isLoading,
    pledge
}: FulfillPledgeDialogProps) {
    
    const missingDocuments = useMemo(() => {
        return pledge.missingDocuments || [];
    }, [pledge]);

    const form = useForm<FulfillPledgeFormValues>({
        resolver: zodResolver(fulfillPledgeSchema),
        defaultValues: {
            documents: missingDocuments.map(doc => ({
                missingDocumentID: doc.missingDocumentID,
                documentTypeName: doc.documentTypeName,
                file: null,
                alreadyFulfilled: doc.status === "Fulfilled"
            }))
        }
    });

    const { control, formState: { errors, isDirty }, handleSubmit, reset } = form;

    const handleFormSubmit = async (values: FulfillPledgeFormValues) => {
        await onSubmit(values);
        reset();
    };

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            onClose={onClose}
            title="استيفاء التعهد برفع الوثائق"
            description={`رفع الوثائق المشمولة في التعهد رقم #${pledge.pledgeNumber || pledge.pledgeID}`}
            size="full"
            isLoading={isLoading}
            isDirty={isDirty}
            onSubmit={handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={reset}
            showGlobalError
            formErrors={errors}
            submitLabel="تحديث وحفظ الوثائق"
        >
            <div className="space-y-6 text-start">
                <NoteBox type="info">
                    يرجى رفع صور الوثائق التي تم استلامها. الوثائق التي لن يتم رفع ملفات لها ستبقى في قائمة النواقص وسيظل التعهد نشطاً حتى استكمالها.
                </NoteBox>

                <div className="space-y-4">
                    {missingDocuments.map((doc, index) => {
                        const isFulfilled = doc.status === "Fulfilled";
                        return (
                            <div 
                                key={doc.missingDocumentID} 
                                className={`p-4 rounded-xl border border-border/60 flex flex-col md:flex-row md:items-center justify-between gap-4 transition-colors ${isFulfilled ? 'bg-success/5 border-success/30' : 'bg-muted/20'}`}
                            >
                                <div className="flex-1">
                                    <div className="flex items-center gap-2 mb-1">
                                        <FileCheck className={`w-4 h-4 ${isFulfilled ? 'text-success' : 'text-primary'}`} />
                                        <span className={`font-black text-sm ${isFulfilled ? 'opacity-70 line-through text-success' : ''}`}>
                                            {doc.documentTypeName}
                                        </span>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        {doc.entityName && (
                                            <Badge variant="outline" className="text-[10px] sm:text-xs font-bold">
                                                {doc.entityName}
                                            </Badge>
                                        )}
                                        {isFulfilled && (
                                            <Badge variant="success" className="text-[10px] sm:text-xs font-bold">
                                                مستلمة مسبقاً
                                            </Badge>
                                        )}
                                    </div>
                                </div>

                                <div className="w-full md:w-64">
                                    {isFulfilled ? (
                                        <div className="h-[60px] flex items-center justify-center rounded-lg border border-dashed border-success/30 bg-success/10 text-success font-bold text-sm ">
                                            تم التسليم
                                        </div>
                                    ) : (
                                        <FileField
                                            name={`documents.${index}.file`}
                                            control={control}
                                            label=""
                                            hint="اسحب الملف هنا"
                                            size="sm"
                                            className="min-h-[60px]"
                                        />
                                    )}
                                </div>
                            </div>
                        );
                    })}
                </div>

                {errors.documents?.root && (
                    <p className="text-xs font-bold text-destructive px-2">
                        {errors.documents.root.message}
                    </p>
                )}

                <NoteBox type="warning">
                    سيتم تحويل حالة الوثائق المرفوعة فقط إلى "مستلمة". لن يتم إغلاق التعهد بالكامل إلا عند رفع كافة الوثائق المطلوبة.
                </NoteBox>
            </div>
        </BaseFormDialog>
    );
}
