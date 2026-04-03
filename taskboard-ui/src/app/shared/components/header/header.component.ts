import { Component, EventEmitter, Output } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Store } from '@ngrx/store';
import { AppState } from '../../../store/app.state';
import { logout } from '../../../store/auth/auth.actions';
import { NotificationBellComponent } from '../../../features/notifications/notification-bell.component';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    NotificationBellComponent
  ],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  @Output() menuToggle = new EventEmitter<void>();

  constructor(private readonly store: Store<AppState>) {}

  onLogout(): void {
    this.store.dispatch(logout());
  }
}
