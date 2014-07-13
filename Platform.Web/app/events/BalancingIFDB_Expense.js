/**
* @class App.events.BalancingIFDB_Expense
* Обработчик клиентских событий ТЧ "Расходы" инструмента "Балансировка доходов, расходов и ИФДБ"
*/
Ext.define('App.events.BalancingIFDB_Expense', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'change', handler: 'onChangeDiffOrChange', item: 'DifferenceOFG' },
        { name: 'change', handler: 'onChangeDiffOrChange', item: 'DifferencePFG1' },
        { name: 'change', handler: 'onChangeDiffOrChange', item: 'DifferencePFG2' },
        { name: 'change', handler: 'onChangeDiffOrChange', item: 'DifferenceAdditionalOFG' },
        { name: 'change', handler: 'onChangeDiffOrChange', item: 'DifferenceAdditionalPFG1' },
        { name: 'change', handler: 'onChangeDiffOrChange', item: 'DifferenceAdditionalPFG2' },
        { name: 'change', handler: 'onChangeDiffOrChange', item: 'ChangeOFG' },
        { name: 'change', handler: 'onChangeDiffOrChange', item: 'ChangePFG1' },
        { name: 'change', handler: 'onChangeDiffOrChange', item: 'ChangePFG2' },
        { name: 'change', handler: 'onChangeDiffOrChange', item: 'ChangeAdditionalOFG' },
        { name: 'change', handler: 'onChangeDiffOrChange', item: 'ChangeAdditionalPFG1' },
        { name: 'change', handler: 'onChangeDiffOrChange', item: 'ChangeAdditionalPFG2' },
        { name: 'dataget', handler: 'onDataget', item: null }
    ],

    //Основной сервис с которым работает Event
    docService: null,

    constructor: function (config) {
        this.docService = SborBalancingIFDB;

        this.callParent([config]);
    },
    
    onChangeDiffOrChange: function (sender) {
        var nmChange;
        var nmOrig;
        var newVal;
        if (sender.name.indexOf('Difference') == 0) {
            nmChange = sender.name.replace('Difference', 'Change');
            nmOrig = sender.name.replace('Difference', '');
            newVal = this.getField(nmOrig).getValue() + sender.getValue();
        } else {
            nmChange = sender.name.replace('Change', 'Difference');
            nmOrig = sender.name.replace('Change', '');
            newVal = sender.getValue() - this.getField(nmOrig).getValue();
        }
        var f = this.getField(nmChange);
        if (f.getValue() != newVal) {
            this.getField(nmChange).setValue(newVal);
        }
    },

    onDataget: function () {
        this.docService.disableFields_Expense(this.getField('id').getValue(), this.onLoadFields, this);
        this.docService.showHideFields_Expense(this.getField('idOwner').getValue(), this.onLoadFields2, this);
    },
    
    onLoadFields: function (result, response) {
        var flds = ['DifferenceOFG', 'DifferencePFG1', 'DifferencePFG2', 'DifferenceAdditionalOFG', 'DifferenceAdditionalPFG1', 'DifferenceAdditionalPFG2'];
        Ext.each(flds, function(field) {
             this.getField(field).setReadOnly(false);
        }, this);
        
        if (Ext.isArray(result) && result.length > 0) {
            Ext.each(result, function (field) {
                var f = this.getField(field);
                if (f) {
                    f.disable();
                }
                f = this.getField(field.replace('Change', 'Difference'));
                if (f) {
                    f.setReadOnly(true);
                }
            }, this);
        }
    },
        
    onLoadFields2: function (result, response) {
        var showFields = result.showFields;
        if (Ext.isArray(showFields) && showFields.length > 0) {
            Ext.each(showFields, function (field) {
                var f = this.getField(field);
                if (f) {
                    f.show();
                }
            }, this);
        }
        
        var hideFields = result.hideFields;
        if (Ext.isArray(hideFields) && hideFields.length > 0) {
            Ext.each(hideFields, function (field) {
                var f = this.getField(field);
                if (f) {
                    f.hide();
                }
            }, this);
        }
    }
});
