import { BaseEntity } from "./BaseEntity";

export interface CargoReceiver extends BaseEntity {
  name: string;
  surname: string;
  fullAddress: string;
  phoneNumber: string;
  email: string;
}