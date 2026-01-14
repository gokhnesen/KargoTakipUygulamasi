import { BaseEntity } from "./BaseEntity";
import { Cargo } from "./Cargo";
import { CargoStatus } from "./CargoStatus";

export interface CargoMovement extends BaseEntity {
status: CargoStatus;
statusText: any;
date: string|number|Date;
  cargoId: string;
  newStatus: CargoStatus;
  location: string;
  description: string;
  cargo?: Cargo;
}