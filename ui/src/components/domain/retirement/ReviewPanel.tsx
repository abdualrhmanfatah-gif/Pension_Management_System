import { X } from "lucide-react";

<<<<<<< HEAD
import { retirementApi } from "@/api/retirement";
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { Heading, Text } from "@/components/ui";
import { Button } from "@/components/ui";
import { ScrollArea } from "@/components/ui";
import { useReviewItemsQuery } from "@/hooks/queries/useReviewItemsQuery";
import { useApiMutation } from "@/hooks/useApiMutation";
<<<<<<< HEAD

=======
import { retirementApi } from "@/api/retirement";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import { ReviewItemCard } from "./ReviewItemCard";

interface ReviewPanelProps {
    fileId: number;
    isOpen: boolean;
    onClose: () => void;
}

export function ReviewPanel({ fileId, isOpen, onClose }: ReviewPanelProps) {
    const { data: reviewData } = useReviewItemsQuery(fileId);

    const approveItemMutation = useApiMutation(
        async (itemId: number) => {
            await retirementApi.approveReviewItem(itemId);
        },
        {
            invalidateKeys: [["review-items", fileId], ["retirement-file", fileId]],
            successMessage: "تم اعتماد التغيير",
        }
    );

    const rejectItemMutation = useApiMutation(
        async ({ itemId, reason }: { itemId: number; reason: string }) => {
            await retirementApi.rejectReviewItem(itemId, reason);
        },
        {
            invalidateKeys: [["review-items", fileId], ["retirement-file", fileId]],
            successMessage: "تم رفض التغيير",
        }
    );

    const approveAllMutation = useApiMutation(
        async () => {
            await retirementApi.approveReview(fileId);
        },
        {
            invalidateKeys: [["review-items", fileId], ["retirement-file", fileId]],
            successMessage: "تم اعتماد المراجعة بنجاح",
        }
    );

    const rejectAllMutation = useApiMutation(
        async () => {
            await retirementApi.rejectReview(fileId, "رفض الكل");
        },
        {
            invalidateKeys: [["review-items", fileId], ["retirement-file", fileId]],
            successMessage: "تم رفض المراجعة",
        }
    );

    if (!isOpen) return null;

    return (
        <div className="w-[480px] border-r border-border bg-card flex-shrink-0 flex flex-col h-full">
            <div className="p-4 border-b border-border">
                <div className="flex items-center justify-between mb-3">
                    <Heading level="h4">مراجعة التعديلات</Heading>
                    <Button variant="ghost" size="icon" className="h-7 w-7" onClick={onClose}>
                        <X className="w-4 h-4" />
                    </Button>
                </div>

                <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 mb-3">
                    <div className="text-center p-2 rounded-lg bg-muted/30">
                        <Text variant="caption">الكل</Text>
                        <p className="text-lg font-bold">{reviewData?.totalCount || 0}</p>
                    </div>
                    <div className="text-center p-2 rounded-lg bg-muted/30">
                        <Text variant="caption">بانتظار</Text>
                        <p className="text-lg font-bold text-amber-600">{reviewData?.pendingCount || 0}</p>
                    </div>
                    <div className="text-center p-2 rounded-lg bg-muted/30">
                        <Text variant="caption">معتمد</Text>
                        <p className="text-lg font-bold text-green-600">{reviewData?.approvedCount || 0}</p>
                    </div>
                    <div className="text-center p-2 rounded-lg bg-muted/30">
                        <Text variant="caption">مرفوض</Text>
                        <p className="text-lg font-bold text-red-600">{reviewData?.rejectedCount || 0}</p>
                    </div>
                </div>

                {(reviewData?.totalCount || 0) > 0 && (
                    <div>
                        <div className="flex justify-between text-xs text-muted-foreground mb-1">
                            <span>تقدم المراجعة</span>
                            <span>{Math.round(((reviewData?.approvedCount || 0) + (reviewData?.rejectedCount || 0)) / (reviewData?.totalCount || 1) * 100)}%</span>
                        </div>
                        <div className="h-1.5 bg-muted rounded-full overflow-hidden">
                            <div
                                className="h-full bg-green-500 rounded-full transition-all"
                                style={{
                                    width: `${((reviewData?.approvedCount || 0) + (reviewData?.rejectedCount || 0)) / (reviewData?.totalCount || 1) * 100}%`,
                                }}
                            />
                        </div>
                    </div>
                )}
            </div>

            <ScrollArea className="h-0 flex-1 p-4">
                <div className="space-y-2 min-w-0 w-full">
                    {reviewData?.items.map((item) => (
                        <ReviewItemCard
                            key={item.itemId}
                            item={item}
                            onApprove={(id) => approveItemMutation.mutate(id)}
                            onReject={(id) => rejectItemMutation.mutate({ itemId: id, reason: "" })}
                        />
                    ))}
                    {(!reviewData?.items || reviewData.items.length === 0) && (
                        <Text variant="description" className="text-center py-8">
                            لا توجد بنود للمراجعة
                        </Text>
                    )}
                </div>
            </ScrollArea>

            <div className="p-4 border-t border-border space-y-2">
                <Button
                    variant="success"
                    className="w-full"
                    onClick={() => approveAllMutation.mutate(undefined)}
                    loading={approveAllMutation.isPending}
                >
                    اعتماد الكل
                </Button>
                <Button
                    variant="destructive"
                    className="w-full"
                    onClick={() => rejectAllMutation.mutate(undefined)}
                    loading={rejectAllMutation.isPending}
                >
                    رفض الكل
                </Button>
            </div>
        </div>
    );
}
