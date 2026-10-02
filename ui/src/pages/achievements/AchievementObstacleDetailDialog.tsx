
import { useQuery } from "@tanstack/react-query";
import { MessageSquare, Target } from "lucide-react";
import { useEffect, useState } from "react";

import { achievementObstacleApi } from "@/api/achievementObstacles";
import {
    Badge,
    Button,
    Card,
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    Heading,
    Input,
    Loader,
    ScrollArea,
    Separator,
    SummaryCard,
    SummaryItem,
    SummaryRow,
    Text,
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import type {
    AchievementObstacleType,
    TrackingCommentDto,
    TrackingStatus,
} from "@/types/achievementObstacle";

// -----------------------------------------------------------------------------
// Constants
// -----------------------------------------------------------------------------

const typeLabels: Record<AchievementObstacleType, string> = {
    Achievement: "إنجاز",
    Obstacle: "معوق",
};

const statusLabels: Record<TrackingStatus, string> = {
    New: "جديد",
    InProgress: "قيد التنفيذ",
    Completed: "مكتمل",
    Closed: "مغلق",
};

const statusVariant: Record<
    TrackingStatus,
    "info" | "warning" | "success" | "ghost"
> = {
    New: "info",
    InProgress: "warning",
    Completed: "success",
    Closed: "ghost",
};

// -----------------------------------------------------------------------------
// Props
// -----------------------------------------------------------------------------

interface AchievementObstacleDetailDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    recordId: number | null;
}

// -----------------------------------------------------------------------------
// Component
// -----------------------------------------------------------------------------

