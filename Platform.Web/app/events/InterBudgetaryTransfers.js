/**
* @class App.events.InterBudgetaryTransfers
* Обработчик клиентских событий сущности Отчет "Межбюджетные трансферты"
*/
Ext.define('App.events.InterBudgetaryTransfers', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'afterrender', handler: 'onAfterRender', item: null },
	    { name: 'change', handler: 'onChangebyApproved', item: 'byApproved' }
    ],

    onAfterRender: function () {
        this.onChangebyApproved(this.getField('byApproved'));
    },

    onChangebyApproved: function (sender, newValue, oldValue) {
        var DateReport = this.getField('DateReport');

        //if (this.getField('byApproved').getValue() == 1) {
        if (newValue == 1) {
            DateReport.show();
            var today = new Date();
            DateReport.setValue(today);
            DateReport.allowNull = false;
        } else {
            DateReport.hide();
            DateReport.setValue(null);
            DateReport.allowNull = true;
        }
        DateReport.clearInvalid();
        DateReport.isValid();
    }
});