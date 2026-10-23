import { useQuery } from "@tanstack/react-query";

import { notificationsApi } from "@/api/notifications";
import { queryKeys } from "@/constants/queryKeys";
import { useApiMutation } from "@/hooks/useApiMutation";
import { useAuthStore } from "@/stores/authStore";
import { useNotificationStore } from "@/stores/useNotificationStore";

/**
 * مصدر واحد لبيانات الإشعارات من الخادم (NF2).
 * مفاتيح الاستعلام تحوي userID لعزل استجابات الحسابات المختلفة.
 * Zustand يحصر في طابور التنبيه وحالة الجلسة — لا ملكية موازية للقائمة أو العدّاد.
 */

export function useNotificationsList(page = 1, pageSize = 20, category?: string) {
  const userId = useAuthStore((s) => s.user?.id) ?? 0;
  const connectionStatus = useNotificationStore(s => s.connectionStatus);

  return useQuery({
    queryKey: queryKeys.notifications.list(userId, page, pageSize, category),
    queryFn: ({ signal }) =>
      notificationsApi.getAll(page, pageSize, category || undefined, signal),
    enabled: userId > 0,
    placeholderData: (prev, previousQuery) => previousQuery?.queryKey[2] === userId ? prev : undefined,
    refetchOnWindowFocus: 'always',
    refetchOnReconnect: 'always',
    refetchInterval: connectionStatus === 'connected' ? false : 30000,
  });
}

export function useUnreadCount() {
  const userId = useAuthStore((s) => s.user?.id) ?? 0;
  const connectionStatus = useNotificationStore(s => s.connectionStatus);

  return useQuery({
    queryKey: queryKeys.notifications.unread(userId),
    queryFn: ({ signal }) => notificationsApi.getUnreadCount(signal),
    enabled: userId > 0,
    refetchOnWindowFocus: 'always',
    refetchOnReconnect: 'always',
    refetchInterval: connectionStatus === 'connected' ? false : 30000,
  });
}

/** قراءة إشعار واحد — يحدّث القائمة والعدّاد عبر إبطال الاستعلامات */
export function useMarkAsRead() {
  const userId = useAuthStore(s => s.user?.id) ?? 0;
  return useApiMutation(
    async (id: number) => {
      const result = await notificationsApi.markAsRead(id);
      if (!result.isSuccess) throw new Error(result.message || 'تعذر تحديث الإشعار');
      return result;
    },
    {
      disableSuccessToast: true,
      invalidateKeys: [queryKeys.notifications.byUser(userId)],
    }
  );
}

/** قراءة الكل — يحدّث كل الأسطح من نفس المصدر */
export function useMarkAllAsRead() {
  const userId = useAuthStore(s => s.user?.id) ?? 0;
  return useApiMutation(
    async () => {
      const result = await notificationsApi.markAllAsRead();
      if (!result.isSuccess) throw new Error(result.message || 'تعذر تحديث الإشعارات');
      return result;
    },
    {
      successMessage: "تم تحديد جميع الإشعارات كمقروءة",
      invalidateKeys: [queryKeys.notifications.byUser(userId)],
    }
  );
}
