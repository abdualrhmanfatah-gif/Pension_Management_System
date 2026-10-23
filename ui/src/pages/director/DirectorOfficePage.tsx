import { useQuery, useQueryClient } from "@tanstack/react-query";
import { FileText, Inbox, LayoutDashboard, Plus,Send } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { directorApi } from "@/api/director";
import { DashboardHeader } from "@/components/common/dashboards";
import { CorrespondenceFormDialog } from "@/components/domain/correspondence/CorrespondenceFormDialog";
import { StatCard } from "@/components/ui";
import { Badge, Button, Card, Heading, PageLayout, Text } from "@/components/ui";
import { queryKeys } from "@/constants/queryKeys";
import { CorrespondenceDirection, type CorrespondenceListItemDto } from "@/types/correspondence";

export function DirectorOfficePage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [formOpen, setFormOpen] = useState(false);
  const [formDirection, setFormDirection] = useState<CorrespondenceDirection>(CorrespondenceDirection.Outgoing);

  const { data: outgoing, isLoading: outgoingLoading } = useQuery({
    queryKey: queryKeys.director.outgoing,
    queryFn: () => directorApi.getRecentCorrespondence({ direction: 0, pageSize: 5 }),
  });

  const { data: incoming, isLoading: incomingLoading } = useQuery({
    queryKey: queryKeys.director.incoming,
    queryFn: () => directorApi.getRecentCorrespondence({ direction: 1, pageSize: 5 }),
  });

  const totalOutgoing = outgoing?.data?.totalCount || 0;
  const totalIncoming = incoming?.data?.totalCount || 0;
  const isLoading = outgoingLoading || incomingLoading;

  const openCreateForm = (direction: CorrespondenceDirection) => {
    setFormDirection(direction);
    setFormOpen(true);
  };

  const handleSaved = () => {
    queryClient.invalidateQueries({ queryKey: queryKeys.director.outgoing });
    queryClient.invalidateQueries({ queryKey: queryKeys.director.incoming });
    queryClient.invalidateQueries({ queryKey: queryKeys.correspondence.all });
  };

  const renderCorrespondenceItem = (item: CorrespondenceListItemDto) => (
    <div
      key={item.correspondenceID}
      className="flex items-center justify-between py-2.5 border-b border-border last:border-0 cursor-pointer hover:bg-muted/50 rounded-lg px-3 transition-colors"
      onClick={() => navigate(`/correspondence/${item.correspondenceID}`)}
    >
      <div className="flex-1 min-w-0">
        <Text variant="body" className="font-bold truncate">{item.subject}</Text>
        <Text variant="caption" className="truncate">
          {item.referenceNumber} — {item.typeName}
        </Text>
      </div>
      <div className="flex items-center gap-2 shrink-0 mr-3">
        <span className="text-[10px] text-muted-foreground">
          {new Date(item.correspondenceDate).toLocaleDateString("ar-EG")}
        </span>
        <Badge variant="outline" className="text-[10px]">
          {item.typeName}
        </Badge>
      </div>
    </div>
  );

  return (
    <PageLayout className="text-start">
      <DashboardHeader
        title="مكتب المدير العام"
        description="إدارة المراسلات الواردة والصادرة من مكتب المدير العام — السكرتارية."
        icon={<LayoutDashboard className="w-6 h-6" />}
        actions={
          <div className="flex gap-2">
            <Button
              variant="outline"
              className="gap-2 rounded-xl h-11 font-bold"
              onClick={() => navigate("/correspondence/outgoing")}
            >
              <Send className="w-4 h-4" /> صادر المكتب
            </Button>
            <Button
              className="gap-2 rounded-xl h-11 font-bold"
              onClick={() => navigate("/correspondence/incoming")}
            >
              <Inbox className="w-4 h-4" /> وارد المكتب
            </Button>
          </div>
        }
      />

      {/* ─── Quick Stats ─────────────────────────── */}
      <div className="grid grid-cols-2 md:grid-cols-3 gap-3 sm:gap-5">
        <StatCard
          title="الصادرة من المكتب"
          value={totalOutgoing}
          icon={<Send className="w-6 h-6" />}
          variant="blue"
          isLoading={outgoingLoading}
        />
        <StatCard
          title="الواردة إلى المكتب"
          value={totalIncoming}
          icon={<Inbox className="w-6 h-6" />}
          variant="emerald"
          isLoading={incomingLoading}
        />
        <StatCard
          title="الإجمالي"
          value={totalOutgoing + totalIncoming}
          icon={<FileText className="w-6 h-6" />}
          variant="amber"
          isLoading={isLoading}
        />
      </div>

      {/* ─── Recent Correspondence ───────────────── */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card className="p-6">
          <div className="flex items-center justify-between mb-4">
            <Heading level="h3" className="text-lg font-bold flex items-center gap-2">
              <Send className="w-5 h-5 text-blue-500" /> آخر الصادرة من المكتب
            </Heading>
            <div className="flex gap-2">
              <Button
                size="sm"
                className="gap-1 text-xs"
                onClick={() => openCreateForm(CorrespondenceDirection.Outgoing)}
              >
                <Plus className="w-3 h-3" /> صادر جديد
              </Button>
         
            </div>
          </div>
          <div className="space-y-0">
            {outgoing?.data?.items?.length === 0 && (
              <Text variant="description" className="text-center py-6">لا توجد مراسلات صادرة من المكتب</Text>
            )}
            {outgoing?.data?.items?.map(renderCorrespondenceItem)}
          </div>
        </Card>

        <Card className="p-6">
          <div className="flex items-center justify-between mb-4">
            <Heading level="h3" className="text-lg font-bold flex items-center gap-2">
              <Inbox className="w-5 h-5 text-emerald-500" /> آخر الواردة إلى المكتب
            </Heading>
            <div className="flex gap-2">
              <Button
                size="sm"
                className="gap-1 text-xs"
                onClick={() => openCreateForm(CorrespondenceDirection.Incoming)}
              >
                <Plus className="w-3 h-3" /> وارد جديد
              </Button>
       
            </div>
          </div>
          <div className="space-y-0">
            {incoming?.data?.items?.length === 0 && (
              <Text variant="description" className="text-center py-6">لا توجد مراسلات واردة إلى المكتب</Text>
            )}
            {incoming?.data?.items?.map(renderCorrespondenceItem)}
          </div>
        </Card>
      </div>

      {/* ─── Form Dialog ────────────────────────── */}
      <CorrespondenceFormDialog
        open={formOpen}
        onOpenChange={setFormOpen}
        direction={formDirection}
        onSaved={handleSaved}
      />
    </PageLayout>
  );
}
