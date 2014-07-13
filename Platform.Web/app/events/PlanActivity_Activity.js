/**
* @class App.events.PlanActivity_Activity
* Обработчик клиентских событий сущности PlanActivity_Activity
*/
Ext.define('App.events.PlanActivity_Activity', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'dataget', handler: 'onDataget', item: null },
	    { name: 'change', handler: 'onChangeActivity', item: 'idActivity' },
        { name: 'change', handler: 'onChangeContingent', item: 'idContingent' }
    ],

    onDataget: function () {
        var activityField = this.getField('idActivity');
        var activityValue = activityField.getValue();
        if (activityValue) {
            DataService.getItem(activityField.initialModel.identitylink, activityValue, this.onLoadActivity, this);
        }
    },

    onChangeActivity: function (sender, newValue, oldValue) {
        var activity = sender.getValue();
        var owner = this.getField('idOwner').getValue();
        
        if (activity) {
            DataService.getItem(sender.initialModel.identitylink, activity, this.onLoadActivity, this);
        }
        
        if (newValue && owner) {
            SborCommonService.getDefault_PlanActivity_Activity(owner, activity, null, true, this.setDefaulContingent, this);
        } else {
            this.getField('idContingent').setValue(undefined, undefined);
        }
    },
    
    onChangeContingent: function (sender, newValue, oldValue) {
        var owner = this.getField('idOwner').getValue();
        var activity = this.getField('idActivity').getValue();

        if (activity && owner) {
            SborCommonService.getDefault_PlanActivity_Activity(owner, activity, newValue, false, this.setDefaulIndicator, this);
        } else {
            this.getField('idIndicatorActivity').setValue(undefined, undefined);
        }
    },
    
    setDefaulContingent: function (result, response) {
        if (result.idcontingent) {
            this.getField('idContingent').setValue(result.idcontingent, result.idcontingent_caption);
        } else {
            this.getField('idContingent').setValue(undefined, undefined);
        }
    },
    
    setDefaulIndicator: function (result, response) {
        if (result.idindicator) {
            this.getField('idIndicatorActivity').setValue(result.idindicator, result.idindicator_caption);
        } else {
            this.getField('idIndicatorActivity').setValue(undefined, undefined);
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
    }
});