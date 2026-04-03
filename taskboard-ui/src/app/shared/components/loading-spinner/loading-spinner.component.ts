import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Observable } from 'rxjs';
import { LoaderService } from '../../../core/services/loader.service';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule, MatProgressBarModule],
  template: `
    <div class="loading-overlay" *ngIf="isLoading$ | async">
      <mat-progress-bar mode="indeterminate" color="accent"></mat-progress-bar>
    </div>
  `,
  styles: [`
    .loading-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      z-index: 9999;
    }
  `]
})
export class LoadingSpinnerComponent implements OnInit {
  isLoading$!: Observable<boolean>;

  constructor(private readonly loaderService: LoaderService) {}

  ngOnInit(): void {
    this.isLoading$ = this.loaderService.isLoading$;
  }
}
