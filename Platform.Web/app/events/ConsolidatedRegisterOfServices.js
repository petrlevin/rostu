/**
* @class App.events.DocumentsOfSED
* Обработчик клиентских событий сущности отчета "ConsolidatedRegisterOfServices"
*/
Ext.define('App.events.ConsolidatedRegisterOfServices', {
    extend: 'App.events.CommonItem',

    events: [
         { name: 'itemloaded', handler: 'onItemLoaded', item: null }
    ],

    onItemLoaded: function (sender) {
        
        if (!this.isLoaded) {

            var PPOid = this.getField('idPublicLegalFormation');
            if (PPOid) {
                DataService.getItem(PPOid.initialModel.identitylink, PPOid.getValue(), this.onLoad, this);
            }
            this.isLoaded = true;
        }
    },

    onLoad: function (result, response) {
        var PPO = result.result[0];
        if (this.getField('Caption').getValue() == "")
        {
            if (PPO.idbudgetlevel == -1879048162)
            { this.getField('Caption').setValue("СВОДНЫЙ РЕЕСТР ГОСУДАРСТВЕННЫХ УСЛУГ (РАБОТ)"); }
            else
            { this.getField('Caption').setValue("СВОДНЫЙ РЕЕСТР МУНИЦИПАЛЬНЫХ УСЛУГ (РАБОТ)"); }

        }
    }

});