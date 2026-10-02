import { useState } from "react";
import { toast } from "sonner";

import { ministryApi } from "@/api/ministry";
import { Button, Dialog, DialogContent, DialogHeader, DialogTitle, Input } from "@/components/ui";
import { useApiMutation } from "@/hooks";

interface Props {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onSuccess: () => void;
}

export function CreateForceBatchDialog({ open, onOpenChange, onSuccess }: Props) {
    const [batchNumber, setBatchNumber] = useState("");
    const [batchDate, setBatchDate] = useState("");

    const createMutation = useApiMutation(
        (data: { batchNumber: string; batchDate: string }) =>
            ministryApi.createBatch(data),
        {
            successMessage: "تم إنشاء الكشف بنجاح",
            invalidateKeys: [["force-batches"]],
            onSuccess: () => {
                onOpenChange(false);
                setBatchNumber("");
                setBatchDate("");
                onSuccess();
            },
        }
    );

    const handleSubmit = () => {
        if (!batchNumber.trim()) {
            toast.error("أدخل رقم الكشف");
            return;
        }
        createMutation.mutate({ batchNumber, batchDate: batchDate || new Date().toISOString() });
    };

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-md">
                <DialogHeader>
                    <DialogTitle>إحالة جديدة</DialogTitle>
                </DialogHeader>
                <div className="space-y-4 py-2">
                    <div>
                        <label className="label-field">رقم الكشف</label>
                        <Input value={batchNumber} onChange={(e) => setBatchNumber(e.target.value)} placeholder="رقم الكشف" />
                    </div>
                    <div>
                        <label className="label-field">التاريخ</label>
                        <Input type="date" value={batchDate} onChange={(e) => setBatchDate(e.target.value)} />
                    </div>
                    <div className="flex justify-end gap-2 pt-2">
                        <Button variant="outline" onClick={() => onOpenChange(false)}>إلغاء</Button>
                        <Button onClick={handleSubmit} disabled={createMutation.isPending}>
                            {createMutation.isPending ? "جاري الإنشاء..." : "إنشاء"}
                        </Button>
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    );
}
