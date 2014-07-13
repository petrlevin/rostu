/**
* @class App.events.BalancingIFDB
* Обработчик клиентских событий сущности инструмента "Балансировка доходов, расходов и ИФДБ"
*/
Ext.define('App.events.BalancingIFDB', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'dataget', handler: 'onDataget', item: null },
        { name: 'afterrender', handler: 'onAfterRender', item: null }
    ],

    //Основной сервис с которым работает Event
    docService: null,

    constructor: function(config) {
        this.docService = SborBalancingIFDB;
        
        this.callParent([config]);
    },
	
    // загружен элемент
    onDataget: function (sender) {
        var docid = this.getField('id').getValue();
        if (docid) {
            this.docService.showHideFields_Expense(docid, this.onLoadFields, this);
        }
    },

    //Отрисовка формы
    onAfterRender: function () {
        this.getField('tpPrograms').grid.on('render', function(grid) {
            var docid = this.getField('id').getValue();
            if (docid) {
                this.docService.showHide_Programs(docid, this.onLoadStatusProgram, this);
            }
        }, this);
        
        //Программы/мероприятия
        var tbarPrograms = this.getTopToolBar('tpPrograms');
        this.addNewButton(tbarPrograms, {
            id: 'tpPrograms_FillData',
            text: 'Заполнить',
            handler: this.tpPrograms_FillDataButton,
            tooltip: 'Заполнить автоматически.'
        }, 0);
        this.addNewButton(tbarPrograms, {
            id: 'tpPrograms_NewRule',
            text: 'Новое правило',
            handler: this.tpPrograms_NewRuleButton,
            tooltip: 'Создать новое правило.'
        }, 1);
        this.hideButtons(tbarPrograms, 'create');
        this.hideButtons(tbarPrograms, 'delete');

        //Расходы
        var tbarExpenses = this.getTopToolBar('tpExpenses');
        this.addNewButton(tbarExpenses, {
            id: 'tpExpenses_FillData',
            text: 'Заполнить',
            handler: this.tpExpenses_FillDataButton,
            tooltip: 'Заполнить автоматически.'
        }, 0);
        this.addNewButton(tbarExpenses, {
            id: 'tpExpenses_NewRule',
            text: 'Новое правило',
            handler: this.tpExpenses_NewRuleButton,
            tooltip: 'Создать новое правило.'
        }, 1);
        this.hideButtons(tbarExpenses, 'create');
        this.hideButtons(tbarExpenses, 'delete');
    },
    
    // создать новое правило
    newRule: function() {
        var g = this.getField('tpRuleIndexs');
        Ext.create('App.logic.EntityItem', {
            parent_id: g.id, // this.id
            entity: App.EntitiesMgr.getEntityByName('BalancingIFDB_RuleIndex'),
            docid: null, // id,
            ownerid: undefined, // ownerid,
            defaults: { idowner: this.getField('id').getValue(), idowner_caption: "Балансировка доходов, расходов и ИФДБ" }, // values,
            gridsValues: g.getDependentGridValues(), // { keys: [] }, // temp,
            protoId: undefined, // protoId,
            selection_form: Ext.isDefined(this.selection_form),
            owner_form_id: this.owner_form_id || this.winId
        });
    },
    
    showHideTpPrograms: function(idToolType) {
        var idType = idToolType;
        if (!idType) {
            idType = this.getField('idBalancingIFDBType').getValue();
        }
        var grid1 = this.getField('tpPrograms');
        var grid2 = this.getField('tpExpenses');
        if (idType == 2) {
            grid1.show();
            grid2.setHeight(grid1.height);
        } else {
            grid1.hide();
            grid2.setHeight(grid1.height * 2);
        }
    },
    
    //Программы/мероприятия.Новое правило
    tpPrograms_NewRuleButton: function () {
        this.newRule();
    },

    //Расходы.Новое правило
    tpExpenses_NewRuleButton: function () {
        this.newRule();
    },
    
    //Программы/мероприятия.Заполнить
    tpPrograms_FillDataButton: function () {
        this.getForm().saveElement();
        this.callServiceFromGrid(this.docService.fillData, ['tpPrograms', 'tpExpenses']);
        this.docService.showHideFields_Expense(this.getField('id').getValue(), this.onLoadFields, this);
        this.showHideTpPrograms();
    },

    //Расходы.Заполнить
    tpExpenses_FillDataButton: function () {
        this.getForm().saveElement();
        this.callServiceFromGrid(this.docService.fillData, ['tpPrograms','tpExpenses']);
        this.docService.showHideFields_Expense(this.getField('id').getValue(), this.onLoadFields, this);
        this.showHideTpPrograms();
    },
    
    onLoadStatusProgram: function (result, response) {
        if (result) {
            this.getField('tpPrograms').grid.getSelectionModel().select(0);
            this.showHideTpPrograms(result);
        }
    },
    
    onLoadFields: function (result, response) {
        var showFields = result.showFields;
        if (Ext.isArray(showFields) && showFields.length > 0) {
            this.setVisisibilityColumnsOnGrid('tpExpenses', showFields, false);
        }
        
        var hideFields = result.hideFields;
        if (Ext.isArray(hideFields) && hideFields.length > 0) {
            this.setVisisibilityColumnsOnGrid('tpExpenses', hideFields, true);
        }
    }
});
