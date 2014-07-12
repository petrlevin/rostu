IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE name = N'DEFAULT_BalancingIFDB_Expense_idExpenseObligationType' AND type = 'D')
BEGIN
ALTER TABLE [tp].[BalancingIFDB_Expense] DROP CONSTRAINT [DEFAULT_BalancingIFDB_Expense_idExpenseObligationType]
END
