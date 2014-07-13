/**
* @class App.events.StateProgram
* Обработчик клиентских событий сущности документа "Государственная программа"
*/
Ext.define('App.events.StateProgram', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'operationbegin', handler: 'onOperationBegin', item: null },
        { name: 'change', handler: 'onChangeAnalyticalCodeStateProgram', item: 'idAnalyticalCodeStateProgram' },
        { name: 'itemloaded', handler: 'onItemLoaded', item: null },
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'change', handler: 'onDataGet', item: 'idSBP' },
        { name: 'dataget', handler: 'onDataGet', item: null }
    ],

    tbar1: null, // tpSystemGoalElement

    onOperationBegin: function () {
        this.UpdateVisual();
    },

    onDataGet: function () {
        this.PermissionsInputAdditionalRequirements();
    },
    
    onChangeAnalyticalCodeStateProgram: function (sender, newValue, oldValue) {
        var acsp = sender.getValue();
        if (sender.list && acsp) {
            DataService.getItem(sender.initialModel.identitylink, acsp, this.onLoadOrg, this);
        }
    },

    UpdateVisual: function () {
        var IdTypeSubSP = '-1543503842';//Тип подпрограмма ГП

        var dt = this.getField('idDocType').getValue();

        if (dt == IdTypeSubSP) {
            this.getField('Date').setReadOnly(true);
            this.getField('DateStart').setReadOnly(true);
            this.getField('DateEnd').setReadOnly(true);
            this.getField('IdVersion').setReadOnly(true);
            this.getField('idResponsibleExecutantType').setReadOnly(true);
            this.getField('IdSBP').setReadOnly(true);
            this.getField('Caption').setReadOnly(true);

            var tpResourceMaintenance = this.getTopToolBar('tpResourceMaintenance');
            this.hideButtons(tpResourceMaintenance, 'create');
            this.hideButtons(tpResourceMaintenance, 'open');
            this.hideButtons(tpResourceMaintenance, 'delete');
            
            var field = this.getField('tpResourceMaintenance');
            field.grid.removeListener('itemdblclick', field.list.onItemDblClick, field.list);
        }

        var vkladka_pp = this.getField('tpListSubProgram').up('panel');

        if (dt != IdTypeSubSP) { // Тип документа равен Государственная программа
            vkladka_pp.enable();

            this.getField('idMasterDoc').hide();
        } else {
            if (dt == IdTypeSubSP) { // Тип документа равен Подпрограмма ГП
                //this.getField('idVersion').setReadOnly();
                //this.getField('Date').readOnly = true;
                //this.getField('DateStart').readOnly = true;
                //this.getField('DateEnd').readOnly = true;
            }
            this.getField('idAnalyticalCodeStateProgram').disable();
            vkladka_pp.disable();
            this.getField('idMasterDoc').show();
        }
        
        this.hideKbkOnGrid(['idactivedocument', 'idDocumentEntity'], 'tpListSubProgram');
        this.hideKbkOnGrid(['idactivedocument', 'idDocumentEntity'], 'tpDepartmentGoalProgramAndKeyActivity');
        
        this.PermissionsInputAdditionalRequirements();

    },
    
    hideKbkOnGrid: function (hiddenFields, gridName) {
        this.hideColumnsOnGrid(gridName, hiddenFields.concat(['idowner', 'id']));
    },
    
    onItemLoaded: function () {
        this.UpdateVisual();
        
        this.onSelectionSubProgram(this, []);
        this.onSelectionKeyActivity(this, []);

        var gridSGE = this.getField("tpSystemGoalElement").grid;
        if (gridSGE){ 
            gridSGE.on('itemexpand', function (item) {
                if (item.isRoot()) {
                    item.expandChildren(true);
                }
            }, this);
        } 
    },
    
    onLoadOrg: function (result, response) {
        this.getField('Caption').setValue(result.result[0].caption);
    },

    getDocId: function () {
        return this.getField('id').getValue();
    }, 


    tpSystemGoalElement_FillDataButton: function () {
        this.callServiceFromGrid(SborCommonService.fillData_StateProgram_SystemGoalElement, ['tpSystemGoalElement', 'tpGoalIndicator']);
    },

    tpSystemGoalElement_RefreshDataButton: function () {
        this.callServiceFromGrid(SborCommonService.refreshData_StateProgram_SystemGoalElement, ['tpSystemGoalElement'], 'tpSystemGoalElement');
    },

    tpGoalIndicator_Value_FillDataButton: function () {
        this.callServiceFromGrid(SborCommonService.fillData_StateProgram_GoalIndicator_Value, ['tpGoalIndicator'], 'tpSystemGoalElement');
    },

    tpGoalIndicator_Value_RefreshDataButton: function () {
        this.callServiceFromGrid(SborCommonService.refreshData_StateProgram_GoalIndicator_Value, ['tpGoalIndicator'], 'tpGoalIndicator');
    },

    onAfterRender: function (sender) {
        var me = this;
        this.customize_tpSystemGoalElement();
        
        tbar1 = this.getTopToolBar('tpSystemGoalElement');
        var tbar2 = this.getTopToolBar('tpGoalIndicator');
        this.hideButtons(tbar2, 'create');
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

        var tbar3 = this.getTopToolBar('tpListSubProgram');
        this.hideButtons(tbar3, 'create');

        this.addNewButton(tbar3, {
            id: 'tpStateProgram_ListSubProgram_OpenLastVersion',
            text: 'Открыть документ',
            handler: this.OpenLastVersionBtnHandler,
            tooltip: 'Открыть документ',
            scope: this.getField('tpListSubProgram').grid,
            setAvailability: function (entityItem) {
                var selectedDoc = me.getField('tpListSubProgram').grid.getSelectionModel().getSelection()[0];
                if (selectedDoc && !selectedDoc.get('idactivedocument'))
                    this.disable();
                else
                    this.enable();
            }
        }, 2);
        
        var tpListSubProgram = this.getField('tpListSubProgram');
        tpListSubProgram.grid.getSelectionModel().on('selectionchange', this.onSelectionSubProgram, this);
        
        var menu = Ext.create("Ext.menu.Menu");
        menu.add(Ext.create("Ext.menu.Item", {
            text:    'Добавить существующую',
            handler: this.addExistedSubProgram,
            scope:   this
        }));
        this.addNewButton(tbar3, {
            id:    'tpStateProgram_ListSubProgram_Add', 
            text: 'Добавить новую', 
            handler: this.getButton(tbar3, 'create').handler.bind(this.getButton(tbar3, 'create').scope),
            tooltip: 'Добавить новую',
            xtype: 'splitbutton',
            menu: menu
        }, 1);
        
        var tbar4 = this.getTopToolBar('tpDepartmentGoalProgramAndKeyActivity');
        
        this.addNewButton(tbar4, {
            id: 'tpStateProgram_DepartmentGoalProgramAndKeyActivity_OpenLastVersion',
            text: 'Открыть документ',
            handler: this.OpenLastVersionBtnHandler,
            tooltip: 'Открыть документ',
            scope: this.getField('tpDepartmentGoalProgramAndKeyActivity').grid,
            setAvailability: function (entityItem) {
                var selectedDoc = me.getField('tpDepartmentGoalProgramAndKeyActivity').grid.getSelectionModel().getSelection()[0];
                if (selectedDoc && !selectedDoc.get('idactivedocument'))
                    this.disable();
                else
                    this.enable();
            }
        }, 2);
        
        var tpKeyActivity = this.getField('tpDepartmentGoalProgramAndKeyActivity');
        tpKeyActivity.grid.getSelectionModel().on('selectionchange', this.onSelectionKeyActivity, this);

    },
    
    customize_tpSystemGoalElement: function() {
        
        var tbar1 = this.getTopToolBar('tpSystemGoalElement');

        this.addNewButton(tbar1, {
            id: 'tpSystemGoalElement_RefreshData',
            text: 'Обновить',
            handler: this.tpSystemGoalElement_RefreshDataButton,
            tooltip: 'Обновить реквизиты выделенных элементов СЦ'
        }, 1);
        
        //На кнопку "Открыть" ТЧ "Элементы СЦ" вешаем открытие элемента справочника 
        var tpSystemGoalElementOpenBtn = this.getButton(tbar1, 'open');
        tpSystemGoalElementOpenBtn.handler = this.tpSystemGoalElementOpenBtnHandler;

        // удаляем стандартный обработчик события открытия элемента по двойному щелчку
        var field = this.getField('tpSystemGoalElement');
        field.grid.removeListener('itemdblclick', field.list.onItemDblClick, field.list);
    },

    addExistedSubProgram:function() {
        var dialog = Ext.create("App.logic.EntitiesList", {
            direct_function: 'SborCommonService.getListSubProgramExisted',
            entity_id: -1543503801,
            iconCls: 'icon_selection',
            formName: 'ForStateProgram',
            openAsWindow: true,
            modal: true,
            formType: 3
        });

        dialog.on('beforerequest', function (sender, proxy) {

            proxy.setExtraParam("docId",this.getField('id').getValue());

        }, this);

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
    
    OpenLastVersionBtnHandler: function(sender) {
        //Проходим по всем выбранным записям ТЧ
        var sm = sender.scope.getSelectionModel().getSelection();
        if (sm) {
            
            Ext.each(sm, function (item) {
                // Entity документа
                var idDocumentEntity = item.get('iddocumententity');
                if (idDocumentEntity) {
                    var entity = App.EntitiesMgr.getEntityById(idDocumentEntity);

                    // id связанного элемента справочника
                    var idDocument = item.get('idactivedocument');
                    if (idDocument == null) {
                        Ext.Msg.show({
                            title: 'Открыть документ невозможно',
                            msg: 'Поле "Актуальный документ" у записи ТЧ не заполнено',
                            width: 300,
                            buttons: Ext.Msg.OK,
                            icon: Ext.window.MessageBox.INFO
                        });
                    }
                    
                    if (entity && idDocument) {
                        //Запрашиваем элемент
                        DataService.getItem(idDocumentEntity, idDocument,
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
                }
            }, this);
        }
    },

    onSelectionSubProgram: function (sender, selected) {
        this.onSelectionWithActualDocument(sender, selected, 'ListSubProgram');
    },
    
    onSelectionKeyActivity: function(sender, selected) {
        this.onSelectionWithActualDocument(sender, selected, 'DepartmentGoalProgramAndKeyActivity');
    },
    
    onSelectionWithActualDocument : function(sender, selected, tpKey) {
        if (!selected) return;

        var selectedRecord = selected[0];
        if (!selectedRecord) {
            selectedRecord = this.getField('tp' + tpKey).grid.getSelectionModel().getSelection()[0];
        }

        if (!selectedRecord) return;

        var tbarListSubProgram = this.getTopToolBar('tp' + tpKey);
        if (!selectedRecord.get('idactivedocument')) {
            this.getButton(tbarListSubProgram, 'tpStateProgram_' + tpKey + '_OpenLastVersion').disable();
        } else
            this.getButton(tbarListSubProgram, 'tpStateProgram_' + tpKey + '_OpenLastVersion').enable();
    },
    
    PermissionsInputAdditionalRequirements: function () {

        var IdTypeSubSP = '-1543503842';//Тип подпрограмма ГП

        var dt = this.getField('idDocType').getValue();

        var HAN = this.getField('HasAdditionalNeed');

        if (dt != IdTypeSubSP) {
            HAN.enable();
        } else {
            HAN.disable();
        }

    }
});