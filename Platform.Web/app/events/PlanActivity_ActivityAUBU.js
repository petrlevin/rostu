/**
* @class App.events.PlanActivity_ActivityAUBU
* Обработчик клиентских событий сущности PlanActivity_ActivityAUBU
*/
Ext.define('App.events.PlanActivity_ActivityAUBU', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'dataget', handler: 'onDataget', item: null },
        { name: 'change', handler: 'onChangeContingent', item: 'idContingent' },
	    { name: 'change', handler: 'onChangeActivity', item: 'idActivity' }
    ],

    onDataget: function () {
        this.onChangeActivity(this.getField('idActivity'));
    },

    onChangeContingent: function (sender, newValue, oldValue) {
        var f = this.getField('idActivity');
        this.onChangeActivity(f, f.getValue());
    },
    
    onChangeActivity: function (sender, newValue, oldValue) {
        var activity = sender.getValue();
        if (activity) {
            DataService.getItem(sender.initialModel.identitylink, activity, this.onLoadActivity, this);
            if (newValue) {
                var owner = this.getField('idOwner').getValue();
                var cont = this.getField('idContingent').getValue();
                SborCommonService.getDefault_PlanActivity_Activity(owner, activity, cont, this.onLoadActivity2, this);
            }
        }
    },

    onLoadActivity: function (result, response) {

        var activ = result.result[0];
        var f = this.getField('idContingent');

        if (activ.idactivitytype == 0 || activ.idactivitytype == 3 || activ.idactivitytype == 7) {
            f.allowBlank = false;
        } else {
            f.allowBlank = true;
        }
        f.clearInvalid();
        f.isValid();
    },

    onLoadActivity2: function (result, response) {
        if (result.idindicator) {
            this.getField('idIndicatorActivity').setLink(result.idindicator, result.idindicator_caption);
        }
        if (result.idcontingent) {
            this.getField('idContingent').setLink(result.idcontingent, result.idcontingent_caption);
        }
    }
});