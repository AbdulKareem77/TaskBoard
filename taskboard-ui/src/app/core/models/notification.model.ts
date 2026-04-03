export interface Notification {
  id: string;
  type: string;
  title: string;
  message: string;
  referenceId: string | null;
  isRead: boolean;
  dateCreated: string;
}

export interface NotificationsResponse {
  items: Notification[];
  totalCount: number;
  unreadCount: number;
}
