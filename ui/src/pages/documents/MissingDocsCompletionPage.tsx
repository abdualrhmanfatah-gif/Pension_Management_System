import { DocumentUploadView } from "@/components/domain/retirement/DocumentUploadView";

/**
 * استكمال النواقص — صفحة مخصصة لتوثيق ورفع الوثائق الناقصة لملفات التقاعد.
 * تعرض قائمة الملفات مع عدد النواقص وحالة التوثيق مع إمكانية التصفية والبحث.
 */
export function MissingDocsCompletionPage() {
    return <DocumentUploadView />;
}
