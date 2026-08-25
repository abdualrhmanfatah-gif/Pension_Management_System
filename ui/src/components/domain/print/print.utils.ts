import { retirementApi } from "@/api/retirement";
import { salaryRecipientApi } from "@/api/salary-recipients";

import type { FileDetailsForPrint,NormalizableItem } from "./print.types";

/**
 * دالة موحدة لتوحيد مسميات الحقول القادمة من API مختلف (Casing normalization)
 * تستخدم خصيصاً في مباينات الطباعة لضمان الوصول الصحيح للصور والمعرفات
 */
export const normalizePrintItem = (item: NormalizableItem) => {
    if (!item) return null;
    
    // استخراج مسار الصورة من عدة احتمالات مسميات (PascalCase vs camelCase)
    const photoPath = (item.personalPhotoPath ??
        item.PersonalPhotoPath ??
        item.personalPhoto ??
        item.PersonalPhoto ??
        item.PhotoPath ??
        item.photoPath ??
        item.path ??
        item.filePath ??
        item.FilePath) as string | undefined;

    return {
        ...item,
        // توحيد المعرفات
        agentId: (item.agentId ?? item.agentID ?? item.AgentID) as number | undefined,
        guardianId: (item.guardianId ?? item.guardianID ?? item.GuardianID) as number | undefined,
        entitledId: (item.entitledId ?? item.entitledID ?? item.EntitledID) as number | undefined,
        recipientId: (item.recipientId ?? item.recipientID ?? item.RecipientID) as number | undefined,
        
        // توحيد النوع
        recipientType: (item.recipientType ?? item.RecipientType) as string | undefined,

        // توحيد الحالة النشطة
        isActive: item.isActive === true || item.IsActive === true || item.isActive === 1 || item.IsActive === 1,
        
        // توحيد النصوص
        fullName: (item.fullName ?? item.FullName) as string,
        recipientName: (item.recipientName ?? item.RecipientName) as string,
        
        // الحقل الموحد للصورة
        personalPhotoPath: photoPath,
    };
};

/**
 * دالة موحدة لتنسيق العملة في صفحات الطباعة
 */
export const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('ar-YE', {
        style: 'currency',
        currency: 'YER',
        maximumFractionDigits: 0
    }).format(amount).replace('ر.ي.‏', '').trim() + ' ر.ي';
};

/**
 * ثوابت التصميم المشتركة لجداول الطباعة
 */
export const PRINT_STYLES = {
    TB: "border border-black",
    CELL: "px-3 py-2 text-[12px] leading-snug",
    LBL: "px-3 py-2 text-[12px] leading-snug bg-slate-100 font-bold text-black whitespace-nowrap",
    VAL: "px-3 py-2 text-[12px] leading-snug font-semibold text-black",
    SECTION_HEADER: "border border-black bg-slate-200 font-extrabold text-[13px] py-2 px-3 flex items-center gap-2",
};

/**
 * دالة موحدة لجلب تفاصيل ملف التقاعد وملحقاته للطباعة
 */
export const fetchFileDetails = async (fileId: number): Promise<FileDetailsForPrint | null> => {
    if (!fileId) return null;

    const [fileRes, entitledRes, agentsRes, guardiansRes, recipientsRes] = await Promise.all([
        retirementApi.getFile(fileId),
        retirementApi.getEntitledPersons(fileId).catch(() => []),
        retirementApi.getAgents(fileId).catch(() => []),
        retirementApi.getGuardians(fileId).catch(() => []),
        salaryRecipientApi.getByFile(fileId).catch(() => ({ data: [] })),
    ]);

    const fileData = (fileRes as unknown as Record<string, unknown> || {}) as Record<string, unknown>;
    
    return {
        file: {
            activePaymentBookNumber: (fileData.activePaymentBookNumber ?? fileData.ActivePaymentBookNumber ?? null) as string | null,
            personalPhotoPath: (fileData.personalPhotoPath ?? fileData.PersonalPhotoPath ?? fileData.PersonalPhoto ?? null) as string | null,
        },
        entitled: (entitledRes || []).map(normalizePrintItem as any).filter((e: any) => e && (e.isActive === true || e.IsActive === true)),
        agents: (agentsRes || []).map(normalizePrintItem as any),
        guardians: (guardiansRes || []).map(normalizePrintItem as any),
        recipients: (recipientsRes.data || []).map(normalizePrintItem as any),
    } as FileDetailsForPrint;
};

/**
 * الحصول على رابط الصور الموحد
 */
export const getPrintBaseUrl = () => import.meta.env.VITE_API_URL?.replace("/api", "").replace(/\/?$/, "/") || "";
