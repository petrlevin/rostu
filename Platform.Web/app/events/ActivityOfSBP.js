/**
* @class App.events.ActivityOfSBP
* Обработчик клиентских событий сущности документа "Деятельность ведомства"
*/
Ext.define('App.events.ActivityOfSBP', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'operationbegin', handler: 'onOperationBegin', item: null },
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'change', handler: 'onChangeDocType', item: 'idDocType' },
        { name: 'itemloaded', handler: 'onItemLoaded', item: null },
        { name: 'change', handler: 'onDataGet', item: 'idSBP' },
        { name: 'dataget', handler: 'onDataGet', item: null }
    ],

    tbar1: null, // tpSystemGoalElement

    onOperationBegin: function () {
        this.onSelectionSystemGoalElement(this, []);
        this.UpdateVisual();
        this.PermissionsInputAdditionalRequirements();
    },
    
    onSelectionSystemGoalElement: function (sender, selected, eOpts) {
        if (!selected) return;

        var selectedRecord = selected[0];
        if (!selectedRecord) {
            selectedRecord = this.getField('tpSystemGoalElement').grid.getSelectionModel().getSelection()[0];
        }

        if (!selectedRecord) return;

        var isRedact = !this.getButton(tbar1, 'create').disabled; // так я определяю режим редактирования

        tpa = this.getField('tpActivity');
        tparm = this.getField('tpActivityResourceMaintenance');

        var pnl = tpa.up('panel');

        var f = selectedRecord.isLeaf(); // является ли элемент листовым?
        if (f) {
            pnl.enable();
            if (isRedact) {
                tpa.enable();
                tparm.enable();
            } else {
                tpa.disable();
                tparm.disable();
            }
        } else {
            pnl.disable();
        }

        tpa.update();
    },
    
    UpdateVisual: function () {
        var dt = this.getField('idDocType');
        
        if (this.getField('HasMasterDoc').getValue() == true) {
            this.getField('Date').setReadOnly(true);
            this.getField('DateStart').setReadOnly(true);
            this.getField('DateEnd').setReadOnly(true);
            this.getField('IdVersion').setReadOnly(true);
            this.getField('idResponsibleExecutantType').setReadOnly(true);
            this.getField('IdSBP').setReadOnly(true);
            this.getField('Caption').setReadOnly(true);
        }

        var acsp = this.getField('idAnalyticalCodeStateProgram');
        var md = this.getField('idMasterDoc');
        var hmd = this.getField('HasMasterDoc');
        var hmd = hmd;

        var bshow = dt.getValue() != -1543503840; //скрывать когда Тип = Непрограммная деятельность

        if (bshow) {
            acsp.show();
            md.show();
            hmd.show();
        } else {
            acsp.hide();
            md.hide();
            hmd.hide();
        }
        
        var ingp = hmd.getValue();
        var trm = this.getField('tpResourceMaintenance');
        if (!ingp) {
            trm.enable();
        } else {
            trm.disable();
        }
    },    

    onChangeDocType: function () {
        this.UpdateVisual();
    },

    onDataGet: function () {
        this.hideKbk();
        this.PermissionsInputAdditionalRequirements();
    },
    
    onItemLoaded: function () {
        this.UpdateVisual();
        this.hideKbk();
        this.PermissionsInputAdditionalRequirements();
        
        var gridSGE = this.getField("tpSystemGoalElement").grid;
        if (gridSGE) {
            //добавляем событие, обрабатывающее смену выбора в "Элементы СЦ"
            gridSGE.getSelectionModel().on('selectionchange', this.onSelectionSystemGoalElement, this);
            gridSGE.on('itemexpand', function (item) {
                if (item.isRoot()) {
                    item.expandChildren(true);
                }
            }, this);
        }
    },
    
    getDocId: function () {
        return this.getField('id').getValue();
    }, 

    tpSystemGoalElement_FillDataButton: function () {
        this.callServiceFromGrid(SborCommonService.fillData_ActivityOfSBP_SystemGoalElement, ['tpSystemGoalElement', 'tpGoalIndicator']);
    },

    tpSystemGoalElement_RefreshDataButton: function () {
        this.callServiceFromGrid(SborCommonService.refreshData_ActivityOfSBP_SystemGoalElement, ['tpSystemGoalElement'], 'tpSystemGoalElement');
    },

    tpGoalIndicator_Value_FillDataButton: function () {
        this.callServiceFromGrid(SborCommonService.fillData_ActivityOfSBP_GoalIndicator_Value, ['tpGoalIndicator'], 'tpSystemGoalElement');
    },

    tpGoalIndicator_Value_RefreshDataButton: function () {
        this.callServiceFromGrid(SborCommonService.refreshData_ActivityOfSBP_GoalIndicator_Value, ['tpGoalIndicator'], 'tpGoalIndicator');
    },

    tpActivityResourceMaintenance_FillDataButton: function () {
        this.callServiceFromGrid(SborCommonService.refreshData_ActivityResourceMaintenance_Value, ['tpActivityResourceMaintenance'], 'tpActivityResourceMaintenance');
    },

    onAfterRender: function (sender) {
        tbar1 = this.getTopToolBar('tpSystemGoalElement');

        this.addNewButton(tbar1, {
            id: 'tpSystemGoalElement_RefreshData',
            text: 'Обновить',
            handler: this.tpSystemGoalElement_RefreshDataButton,
            tooltip: 'Обновить реквизиты выделенных элементов СЦ'
        }, 1);

        var tbar2 = this.getTopToolBar('tpGoalIndicator');

        this.addNewButton(tbar2, {
            id: 'tpGoalIndicator_Value_FillData',
            text: 'Заполнить',
            handler: this.tpGoalIndicator_Value_FillDataButton,
            tooltip: 'Заполнить (обновить) целевые показатели  выделенных элементов СЦ по актуальному состоянию справочника «Система целеполагания».'
        }, 0);

        this.addNewButton(tbar2, {
            id: 'tpGoalIndicator_Value_RefreshData',
            text: 'Обновить',
            handler: this.tpGoalIndicator_Value_RefreshDataButton,
            tooltip: 'Обновить значения выделенных целевых показателей'
        }, 1);


        tbar3 = this.getTopToolBar('tpActivityResourceMaintenance');

        this.addNewButton(tbar3, {
            id: 'tpActivityResourceMaintenance_FillData',
            text: 'Заполнить из смет учреждений',
            handler: this.tpActivityResourceMaintenance_FillDataButton,
            tooltip: 'Заполнить из документа «Смета казенных учреждений»'
        }, 1);


        //На кнопку "Открыть" ТЧ "Элементы СЦ" вешаем открытие элемента справочника 
        var tpSystemGoalElementOpenBtn = this.getButton(tbar1, 'open');
        tpSystemGoalElementOpenBtn.handler = this.tpSystemGoalElementOpenBtnHandler;

        this.hideButtons(tbar2, 'create');
        //this.hideButtons(tbar2, 'create|delete');
    },

    tpSystemGoalElementOpenBtnHandler: function (sender) {
        //Проходим по всем выбранным записям ТЧ
        var sm = sender.scope.grid.getSelectionModel().getSelection();
        if (sm) {
            // Entity элемента справочника СЦ
            var systemGoalEntityId = -2013265862;
            var entity = App.EntitiesMgr.getEntityById(systemGoalEntityId);

            Ext.each(sm, function (item) {
                // id связанного элемента справочника
                var idSystemGoal = item.get('idsystemgoal');

                if (idSystemGoal) {
                    //Запрашиваем элемент
                    DataService.getItem(systemGoalEntityId, idSystemGoal,
                        function (result) {
                            Ext.each(result.result, function () {
                                //Открываем элементы в новых окнах
                                Ext.create("App.logic.EntityItem", {
                                    entity: entity,
                                    docid: this.id
                                });
                            });
                        }, this);
                }

            }, this);
        }
    },
    
    hideKbk: function () {
    if (this.getDocId())
        SborCommonService.getSbpBlank_ActivityOfSBP(this.getDocId(), null, false, function (result) { this.onLoadSbpBlank(result, 'tpActivityResourceMaintenance'); }, this);
    },

    
    onLoadSbpBlank: function (result, tParts) {
        if (result != null) {
            var me = this;
            var hiddenFields = result.hidden;
            var requiredFields = result.required;

            if (!Ext.isArray(tParts))
                tParts = [tParts];

            Ext.each(tParts, function(tPart) {
                if (Ext.isArray(hiddenFields) && hiddenFields.length > 0) {
                    me.hideKbkOnGrid(hiddenFields, requiredFields, tPart);
                }

            });
        }
    },
    
    hideKbkOnGrid: function (hiddenFields, requiredFields, gridName) {
        this.hideColumnsOnGrid(gridName, hiddenFields.concat(['idowner', 'idmaster', 'id']));
    },
    
    PermissionsInputAdditionalRequirements: function () {
        
        SborCommonService.getPermissionsInputAdditionalRequirements(this.getField('IdSBP').getValue(), function (result) { this.onPermissionsInputAdditionalRequirements(result); }, this);

    },
    
    onPermissionsInputAdditionalRequirements: function (result) {
        var HAN = this.getField('HasAdditionalNeed');

        var hmd = this.getField('HasMasterDoc').getValue();

        if (result && !hmd) {
            HAN.enable();
        } else {
            HAN.disable();
        }
    }
    
});

