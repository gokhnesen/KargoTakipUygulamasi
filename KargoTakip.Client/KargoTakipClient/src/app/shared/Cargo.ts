import { CargoMovement } from "./CargoMovement";
import { CargoStatus } from "./CargoStatus";

export interface Cargo {
  id: string;
  trackingNumber: string;
  senderName: string;
  senderAddress: string;
  senderPhoneNumber: string;
  receiverName: string;
  receiverSurname: string;
  receiverAddress: string;
  receiverPhoneNumber: string;
  receiverEmail: string;
  status: CargoStatus;
  createdDate: Date;
  updatedDate?: Date;
  /** API liste cevabinda doner (movements yerine). */
  lastLocation?: string;
  currentLocation?: string;
  movements?: CargoMovement[];
}
