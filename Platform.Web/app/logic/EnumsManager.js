/**
* @class App.logic.EnumsManager
* @singleton
* Класс, который загружает с сервера перечисления, помченные атрибутом ClientEnum.
*/

Ext.define('App.logic.EnumsManager', {

    /**
     * @private
     */
    singleton: true,

    /**
     * Запрашивает перечисления с сервера. Размещает их в виде объектов в пространстве имен App.enums
     */
    getEnums: function () {
        EnumsService.getEnums(this.onEnumsLoaded, this);
    },
    
    /**
     * @private
     */
    onEnumsLoaded: function (result, response) {
        Ext.iterate(result, function (enumName) {
            var values = {};
            Ext.iterate(result[enumName], function (valueName) {
                values[this.capitalizeFirstLetter(valueName)] = result[enumName][valueName];
            }, this);
            App.enums[this.capitalizeFirstLetter(enumName)] = values;
        }, this);
    },
    
    capitalizeFirstLetter: function (string) {
        return string.charAt(0).toUpperCase() + string.slice(1);
    }

}, function () {

    Ext.namespace('App.enums');
    App.EnumsMgr = this;
});