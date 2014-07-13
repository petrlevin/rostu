/**
* @class App.events.Budget
* Обработчик клиентских событий сущности "Бюджет"
*/
Ext.define('App.events.Budget', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'dataget', handler: 'setCaption', item: null},
        { name: 'change', handler: 'setCaption', item: 'Year' }
    ],

    
    setCaption: function () {
        var year = this.getField('Year').getValue();

        if (year)
            this.getField('Caption').setValue(year + ' - ' + (year + 2) + ' гг.');
    }    
});