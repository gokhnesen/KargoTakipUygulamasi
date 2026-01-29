import { CargoStatus } from './CargoStatus';

export interface CargoFilterParams {
    status?: CargoStatus;
    trackingNumber?: string;
    searchTerm?: string;
    senderName?: string;
    receiverName?: string;
    receiverEmail?: string;
    startDate?: Date;
    endDate?: Date;
    orderBy?: string;
    isDescending?: boolean;
    pageIndex?: number;
    pageSize?: number;
    includeMovements?: boolean;
}
