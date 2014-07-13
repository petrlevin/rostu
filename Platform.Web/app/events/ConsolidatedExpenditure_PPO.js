/**
* @class App.events.ConsolidatedExpenditure_PPO
* Обработчик клиентских событий сущности ConsolidatedExpenditure_PPO
*/
Ext.define('App.events.ConsolidatedExpenditure_PPO', {
    extend: 'App.events.CommonItem',
    
    events: [
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'change', handler: 'onChangePublicLegalFormation', item: 'idPublicLegalFormation' }
    ],
    
    onChangePublicLegalFormation: function (sender, newValue, oldValue) {
        if (newValue != oldValue) {
            this.disableItem();
        }
    },
    

    onAfterRender: function () {
        this.disableItem();
    },

    disableItem: function () {

        if (this.getField('IdPublicLegalFormation').getValue()) {
            this.getField('idBudget').enable();
            this.getField('idVersion').enable();
        } else {
            this.getField('idBudget').disable();
            this.getField('idBudget').setValue(undefined);
            this.getField('idVersion').disable();
            this.getField('idVersion').setValue(undefined);
        }
    },

});