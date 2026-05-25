import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TaskItem, TaskItemDetail } from '../models/task-item.model';
import { TaskComment } from '../models/comment.model';
import { PagedResult } from '../models/paged-result.model';

export interface CreateTaskRequest {
  title: string;
  description?: string | null;
  status?: string;
  dueDate?: string | null;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string | null;
  status: string;
  dueDate?: string | null;
  rowVersion: number;
}

export interface TaskSearchParams {
  q?: string;
  projectId?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getTasks(
    projectId: string,
    page = 1,
    pageSize = 20,
    status?: string
  ): Observable<PagedResult<TaskItem>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (status) {
      params = params.set('status', status);
    }

    return this.http.get<PagedResult<TaskItem>>(
      `${this.apiUrl}/projects/${projectId}/tasks`,
      { params }
    );
  }

  getTask(projectId: string, taskId: string): Observable<TaskItemDetail> {
    return this.http.get<TaskItemDetail>(
      `${this.apiUrl}/projects/${projectId}/tasks/${taskId}`
    );
  }

  createTask(projectId: string, request: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(
      `${this.apiUrl}/projects/${projectId}/tasks`,
      request
    );
  }

  updateTask(
    projectId: string,
    taskId: string,
    request: UpdateTaskRequest
  ): Observable<TaskItemDetail> {
    return this.http.put<TaskItemDetail>(
      `${this.apiUrl}/projects/${projectId}/tasks/${taskId}`,
      request
    );
  }

  assignTask(projectId: string, taskId: string, userId: string): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/projects/${projectId}/tasks/${taskId}/assign`,
      { userId }
    );
  }

  unassignTask(projectId: string, taskId: string, userId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/projects/${projectId}/tasks/${taskId}/assign/${userId}`
    );
  }

  deleteTask(projectId: string, taskId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/projects/${projectId}/tasks/${taskId}`
    );
  }

  getTaskComments(projectId: string, taskId: string): Observable<TaskComment[]> {
    return this.http.get<TaskComment[]>(
      `${this.apiUrl}/projects/${projectId}/tasks/${taskId}/comments`
    );
  }

  createTaskComment(
    projectId: string,
    taskId: string,
    content: string
  ): Observable<TaskComment> {
    return this.http.post<TaskComment>(
      `${this.apiUrl}/projects/${projectId}/tasks/${taskId}/comments`,
      { content }
    );
  }

  searchTasks(params: TaskSearchParams): Observable<PagedResult<TaskItem>> {
    let httpParams = new HttpParams();
    if (params.q) httpParams = httpParams.set('q', params.q);
    if (params.projectId) httpParams = httpParams.set('projectId', params.projectId);
    if (params.page) httpParams = httpParams.set('page', params.page.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());

    return this.http.get<PagedResult<TaskItem>>(`${this.apiUrl}/tasks/search`, { params: httpParams });
  }
}
