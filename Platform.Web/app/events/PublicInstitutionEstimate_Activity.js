/**
* @class App.events.PublicInstitutionEstimate_Activity
* Обработчик клиентских событий ТЧ "Мероприятия"
*/
Ext.define('App.events.PublicInstitutionEstimate_Activity', {
    extend: 'App.events.CommonItem',

    events: [
         { name: 'dataget', handler: 'onDataget', item: null },
         { name: 'change', handler: 'onChangeActivity', item: 'idActivity' },
         { name: 'change', handler: 'onChangeIndicatorActivity', item: 'idIndicatorActivity' }
    ],

    docService: null,
    
	constructor: function(config){
			this.docService = SborPublicInstitutionEstimateService;
			
			this.callParent([config]);
	},
	
    onDataget: function () {
        this.onChangeActivity(this.getField('idActivity'));

        this.getField('ActivityTypeCaption').clearInvalid();
        this.getField('idUnitDimension').clearInvalid();

    },

    onChangeActivity: function (sender, newValue, oldValue) {
        var activity = sender.getValue();
        if (activity && newValue) {
            DataService.getItem(sender.initialModel.identitylink, activity, this.onLoadActivity, this);
            if (newValue) {
                this.setContingentAndIndicator(activity);
            }
        } else {
            this.getField('ActivityTypeCaption').setValue('');
        }
    },

    setContingentAndIndicator: function (activity) {
        var owner = this.getField('idOwner').getValue();
        var cont = this.getField('idContingent').getValue();
        this.docService.getDefault_Activity(owner, activity, cont, false, this.onLoadActivity2, this);
    },
    
    onChangeContingent: function (sender, newValue, oldValue) {
        var f = this.getField('idActivity');
        this.setContingentAndIndicator(f, f.getValue());
    },

    onLoadActivity: function (result, response) {

        var activ = result.result[0];
        var f = this.getField('idContingent');

        if (activ.idactivitytype == 0 || activ.idactivitytype == 3) {
            f.allowBlank = false;
        } else {
            f.allowBlank = true;
        }

        f.clearInvalid();
        f.isValid();

        var idactivitytypeentity = -1543503845;
        DataService.getItem(idactivitytypeentity, activ.idactivitytype, this.onLoadActivityType, this);
    },

    onLoadActivity2: function (result, response) {
        if (result.idcontingent) {
            this.getField('idContingent').setValue(result.idcontingent, result.idcontingent_caption);

            if (result.idindicator) {
                this.getField('idIndicatorActivity').setValue(result.idindicator, result.idindicator_caption);
                this.getField('idUnitDimension').setValue(result.idindicatoractivitytype, result.idindicatoractivitytype_caption);
            }
        }
    },
    
    onLoadActivityType: function(result) {
        var activityType = result.result[0];
        this.getField('ActivityTypeCaption').setValue(activityType.caption);
    },
    
    onChangeIndicatorActivity: function (sender, newValue, oldValue) {
        if (sender.list && newValue) 
        {
                var indicatorActivity = sender.list.grid.getSelectionModel().getLastSelected();

                this.getField('idUnitDimension').setValue(indicatorActivity.get('idunitdimension'), indicatorActivity.get('idunitdimension_caption'));
        }else
                this.getField('idUnitDimension').setValue(null, '');
            
        
    }
    
    
});