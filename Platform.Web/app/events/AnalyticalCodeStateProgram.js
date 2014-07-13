/**
* @class App.events.AnalyticalCodeStateProgram
* Обработчик клиентских событий сущности User
*/
Ext.define('App.events.AnalyticalCodeStateProgram', {
    extend: 'App.events.CommonItem',

    events: [
		{ name: 'change', handler: 'onChangeidType', item: 'idTypeOfAnalyticalCodeStateProgram' }
    ],

    //При перевыборе значения поля "Тип" очищать поле «Вышестоящий».
    onChangeidType: function (sender, newvalue, oldvalue) {
        this.getField('idParent').setValue(null, null);
        //this.getField('idParent').fieldValue = { id: null, caption: '' }; //очищает значение
        //this.getField('idParent').setValue(''); //очищает видимое значение
    }

});