/**
* @class App.events.BudgetExpenseStructure
* Обработчик клиентских событий сущности BudgetExpenseStructure
*/
Ext.define('App.events.BudgetExpenseStructure', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'change', handler: 'onChangeByApproved', item: 'IsApprovedOnly' },
        { name: 'change', handler: 'onChangeDocType', item: 'IdDocType' },
        { name: 'change', handler: 'onChangeShowProgram', item: 'ShowProgram' },
        { name: 'change', handler: 'onChangeSource', item: 'IdSourcesDataReports' }
    ],

    onAfterRender: function () {
        this.onChangeByApproved(this.getField('IsApprovedOnly'));
        this.onChangeShowProgram(this.getField('ShowProgram'));
    },
    
    onChangeShowProgram: function(sender) {
        if (sender.getValue()) {
            this.getField('IdDocType').setDisabled(false);
        } else {
            this.getField('IdDocType').setValue(null);
            this.getField('IdDocType').setDisabled(true);
        }
    },

    onChangeDocType: function (sender) {
        if (sender.getValue()) {
            this.getField('ShowActivities').setValue(false);
            this.getField('ShowActivities').setDisabled(true);
        } else {
            this.getField('ShowActivities').setDisabled(false);
        }
    },

    onChangeByApproved: function (sender, newValue, oldValue) {
        if (sender.getValue()) {
            this.getField('ReportDate').setValue(new Date());
            this.getField('ReportDate').allowBlank = false;
            this.getField('ReportDate').show();
        } else {
            this.getField('ReportDate').hide();
            this.getField('ReportDate').allowBlank = true;
            this.getField('ReportDate').setValue(undefined);
        }
    },
    
    onChangeSource: function(sender, newValue, oldValue) {
        if (newValue == 7) {
            this.getField('ShowProgram').setValue(false);
            this.getField('ShowProgram').setDisabled(true);
            
            this.getField('ShowGoals').setValue(false);
            this.getField('ShowGoals').setDisabled(true);
            
            this.getField('ShowActivities').setValue(false);
            this.getField('ShowActivities').setDisabled(true);

            this.getField('IdDocType').setValue(null);
            this.getField('IdDocType').setDisabled(true);
        } else {
            this.getField('ShowProgram').setDisabled(false);
            this.getField('ShowGoals').setDisabled(false);
            this.getField('ShowActivities').setDisabled(false);
        }
    }
});