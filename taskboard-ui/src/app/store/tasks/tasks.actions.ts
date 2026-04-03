import { createAction, props } from '@ngrx/store';
import { TaskItem, TaskItemDetail } from '../../core/models/task-item.model';
import { PagedResult } from '../../core/models/paged-result.model';

export const loadTasks = createAction(
  '[Tasks] Load Tasks',
  props<{ projectId: string; page?: number; pageSize?: number; status?: string }>()
);

export const loadTasksSuccess = createAction(
  '[Tasks] Load Tasks Success',
  props<{ pagedResult: PagedResult<TaskItem> }>()
);

export const loadTasksFailure = createAction(
  '[Tasks] Load Tasks Failure',
  props<{ error: string }>()
);

export const loadTask = createAction(
  '[Tasks] Load Task',
  props<{ projectId: string; taskId: string }>()
);

export const loadTaskSuccess = createAction(
  '[Tasks] Load Task Success',
  props<{ task: TaskItemDetail }>()
);

export const loadTaskFailure = createAction(
  '[Tasks] Load Task Failure',
  props<{ error: string }>()
);

export const createTask = createAction(
  '[Tasks] Create Task',
  props<{
    projectId: string;
    title: string;
    description?: string | null;
    status?: string;
    dueDate?: string | null;
  }>()
);

export const createTaskSuccess = createAction(
  '[Tasks] Create Task Success',
  props<{ task: TaskItem }>()
);

export const createTaskFailure = createAction(
  '[Tasks] Create Task Failure',
  props<{ error: string }>()
);

export const updateTask = createAction(
  '[Tasks] Update Task',
  props<{
    projectId: string;
    taskId: string;
    title: string;
    description?: string | null;
    status: string;
    dueDate?: string | null;
    rowVersion: number;
  }>()
);

export const updateTaskSuccess = createAction(
  '[Tasks] Update Task Success',
  props<{ task: TaskItemDetail }>()
);

export const updateTaskFailure = createAction(
  '[Tasks] Update Task Failure',
  props<{ error: string }>()
);

export const updateTaskConflict = createAction(
  '[Tasks] Update Task Conflict',
  props<{ error: string }>()
);

export const deleteTask = createAction(
  '[Tasks] Delete Task',
  props<{ projectId: string; taskId: string }>()
);

export const deleteTaskSuccess = createAction(
  '[Tasks] Delete Task Success',
  props<{ taskId: string }>()
);

export const deleteTaskFailure = createAction(
  '[Tasks] Delete Task Failure',
  props<{ error: string }>()
);

export const assignTask = createAction(
  '[Tasks] Assign Task',
  props<{ projectId: string; taskId: string; userId: string }>()
);

export const assignTaskSuccess = createAction('[Tasks] Assign Task Success');

export const assignTaskFailure = createAction(
  '[Tasks] Assign Task Failure',
  props<{ error: string }>()
);

export const unassignTask = createAction(
  '[Tasks] Unassign Task',
  props<{ projectId: string; taskId: string; userId: string }>()
);

export const unassignTaskSuccess = createAction('[Tasks] Unassign Task Success');

export const unassignTaskFailure = createAction(
  '[Tasks] Unassign Task Failure',
  props<{ error: string }>()
);

export const clearSelectedTask = createAction('[Tasks] Clear Selected Task');
