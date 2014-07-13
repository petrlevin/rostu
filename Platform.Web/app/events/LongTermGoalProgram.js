/**
* @class App.events.LongTermGoalProgram
* Обработчик клиентских событий сущности документа "Долгосрочная целевая программа"
*/
Ext.define('App.events.LongTermGoalProgram', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'operationbegin', handler: 'onOperationBegin', item: null },
        { name: 'itemloaded', handler: 'onItemLoaded', item: null },
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'change', handler: 'onDataGet', item: 'idSBP' },
        { name: 'dataget', handler: 'onDataGet', item: null }
    ],
    
    tbar1: null, // tpSystemGoalElement
    tbar2: null, // tpGoalIndicator
    isSubDoc: null,
    IdTypeSubLTGP: '-1543503835',//Тип подпрограмма ДЦП
    HasMasterDoc: null,
    fldMSP: null,
    fldMLTGP: null,
    
    onOperationBegin: function() {
        this.onSelectionSystemGoalElement(this, []);
        this.UpdateVisual();
    },
    
    onDataGet: function () {
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

        if (isSubDoc) {
            this.controlButtons(tbar1, 'delete', isRedact && !selectedRecord.get('ismaingoal'));
        }

        tpa   = this.getField('tpActivity');
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
    },

    UpdateVisual: function() {
        var dt = this.getField('idDocType').getValue();

        var vkladka_pp = this.getField('tpListSubProgram').up('panel');
        var tpResourceMaintenance = this.getField('tpResourceMaintenance');
        fldMLTGP = this.getField('IdMasterLongTermGoalProgram');
        fldMSP = this.getField('IdMasterStateProgram');
        var fldHMD = this.getField('HasMasterDoc');
        HasMasterDoc = fldHMD.getValue();
        isSubDoc = (HasMasterDoc && fldMSP.getValue()) || fldMLTGP.getValue();

        if (dt == this.IdTypeSubLTGP || this.getField('HasMasterDoc').getValue() == true) {
            this.getField('Date').setReadOnly(true);
            this.getField('DateStart').setReadOnly(true);
            this.getField('DateEnd').setReadOnly(true);
            this.getField('IdVersion').setReadOnly(true);
            this.getField('idResponsibleExecutantType').setReadOnly(true);
            this.getField('IdSBP').setReadOnly(true);
            this.getField('Caption').setReadOnly(true);
            //this.getField('Caption').initialModel.readonly = true;
        }
        
        if (dt == -1543503837) { // Тип документа равен ДЦП
            vkladka_pp.enable();

            fldMSP.show();
            fldHMD.show();

            fldMLTGP.hide();
        } else {
            fldMLTGP.show();

            fldMSP.hide();
            fldHMD.hide();
            vkladka_pp.disable();
            HasMasterDoc = 1;
        }

        if (HasMasterDoc) {
            tpResourceMaintenance.disable();
        }
        
        this.PermissionsInputAdditionalRequirements();

    },

    onItemLoaded: function () {

        this.UpdateVisual();

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
        
        this.onSelectionSubProgram(this, []);
    },

    getDocId: function () {
        return this.getField('id').getValue();
    }, 


    tpSystemGoalElement_FillDataButton: function () {
        this.callServiceFromGrid(SborCommonService.fillData_LongTermGoalProgram_SystemGoalElement, ['tpSystemGoalElement', 'tpGoalIndicator']);
    },

    tpSystemGoalElement_RefreshDataButton: function () {
        this.callServiceFromGrid(SborCommonService.refreshData_LongTermGoalProgram_SystemGoalElement, ['tpSystemGoalElement'], 'tpSystemGoalElement');
    },

    tpGoalIndicator_FillDataButton: function () {
        this.callServiceFromGrid(SborCommonService.fillData_LongTermGoalProgram_GoalIndicator_Value, ['tpGoalIndicator'], 'tpSystemGoalElement');
    },

    tpGoalIndicator_RefreshDataButton: function () {
        this.callServiceFromGrid(SborCommonService.refreshData_LongTermGoalProgram_GoalIndicator_Value, ['tpGoalIndicator'], 'tpGoalIndicator');
    },

    onAfterRender: function (sender) {
        var me = this;

        tbar1 = this.getTopToolBar('tpSystemGoalElement');

        this.addNewButton(tbar1, {
            id:    'tpSystemGoalElement_RefreshData',
            text:    'Обновить',
            handler: this.tpSystemGoalElement_RefreshDataButton,
            tooltip: 'Обновить реквизиты выделенных элементов СЦ'
        }, 1);

        tbar2 = this.getTopToolBar('tpGoalIndicator');
        this.addNewButton(tbar2, {
            id: 'tpGoalIndicator_FillData',
            text: 'Заполнить',
            handler: this.tpGoalIndicator_FillDataButton,
            tooltip: 'Заполнить (обновить) целевые показатели  выделенных элементов СЦ по актуальному состоянию справочника «Система целеполагания».'
        }, 0);
        this.addNewButton(tbar2, {
            id: 'tpGoalIndicator_RefreshData',
            text: 'Обновить',
            handler: this.tpGoalIndicator_RefreshDataButton,
            tooltip: 'Обновить значения выделенных целевых показателей'
        }, 1);

        //Кнопка "Открыть документ" во вкладке "Мероприятия"
        tbar3 = this.getTopToolBar('tpListSubProgram');
        this.addNewButton(tbar3, {
            id: 'tpLongTermGoalProgram_ListSubProgram_OpenLastVersion',
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

        //На кнопку "Открыть" ТЧ "Элементы СЦ" вешаем открытие элемента справочника 
        var tpSystemGoalElementOpenBtn = this.getButton(tbar1, 'open');
        tpSystemGoalElementOpenBtn.handler = this.tpSystemGoalElementOpenBtnHandler;

        this.hidePagging('tpSBPs');
        this.hidePagging('tpListSubProgram');
        this.hidePagging('tpGoalIndicator');
        this.hidePagging('tpSubProgramResourceMaintenance');
        this.hidePagging('tpActivityResourceMaintenance');
        this.hidePagging('tpActivity');
        this.hidePagging('tpResourceMaintenance');
        this.hidePagging('tpIndicatorActivity');

        this.hideButtons(tbar2, 'create');
        
        var sge = this.getField('tpSystemGoalElement');
        sge.height = sge.height * 0.6;

    },
    
    tpSystemGoalElementOpenBtnHandler : function (sender) {
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
    
    onSelectionSubProgram: function(sender, selected) {
        if (!selected) return;
        
        var selectedRecord = selected[0];
        if (!selectedRecord) {
            selectedRecord = this.getField('tpListSubProgram').grid.getSelectionModel().getSelection()[0];
        }

        if (!selectedRecord) return;

        var tbarListSubProgram = this.getTopToolBar('tpListSubProgram');
        if (!selectedRecord.get('idactualdocument')) {
            this.getButton(tbarListSubProgram, 'tpLongTermGoalProgram_ListSubProgram_OpenLastVersion').disable();
        }else
            this.getButton(tbarListSubProgram, 'tpLongTermGoalProgram_ListSubProgram_OpenLastVersion').enable();
    },

        // Открывает документ, указанный в поле "Актуальный документ". Вызывается кнопкой "Открыть документ" во вкладке "Мероприятия"
    OpenLastVersionBtnHandler: function (sender) {
        //Проходим по всем выбранным записям ТЧ
        var sm = sender.scope.getSelectionModel().getSelection();
        if (sm) {

            Ext.each(sm, function (item) {
                // Entity документа
                var idDocumentEntity = item.get('iddocumententity');
                if (idDocumentEntity) {
                    var entity = App.EntitiesMgr.getEntityById(idDocumentEntity);

                    // id связанного элемента справочника
                    var idDocument = item.get('idactualdocument');
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
    
    PermissionsInputAdditionalRequirements: function () {

        SborCommonService.getPermissionsInputAdditionalRequirements(this.getField('IdSBP').getValue(), function (result) { this.onPermissionsInputAdditionalRequirements(result); }, this);

    },

    onPermissionsInputAdditionalRequirements: function (result) {
        var HAN = this.getField('HasAdditionalNeed');

        var hmd = this.getField('HasMasterDoc').getValue();
        var dt = this.getField('idDocType').getValue();
        var f = !(dt == this.IdTypeSubLTGP || hmd);

        if (result && f) {
            HAN.enable();
        } else {
            HAN.disable();
            if (f) {
                HAN.setValue(0);
            }
        }
    }
});