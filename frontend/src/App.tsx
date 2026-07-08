import { useCallback, useEffect, useState } from 'react';
import { getAccounts, getTransactions } from './api';
import { AccountList } from './components/AccountList';
import { TransactionList } from './components/TransactionList';
import { TransferForm } from './components/TransferForm';
import type { Account, Transaction } from './types';

export default function App() {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const refresh = useCallback(async () => {
    try {
      const [accountsData, transactionsData] = await Promise.all([
        getAccounts(),
        getTransactions(),
      ]);
      setAccounts(accountsData);
      setTransactions(transactionsData);
      setLoadError(null);
    } catch (e) {
      setLoadError(e instanceof Error ? e.message : 'Failed to load data.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  return (
    <main>
      <h1>Money Transfer</h1>

      {loadError && <p className="message error">{loadError}</p>}
      {loading && <p>Loading…</p>}

      {!loading && (
        <div className="layout">
          <section>
            <h2>Accounts</h2>
            <AccountList accounts={accounts} />

            <h2>New transfer</h2>
            <TransferForm accounts={accounts} onTransferCompleted={() => void refresh()} />
          </section>

          <section>
            <h2>Transactions</h2>
            <TransactionList transactions={transactions} />
          </section>
        </div>
      )}
    </main>
  );
}
