/**
* @class App.events.ConsolidatedExpenditure
* Обработчик клиентских событий сущности ConsolidatedExpenditure
*/
Ext.define('App.events.ConsolidatedExpenditure', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'change', handler: 'onChangeByApproved', item: 'IsApprovedOnly' },
        { name: 'change', handler: 'onChangePublicLegalFormation', item: 'idPublicLegalFormation' },
        { name: 'change', handler: 'onChangeHierarchyPeriod', item: 'idHierarchyPeriod' },
        { name: 'itemloaded', handler: 'onItemLoaded', item: null }
    ],

    FormLoad: false,

    onItemLoaded: function(sender) {
        this.FormLoad = true;
    },

    onAfterRender: function () {
        
        tbar1 = this.getTopToolBar('tpPublicLegalFormations');

        this.addNewButton(tbar1, {
            id: 'tpPublicLegalFormations_InsertData',
            text: 'Добавить',
            handler: this.tpPublicLegalFormations_InsertDataButton,
            tooltip: 'Добавить ППО'
        }, 0);

         this.addNewButton(tbar1, {
            id: 'tpPublicLegalFormations_FillData',
            text: 'Заполнить',
            handler: this.tpPublicLegalFormations_FillDataButton,
            tooltip: 'Заполнить ППО'
        }, 1);

        //this.hideButtons(tbar1, 'create');

        this.onChangeByApproved(this.getField('IsApprovedOnly'));
        SborReportsService.getIdCurrentBudget(this.getField('idHierarchyPeriod').getValue(), this.getIdCurrentBudget, this); // Находим значение по умолчанию в поле год
    },
    
    onChangeByApproved: function(sender, newValue, oldValue) {
        // показываем/скрываем "Дата отчета" при изменении "Строить отчет по утвержденным данным"
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

    onChangePublicLegalFormation: function(sender, newValue, oldValue) {
        if (this.FormLoad && newValue != oldValue) {
            this.getForm().saveElement();
            this.onClearTables();
        }
    },

    onChangeHierarchyPeriod: function(sender, newValue, oldValue) {
        if (this.FormLoad && newValue != oldValue) {
            this.getForm().saveElement();
            this.onClearTables();
        }
    },

    getIdCurrentBudget: function(result, response) {
        // Устанавливаем значение по умолчанию в поле год
        this.getField('idHierarchyPeriod').setValue(result.id, result.caption);
    },
    
    tpPublicLegalFormations_InsertDataButton: function () {
        
    },
    
    tpPublicLegalFormations_FillDataButton: function () {
        this.callServiceFromGrid(SborReportsService.fillData_tpPublicLegalFormations, ['tpPublicLegalFormations']);
    },

    onClearTables: function () {
        this.callServiceFromGrid(SborReportsService.clearTables_ConsolidatedExpenditure_PPO, ['tpPublicLegalFormations']);
    }
    
});