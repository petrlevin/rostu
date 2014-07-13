/**
* @class App.events.CommonTablePart
* Description
*/
Ext.define('App.events.CommonTablePart', {
    extend: 'App.events.CommonItem',
   /**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function (config) {
        // <debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Общий конструктор объектов обработчиков клиентских событий.');
        }
        // </debug>

        this.callParent([config]);
        
        this.ownerEntity = this.getForm().getParent();
        if (this.ownerEntity && this.ownerEntity.getParent)
            this.ownerEntity = this.ownerEntity.getParent();
    },

    /* Родительский объект */
    ownerEntity : null
});