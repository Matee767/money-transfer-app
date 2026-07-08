export interface Account {
  id: number;
  owner: string;
  balance: number;
}

export interface Transaction {
  id: number;
  fromAccountId: number;
  fromOwner: string;
  toAccountId: number;
  toOwner: string;
  amount: number;
  createdAtUtc: string;
}

export interface TransferRequest {
  fromAccountId: number;
  toAccountId: number;
  amount: number;
}

/** RFC 7807 problem details returned by the API on failures. */
export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
}
