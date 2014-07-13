/**
* @class App.events.LongTermGoalProgram_ListSubProgram
* Обработчик клиентских событий сущности LongTermGoalProgram_ListSubProgram
*/
Ext.define('App.events.LongTermGoalProgram_ListSubProgram', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'dataget', handler: 'onItemLoaded', item: null },
        { name: 'change', handler: 'onChangeAnalyticalCodeStateProgram', item: 'idAnalyticalCodeStateProgram' },
        { name: 'change', handler: 'onChangeSbp', item: 'idSbp' }
    ],

    onItemLoaded: function() {
        var idOwner = this.getField('idOwner').getValue();
        if (idOwner) {
            DataService.getItem(this.getField('idOwner').initialModel.identitylink, idOwner, this.onLoad1, this);
        }
        
        var idActualDocument = this.getField('idActualDocument').getValue();
        if (idActualDocument != null)
            this.getField('idSbp').setReadOnly(true);
    },

    onChangeAnalyticalCodeStateProgram: function(sender, newValue, oldValue) {
        var acsp = sender.getValue();
        if (sender.list && acsp) {
            DataService.getItem(sender.initialModel.identitylink, acsp, this.onLoad2, this);
        }
    },

    onLoad1: function(result, response) {
        var f = this.getField('idAnalyticalCodeStateProgram');
        if (!result.result[0].hasmasterdoc) {
            f.hide();
        } else {
            f.allowBlank = false;
            f.clearInvalid();
            f.isValid();
        }
    },

    onLoad2: function(result, response) {
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