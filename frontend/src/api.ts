import type { Account, ProblemDetails, Transaction, TransferRequest } from './types';

export class ApiError extends Error {
  constructor(message: string, public status: number) {
    super(message);
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (response.ok) {
    return response.json() as Promise<T>;
  }

  let message = `Request failed (${response.status})`;
  try {
    const problem = (await response.json()) as ProblemDetails;
    message = problem.detail ?? problem.title ?? message;
  } catch {
    // response body was not JSON; keep the generic message
  }
  throw new ApiError(message, response.status);
}

export function getAccounts(): Promise<Account[]> {
  return fetch('/api/accounts').then((r) => handleResponse<Account[]>(r));
}

export function getTransactions(): Promise<Transaction[]> {
  return fetch('/api/transactions').then((r) => handleResponse<Transaction[]>(r));
}

export function createTransfer(request: TransferRequest): Promise<unknown> {
  return fetch('/api/transfers', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  }).then((r) => handleResponse<unknown>(r));
}
