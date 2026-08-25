import { BaseFormDialog } from "@/components/ui/BaseFormDialog";
import { InvestmentDocumentsManager } from "@/components/domain/investment/InvestmentDocumentsManager";

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    entityType: "investment-portfolio" | "investment-project" | "investment-property" | "property-unit" | "lease-contract" | "tenant" | "maintenance-ticket" | "work-order" | "property-valuation" | "depreciation-record" | "lease-payment" | "operational-expense" | "investment-plan" | "investment-txn" | "risk-assessment";
    entityId: number;
    title: string;
    entityName?: string;
}

/**
 * A standardized shared dialog for viewing and managing investment-related documents.
 * Wraps the InvestmentDocumentsManager component inside a BaseFormDialog for consistency.
 */
export function InvestmentDocsViewerDialog({ 
    open, 
    onOpenChange, 
    entityType, 
    entityId, 
    title,
    entityName 
}: Props) {
    return (
        <BaseFormDialog 
            open={open} 
            onOpenChange={onOpenChange}
            title={title}
            description="إدارة الوثائق والمستندات الثبوتية المرتبطة بهذا السجل"
            size="4xl"
            onSubmit={() => {}}
            submitLabel=""
            footer={null}
        >
            {entityId > 0 && (
                <InvestmentDocumentsManager 
                    entityType={entityType} 
                    entityId={entityId} 
                    entityName={entityName}
                />
            )}
        </BaseFormDialog>
    );
}
