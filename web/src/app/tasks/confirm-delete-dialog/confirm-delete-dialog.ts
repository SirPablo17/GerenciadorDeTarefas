import { AfterViewInit, Component, ElementRef, ViewChild, input, output } from '@angular/core';

@Component({
  selector: 'app-confirm-delete-dialog',
  templateUrl: './confirm-delete-dialog.html',
})
export class ConfirmDeleteDialog implements AfterViewInit {
  readonly taskTitle = input('');
  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

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
