import { z } from "zod";
import { yemenPhoneSchema } from "./common";

/**
 * Schema موحد لتحقق من نماذج ملف التقاعد
 * رسائل خطأ عربية واضحة
 */

export const retirementFileSchema = z.object({
    retireeID: z.number().min(1, "يجب اختيار المتقاعد"),
    nationalID: z
        .string()
        .min(1, "الرقم الوطني مطلوب")
        .regex(/^\d{12}$/, "يجب أن يتكون الرقم الوطني من 12 أرقام"),
    gender: z.string().min(1, "الجنس مطلوب"),
    birthDate: z.string().optional(),
    socialStatusID: z.string().optional(),
    healthStatusID: z.string().optional(),
    isAlive: z.boolean(),
    deathDate: z.string().optional(),
    governorate: z.string().optional(),
    city: z.string().optional(),
    address: z.string().optional(),
    phone: yemenPhoneSchema.optional(),
    personalPhoto: z
        .instanceof(File, { message: "الصورة الشخصية مطلوبة" })
        .nullable()
        .refine((file) => file !== null, { message: "الصورة الشخصية مطلوبة" }),
    idDocument: z.instanceof(File).nullable().optional(),
    deathCertificate: z.instanceof(File).nullable().optional(),
    notes: z.string().optional(),
}).superRefine((data, ctx) => {
    if (!data.isAlive && !data.deathDate) {
        ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: "تاريخ الوفاة مطلوب إذا كان المتقاعد متوفياً",
            path: ["deathDate"],
        });
    }
    if (data.isAlive && !data.idDocument) {
        ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: "صورة الهوية مطلوبة إذا كان المتقاعد على قيد الحياة",
            path: ["idDocument"],
        });
    }
    if (!data.isAlive && !data.deathCertificate) {
        ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: "شهادة الوفاة مطلوبة إذا كان المتقاعد متوفياً",
            path: ["deathCertificate"],
        });
    }
});

export type RetirementFileFormData = z.infer<typeof retirementFileSchema>;

/**
 * Schema للخطوة الأولى (اختيار المتقاعد)
 */
export const selectRetireeSchema = z.object({
    retireeID: z.number().min(1, "يجب اختيار المتقاعد"),
});

/**
 * Schema للخطوة الثانية (البيانات الشخصية)
 */
export const personalDataSchema = z.object({
    nationalID: z
        .string()
        .min(1, "الرقم الوطني مطلوب")
        .regex(/^\d{12}$/, "يجب أن يتكون الرقم الوطني من 12 أرقام"),
    gender: z.string().min(1, "الجنس مطلوب"),
    birthDate: z.string().optional(),
    socialStatusID: z.string().optional(),
    healthStatusID: z.string().optional(),
    isAlive: z.boolean(),
    deathDate: z.string().optional(),
}).superRefine((data, ctx) => {
    if (!data.isAlive && !data.deathDate) {
        ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: "تاريخ الوفاة مطلوب إذا كان المتقاعد متوفياً",
            path: ["deathDate"],
        });
    }
});

/**
 * Schema للخطوة الثالثة (العنوان والاتصال)
 */
export const contactDataSchema = z.object({
    governorate: z.string().optional(),
    city: z.string().optional(),
    address: z.string().optional(),
    phone: yemenPhoneSchema.optional(),
});

/**
 * Schema للخطوة الرابعة (الوثائق)
 */
export const documentsSchema = z
    .object({
        personalPhoto: z
            .instanceof(File, { message: "الصورة الشخصية مطلوبة" })
            .nullable()
            .refine((file) => file !== null, { message: "الصورة الشخصية مطلوبة" }),
        isAlive: z.boolean(),
        idDocument: z.instanceof(File).nullable().optional(),
        deathCertificate: z.instanceof(File).nullable().optional(),
    })
    .refine(
        (data) => {
            if (data.isAlive) {
                return data.idDocument !== null;
            } else {
                return data.deathCertificate !== null;
            }
        },
        {
            message: "يجب رفع صورة الهوية للمتقاعد الحي أو شهادة الوفاة للمتوفي",
            path: ["idDocument"],
        }
    );
