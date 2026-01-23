import { BaseEntity } from "./BaseEntity";
import { CargoMovement } from "./CargoMovement";
import { CargoReceiver } from "./CargoReceiver";
import { CargoSender } from "./CargoSender";
import { CargoStatus } from "./CargoStatus";

export interface Cargo extends BaseEntity {
  currentLocation?: string;
  trackingNumber: string;
  sender?: CargoSender;
  senderId?: string;
  receiver?: CargoReceiver;
  receiverId?: string;
  movements?: CargoMovement[];  // Kargo hareketleri listesi
  status: CargoStatus;
  senderName?: string;
  senderAddress?: string;
  senderPhoneNumber?: string;
  receiverName?: string;
  receiverSurname?: string;
  receiverAddress?: string;
  receiverPhoneNumber?: string;
  receiverEmail?: string;
}