import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';

export interface ConfirmDeleteDialogData {
  taskTitle: string;
}

@Component({
  selector: 'app-confirm-delete-dialog',
  imports: [MatButtonModule, MatDialogModule],
  templateUrl: './confirm-delete-dialog.html',
})
export class ConfirmDeleteDialog {
  private readonly dialogRef = inject(MatDialogRef<ConfirmDeleteDialog>);
  readonly data = inject<ConfirmDeleteDialogData>(MAT_DIALOG_DATA);

  confirm(): void {
    this.dialogRef.close(true);
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
