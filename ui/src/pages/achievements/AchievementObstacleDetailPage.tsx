import { useQuery } from "@tanstack/react-query";
import {
    ArrowRight,
    CheckCircle,
    Clock,
    FileText,
    MessageSquare,
    Target,
    User,
    XCircle,
} from "lucide-react";
import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { toast } from "sonner";

import { achievementObstacleApi } from "@/api/achievementObstacles";
import {
    Badge,
    Breadcrumb,
    Button,
    ErrorState,
    Heading,
    InfoPair,
    Input,
    Loader,
    PageLayout,
    Separator,
    SummaryCard,
    Text,
} from "@/components/ui";
import { useApiMutation } from "@/hooks";
import { formatYemenDate } from "@/lib/date";
import type {
    AchievementObstacleType,
    TrackingCommentDto,
    TrackingStatus,
} from "@/types/achievementObstacle";

// ─── Constants ─────────────────────────────────────────

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

const statusVariant: Record<TrackingStatus, "info" | "warning" | "success" | "ghost"> = {
    New: "info",
    InProgress: "warning",
    Completed: "success",
    Closed: "ghost",
};

const statusIcons: Record<TrackingStatus, React.ReactNode> = {
    New: <Target className="w-4 h-4" />,
    InProgress: <Clock className="w-4 h-4" />,
    Completed: <CheckCircle className="w-4 h-4" />,
    Closed: <XCircle className="w-4 h-4" />,
};

// ─── Component ─────────────────────────────────────────

