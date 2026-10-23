import { useCallback } from 'react';
import { useNavigate } from 'react-router-dom';

import { useMarkAsRead } from '@/hooks/useNotifications';
import { notificationDestination } from '@/lib/notificationNavigation';
import type { NotificationDto } from '@/types/notifications';

export function useOpenNotification() {
  const navigate = useNavigate();
  const { mutate } = useMarkAsRead();
  return useCallback((notification: NotificationDto) => {
    if (!notification.isRead) mutate(notification.notificationID);
    navigate(notificationDestination(notification));
  }, [mutate, navigate]);
}
