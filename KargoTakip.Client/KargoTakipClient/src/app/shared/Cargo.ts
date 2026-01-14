import { BaseEntity } from "./BaseEntity";
import { CargoMovement } from "./CargoMovement";
import { CargoReceiver } from "./CargoReceiver";
import { CargoSender } from "./CargoSender";
import { CargoStatus } from "./CargoStatus";


export interface Cargo extends BaseEntity {
currentLocation: any;
  trackingNumber: string;
  sender: CargoSender;
  senderId: string;
  receiver: CargoReceiver;
  receiverId: string;
  movements: CargoMovement[];
  status: CargoStatus;
}