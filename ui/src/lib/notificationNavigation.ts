import type { NotificationDto } from '../types/notifications.ts';

/** Titles are display text, never routing instructions. */
export function notificationDestination(notification: NotificationDto): string {
  const actionUrl = notification.actionUrl?.trim();
  if (actionUrl?.startsWith('/') && !actionUrl.startsWith('//') && !actionUrl.includes('\\') && !/\s/.test(actionUrl)) return actionUrl;
  if (notification.category === 'WORKFLOW' && notification.relatedRecordID) return `/files/${notification.relatedRecordID}`;
  return '/notifications';
}
