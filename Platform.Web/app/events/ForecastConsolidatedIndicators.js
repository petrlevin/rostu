/**
* @class App.events.ForecastConsolidatedIndicators
* Обработчик клиентских событий сущности ForecastConsolidatedIndicators
*/
Ext.define('App.events.ForecastConsolidatedIndicators', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'change', handler: 'onChangeByApproved', item: 'ByApproved' },
        { name: 'change', handler: 'onChangeIdVersion', item: 'idVersion' }
    ],

    onAfterRender: function () {
        this.onChangeByApproved(this.getField('ByApproved'));
    },

    onChangeByApproved: function (sender, newValue, oldValue) {
        if (sender.getValue()) {
            this.getField('EvaluationAdditionalNeeds').disable();
            this.getField('EvaluationAdditionalNeeds').setValue(false);
            this.getField('DateReport').setValue(new Date());
            this.getField('DateReport').allowBlank = false;
            this.getField('DateReport').show();
        } else {
            this.getField('EvaluationAdditionalNeeds').enable();
            this.getField('DateReport').hide();
            this.getField('DateReport').allowBlank = true;
            this.getField('DateReport').setValue(undefined);
        }
    },
    
    onChangeIdVersion: function (sender, newValue, oldValue) {
        this.getField('idProgram').setValue(undefined, undefined);
    }
});