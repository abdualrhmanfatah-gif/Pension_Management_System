import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Calendar, CheckCircle, PlayCircle, XCircle } from "lucide-react";
import { useCallback, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import * as z from "zod";

import { fiscalYearsApi } from "@/api/accounting";
import type { FiscalYearSummaryDto } from "@/types/accounting";
import { queryKeys } from "@/constants/queryKeys";
import { fixedAssetApi } from "@/api/fixedAssets";
import {
    BaseFormDialog,
    Badge,
    FormField,
    FormSection,
    NativeSelect,
    NoteBox,
    Text
} from "@/components/ui";
import { useApiMutation } from "@/hooks";

// ─── Validation Schema ─────────────────────────────────

const schema = z.object({
    fiscalYearID: z.number().min(1, "اختر السنة المالية"),
});

type DepreciationRunFormValues = z.infer<typeof schema>;

// ─── Types ─────────────────────────────────────────────

interface StepResult {
    step: string;
    success: boolean;
    message: string;
}

// ─── Props ─────────────────────────────────────────────

interface DepreciationRunDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSuccess?: () => void;
}

// ─── Component ─────────────────────────────────────────

export function DepreciationRunDialog({
    open,
    onOpenChange,
    onSuccess: _onSuccess,
}: DepreciationRunDialogProps) {
    const [stepResults, setStepResults] = useState<StepResult[]>([]);

    // جلب السنوات المالية من الـ API
    const { data: fiscalYears = [], isLoading: loadingYears } = useQuery({
        queryKey: queryKeys.accounting.fiscalYears,
        queryFn: () => fiscalYearsApi.getAll(),
        enabled: open,
    });

    // خيارات السنوات المالية
    const yearOptions = useMemo(() =>
        fiscalYears.map((y: FiscalYearSummaryDto) => ({
            value: String(y.fiscalYearID),
            label: y.yearName,
        })),
        [fiscalYears]
    );

    // إيجاد أقرب سنة مالية نشطة
    const defaultYearID = useMemo(() => {
        if (fiscalYears.length === 0) return 0;
        const active = fiscalYears.find((y: FiscalYearSummaryDto) => !y.isClosed);
        return active?.fiscalYearID || fiscalYears[0].fiscalYearID;
    }, [fiscalYears]);

    const form = useForm<DepreciationRunFormValues>({
        resolver: zodResolver(schema),
        defaultValues: useMemo(() => ({
            fiscalYearID: defaultYearID,
        }), [defaultYearID]),
    });

    const { reset, formState: { errors, isDirty } } = form;

    const mutation = useApiMutation(
        async (values: DepreciationRunFormValues) => {
            setStepResults([]);

            const selectedYear = fiscalYears.find((y: FiscalYearSummaryDto) => y.fiscalYearID === values.fiscalYearID);
            if (!selectedYear) throw new Error("السنة المالية غير موجودة");

            const year = new Date(selectedYear.startDate).getFullYear();
            const results: StepResult[] = [];

            // الخطوة 1: حساب الإهلاك السنوي
            const calcResult = await fixedAssetApi.runYearlyDepreciation(year);
            results.push({
                step: `حساب الإهلاك السنوي - ${selectedYear.yearName}`,
                success: calcResult?.isSuccess ?? false,
                message: calcResult?.message || "فشل في حساب الإهلاك",
            });
            setStepResults([...results]);

            // الخطوة 2: ترحيل الإهلاك إلى الدليل المحاسبي
            const postResult = await fixedAssetApi.postYearlyDepreciation(year);
            results.push({
                step: `ترحيل الإهلاك السنوي - ${selectedYear.yearName}`,
                success: postResult?.isSuccess ?? false,
                message: postResult?.message || "فشل في ترحيل الإهلاك",
            });
            setStepResults([...results]);

            // إذا فشلت أي خطوة
            const failedStep = results.find(r => !r.success);
            if (failedStep) {
                throw new Error(failedStep.message);
            }

            return { message: `تم حساب وترحيل الإهلاك السنوي بنجاح - ${selectedYear.yearName}` };
        },
        {
            disableSuccessToast: true,
            invalidateKeys: [["fixed-assets"], ["depreciation-schedule"], ["accounting"]],
        }
    );

    const handleFormSubmit = useCallback((values: DepreciationRunFormValues) => {
        mutation.mutate(values);
    }, [mutation]);

    const hasResults = stepResults.length > 0;
    const allSuccess = hasResults && stepResults.every(r => r.success);

    return (
        <BaseFormDialog
            open={open}
            onOpenChange={onOpenChange}
            title="الإهلاك السنوي"
            description="حساب إهلاك جميع الأصول النشطة للسنة المالية المحددة وترحيله إلى الدليل المحاسبي (قيد واحد لكل حساب مصروف)"
            size="md"
            isLoading={mutation.isPending}
            isDirty={isDirty}
            onSubmit={form.handleSubmit(handleFormSubmit)}
            resetOnOpen={true}
            reset={reset}
            initialValues={{ fiscalYearID: defaultYearID }}
            showGlobalError
            formErrors={errors}
            submitLabel={hasResults && allSuccess ? "تم بنجاح" : "تشغيل الإهلاك السنوي"}
            submitIcon={hasResults && allSuccess ? <CheckCircle className="w-4 h-4" /> : <PlayCircle className="w-4 h-4" />}
        >
            <div className="space-y-4 text-start">
                <FormSection title="الفترة المحاسبية" icon={Calendar}>
                    <FormField label="السنة المالية" required error={errors.fiscalYearID?.message}>
                        <NativeSelect
                            options={yearOptions}
                            value={String(form.watch("fiscalYearID"))}
                            onChange={(v) => {
                                form.setValue("fiscalYearID", Number(v), { shouldValidate: true });
                            }}
                            placeholder={loadingYears ? "جاري التحميل..." : "اختر السنة..."}
                            disabled={loadingYears}
                        />
                    </FormField>
                </FormSection>

                <NoteBox type="warning">
                    سيتم حساب إهلاك جميع الأصول النشطة لـ 12 شهر وترحيله كقيد محاسبي واحد لكل حساب مصروف.
                    تأكد من صحة السنة المالية قبل التشغيل.
                </NoteBox>

                {/* عرض نتائج التنفيذ */}
                {hasResults && (
                    <div className="mt-4 space-y-2">
                        <Text variant="body" className="font-bold text-foreground">نتائج التنفيذ:</Text>
                        {stepResults.map((result, index) => (
                            <div
                                key={index}
                                className={`flex items-start gap-2 p-3 rounded-lg border ${
                                    result.success
                                        ? "bg-green-50 dark:bg-green-950/20 border-green-200 dark:border-green-800"
                                        : "bg-red-50 dark:bg-red-950/20 border-red-200 dark:border-red-800"
                                }`}
                            >
                                {result.success ? (
                                    <CheckCircle className="w-5 h-5 text-green-600 dark:text-green-400 mt-0.5 shrink-0" />
                                ) : (
                                    <XCircle className="w-5 h-5 text-red-600 dark:text-red-400 mt-0.5 shrink-0" />
                                )}
                                <div className="flex-1 min-w-0">
                                    <div className="flex items-center gap-2">
                                        <span className="text-sm font-semibold text-foreground">{result.step}</span>
                                        <Badge variant={result.success ? "success" : "destructive"} className="text-xs">
                                            {result.success ? "نجح" : "فشل"}
                                        </Badge>
                                    </div>
                                    <Text variant="body" className={`mt-1 whitespace-pre-line ${
                                        result.success
                                            ? "text-green-700 dark:text-green-300"
                                            : "text-red-700 dark:text-red-300"
                                    }`}>
                                        {result.message}
                                    </Text>
                                </div>
                            </div>
                        ))}

                        {allSuccess && (
                            <div className="flex items-center gap-2 p-3 rounded-lg bg-green-50 dark:bg-green-950/20 border border-green-200 dark:border-green-800">
                                <CheckCircle className="w-5 h-5 text-green-600 dark:text-green-400" />
                                <Text variant="body" className="font-semibold text-green-700 dark:text-green-300">
                                    تم بنجاح! يمكنك إغلاق النموذج الآن.
                                </Text>
                            </div>
                        )}
                    </div>
                )}
            </div>
        </BaseFormDialog>
    );
}
