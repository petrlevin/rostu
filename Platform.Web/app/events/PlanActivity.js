/**
* @class App.events.PlanActivity 
* Обработчик клиентских событий сущности документа "План деятельности"
*/
Ext.define('App.events.PlanActivity', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'itemloaded', handler: 'onItemLoaded', item: null },
        { name: 'operationbegin', handler: 'onOperationBegin', item: null },
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'change', handler: 'onChangeSBP', item: 'idSBP' },
        { name: 'change', handler: 'onChangeAdditionalNeed', item: 'isAdditionalNeed' }
    
    ],

    constructor: function (config) {
        this.callParent([config]);
    },

    
    onItemLoaded: function () {
        this.onChangeAdditionalNeed();
        this.onChangeSBP();
        this.PermissionsInputAdditionalRequirements();
        
        this.hideKbkOnGrid(['idactivityofsbp_a'], 'tpActivity');

    },
    
    onOperationBegin: function () {
        this.onChangeSBP();
        this.PermissionsInputAdditionalRequirements();
    },


    getDocId: function () {
        return this.getField('id').getValue();
    },

    onChangeAdditionalNeed: function (sender, newValue, oldValue) {
        var isAddNeed = this.getField('isAdditionalNeed').getValue();
        this.setVisisibilityColumnsOnGrid('tpActivityVolumes', ['additionalvolume'], !isAddNeed);
        this.setVisisibilityColumnsOnGrid('tpActivityVolumeAUBU', ['additionalvolume'], !isAddNeed);
        this.setVisisibilityColumnsOnGrid('tpIndicatorQualityActivityValues', ['additionalvalue'], !isAddNeed);
    },
    
    onChangeSBP: function (sender, newValue, oldValue) {
        var idSbp = this.getField('idSBP').getValue();
        if (idSbp && this.getDocId()) {
            DataService.getItem(-2013265882, idSbp, this.onLoadSBP, this);
            SborCommonService.getSbpBlank_PlanActivity(this.getDocId(), idSbp, function (result) { this.onLoadSbpBlank(result, 'tpKBKOfFinancialProvisions'); }, this);
        }
    },

    onLoadSBP: function (result, response) {
        var obj = result.result[0];

        var panelAUBU = this.getField('tpActivityAUBU').up('tabpanel').getTabBar().items.get(1);
        if (obj.idsbptype == 3 && obj.isfounder) { // Казенное учереждение и флаг учередитель
            panelAUBU.show();
        } else {
            panelAUBU.hide();
        }
        
        Ext.each(this.getField('tpKBKOfFinancialProvisions').grid.columns, function(column) {
            if (column.name.toLowerCase() == "ismeanaubu") {
                if (obj.idsbptype == 3 && obj.isfounder) {
                    column.show();
                } else {
                    column.hide();
                }
            }
        });
    },

    onLoadSbpBlank: function (result, tParts) {
        if (result != null) {
            var me = this;
            var hiddenFields = result.hidden;

            if (!Ext.isArray(tParts))
                tParts = [tParts];

            Ext.each(tParts, function(tPart) {
                if (Ext.isArray(hiddenFields) && hiddenFields.length > 0) {
                    me.hideKbkOnGrid(hiddenFields, tPart);
                }
            });
        }
    },
    

    hideKbkOnGrid: function (hiddenFields, gridName) {
        this.hideColumnsOnGrid(gridName, hiddenFields.concat(['idowner', 'idmaster', 'id', 'hasadditionalneed']));
    },
    
    callServiceFromGrid: function (service, gridRefresh) {
        var docid = this.getDocId();
        if (docid) {
            var refresh = function () {
                Ext.each(gridRefresh, function (name) { this.getField(name).refresh(); }, this);
            };
            service(docid, refresh, this);
        }
    },

    tpActivityAUBU_FillDataButton: function () {
        this.callServiceFromGrid(SborCommonService.fillData_PlanActivity_ActivityAUBU, ['tpActivityAUBU', 'tpActivityVolumeAUBU']);
    },

    onAfterRender: function (sender) {
        var tbar1 = this.getTopToolBar('tpActivityAUBU');
        this.addNewButton(tbar1, {
            id: 'tpActivityAUBU_FillData',
            text: 'Заполнить',
            handler: this.tpActivityAUBU_FillDataButton,
            tooltip: 'Заполнить (обновить) автоматически мероприятиями и объемами (с периодом Год) из документов «План деятельности» по автономным и бюджетным учреждениям.'
        }, 0);
        
        var tbar4 = this.getTopToolBar('tpActivity');

        this.addNewButton(tbar4, {
            id: 'tpPlanActivity_Activity_OpenLastVersion',
            text: 'Открыть документ',
            handler: this.OpenLastVersionBtnHandler,
            tooltip: 'Открыть документ',
            scope: this.getField('tpActivity').grid,
            setAvailability: function (entityItem) {
                this.enable();
            }
        }, 2);

        //this.hidePagging('tp');
    },


    OpenLastVersionBtnHandler: function (sender) {
        //Проходим по всем выбранным записям ТЧ
        var sm = sender.scope.getSelectionModel().getSelection();
        if (sm) {

            Ext.each(sm, function (item) {
                var idDocument = item.get('idactivityofsbp_a');
                var idDocumentEntity = -1543503797;

                var entity = App.EntitiesMgr.getEntityById(idDocumentEntity);

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

            }, this);
        }
    },
    
    PermissionsInputAdditionalRequirements: function () {

        var isRedact = !this.getButton(this.getTopToolBar('tpActivity'), 'create').disabled; // так я определяю режим редактирования
        if (isRedact)
            SborCommonService.getPermissionsInputAdditionalRequirements(this.getField('IdSBP').getValue(), function (result) { this.onPermissionsInputAdditionalRequirements(result); }, this);

    },
    onPermissionsInputAdditionalRequirements: function (result) {
        var HAN = this.getField('isAdditionalNeed');

        if (result) {
            HAN.enable();
        } else {
            HAN.disable();
        }
    }
    
});
