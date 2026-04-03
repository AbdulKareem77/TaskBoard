import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ReportRequestResponse {
  reportId: string;
  status: string;
}

export interface ReportResult {
  reportId: string;
  status: string;
  data?: unknown;
  generatedAt?: string;
}

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  requestProjectSummary(projectId: string): Observable<ReportRequestResponse> {
    return this.http.post<ReportRequestResponse>(`${this.apiUrl}/reports/project-summary`, { projectId });
  }

  getReport(reportId: string): Observable<ReportResult> {
    return this.http.get<ReportResult>(`${this.apiUrl}/reports/${reportId}`);
  }
}
