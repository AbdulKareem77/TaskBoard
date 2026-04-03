import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { RecentActivity } from '../../../core/models/dashboard.model';
import { TimeAgoPipe } from '../../../shared/pipes/time-ago.pipe';

@Component({
  selector: 'app-recent-activity',
  standalone: true,
  imports: [CommonModule, MatListModule, MatIconModule, MatDividerModule, TimeAgoPipe],
  template: `
    <mat-list>
      <ng-container *ngFor="let activity of activities; let last = last">
        <mat-list-item>
          <mat-icon matListItemIcon>history</mat-icon>
          <div matListItemTitle class="activity-title">
            <strong>{{ activity.userName }}</strong> {{ activity.action }}
            <em>{{ activity.taskTitle }}</em>
          </div>
          <div matListItemLine class="activity-meta">
            {{ activity.projectName }} &bull; {{ activity.date | timeAgo }}
          </div>
        </mat-list-item>
        <mat-divider *ngIf="!last"></mat-divider>
      </ng-container>
      <mat-list-item *ngIf="activities.length === 0">
        <div matListItemTitle>No recent activity</div>
      </mat-list-item>
    </mat-list>
  `,
  styles: [`
    .activity-title {
      font-size: 14px;
      line-height: 1.4;
    }

    .activity-meta {
      font-size: 12px;
      color: #888;
    }
  `]
})
export class RecentActivityComponent {
  @Input({ required: true }) activities: RecentActivity[] = [];
}
