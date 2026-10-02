export interface SalaryBatch {
  batchID: number;
  bankName?: string;
  batchNumber?: string;
  batchMonth: string;
  totalCount: number;
  totalAmount: number;
  status: string;
  createdAt: string;
  approvedAt?: string;
  approvedByUserName?: string;
  importedByUserName?: string;
}

export interface SalaryBatchItem {
  itemID: number;
  batchID: number;
  fileNumber: string;
  retireeName: string;
  amount: number;
  status: string;
  paymentBookNumber?: string;
  recipientType?: string;
  transferNumber?: string;
  sendingBranch?: string;
  receivingBranch?: string;
  currency?: string;
  senderName?: string;
  sentDate?: string;
  receivedDate?: string;
  salaryStatus?: string;
}

export interface CreateSalaryBatchDto {
  batchMonth: string;
  bankID?: number;
  notes?: string;
}
