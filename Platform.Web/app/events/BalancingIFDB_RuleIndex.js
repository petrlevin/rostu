/**
* @class App.events.BalancingIFDB_RuleIndex
* Обработчик клиентских событий ТЧ "Примененные правила индексации" инструмента "Балансировка доходов, расходов и ИФДБ"
*/
Ext.define('App.events.BalancingIFDB_RuleIndex', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'dataget', handler: 'onDataget', item: null }
    ],
    
    //Основной сервис с которым работает Event
    docService: null,

    fields: ['Caption', 'IndexOFG', 'IndexPFG1', 'IndexPFG2', 'isIncludeAdditionalNeed', 'tpBalancingIFDB_RuleFilterKBK'],

    constructor: function (config) {
        this.docService = SborBalancingIFDB;
        
        this.callParent([config]);
    },
	
    BalancingIFDB_RuleIndex_ApplyButton: function () {
        this.getForm().saveElement();
        var refresh = function () {
            var entityItem = this.getForm();
            App.EntitiesMgr.registerUpdate(entityItem.entity.id, entityItem.docid);
            entityItem.refreshElement();
        };
        this.getForm().panel.getEl().mask();
        this.docService.applyRule({}, this.getField('id').getValue(), refresh, this);
        this.getForm().panel.getEl().unmask();
    },
    
    onDataget: function () {
        var tbar = this.getForm().panel.getDockedItems('toolbar[dock="bottom"]')[0];
        
        if (this.getField('isApplied').getValue()) {
            this.hideButtons(tbar, 'apply');
            Ext.each(this.fields, function (name) { this.getField(name).disable(); }, this);
        } else {
            Ext.each(this.fields, function (name) { if (this.getField('id').getValue()) { this.getField(name).enable(); } }, this);
            var apply = this.getButton(tbar, 'apply');
            if (apply == null) {
                this.addNewButton(tbar, {
                    id: 'apply',
                    text: 'Применить',
                    handler: this.BalancingIFDB_RuleIndex_ApplyButton,
                    tooltip: 'Применить правило'
                }, 1);
            } else {
                apply.show();
                apply.enable();
            }
        }
    }
    
});