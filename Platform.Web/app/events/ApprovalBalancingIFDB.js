/**
* @class App.events.ApprovalBalancingIFDB
* Обработчик клиентских событий сущности инструмента "Утверждение балансировки расходов, доходов и ИФДБ"
*/
Ext.define('App.events.ApprovalBalancingIFDB', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'afterrender', handler: 'onAfterRender', item: null }
    ],

    //Основной сервис с которым работает Event
    toolService: null,

    constructor: function (config) {
        this.toolService = SborBalancingIFDB;

        this.callParent([config]);
    },

    //Отрисовка формы
    onAfterRender: function () {
        var mls = ['mlActivityOfSBPs', 'mlLimitBudgetAllocations'];
        Ext.each(mls, function (ml) {
            var tbar = this.getTopToolBar(ml);
            
            this.hideButtons(tbar, 'create');
            this.hideButtons(tbar, 'delete');

            //this.addNewButton(tbar, {
            //    id: ml + '_ProcessDocs',
            //    text: 'Провести документы',
            //    handler: this.mlXXXX_ProcessDocs,
            //    tooltip: 'Последовательно выполнить над выделенными документами операции «Обработать», «Утвердить»',
            //    myGridName: ml,
            //    myFuncName: "processDocs_" + ml
            //}, 0);
        }, this);

        var ml = 'mlLimitBudgetAllocations';
        var tbar = this.getTopToolBar(ml);
        this.addNewButton(tbar, {
            id: ml + '_ProcessDocs',
            text: 'Провести документы',
            handler: this.mlXXXX_ProcessDocs,
            tooltip: 'Последовательно выполнить над выделенными документами операции «Обработать», «Утвердить»',
            myGridName: ml,
            myFuncName: "processDocs_" + ml
        }, 0);
    },
    
    mlXXXX_ProcessDocs: function (sender) {
        this.callServiceFromGrid(this.toolService[sender.myFuncName], [sender.myGridName], sender.myGridName);
    }
});
