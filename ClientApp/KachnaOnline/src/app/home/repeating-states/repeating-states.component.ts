import { Component, OnInit } from '@angular/core';
import { RepeatingStatesService } from "../../shared/services/repeating-states.service";
import { RepeatingState } from "../../models/states/repeating-state.model";
import { groupBy, map, tap, toArray } from "rxjs/operators";
import { from } from "rxjs";
import { ClubStateTypes } from "../../models/states/club-state-types.model";

@Component({
  selector: 'app-repeating-states',
  templateUrl: './repeating-states.component.html',
  styleUrls: ['./repeating-states.component.css']
})
export class RepeatingStatesComponent implements OnInit {
  constructor(public service: RepeatingStatesService) { }

  states: RegularOpeningDay[];

  ngOnInit(): void {
    this.service.get().subscribe(arr =>
      from(arr).pipe(
        groupBy(e => e.dayOfWeek),
        map(e => {
          let ro = <RegularOpeningDay>{
            day: e.key,
            values: []
          };

          e.pipe(map(a => ({from: a.timeFrom, to: a.timeTo, type: a.state})),
            toArray()).subscribe(x => {
            ro.values = x;
            ro.values.sort();
          });

          return ro;
        }),
        toArray()
      ).subscribe(e => {
        this.states = e;

        const days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
        this.states.sort((a, b) => days.indexOf(a.day) - days.indexOf(b.day));
      }));
  }
}

export class RegularOpeningDay {
  day: string;
  values: {
    from: string;
    to: string;
    type: ClubStateTypes;
  }[];
}
