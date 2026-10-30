import { useQuery } from "@tanstack/react-query";
<<<<<<< HEAD
import {
  ArrowDownRight,
  ArrowUpRight,
  Landmark,
  TrendingDown,
  TrendingUp,
  Vault,
} from "lucide-react";
import { useEffect, useMemo, useState } from "react";

import { accountsApi, fiscalYearsApi,reportsApi } from "@/api/accounting";
=======
import { useEffect, useMemo, useState } from "react";
import {
  Vault,
  ArrowUpRight,
  ArrowDownRight,
  Landmark,
  TrendingUp,
  TrendingDown,
} from "lucide-react";

import { accountsApi, reportsApi, fiscalYearsApi } from "@/api/accounting";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
import {
  Card,
  Heading,
  PageLayout,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Skeleton,
  StickyHeader,
  Text,
} from "@/components/ui";
import { formatCurrency } from "@/lib/utils";
<<<<<<< HEAD
import type { AccountStatementResultDto,AccountTreeNode, FiscalYearSummaryDto } from "@/types/accounting";
=======
import type { AccountTreeNode, FiscalYearSummaryDto, AccountStatementResultDto } from "@/types/accounting";
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

export function TreasuryPage() {
  const [fiscalYearId, setFiscalYearId] = useState<number | null>(null);
  const [selectedAccountId, setSelectedAccountId] = useState<number | null>(null);

  const { data: fiscalYears } = useQuery<FiscalYearSummaryDto[]>({
    queryKey: ["fiscal-years"],
    queryFn: () => fiscalYearsApi.getAll(),
  });

  useEffect(() => {
    if (fiscalYears && fiscalYearId === null) {
      const currentYear = new Date().getFullYear().toString();
      const match = fiscalYears.find((y) => y.yearName === currentYear);
      if (match) setFiscalYearId(match.fiscalYearID);
      else if (fiscalYears.length > 0) setFiscalYearId(fiscalYears[0].fiscalYearID);
    }
  }, [fiscalYears, fiscalYearId]);

  // جلب شجرة الحسابات للحسابات التي تبدأ بـ 18 (الخزينة)
  const { data: allAccounts, isLoading: accountsLoading } = useQuery<AccountTreeNode[]>({
    queryKey: ["accounts", "tree"],
    queryFn: () => accountsApi.getTree(),
  });

  const treasuryAccounts = useMemo(() => {
    if (!allAccounts) return [];
    const flatten = (nodes: AccountTreeNode[]): AccountTreeNode[] => {
      const result: AccountTreeNode[] = [];
      for (const node of nodes) {
        if (node.accountCode?.startsWith("18")) result.push(node);
        if (node.children) result.push(...flatten(node.children));
      }
      return result;
    };
    return flatten(allAccounts);
  }, [allAccounts]);

  // جلب كشف حساب الخزينة
  const { data: statement, isLoading: statementLoading } = useQuery<AccountStatementResultDto>({
    queryKey: ["treasury-statement", selectedAccountId, fiscalYearId],
    queryFn: () =>
      reportsApi.accountStatement({
        accountId: selectedAccountId!,
        fiscalYearId: fiscalYearId!,
        fromDate: `${new Date().getFullYear()}-01-01`,
        toDate: new Date().toISOString().slice(0, 10),
      }),
    enabled: selectedAccountId != null && fiscalYearId != null,
  });

  useEffect(() => {
    if (treasuryAccounts.length > 0 && selectedAccountId === null) {
      setSelectedAccountId(treasuryAccounts[0].accountID);
    }
  }, [treasuryAccounts, selectedAccountId]);

  useEffect(() => {
    if (statement === undefined) return;
  }, [statement]);

  const totalDebit = statement?.totalDebit ?? 0;
  const totalCredit = statement?.totalCredit ?? 0;
  const closingBalance = statement?.closingBalance ?? 0;
  const openingBalance = statement?.openingBalance ?? 0;

  if (accountsLoading) {
    return (
      <div className="p-8 space-y-8" dir="rtl">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3 sm:gap-6">
          {[1, 2, 3, 4].map((i) => (
            <Skeleton key={i} className="h-32 rounded-3xl" />
          ))}
        </div>
        <Skeleton className="h-[400px] rounded-3xl" />
      </div>
    );
  }

  return (
    <PageLayout>
      <StickyHeader>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
<<<<<<< HEAD
            <div className="p-2 rounded-xl bg-success/10 ">
              <Vault className="h-6 w-6 text-success " />
=======
            <div className="p-2 rounded-xl bg-emerald-100 dark:bg-emerald-900/30">
              <Vault className="h-6 w-6 text-emerald-600 dark:text-emerald-400" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            </div>
            <Heading level="h2">الخزينة</Heading>
          </div>
          <div className="flex items-center gap-2">
            {fiscalYears && (
              <Select
                value={fiscalYearId?.toString() ?? ""}
                onValueChange={(v) => setFiscalYearId(Number(v))}
              >
                <SelectTrigger className="w-[160px]">
                  <SelectValue placeholder="السنة المالية" />
                </SelectTrigger>
                <SelectContent>
                  {fiscalYears.map((fy) => (
                    <SelectItem key={fy.fiscalYearID} value={fy.fiscalYearID.toString()}>
                      {fy.yearName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
            {treasuryAccounts.length > 1 && (
              <Select
                value={selectedAccountId?.toString() ?? ""}
                onValueChange={(v) => setSelectedAccountId(Number(v))}
              >
                <SelectTrigger className="w-[220px]">
                  <SelectValue placeholder="اختر الحساب" />
                </SelectTrigger>
                <SelectContent>
                  {treasuryAccounts.map((acc) => (
                    <SelectItem key={acc.accountID} value={acc.accountID.toString()}>
                      {acc.accountCode} - {acc.accountName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
          </div>
        </div>
      </StickyHeader>

      <div className="p-4 md:p-6 space-y-6">
        {/* بطاقات الملخص */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
          <Card className="p-4">
            <div className="flex items-center gap-2 text-muted-foreground text-sm mb-1">
              <Landmark className="h-4 w-4" />
              الرصيد الافتتاحي
            </div>
            <Text variant="body" className="font-bold">
              {formatCurrency(openingBalance)}
            </Text>
          </Card>
          <Card className="p-4">
            <div className="flex items-center gap-2 text-muted-foreground text-sm mb-1">
<<<<<<< HEAD
              <ArrowDownRight className="h-4 w-4 text-success" />
              إجمالي المدين (الوارد)
            </div>
            <Text variant="body" className="font-bold text-success">
=======
              <ArrowDownRight className="h-4 w-4 text-green-600" />
              إجمالي المدين (الوارد)
            </div>
            <Text variant="body" className="font-bold text-green-600">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
              {formatCurrency(totalDebit)}
            </Text>
          </Card>
          <Card className="p-4">
            <div className="flex items-center gap-2 text-muted-foreground text-sm mb-1">
<<<<<<< HEAD
              <ArrowUpRight className="h-4 w-4 text-danger" />
              إجمالي الدائن (الصادر)
            </div>
            <Text variant="body" className="font-bold text-danger">
=======
              <ArrowUpRight className="h-4 w-4 text-red-600" />
              إجمالي الدائن (الصادر)
            </div>
            <Text variant="body" className="font-bold text-red-600">
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
              {formatCurrency(totalCredit)}
            </Text>
          </Card>
          <Card className="p-4">
            <div className="flex items-center gap-2 text-muted-foreground text-sm mb-1">
              {closingBalance >= 0 ? (
<<<<<<< HEAD
                <TrendingUp className="h-4 w-4 text-success" />
              ) : (
                <TrendingDown className="h-4 w-4 text-danger" />
=======
                <TrendingUp className="h-4 w-4 text-emerald-600" />
              ) : (
                <TrendingDown className="h-4 w-4 text-red-600" />
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
              )}
              الرصيد الختامي
            </div>
            <Text
              variant="body"
<<<<<<< HEAD
              className={`font-bold ${closingBalance >= 0 ? "text-success" : "text-danger"}`}
=======
              className={`font-bold ${closingBalance >= 0 ? "text-emerald-600" : "text-red-600"}`}
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            >
              {formatCurrency(closingBalance)}
            </Text>
          </Card>
        </div>

        {/* كشف الحساب */}
        <Card>
          <div className="p-4 border-b flex items-center justify-between">
            <Heading level="h3">كشف حساب الخزينة</Heading>
            {statement && (
              <Text variant="small" className="text-muted-foreground">
                {statement.accountCode} - {statement.accountName}
              </Text>
            )}
          </div>
          {statementLoading ? (
            <div className="p-8">
              <Skeleton className="h-[300px] rounded-xl" />
            </div>
          ) : statement && statement.items.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b bg-muted/50">
                    <th className="p-3 text-right font-medium">التاريخ</th>
                    <th className="p-3 text-right font-medium">رقم القيد</th>
                    <th className="p-3 text-right font-medium">البيان</th>
                    <th className="p-3 text-left font-medium">مدين</th>
                    <th className="p-3 text-left font-medium">دائن</th>
                    <th className="p-3 text-left font-medium">الرصيد الجاري</th>
                  </tr>
                </thead>
                <tbody>
                  {statement.items.map((item, idx) => (
                    <tr key={idx} className="border-b hover:bg-muted/30">
                      <td className="p-3">{item.entryDate?.slice(0, 10)}</td>
                      <td className="p-3 font-mono text-xs">{item.entryNumber}</td>
                      <td className="p-3">{item.description}</td>
                      <td className="p-3 text-left font-mono">
                        {item.debit > 0 ? formatCurrency(item.debit) : "-"}
                      </td>
                      <td className="p-3 text-left font-mono">
                        {item.credit > 0 ? formatCurrency(item.credit) : "-"}
                      </td>
                      <td className="p-3 text-left font-mono font-bold">
                        {formatCurrency(item.runningBalance)}
                      </td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr className="border-t-2 bg-muted/50 font-bold">
                    <td className="p-3" colSpan={3}>
                      المجموع
                    </td>
                    <td className="p-3 text-left font-mono">{formatCurrency(totalDebit)}</td>
                    <td className="p-3 text-left font-mono">{formatCurrency(totalCredit)}</td>
                    <td className="p-3 text-left font-mono">{formatCurrency(closingBalance)}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          ) : (
            <div className="p-8 text-center text-muted-foreground">
              لا توجد حركات في الخزينة لهذا الفترة
            </div>
          )}
        </Card>
      </div>
    </PageLayout>
  );
}