export function AchievementObstacleDetailPage() {
    const { id } = useParams<{ id: string }>();
    const recordId = Number(id);
    const [commentText, setCommentText] = useState("");

    const { data: record, isLoading, error } = useQuery({
        queryKey: ["achievement-obstacle-detail", recordId],
        queryFn: () => achievementObstacleApi.get(recordId),
        enabled: !!recordId,
    });

    const { data: comments = [], isLoading: isCommentsLoading } = useQuery({
        queryKey: ["achievement-obstacle-comments", recordId],
        queryFn: () => achievementObstacleApi.getComments(recordId),
        enabled: !!recordId,
    });

    const addCommentMutation = useApiMutation(
        ({ id, comment }: { id: number; comment: string }) =>
            achievementObstacleApi.addComment(id, comment),
        {
            successMessage: "تمت إضافة الملاحظة بنجاح",
            invalidateKeys: [
                ["achievement-obstacle-comments", recordId],
                ["achievement-obstacle-detail", recordId],
            ],
            onSuccessCallback: () => setCommentText(""),
        }
    );

    useEffect(() => {
        if (error) toast.error("تعذر تحميل تفاصيل السجل", { id: "achievement-detail" });
    }, [error]);

    const handleAddComment = () => {
        const comment = commentText.trim();
        if (!recordId || !comment || addCommentMutation.isPending) return;
        addCommentMutation.mutate({ id: recordId, comment });
    };

    const handleCommentKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key !== "Enter" || e.shiftKey) return;
        e.preventDefault();
        handleAddComment();
    };

    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-[60dvh]">
                <Loader variant="page" />
            </div>
        );
    }

    if (error) {
        return (
            <PageLayout>
                <ErrorState message="تعذر تحميل تفاصيل السجل" />
            </PageLayout>
        );
    }

    if (!record) {
        return (
            <PageLayout>
                <div className="py-16 text-center text-destructive font-bold">السجل غير موجود</div>
            </PageLayout>
        );
    }

    return (
        <PageLayout>
            <Breadcrumb
                items={[
                    { label: "الإنجازات والمعوقات", href: "/achievements" },
                    { label: record.number },
                ]}
            />

            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex items-center gap-4">
                    <Link
                        to="/achievements"
                        className="p-2 hover:bg-muted rounded-full transition-colors border"
                        title="رجوع"
                    >
                        <ArrowRight className="w-5 h-5" />
                    </Link>
                    <div>
                        <div className="flex items-center gap-2 flex-wrap">
                            <Heading level="h1" className="font-black text-2xl">{record.title}</Heading>
                            <Badge variant={record.type === "Achievement" ? "success" : "destructive"} className="text-sm">
                                {typeLabels[record.type]}
                            </Badge>
                            <Badge variant={statusVariant[record.status]} className="gap-1 text-sm">
                                {statusIcons[record.status]}
                                {statusLabels[record.status]}
                            </Badge>
                        </div>
                        <Text variant="description" className="flex items-center gap-2 font-medium mt-1 text-base">
                            <span className="font-mono text-sm" dir="ltr">{record.number}</span>
                            <span className="opacity-30 mx-1">|</span>
                            <User className="w-4 h-4 text-muted-primary" /> {record.employeeName || record.createdByUsername}
                            <span className="opacity-30 mx-1">|</span>
                            {record.departmentName || "بدون قسم"}
                        </Text>
                    </div>
                </div>
            </div>

            {/* <SummaryCard className="[&_p]:text-base [&_span]:text-base">
                <SummaryRow className="sm:grid-cols-2">
                    <InfoPair label="الرقم" value={record.number} icon={<Hash className="w-4 h-4" />} mono dir="ltr" />
                    <InfoPair label="النوع" value={typeLabels[record.type]} icon={<Tag className="w-4 h-4" />} />
                    <InfoPair label="العنوان" value={record.title} icon={<FileText className="w-4 h-4" />} />
                    <InfoPair
                        label="الحالة"
                        icon={<CheckCircle className="w-4 h-4" />}
                        value={
                            <Badge variant={statusVariant[record.status]} className="gap-1 text-sm">
                                {statusIcons[record.status]}
                                {statusLabels[record.status]}
                            </Badge>
                        }
                    />
                </SummaryRow>

                <Separator />

                <SummaryRow className="sm:grid-cols-2">
                    <InfoPair label="القسم" value={record.departmentName} icon={<Target className="w-4 h-4" />} />
                    <InfoPair label="الموظف" value={record.employeeName} icon={<User className="w-4 h-4" />} />
                    <InfoPair label="المالك" value={record.ownerUsername} icon={<User className="w-4 h-4" />} />
                    <InfoPair label="أنشأه" value={record.createdByUsername} icon={<User className="w-4 h-4" />} />
                </SummaryRow>

                {record.updatedByUsername && (
                    <>
                        <Separator />
                        <SummaryRow className="sm:grid-cols-2">
                            <InfoPair label="عدّله" value={record.updatedByUsername} icon={<User className="w-4 h-4" />} />
                            <InfoPair
                                label="آخر تعديل"
                                value={
                                    record.updatedAt
                                        ? new Date(record.updatedAt).toLocaleDateString("ar-YE")
                                        : undefined
                                }
                                icon={<Calendar className="w-4 h-4" />}
                            />
                        </SummaryRow>
                    </>
                )}
            </SummaryCard> */}

            {record.description?.trim() && (
                <SummaryCard className="[&_p]:text-base [&_span]:text-base">
                    <InfoPair label="الوصف" value={record.description} icon={<FileText className="w-4 h-4" />} className="[&_p]:!text-lg [&_span]:!text-lg" />
                </SummaryCard>
            )}

            <SummaryCard className="[&_p]:text-base [&_span]:text-base [&_div]:text-base">
                <div className="flex items-center gap-2 mb-4 pb-3 border-b border-muted/50">
                    <MessageSquare className="w-5 h-5 text-primary" />
                    <h3 className="text-base font-semibold">الملاحظات ({comments.length})</h3>
                </div>

                {isCommentsLoading ? (
                    <div className="flex justify-center py-6">
                        <Loader />
                    </div>
                ) : comments.length > 0 ? (
                    <div className="space-y-3">
                        {comments.map((comment: TrackingCommentDto) => (
                            <div
                                key={comment.id}
                                className="rounded-lg bg-muted/30 p-3"
                            >
                                <Text variant="body" className="text-base">{comment.comment}</Text>
                                <Text
                                    variant="small"
                                    className="mt-1 block text-muted-foreground text-sm"
                                >
                                    {comment.createdByUsername} —{" "}
                                    {formatYemenDate(comment.createdAt)}
                                </Text>
                            </div>
                        ))}
                    </div>
                ) : (
                    <Text variant="muted" className="block py-4 text-center">
                        لا توجد ملاحظات
                    </Text>
                )}

                <Separator className="my-4" />

                <div className="flex gap-2">
                    <Input
                        value={commentText}
                        onChange={(e) => setCommentText(e.target.value)}
                        placeholder="أضف ملاحظة..."
                        disabled={addCommentMutation.isPending}
                        onKeyDown={handleCommentKeyDown}
                        aria-label="نص الملاحظة"
                        className="flex-1 h-11 text-base"
                    />
                    <Button
                        type="button"
                        onClick={handleAddComment}
                        disabled={!commentText.trim() || addCommentMutation.isPending || !recordId}
                        className="h-11 px-6 text-base"
                    >
                        {addCommentMutation.isPending ? "جاري الإضافة..." : "إضافة"}
                    </Button>
                </div>
            </SummaryCard>
        </PageLayout>
    );
}
