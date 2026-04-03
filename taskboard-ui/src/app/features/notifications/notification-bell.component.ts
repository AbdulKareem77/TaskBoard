import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatBadgeModule } from '@angular/material/badge';
import { AppState } from '../../store/app.state';
import { selectUnreadCount } from '../../store/notifications/notifications.selectors';
import { loadNotifications } from '../../store/notifications/notifications.actions';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatBadgeModule],
  template: `
    <button
      mat-icon-button
      (click)="navigateToNotifications()"
      [matBadge]="(unreadCount$ | async) || null"
      matBadgeColor="warn"
      [matBadgeHidden]="(unreadCount$ | async) === 0"
      aria-label="Notifications"
    >
      <mat-icon>notifications</mat-icon>
    </button>
  `,
  styles: [`
    :host ::ng-deep .mat-badge-content {
      top: 4px !important;
      right: 4px !important;
    }
  `]
})
export class NotificationBellComponent implements OnInit {
  unreadCount$: Observable<number>;

  constructor(
    private readonly store: Store<AppState>,
    private readonly router: Router
  ) {
    this.unreadCount$ = this.store.select(selectUnreadCount);
  }

  ngOnInit(): void {
    this.store.dispatch(loadNotifications({ unreadOnly: true, page: 1, pageSize: 1 }));
  }

  navigateToNotifications(): void {
    this.router.navigate(['/notifications']);
  }
}
