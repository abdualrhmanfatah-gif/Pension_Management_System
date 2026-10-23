import { useQuery } from "@tanstack/react-query";
import { Percent } from "lucide-react";
import { useId } from "react";
import type { Ref } from "react";

import { retirementApi } from "@/api/retirement";
import { FormField, Input } from "@/components/ui";

interface EntitlementAmountFieldProps {
    retireeId?: number;
    value?: string;
    onChange: (value: string) => void;
    onBlur: () => void;
    inputRef?: Ref<HTMLInputElement>;
    savedSharePercent?: number | null;
    error?: string;
}

const numberFormat = new Intl.NumberFormat("ar-YE", { maximumFractionDigits: 2 });

export function EntitlementAmountField({ retireeId, value, onChange, onBlur, inputRef, savedSharePercent, error }: EntitlementAmountFieldProps) {
    const id = useId();
    const { data, isLoading, isError, refetch } = useQuery({
        queryKey: ["financial-entitlements", retireeId],
        queryFn: () => retirementApi.getFinancialEntitlements(retireeId!),
        enabled: !!retireeId,
    });
    // The API returns the latest entitlement first, matching amount conversion on save.
    const pensionAmount = data?.find(item => item.entitlementID > 0)?.amount;
    const hasPension = pensionAmount != null && pensionAmount > 0;
    const savedAmount = hasPension && savedSharePercent != null
        ? (pensionAmount * savedSharePercent / 100).toFixed(2) : "";
    const displayedValue = value ?? savedAmount;
    const amount = Number(displayedValue);
    const percent = hasPension && displayedValue !== "" && Number.isFinite(amount) && amount > 0
        ? amount / pensionAmount * 100 : null;
    const exceedsPension = hasPension && amount > pensionAmount;

    return (
        <div className="rounded-xl border border-border bg-muted/20 p-4 space-y-3">
            <div className="grid grid-cols-[minmax(0,1fr)_minmax(96px,0.55fr)] gap-3 items-start">
                <FormField label="مبلغ الاستحقاق الشهري (ريال)" htmlFor={id} error={error || (exceedsPension ? "المبلغ يتجاوز إجمالي المعاش" : undefined)}>
                    <Input id={id} ref={inputRef} type="number" inputMode="decimal" min="0.01" max={hasPension ? pensionAmount : undefined} step="0.01"
                        value={displayedValue} onChange={event => onChange(event.target.value)} onBlur={onBlur}
                        disabled={!hasPension || isError} placeholder="0.00" dir="ltr" className="h-11 sm:h-10 text-base font-semibold tabular-nums text-start" />
                </FormField>
                <div className="space-y-1">
                    <span id={`${id}-percentage-label`} className="label-field flex items-center gap-1.5"><Percent className="h-4 w-4" aria-hidden="true" />النسبة المحسوبة</span>
                    <output aria-labelledby={`${id}-percentage-label`} aria-live="polite" className={`flex h-11 sm:h-10 items-center justify-center rounded-md border text-base font-bold tabular-nums ${exceedsPension ? "border-destructive bg-destructive/10 text-destructive" : "border-primary bg-primary/5 text-primary"}`}>
                        {percent == null ? "—" : `${numberFormat.format(percent)}٪`}
                    </output>
                </div>
            </div>
            <p className="text-xs text-muted-foreground">
                {isLoading ? "جارٍ تحميل إجمالي المعاش…" : isError ? "تعذر تحميل إجمالي المعاش." : hasPension
                    ? `من إجمالي معاش ${numberFormat.format(pensionAmount)} ريال. تُحسب النسبة تلقائياً عند الحفظ.`
                    : "يجب تسجيل الاستحقاق المالي للمتقاعد أولاً لحساب حصة المستحق."}
            </p>
            {savedSharePercent != null && <p className="text-xs text-muted-foreground">الحصة المحفوظة: {numberFormat.format(savedSharePercent)}٪. ترك المبلغ فارغاً يحافظ عليها.</p>}
            {isError && <button type="button" onClick={() => void refetch()} className="min-h-11 text-sm text-primary underline">إعادة المحاولة</button>}
        </div>
    );
}
