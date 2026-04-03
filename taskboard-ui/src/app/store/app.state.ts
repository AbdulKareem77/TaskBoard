import { ActionReducerMap } from '@ngrx/store';
import { AuthState, authReducer } from './auth/auth.reducer';
import { ProjectsState, projectsReducer } from './projects/projects.reducer';
import { TasksState, tasksReducer } from './tasks/tasks.reducer';
import { NotificationsState, notificationsReducer } from './notifications/notifications.reducer';

export interface AppState {
  auth: AuthState;
  projects: ProjectsState;
  tasks: TasksState;
  notifications: NotificationsState;
}

export const reducers: ActionReducerMap<AppState> = {
  auth: authReducer,
  projects: projectsReducer,
  tasks: tasksReducer,
  notifications: notificationsReducer
};
