import { Component } from '@angular/core';

@Component({
    standalone: true,
    selector: 'app-cover-letter',
    template: `
    <div class="layout">
      <h2 class="section-title">Cover Letters</h2>
      <div class="card">
        <p class="text-muted">Generate, personalize, and preview tailored cover letters.</p>
        <button class="button" type="button">New Cover Letter</button>
      </div>
    </div>
  `
})
export class CoverLetterComponent { }