export function AchievementObstacleDetailDialog({
    open,
    onOpenChange,
    recordId,
}: AchievementObstacleDetailDialogProps) {
    const [commentText, setCommentText] = useState("");

    /**
     * Clear the comment input whenever the selected record changes
     * or the dialog is closed.
     */
    useEffect(() => {
        if (!open) {
            setCommentText("");
        }
    }, [open, recordId]);

    // -------------------------------------------------------------------------
    // Queries
    // -------------------------------------------------------------------------

    const {
        data: record,
        isLoading: isRecordLoading,
        isError: isRecordError,
    } = useQuery({
        queryKey: ["achievement-obstacle-detail", recordId],
        queryFn: async () => {
            if (recordId === null) {
                throw new Error("Record ID is required");
            }

            return achievementObstacleApi.get(recordId);
        },
        enabled: open && recordId !== null,
    });

    const {
        data: comments = [],
        isLoading: isCommentsLoading,
    } = useQuery({
        queryKey: ["achievement-obstacle-comments", recordId],
        queryFn: async () => {
            if (recordId === null) {
                throw new Error("Record ID is required");
            }

            return achievementObstacleApi.getComments(recordId);
        },
        enabled: open && recordId !== null,
    });

    // -------------------------------------------------------------------------
    // Mutations
    // -------------------------------------------------------------------------

    const addCommentMutation = useApiMutation(
        ({ id, comment }: { id: number; comment: string }) =>
            achievementObstacleApi.addComment(id, comment),
        {
            successMessage: "تمت إضافة الملاحظة بنجاح",
            invalidateKeys: [
                ["achievement-obstacle-comments", recordId],
            ],
            onSuccessCallback: () => {
                setCommentText("");
            },
        }
    );

    // -------------------------------------------------------------------------
    // Handlers
    // -------------------------------------------------------------------------

    const handleAddComment = () => {
        const comment = commentText.trim();

        if (recordId === null || !comment) {
            return;
        }

        if (addCommentMutation.isPending) {
            return;
        }

        addCommentMutation.mutate({
            id: recordId,
            comment,
        });
    };

    const handleCommentKeyDown = (
        event: React.KeyboardEvent<HTMLInputElement>
    ) => {
        if (event.key !== "Enter" || event.shiftKey) {
            return;
        }

        event.preventDefault();
        handleAddComment();
    };

    // -------------------------------------------------------------------------
    // Render
    // -------------------------------------------------------------------------

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent
                className="flex max-h-[90dvh] max-w-2xl flex-col"
                dir="rtl"
            >
                <DialogHeader>
                    <DialogTitle className="flex items-center gap-2">
                        <Target className="h-5 w-5" />
                        تفاصيل السجل
                    </DialogTitle>
                </DialogHeader>

                {isRecordLoading ? (
                    <div className="flex items-center justify-center py-16">
                        <Loader />
                    </div>
                ) : isRecordError ? (
                    <Text
                        variant="muted"
                        className="py-16 text-center"
                    >
                        تعذر تحميل بيانات السجل
                    </Text>
                ) : !record ? (
                    <Text
                        variant="muted"
                        className="py-16 text-center"
                    >
                        السجل غير موجود
                    </Text>
                ) : (
                    <ScrollArea className="max-h-[70dvh] flex-1">
                        <div className="space-y-4 p-1">
                            {/* Header */}
                            <Card className="p-4">
                                <div className="flex items-start justify-between gap-4">
                                    <div className="flex shrink-0 gap-2">
                                        <Badge
                                            variant={
                                                record.type === "Achievement"
                                                    ? "success"
                                                    : "destructive"
                                            }
                                        >
                                            {typeLabels[record.type]}
                                        </Badge>

                                        <Badge
                                            variant={
                                                statusVariant[record.status]
                                            }
                                        >
                                            {statusLabels[record.status]}
                                        </Badge>
                                    </div>

                                    <div className="min-w-0 flex-1 text-end">
                                        <Heading level="h3">
                                            {record.title}
                                        </Heading>

                                        <Text
                                            variant="small"
                                            className="font-mono text-muted-foreground"
                                            dir="ltr"
                                        >
                                            {record.number}
                                        </Text>
                                    </div>
                                </div>
                            </Card>

                            {/* Details */}
                            <SummaryCard
                                title="البيانات"
                                icon={<Target className="h-4 w-4" />}
                            >
                                <SummaryRow className="grid-cols-1 sm:grid-cols-2">
                                    <SummaryItem
                                        label="القسم"
                                        value={record.departmentName}
                                        
                                    />

                                    <SummaryItem
                                        label="الموظف"
                                        value={record.employeeName}
                                        
                                    />

                                    <SummaryItem
                                        label="تاريخ الإنشاء"
                                        value={new Date(
                                            record.createdAt
                                        ).toLocaleDateString("ar-YE")}
                                        
                                    />

                                    {record.updatedAt && (
                                        <SummaryItem
                                            label="آخر تعديل"
                                            value={new Date(
                                                record.updatedAt
                                            ).toLocaleDateString("ar-YE")}
                                            
                                        />
                                    )}
                                </SummaryRow>

                                <SummaryRow className="grid-cols-1 sm:grid-cols-2">
                                    <SummaryItem
                                        label="أنشأه"
                                        value={record.createdByUsername}
                                        
                                    />

                                    {record.updatedByUsername && (
                                        <SummaryItem
                                            label="عدّله"
                                            value={record.updatedByUsername}
                                            
                                        />
                                    )}
                                </SummaryRow>
                            </SummaryCard>

                            {/* Description */}
                            {record.description?.trim() && (
                                <SummaryCard title="الوصف">
                                    <Text
                                        variant="body"
                                        className="whitespace-pre-wrap text-end"
                                    >
                                        {record.description}
                                    </Text>
                                </SummaryCard>
                            )}

                            {/* Comments */}
                            <SummaryCard
                                title={`الملاحظات (${comments.length})`}
                                icon={
                                    <MessageSquare className="h-4 w-4" />
                                }
                            >
                                {isCommentsLoading ? (
                                    <div className="flex justify-center py-6">
                                        <Loader />
                                    </div>
                                ) : comments.length > 0 ? (
                                    <div className="space-y-3">
                                        {comments.map(
                                            (comment: TrackingCommentDto) => (
                                                <div
                                                    key={comment.id}
                                                    className="rounded-lg bg-muted/30 p-3"
                                                >
                                                    <Text variant="body">
                                                        {comment.comment}
                                                    </Text>

                                                    <Text
                                                        variant="small"
                                                        className="mt-1 block text-muted-foreground"
                                                    >
                                                        {
                                                            comment.createdByUsername
                                                        }{" "}
                                                        —{" "}
                                                        {new Date(
                                                            comment.createdAt
                                                        ).toLocaleDateString(
                                                            "ar-YE"
                                                        )}
                                                    </Text>
                                                </div>
                                            )
                                        )}
                                    </div>
                                ) : (
                                    <Text
                                        variant="muted"
                                        className="block py-4 text-center"
                                    >
                                        لا توجد ملاحظات
                                    </Text>
                                )}

                                <Separator className="my-4" />

                                <div className="flex gap-2">
                                    <Input
                                        value={commentText}
                                        onChange={(event) =>
                                            setCommentText(event.target.value)
                                        }
                                        placeholder="أضف ملاحظة..."
                                        disabled={
                                            addCommentMutation.isPending
                                        }
                                        onKeyDown={handleCommentKeyDown}
                                        aria-label="نص الملاحظة"
                                    />

                                    <Button
                                        type="button"
                                        size="sm"
                                        onClick={handleAddComment}
                                        disabled={
                                            !commentText.trim() ||
                                            addCommentMutation.isPending ||
                                            recordId === null
                                        }
                                    >
                                        {addCommentMutation.isPending
                                            ? "جاري الإضافة..."
                                            : "إضافة"}
                                    </Button>
                                </div>
                            </SummaryCard>
                        </div>
                    </ScrollArea>
                )}
            </DialogContent>
        </Dialog>
    );
}

