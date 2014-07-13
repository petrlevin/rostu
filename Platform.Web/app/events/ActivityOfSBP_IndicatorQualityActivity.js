/**
* @class App.events.ActivityOfSBP_IndicatorQualityActivity
* Обработчик клиентских событий сущности ActivityOfSBP_IndicatorQualityActivity
*/
Ext.define('App.events.ActivityOfSBP_IndicatorQualityActivity', {
    extend: 'App.events.CommonItem',

	events: [
	    { name: 'beforeshow', handler: 'onAfterRender', item: null },
        { name: 'itemloaded', handler: 'onItemLoaded', item: null }
	],


	onAfterRender: function () {
	    this.SHAdditionalValue();
	},

	onItemLoaded: function () {
	    this.SHAdditionalValue();
	},
	
	SHAdditionalValue: function () {
	//    var IdOwner = this.getField('IdOwner');
	//    if (IdOwner) {
	//        DataService.getItem(IdOwner.initialModel.identitylink, IdOwner.getValue(), this.onLoadPage0, this);
	//    }
	//},

	//onLoadPage0: function (result, response) {
	//    var doc = result.result[0];

	//    han = doc.hasadditionalneed;

	//    var columnsTo = this.getForm().form.getFields().filterBy(function (field) {
	//        return field.name.substring(0, 15) == 'AdditionalValue';
	//    });
	//    Ext.each(columnsTo.items, function (item) {
	//        if (han == 1) {
	//            item.show();
	//        } else {
	//            item.hide();
	//        }
	//    }, this);
	}
});