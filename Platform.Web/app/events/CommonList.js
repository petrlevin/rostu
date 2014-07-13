/**
* @class App.events.CommonList
* Description
*/
Ext.define('App.events.CommonList', {

   extend: 'App.events.Common',

   getGrid: function () {
        var form = this.getForm();
        //<debug>
        if (Ext.isEmpty(form)) {
            Ext.Error.raise('Компонент событий не может обнаружить вышестоящую форму!');
        };
        //</debug>
        return form.grid;
   },

    /**
    * Получает верхнее меню для грида
    */
    getTopToolBar: function () {
        return this.getGrid().getDockedItems('toolbar[dock="top"]')[0];
    },
   
    /**
   * Получает кнопку
   * @param  {String}   buttons Ключ для поиска кнопки, например "create" или "delete" или "open" или аналогичные
   */
    getButton: function (button) {
        var result = null;
        var tbar = this.getTopToolBar();
        tbar.items.items.forEach(function (item) {
            if (new RegExp("-button-(" + button + ")$").test(item.id)) {
                result = item;
            }
        });
        return result;
    }
});