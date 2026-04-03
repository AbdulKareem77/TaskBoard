import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { Observable, Subject } from 'rxjs';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { AppState } from '../../store/app.state';
import {
  loadNotifications,
  markAsRead
} from '../../store/notifications/notifications.actions';
import {
  selectNotifications,
  selectNotificationsLoading
} from '../../store/notifications/notifications.selectors';
import { NotificationsResponse, Notification } from '../../core/models/notification.model';
import { TimeAgoPipe } from '../../shared/pipes/time-ago.pipe';

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [
    CommonModule,
    MatListModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatBadgeModule,
    MatDividerModule,
    MatChipsModule,
    TimeAgoPipe
  ],
  templateUrl: './notification-list.component.html',
  styleUrls: ['./notification-list.component.scss']
})
export class NotificationListComponent implements OnInit, OnDestroy {
  notifications$: Observable<NotificationsResponse | null>;
  isLoading$: Observable<boolean>;

  showUnreadOnly = false;

  private readonly destroy$ = new Subject<void>();

  constructor(private readonly store: Store<AppState>) {
    this.notifications$ = this.store.select(selectNotifications);
    this.isLoading$ = this.store.select(selectNotificationsLoading);
  }

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  load(): void {
    this.store.dispatch(loadNotifications({
      unreadOnly: this.showUnreadOnly,
      page: 1,
      pageSize: 50
    }));
  }

  toggleUnreadFilter(): void {
    this.showUnreadOnly = !this.showUnreadOnly;
    this.load();
  }

  onMarkAsRead(notification: Notification): void {
    if (!notification.isRead) {
      this.store.dispatch(markAsRead({ id: notification.id }));
    }
  }

  getTypeIcon(type: string): string {
    switch (type.toLowerCase()) {
      case 'taskassigned': return 'assignment_ind';
      case 'taskupdated': return 'edit';
      case 'taskcompleted': return 'check_circle';
      case 'commentadded': return 'comment';
      case 'mentioned': return 'alternate_email';
      default: return 'notifications';
    }
  }
}
