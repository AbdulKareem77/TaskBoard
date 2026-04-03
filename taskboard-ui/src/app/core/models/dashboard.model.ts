import { TaskItem } from './task-item.model';

export interface RecentActivity {
  taskTitle: string;
  projectName: string;
  action: string;
  userName: string;
  date: string;
}

export interface ProjectSummary {
  projectId: string;
  projectName: string;
  totalTasks: number;
  completedTasks: number;
}

export interface MyTasks {
  todo: TaskItem[];
  inProgress: TaskItem[];
  review: TaskItem[];
  done: TaskItem[];
}

export interface Dashboard {
  myTasks: MyTasks;
  recentActivity: RecentActivity[];
  projectSummaries: ProjectSummary[];
}
