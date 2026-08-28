import { AfterViewInit, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';

@Component({
  selector: 'app-confirm-delete-dialog',
  templateUrl: './confirm-delete-dialog.html',
})
export class ConfirmDeleteDialog implements AfterViewInit {
  @Input() taskTitle = '';
  @Output() readonly confirmed = new EventEmitter<void>();
  @Output() readonly cancelled = new EventEmitter<void>();

  @ViewChild('dialogEl') private readonly dialogEl!: ElementRef<HTMLDialogElement>;

  ngAfterViewInit(): void {
    this.dialogEl.nativeElement.showModal();
  }

  close(): void {
    this.dialogEl.nativeElement.close();
  }

  confirm(): void {
    this.confirmed.emit();
    this.close();
  }

  cancel(): void {
    this.cancelled.emit();
    this.close();
  }
}
