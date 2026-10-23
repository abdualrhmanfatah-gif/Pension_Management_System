import { retirementApi } from "@/api/retirement";
import { PermissionGate } from "@/components/auth/PermissionGate";
import { Button, NoteBox } from "@/components/ui";
import { FILE_STATUS } from "@/constants/retirement";
import { useApiMutation } from "@/hooks/useApiMutation";
import { submitAndConfirmFileReview } from "@/lib/fileReviewSubmission";

export function FileLifecycleActions({ fileId, statusId, previousStatusId, reviewStatus, salaryStatus }: {
    fileId: number; statusId: number | null | undefined; previousStatusId?: number | null; reviewStatus?: number; salaryStatus?: string;
}) {
    const canReinstate = statusId === FILE_STATUS.EntitlementEnded ||
        statusId === FILE_STATUS.Pending && previousStatusId === FILE_STATUS.EntitlementEnded;
    const submit = useApiMutation(
        () => submitAndConfirmFileReview(
            () => canReinstate ? retirementApi.reinstateEntitlement(fileId) : retirementApi.requestReview(fileId),
            () => retirementApi.getFile(fileId),
        ),
        { successMessage: response => response.message || "تم تأكيد إرسال الملف للمراجعة وإيقاف صرف المعاش" },
    );
    return (
        <div className="flex flex-wrap items-center gap-3" aria-live="polite">
            <span className="text-sm text-muted-foreground">
                حالة المراجعة: {reviewStatus === 1 ? "بانتظار المراجعة" : reviewStatus === 2 ? "معتمدة" : reviewStatus === 3 ? "مرفوضة — يلزم التصحيح وإعادة الإرسال" : "لم تُرسل"}
            </span>
            {salaryStatus && <span className="text-sm">حالة المعاش: {salaryStatus}</span>}
            {(statusId === FILE_STATUS.New || canReinstate || reviewStatus === 3) && (
                <PermissionGate permission="File.Edit.BasicData">
                    <Button type="button" variant="outline" loading={submit.isPending} onClick={() => submit.mutate(undefined)}>
                        {canReinstate ? "إعادة الاستحقاق" : "إرسال للمراجعة"}
                    </Button>
                </PermissionGate>
            )}
            {statusId === FILE_STATUS.Pending && (
                <NoteBox type="warning" title="صرف المعاش موقوف">
                    تُحفظ المبالغ المستحقة أثناء التعليق، وتصبح قابلة للصرف بعد اعتماد المراجعة وزوال جميع الإيقافات.
                </NoteBox>
            )}
        </div>
    );
}
