// event.model.ts
// Author: David Chocholatý

export class Event {
  id: number = -1;
  name: string = "";
  place: string = "";
  placeUrl: string = "";
  imageUrl: string = "";
  shortDescription: string = "";
  fullDescription: string = "";
  url: string = "";
  from: Date;
  to: Date;
  linkedPlannedStateIds: number[] | null;
  madeById: string = "";
}
