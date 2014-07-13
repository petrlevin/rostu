/**
* @class App.events.PublicInstitutionEstimate_Expense
* Обработчик клиентских событий ТЧ "Расходы"
*/
Ext.define('App.events.FBA_Activity', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'dataget', handler: 'onDataget', item: null },
        { name: 'change', handler: 'onChangeActivity', item: 'idActivity' }
    ],

	docService: null,
    
	constructor: function(config){
			this.docService = SborFinancialAndBusinessActivitiesService;
			
			this.callParent([config]);
	},
    
	onDataget: function () {

	    var isOwnActivity = this.getField('isOwnActivity').getValue();

	    if (!isOwnActivity) {
	        this.getField('idActivity').disable();
	        this.getField('idContingent').disable();
	    }

	},
	
    onChangeActivity: function (sender, newValue, oldValue) {
        var Activity = sender.getValue();
        if (sender.list && Activity) {
            DataService.getItem(sender.initialModel.identitylink, Activity, this.onLoadActivity, this);
        }

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