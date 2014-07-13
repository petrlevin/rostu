/**
* @class App.events.StateProgramGoalIndicatorValue
* Обработчик клиентских событий сущности StateProgramGoalIndicatorValue
*/
Ext.define('App.events.StateProgramGoalIndicatorValue', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'change', handler: 'onChangeByApproved', item: 'ByApproved' },
        { name: 'change', handler: 'onChangeIdVersion', item: 'idVersion' }
    ],

    onChangeByApproved: function (sender, newValue, oldValue) {
        if (sender.getValue()) {
            this.getField('DateReport').setValue(new Date());
            this.getField('DateReport').allowBlank = false;
            this.getField('DateReport').show();
        } else {
            this.getField('DateReport').hide();
            this.getField('DateReport').allowBlank = true;
            this.getField('DateReport').setValue(undefined);
        }
    },
    
    onChangeIdVersion: function (sender, newValue, oldValue) {
        this.getField('idProgram').setValue(undefined, undefined);
    }
});