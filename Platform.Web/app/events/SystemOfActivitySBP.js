/**
* @class App.events.SystemOfActivitySBP
* Обработчик клиентских событий сущности Отчет "Система мероприятий деятельности ведомства"
*/
Ext.define('App.events.SystemOfActivitySBP', {
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
        var HasAdditionalNeed = this.getField('HasAdditionalNeed');

        //if (this.getField('byApproved').getValue() == 1) {
        if (newValue == 1) {
            DateReport.show();
            var today = new Date();
            DateReport.setValue(today);
            DateReport.allowNull = false;
            HasAdditionalNeed.disable();
            HasAdditionalNeed.setValue(0);
        } else {
            DateReport.hide();
            DateReport.setValue(null);
            DateReport.allowNull = true;
            HasAdditionalNeed.enable();
        }
        DateReport.clearInvalid();
        DateReport.isValid();
    }
});