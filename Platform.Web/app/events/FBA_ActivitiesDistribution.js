/**
* @class App.events.PublicInstitutionEstimate_DistributionActivity
Обработчик клиентских событий ТЧ "Мероприятия для распределения"
*/
Ext.define('App.events.FBA_ActivitiesDistribution', {
    extend: 'App.events.CommonTablePart',

    events: [
         { name: 'dataget', handler: 'onDataget', item: null },
         { name: 'change', handler: 'onChangeActivity', item: 'IdFBA_Activity' }
    ],

	docService: null,
    
	constructor: function(config){
			this.docService = SborPublicInstitutionEstimateService;
			
			this.callParent([config]);
	},
	

	
    distributionMethodM1: 1,
    distributionMethodM2: 2,
    distributionMethodM3: 3,
    distributionMethodM4: 4,
    distributionMethodM5: 5,

    onDataget: function () {
        this.onChangeActivity(this.getField('idActivity'));

        this.hideByMethod();

    },

    hideByMethod: function () {
        if (!this.ownerEntity)
            return;

        var selectedRecord = this.ownerEntity.getField('tpDistributionMethodss').grid.getSelectionModel().getSelection()[0];
        var distributiveMethod = selectedRecord.get('idindirectcostsdistributionmethod');

        var directFields = ['ofg_direct', 'pfg1_direct', 'pfg2_direct'];
        var volumeFields = ['ofg_activity', 'pfg1_activity', 'pfg2_activity'];
        var factorFields = ['factorofg', 'factorpfg1', 'factorpfg2'];

        var hiddenFields = [];

        switch (distributiveMethod) {
            case this.distributionMethodM1:
                hiddenFields = directFields.concat(volumeFields).concat(factorFields);
                break;
            case this.distributionMethodM2:
                hiddenFields = volumeFields.concat(factorFields);
                break;
            case this.distributionMethodM3:
                hiddenFields = directFields.concat(factorFields);
                break;
            case this.distributionMethodM4:
                hiddenFields = directFields.concat(volumeFields);
                break;
            case this.distributionMethodM5:
                hiddenFields = volumeFields.concat(factorFields);
                break;
        }

        this.hideFields(hiddenFields);

    },


    onChangeActivity: function (sender, newValue, oldValue) {
        if (newValue && sender.list) {
            var activity = sender.list.grid.getSelectionModel().getLastSelected().get('idactivity');
            var contingent = sender.list.grid.getSelectionModel().getLastSelected().get('idcontingent');

            var activityEntityId = -1543503842;
            DataService.getItem(activityEntityId, activity, this.onLoadActivity, this);

            this.setContingent(activity, contingent);
        } else {
            this.getField('ContingentCaption').setValue(null, '');
            this.getField('ActivityTypeCaption').setValue('');
        }
    },

    setContingent: function (activity, contingent) {
        var owner = this.getField('idOwner').getValue();
        this.docService.getDefault_Activity(owner, activity, contingent, false, this.onLoadActivity2, this);
    },

    onLoadActivity: function (result, response) {
        var activ = result.result[0];
        var idactivitytypeentity = -1543503845;
        DataService.getItem(idactivitytypeentity, activ.idactivitytype, this.onLoadActivityType, this);
    },

    onLoadActivity2: function (result, response) {
        if (result.idcontingent) {
            this.getField('ContingentCaption').setValue(result.idcontingent_caption);
        }
    },

    onLoadActivityType: function (result) {
        var activityType = result.result[0];
        this.getField('ActivityTypeCaption').setValue(activityType.caption);
    },

    onChangeIndicatorActivity: function (sender, newValue, oldValue) {
        if (newValue) {
            var idindicatortypeentity = -1543503844;
            var idindicatortype = sender.list.grid.getSelectionModel().getLastSelected().get('idindicatoractivitytype');
            DataService.getItem(idindicatortypeentity, idindicatortype, this.onLoadIndicatorActivityType, this);
        }
    },

    onLoadIndicatorActivityType: function (result) {
        var indicatorType = result.result[0];
        this.getField('ActivityTypeCaption').setValue(indicatorType.caption);
    }
});