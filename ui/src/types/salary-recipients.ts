import { z } from "zod";

export const createSalaryRecipientSchema = z.object({
    recipientType: z.enum(["متقاعد", "وكيل", "مستحق", "وصي"], {
        message: "نوع المستلم مطلوب",
    }),
    retireeID: z.number().optional(),
    agentID: z.number().optional(),
    entitledID: z.number().optional(),
    guardianID: z.number().optional(),
    startDate: z.string().min(1, "تاريخ البدء مطلوب"),
    notes: z.string().optional(),
    agentDocument: z.instanceof(File).optional(),
    personalPhoto: z.instanceof(File).optional(),
    nationalIdImage: z.instanceof(File).optional(),
    // Data completion fields
    birthDate: z.string().optional(),
    nationalID: z.string().optional(),
    socialStatusID: z.number().optional(),
    healthStatusID: z.number().optional(),
    entitledAmount: z.string().optional().refine(value => !value || Number(value) > 0, "أدخل مبلغاً أكبر من صفر"),
}).refine(data => {
    return data.retireeID || data.agentID || data.entitledID || data.guardianID;
}, {
    message: "يجب اختيار مستلم",
    path: ["recipientType"],
}).refine(data => data.recipientType !== "وصي" || !!data.entitledID, {
    message: "اختر المستحق الذي ينوب عنه الوصي", path: ["entitledID"],
});

export const endSalaryRecipientSchema = z.object({
    endDate: z.string().min(1, "تاريخ الانتهاء مطلوب"),
    notes: z.string().optional(),
});

export type CreateSalaryRecipientForm = z.infer<typeof createSalaryRecipientSchema>;
export type EndSalaryRecipientForm = z.infer<typeof endSalaryRecipientSchema>;
