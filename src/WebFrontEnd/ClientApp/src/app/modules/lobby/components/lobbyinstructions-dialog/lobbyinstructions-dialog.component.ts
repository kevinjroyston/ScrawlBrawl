import { Component, OnInit} from '@angular/core';
import { MatLegacyDialogRef as MatDialogRef } from '@angular/material/legacy-dialog';

@Component({
  selector: 'app-lobby-instructions-dialog',
  templateUrl: './lobbyinstructions-dialog.component.html',
  styleUrls: ['./lobbyinstructions-dialog.component.scss']
})
export class LobbyInstructionsDialogComponent implements OnInit {

  constructor(private dialogRef: MatDialogRef<LobbyInstructionsDialogComponent>) {
  }

  ngOnInit() {
  }

  close() {
    this.dialogRef.close();
  }
}
