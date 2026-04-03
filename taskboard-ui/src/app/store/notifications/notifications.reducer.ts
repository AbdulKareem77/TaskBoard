import { createReducer, on } from '@ngrx/store';
import { NotificationsResponse } from '../../core/models/notification.model';
import * as NotificationsActions from './notifications.actions';

export interface NotificationsState {
  notifications: NotificationsResponse | null;
  unreadCount: number;
  isLoading: boolean;
  error: string | null;
}

const initialState: NotificationsState = {
  notifications: null,
  unreadCount: 0,
  isLoading: false,
  error: null
};

export const notificationsReducer = createReducer(
  initialState,
  on(NotificationsActions.loadNotifications, state => ({
    ...state,
    isLoading: true,
    error: null
  })),
  on(NotificationsActions.loadNotificationsSuccess, (state, { response }) => ({
    ...state,
    notifications: response,
    unreadCount: response.unreadCount,
    isLoading: false,
    error: null
  })),
  on(NotificationsActions.loadNotificationsFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),
  on(NotificationsActions.markAsRead, state => ({ ...state, isLoading: true })),
  on(NotificationsActions.markAsReadSuccess, (state, { id }) => ({
    ...state,
    isLoading: false,
    notifications: state.notifications
      ? {
          ...state.notifications,
          items: state.notifications.items.map(n =>
            n.id === id ? { ...n, isRead: true } : n
          ),
          unreadCount: Math.max(0, state.notifications.unreadCount - 1)
        }
      : null,
    unreadCount: Math.max(0, state.unreadCount - 1)
  })),
  on(NotificationsActions.markAsReadFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  }))
);
