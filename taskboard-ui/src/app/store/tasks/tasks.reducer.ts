import { createReducer, on } from '@ngrx/store';
import { TaskItem, TaskItemDetail } from '../../core/models/task-item.model';
import { PagedResult } from '../../core/models/paged-result.model';
import * as TasksActions from './tasks.actions';

export interface TasksState {
  tasks: PagedResult<TaskItem> | null;
  selectedTask: TaskItemDetail | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: TasksState = {
  tasks: null,
  selectedTask: null,
  isLoading: false,
  error: null
};

export const tasksReducer = createReducer(
  initialState,
  on(TasksActions.loadTasks, state => ({ ...state, isLoading: true, error: null })),
  on(TasksActions.loadTasksSuccess, (state, { pagedResult }) => ({
    ...state,
    tasks: pagedResult,
    isLoading: false,
    error: null
  })),
  on(TasksActions.loadTasksFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),
  on(TasksActions.loadTask, state => ({ ...state, isLoading: true, error: null })),
  on(TasksActions.loadTaskSuccess, (state, { task }) => ({
    ...state,
    selectedTask: task,
    isLoading: false,
    error: null
  })),
  on(TasksActions.loadTaskFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),
  on(TasksActions.createTask, state => ({ ...state, isLoading: true, error: null })),
  on(TasksActions.createTaskSuccess, (state, { task }) => ({
    ...state,
    tasks: state.tasks
      ? {
          ...state.tasks,
          items: [task, ...state.tasks.items],
          totalCount: state.tasks.totalCount + 1
        }
      : null,
    isLoading: false,
    error: null
  })),
  on(TasksActions.createTaskFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),
  on(TasksActions.updateTask, state => ({ ...state, isLoading: true, error: null })),
  on(TasksActions.updateTaskSuccess, (state, { task }) => ({
    ...state,
    selectedTask: task,
    tasks: state.tasks
      ? {
          ...state.tasks,
          items: state.tasks.items.map(t => (t.id === task.id ? task : t))
        }
      : null,
    isLoading: false,
    error: null
  })),
  on(TasksActions.updateTaskFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),
  on(TasksActions.updateTaskConflict, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),
  on(TasksActions.deleteTask, state => ({ ...state, isLoading: true, error: null })),
  on(TasksActions.deleteTaskSuccess, (state, { taskId }) => ({
    ...state,
    tasks: state.tasks
      ? {
          ...state.tasks,
          items: state.tasks.items.filter(t => t.id !== taskId),
          totalCount: state.tasks.totalCount - 1
        }
      : null,
    selectedTask: state.selectedTask?.id === taskId ? null : state.selectedTask,
    isLoading: false,
    error: null
  })),
  on(TasksActions.deleteTaskFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),
  on(TasksActions.assignTask, state => ({ ...state, isLoading: true, error: null })),
  on(TasksActions.assignTaskSuccess, state => ({ ...state, isLoading: false })),
  on(TasksActions.assignTaskFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),
  on(TasksActions.unassignTask, state => ({ ...state, isLoading: true, error: null })),
  on(TasksActions.unassignTaskSuccess, state => ({ ...state, isLoading: false })),
  on(TasksActions.unassignTaskFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),
  on(TasksActions.clearSelectedTask, state => ({ ...state, selectedTask: null }))
);
