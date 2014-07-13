/**
* @class App.events.PublicInstitutionEstimate
* Обработчик клиентских событий сущности документа "Смета казенного учреждения"
*/
Ext.define('App.events.PublicInstitutionEstimate', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'operationbegin', handler: 'onOperationBegin', item: null },
        { name: 'change', handler: 'onSBPChange', item: 'idSBP' },
        { name: 'change', handler: 'onHasAdditionalChanged', item: 'HasAdditionalNeed' },
        { name: 'dataget', handler: 'onDataget', item: null },
        { name: 'itemloaded', handler: 'onItemLoaded', item: null }
    ],

    //Основной сервис с которым работает Event
    docService: null,

    distributionMethodM1: 1,
    distributionMethodM2: 2,
    distributionMethodM3: 3,
    distributionMethodM4: 4,
    distributionMethodM5: 5,

	constructor: function(config){
			this.docService = SborPublicInstitutionEstimateService;
			
			this.callParent([config]);
	},
    
	onOperationBegin: function () {
	    this.hideKbk();
	    this.PermissionsInputAdditionalRequirements();
	},
	
    //Получение данных
	onDataget: function () {
	    this.PermissionsInputAdditionalRequirements();
        var sge = this.getField('tpDistributionMethods');
        sge.grid.getSelectionModel().on('selectionchange', this.onSelectionDistributionMethod, this);
    },
    
    onItemLoaded: function () {
        this.PermissionsInputAdditionalRequirements();
    },

    //Отрисовка формы
    onAfterRender: function() {
		//Мероприятия
        var tbarActivities = this.getTopToolBar('tpActivities');
        this.addNewButton(tbarActivities, {
            id: 'tpActivities_FillData',
            text: 'Заполнить',
            handler: this.tpActivities_FillDataButton,
            tooltip: 'Заполнить автоматически.'
        }, 0);
        this.hideButtons(tbarActivities, 'create');
        this.addNewButton(tbarActivities, {
            id: 'tpActivities_calculateRoButton',
            text: 'Вычислить коды РО',
            handler: this.tpActivities_calculateRoButton
        }, 1);

        //Мероприятия АуБу
        var tbarActivitiesAUBU = this.getTopToolBar('tpActivitiesAUBU');
        this.addNewButton(tbarActivitiesAUBU, {
            id: 'tpActivitiesAUBU_FillData',
            text: 'Заполнить',
            handler: this.tpActivitiesAUBU_FillDataButton,
            tooltip: 'Заполнить автоматически.'
        }, 0);
        this.addNewButton(tbarActivitiesAUBU, {
            id: 'tpActivitiesAUBU_calculateRoButton',
            text: 'Вычислить коды РО',
            handler: this.tpActivitiesAUBU_calculateRoButton
        }, 1);
        this.hideButtons(tbarActivitiesAUBU, 'create');

        //Расходы 
        var tbarExpenses = this.getTopToolBar('tpExpenses');
        this.addNewButton(tbarExpenses, {
            id: 'tpExpenses_calculateRoButton',
            text: 'Вычислить коды РО',
            handler: this.tpExpenses_calculateRoButton
        }, 2);

        //Расходы казенного учреждения
        var tbarAloneAndBudgetInstitutionExpenses = this.getTopToolBar('tpAloneAndBudgetInstitutionExpenses');
        this.addNewButton(tbarAloneAndBudgetInstitutionExpenses, {
            id: 'tpAloneAndBudgetInstitutionExpenses_FillData',
            text: 'Заполнить',
            handler: this.tpAloneAndBudgetInstitutionExpenses_FillDataButton,
            tooltip: 'Заполнить автоматически.'
        }, 0);
        this.hideButtons(tbarAloneAndBudgetInstitutionExpenses, 'create');

        //Расходы учредителя
        var tbarFounderAUBUExpenses = this.getTopToolBar('tpFounderAUBUExpenses');
        this.addNewButton(tbarFounderAUBUExpenses, {
            id: 'tpFounderAUBUExpenses_FillData',
            text: 'Заполнить на основании ПФХД',
            handler: this.tpFounderAUBUExpenses_FillDataButton,
            tooltip: 'Заполнить автоматически на основании ПФХД.'
        }, 0);
        this.addNewButton(tbarFounderAUBUExpenses, {
            id: 'tpFounderAUBUExpenses_calculateRoButton',
            text: 'Вычислить коды РО',
            handler: this.tpFounderAUBUExpenses_calculateRoButton
        }, 1);

        //Методы распределения косвенных расходов
        var tbarDistributionMethods = this.getTopToolBar('tpDistributionMethods');
        this.addNewButton(tbarDistributionMethods, {
            id: 'calculateInderectExpensesButton',
            text: 'Рассчитать косвенные',
            handler: this.calculateInderectExpenses
        }, 10);

        /*
        * При удалении строки из ТЧ «Методы распределения», найти и удалить в ТЧ Расходы строки, 
        * где «Метод» = выбранный метод распределения. Предварительно выдать сообщение: 
        * "Все строки расходов, распределенные по удаляемому методу, будут удалены из документа".
        */
        var deleteDistributionMethodBtn = this.getButton(tbarDistributionMethods, 'delete');
		var tpDistributionMethodsList = this.getField('tpDistributionMethods').list;
        deleteDistributionMethodBtn.setHandler(Ext.bind(tpDistributionMethodsList.deleteItem, tpDistributionMethodsList, ['Все строки расходов, распределенные по удаляемому методу, будут удалены из документа. Продолжить?'], true));
        
        //При выборе метода распределения -- скрываем лишние колонки
        var sge = this.getField('tpDistributionMethods');
        sge.grid.getSelectionModel().on('selectionchange', this.onSelectionDistributionMethod, this);
        sge.grid.searchField.hide();
        
        //Скрываем поле поиска на доп. параметрах распределения
        var addField = this.getField('tpDistributionAdditionalParams');
        addField.grid.searchField.hide();

    },

    //Выбор метода распределения
    onSelectionDistributionMethod: function (sender, selected) {
        if (!selected) return;

        var selectedRecord = selected[0];
        if (!selectedRecord) {
            selectedRecord = this.getField('tpDistributionMethods').grid.getSelectionModel().getSelection()[0];
        }

        if (!selectedRecord) return;
        var distributiveMethod = selectedRecord.get('idindirectcostsdistributionmethod');

        //Колонки, видимые во всех случаях
        var visibleColumns = ['idpublicinstitutionestimate_activity', 'idactivitytype', 'idcontingent'];
        //Добавляем колонки в зависимости от метода
        switch (distributiveMethod) {
            case this.distributionMethodM1:
                break;
            case this.distributionMethodM2:
                visibleColumns = visibleColumns.concat(['directofg', 'directpfg1', 'directpfg2']);
                break;
            case this.distributionMethodM3:
                visibleColumns = visibleColumns.concat(['volumeofg', 'volumepfg1', 'volumepfg2']);
                break;
            case this.distributionMethodM4:
                visibleColumns = visibleColumns.concat(['factorofg', 'factorpfg1', 'factorpfg2']);
                break;
            case this.distributionMethodM5:
                visibleColumns = visibleColumns.concat(['directofg', 'directpfg1', 'directpfg2']);
                break;
        }
        this.hideColumnsOnGrid('tpDistributionActivities', null, visibleColumns);

        var tbarAdditionalMethods = this.getTopToolBar('tpDistributionAdditionalParams');
        //Если метод != M5, скрываем кнопку 'Добавить'
        if (distributiveMethod == this.distributionMethodM5)
            this.getButton(tbarAdditionalMethods, 'create').enable();
        else
            this.getButton(tbarAdditionalMethods, 'create').disable();

    },

    //Смена СБП
    onSBPChange: function(sender) {
        //Сюда зайдем и при выборе элемента и при загрузке страницы
        var isFounder = this.getField('isSBPFounder').getValue();
        //Значение было изменено
        if (sender.list)
            isFounder = sender.list.grid.getSelectionModel().getLastSelected().get('isfounder');

        var panelExpensesAuBu = this.getField('tpActivitiesAUBU').up('panel');

        //todo: разобраться почему не работает hide()
        if (!isFounder)
            //panelExpensesAuBu.hide();
            panelExpensesAuBu.disable();
        else {
            /*if (panelExpensesAuBu.hidden) {
                panelExpensesAuBu.show();
            }*/
            panelExpensesAuBu.enable();
        }

        this.hideKbk();

    },
    
    //Загрузился бланк СБП
    onLoadSbpBlank: function (result, tParts) {
        if (result != null) {
            var me = this;
            var hiddenFields = result.hidden;

            if (!Ext.isArray(tParts))
                tParts = [tParts];

            Ext.each(tParts, function(tPart) {
                if (Ext.isArray(hiddenFields)) {
                    me.hideKbkOnGrid(hiddenFields, tPart);
                }
            });
        }
    },

    //Смена доп. потребности
    onHasAdditionalChanged: function (sender, newvalue) {
        if (newvalue != null) {
            this.hideAdditionalsColumns('tpExpenses');
            this.hideAdditionalsColumns('tpFounderAUBUExpenses');
        }
    },

    hideKbk: function() {
        var docId = this.getField('id').getValue();

        //У нас разные ТЧ сворачиаются по 3м разным бланкам
        if (docId) {
            //Формирование КУ
            this.docService.get_Sbpblank(docId, function (result) { this.onLoadSbpBlank(result, ['tpFounderAUBUExpenses', 'tpIndirectExpenses', 'tpExpenses']); }, this);
            //Формирование АУБУ
            this.docService.get_SbpblankAuBu(docId, function (result) { this.onLoadSbpBlank(result, 'tpAloneAndBudgetInstitutionExpenses'); }, this);
        }
    },

    hideKbkOnGrid: function(hiddenFields, gridName) {
        this.hideColumnsOnGrid(gridName, hiddenFields.concat(['idowner', 'idmaster', 'id', 'hasadditionalneed']));
        this.hideAdditionalsColumns(gridName);
    },

    hideAdditionalsColumns: function(gridName) {
        var hasAdditionals = this.getField('HasAdditionalNeed').getValue();
        this.setVisisibilityColumnsOnGrid(gridName, ['additionalofg', 'additionalpfg1', 'additionalpfg2'], !hasAdditionals);
    },

    getDocId: function () {
        return this.getField('id').getValue();
    },

    /*
    / #region хэндлеры кастомных кнопок 
    */
    //Мероприятия.Заполнить
    tpActivities_FillDataButton: function () {
        this.callServiceFromGrid(this.docService.fillData_PublicInstitutionEstimate_Activities, 'tpActivities');
    },
  
    //МероприятияАуБу.Заполнить
    tpActivitiesAUBU_FillDataButton: function () {
        this.callServiceFromGrid(this.docService.fillData_PublicInstitutionEstimate_ActivitiesAUBU, 'tpActivitiesAUBU');
    },

    //РасходыАуБу.Заполнить
    tpAloneAndBudgetInstitutionExpenses_FillDataButton: function () {
        this.callServiceFromGrid(this.docService.fillData_PublicInstitutionEstimate_ExpenseAloneSubjects, ['tpAloneAndBudgetInstitutionExpenses', 'tpActivitiesAUBU']);
    },
    
    //Мероприятия.ВычислитьКодыРО
    tpActivities_calculateRoButton: function () {
        this.callServiceFromGrid(this.docService.calculateRo_PublicInstitutionEstimate_Activities, ['tpActivities'], 'tpActivities');
    },
    //МероприятияАуБу.ВычислитьКодыРО
    tpActivitiesAUBU_calculateRoButton: function () {
        this.callServiceFromGrid(this.docService.calculateRo_PublicInstitutionEstimate_ActivitiesAUBU, ['tpActivitiesAUBU'], 'tpActivitiesAUBU');
    },
    //Расходы.ВычислитьКодыРО
    tpExpenses_calculateRoButton: function () {
        this.callServiceFromGrid(this.docService.calculateRo_PublicInstitutionEstimate_Expenses, ['tpExpenses'], 'tpExpenses');
    },
    //РасходыУчередителяАуБу.ВычислитьКодыРО
    tpFounderAUBUExpenses_calculateRoButton: function () {
        this.callServiceFromGrid(this.docService.calculateRo_PublicInstitutionEstimate_FounderAUBUExpenses, ['tpFounderAUBUExpenses'], 'tpFounderAUBUExpenses');
    },

    //РасходыУчередителяАуБу.Заполнить.Подтверждение очистки
    tpFounderAUBUExpenses_FillDataButton: function () {
        var me = this;
        if (this.getField('tpAloneAndBudgetInstitutionExpenses').grid.getView().store.data.length > 0)
            Ext.Msg.show({
                title: 'Очистить данные?',
                msg: 'Данные в таблице будут очищены. Продолжить?',
                buttons: Ext.MessageBox.YESNO,
                icon: Ext.Msg.QUESTION,
                fn: function (btn) {
                    if (btn == 'yes')
                        me.fillData_PublicInstitutionEstimate_FounderAUBUExpenses_ok();
                }
            });
        else
            me.fillData_PublicInstitutionEstimate_FounderAUBUExpenses_ok();
    },

    //РасходыУчередителяАуБу.Заполнить
    fillData_PublicInstitutionEstimate_FounderAUBUExpenses_ok: function () {
        this.callServiceFromGrid(this.docService.fillData_PublicInstitutionEstimate_FounderAUBUExpenses, 'tpFounderAUBUExpenses', 'tpActivitiesAUBU');
    },

    //Косвенные расходы. Рассчитать косвенные
    calculateInderectExpenses: function () {
        var me = this;
        
        if (!this.getField('tpDistributionActivities').grid.getView().store.data.length) {
            Ext.Msg.show({
                title: '',
                msg: 'В таблице «Мероприятия для распределения» отсутствуют строки.',
                buttons: Ext.MessageBox.YES,
                icon: Ext.Msg.WARNING
            });
            return;
        }
        
        if (!this.getField('tpIndirectExpenses').grid.getView().store.data.length) {
            Ext.Msg.show({
                title: '',
                msg: 'В таблице «Косвенные расходы» отсутствуют строки.',
                buttons: Ext.MessageBox.YES,
                icon: Ext.Msg.WARNING
            });
            return;
        }
        
        var sge = this.getField('tpDistributionMethods');
        var noErrors = true;
        Ext.each(sge.grid.getSelectionModel().getSelection(), function(selection) {
            if (selection.get('idindirectcostsdistributionmethod') == me.distributionMethodM5) {
                if (!me.getField('tpDistributionAdditionalParams').grid.getView().store.data.length) {
                    Ext.Msg.show({
                        title: '',
                        msg: 'В таблице «Дополнительный параметр распределения» отсутствуют строки.',
                        buttons: Ext.MessageBox.YES,
                        icon: Ext.Msg.WARNING
                    });
                    noErrors = false;
                    return;
                } else
                    noErrors = true;
            }
        });
        
        if (noErrors)
            this.callServiceFromGrid(this.docService.calculateIndirectExpenses, ['tpDistributionActivities', 'tpExpenses', 'tpDistributionMethods'], 'tpDistributionMethods');
        
        
    },

    PermissionsInputAdditionalRequirements: function () {

        var isRedact = !this.getButton(this.getTopToolBar('tpActivities'), 'create').disabled; // так я определяю режим редактирования
        if (isRedact)
            SborCommonService.getPermissionsInputAdditionalRequirements(this.getField('IdSBP').getValue(), function (result) { this.onPermissionsInputAdditionalRequirements(result); }, this);

    },
    onPermissionsInputAdditionalRequirements: function (result) {
        var HAN = this.getField('HasAdditionalNeed');

        if (result) {
            HAN.enable();
        } else {
            HAN.disable();
        }
    }
    /*
    / #endregion
    */
});
