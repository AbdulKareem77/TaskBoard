import { createAction, props } from '@ngrx/store';
import { NotificationsResponse } from '../../core/models/notification.model';

export const loadNotifications = createAction(
  '[Notifications] Load Notifications',
  props<{ unreadOnly?: boolean; page?: number; pageSize?: number }>()
);

export const loadNotificationsSuccess = createAction(
  '[Notifications] Load Notifications Success',
  props<{ response: NotificationsResponse }>()
);

export const loadNotificationsFailure = createAction(
  '[Notifications] Load Notifications Failure',
  props<{ error: string }>()
);

export const markAsRead = createAction(
  '[Notifications] Mark As Read',
  props<{ id: string }>()
);

export const markAsReadSuccess = createAction(
  '[Notifications] Mark As Read Success',
  props<{ id: string }>()
);

export const markAsReadFailure = createAction(
  '[Notifications] Mark As Read Failure',
  props<{ error: string }>()
);
