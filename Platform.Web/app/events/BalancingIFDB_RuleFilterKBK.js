/**
* @class App.events.BalancingIFDB_RuleFilterKBK
* Обработчик клиентских событий ТЧ "Набор КБК для правила" инструмента "Балансировка доходов, расходов и ИФДБ"
*/
Ext.define('App.events.BalancingIFDB_RuleFilterKBK', {
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
        { name: 'change', handler: 'onChangeFilterFieldType', item: 'idFilterFieldType_OKATO' },
        { name: 'afterrender', handler: 'onAfterRender', item: null }
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
    },
    
    onAfterRender: function () {

        this.setDefaults();
    },
    
    setDefaults: function () {

        var defaults = this.getForm().defaults;
        if (defaults) {
            if (!Ext.isDefined(defaults.idowner)) {
                var tmp = this.getForm().getParent();
                if (tmp) tmp = tmp.getParent();
                if (tmp) tmp = tmp.getField('idOwner');
                if (tmp) defaults.idowner = tmp.getValue();
            }
        }
    }
});