import { z } from "zod";

export const createEntitlementSchema = z.object({
    amount: z.number().min(1, "المبلغ يجب أن يكون أكبر من 0"),
    effectiveDate: z.string().min(1, "تاريخ السريان مطلوب"),
    source: z.string().min(1, "المصدر مطلوب"),
    notes: z.string().optional(),
});

export const endEntitlementSchema = z.object({
    endDate: z.string().min(1, "تاريخ الانتهاء مطلوب"),
});

export type CreateEntitlementForm = z.infer<typeof createEntitlementSchema>;
export type EndEntitlementForm = z.infer<typeof endEntitlementSchema>;
