import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ProjectMember } from '../../../../core/models/project.model';

@Component({
  selector: 'app-member-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule
  ],
  template: `
    <table mat-table [dataSource]="members" class="full-width">
      <ng-container matColumnDef="name">
        <th mat-header-cell *matHeaderCellDef>Name</th>
        <td mat-cell *matCellDef="let member">
          {{ member.firstName }} {{ member.lastName }}
        </td>
      </ng-container>

      <ng-container matColumnDef="email">
        <th mat-header-cell *matHeaderCellDef>Email</th>
        <td mat-cell *matCellDef="let member">{{ member.email }}</td>
      </ng-container>

      <ng-container matColumnDef="role">
        <th mat-header-cell *matHeaderCellDef>Role</th>
        <td mat-cell *matCellDef="let member">
          <span class="role-chip role-{{ member.role | lowercase }}">{{ member.role }}</span>
        </td>
      </ng-container>

      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef></th>
        <td mat-cell *matCellDef="let member">
          <button
            mat-icon-button
            color="warn"
            (click)="removeMember.emit(member.userId)"
            matTooltip="Remove member"
          >
            <mat-icon>person_remove</mat-icon>
          </button>
        </td>
      </ng-container>

      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>

      <tr class="mat-row" *matNoDataRow>
        <td class="mat-cell empty-cell" [attr.colspan]="displayedColumns.length">
          No members yet
        </td>
      </tr>
    </table>
  `,
  styles: [`
    .full-width { width: 100%; }
    .role-chip {
      padding: 2px 10px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 500;
    }
    .role-owner { background: #e3f2fd; color: #1565c0; }
    .role-admin { background: #fce4ec; color: #c62828; }
    .role-member { background: #e8f5e9; color: #2e7d32; }
    .role-viewer { background: #f5f5f5; color: #616161; }
    .empty-cell { padding: 24px; text-align: center; color: #666; }
  `]
})
export class MemberListComponent {
  @Input({ required: true }) members: ProjectMember[] = [];
  @Output() removeMember = new EventEmitter<string>();

  displayedColumns = ['name', 'email', 'role', 'actions'];
}
