import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Project, ProjectDetail } from '../models/project.model';

export interface CreateProjectRequest {
  name: string;
  description?: string | null;
}

export interface AddMemberRequest {
  email: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getProjects(includeArchived = false): Observable<Project[]> {
    return this.http.get<Project[]>(`${this.apiUrl}/projects`, {
      params: { includeArchived: includeArchived.toString() }
    });
  }

  getProject(id: string): Observable<ProjectDetail> {
    return this.http.get<ProjectDetail>(`${this.apiUrl}/projects/${id}`);
  }

  createProject(request: CreateProjectRequest): Observable<Project> {
    return this.http.post<Project>(`${this.apiUrl}/projects`, request);
  }

  addMember(projectId: string, request: AddMemberRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/projects/${projectId}/members`, request);
  }

  removeMember(projectId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/projects/${projectId}/members/${userId}`);
  }

  archiveProject(projectId: string): Observable<Project> {
    return this.http.post<Project>(`${this.apiUrl}/projects/${projectId}/archive`, {});
  }

  unarchiveProject(projectId: string): Observable<Project> {
    return this.http.post<Project>(`${this.apiUrl}/projects/${projectId}/unarchive`, {});
  }
}
