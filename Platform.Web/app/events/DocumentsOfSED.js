/**
* @class App.events.DocumentsOfSED
* Обработчик клиентских событий сущности документа "Документы СЭР"
*/
Ext.define('App.events.DocumentsOfSED', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'itemloaded', handler: 'onItemLoaded', item: null }
    ],
    
    getDocId: function () {
        return this.getField('id').getValue();
    },

    onItemLoaded: function() {
        var gridSGE = this.getField("tpItemsSystemGoals").grid;

        if (gridSGE) {
            gridSGE.on('itemexpand', function (item) {
                if (item.isRoot()) {
                    item.expandChildren(true);
                }
            }, this);
        }
    },
    
    tpItemsSystemGoals_FillDataButton: function () {
        this.callServiceFromGrid(SborCommonService.fillData_tpDocumentsOfSED_ItemsSystemGoals, ['tpItemsSystemGoals','tpGoalIndicators']);
    },

    tpItemsSystemGoals_RefreshDataButton: function () {
        this.callServiceFromGrid(SborCommonService.refreshData_tpDocumentsOfSED_ItemsSystemGoals, ['tpItemsSystemGoals'], 'tpItemsSystemGoals');
    },

    tpGoalIndicatorValues_FillDataButton: function () {
        this.callServiceFromGrid(SborCommonService.fillData_tpDocumentsOfSED_GoalIndicatorValues, ['tpGoalIndicators'], 'tpItemsSystemGoals');
    },

    tpGoalIndicatorValues_RefreshDataButton: function () {
        this.callServiceFromGrid(SborCommonService.refreshData_tpDocumentsOfSED_GoalIndicatorValues, ['tpGoalIndicators'], 'tpGoalIndicators');
    },

    onAfterRender: function (sender) {
        var tbar1 = this.getTopToolBar('tpItemsSystemGoals');
        this.addNewButton(tbar1, {
            id: 'tpItemsSystemGoals_FillData',
            text: 'Заполнить',
            handler: this.tpItemsSystemGoals_FillDataButton,
            tooltip: 'Заполнить (обновить) автоматически по актуальному состоянию справочника «Система целеполагания».'
        }, 0);
        this.addNewButton(tbar1, {
            id: 'tpItemsSystemGoals_RefreshData',
            text: 'Обновить',
            handler: this.tpItemsSystemGoals_RefreshDataButton,
            tooltip: 'Обновить реквизиты выделенных элементов СЦ'
        }, 1);

        var tbar2 = this.getTopToolBar('tpGoalIndicators');

        this.addNewButton(tbar2, {
            id: 'tpGoalIndicatorValues_FillData',
            text: 'Заполнить',
            handler: this.tpGoalIndicatorValues_FillDataButton,
            tooltip: 'Заполнить (обновить) целевые показатели  выделенных элементов СЦ по актуальному состоянию справочника «Система целеполагания».'
        }, 0);
        this.addNewButton(tbar2, {
            id: 'tpGoalIndicatorValues_RefreshData',
            text: 'Обновить',
            handler: this.tpGoalIndicatorValues_RefreshDataButton,
            tooltip: 'Обновить значения выделенных целевых показателей'
        }, 1);

        //На кнопку "Открыть" ТЧ "Элементы СЦ" вешаем открытие элемента справочника 
        var tpSystemGoalElementOpenBtn = this.getButton(tbar1, 'open');
        tpSystemGoalElementOpenBtn.handler = this.tpSystemGoalElementOpenBtnHandler;
        
        this.hidePagging('tpGoalIndicators');

        this.hideButtons(tbar2, 'create|delete');
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
    }
});