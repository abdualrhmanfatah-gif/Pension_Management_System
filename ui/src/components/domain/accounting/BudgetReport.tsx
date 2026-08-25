import { Card, Skeleton, Badge, Button, Heading } from "@/components/ui";
import { formatCurrency } from "@/lib/utils";
import { Pencil, Trash2 } from "lucide-react";

interface Props {
  data: any;
  isLoading: boolean;
  onRefresh: () => void;
  onEdit?: (item: any) => void;
  onDelete?: (item: any) => void;
  showQuarters?: boolean;
  showMonthly?: boolean;
  selectedMonth?: number | null;
}

export function BudgetReport({ data, isLoading, showQuarters, showMonthly, selectedMonth, onEdit, onDelete }: Props) {
  if (isLoading) {
    return <Skeleton className="h-96 rounded-3xl" />;
  }

  if (!data) {
    return (
      <Card className="p-12 text-center text-muted-foreground">
        لا توجد بيانات موازنة
      </Card>
    );
  }

  // حالة التقرير الشهري
  if (showMonthly && data?.months) {
    // تصفية الشهور حسب الاختيار
    const filteredMonths = selectedMonth
      ? data.months.filter((m: any) => m.periodID === selectedMonth)
      : data.months;

    return (
      <div className="space-y-4">
        {filteredMonths.length === 0 ? (
          <Card className="p-12 text-center text-muted-foreground">
            لا توجد بيانات موازنة لهذا الشهر
          </Card>
        ) : (
          filteredMonths.map((month: any) => (
          <Card
            key={month.periodID}
            className="border-none rounded-3xl overflow-hidden"
          >
            <div className="p-4 bg-muted/30 flex items-center justify-between">
               <Heading level="h3" className="font-bold text-lg">{month.periodName}</Heading>
              <div className="flex gap-4 text-sm">
                <span>المخطط: {formatCurrency(month.totalPlanned)}</span>
                <span>الفعلي: {formatCurrency(month.totalActual)}</span>
              </div>
            </div>
            <table className="w-full text-sm" role="table" aria-label="تقرير الموازنة">
              <thead className="bg-muted/50 sticky top-0 z-10">
                <tr>
                  <th className="text-end p-3">رقم الحساب</th>
                  <th className="text-end p-3">الحساب</th>
                  <th className="text-end p-3">المخطط</th>
                  <th className="text-end p-3">الفعلي</th>
                  <th className="text-end p-3">الحالة</th>
                  {(onEdit || onDelete) && <th className="text-end p-3">إجراءات</th>}
                </tr>
              </thead>
              <tbody>
                {month.items?.map((item: any) => (
                  <tr
                    key={item.budgetItemID}
                    className="border-t border-border/50 hover:bg-muted/20"
                  >
                    <td className="p-3 font-mono text-xs">{item.accountCode}</td>
                    <td className="p-3 font-medium">{item.accountName}</td>
                    <td className="p-3">{formatCurrency(item.plannedAmount)}</td>
                    <td className="p-3">{formatCurrency(item.actualAmount)}</td>
                    <td className="p-3">
                      <Badge
                        variant={
                          item.status === "مطابق"
                            ? "success"
                            : item.status === "تجاوز"
                              ? "destructive"
                              : "secondary"
                        }
                      >
                        {item.status}
                      </Badge>
                    </td>
                    {(onEdit || onDelete) && (
                      <td className="p-3">
                        <div className="flex gap-1">
                          {onEdit && (
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => onEdit(item)}
                            >
                              <Pencil className="w-4 h-4" />
                            </Button>
                          )}
                          {onDelete && (
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => onDelete(item)}
                              className="text-destructive hover:text-destructive"
                            >
                              <Trash2 className="w-4 h-4" />
                            </Button>
                          )}
                        </div>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </Card>
        ))
        )}
      </div>
    );
  }

  // حالة التقرير السنوي
  if (!showQuarters) {
    return (
      <Card className="border-none rounded-3xl overflow-hidden">
        <div className="p-4 bg-muted/30">
          <Heading level="h3" className="font-bold text-lg">
            تقرير الموازنة السنوي - {data.yearName}
          </Heading>
        </div>
        <table className="w-full text-sm">
          <thead className="bg-muted/50 sticky top-0 z-10">
            <tr>
              <th className="text-end p-3">رقم الحساب</th>
              <th className="text-end p-3">الحساب</th>
              <th className="text-end p-3">المخطط</th>
              <th className="text-end p-3">الفعلي</th>
              <th className="text-end p-3">الفرق</th>
              <th className="text-end p-3">النسبة</th>
              <th className="text-end p-3">الحالة</th>
              {(onEdit || onDelete) && <th className="text-end p-3">إجراءات</th>}
            </tr>
          </thead>
          <tbody>
            {data.items?.map((item: any) => (
              <tr
                key={item.budgetItemID}
                className="border-t border-border/50 hover:bg-muted/20"
              >
                <td className="p-3 font-mono text-xs">{item.accountCode}</td>
                <td className="p-3 font-medium">{item.accountName}</td>
                <td className="p-3">{formatCurrency(item.plannedAmount)}</td>
                <td className="p-3">{formatCurrency(item.actualAmount)}</td>
                <td
                  className={`p-3 font-medium ${
                    item.variance > 0
                      ? "text-destructive"
                      : item.variance < 0
                        ? "text-emerald-600 dark:text-emerald-400"
                        : ""
                  }`}
                >
                  {formatCurrency(Math.abs(item.variance))}
                </td>
                <td className="p-3">{Math.abs(item.variancePercentage)}%</td>
                <td className="p-3">
                  <Badge
                    variant={
                      item.status === "مطابق"
                        ? "success"
                        : item.status === "تجاوز"
                          ? "destructive"
                          : "secondary"
                    }
                  >
                    {item.status}
                  </Badge>
                </td>
                {(onEdit || onDelete) && (
                  <td className="p-3">
                    <div className="flex gap-1">
                      {onEdit && (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => onEdit(item)}
                        >
                          <Pencil className="w-4 h-4" />
                        </Button>
                      )}
                      {onDelete && (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => onDelete(item)}
                          className="text-destructive hover:text-destructive"
                        >
                          <Trash2 className="w-4 h-4" />
                        </Button>
                      )}
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
          <tfoot className="bg-muted/50 font-bold">
            <tr>
              <td className="p-3"></td>
              <td className="p-3">الإجمالي</td>
              <td className="p-3">{formatCurrency(data.totalPlanned)}</td>
              <td className="p-3">{formatCurrency(data.totalActual)}</td>
              <td
                className={`p-3 ${
                  data.totalVariance > 0 ? "text-destructive" : "text-emerald-600 dark:text-emerald-400"
                }`}
              >
                {formatCurrency(Math.abs(data.totalVariance))}
              </td>
              <td className="p-3">{Math.abs(data.totalVariancePercentage)}%</td>
              <td></td>
            </tr>
          </tfoot>
        </table>
      </Card>
    );
  }

  // حالة التقرير حسب الفصول
  return (
    <div className="space-y-4">
      {data.quarters?.map((quarter: any) => (
        <Card
          key={quarter.quarterID}
          className="border-none rounded-3xl overflow-hidden"
        >
          <div className="p-4 bg-muted/30 flex items-center justify-between">
            <Heading level="h3" className="font-bold text-lg">{quarter.quarterName}</Heading>
            <div className="flex gap-4 text-sm">
              <span>المخطط: {formatCurrency(quarter.totalPlanned)}</span>
              <span>الفعلي: {formatCurrency(quarter.totalActual)}</span>
            </div>
          </div>
          <table className="w-full text-sm">
            <thead className="bg-muted/50 sticky top-0 z-10">
              <tr>
                <th className="text-end p-3">رقم الحساب</th>
                <th className="text-end p-3">الحساب</th>
                <th className="text-end p-3">المخطط</th>
                <th className="text-end p-3">الفعلي</th>
                <th className="text-end p-3">الحالة</th>
                {(onEdit || onDelete) && <th className="text-end p-3">إجراءات</th>}
              </tr>
            </thead>
            <tbody>
              {quarter.items?.map((item: any) => (
                <tr
                  key={item.budgetItemID}
                  className="border-t border-border/50 hover:bg-muted/20"
                >
                  <td className="p-3 font-mono text-xs">{item.accountCode}</td>
                  <td className="p-3 font-medium">{item.accountName}</td>
                  <td className="p-3">{formatCurrency(item.plannedAmount)}</td>
                  <td className="p-3">{formatCurrency(item.actualAmount)}</td>
                  <td className="p-3">
                    <Badge
                      variant={
                        item.status === "مطابق"
                          ? "success"
                          : item.status === "تجاوز"
                            ? "destructive"
                            : "secondary"
                      }
                    >
                      {item.status}
                    </Badge>
                  </td>
                  {(onEdit || onDelete) && (
                    <td className="p-3">
                      <div className="flex gap-1">
                        {onEdit && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => onEdit(item)}
                          >
                            <Pencil className="w-4 h-4" />
                          </Button>
                        )}
                        {onDelete && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => onDelete(item)}
                            className="text-destructive hover:text-destructive"
                          >
                            <Trash2 className="w-4 h-4" />
                          </Button>
                        )}
                      </div>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      ))}
    </div>
  );

  return null;
}
