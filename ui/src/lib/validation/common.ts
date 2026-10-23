import { z } from "zod";

// هاتف يمني: موبايل (7 + 8 أرقام = 9) أو أرضي (0 + 7 أرقام = 8)
export const YEMEN_PHONE_REGEX = /^(7\d{8}|0\d{7})$/;

export const PHONE_ERROR_MESSAGE = "رقم الهاتف غير صحيح (موبايل: 7XXXXXXXX / أرضي: 0XXXXXXX)";

export const yemenPhoneSchema = z.string()
  .trim()
  .min(8, "رقم الهاتف يجب أن يكون 8 أو 9 أرقام")
  .max(9, "رقم الهاتف يجب أن يكون 8 أو 9 أرقام")
  .regex(YEMEN_PHONE_REGEX, PHONE_ERROR_MESSAGE);

export const nationalIdSchema = z.string()
  .trim()
  .length(12, "الرقم الوطني يجب أن يتكون من 12 رقماً")
  .regex(/^\d+$/, "الرقم الوطني يجب أن يحتوي على أرقام فقط");

export const emailSchema = z
  .string()
  .email("صيغة البريد الإلكتروني غير صحيحة")
  .optional()
  .nullable()
  .or(z.literal(""));

export function requiredString(message: string) {
  return z.string().min(1, message);
}

export function requiredNumber(message: string) {
  return z.number().min(1, message);
}

export function requiredFile(message: string) {
  return z.instanceof(File, { message }).nullable().refine((f) => f !== null, { message });
}
