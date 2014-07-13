/**
* @class App.events.PublicInstitutionEstimate_DistributionAdditionalParam
* Обработчик клиентских событий ТЧ "Дополнительный параметр распределения"
*/
Ext.define('App.events.PublicInstitutionEstimate_DistributionAdditionalParam', {
    extend: 'App.events.CommonItem',

    events: [
         { name: 'change', handler: 'onChangeKOSGU', item: 'idKOSGU' },
         { name: 'dataget', handler: 'onDataGet', item: null }
    ],

    onChangeKOSGU: function (sender, newValue, oldValue) {
        if (sender.list) {
            this.getField('KOSGUCaption').setValue(sender.list.grid.getSelectionModel().getLastSelected().get('caption'));
        }
    },

    onDataGet: function() {
        this.getField('KOSGUCaption').clearInvalid();
    }
});