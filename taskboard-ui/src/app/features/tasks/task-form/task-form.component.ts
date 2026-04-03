import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { Store } from '@ngrx/store';
import { AppState } from '../../../store/app.state';
import { createTask, updateTask } from '../../../store/tasks/tasks.actions';
import { TaskItemDetail } from '../../../core/models/task-item.model';

export interface TaskFormData {
  projectId: string;
  task?: TaskItemDetail;
}

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './task-form.component.html',
  styleUrls: ['./task-form.component.scss']
})
export class TaskFormComponent implements OnInit {
  form!: FormGroup;
  isEditMode = false;

  readonly statusOptions = ['Todo', 'InProgress', 'Review', 'Done'];

  constructor(
    private readonly fb: FormBuilder,
    private readonly store: Store<AppState>,
    private readonly dialogRef: MatDialogRef<TaskFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: TaskFormData
  ) {}

  ngOnInit(): void {
    this.isEditMode = !!this.data.task;
    const task = this.data.task;

    this.form = this.fb.group({
      title: [task?.title ?? '', [Validators.required, Validators.maxLength(500)]],
      description: [task?.description ?? ''],
      status: [task?.status ?? 'Todo', Validators.required],
      dueDate: [task?.dueDate ? new Date(task.dueDate) : null]
    });
  }

  get title() { return this.form.get('title'); }
  get status() { return this.form.get('status'); }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { title, description, status, dueDate } = this.form.value;
    const dueDateStr: string | null = dueDate
      ? (dueDate instanceof Date ? dueDate.toISOString() : dueDate)
      : null;

    if (this.isEditMode && this.data.task) {
      this.store.dispatch(updateTask({
        projectId: this.data.projectId,
        taskId: this.data.task.id,
        title,
        description: description || null,
        status,
        dueDate: dueDateStr,
        rowVersion: this.data.task.rowVersion
      }));
    } else {
      this.store.dispatch(createTask({
        projectId: this.data.projectId,
        title,
        description: description || null,
        status,
        dueDate: dueDateStr
      }));
    }

    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
