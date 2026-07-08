import type { Account } from '../types';

interface Props {
  accounts: Account[];
}

const formatMoney = (value: number) =>
  value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

export function AccountList({ accounts }: Props) {
  if (accounts.length === 0) {
    return <p>No accounts yet.</p>;
  }

  return (
    <table>
      <thead>
        <tr>
          <th>ID</th>
          <th>Owner</th>
          <th className="num">Balance</th>
        </tr>
      </thead>
      <tbody>
        {accounts.map((account) => (
          <tr key={account.id}>
            <td>{account.id}</td>
            <td>{account.owner}</td>
            <td className="num">{formatMoney(account.balance)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
