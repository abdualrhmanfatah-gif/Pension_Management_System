import { Settings } from "lucide-react";

import { investmentApi } from "@/api/investment";
import { Heading, PageLayout, Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui";

import { LookupsTab } from "./tabs/LookupsTab";

export function InvestmentSettingsPage() {
    return (
        <PageLayout>
            <div className="flex items-center gap-2">
                <Settings className="w-6 h-6 text-primary" />
                <Heading level="h1">إعدادات الاستثمار</Heading>
            </div>

            <Tabs defaultValue="invTypes" className="w-full" dir="rtl">
                <TabsList className="grid w-full grid-cols-2 sm:grid-cols-4 md:w-[800px] h-12">
                    <TabsTrigger value="invTypes">أنواع الاستثمار</TabsTrigger>
                    <TabsTrigger value="propTypes">أنواع العقارات</TabsTrigger>
                    <TabsTrigger value="unitTypes">أنواع الوحدات</TabsTrigger>
                    <TabsTrigger value="txnTypes">أنواع الحركات</TabsTrigger>
                </TabsList>

                <TabsContent value="invTypes" className="mt-4">
                    <LookupsTab
                        title="أنواع الاستثمار"
                        modalTitle="تحديث نوع استثمار"
                        queryKey="investment-types"
                        fetchFn={() => investmentApi.getInvestmentTypes()}
                        createFn={(data) => investmentApi.createInvestmentType(data)}
                        updateFn={async () => true} // Update endpoint to be implemented if needed
                        deleteFn={async () => true} // Delete endpoint to be implemented if needed
                    />
                </TabsContent>

                <TabsContent value="propTypes" className="mt-4">
                    <LookupsTab
                        title="أنواع العقارات"
                        modalTitle="تحديث نوع عقار"
                        queryKey="property-types"
                        fetchFn={() => investmentApi.getPropertyTypes()}
                        createFn={(data) => investmentApi.createPropertyType(data)}
                        updateFn={async () => true}
                        deleteFn={async () => true}
                    />
                </TabsContent>

                <TabsContent value="unitTypes" className="mt-4">
                    <LookupsTab
                        title="أنواع الوحدات"
                        modalTitle="تحديث نوع وحدة"
                        queryKey="unit-types"
                        fetchFn={() => investmentApi.getUnitTypes()}
                        createFn={(data) => investmentApi.createUnitType(data)}
                        updateFn={async () => true}
                        deleteFn={async () => true}
                    />
                </TabsContent>

                <TabsContent value="txnTypes" className="mt-4">
                    <LookupsTab
                        title="أنواع الحركات المالية"
                        modalTitle="تحديث نوع حركة"
                        queryKey="transaction-types"
                        fetchFn={() => investmentApi.getTransactionTypes()}
                        createFn={(data) => investmentApi.createTransactionType(data)}
                        updateFn={async () => true}
                        deleteFn={async () => true}
                    />
                </TabsContent>
            </Tabs>
        </PageLayout>
    );
}
