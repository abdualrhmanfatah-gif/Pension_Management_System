import { ClipboardCheck, ShieldCheck } from "lucide-react";

import { Heading, PageLayout, Tabs, TabsContent, TabsList, TabsTrigger, Text } from "@/components/ui";

import { ApprovalRequestsTab } from "./tabs/ApprovalRequestsTab";
import { RiskAssessmentsTab } from "./tabs/RiskAssessmentsTab";

export function InvestmentGovernancePage() {
    return (
        <PageLayout>
            <div className="flex flex-col gap-1">
                <Heading level="h1">الحوكمة والاعتمادات</Heading>
                <Text variant="description">إدارة مخاطر المشاريع وسير عمل الاعتمادات الاستثمارية</Text>
            </div>

            <Tabs defaultValue="risks" className="w-full" dir="rtl">
                <TabsList className="grid w-full grid-cols-2 md:w-[400px] h-12">
                    <TabsTrigger value="risks" className="flex items-center gap-2">
                        <ShieldCheck className="w-4 h-4" /> تقييم المخاطر
                    </TabsTrigger>
                    <TabsTrigger value="approvals" className="flex items-center gap-2">
                        <ClipboardCheck className="w-4 h-4" /> طلبات الاعتماد
                    </TabsTrigger>
                </TabsList>

                <TabsContent value="risks" className="mt-4">
                    <RiskAssessmentsTab />
                </TabsContent>

                <TabsContent value="approvals" className="mt-4">
                    <ApprovalRequestsTab />
                </TabsContent>
            </Tabs>
        </PageLayout>
    );
}
