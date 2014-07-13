/**
* @class App.events.Activity
* Обработчик клиентских событий сущности testDependantTablefields Тест связанных ТЧ
*/
Ext.define('App.events.testDependantTableFields', {
    extend: 'App.events.CommonItem',

    events: [
	    { name: 'afterrender', handler: 'onAfterRender', item: null }
    ],

    onAfterRender: function (sender) {

        var grid = this.getField('tpA').grid;
        grid.getView().getRowClass = function(rec, rowIdx, params, store) {
            return rec.get('name') == 'red' ? 'row-red' : '';
        };
    }
});