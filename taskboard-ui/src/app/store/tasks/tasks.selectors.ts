import { createFeatureSelector, createSelector } from '@ngrx/store';
import { TasksState } from './tasks.reducer';

export const selectTasksState = createFeatureSelector<TasksState>('tasks');

export const selectTasks = createSelector(
  selectTasksState,
  state => state.tasks
);

export const selectSelectedTask = createSelector(
  selectTasksState,
  state => state.selectedTask
);

export const selectTasksLoading = createSelector(
  selectTasksState,
  state => state.isLoading
);

export const selectTasksError = createSelector(
  selectTasksState,
  state => state.error
);

export const selectTaskItems = createSelector(
  selectTasks,
  pagedResult => pagedResult?.items ?? []
);
