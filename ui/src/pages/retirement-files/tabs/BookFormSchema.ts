export interface BookFormValues {
  bookNumber?: string;
  bookType?: string;
  issueDate?: string;
  expiryDate?: string;
  status?: string;
  legacyColor?: string;
  changeReason?: string;
  recipientName?: string;
  recipientType?: string;
  notes?: string;
  issuingBranchID?: number | null;
  bookImage?: File | null;
  autoCreateSalaryRecipient?: boolean;
  salaryRecipientType?: string;
  salaryRecipientName?: string;
  salaryRecipientID?: number | null;
  [key: string]: any;
}
