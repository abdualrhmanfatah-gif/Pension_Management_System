import { CheckCircle2, Clock, Eye,FileSignature, FileWarning, Trash2, Upload } from "lucide-react";

import { Button } from "@/components/ui";
import type { DocumentItem, MissingDocumentInfo, PledgeItem } from "@/types/retirement";

interface PersonDocCardUploadedProps {
    type: "uploaded";
    document: DocumentItem;
    onView?: () => void;
    onDelete?: () => void;
}

interface PersonDocCardMissingProps {
    type: "missing";
    document: MissingDocumentInfo;
    onUpload?: () => void;
    onPledge?: () => void;
    onExempt?: () => void;
}

interface PersonDocCardPledgeProps {
    type: "pledge";
    pledge: PledgeItem;
    onFulfill?: () => void;
    onUpload?: () => void;
}

type PersonDocCardProps = PersonDocCardUploadedProps | PersonDocCardMissingProps | PersonDocCardPledgeProps;

export function PersonDocCard(props: PersonDocCardProps) {
    if (props.type === "uploaded") {
        return (
            <div className="flex items-center gap-3 p-3 rounded-lg border border-border bg-card hover:border-muted-foreground/30 transition-colors">
                <CheckCircle2 className="w-5 h-5 text-green-600 shrink-0" />
                <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium truncate">{props.document.documentTypeName}</p>
                    <p className="text-xs text-muted-foreground">
                        {props.document.createdAt && new Date(props.document.createdAt).toLocaleDateString("ar-YE")}
                        {props.document.uploadedByUser && ` — ${props.document.uploadedByUser}`}
                    </p>
                </div>
                <div className="flex gap-1 shrink-0">
                    {props.onView && (
                        <Button variant="ghost" size="icon" className="h-7 w-7" onClick={props.onView}>
                            <Eye className="w-3.5 h-3.5" />
                        </Button>
                    )}
                    {props.onDelete && (
                        <Button variant="ghost" size="icon" className="h-7 w-7 text-destructive" onClick={props.onDelete}>
                            <Trash2 className="w-3.5 h-3.5" />
                        </Button>
                    )}
                </div>
            </div>
        );
    }

    if (props.type === "missing") {
        return (
            <div className="flex items-center gap-3 p-3 rounded-lg border border-red-200 bg-red-50 dark:border-red-800 dark:bg-red-950/20">
                <FileWarning className="w-5 h-5 text-red-500 shrink-0" />
                <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium truncate">{props.document.documentTypeName}</p>
                    <p className="text-xs text-red-600 dark:text-red-400">
                        {props.document.entityName ? `${props.document.entityName} — ` : ""}
                        مطلوبة
                    </p>
                </div>
                <div className="flex gap-1 shrink-0">
                    <Button variant="ghost" size="icon" className="h-7 w-7" onClick={props.onUpload} title="رفع">
                        <Upload className="w-3.5 h-3.5" />
                    </Button>
                    <Button variant="ghost" size="icon" className="h-7 w-7" onClick={props.onPledge} title="تعهد">
                        <FileSignature className="w-3.5 h-3.5" />
                    </Button>
                </div>
            </div>
        );
    }

    return (
        <div className="flex items-center gap-3 p-3 rounded-lg border border-amber-200 bg-amber-50 dark:border-amber-800 dark:bg-amber-950/20">
            <Clock className="w-5 h-5 text-amber-600 shrink-0" />
            <div className="flex-1 min-w-0">
                <p className="text-sm font-medium truncate">
                    تعهد — {props.pledge.missingDocuments?.map((d) => d.documentTypeName).join(", ") || "وثائق معلقة"}
                </p>
                <p className="text-xs text-amber-600 dark:text-amber-400">
                    المهلة: {new Date(props.pledge.deadline).toLocaleDateString("ar-YE")}
                    {props.pledge.pledgedByName && ` — المتعهد: ${props.pledge.pledgedByName}`}
                </p>
            </div>
            <div className="flex gap-1 shrink-0">
                <Button variant="ghost" size="icon" className="h-7 w-7" onClick={props.onFulfill} title="مستوفى">
                    <CheckCircle2 className="w-3.5 h-3.5" />
                </Button>
                <Button variant="ghost" size="icon" className="h-7 w-7" onClick={props.onUpload} title="رفع">
                    <Upload className="w-3.5 h-3.5" />
                </Button>
            </div>
        </div>
    );
}
