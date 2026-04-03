import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { MatSidenavModule } from '@angular/material/sidenav';
import { AppState } from './store/app.state';
import { selectIsAuthenticated } from './store/auth/auth.selectors';
import { restoreSessionSuccess } from './store/auth/auth.actions';
import { SessionService } from './core/services/session.service';
import { HeaderComponent } from './shared/components/header/header.component';
import { SidebarComponent } from './shared/components/sidebar/sidebar.component';
import { LoadingSpinnerComponent } from './shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    HeaderComponent,
    SidebarComponent,
    LoadingSpinnerComponent
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  isAuthenticated$: Observable<boolean>;

  constructor(
    private readonly store: Store<AppState>,
    private readonly sessionService: SessionService
  ) {
    this.isAuthenticated$ = this.store.select(selectIsAuthenticated);
  }

  ngOnInit(): void {
    // Restore session from localStorage on app start
    const token = this.sessionService.getToken();
    const user = this.sessionService.getUser();
    if (token && user) {
      this.store.dispatch(restoreSessionSuccess({ user, token }));
    }
  }
}
