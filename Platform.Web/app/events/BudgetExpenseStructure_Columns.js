/**
* @class App.events.BudgetExpenseStructure
* Обработчик клиентских событий сущности BudgetExpenseStructure_Columns
*/
Ext.define('App.events.BudgetExpenseStructure_Columns', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'change', handler: 'onChangeByApproved', item: 'IsGroupResult' },
        { name: 'blur', handler: 'onChangeMaxValue', item: 'MaxLevel' },
        { name: 'blur', handler: 'onChangeMinValue', item: 'MinLevel' }
    ],

    onAfterRender: function () {
        this.onChangeByApproved(this.getField('IsGroupResult'));
    },

    onChangeByApproved: function (sender, newValue, oldValue) {
        if (sender.getValue()) {
            this.getField('MinLevel').setValue(1);
            this.getField('MinLevel').allowBlank = false;
            this.getField('MinLevel').show();
            
            this.getField('MaxLevel').setValue(undefined);
            this.getField('MaxLevel').show();

        } else {
            this.getField('MinLevel').hide();
            this.getField('MinLevel').allowBlank = true;
            this.getField('MinLevel').setValue(null);
            
            this.getField('MaxLevel').hide();
            this.getField('MaxLevel').setValue(null);
        }
    },

    onChangeMaxValue: function (sender) {
        var value = sender.getValue();
        if (!value)
            return;

        var minValue = this.getField('MinLevel').getValue();

        if (value < minValue)
            sender.setValue(minValue);
    },

    onChangeMinValue: function (sender) {
        var value = sender.getValue();
        if (!value) {
            sender.setValue(1);
            return;
        }

        var maxValueField = this.getField('MaxLevel');
        var maxValue = maxValueField.getValue();

        if (!maxValue)
            return;

        if (maxValue < value)
            maxValueField.setValue(value);
    }
});