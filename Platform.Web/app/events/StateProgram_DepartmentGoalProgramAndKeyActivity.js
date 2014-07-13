/**
* @class App.events.StateProgram_DepartmentGoalProgramAndKeyActivity
* Обработчик клиентских событий сущности StateProgram_DepartmentGoalProgramAndKeyActivity
*/
Ext.define('App.events.StateProgram_DepartmentGoalProgramAndKeyActivity', {
	extend: 'App.events.CommonItem',

	events: [
        { name: 'dataget', handler: 'onItemLoaded', item: null },
        { name: 'change', handler: 'onChangeidDocType', item: 'idDocType' },
	    { name: 'change', handler: 'onChangeAnalyticalCodeStateProgram', item: 'idAnalyticalCodeStateProgram' },
        { name: 'change', handler: 'onChangeSbp', item: 'idSbp' }
	],

	onItemLoaded: function () {
	    var idActualDocument = this.getField('idActiveDocument').getValue();
	    if (idActualDocument != null) 
	        this.getField('idSbp').setReadOnly(true);
	},

	onChangeidDocType: function (sender, newValue, oldValue) {
	    if (newValue != oldValue) {
	        this.getField('idSystemGoal').setValue(null, "");
	    }
	},

	onChangeAnalyticalCodeStateProgram: function (sender, newValue, oldValue) {
	    var acsp = sender.getValue();
	    if (sender.list && acsp) {
	        DataService.getItem(sender.initialModel.identitylink, acsp, this.onLoad, this);
	    }
	},

	onLoad: function (result, response) {
	    this.getField('Caption').setValue(result.result[0].caption);
	},

	idsbpnew: null,

	onChangeSbp: function (sender, newValue, oldValue) {
	    idsbpnew = newValue;

	    var goal = this.getField('idSystemGoal');
	    var goalvalue = goal.getValue();
	    if (goalvalue != null) {
	        DataService.getItem(goal.initialModel.identitylink, goalvalue, this.onLoad3, this);
	    }
	},

	onLoad3: function (result, response) {
	    var idsbpgoal = result.result[0].idsbp;
	    if (idsbpgoal != null && idsbpgoal != idsbpnew) {
	        this.getField('idSystemGoal').setValue(null, null);
	    }
	}
});