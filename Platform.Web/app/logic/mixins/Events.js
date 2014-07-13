/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.logic.mixins.Events
* Description
*/
Ext.define('App.logic.mixins.Events', {

    requires: [
        'Ext.Error'
    ],

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function (config) {
    	//<debug>
    		if (!Ext.isDefined(config.form_id)) {
    			if (Ext.isDefined(Ext.global.console)) {
    				Ext.Error.raise('Parent form identifier is not supplied!');
    			}
    		};
            if (!Ext.isDefined(config.entity_name)) {
                if (Ext.isDefined(Ext.global.console)) {
                    Ext.Error.raise('Entity name is not supplied!');
                }
            };
    	//</debug>
        config.model_name = config.model_name || "";

        //config.id = config.id || Ext.id();
        Ext.apply(this, config);

        this.createInstance(config);
    },

    createInstance: function(config) {
        var class_name = Ext.String.format('App.events.{0}{1}',
                    this.entity_name,
                    this.model_name
                );

        try {
            this.userEvents = Ext.create(class_name, config);
        } catch(ex) {
            // <debug>
            if (Ext.isDefined(Ext.global.console)) {
                Ext.global.console.log(Ext.String.format('Пользовательские события для сущности не предусмотрены. Сущность {0}', this.entity_name));
            }
            // </debug>
        }
    }
});