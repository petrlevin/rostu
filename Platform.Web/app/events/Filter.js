/**
* @class App.events.Filter
* Простейший обработчик клиентских событий сущности Filter
*/
Ext.define('App.events.Filter', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'change', handler: 'onChangeRightSqlExpression', item: 'RightSqlExpression' },
        { name: 'change', handler: 'onRightValue', item: 'RightValue' },
        { name: 'change', handler: 'onidRightEntityField', item: 'idRightEntityField' }
    ],

    onChangeRightSqlExpression: function (sender, newValue, oldValue) {
        if (!this.getField('idComparisionOperator').getValue() )
            this.getField('idComparisionOperator').setValue(6, 'В списке');
    },
    
    onRightValue: function (sender, newValue, oldValue) {
        if (!this.getField('idComparisionOperator').getValue())
            this.getField('idComparisionOperator').setValue(0, 'Равно');
    },

    onidRightEntityField: function (sender, newValue, oldValue) {
        if (!this.getField('idComparisionOperator').getValue())
            this.getField('idComparisionOperator').setValue(0, 'Равно');
    }
});