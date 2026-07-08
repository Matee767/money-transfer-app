import type { Transaction } from '../types';

interface Props {
  transactions: Transaction[];
}

const formatMoney = (value: number) =>
  value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

const formatDate = (iso: string) => new Date(iso).toLocaleString();

export function TransactionList({ transactions }: Props) {
  if (transactions.length === 0) {
    return <p>No transactions yet — make the first transfer!</p>;
  }

  return (
    <table>
      <thead>
        <tr>
          <th>When</th>
          <th>From</th>
          <th>To</th>
          <th className="num">Amount</th>
        </tr>
      </thead>
      <tbody>
        {transactions.map((tx) => (
          <tr key={tx.id}>
            <td>{formatDate(tx.createdAtUtc)}</td>
            <td>
              {tx.fromOwner} <span className="muted">(#{tx.fromAccountId})</span>
            </td>
            <td>
              {tx.toOwner} <span className="muted">(#{tx.toAccountId})</span>
            </td>
            <td className="num">{formatMoney(tx.amount)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
