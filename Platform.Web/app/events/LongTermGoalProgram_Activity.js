/**
* @class App.events.LongTermGoalProgram_Activity_Value
* Обработчик клиентских событий сущности LongTermGoalProgram_Activity_Value
*/
Ext.define('App.events.LongTermGoalProgram_Activity', {
	extend: 'App.events.CommonItem',

	events: [
        { name: 'dataget', handler: 'onDataget', item: null },
	    { name: 'change', handler: 'onChangeActivity', item: 'idActivity' }
	],

	onDataget: function () {
	    // SBORIII-66 Алексей Кузнецов added a comment - 24/05/13 11:44 - edited
	    // 1. При добавлении строки в ТЧ Мероприятие автоматом заполняется поле "Исполнитель" , подтягивается СБП из поля ШапкаДокумента.Ответственный исполнитель - ошибка.
	    //var IdOwner = this.getField('IdOwner');
	    //if (IdOwner) {
	    //    DataService.getItem(IdOwner.initialModel.identitylink, IdOwner.getValue(), this.onLoadPage, this);
	    //}
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

	    var idSbp = this.getField('IdSBP').getValue();
	    
	    if (newValue) {
	        SborCommonService.getDefaulLongTermGoalProgram_Activity(newValue, idSbp, this.onLoadActivity2, this);
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

	    if (activ.idactivitytype == 0 || activ.idactivitytype == 3) {
	        f.allowBlank = false;
        } else {
            f.allowBlank = true;
        }
	    f.clearInvalid();
	    f.isValid();
	}
});