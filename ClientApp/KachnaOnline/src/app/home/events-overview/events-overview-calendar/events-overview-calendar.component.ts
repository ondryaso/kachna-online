// events-overview-calendar.component.ts
// Author: František Nečas

import { Component, OnInit, ViewChild } from '@angular/core';
import { CalendarOptions, EventClickArg, FullCalendarComponent } from "@fullcalendar/angular";
import { StatesService } from "../../../shared/services/states.service";
import { EventsService } from "../../../shared/services/events.service";
import { ClubStateTypes } from "../../../models/states/club-state-types.model";
import { ToastrService } from "ngx-toastr";
import { Router } from "@angular/router";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { StateModalComponent } from "../state-modal/state-modal.component";

@Component({
  selector: 'app-events-overview-calendar',
  templateUrl: './events-overview-calendar.component.html',
  styleUrls: ['./events-overview-calendar.component.css']
})
export class EventsOverviewCalendarComponent implements OnInit {
  @ViewChild('calendar') calendarComponent: FullCalendarComponent

  calendarOptions: CalendarOptions = {
    headerToolbar: {
      left: 'title',
      center: '',
      right: 'prev,next'
    },
    initialView: 'dayGridMonth',
    weekends: true,
    locale: 'cs-CZ',
    themeSystem: 'bootstrap',
    datesSet: this.onDateChange.bind(this),
    displayEventEnd: true,
    editable: true, // this adds cursor pointer
    eventClick: this.eventClick.bind(this)
  }

  private statePrefix = "state-"
  private eventPrefix = "event-";

  constructor(private statesService: StatesService, private eventsService: EventsService,
              private toastrService: ToastrService, private router: Router, private modalService: NgbModal) { }

  ngOnInit(): void {
  }

  onDateChange(dateInfo: {start: Date, end: Date}): void {
    this.updateCalendar(dateInfo.start, dateInfo.end);
  }

  updateCalendar(start: Date, end: Date): void {
    let api = this.calendarComponent.getApi();
    api.removeAllEvents();
    this.statesService.getBetween(start, end).subscribe(states => {
      for (let state of states) {
        let title = "";
        let color = "#2A72FF";
        if (state.state == ClubStateTypes.OpenChillzone) {
          color = "#3DA744";
          title = "Chillzóna";
        } else if (state.state == ClubStateTypes.OpenBar) {
          title = "Bar";
        }
        api.addEvent({
          id: `${this.statePrefix}${state.id}`,
          title: title,
          start: state.start,
          end: state.plannedEnd,
          display: 'block',
          color: color,
        })
      }
    }, err => {
      console.log(err);
      this.toastrService.error("Nepodařilo se načíst stavy klubu pro tento měsíc.");
    })

    this.eventsService.getBetween(start, end).subscribe(events => {
      for (let event of events) {
        api.addEvent({
          id: `${this.eventPrefix}${event.id}`,
          title: event.name,
          start: event.from,
          end: event.to,
          display: 'block',
          color: "#D5384A"
        })
      }
    })
  }

  eventClick(clickInfo: EventClickArg) {
    if (clickInfo.event.id.startsWith(this.eventPrefix)) {
      let eventId = clickInfo.event.id.replace(this.eventPrefix, "");
      this.router.navigate([`/events/${eventId}`]).then();
    } else if (clickInfo.event.id.startsWith(this.statePrefix)) {
      let stateId = clickInfo.event.id.replace(this.statePrefix, "");
      this.statesService.get(parseInt(stateId)).subscribe(state => {
        const modalRef = this.modalService.open(StateModalComponent);
        modalRef.componentInstance.state = state;
      })
    }
  }
}
