/**
* @class App.events.AuthorityOfExpenseObligation
* Обработчик клиентских событий сущности AuthorityOfExpenseObligation
*/
Ext.define('App.events.AuthorityOfExpenseObligation', {
    extend: 'App.events.CommonItem',

	events: [
        //{ name: 'afterrender', handler: 'onDataget', item: null },
	    { name: 'blur', handler: 'onChangeCaption', item: 'Caption' }
	],

	onChangeCaption: function (sender, The, eOpts) {
	    var cap = this.getField('Caption');
        cap.setValue(cap.getValue().toUpperCase());
	}

    //,onDataget: function() {
	//    var cap = this.getField('Caption');
	//    cap.setFieldLabel('Код. (Необходимо использовать только заглавные русские буквы!)');
	//}
});