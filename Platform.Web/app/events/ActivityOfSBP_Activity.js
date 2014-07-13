/**
* @class App.events.ActivityOfSBP_Activity_Value
* Обработчик клиентских событий сущности ActivityOfSBP_Activity_Value
*/
Ext.define('App.events.ActivityOfSBP_Activity', {
    extend: 'App.events.CommonItem',

	events: [
        { name: 'dataget', handler: 'onDataget', item: null },
	    { name: 'change', handler: 'onChangeActivity', item: 'idActivity' }
	],

	onDataget: function () {
	    if (this.getField('IdSBP').getValue() == null) {
	        var IdOwner = this.getField('IdOwner');
	        if (IdOwner) {
	            DataService.getItem(IdOwner.initialModel.identitylink, IdOwner.getValue(), this.onLoadPage, this);
	        }
	    }

	    if (this.getField('IdActivity').getValue() == null) {
	        this.getField('IdContingent').readOnly = true;
	        this.getField('IdIndicatorActivity_Volume').readOnly = true;
	    }
	},
	
	onLoadPage: function (result, response) {
	    var doc = result.result[0];
	    this.getField('IdSBP').setValue(doc.idsbp, doc.idsbp_caption);
	},
	
	onChangeActivity: function(sender, newValue, oldValue) {
	    var Activity = sender.getValue();
	    if (sender.list && Activity) {
	        DataService.getItem(sender.initialModel.identitylink, Activity, this.onLoadActivity, this);
	    }

	    var idOwner = this.getField('IdOwner').getValue();
	    
	    if (newValue) {
	        SborCommonService.getDefaultActivityOfSBP_Activity(idOwner, newValue, this.onLoadActivity2, this);
	    }

	    if (newValue == null) {
	        this.getField('IdContingent').readOnly = true;
	        this.getField('IdIndicatorActivity_Volume').readOnly = true;
	    } else {
	        this.getField('IdContingent').readOnly = false;
	        this.getField('IdIndicatorActivity_Volume').readOnly = false;
	    }
	    if (newValue != oldValue) {
	        this.getField('IdContingent').setValue(null, "");
	        this.getField('IdIndicatorActivity_Volume').setValue(null, "");
	    }

	},

	onLoadActivity2: function (result, response) {
	    if (result.idindicator) {
	        this.getField('IdIndicatorActivity_Volume').setValue(result.idindicator, result.idindicator_caption);
	    }
	    if (result.idcontingent) {
	        this.getField('IdContingent').setValue(result.idcontingent, result.idcontingent_caption);
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