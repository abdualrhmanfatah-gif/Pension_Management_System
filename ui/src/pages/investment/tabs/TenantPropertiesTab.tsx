import { useQuery } from "@tanstack/react-query";
import { ExternalLink } from "lucide-react";
import { useEffect, useMemo } from "react";
import { Link } from "react-router-dom";
import { toast } from "sonner";

import { investmentApi, type PropertyDto } from "@/api/investment";
import { Badge, CrudPageLayout, type CustomColumnDef, ErrorState } from "@/components/ui";

interface TenantPropertiesTabProps {
    tenantId: number;
}

/**
 * تبويب عقارات المستأجر
 */
export function TenantPropertiesTab({ tenantId }: TenantPropertiesTabProps) {
    const { data: properties, isLoading, error } = useQuery({
        queryKey: ["tenant-properties", tenantId],
        queryFn: () => investmentApi.getTenantProperties(tenantId),
        enabled: !!tenantId
    });

    useEffect(() => {
        if (error) toast.error("تعذر تحميل العقارات", { id: "tenant-properties" });
    }, [error]);

    const items = Array.isArray(properties) ? properties : [];

    const getStatusColor = (status: string): "success" | "warning" | "danger" | "outline" => {
        switch (status) {
            case "Ready": return "success";
            case "Occupied": return "success";
            case "UnderMaintenance": return "warning";
            case "Vacant": return "outline";
            default: return "outline";
        }
    };

    const getStatusLabel = (status: string) => {
        switch (status) {
            case "Ready": return "جاهز";
            case "Occupied": return "مؤجر";
            case "UnderMaintenance": return "قيد الصيانة";
            case "Vacant": return "شاغر";
            default: return status;
        }
    };

    const columns = useMemo<CustomColumnDef<PropertyDto>[]>(() => [
        { key: "propertyName", header: "اسم العقار", accessor: (row) => row.propertyName },
        { key: "locationCity", header: "المدينة", accessor: (row) => row.locationCity || "—" },
        {
            key: "totalArea",
            header: "المساحة",
            render: (row) => <span className="font-mono">{row.totalArea.toLocaleString("ar-YE")} م²</span>
        },
        {
            key: "currentMarketValue",
            header: "القيمة السوقية",
            render: (row) => <span className="font-mono">{row.currentMarketValue.toLocaleString("ar-YE")} ر.ي</span>
        },
        {
            key: "status",
            header: "الحالة",
            render: (row) => (
                <Badge variant={getStatusColor(row.status)}>
                    {getStatusLabel(row.status)}
                </Badge>
            )
        },
        {
            key: "actions",
            header: "عرض",
            render: (row) => (
                <Link to={`/investment/properties/${row.propertyID}`} className="p-2 hover:bg-muted rounded-md transition-colors inline-flex">
                    <ExternalLink className="w-4 h-4" />
                </Link>
            )
        }
    ], []);

    if (error) {
        return <ErrorState message="خطأ في تحميل عقارات المستأجر" />;
    }

    return (
        <CrudPageLayout
            title="عقارات المستأجر"
            description="العقارات المرتبطة بهذا المستأجر"
            columns={columns}
            data={items}
            loading={isLoading}
            emptyMessage="لم يتم ربط أي عقارات بهذا المستأجر بعد"
        />
    );
}
