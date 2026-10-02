import { CalendarRange, FileText, Users } from "lucide-react";

import { Heading, PageLayout, Tabs, TabsContent, TabsList, TabsTrigger, Text } from "@/components/ui";

import { ContractsTab } from "./tabs/ContractsTab";
import { PaymentSchedulesTab } from "./tabs/PaymentSchedulesTab";
import { TenantsTab } from "./tabs/TenantsTab";

export function LeasingPage() {
    return (
        <PageLayout>
            <div className="flex items-center justify-between">
                <div>
                    <Heading level="h1">إدارة التأجير</Heading>
                    <Text variant="description">تنظيم علاقات المستأجرين وعقود الإيجار والتحصيل</Text>
                </div>
            </div>

            <Tabs defaultValue="tenants" className="w-full" dir="rtl">
                <TabsList className="grid w-full grid-cols-2 sm:grid-cols-3 md:w-[500px] h-12 justify-start bg-muted/50 p-1 underline-none">
                    <TabsTrigger value="tenants" className="flex items-center gap-2 py-2">
                        <Users className="w-4 h-4" /> <span>المستأجرين</span>
                    </TabsTrigger>
                    <TabsTrigger value="contracts" className="flex items-center gap-2 py-2">
                        <FileText className="w-4 h-4" /> <span>عقود الإيجار</span>
                    </TabsTrigger>
                    <TabsTrigger value="payments" className="flex items-center gap-2 py-2">
                        <CalendarRange className="w-4 h-4" /> <span>جداول الدفع</span>
                    </TabsTrigger>
                </TabsList>

                <TabsContent value="tenants" className="mt-4">
                    <TenantsTab />
                </TabsContent>

                <TabsContent value="contracts" className="mt-4">
                    <ContractsTab />
                </TabsContent>

                <TabsContent value="payments" className="mt-4">
                    <PaymentSchedulesTab />
                </TabsContent>
            </Tabs>
        </PageLayout>
    );
}
