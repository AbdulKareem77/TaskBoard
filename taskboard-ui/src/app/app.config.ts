import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { routes } from './app.routes';
import { reducers } from './store/app.state';
import { AuthEffects } from './store/auth/auth.effects';
import { ProjectsEffects } from './store/projects/projects.effects';
import { TasksEffects } from './store/tasks/tasks.effects';
import { NotificationsEffects } from './store/notifications/notifications.effects';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { loaderInterceptor } from './core/interceptors/loader.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, loaderInterceptor])),
    provideAnimations(),
    provideStore(reducers),
    provideEffects([AuthEffects, ProjectsEffects, TasksEffects, NotificationsEffects]),
    provideStoreDevtools({ maxAge: 25, logOnly: false })
  ]
};
