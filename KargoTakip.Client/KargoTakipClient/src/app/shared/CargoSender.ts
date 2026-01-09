import { BaseEntity } from "./BaseEntity";

export interface CargoSender extends BaseEntity  {
  fullName: string;
  fullAddress: string;
  phoneNumber: string;
}