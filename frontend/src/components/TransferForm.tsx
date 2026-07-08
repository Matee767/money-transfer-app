import { FormEvent, useState } from 'react';
import { createTransfer } from '../api';
import type { Account } from '../types';

interface Props {
  accounts: Account[];
  /** Called after a successful transfer so the parent can refresh data. */
  onTransferCompleted: () => void;
}

export function TransferForm({ accounts, onTransferCompleted }: Props) {
  const [fromAccountId, setFromAccountId] = useState('');
  const [toAccountId, setToAccountId] = useState('');
  const [amount, setAmount] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);
    setSuccess(null);

    const parsedAmount = Number(amount);
    if (!fromAccountId || !toAccountId) {
      setError('Please choose both accounts.');
      return;
    }
    if (fromAccountId === toAccountId) {
      setError('The source and destination accounts must be different.');
      return;
    }
    if (!Number.isFinite(parsedAmount) || parsedAmount <= 0) {
      setError('The amount must be a positive number.');
      return;
    }

    setSubmitting(true);
    try {
      await createTransfer({
        fromAccountId: Number(fromAccountId),
        toAccountId: Number(toAccountId),
        amount: parsedAmount,
      });
      setSuccess(`Transferred ${parsedAmount.toFixed(2)} successfully.`);
      setAmount('');
      onTransferCompleted();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Transfer failed.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="transfer-form">
      <label>
        From
        <select
          value={fromAccountId}
          onChange={(e) => setFromAccountId(e.target.value)}
          required
        >
          <option value="">Select account…</option>
          {accounts.map((a) => (
            <option key={a.id} value={a.id}>
              {a.owner} (balance: {a.balance.toFixed(2)})
            </option>
          ))}
        </select>
      </label>

      <label>
        To
        <select
          value={toAccountId}
          onChange={(e) => setToAccountId(e.target.value)}
          required
        >
          <option value="">Select account…</option>
          {accounts.map((a) => (
            <option key={a.id} value={a.id}>
              {a.owner}
            </option>
          ))}
        </select>
      </label>

      <label>
        Amount
        <input
          type="number"
          min="0.01"
          step="0.01"
          value={amount}
          onChange={(e) => setAmount(e.target.value)}
          placeholder="0.00"
          required
        />
      </label>

      <button type="submit" disabled={submitting}>
        {submitting ? 'Transferring…' : 'Transfer'}
      </button>

      {error && <p className="message error">{error}</p>}
      {success && <p className="message success">{success}</p>}
    </form>
  );
}
