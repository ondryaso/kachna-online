// reservation.component.ts
// Author: František Nečas

import { Component, Input, OnInit } from '@angular/core';
import { Reservation } from "../../../models/board-games/reservation-model";
import { formatDate } from "@angular/common";
import { MadeByUser } from "../../../models/users/made-by-user-model";

@Component({
  selector: '[app-reservation]',
  templateUrl: './reservation.component.html',
  styleUrls: ['./reservation.component.css']
})
export class ReservationComponent implements OnInit {
  @Input() reservation: Reservation;
  formattedDate: string = "";
  formattedNote: string = "";
  assignedUsers: Set<MadeByUser> = new Set();
  shownNoteChars: number = 128
  noteTooLong: boolean = false;

  constructor() {
  }

  ngOnInit(): void {
    this.formattedDate = formatDate(this.reservation.madeOn, "d. M. y", "cs-CZ");
    this.formattedNote = this.reservation.noteUser.substr(0, this.shownNoteChars);
    if (this.reservation.noteUser.length > this.shownNoteChars) {
      this.noteTooLong = true;
      this.formattedNote += "...";
    }
    for (let reservationItem of this.reservation.items) {
      if (reservationItem.assignedTo !== null) {
        this.assignedUsers.add(reservationItem.assignedTo);
      }
    }
  }
}
