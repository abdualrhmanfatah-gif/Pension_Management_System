import { DollarSign as DollarIcon } from "lucide-react";

import { Button, Card } from "@/components/ui";
import { useHasPermission } from "@/stores/authStore";

interface MaintenanceCostsTabProps {
    onAddCost: () => void;
}

export function MaintenanceCostsTab({ onAddCost }: MaintenanceCostsTabProps) {
    const canManageCosts = useHasPermission('Investment.Finance', 'System.Admin');

    return (
        <Card className="p-8 flex flex-col items-center justify-center text-center" dir="rtl">
            <div className="w-16 h-16 bg-muted rounded-full flex items-center justify-center mb-4 text-muted-foreground">
                <DollarIcon className="w-8 h-8" />
            </div>
            <h3 className="text-lg font-bold">إدارة التكاليف والاعتمادات</h3>
            <p className="text-muted-foreground max-w-sm mt-2 mb-6">تسجيل المبالغ المصروفة فعلياً على هذا البلاغ.</p>
            
            {canManageCosts ? (
                <Button 
                    variant="primary"
                    onClick={onAddCost}
                    className="px-6 py-2 rounded-lg"
                >
                    إضافة تكلفة جديدة
                </Button>
            ) : (
                <div className="text-sm text-amber-600 bg-amber-50 px-4 py-2 rounded-lg border border-amber-100">
                    لا تملك صلاحية إضافة تكاليف. يرجى مراجعة المسؤول المالي.
                </div>
            )}
        </Card>
    );
}
