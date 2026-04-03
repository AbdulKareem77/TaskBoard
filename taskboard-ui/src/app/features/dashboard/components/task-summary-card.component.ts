import { Component, Input } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { TaskItem } from '../../../core/models/task-item.model';

@Component({
  selector: 'app-task-summary-card',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatChipsModule, MatIconModule, DatePipe],
  template: `
    <mat-card class="task-card" [routerLink]="['/projects', task.projectId, 'tasks', task.id]">
      <mat-card-content>
        <div class="task-header">
          <span class="task-title">{{ task.title }}</span>
          <span class="status-chip status-{{ task.status | lowercase }}">
            {{ task.status }}
          </span>
        </div>
        <div class="task-meta" *ngIf="task.dueDate">
          <mat-icon class="meta-icon">event</mat-icon>
          <span>{{ task.dueDate | date:'mediumDate' }}</span>
        </div>
        <div class="task-meta" *ngIf="task.priority">
          <mat-icon class="meta-icon">flag</mat-icon>
          <span class="priority-chip priority-{{ task.priority | lowercase }}">{{ task.priority }}</span>
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .task-card {
      margin-bottom: 8px;
      cursor: pointer;
      transition: box-shadow 0.2s ease;

      &:hover {
        box-shadow: 0 4px 12px rgba(0,0,0,0.15) !important;
      }
    }

    .task-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 8px;
      margin-bottom: 8px;
    }

    .task-title {
      font-size: 14px;
      font-weight: 500;
      line-height: 1.4;
      flex: 1;
    }

    .task-meta {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 12px;
      color: #666;
      margin-top: 4px;
    }

    .meta-icon {
      font-size: 14px;
      width: 14px;
      height: 14px;
    }
  `]
})
export class TaskSummaryCardComponent {
  @Input({ required: true }) task!: TaskItem;
}
