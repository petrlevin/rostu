/**
* @class App.events.BalanceConfig_FilterKBK
* Обработчик клиентских событий ТЧ "Фильтры КБК" инструмента "Настройка балансировки"
*/
Ext.define('App.events.BalanceConfig_FilterKBK', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_BranchCode' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_CodeSubsidy' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_DEK' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_DFK' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_DKR' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_ExpenseObligationType' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_FinanceSource' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_KCSR' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_KFO' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_KOSGU' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_KVR' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_KVSR' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_RZPR' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_AuthorityOfExpenseObligation' },
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_OKATO' }
    ],

    onChangeFilterFieldType: function (sender, newValue, oldValue) {
        var nm = sender.name.replace('idFilterFieldType_', 'ml') + 's';
        if (newValue >= 1) {
            if (!(this.getField('id').getValue())) {
                this.getForm().saveElement();
            }
            this.getField(nm).enable();
        } else {
            this.getField(nm).disable();
        }
    }

});