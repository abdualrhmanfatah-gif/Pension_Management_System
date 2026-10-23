import { FILE_STATUS } from "../constants/retirement.ts";
import type { ApiResponse } from "../types/api.ts";

interface ReviewState {
    statusID?: number | null;
    reviewStatus?: number;
}

/** A successful request must also be visible in the persisted file state. */
export async function submitAndConfirmFileReview(
    send: () => Promise<ApiResponse<boolean>>,
    readFile: () => Promise<ReviewState>,
): Promise<ApiResponse<boolean>> {
    const response = await send();
    if (!response.isSuccess || response.data !== true) {
        throw new Error(response.message || "تعذر إرسال الملف للمراجعة");
    }
    const file = await readFile();
    if (file.statusID !== FILE_STATUS.Pending || file.reviewStatus !== 1) {
        throw new Error("لم يؤكد الخادم انتقال الملف إلى معلق وبانتظار المراجعة؛ حدّث البيانات وتحقق من حالة الملف قبل إعادة المحاولة");
    }
    return response;
}
