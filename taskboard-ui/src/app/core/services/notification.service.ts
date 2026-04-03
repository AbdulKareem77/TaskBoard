import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { NotificationsResponse } from '../models/notification.model';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getNotifications(
    unreadOnly = false,
    page = 1,
    pageSize = 20
  ): Observable<NotificationsResponse> {
    const params = new HttpParams()
      .set('unreadOnly', unreadOnly.toString())
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<NotificationsResponse>(`${this.apiUrl}/notifications`, { params });
  }

  markAsRead(id: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/notifications/${id}/read`, {});
  }
}
