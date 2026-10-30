# خطة توحيد آلية جلب الوثائق والصور — الشاملة النهائية

**التاريخ:** 2026-08-20
**الحالة:** جاهزة للتنفيذ
**المدة المقدرة:** 14-18 ساعة عمل
**عدد الملفات المتغيرة:** 74 ملف (جديد + تعديل)
**عدد الملفات المؤثرة:** 100+ ملف

---

## فهرس المحتويات

1. [المشكلة الأساسية](#1-المشكلة-الأساسية)
2. [النطاق والقرارات](#2-النطاق-والقرارات)
3. [المرحلة 1: Backend — JWT File Serving](#3-المرحلة-1-backend--jwt-file-serving)
4. [المرحلة 2: Frontend — API + Utility موحّدة](#4-المرحلة-2-frontend--api--utility-موحّدة)
5. [المرحلة 3: Frontend — Hooks موحّدة](#5-المرحلة-3-frontend--hooks-موحّدة)
6. [المرحلة 4: Frontend — تحديث المكونات (Tier 1)](#6-المرحلة-4-frontend--تحديث-المكونات-tier-1)
7. [المرحلة 5: Frontend — تحديث المكونات (Tier 2)](#7-المرحلة-5-frontend--تحديث-المكونات-tier-2)
8. [المرحلة 6: Frontend — تحديث المكونات (Tier 3)](#8-المرحلة-6-frontend--تحديث-المكونات-tier-3)
9. [المرحلة 7: Vite Config + Proxy](#9-المرحلة-7-vite-config--proxy)
10. [ملخص جميع الملفات](#10-ملخص-جميع-الملفات)
11. [أخطار ومصاعب متوقعة](#11-أخطار-ومصاعب-متوقعة)
12. [خطة الاختبار](#12-خطة-الاختبار)
13. [خطة التنفيذ المقترحة](#13-خطة-التنفيذ-المقترحة)

---

## 1. المشكلة الأساسية

### إحصائيات الكود الحالي

| المؤشر | العدد |
|--------|-------|
| ملفات تستدعي `getFileBaseUrl()` | **51** ملف |
| ملفات تستخدم `responseType: "blob"` | **26** ملف |
| استخدام `queryKey` متعلق بالوثائق | **42** استخدام |
| ملفات تستخدم `getPhotoUrl()` | **49** استخدام |
| ملفات تستورد `retirementApi` | **40** ملف |
| ملفات تستورد `DocumentItem` | **10** ملفات |
| ملفات تستورد `DocumentViewerDialog` | **4** ملفات |
| ملفات تستدعي `api.get` مع مسارات وثائق | **56** استخدام |
| ملفات تستدعي `api.post/put/delete` مع مسارات وثائق | **15** استخدام |

### المشاكل المحددة

| # | المشكلة | الخطورة | الملفات المتضررة |
|---|---------|---------|-----------------|
| 1 | **`getFileBaseUrl()` مكرر في 51 ملف** — كل مكون يعيد بناء الرابط بنفس الطريقة | 🔴 عالية | 51 ملف |
| 2 | **3 استراتيجيات عرض مختلفة** — blob authenticated + direct URL + local blob | 🔴 عالية | `DocumentsSection`, `DocumentViewerDialog`, `BulkUploadDialog` |
| 3 | **Static files بدون auth** — `UseStaticFiles` في `Program.cs:695` يخدم uploads بدون JWT | 🔴 عالية | `Program.cs` |
| 4 | **`DocumentItem` type مكرر** — معرّف في `types/retirement.ts:348` و `DocumentViewerDialog.tsx:16` | 🟡 متوسطة | ملفان |
| 5 | **File entity fetch مكرر** — `UploadDocumentDialog:82` + `BulkUploadDialog:241` بنفس المنطق | 🟡 متوسطة | ملفان |
| 6 | **`useQuery` مكرر بنفس key** — `["documents", fileId]" في 3+ أماكن مختلفة | 🟡 متوسطة | 3+ ملفات |
| 7 | **API functions موزّعة** — وثائق في `retirement.ts` + `archive.ts` + `pension.ts` + `hr.ts` + `investment.ts` | 🟡 متوسطة | 5 ملفات API |
| 8 | **`usePersonDevicesQuery` غير مصدر** — معرّف في `usePersonDocumentsQuery.ts` لكن غير في `queries/index.ts` | 🟡 متوسطة | ملفان |
| 9 | **CORS مفتوح** — `Access-Control-Allow-Origin: *` على static files | 🟡 متوسطة | `Program.cs` |
| 10 | **Magic bytes validation مفقودة في photo endpoints** — services الصور لا تتحقق من magic bytes | 🟡 متوسطة | RetireeService, HrEmployeeService, etc. |

---

## 2. النطاق والقرارات

### القرارات المتخذة

| القرار | الاختيار |
|--------|---------|
| **النطاق** | Frontend + Backend |
| **المصادقة** | كل الملفات تحتاج JWT (أكثر أماناً) |
| **إزالة الكود** | لا — الكود الجديد يضاف بجانب القديم (coexistence) |
| **صور الأشخاص** | الاحتفاظ بـ endpoints موجودة (Retiree, Agent, Guardian...) |
| **المجلدات** | لا تتغير — كل entity يحفظ في مجلده الحالي |

---

## 3. المرحلة 1: Backend — JWT File Serving

### الهدف
استبدال `UseStaticFiles` المفتوح بنقطة نهاية موثوقة تتحقق من JWT.

### 3.1. ملف جديد: `PensionMS.API/Controllers/FilesController.cs`

**المسار:** `F:\Pension_Management_System\PensionMS.API\Controllers\FilesController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace PensionMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly string _uploadsPath;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;

    public FilesController(IWebHostEnvironment env)
    {
        _env = env;
        _uploadsPath = Path.Combine(env.WebRootPath, "uploads");
        _contentTypeProvider = new FileExtensionContentTypeProvider();
    }

    /// <summary>
    /// جلب ملف من uploads — يدعم JWT + static tokens
    /// </summary>
    [HttpGet("{*filePath}")]
    public IActionResult GetFile(string filePath)
    {
        // 1. تنظيف المسار — منع path traversal
        var sanitizedPath = filePath.Replace('/', '\\');
        var fullPath = Path.GetFullPath(Path.Combine(_uploadsPath, sanitizedPath));

        if (!fullPath.StartsWith(_uploadsPath))
            return BadRequest("Invalid path");

        // 2. التحقق من الوجود
        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        // 3. تحديد Content-Type
        if (!_contentTypeProvider.TryGetContentType(fullPath, out var contentType))
            contentType = "application/octet-stream";

        // 4. Cache headers
        Response.Headers["Cache-Control"] = "private, max-age=86400";
        Response.Headers["X-Content-Type-Options"] = "nosniff";

        // 5. إرجاع الملف
        return PhysicalFile(fullPath, contentType);
    }
}
```

### 3.2. تعديل: `PensionMS.API/Program.cs`

**الملف:** `F:\Pension_Management_System\PensionMS.API\Program.cs`

**السطور 687-702:**

```csharp
// قبل:
var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var uploadsPath = Path.Combine(wwwrootPath, "uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(wwwrootPath),
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    }
});

// بعد:
var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var uploadsPath = Path.Combine(wwwrootPath, "uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Static files — فقط لـ /avatars و ملفات app أخرى (بدون uploads)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(wwwrootPath)),  // wwwroot root — لكن نستبعد uploads
    OnPrepareResponse = ctx =>
    {
        // فقط avatars بدون auth — الباقي عبر FilesController
        var path = ctx.Context.Request.Path.Value ?? "";
        if (!path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        }
    },
    // نحتاج RequestPath فارغ لكن نمنع uploads
    // الحل: ن移除 UseStaticFiles للـ uploads ونستخدم FilesController فقط
});

// ⚠️ ملاحظة: الأفضل تعطيل UseStaticFiles بالكامل وtnumber uploads
// عبر FilesController. لكن للحفاظ على التوافق مع /avatars/
// نحتفظ بـ UseStaticFiles لكن نمنع uploads بشكل صريح.
```

**ملاحظة تنفيذية:** الطريقة الأبسط هي تغيير `UseStaticFiles` ليجمع من `wwwroot` لكن نمنع `uploads/` عبر `OnPrepareResponse`:

```csharp
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(wwwrootPath),
    OnPrepareResponse = ctx =>
    {
        var path = ctx.Context.Request.Path.Value ?? "";
        if (path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            // منع الوصول المباشر — يُ handled عبر FilesController
            ctx.Context.Response.StatusCode = 404;
            ctx.Context.Response.Body.Close();
            return;
        }
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    }
});
```

### 3.3. ملفات Backend لا تتغير

| الملف | السبب |
|-------|-------|
| `PensionMS.Core/Interfaces/IFileStorageService.cs` | واجهة موجودة ✅ |
| `PensionMS.Core/Interfaces/IDocumentService.cs` | موجود ✅ |
| `PensionMS.Core/Interfaces/IRemoteFile.cs` | موجود ✅ |
| `PensionMS.Core/Entities/Document.cs` | لا يتغير |
| `PensionMS.Core/DTOs/Documents/UploadDto.cs` | لا يتغير |
| `PensionMS.Core/DTOs/Documents/DocumentDtos.cs` | لا يتغير |
| `PensionMS.Core/DTOs/Documents/DocumentTypeDtos.cs` | لا يتغير |
| `PensionMS.Infrastructure/Services/DocumentService.cs` | لا يتغير |
| `PensionMS.Infrastructure/Services/LocalFileStorageService.cs` | لا يتغير |
| `PensionMS.Infrastructure/Helpers/ImageMergeHelper.cs` | لا يتغير |
| `PensionMS.API/Controllers/DocumentsController.cs` | لا يتغير |
| `PensionMS.API/Controllers/DocumentTypesController.cs` | لا يتغير |
| `PensionMS.API/Helpers/RemoteFileWrapper.cs` | لا يتغير |
| `PensionMS.API/Helpers/RemoteFileModelBinder.cs` | لا يتغير |

---

## 4. المرحلة 2: Frontend — API + Utility موحّدة

### الهدف
إنشاء `api/documents.ts` موحّد + `getFileUrl()` utility واحدة.

### 4.1. ملف جديد: `ui/src/api/documents.ts`

**المسار:** `F:\Pension_Management_System\ui\src\api\documents.ts`

```typescript
import api from "./axios";
import type {
    DocumentItem,
    MissingDocumentInfo,
    MissingDocumentSummary,
    DocumentTypeLookup,
} from "@/types/retirement";

// ─── ApiResponse wrapper ──────────────────────
interface ApiResponse<T> {
    isSuccess: boolean;
    data: T;
    message?: string;
}

// ─── Document CRUD ────────────────────────────
export const documentsApi = {
    // ── جلب وثائق حسب ملف ──
    getByFileId(fileId: number): Promise<DocumentItem[]> {
        return api
            .get<ApiResponse<DocumentItem[]>>(`/documents/by-file/${fileId}`)
            .then((r) => r.data.data);
    },

    // ── جلب وثيقة بالمعرف ──
    getById(id: number): Promise<DocumentItem> {
        return api
            .get<ApiResponse<DocumentItem>>(`/documents/${id}`)
            .then((r) => r.data.data);
    },

    // ── حذف وثيقة ──
    delete(id: number): Promise<boolean> {
        return api
            .delete<ApiResponse<boolean>>(`/documents/${id}`)
            .then((r) => r.data);
    },

    // ── رفع وثيقة ──
    upload(formData: FormData): Promise<DocumentItem> {
        return api
            .post<ApiResponse<DocumentItem>>("/documents", formData, {
                headers: { "Content-Type": "multipart/form-data" },
                timeout: 300000, // 5 دقائق
            })
            .then((r) => r.data.data);
    },

    // ── تعديل وثيقة ──
    update(id: number, formData: FormData): Promise<DocumentItem> {
        return api
            .put<ApiResponse<DocumentItem>>(`/documents/${id}`, formData, {
                headers: { "Content-Type": "multipart/form-data" },
            })
            .then((r) => r.data.data);
    },

    // ── مراجعة وثيقة ──
    review(
        id: number,
        dto: {
            pendingReviewStatus: number;
            notes?: string;
            rejectionReason?: string;
        }
    ): Promise<boolean> {
        return api
            .put<ApiResponse<boolean>>(`/documents/${id}/review`, dto)
            .then((r) => r.data);
    },

    // ── جلب أنواع الوثائق ──
    getTypes(): Promise<DocumentTypeLookup[]> {
        return api
            .get<ApiResponse<DocumentTypeLookup[]>>("/lookups/document-types")
            .then((r) => r.data.data);
    },

    // ── جلب أنواع وثائق حسب المالك ──
    getTypesByOwner(ownerType: string): Promise<DocumentTypeLookup[]> {
        return api
            .get<ApiResponse<{ documentTypeID: number; typeName: string }[]>>(
                `/document-types/by-owner/${ownerType}`
            )
            .then((r) =>
                r.data.data.map((t) => ({ id: t.documentTypeID, name: t.typeName }))
            );
    },

    // ── الوثائق الناقصة ──
    getMissingByFileId(fileId: number): Promise<MissingDocumentInfo[]> {
        return api
            .get<ApiResponse<MissingDocumentInfo[]>>(
                `/missing-documents/by-file/${fileId}`
            )
            .then((r) => r.data.data);
    },

    getAutoDetectedMissing(fileId: number): Promise<MissingDocumentInfo[]> {
        return api
            .get<ApiResponse<MissingDocumentInfo[]>>(
                `/missing-documents/auto-detected/by-file/${fileId}`
            )
            .then((r) => r.data.data);
    },

    fulfillMissing(id: number): Promise<boolean> {
        return api
            .put<ApiResponse<boolean>>(`/missing-documents/${id}/fulfill`)
            .then((r) => r.data);
    },

    createMissing(data: {
        fileID: number;
        documentTypeID: number;
        agentID?: number;
        guardianID?: number;
        entitledID?: number;
    }): Promise<number> {
        return api
            .post<ApiResponse<{ missingDocumentID: number }>>(
                "/missing-documents",
                data
            )
            .then((r) => r.data.data.missingDocumentID);
    },

    // ── إحصائيات الأرشيف ──
    getArchiveStats(): Promise<{
        totalDocuments: number;
        verifiedDocuments: number;
        pendingReview: number;
        expiredDocuments: number;
        filesWithMissingDocuments: number;
        totalMissingDocuments: number;
        pendingMissingDocuments: number;
        pledgedMissingDocuments: number;
    }> {
        return api
            .get<ApiResponse<{
                totalDocuments: number;
                verifiedDocuments: number;
                pendingReview: number;
                expiredDocuments: number;
                filesWithMissingDocuments: number;
                totalMissingDocuments: number;
                pendingMissingDocuments: number;
                pledgedMissingDocuments: number;
            }>>("/documents/archive-stats")
            .then((r) => r.data.data);
    },

    // ── جلب الملفات (أرشيف) ──
    getArchiveDocuments(filters: Record<string, unknown>): Promise<{
        items: DocumentItem[];
        totalCount: number;
        page: number;
        pageSize: number;
    }> {
        return api
            .get<ApiResponse<{
                items: DocumentItem[];
                totalCount: number;
                page: number;
                pageSize: number;
            }>>("/documents", { params: filters })
            .then((r) => r.data.data);
    },

    // ── مراجعة من الأرشيف ──
    getReviewDocuments(filters: Record<string, unknown>): Promise<{
        items: DocumentItem[];
        totalCount: number;
        page: number;
        pageSize: number;
    }> {
        return api
            .get<ApiResponse<{
                items: DocumentItem[];
                totalCount: number;
                page: number;
                pageSize: number;
            }>>("/documents", { params: filters })
            .then((r) => r.data.data);
    },
};
```

### 4.2. تعديل: `ui/src/api/axios.ts`

**الملف:** `F:\Pension_Management_System\ui\src\api\axios.ts`

**السطور 33-46:**

```typescript
// قبل:
export function getFileBaseUrl(): string {
    const override = import.meta.env.VITE_FILE_BASE_URL;
    if (override) return override.replace(/\/?$/, "/");
    try {
        const url = new URL(API_BASE_URL);
        return `${url.origin}/`;
    } catch {
        const base = API_BASE_URL.replace(/\/api\/?$/, "");
        if (!base) {
            return typeof window !== "undefined" ? `${window.location.origin}/` : "";
        }
        return base.replace(/\/?$/, "/");
    }
}

// بعد: إضافة getFileUrl بجانب getFileBaseUrl
export function getFileBaseUrl(): string {
    // ... (يبقى كما هو — legacy support)
}

/**
 * بناء رابط ملف موحّد — يمر عبر FilesController (JWT authenticated).
 * يحل محل 51 استخدام مكرر لـ getFileBaseUrl() + بناء الـ URL يدوياً.
 *
 * @param filePath - المسار النسبي (مثل "uploads/documents/abc.pdf")
 * @returns الرابط الكامل عبر /api/files/
 *
 * أمثلة:
 *   getFileUrl("uploads/documents/abc.pdf") → "/api/files/uploads/documents/abc.pdf"
 *   getFileUrl("/uploads/photos/xyz.jpg")   → "/api/files/uploads/photos/xyz.jpg"
 *   getFileUrl(null)                        → null
 *   getFileUrl("https://example.com/img.jpg") → "https://example.com/img.jpg"
 */
export function getFileUrl(filePath: string | null | undefined): string | null {
    if (!filePath) return null;
    // لا نغير الروابط المطلقة أو blob/data URLs
    if (/^(https?:|blob:|data:)/i.test(filePath)) return filePath;
    // تنظيف المسار
    const normalized = filePath
        .replace(/\\/g, "/")
        .replace(/^\/+/, "")
        .replace(/\/{2,}/g, "/");
    return `/api/files/${normalized}`;
}
```

### 4.3. تعديل: `ui/src/lib/utils.ts`

**الملف:** `F:\Pension_Management_System\ui\src\lib\utils.ts`

```typescript
// قبل:
import { getFileBaseUrl } from "@/api/axios";

export function getPhotoUrl(path?: string | null): string | null {
    if (!path) return null;
    if (/^(https?:|blob:|data:)/i.test(path)) return path;
    const normalized = path.replace(/\\/g, "/").replace(/^\/+/, "").replace(/\/{2,}/g, "/");
    return `${getFileBaseUrl()}${normalized}`;
}

// بعد:
import { getFileUrl } from "@/api/axios";

// getPhotoUrl — يبقى كما هو (legacy support) لكن يستخدم getFileUrl
export function getPhotoUrl(path?: string | null): string | null {
    if (!path) return null;
    if (/^(https?:|blob:|data:)/i.test(path)) return path;
    const normalized = path.replace(/\\/g, "/").replace(/^\/+/, "").replace(/\/{2,}/g, "/");
    return `/api/files/${normalized}`;
}

// resolvePersonalPhotoPath — لا يتغير
export function resolvePersonalPhotoPath(entity?: Record<string, any>): string | null {
    if (!entity) return null;
    return entity.personalPhotoPath ?? entity.PersonalPhotoPath ?? entity.personalPhoto ?? entity.PersonalPhoto ?? entity.photoPath ?? entity.PhotoPath ?? null;
}

// getDefaultDeceasedAvatar — لا يتغير
export function getDefaultDeceasedAvatar(gender?: number | null): string {
    return gender === 2 ? "/avatars/deceased-female.svg" : "/avatars/deceased-male.svg";
}
```

### 4.4. تعديل: `ui/src/api/retirement.ts`

**الملف:** `F:\Pension_Management_System\ui\src\api\retirement.ts`

**السطور 278-302 و 368-369:**

```typescript
// قبل:
import api from "./axios";
// ... (باقي الاستيرادات)

export const retirementApi = {
    // ... (باقي الدوال)

    // الوثائق والمستندات
    getDocuments: (fileId: number) =>
        api.get<ApiResponse<DocumentItem[]>>(`/documents/by-file/${fileId}`).then(r => r.data.data),

    deleteDocument: (id: number) =>
        api.delete<ApiResponse<boolean>>(`/documents/${id}`).then(r => r.data),

    getDocumentTypes: () =>
        api.get<ApiResponse<DocumentTypeLookup[]>>("/lookups/document-types").then(r => r.data.data),

    // الوثائق الناقصة
    getMissingDocuments: (fileId: number) =>
        api.get<ApiResponse<MissingDocumentInfo[]>>(`/missing-documents/by-file/${fileId}`).then(r => r.data.data),

    getAutoDetectedMissing: (fileId: number) =>
        api.get<ApiResponse<MissingDocumentInfo[]>>(`/missing-documents/auto-detected/by-file/${fileId}`).then(r => r.data.data),

    fulfillMissingDocument: (id: number) =>
        api.put<ApiResponse<boolean>>(`/missing-documents/${id}/fulfill`).then(r => r.data),

    // ... (باقي الدوال)

    createMissingDocument: (data: { fileID: number; documentTypeID: number; agentID?: number; guardianID?: number; entitledID?: number }) =>
        api.post<ApiResponse<{ missingDocumentID: number }>>("/missing-documents", data).then(r => r.data),
}

// بعد:
import api from "./axios";
import { documentsApi } from "./documents";
// ... (باقي الاستيرادات)

export const retirementApi = {
    // ... (باقي الدوال — لا تتغير)

    // ── الوثائق — الآن تُounce إلى documentsApi ──
    getDocuments: documentsApi.getByFileId,
    deleteDocument: (id: number) => documentsApi.delete(id).then(() => ({ isSuccess: true } as any)),
    getDocumentTypes: documentsApi.getTypes,
    getMissingDocuments: documentsApi.getMissingByFileId,
    getAutoDetectedMissing: documentsApi.getAutoDetectedMissing,
    fulfillMissingDocument: (id: number) => documentsApi.fulfillMissing(id).then(() => ({ isSuccess: true } as any)),
    createMissingDocument: (data: { fileID: number; documentTypeID: number; agentID?: number; guardianID?: number; entitledID?: number }) =>
        documentsApi.createMissing(data).then((id) => ({ data: { missingDocumentID: id }, isSuccess: true } as any)),

    // ... (باقي الدوال — لا تتغير)
}
```

### 4.5. تعديل: `ui/src/api/archive.ts`

**الملف:** `F:\Pension_Management_System\ui\src\api\archive.ts`

**السطور 78-143:**

```typescript
// قبل:
export const archiveApi = {
    getDocuments: (filters: ArchiveFilters) =>
        api.get<ApiResponse<PagedResult<ArchiveDocument>>>("/documents", { params: filters })...

    getDocument: (id: number) =>
        api.get<ApiResponse<ArchiveDocument>>(`/documents/${id}`)...

    getStats: () =>
        api.get<ApiResponse<{...}>>("/documents/archive-stats")...

    getDocumentTypes: () =>
        api.get<ApiResponse<{ documentTypeID: number; typeName: string }[]>>("/document-types")...

    getReviewDocuments: (filters: DocumentReviewFilters) =>
        api.get<ApiResponse<PagedResult<DocumentReviewItem>>>("/documents", { params: filters })...

    reviewDocument: (documentId: number, dto: {...}) =>
        api.put<ApiResponse<boolean>>(`/documents/${documentId}/review`, dto)...
}

// بعد:
import { documentsApi } from "./documents";

export const archiveApi = {
    getDocuments: documentsApi.getArchiveDocuments as any,
    getDocument: documentsApi.getById,
    getStats: documentsApi.getArchiveStats,
    getDocumentTypes: async () => {
        const types = await documentsApi.getTypes();
        return types.map((t) => ({ documentTypeId: t.id, typeName: t.name }));
    },
    getReviewDocuments: documentsApi.getReviewDocuments as any,
    reviewDocument: documentsApi.review,
    // ... (باقي الدوال لا يتغير)
}
```

### 4.6. تعديل: `ui/src/api/pension.ts`

**الملف:** `F:\Pension_Management_System\ui\src\api\pension.ts`

**السطور 131-141:**

```typescript
// قبل:
export const documentReviewApi = {
    getForDocumentUpload: (fileId: number) =>
        api.get<{ data: any[] }>(`/missing-documents/auto-detected/by-file/${fileId}`)...

    getUploadStats: () => ...

    getAutoDetected: (fileId: number) =>
        api.get<{ data: any[] }>(`/missing-documents/auto-detected/by-file/${fileId}`)...

    getManual: (fileId: number) =>
        api.get<{ data: any[] }>(`/missing-documents/by-file/${fileId}`)...

    fulfillDocument: (id: number) =>
        api.put<{ data: boolean }>(`/missing-documents/${id}/fulfill`)...
}

// بعد:
import { documentsApi } from "./documents";

export const documentReviewApi = {
    getForDocumentUpload: documentsApi.getAutoDetectedMissing as any,
    getUploadStats: async () => { /* يبقى كما هو */ },
    getAutoDetected: documentsApi.getAutoDetectedMissing as any,
    getManual: documentsApi.getMissingByFileId as any,
    fulfillDocument: async (id: number) => {
        await documentsApi.fulfillMissing(id);
        return { data: true };
    },
}
```

### 4.7. تعديل: `ui/src/api/hr.ts`

**الملف:** `F:\Pension_Management_System\ui\src\api\hr.ts`

**السطور 397-442:**

```typescript
// قبل:
export const hrApi = {
    // ... (باقي الدوال)

    getGlobalDocuments: (params: any) =>
        api.get<ApiResponse<PagedResult<HrGlobalDocumentDto>>>("/hr/HrEmployees/documents/global", { params })...

    getEmployeeDocuments: (employeeId: number) =>
        api.get<ApiResponse<HrDocumentDto[]>>(`/hr/HrEmployees/${employeeId}/documents`)...

    uploadDocument: (employeeId: number, formData: FormData) =>
        api.post<ApiResponse<HrDocumentDto>>(`/hr/HrEmployees/${employeeId}/documents`, formData, { headers: { "Content-Type": "multipart/form-data" } })...

    deleteDocument: (employeeId: number, documentId: number) =>
        api.delete<ApiResponse<boolean>>(`/hr/HrEmployees/${employeeId}/documents/${documentId}`)...
}

// بعد:
import { documentsApi } from "./documents";

export const hrApi = {
    // ... (باقي الدوال — لا يتغير)

    // ── الوثائق — يبقى كما هو لأن HR endpoints خاصة ──
    // (HR uses its own /hr/HrEmployees/documents/* endpoints — مختلف عن /api/documents)
    getGlobalDocuments: hrApi_original.getGlobalDocuments,
    getEmployeeDocuments: hrApi_original.getEmployeeDocuments,
    uploadDocument: hrApi_original.uploadDocument,
    deleteDocument: hrApi_original.deleteDocument,
}
```

**ملاحظة:** `hrApi` يستخدم endpoints مختلفة (`/hr/HrEmployees/documents/*`) — لا يمكن توحيد OCR مع `documentsApi` مباشرة. يُترك كما هو.

### 4.8. تعديل: `ui/src/api/correspondence.ts`

**الملف:** `F:\Pension_Management_System\ui\src\api\correspondence.ts`

**السطور 56-68:**

```typescript
// قبل:
export const correspondenceApi = {
    // ...
    getDocuments: (correspondenceId: number) =>
        api.get<ApiResponse<HrDocumentDto[]>>(`/Correspondence/${correspondenceId}/documents`)...

    uploadDocument: (correspondenceId: number, file: File) => ...

    deleteDocument: (correspondenceId: number, documentId: number) =>
        api.delete<ApiResponse<boolean>>(`/Correspondence/${correspondenceId}/documents/${documentId}`)...
}

// ملاحظة: Correspondence يستخدم endpoints مختلفة — يُترك كما هو
// (لا يمكن توحيد OCR لأن المسار مختلف: /Correspondence/{id}/documents)
```

### 4.9. تعديل: `ui/src/api/investment.ts`

**الملف:** `F:\Pension_Management_System\ui\src\api\investment.ts`

**السطور 529-532:**

```typescript
// قبل:
getDocumentTypes: () =>
    api.get<ApiResponse<{ documentTypeID: number; typeName: string }[]>>("/document-types")...

getDocumentTypesByOwner: (ownerType: number) =>
    api.get<ApiResponse<{ documentTypeID: number; typeName: string }[]>>(`/document-types/by-owner/${ownerType}`)...

getDocumentReadiness: (ownerType: number, entityId: number) =>
    api.get<ApiResponse<unknown>>(`/investment/documents/readiness/${ownerType}/${entityId}`)...

// بعد:
import { documentsApi } from "./documents";

// getDocumentTypes و getDocumentTypesByOwner يُounceان إلى documentsApi
// getDocumentReadiness يبقى كما هو (investment-specific endpoint)
```

### 4.10. تعديل: `ui/src/utils/missing-documents.ts`

**الملف:** `F:\Pension_Management_System\ui\src\utils\missing-documents.ts`

**السطر 39:**

```typescript
// قبل:
const res = await api.post<{ data: { missingDocumentID: number } }>(
    "/missing-documents", { fileID, documentTypeID, agentID, guardianID, entitledID }
);

// بعد:
import { documentsApi } from "@/api/documents";
const missingId = await documentsApi.createMissing({ fileID, documentTypeID, agentID, guardianID, entitledID });
```

---

## 5. المرحلة 3: Frontend — Hooks موحّدة

### الهدف
إنشاء 3 hooks جديدة + تصحيح exports.

### 5.1. ملف جديد: `ui/src/hooks/useDocumentBlob.ts`

**المسار:** `F:\Pension_Management_System\ui\src\hooks\useDocumentBlob.ts`

```typescript
import { useState, useEffect } from "react";
import api, { getFileUrl } from "@/api/axios";

/**
 * جلب ملف كـ blob URL مع التنظيف التلقائي.
 *
 * يحل محل:
 *   DocumentsSection.useDocumentBlob (سطر 20-61)
 *   DocumentActions.handleDownload (جزئياً)
 *
 * الميزة: يمر عبر JWT interceptor ( authenticated ).
 *
 * @param filePath - المسار النسبي للملف
 * @param options.enabled - هل يُفعل الجلب (default: true)
 * @returns { blobUrl, loading }
 *
 * مثال:
 *   const { blobUrl, loading } = useDocumentBlob(doc.filePath);
 *   return <img src={blobUrl!} />;
 */
export function useDocumentBlob(
    filePath: string | null | undefined,
    options?: { enabled?: boolean }
): { blobUrl: string | null; loading: boolean } {
    const [blobUrl, setBlobUrl] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (!filePath || options?.enabled === false) {
            setBlobUrl(null);
            return;
        }

        let cancelled = false;
        setLoading(true);

        const url = getFileUrl(filePath);
        if (!url) {
            setBlobUrl(null);
            setLoading(false);
            return;
        }

        api.get(url, { responseType: "blob" })
            .then((res) => {
                if (cancelled) return;
                const blob = new Blob([res.data], {
                    type: res.headers["content-type"] || "application/octet-stream",
                });
                const objectUrl = URL.createObjectURL(blob);
                setBlobUrl(objectUrl);
            })
            .catch(() => {
                if (!cancelled) setBlobUrl(null);
            })
            .finally(() => {
                if (!cancelled) setLoading(false);
            });

        return () => {
            cancelled = true;
            setBlobUrl((prev) => {
                if (prev) URL.revokeObjectURL(prev);
                return null;
            });
        };
    }, [filePath, options?.enabled]);

    return { blobUrl, loading };
}
```

### 5.2. ملف جديد: `ui/src/hooks/useDocumentsQuery.ts`

**المسار:** `F:\Pension_Management_System\ui\src\hooks\useDocumentsQuery.ts`

```typescript
import { useQuery } from "@tanstack/react-query";
import { documentsApi } from "@/api/documents";
import type { DocumentItem, MissingDocumentSummary, DocumentTypeLookup } from "@/types/retirement";

export const DOCUMENTS_QUERY_KEY = "documents";

/**
 * جلب الوثائق + الناقصة + الأنواع — hook واحد.
 *
 * يحل محل:
 *   DocumentsTab (سطر 120-175) — 4 parallel calls
 *   DocumentsSection (سطر 132-148) — 2 useQuery calls
 *   useMissingDocumentsQuery (سطر 8-32)
 *
 * مثال:
 *   const { documents, missingDocs, docTypes, isLoading } = useDocumentsQuery(fileId);
 *   return documents.map(doc => <DocumentRow key={doc.documentID} doc={doc} />);
 */
export function useDocumentsQuery(fileId: number) {
    return useQuery({
        queryKey: [DOCUMENTS_QUERY_KEY, fileId],
        queryFn: async () => {
            const [docs, missing, autoDetected, types] = await Promise.all([
                documentsApi.getByFileId(fileId).catch(() => [] as DocumentItem[]),
                documentsApi.getMissingByFileId(fileId).catch(() => [] as any[]),
                documentsApi.getAutoDetectedMissing(fileId).catch(() => [] as any[]),
                documentsApi.getTypes().catch(() => [] as DocumentTypeLookup[]),
            ]);

            // دمج الوثائق الناقصة (يدوية + تلقائية)
            const manualMissing = missing.map((m: any) => {
                let entityName = m.entityName;
                let entityType = m.entityType;
                if (!entityName) {
                    if (m.agentName) { entityName = m.agentName; entityType = "Agent"; }
                    else if (m.guardianName) { entityName = m.guardianName; entityType = "Guardian"; }
                    else if (m.entitledName) { entityName = m.entitledName; entityType = "EntitledPerson"; }
                }
                return { ...m, entityName, entityType };
            });

            const autoMissing: MissingDocumentSummary[] = autoDetected.map((ad: any) => ({
                missingDocumentID: 0,
                documentTypeID: ad.documentTypeID,
                documentTypeName: ad.documentTypeName,
                status: "Pending" as const,
                createdAt: new Date().toISOString(),
                isAutoDetected: true,
                entityName: ad.entityName,
                entityType: ad.entityType,
                retireeID: ad.retireeID || undefined,
                agentID: ad.agentID || undefined,
                agentName: ad.entityType === "Agent" ? ad.entityName : undefined,
                guardianID: ad.guardianID || undefined,
                guardianName: ad.entityType === "Guardian" ? ad.entityName : undefined,
                entitledID: ad.entitledID || undefined,
                entitledName: ad.entityType === "EntitledPerson" ? ad.entityName : undefined,
                pledgeID: ad.pledgeID || undefined,
                exemptionID: ad.exemptionID || undefined,
                notes: undefined,
            }));

            return {
                documents: docs,
                missingDocs: [...manualMissing, ...autoMissing],
                docTypes: types,
            };
        },
        enabled: !!fileId && fileId > 0,
    });
}
```

### 5.3. ملف جديد: `ui/src/hooks/useDocumentMutations.ts`

**المسار:** `F:\Pension_Management_System\ui\src\hooks\useDocumentMutations.ts`

```typescript
import { useApiMutation } from "./useApiMutation";
import { documentsApi } from "@/api/documents";

const DOCUMENT_KEYS = ["documents"];

/**
 * رفع وثيقة.
 *
 * مثال:
 *   const uploadMutation = useUploadDocument();
 *   uploadMutation.mutate(formData);
 */
export function useUploadDocument(options?: {
    fileId?: number;
    onSuccess?: () => void;
}) {
    return useApiMutation(
        async (formData: FormData) => documentsApi.upload(formData),
        {
            invalidateKeys: options?.fileId
                ? [DOCUMENT_KEYS, [options.fileId], ["missing-documents", options.fileId]]
                : [DOCUMENT_KEYS],
            successMessage: "تم رفع الوثيقة بنجاح",
            errorTitle: "خطأ في رفع الوثيقة",
            onSuccessCallback: options?.onSuccess,
        }
    );
}

/**
 * حذف وثيقة.
 *
 * مثال:
 *   const deleteMutation = useDeleteDocument(fileId);
 *   deleteMutation.mutate(docId);
 */
export function useDeleteDocument(
    fileId: number,
    options?: { onSuccess?: () => void }
) {
    return useApiMutation(
        async (docId: number) => documentsApi.delete(docId),
        {
            invalidateKeys: [
                [DOCUMENT_KEYS[0], fileId],
                ["missing-documents", fileId],
            ],
            successMessage: "تم حذف الوثيقة بنجاح",
            errorTitle: "خطأ في حذف الوثيقة",
            onSuccessCallback: options?.onSuccess,
        }
    );
}

/**
 * تعديل وثيقة.
 *
 * مثال:
 *   const updateMutation = useUpdateDocument(fileId);
 *   updateMutation.mutate({ docId: 1, formData });
 */
export function useUpdateDocument(
    fileId: number,
    options?: { onSuccess?: () => void }
) {
    return useApiMutation(
        async ({ docId, formData }: { docId: number; formData: FormData }) =>
            documentsApi.update(docId, formData),
        {
            invalidateKeys: [[DOCUMENT_KEYS[0], fileId]],
            successMessage: "تم تعديل الوثيقة بنجاح",
            errorTitle: "خطأ في تعديل الوثيقة",
            onSuccessCallback: options?.onSuccess,
        }
    );
}

/**
 * مراجعة وثيقة.
 *
 * مثال:
 *   const reviewMutation = useReviewDocument();
 *   reviewMutation.mutate({ id: 1, dto: { pendingReviewStatus: 1 } });
 */
export function useReviewDocument(options?: {
    invalidateKey?: any[];
    onSuccess?: () => void;
}) {
    return useApiMutation(
        async ({
            id,
            dto,
        }: {
            id: number;
            dto: { pendingReviewStatus: number; notes?: string; rejectionReason?: string };
        }) => documentsApi.review(id, dto),
        {
            invalidateKeys: options?.invalidateKey || [DOCUMENT_KEYS],
            successMessage: "تم حفظ نتيجة الفحص بنجاح",
            errorTitle: "خطأ في المراجعة",
            onSuccessCallback: options?.onSuccess,
        }
    );
}

/**
 * تأشير وثيقة ناقصة كمسلّمة.
 *
 * مثال:
 *   const fulfillMutation = useFulfillMissingDocument(fileId);
 *   fulfillMutation.mutate(missingId);
 */
export function useFulfillMissingDocument(
    fileId: number,
    options?: { onSuccess?: () => void }
) {
    return useApiMutation(
        async (missingId: number) => documentsApi.fulfillMissing(missingId),
        {
            invalidateKeys: [
                [DOCUMENT_KEYS[0], fileId],
                ["missing-documents", fileId],
                ["retirement-file"],
            ],
            successMessage: "تم تأشير الوثيقة كمسلّمة بنجاح",
            errorTitle: "خطأ في التأشير",
            onSuccessCallback: options?.onSuccess,
        }
    );
}

/**
 * إنشاء وثيقة ناقصة يدوياً.
 */
export function useCreateMissingDocument(
    fileId: number,
    options?: { onSuccess?: () => void }
) {
    return useApiMutation(
        async (data: {
            fileID: number;
            documentTypeID: number;
            agentID?: number;
            guardianID?: number;
            entitledID?: number;
        }) => documentsApi.createMissing(data),
        {
            invalidateKeys: [
                [DOCUMENT_KEYS[0], fileId],
                ["missing-documents", fileId],
            ],
            successMessage: "تم إضافة الوثيقة الناقصة بنجاح",
            errorTitle: "خطأ في الإضافة",
            onSuccessCallback: options?.onSuccess,
        }
    );
}
```

### 5.4. ملف جديد: `ui/src/hooks/useFileEntitiesQuery.ts`

**المسار:** `F:\Pension_Management_System\ui\src\hooks\useFileEntitiesQuery.ts`

```typescript
import { useQuery } from "@tanstack/react-query";
import { retirementApi } from "@/api/retirement";

interface FileEntity {
    value: string;
    label: string;
}

/**
 * جلب كيانات الملف (متقاعد + وكلاء + أوصياء + مستحقين) — hook واحد.
 *
 * يحل محل:
 *   UploadDocumentDialog (سطر 82-103)
 *   BulkUploadDialog (سطر 241-268)
 *   (كود مكرر بالضبط)
 *
 * مثال:
 *   const { data: entities } = useFileEntitiesQuery(fileId);
 *   return entities?.map(e => <option key={e.value}>{e.label}</option>);
 */
export function useFileEntitiesQuery(fileId: number | undefined, enabled = true) {
    return useQuery({
        queryKey: ["file-entities", fileId],
        queryFn: async (): Promise<FileEntity[]> => {
            if (!fileId) return [];

            const [agents, guardians, entitled] = await Promise.all([
                retirementApi.getAgents(fileId).catch(() => []),
                retirementApi.getGuardians(fileId).catch(() => []),
                retirementApi.getEntitledPersons(fileId).catch(() => []),
            ]);

            const entities: FileEntity[] = [
                { value: "retiree", label: "المتقاعد (صاحب الملف)" },
            ];

            agents.forEach((a: any) =>
                entities.push({ value: `agent_${a.agentID}`, label: `الوكيل: ${a.fullName}` })
            );
            guardians.forEach((g: any) =>
                entities.push({ value: `guardian_${g.guardianID}`, label: `الوصي: ${g.fullName}` })
            );
            entitled.forEach((e: any) =>
                entities.push({ value: `entitled_${e.entitledID}`, label: `المستفيد: ${e.fullName}` })
            );

            return entities;
        },
        enabled: enabled && !!fileId,
        staleTime: 5 * 60 * 1000,
    });
}
```

### 5.5. تعديل: `ui/src/hooks/queries/index.ts`

**الملف:** `F:\Pension_Management_System\ui\src\hooks\queries\index.ts`

```typescript
// قبل:
export * from "./useAgentQuery";
export * from "./useAgentsQuery";
export * from "./useEntitledPersonQuery";
export * from "./useEntitledPersonsQuery";
export * from "./useFileDetailsQuery";
export * from "./useFiscalYearsQuery";
export * from "./useAccountsQuery";
export * from "./usePeriodsQuery";
export * from "./useGuardianQuery";
export * from "./useGuardiansQuery";
export * from "./useMissingDocumentsQuery";

// بعد:
export * from "./useAgentQuery";
export * from "./useAgentsQuery";
export * from "./useEntitledPersonQuery";
export * from "./useEntitledPersonsQuery";
export * from "./useFileDetailsQuery";
export * from "./useFiscalYearsQuery";
export * from "./useAccountsQuery";
export * from "./usePeriodsQuery";
export * from "./useGuardianQuery";
export * from "./useGuardiansQuery";
export * from "./useMissingDocumentsQuery";
export * from "./usePersonDocumentsQuery";   // ✅ كان مفقوداً — الآن مصدر
export * from "./useDocumentsQuery";         // ✅ الجديد
```

### 5.6. تعديل: `ui/src/hooks/index.ts`

**الملف:** `F:\Pension_Management_System\ui\src\hooks\index.ts`

```typescript
// إضافة exports جديدة (إذا كان الملف موجود — وإلا نbelated):
export { useDocumentBlob } from "./useDocumentBlob";
export {
    useUploadDocument,
    useDeleteDocument,
    useUpdateDocument,
    useReviewDocument,
    useFulfillMissingDocument,
    useCreateMissingDocument,
} from "./useDocumentMutations";
export { useFileEntitiesQuery } from "./useFileEntitiesQuery";
```

### 5.7. تعديل: `ui/src/hooks/queries/usePersonDocumentsQuery.ts`

**الملف:** `F:\Pension_Management_System\ui\src\hooks\queries\usePersonDocumentsQuery.ts`

```typescript
// قبل:
import { useQuery } from "@tanstack/react-query";
import api from "@/api/axios";
import type { DocumentItem, MissingDocumentInfo, PledgeItem, PersonDocumentsResult } from "@/types/retirement";

export const PERSON_DOCUMENTS_KEY = "person-documents";

export function usePersonDocumentsQuery(fileId: number, personId: number | null, personType: string | null) {
    return useQuery({
        queryKey: [PERSON_DOCUMENTS_KEY, fileId, personId, personType],
        queryFn: async (): Promise<PersonDocumentsResult> => {
            const [docsRes, missingRes, pledgesRes] = await Promise.all([
                api.get(`/documents/by-file/${fileId}`).catch(() => ({ data: { data: [] } })),
                api.get(`/missing-documents/by-file/${fileId}`).catch(() => ({ data: { data: [] } })),
                api.get(`/document-pledges/by-file/${fileId}`).catch(() => ({ data: { data: [] } })),
            ]);
            // ... (باقي الكود)
        },
        enabled: !!fileId && fileId > 0 && !!personId && !!personType,
        staleTime: 30_000,
    });
}

// بعد:
import { useQuery } from "@tanstack/react-query";
import { documentsApi } from "@/api/documents";
import api from "@/api/axios";
import type { DocumentItem, MissingDocumentInfo, PledgeItem, PersonDocumentsResult } from "@/types/retirement";

export const PERSON_DOCUMENTS_KEY = "person-documents";

export function usePersonDocumentsQuery(fileId: number, personId: number | null, personType: string | null) {
    return useQuery({
        queryKey: [PERSON_DOCUMENTS_KEY, fileId, personId, personType],
        queryFn: async (): Promise<PersonDocumentsResult> => {
            const [allDocs, allMissing, pledgesRes] = await Promise.all([
                documentsApi.getByFileId(fileId).catch(() => [] as DocumentItem[]),
                documentsApi.getMissingByFileId(fileId).catch(() => [] as MissingDocumentInfo[]),
                api.get(`/document-pledges/by-file/${fileId}`).catch(() => ({ data: { data: [] } })),
            ]);

            const allPledges = (pledgesRes?.data?.data || []) as PledgeItem[];

            let uploaded: DocumentItem[] = [];
            let missing: MissingDocumentInfo[] = [];
            let pledges: PledgeItem[] = [];

            if (personType === "entitled") {
                uploaded = allDocs.filter((d) => d.entitledID === personId);
                missing = allMissing.filter((m) => m.entitledID === personId);
                pledges = allPledges.filter((p) => p.missingDocuments?.some((md) => md.entitledID === personId));
            } else if (personType === "agent") {
                uploaded = allDocs.filter((d) => d.agentID === personId);
                missing = allMissing.filter((m) => m.agentID === personId);
                pledges = allPledges.filter((p) => p.missingDocuments?.some((md) => md.agentID === personId));
            } else if (personType === "guardian") {
                uploaded = allDocs.filter((d) => d.guardianID === personId);
                missing = allMissing.filter((m) => m.guardianID === personId);
                pledges = allPledges.filter((p) => p.missingDocuments?.some((md) => md.guardianID === personId));
            }

            return { uploaded, missing, pledges };
        },
        enabled: !!fileId && fileId > 0 && !!personId && !!personType,
        staleTime: 30_000,
    });
}
```

### 5.8. تعديل: `ui/src/hooks/queries/useMissingDocumentsQuery.ts`

**الملف:** `F:\Pension_Management_System\ui\src\hooks\queries\useMissingDocumentsQuery.ts`

```typescript
// قبل:
import { useQuery } from "@tanstack/react-query";
import api from "@/api/axios";
import type { MissingDocumentSummary } from "@/types/retirement";

export const MISSING_DOCUMENTS_KEY = "missing-documents";

export function useMissingDocumentsQuery(fileId: number) {
    return useQuery({
        queryKey: [MISSING_DOCUMENTS_KEY, fileId],
        queryFn: async () => {
            const [manualRes, autoRes] = await Promise.all([
                api.get(`/missing-documents/by-file/${fileId}`).catch(() => ({ data: { data: [] } })),
                api.get(`/missing-documents/auto-detected/by-file/${fileId}`).catch(() => ({ data: { data: [] } })),
            ]);
            // ...
        },
        enabled: !!fileId && fileId > 0,
    });
}

// بعد:
import { useQuery } from "@tanstack/react-query";
import { documentsApi } from "@/api/documents";
import type { MissingDocumentSummary } from "@/types/retirement";

export const MISSING_DOCUMENTS_KEY = "missing-documents";

export function useMissingDocumentsQuery(fileId: number) {
    return useQuery({
        queryKey: [MISSING_DOCUMENTS_KEY, fileId],
        queryFn: async () => {
            const [manual, autoDetected] = await Promise.all([
                documentsApi.getMissingByFileId(fileId).catch(() => [] as any[]),
                documentsApi.getAutoDetectedMissing(fileId).catch(() => [] as any[]),
            ]);

            const auto: MissingDocumentSummary[] = autoDetected.map((ad: any) => ({
                missingDocumentID: 0,
                documentTypeID: ad.documentTypeID,
                documentTypeName: ad.documentTypeName,
                isAutoDetected: true,
                entityType: ad.entityType,
                entityName: ad.entityName,
                status: "Pending",
                createdAt: new Date().toISOString(),
            }));

            return [...manual, ...auto];
        },
        enabled: !!fileId && fileId > 0,
    });
}
```

---

## 6. المرحلة 4: تحديث المكونات (Tier 1 — 13 ملف)

### 6.1. `ui/src/components/domain/retirement/sections/DocumentsSection.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\sections\DocumentsSection.tsx`

**الخطوط المتغيرة:**

| السطر | قبل | بعد |
|-------|------|-----|
| 9 | `import api, { getFileBaseUrl } from "@/api/axios"` | `import api, { getFileUrl } from "@/api/axios"` |
| 14 | `import type { DocumentItem, MissingDocumentInfo } from "@/types/retirement"` | يبقى كما هو |
| 20-61 | `function useDocumentBlob(filePath)` — hook محلي | **حذف** — يستخدم `useDocumentBlob` من `@/hooks` |
| 33-35 | بناء URL يدوياً مع `getFileBaseUrl()` | يُ handled عبر `useDocumentBlob` |
| 37 | `api.get(url, { responseType: "blob" })` | يُ handled عبر `useDocumentBlob` |
| 132-148 | `useQuery` منفصلة للوثائق والناقصة | يستخدم `useDocumentsQuery(fileId)` |

**الكود الجديد الجزئي:**

```typescript
// قبل:
import api, { getFileBaseUrl } from "@/api/axios";
// ...
function useDocumentBlob(filePath: string | null | undefined) {
    // 40 سطر — hook محلي
}

// بعد:
import { getFileUrl } from "@/api/axios";
import { useDocumentBlob } from "@/hooks/useDocumentBlob";
import { useDocumentsQuery } from "@/hooks/useDocumentsQuery";
// ... (حذف useDocumentBlob المحلي بالكامل — سطر 20-61)

// في المكون:
const { data, isLoading } = useDocumentsQuery(fileId);
const documents = data?.documents ?? [];
const missingDocs = data?.missingDocs ?? [];
```

### 6.2. `ui/src/pages/retirement-files/tabs/DocumentsTab.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\pages\retirement-files\tabs\DocumentsTab.tsx`

**الخطوط المتغيرة:**

| السطر | قبل | بعد |
|-------|------|-----|
| 21 | `import api, { getFileBaseUrl } from "@/api/axios"` | `import { getFileUrl } from "@/api/axios"` |
| 22 | `import { retirementApi } from "@/api/retirement"` | `import { retirementApi } from "@/api/retirement"` (يبقى لباقي الدوال) |
| 120-175 | `useQuery` مع 4 `Promise.all` + دمج يدوي | يستخدم `useDocumentsQuery(fileId)` |
| 123-128 | `Promise.all([retirementApi.getDocuments, ...])` | يُ handled عبر `useDocumentsQuery` |
| 130-171 | دمج manual + auto-detected missing docs يدوياً | يُ handled عبر `useDocumentsQuery` |
| 223 | `const base = getFileBaseUrl()` | `// لا يتغير — print يستخدم نافذة منفصلة` |
| 234-235 | `const url = \`${base}/${cleanPath}\`` | `// print window — يبقى كما هو لأن new window لا يدعم JWT` |
| 290-312 | `fulfillMutation` مع `api.post` + `retirementApi.fulfillMissingDocument` | يستخدم `useFulfillMissingDocument(fileId)` |

### 6.3. `ui/src/components/domain/retirement/DocumentViewerDialog.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\DocumentViewerDialog.tsx`

**الخطوط المتغيرة:**

| السطر | قبل | بعد |
|-------|------|-----|
| 3 | `import { getFileBaseUrl } from "@/api/axios"` | `import { getFileUrl } from "@/api/axios"` |
| 13 | `import type { DocumentItem as DocumentActionsItem } from "@/types/retirement"` | `import type { DocumentItem } from "@/types/retirement"` (بدون alias) |
| 16-25 | `export interface DocumentItem { ... }` — type مكرر | **حذف** — يستخدم `DocumentItem` من `@/types/retirement` |
| 43-57 | `function mapToActionsItem(doc: DocumentItem)` — يحول local type إلى actions type | **删除** — لا حاجة لأن types موحّدة الآن |
| 69-73 | `const getFullUrl = useCallback((path) => { ... })` | `const fullUrl = document.filePath ? getFileUrl(document.filePath) : null` |

### 6.4. `ui/src/components/domain/retirement/DocumentActions.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\DocumentActions.tsx`

**الخطوط المتغيرة:**

| السطر | قبل | بعد |
|-------|------|-----|
| 7 | `import api, { getFileBaseUrl } from "@/api/axios"` | `import { getFileUrl } from "@/api/axios"` |
| 45-52 | `deleteMutation = useApiMutation(...)` | يستخدم `useDeleteDocument(fileId)` من `@/hooks` |
| 54-68 | `noteMutation = useApiMutation(...)` | يبقى كما هو (update-specific) |
| 70-86 | `handleDownload` مع بناء URL يدوياً + `api.get(blob)` | يستخدم `useDocumentBlob` أو `getFileUrl()` |
| 72-75 | بناء URL مع `getFileBaseUrl()` + startsWith check | `const url = getFileUrl(doc.filePath)` |

### 6.5. `ui/src/components/domain/retirement/UploadDocumentDialog.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\UploadDocumentDialog.tsx`

**الخطوط المتغيرة:**

| السطر | قبل | بعد |
|-------|------|-----|
| 8 | `import { retirementApi } from "@/api/retirement"` | `import { documentsApi } from "@/api/documents"` + `import { useFileEntitiesQuery } from "@/hooks/useFileEntitiesQuery"` |
| 70-77 | `useQuery` لأنواع الوثائق | يستخدم `documentsApi.getTypesByOwner(ownerType)` |
| 82-103 | `useQuery` لجلب الكيانات (agents + guardians + entitled) | يستخدم `useFileEntitiesQuery(fileId)` |
| 107-121 | `mutation = useApiMutation(api.post("/documents", ...))` | يستخدم `useUploadDocument({ fileId })` |

### 6.6. `ui/src/components/domain/retirement/BulkUploadDialog.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\BulkUploadDialog.tsx`

**الخطوط المتغيرة:**

| السطر | قبل | بعد |
|-------|------|-----|
| 7 | `import { retirementApi } from "@/api/retirement"` | `import { documentsApi } from "@/api/documents"` + `import { useFileEntitiesQuery } from "@/hooks/useFileEntitiesQuery"` |
| 241-268 | `useQuery` لجلب الكيانات | يستخدم `useFileEntitiesQuery(fileId)` |
| 439 | `await api.post("/documents", fd, ...)` | `await documentsApi.upload(fd)` |

### 6.7. `ui/src/components/domain/retirement/EditDocumentDialog.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\EditDocumentDialog.tsx`

**الخطوط المتغيرة:**

| السطر | قبل | بعد |
|-------|------|-----|
| 8 | `import api, { getFileBaseUrl } from "@/api/axios"` | `import { getFileUrl } from "@/api/axios"` |
| 9 | `import { retirementApi } from "@/api/retirement"` | `import { documentsApi } from "@/api/documents"` + `import { useFileEntitiesQuery } from "@/hooks/useFileEntitiesQuery"` |
| 74-81 | `useQuery` لأنواع الوثائق | يستخدم `documentsApi.getTypes()` |
| 86-107 | `useQuery` لجلب الكيانات | يستخدم `useFileEntitiesQuery(fileId)` |
| 110-138 | `mutation = useApiMutation(api.put("/documents", ...))` | يستخدم `useUpdateDocument(fileId)` |
| 192-196 | `getFullUrl` مع `getFileBaseUrl()` | `const fullUrl = document?.filePath ? getFileUrl(document.filePath) : null` |

### 6.8. `ui/src/components/domain/retirement/DocumentProcessDialog.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\DocumentProcessDialog.tsx`

**الخطوط المتغيرة:**

| السطر | قبل | بعد |
|-------|------|-----|
| 19 | `import api, { getFileBaseUrl } from "@/api/axios"` | `import { getFileUrl } from "@/api/axios"` |
| 222-229 | بناء URL مع `getFileBaseUrl()` | `const imageUrl = docData?.filePath ? getFileUrl(docData.filePath) : null` |
| 482 | `await api.post("/documents", formData, ...)` | `await documentsApi.upload(formData)` |
| 522 | `await api.post("/documents", formData, ...)` | `await documentsApi.upload(formData)` |

### 6.9. `ui/src/components/domain/retirement/DocumentReviewPanel.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\DocumentReviewPanel.tsx`

**الخطوط المتغيرة:**

| السطر | قبل | بعد |
|-------|------|-----|
| 9 | `import { getFileBaseUrl } from "@/api/axios"` | `import { getFileUrl } from "@/api/axios"` |
| 52 | بناء URL مع `getFileBaseUrl()` | `const fullUrl = doc?.filePath ? getFileUrl(doc.filePath) : null` |
| 80-97 | `useMutation` للمراجعة | يستخدم `useReviewDocument()` |

### 6.10. `ui/src/components/domain/retirement/DocumentUploadView.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\DocumentUploadView.tsx`

**الخطوط المتغيرة:**

| السطر | قبل | بعد |
|-------|------|-----|
| 14 | `import api, { getFileBaseUrl } from "@/api/axios"` | `import { getFileUrl } from "@/api/axios"` |
| 145 | `await api.get(...)` لجلب الوثائق | `await documentsApi.getByFileId(fileId)` |
| 149 | بناء URL مع `getFileBaseUrl()` | `getFileUrl(filePath)` |

### 6.11. `ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\AddMissingDocumentDialog.tsx`

**الخطوط المتغيرة:**

| السطر | قبل | بعد |
|-------|------|-----|
| 57 | `await api.post("/missing-documents", { ... })` | `await documentsApi.createMissing({ ... })` |

### 6.12. `ui/src/components/domain/retirement/MissingDocActionDialog.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\MissingDocActionDialog.tsx`

**الخطوط المتغيرة:**

| السطر | قبل | بعد |
|-------|------|-----|
| 72 | `await api.post("/missing-documents", { ... })` | `await documentsApi.createMissing({ ... })` |
| 91 | `await api.post("/documents", formData, ...)` | `await documentsApi.upload(formData)` |
| 112 | `await api.post("/documents", formData, ...)` | `await documentsApi.upload(formData)` |

### 6.13. `ui/src/components/domain/retirement/sections/FinancialSection.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\sections\FinancialSection.tsx`

**الخطوط المتغيرة:**

| السطر | قبل | بعد |
|-------|------|-----|
| 10 | `import api, { getFileBaseUrl } from "@/api/axios"` | `import api, { getFileUrl } from "@/api/axios"` + `import { useDocumentBlob } from "@/hooks/useDocumentBlob"` |
| 39 | بناء URL مع `getFileBaseUrl()` | `getFileUrl(path)` |
| 41 | `api.get(url, { responseType: "blob" })` | `useDocumentBlob(path)` |
| 161-162 | بناء URL + blob fetch | `getFileUrl(path)` + `useDocumentBlob` |
| 195 | `queryClient.invalidateQueries({ queryKey: ["documents", fileId] })` | يبقى كما هو |
| 256-258 | `useQuery` + `api.get` لجلب الوثائق | `documentsApi.getByFileId(fileId)` |
| 536-538 | بناء URL + blob fetch | `getFileUrl(path)` + `useDocumentBlob` |
| 634-635 | `api.delete` + invalidateQueries | `documentsApi.delete(documentID)` |

---

## 7. المرحلة 5: تحديث المكونات (Tier 2 — 12 ملف)

### 7.1. `ui/src/components/domain/retirement/sections/PeopleSection.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\sections\PeopleSection.tsx`

| السطر | التغيير |
|-------|---------|
| 10 | إضافة `getFileUrl` للاستيراد |
| 17 | `import { usePersonDocumentsQuery } from "@/hooks/queries/usePersonDocumentsQuery"` — يبقى كما هو |
| 22 | `import { DocumentViewerDialog } from "../DocumentViewerDialog"` — يبقى |
| 229 | `api.delete(/documents/${docId})` → `documentsApi.delete(docId)` |
| 386 | `getPhotoUrl(resolvePersonalPhotoPath(person))` → يبقى (getPhotoUrl يُحدّث في utils.ts) |
| 486 | `getPhotoUrl(...)` → يبقى |
| 674, 694, 713, 737 | `invalidateQueries({ queryKey: ["person-documents", fileId] })` → يبقى |

### 7.2. `ui/src/components/domain/retirement/sections/BasicInfoSection.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\sections\BasicInfoSection.tsx`

| السطر | التغيير |
|-------|---------|
| 14 | إضافة `getFileUrl` للاستيراد |
| 167 | `getPhotoUrl(resolvePersonalPhotoPath(retiree))` → يبقى |
| 176 | `getPhotoUrl(res.data.data)` → يبقى |

### 7.3. `ui/src/components/domain/retirement/RetirementFileSidebar.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\RetirementFileSidebar.tsx`

| السطر | التغيير |
|-------|---------|
| 9 | يبقى كما هو |
| 82, 91 | `getPhotoUrl(...)` → يبقى |

### 7.4. `ui/src/components/domain/retirement/FileReviewSidebar.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\FileReviewSidebar.tsx`

| السطر | التغيير |
|-------|---------|
| 7 | يبقى كما هو |
| 37 | `getPhotoUrl(...)` → يبقى |

### 7.5. `ui/src/components/domain/retirement/EntitledPersonDetails.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\EntitledPersonDetails.tsx`

| السطر | التغيير |
|-------|---------|
| 3 | إضافة `getFileUrl` للاستيراد |
| 32-35 | `const getPhotoUrl = (path) => \`${getFileBaseUrl()}/${path...}\`` → `getFileUrl(path)` |

### 7.6. `ui/src/components/domain/retirement/ViewAgentDialog.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\ViewAgentDialog.tsx`

| السطر | التغيير |
|-------|---------|
| 4 | إضافة `getFileUrl` للاستيراد |
| 5 | `import { retirementApi }` → `import { documentsApi } from "@/api/documents"` |
| 47-49 | `useQuery` لجلب الوثائق → `documentsApi.getByFileId(fileId)` |
| 67-70 | بناء URL مع `getFileBaseUrl()` → `getFileUrl(path)` |

### 7.7. `ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\ViewEntitledPersonDialog.tsx`

| السطر | التغيير |
|-------|---------|
| 7 | إضافة `getFileUrl` للاستيراد |
| 8 | `import { retirementApi }` → إضافة `import { documentsApi }` |
| 87-90 | `useQuery` لجلب الوثائق → `documentsApi.getByFileId(person.fileID)` |
| 169 | بناء URL مع `getFileBaseUrl()` → `getFileUrl(path)` |
| 382 | `invalidateQueries({ queryKey: ["documents", person.fileID] })` → يبقى |

### 7.8. `ui/src/components/domain/retirement/EntitledPersonFormDialog.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\EntitledPersonFormDialog.tsx`

| السطر | التغيير |
|-------|---------|
| 9 | إضافة `getFileUrl` للاستيراد |
| 330 | بناء URL مع `getFileBaseUrl()` → `getFileUrl(url)` |

### 7.9. `ui/src/components/domain/retirement/PaymentBookDialog.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\PaymentBookDialog.tsx`

| السطر | التغيير |
|-------|---------|
| 27-28 | استيراد `getFileBaseUrl` → `getFileUrl` |
| 97-98 | بناء URL + blob fetch → `getFileUrl(filePath)` + `useDocumentBlob` |

### 7.10. `ui/src/components/domain/retirement/ReviewItemCard.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\retirement\ReviewItemCard.tsx`

| السطر | التغيير |
|-------|---------|
| 4 | إضافة `getFileUrl` للاستيراد |
| 38 | بناء URL مع `getFileBaseUrl()` → `getFileUrl(path)` |

### 7.11. `ui/src/components/common/ReviewFilePreview.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\common\ReviewFilePreview.tsx`

| السطر | التغيير |
|-------|---------|
| 3 | إضافة `getFileUrl` للاستيراد |
| 35 | `const baseUrl = getFileBaseUrl()` → `getFileUrl(path)` |

### 7.12. `ui/src/components/domain/investment/InvestmentDocumentsManager.tsx`

**الملف:** `F:\Pension_Management_System\ui\src\components\domain\investment\InvestmentDocumentsManager.tsx`

| السطر | التغيير |
|-------|---------|
| 91 | `api.get(...)` لجلب الوثائق → `documentsApi.getByEntity(entityType, entityId)` |
| 151 | `api.post("/documents", form, ...)` → `documentsApi.upload(form)` |
| 171 | `api.delete(...)` → `documentsApi.delete(docId)` |

---

## 8. المرحلة 6: تحديث المكونات (Tier 3 — 30 ملف — تغيير بسيط)

### 8.1. ملفات Print (4 ملفات)

| # | الملف | السطر | التغيير |
|---|-------|-------|---------|
| 1 | `ui/src/pages/print/PrintableRetirementFileReport.tsx` | 3, 19 | `getFileBaseUrl()` → `getFileUrl()` |
| 2 | `ui/src/pages/print/PrintableEmployeeReport.tsx` | 1, 17 | `getFileBaseUrl()` → `getFileUrl()` |
| 3 | `ui/src/pages/print/PaymentBookPrint.tsx` | 7, 26 | `getFileBaseUrl()` → `getFileUrl()` |
| 4 | `ui/src/pages/print/MissingDocumentsPrintPage.tsx` | 32-34, 41-43 | `api.get(...)` → `documentsApi.getByFileId(...)` + `documentsApi.getAutoDetectedMissing(...)` |

### 8.2. ملفات Archive (2 ملفات)

| # | الملف | السطور | التغيير |
|---|-------|--------|---------|
| 5 | `ui/src/pages/archive/DocumentReviewArchivePage.tsx` | 22, 23, 256 | `getFileBaseUrl()` → `getFileUrl()` + `import { DocumentViewerDialog } from ...` → يستخدم `DocumentItem` من types |
| 6 | `ui/src/pages/archive/ArchivePage.tsx` | 42 | `queryKey` يبقى — `archiveApi` مُحدّث في المرحلة 2 |

### 8.3. ملفات HR (3 ملفات)

| # | الملف | السطر | التغيير |
|---|-------|-------|---------|
| 7 | `ui/src/pages/hr/EmployeeDetailsPage.tsx` | 24, 50, 134, 452, 503 | `getFileBaseUrl()` → `getFileUrl()` + `DocumentViewerDialog` يستخدم `DocumentItem` من types |
| 8 | `ui/src/pages/hr/EmployeesPage.tsx` | 12, 120 | `getPhotoUrl(...)` → يبقى |
| 9 | `ui/src/pages/hr/HrDocumentsPage.tsx` | 29 | `queryKey` يبقى |

### 8.4. ملفات Correspondence (1 ملف)

| # | الملف | السطر | التغيير |
|---|-------|-------|---------|
| 10 | `ui/src/pages/correspondence/CorrespondenceDetailsPage.tsx` | 58, 93, 106 | لا يتغير (يستخدم endpoints خاصة بالمراسلات) |

### 8.5. ملفات Pension (4 ملفات)

| # | الملف | السطر | التغيير |
|---|-------|-------|---------|
| 11 | `ui/src/pages/pension/DocumentReviewPage.tsx` | 16, 98 | `retirementApi` → يستخدم `documentsApi` جزئياً |
| 12 | `ui/src/pages/pension/SalaryRecipientsPage.tsx` | 16, 65-66, 228-229 | `getPhotoUrl(...)` → يبقى |
| 13 | `ui/src/pages/pension/AgentsPage.tsx` | 15, 40-41 | `getPhotoUrl(...)` → يبقى |
| 14 | `ui/src/pages/pension/GuardiansPage.tsx` | 19, 71-72, 226-227 | `getPhotoUrl(...)` → يبقى |

### 8.6. ملفات Retirement Files Tabs (7 ملفات)

| # | الملف | السطر | التغيير |
|---|-------|-------|---------|
| 15 | `ui/src/pages/retirement-files/tabs/ReviewTab.tsx` | 5, 6, 57, 104, 385 | `getFileBaseUrl()` → `getFileUrl()` + `documentsApi` |
| 16 | `ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx` | 22, 32, 90, 118, 130, 154, 166, 171, 188, 201 | `api.post("/documents", ...)` → `documentsApi.upload(...)` |
| 17 | `ui/src/pages/retirement-files/tabs/EntitledPersonsTab.tsx` | 15, 83 | `getPhotoUrl(...)` → يبقى |
| 18 | `ui/src/pages/retirement-files/tabs/AgentsTab.tsx` | 11, 68 | `getPhotoUrl(...)` → يبقى |
| 19 | `ui/src/pages/retirement-files/tabs/GuardiansTab.tsx` | 13, 176 | `getPhotoUrl(...)` → يبقى |
| 20 | `ui/src/pages/retirement-files/DocumentProcessPage.tsx` | 21, 217, 225, 483, 494, 522 | `getFileBaseUrl()` → `getFileUrl()` + `documentsApi` |
| 21 | `ui/src/pages/retirees/RetireeDetailsPage.tsx` | 28, 161 | `getPhotoUrl(...)` → يبقى |

### 8.7. ملفات أخرى (12 ملف — لا تتغير أو تغيير طفيف)

| # | الملف | التغيير |
|---|-------|---------|
| 22 | `ui/src/pages/retirement-files/RetirementFileDetailsPage.tsx` | لا يتغير |
| 23 | `ui/src/pages/retirement-files/RetirementFilesPage.tsx` | لا يتغير |
| 24 | `ui/src/pages/retirement-files/FileReviewPage.tsx` | لا يتغير |
| 25 | `ui/src/pages/retirement-files/FileWorkflowManager.tsx` | لا يتغير |
| 26 | `ui/src/pages/review/FileReviewDetailPage.tsx` | لا يتغير |
| 27 | `ui/src/pages/suspensions/SuspensionsPage.tsx` | لا يتغير |
| 28 | `ui/src/pages/pension/FileReviewListPage.tsx` | لا يتغير |
| 29 | `ui/src/pages/pension/EntitledPersonsPage.tsx` | لا يتغير |
| 30 | `ui/src/pages/pension/PensionSuspensionsPage.tsx` | لا يتغير |
| 31 | `ui/src/pages/pension/PaymentCardsPage.tsx` | لا يتغير |
| 32 | `ui/src/pages/reports/ReportViewerPage.tsx` | لا يتغير |
| 33 | `ui/src/pages/assets/AssetDetailPage.tsx` | لا يتغير |

---

## 9. المرحلة 7: Vite Config + Proxy

### 9.1. تعديل: `ui/vite.config.ts`

**الملف:** `F:\Pension_Management_System\ui\vite.config.ts`

**السطور 73-78:**

```typescript
// قبل:
'/uploads': {
    target: 'http://localhost:5141',
    changeOrigin: true,
    secure: false,
    xfwd: true,
},

// بعد:
'/api/files': {
    target: 'http://localhost:5141',
    changeOrigin: true,
    secure: false,
    xfwd: true,
},
```

---

## 10. ملخص جميع الملفات

### Backend (2 ملف)

| # | الملف | الحالة |
|---|-------|--------|
| 1 | `PensionMS.API/Controllers/FilesController.cs` | **جديد** |
| 2 | `PensionMS.API/Program.cs` | **تعديل** (سطر 687-702) |

### Frontend — ملفات جديدة (5)

| # | الملف |
|---|-------|
| 1 | `ui/src/api/documents.ts` |
| 2 | `ui/src/hooks/useDocumentBlob.ts` |
| 3 | `ui/src/hooks/useDocumentsQuery.ts` |
| 4 | `ui/src/hooks/useDocumentMutations.ts` |
| 5 | `ui/src/hooks/useFileEntitiesQuery.ts` |

### Frontend — ملفات API تتغير (7)

| # | الملف |
|---|-------|
| 1 | `ui/src/api/axios.ts` |
| 2 | `ui/src/api/retirement.ts` |
| 3 | `ui/src/api/archive.ts` |
| 4 | `ui/src/api/pension.ts` |
| 5 | `ui/src/api/hr.ts` |
| 6 | `ui/src/api/investment.ts` |
| 7 | `ui/src/utils/missing-documents.ts` |

### Frontend — ملفات Hooks تتغير (4)

| # | الملف |
|---|-------|
| 1 | `ui/src/hooks/queries/index.ts` |
| 2 | `ui/src/hooks/index.ts` |
| 3 | `ui/src/hooks/queries/usePersonDocumentsQuery.ts` |
| 4 | `ui/src/hooks/queries/useMissingDocumentsQuery.ts` |

### Frontend — ملفات Utility تتغير (2)

| # | الملف |
|---|-------|
| 1 | `ui/src/lib/utils.ts` |
| 2 | `ui/vite.config.ts` |

### Frontend — ملفات مكونات Tier 1 (13)

| # | الملف |
|---|-------|
| 1 | `ui/src/components/domain/retirement/sections/DocumentsSection.tsx` |
| 2 | `ui/src/pages/retirement-files/tabs/DocumentsTab.tsx` |
| 3 | `ui/src/components/domain/retirement/DocumentViewerDialog.tsx` |
| 4 | `ui/src/components/domain/retirement/DocumentActions.tsx` |
| 5 | `ui/src/components/domain/retirement/UploadDocumentDialog.tsx` |
| 6 | `ui/src/components/domain/retirement/BulkUploadDialog.tsx` |
| 7 | `ui/src/components/domain/retirement/EditDocumentDialog.tsx` |
| 8 | `ui/src/components/domain/retirement/DocumentProcessDialog.tsx` |
| 9 | `ui/src/components/domain/retirement/DocumentReviewPanel.tsx` |
| 10 | `ui/src/components/domain/retirement/DocumentUploadView.tsx` |
| 11 | `ui/src/components/domain/retirement/AddMissingDocumentDialog.tsx` |
| 12 | `ui/src/components/domain/retirement/MissingDocActionDialog.tsx` |
| 13 | `ui/src/components/domain/retirement/sections/FinancialSection.tsx` |

### Frontend — ملفات مكونات Tier 2 (12)

| # | الملف |
|---|-------|
| 1 | `ui/src/components/domain/retirement/sections/PeopleSection.tsx` |
| 2 | `ui/src/components/domain/retirement/sections/BasicInfoSection.tsx` |
| 3 | `ui/src/components/domain/retirement/RetirementFileSidebar.tsx` |
| 4 | `ui/src/components/domain/retirement/FileReviewSidebar.tsx` |
| 5 | `ui/src/components/domain/retirement/EntitledPersonDetails.tsx` |
| 6 | `ui/src/components/domain/retirement/ViewAgentDialog.tsx` |
| 7 | `ui/src/components/domain/retirement/ViewEntitledPersonDialog.tsx` |
| 8 | `ui/src/components/domain/retirement/EntitledPersonFormDialog.tsx` |
| 9 | `ui/src/components/domain/retirement/PaymentBookDialog.tsx` |
| 10 | `ui/src/components/domain/retirement/ReviewItemCard.tsx` |
| 11 | `ui/src/components/common/ReviewFilePreview.tsx` |
| 12 | `ui/src/components/domain/investment/InvestmentDocumentsManager.tsx` |

### Frontend — ملفات مكونات Tier 3 (21)

| # | الملف |
|---|-------|
| 1 | `ui/src/pages/print/PrintableRetirementFileReport.tsx` |
| 2 | `ui/src/pages/print/PrintableEmployeeReport.tsx` |
| 3 | `ui/src/pages/print/PaymentBookPrint.tsx` |
| 4 | `ui/src/pages/print/MissingDocumentsPrintPage.tsx` |
| 5 | `ui/src/pages/archive/DocumentReviewArchivePage.tsx` |
| 6 | `ui/src/pages/archive/ArchivePage.tsx` |
| 7 | `ui/src/pages/hr/EmployeeDetailsPage.tsx` |
| 8 | `ui/src/pages/hr/EmployeesPage.tsx` |
| 9 | `ui/src/pages/hr/HrDocumentsPage.tsx` |
| 10 | `ui/src/pages/correspondence/CorrespondenceDetailsPage.tsx` |
| 11 | `ui/src/pages/pension/DocumentReviewPage.tsx` |
| 12 | `ui/src/pages/pension/SalaryRecipientsPage.tsx` |
| 13 | `ui/src/pages/pension/AgentsPage.tsx` |
| 14 | `ui/src/pages/pension/GuardiansPage.tsx` |
| 15 | `ui/src/pages/retirement-files/tabs/ReviewTab.tsx` |
| 16 | `ui/src/pages/retirement-files/tabs/PledgesAndExemptionsTab.tsx` |
| 17 | `ui/src/pages/retirement-files/DocumentProcessPage.tsx` |
| 18 | `ui/src/pages/retirees/RetireeDetailsPage.tsx` |
| 19 | `ui/src/pages/retirement-files/tabs/EntitledPersonsTab.tsx` |
| 20 | `ui/src/pages/retirement-files/tabs/AgentsTab.tsx` |
| 21 | `ui/src/pages/retirement-files/tabs/GuardiansTab.tsx` |

### الملفات التي لا تتغير (30+)

| الملف | السبب |
|-------|-------|
| `PensionMS.Core/Interfaces/IFileStorageService.cs` | واجهة موجودة |
| `PensionMS.Core/Interfaces/IDocumentService.cs` | موجود |
| `PensionMS.Core/Interfaces/IRemoteFile.cs` | موجود |
| `PensionMS.Core/Entities/Document.cs` | لا يتغير |
| `PensionMS.Core/DTOs/Documents/*.cs` | لا يتغير |
| `PensionMS.Infrastructure/Services/DocumentService.cs` | لا يتغير |
| `PensionMS.Infrastructure/Services/LocalFileStorageService.cs` | لا يتغير |
| `PensionMS.API/Controllers/DocumentsController.cs` | لا يتغير |
| `PensionMS.API/Controllers/DocumentTypesController.cs` | لا يتغير |
| `PensionMS.API/Helpers/RemoteFileWrapper.cs` | لا يتغير |
| `PensionMS.API/Helpers/RemoteFileModelBinder.cs` | لا يتغير |
| `ui/src/types/retirement.ts` | لا يتغير |
| `ui/src/types/api/retirement-file.ts` | لا يتغير |
| `ui/src/types/achievementObstacle.ts` | لا يتغير |
| `ui/src/constants/index.ts` | لا يتغير |
| `ui/src/lib/validation/retirementFileSchema.ts` | لا يتغير |
| `ui/src/lib/validation/index.ts` | لا يتغير |
| `ui/src/api/reports.ts` | لا يتغير |
| `ui/src/api/accounting.ts` | لا يتغير |
| `ui/src/api/discrepancy.ts` | لا يتغير |
| `ui/src/api/settlements.ts` | لا يتغير |
| `ui/src/api/salaries.ts` | لا يتغير |
| `ui/src/api/ministry.ts` | لا يتغير |
| `ui/src/App.tsx` | لا يتغير |
| `ui/src/components/domain/retirement/PersonDocCard.tsx` | لا يتغير |
| `ui/src/components/domain/retirement/DocumentChecklistField.tsx` | لا يتغير |
| `ui/src/components/domain/retirement/PledgeDialog.tsx` | لا يتغير |
| `ui/src/components/domain/retirement/ExemptionDialog.tsx` | لا يتغير |
| `ui/src/components/domain/retirement/AgentDialog.tsx` | لا يتغير |
| `ui/src/components/domain/retirement/RecipientsDialog.tsx` | لا يتغير |

---

## 11. أخطار ومصاعب متوقعة

| # | الخطأ | الحل |
|---|-------|------|
| 1 | **Print window لا يدعم JWT** — `window.open()` لا يرسل Authorization header | الحل: الاحتفاظ بـ `getFileBaseUrl()` للطباعة فقط، أو إضافة query param `?token=xxx` |
| 2 | **CORS على `/api/files`** — يجب التأكد من أن CORS policy يدعم المسار الجديد | الحل: التأكد من `AllowFrontend` CORS policy يشمل `/api/files` |
| 3 | **Cache browser** — الملفات القديمة قد تُ cached بعد تغيير URL | الحل: إضافة cache-busting param أو `version` query |
| 4 | **break existing components** — بعض المكونات قد تتوقف عن العمل أثناء الترحيل | الحل: الترحيل التدريجي — Tier 1 أولاً ثم Tier 2 ثم Tier 3 |
| 5 | **TypeScript errors** — `DocumentItem` type قد يتغير بين `types/retirement.ts` و `DocumentViewerDialog.tsx` | الحل: استخدام النوع من `types/retirement.ts` فقط |
| 6 | **Missing error handling** — `documentsApi` قد لا يتعامل مع 409 Conflict | الحل: إضافة try/catch في `documentsApi.upload()` |
| 7 | **Missing `FileEntity` type** — `useFileEntitiesQuery` يحتاج type محدد | الحل: تعريف `FileEntity` interface في الملف |
| 8 | **Print window + getFileUrl** — `getFileUrl` يُرجع `/api/files/...` لكن print window يحتاج URL كامل | الحل: في print window، استخدم `getFileBaseUrl()` + المسار مباشرة |

---

## 12. خطة الاختبار

### اختبارات Backend

| # | الاختبار | الطريقة |
|---|---------|---------|
| 1 | JWT-authenticated file serving | `curl -H "Authorization: Bearer xxx" http://localhost:5141/api/files/uploads/documents/abc.pdf` |
| 2 | Path traversal prevention | `curl http://localhost:5141/api/files/../../../etc/passwd` → يجب أن يرجع 400 |
| 3 | Missing file | `curl http://localhost:5141/api/files/uploads/nonexistent.pdf` → يجب أن يرجع 404 |
| 4 | Content-Type detection | التأكد من أن PDF يُرجع `application/pdf` و JPEG يُرجع `image/jpeg` |
| 5 | CORS headers | `curl -I http://localhost:5141/api/files/uploads/...` → التأكد من عدم وجود `Access-Control-Allow-Origin: *` |
| 6 | Static files block | `curl http://localhost:5141/uploads/documents/abc.pdf` → يجب أن يرجع 404 |

### اختبارات Frontend

| # | الاختبار | الطريقة |
|---|---------|---------|
| 1 | `getFileUrl()` returns correct path | `expect(getFileUrl("uploads/documents/abc.pdf")).toBe("/api/files/uploads/documents/abc.pdf")` |
| 2 | `useDocumentBlob` loads file | رفع وثيقة → التأكد من أن الصورة تظهر |
| 3 | `useDocumentsQuery` loads all data | فتح ملف → التأكد من عرض الوثائق + الناقصة + الأنواع |
| 4 | `useUploadDocument` works | رفع وثيقة جديدة → التأكد من الظهور في القائمة |
| 5 | `useDeleteDocument` works | حذف وثيقة → التأكد من الاختفاء |
| 6 | `DocumentViewerDialog` shows file | النقر على وثيقة → التأكد من العرض في النافذة |
| 7 | Bulk upload works | رفع عدة ملفات → التأكد من التقدم والنجاح |
| 8 | Print still works | طباعة وثائق → التأكد من فتح النافذة والعرض |
| 9 | Photo URLs work | فتح صفحة متقاعد → التأكد من ظهور الصورة |
| 10 | Archive page works | فتح الأرشيف → التأكد من عرض الوثائق |

---

## 13. خطة التنفيذ المقترحة

```
الأسبوع 1:
  ├── اليوم 1-2: Phase 1 (Backend FilesController + Program.cs)
  ├── اليوم 3:   Phase 2 (Frontend API + Utility)
  └── اليوم 4-5: Phase 3 (Frontend Hooks)

الأسبوع 2:
  ├── اليوم 1-2: Phase 4 (Tier 1 Components — 13 ملف)
  ├── اليوم 3:   Phase 5 (Tier 2 Components — 12 ملف)
  ├── اليوم 4:   Phase 6 (Tier 3 Components — 21 ملف)
  └── اليوم 5:   Phase 7 (Vite Config + Testing)

المجموع: ~14-18 ساعة عمل
```

---

## المراجع

- `PensionMS.Core/Interfaces/IFileStorageService.cs` — واجهة التخزين
- `PensionMS.Infrastructure/Services/LocalFileStorageService.cs` — تنفيذ التخزين
- `PensionMS.Infrastructure/Services/DocumentService.cs` — خدمة الوثائق
- `PensionMS.API/Controllers/DocumentsController.cs` — نقاط نهاية الوثائق
- `PensionMS.API/Program.cs` — إعداد Static Files
- `ui/src/api/axios.ts` — إعداد Axios + `getFileBaseUrl()`
- `ui/src/lib/utils.ts` — `getPhotoUrl()` + `resolvePersonalPhotoPath()`
- `ui/src/hooks/useApiMutation.ts` — hook التعديلات
- `ui/src/components/domain/retirement/sections/DocumentsSection.tsx` — المكون الرئيسي
- `ui/src/pages/retirement-files/tabs/DocumentsTab.tsx` — تبويب الوثائق
