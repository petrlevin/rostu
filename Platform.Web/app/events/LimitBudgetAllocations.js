/**
* @class App.events.LimitBudgetAllocations
* Обработчик клиентских событий сущности "Предельные объемы бюджетных ассигнований"
*/
Ext.define('App.events.LimitBudgetAllocations', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'itemloaded', handler: 'onItemLoaded', item: null },
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'operationbegin', handler: 'onOperationBegin', item: null },
        { name: 'change', handler: 'hideKbk', item: 'idSBP' },
        { name: 'change', handler: 'hideAdditionalNeed', item: 'isAdditionalNeed' },
        { name: 'change', handler: 'onChangeCompareDocument', item: 'IdCompareWithDocument' }
    ],
   
    onItemLoaded: function() {
        this.hideKbk();
        this.PermissionsInputAdditionalRequirements();
        this.hideAdditionalNeed();
        this.hideTab();
     },

    onOperationBegin: function() {
        this.hideKbk();
        this.PermissionsInputAdditionalRequirements();
        this.hideTab();
        // Обновляем ТЧ иначе при определенных условиях падаем
        this.getField('tpLimitAllocations').refresh();
    },

    hideKbk: function() {
        var docId = this.getField('id').getValue();

        if (docId) {
            SborLimitBudgetAllocationsService.get_SbpblankByDocument(docId, this.onLoadSbpBlank, this);
            SborLimitBudgetAllocationsService.get_HideColumn_tpControlRelation(docId, this.onHideColumn_tpControlRelation, this);
        }
    },

    onLoadSbpBlank: function(result, response) {
        if (result != null) {
            var hiddenFields = result.hidden;
            var columns = ['idowner', 'id', 'ownerdate', 'additionalneedofg', 'additionalneedpfg1', 'additionalneedpfg2'];
            if (Ext.isArray(hiddenFields) && hiddenFields.length > 0) {
                
                // скрываем в ТЧ Предельные объемы бюджетных ассигнований
                var grid = this.getField('tpLimitAllocations').grid;
                Ext.each(grid.columns, function(column) {
                    if (Ext.Array.contains(hiddenFields, column.name.toLowerCase()))
                        column.hide();
                    else if (column.isHidden() && !Ext.Array.contains(columns, column.name.toLowerCase()))
                        column.show();
                });
                
                // скрываем в ТЧ Контрольные соотношения
                this.hideColumnsOnGrid('tpControlRelation', hiddenFields.concat(['idowner', 'id']));

                // скрываем в ТЧ Просмотр изменений
                this.hideColumnsOnGrid('tpShowChanges', hiddenFields.concat(['idowner', 'id']));

            }
        }
    },

    PermissionsInputAdditionalRequirements: function() {

        var isRedact = !this.getButton(this.getTopToolBar('tpLimitAllocations'), 'create').disabled; // так я определяю режим редактирования
        if (isRedact)
            SborCommonService.getPermissionsInputAdditionalRequirements(this.getField('IdSBP').getValue(), function(result) { this.onPermissionsInputAdditionalRequirements(result); }, this);

    },
    onPermissionsInputAdditionalRequirements: function(result) {
        var HAN = this.getField('isAdditionalNeed');
        if (result) {
            HAN.enable();
        } else {
            HAN.disable();
        }
    },

    onHideColumn_tpControlRelation: function (result) {
        if (result != null) {
            var hiddenFields = result.hidden;
            var requiredFields = result.required;
            var columns = ['idowner', 'id'];

            var grid = this.getField('tpControlRelation').grid;
            Ext.each(grid.columns, function (column) {
                if (column.isHidden() && !Ext.Array.contains(requiredFields, column.name.toLowerCase()))
                    hiddenFields = hiddenFields.concat(column.name.toLowerCase());
                else if (!Ext.Array.contains(columns, column.name.toLowerCase()))
                    requiredFields = requiredFields.concat(column.name.toLowerCase());
            });

            this.hideColumnsOnGrid('tpControlRelation', hiddenFields, requiredFields);
        }
    },

    hideAdditionalNeed: function() {
        var hasAdditionals = this.getField('isAdditionalNeed').getValue();
        this.setVisisibilityColumnsOnGrid('tpLimitAllocations', ['additionalneedofg', 'additionalneedpfg1', 'additionalneedpfg2'], !hasAdditionals);
    },

    onAfterRender: function(sender) {
        tbar1 = this.getTopToolBar('tpControlRelation');

        this.addNewButton(tbar1, {
            id: 'tpControlRelation_FillData',
            text: 'Заполнить',
            handler: this.tpControlRelation_FillData,
            tooltip: 'Заполнить',
            scope: this.getField('tpControlRelation').grid,
            setAvailability: function(entityItem) {
                this.enable();
                }
            }, 0);
                
        this.hideButtons(tbar1, 'create');

        tbar2 = this.getTopToolBar('tpShowChanges');

        this.addNewButton(tbar2, {
            id: 'tpShowChanges_FillData',
            text: 'Заполнить',
            handler: this.tpShowChanges_FillData,
            tooltip: 'Заполнить',
            scope: this.getField('tpShowChanges').grid,
            setAvailability: function(entityItem) {
            this.enable();
        }
            
        }, 0);
        
        this.hideButtons(tbar2, 'create');
    },    
    
    tpControlRelation_FillData: function() {
        this.callServiceFromGrid(SborCommonService.tpControlRelation_FillData, 'tpControlRelation');
    },
    tpShowChanges_FillData: function () {
        this.getForm().saveElement();
        this.callServiceFromGrid(SborCommonService.tpShowChanges_FillData, 'tpShowChanges');
    },

    onChangeCompareDocument: function (sender, newValue, oldValue)
    {
        if (newValue!=oldValue) {
            this.tpShowChanges_FillData();
        }
    },
    
    hideTab: function () {

        var DS = this.getField('idDocStatus');
        
        var ControlRelation = this.getField('tpControlRelation').up('tabpanel').getTabBar().items.get(1);
        if (DS == null || DS.getValue() == -2147483613) {
            ControlRelation.hide();
        }else {
            ControlRelation.show();
            
            var Bytton = this.getButton(this.getTopToolBar('tpControlRelation'), 'tpControlRelation_FillData');
        }
        
        var Parent = this.getField('idParent').getValue();
        
        var ShowChanges = this.getField('tpShowChanges').up('tabpanel').getTabBar().items.get(2);
        if (Parent == null) {
            ShowChanges.hide();
        } else {
            ShowChanges.show();
        }
    }
    
});