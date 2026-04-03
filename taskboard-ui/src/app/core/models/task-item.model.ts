export interface Assignee {
  userId: string;
  fullName: string;
}

export interface TaskHistoryItem {
  action: string;
  oldValue: string | null;
  newValue: string | null;
  userName: string;
  date: string;
}

export interface TaskItem {
  id: string;
  projectId: string;
  title: string;
  status: string;
  priority: string | null;
  dueDate: string | null;
  assignees: Assignee[];
  dateCreated: string;
  dateUpdated: string;
}

export interface TaskItemDetail extends TaskItem {
  description: string | null;
  createdBy: { userId: string; fullName: string };
  history: TaskHistoryItem[];
  rowVersion: number;
}
