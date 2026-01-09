import { BaseEntity } from "./BaseEntity";
import { Cargo } from "./Cargo";
import { CargoStatus } from "./CargoStatus";

export interface CargoMovement extends BaseEntity {
  cargoId: string;
  newStatus: CargoStatus;
  location: string;
  description: string;
  cargo?: Cargo;
}