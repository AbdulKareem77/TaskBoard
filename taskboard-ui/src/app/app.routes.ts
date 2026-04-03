import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { LoginComponent } from './features/login/login.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { ProjectListComponent } from './features/projects/project-list/project-list.component';
import { ProjectDetailComponent } from './features/projects/project-detail/project-detail.component';
import { TaskListComponent } from './features/tasks/task-list/task-list.component';
import { TaskDetailComponent } from './features/tasks/task-detail/task-detail.component';
import { TaskBoardComponent } from './features/tasks/task-board/task-board.component';
import { NotificationListComponent } from './features/notifications/notification-list.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'projects', component: ProjectListComponent },
      { path: 'projects/:projectId', component: ProjectDetailComponent },
      { path: 'projects/:projectId/tasks', component: TaskListComponent },
      { path: 'projects/:projectId/tasks/:taskId', component: TaskDetailComponent },
      { path: 'projects/:projectId/board', component: TaskBoardComponent },
      { path: 'notifications', component: NotificationListComponent }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
