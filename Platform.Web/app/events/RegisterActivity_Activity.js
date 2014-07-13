/**
* @class App.events.RegisterActivity_Activity
* Обработчик клиентских событий сущности документа "Реестр мероприятий" ТЧ "Мероприятия"
*/
Ext.define('App.events.RegisterActivity_Activity', {
    extend: 'App.events.CommonItem',

    events: [

        { name: 'dataget', handler: 'onItemLoaded', item: null },
        { name: 'change', handler: 'onChangeActivity', item: 'idActivity' }    
    ],

    onItemLoaded: function () {
        var IdOwner = this.getField('IdOwner');
        if (IdOwner) {
            DataService.getItem(IdOwner.initialModel.identitylink, IdOwner.getValue(), this.onLoad1, this);
        }
        var idActivity = this.getField('idActivity');
        if (idActivity.getValue() != null) {
            DataService.getItem(idActivity.initialModel.identitylink, idActivity.getValue(), this.onLoadActivity, this);
        }

    },
    
    onChangeActivity: function (sender, newValue, oldValue) {
        var Activity = sender.getValue();
        if (sender.list && Activity) {
            if (newValue != oldValue) {
                var f = this.getField('idIndicatorActivity_Volume');
                f.setValue(null, null);
                f = this.getField('idContingent');
                f.setValue(null, null);
            }
            DataService.getItem(sender.initialModel.identitylink, Activity, this.onLoadActivity, this);
        }
    },
    
    onLoad1: function (result, response) {
        var doc = result.result[0];
        var idDocType = doc.iddoctype;
        var idRegistryKeyActivity = this.getField('idRegistryKeyActivity');
        var idRegystryActivity_ActivityMain = this.getField('idRegystryActivity_ActivityMain');

        if (idDocType != -1543503848)
        {
            idRegistryKeyActivity.allowBlank = true;
            idRegistryKeyActivity.hide();
            idRegystryActivity_ActivityMain.hide();

        } else {
            idRegistryKeyActivity.allowBlank = false;
            idRegistryKeyActivity.show();
            idRegystryActivity_ActivityMain.show();
        }
        idRegistryKeyActivity.clearInvalid();
        idRegistryKeyActivity.isValid();
    },
    

    onLoadActivity: function (result, response) {
        var Activity = result.result[0];
        var f = this.getField('idContingent');
        
        var activ = result.result[0];
        if (activ.idactivitytype == 0 || activ.idactivitytype == 3 || activ.idactivitytype == 7) {
            f.allowBlank = false;
        } else {
            f.allowBlank = true;
        }
        f.clearInvalid();
        f.isValid();

    }
});