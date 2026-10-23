import assert from 'node:assert/strict';
import { beforeEach, test } from 'node:test';

import { notificationDestination } from '../src/lib/notificationNavigation.ts';
import { MAX_PENDING_TOASTS, useNotificationStore } from '../src/stores/useNotificationStore.ts';
import type { NotificationDto } from '../src/types/notifications.ts';

function notification(id: number, userID = 1): NotificationDto {
  return { notificationID: id, userID, title: 'إشعار', message: 'نص', priority: 1, isRead: false, createdAt: '2026-10-11T10:00:00Z' };
}
const state = () => useNotificationStore.getState();
beforeEach(() => { state().reset(); state().bindUser(1); });

test('duplicate delivery is rejected even after the card is dismissed', () => {
  assert.equal(state().enqueueToast(notification(1)), true);
  state().dequeueToast(1);
  assert.equal(state().enqueueToast(notification(1)), false);
  assert.equal(state().toastQueue.length, 0);
});

test('foreign-session events are rejected without consuming their IDs', () => {
  assert.equal(state().enqueueToast(notification(1, 2)), false);
  assert.equal(state().seenIds.size, 0);
  assert.equal(state().enqueueToast(notification(1)), true);
});

test('four events stay queued until dismissal and keep arrival order', () => {
  for (let id = 1; id <= 4; id++) state().enqueueToast(notification(id));
  assert.deepEqual(state().toastQueue.map(n => n.notificationID), [1, 2, 3, 4]);
  state().dequeueToast(1);
  assert.deepEqual(state().toastQueue.map(n => n.notificationID), [2, 3, 4]);
});

test('queue overflow preserves older cards and exposes history count once', () => {
  for (let id = 1; id <= MAX_PENDING_TOASTS + 2; id++) state().enqueueToast(notification(id));
  assert.equal(state().toastQueue.length, MAX_PENDING_TOASTS);
  assert.equal(state().toastQueue[0].notificationID, 1);
  assert.equal(state().overflowCount, 2);
  state().enqueueToast(notification(MAX_PENDING_TOASTS + 1));
  assert.equal(state().overflowCount, 2);
  state().clearOverflow();
  assert.equal(state().overflowCount, 0);
});

test('same account preserves deduplication; different account clears all transient state', () => {
  state().enqueueToast(notification(1));
  state().setConnectionStatus('connected');
  state().bindUser(1);
  assert.equal(state().toastQueue.length, 1);
  state().bindUser(2);
  assert.equal(state().toastQueue.length, 0);
  assert.equal(state().seenIds.size, 0);
  assert.equal(state().connectionStatus, 'disconnected');
  assert.equal(state().enqueueToast(notification(1, 2)), true);
});

test('logout clears ownership, events and overflow', () => {
  state().enqueueToast(notification(1));
  state().reset();
  assert.equal(state().ownerId, null);
  assert.equal(state().toastQueue.length, 0);
  assert.equal(state().seenIds.size, 0);
});

test('server URL takes precedence over category and title', () => {
  assert.equal(notificationDestination({ ...notification(1), category: 'FINANCIAL', title: 'رواتب', actionUrl: '/settlements/14' }), '/settlements/14');
});

test('legacy workflow IDs work; ambiguous titles and external URLs go to history', () => {
  assert.equal(notificationDestination({ ...notification(1), category: 'WORKFLOW', relatedRecordID: 14 }), '/files/14');
  for (const actionUrl of ['https://example.com', '//example.com', '/\\example.com', 'javascript:alert(1)']) {
    assert.equal(notificationDestination({ ...notification(1), category: 'FINANCIAL', title: 'تسوية', actionUrl }), '/notifications');
  }
});