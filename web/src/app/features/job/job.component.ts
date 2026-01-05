import { Component } from '@angular/core';

@Component({
    standalone: true,
    selector: 'app-job',
    template: `
    <div class="layout">
      <h2 class="section-title">Jobs</h2>
      <div class="card">
        <p class="text-muted">Add job descriptions or upload images for OCR extraction.</p>
        <button class="button" type="button">Add Job</button>
      </div>
    </div>
  `
})
export class JobComponent { }
