/**
* @class App.events.RegisterActivity
* Обработчик клиентских событий сущности документа "Реестр мероприятий"
*/
Ext.define('App.events.RegisterActivity', {
    extend: 'App.events.CommonItem',

    events: [
         { name: 'dataget', handler: 'onItemLoaded', item: null },
         { name: 'change', handler: 'onChangeDocType', item: 'idDocType' }
    ],

    onItemLoaded: function () {
        this.onChangeDocType();
    },

    onChangeDocType: function (sender, newValue, oldValue) {
        var typ = this.getField('idDocType').getValue();
        var idSbp = this.getField('idSbp');
        var grid = this.getField('tpActivity').grid;
        //var Column_idRegistryKeyActivity = grid.columns.column('idRegistryKeyActivity');
        
        if (typ != -1543503848) {
            idSbp.enable();
            idSbp.show();
            idSbp.allowBlank = false;

            Ext.each(grid.columns, function (column) {
                if (column.name == "idRegistryKeyActivity") {
                    column.allowBlank = true;
                    column.hide();
                }
                else if (column.name == "idRegystryActivity_ActivityMain") {
                    column.hide();
                }
            });

            
        } else {
            idSbp.allowBlank = true;
            this.getField('idSbp').clear();
            idSbp.disable();
            idSbp.hide();
            
            Ext.each(grid.columns, function (column) {
                if (column.name == "idRegistryKeyActivity") {
                    column.allowBlank = false;
                    column.show();
                }
                else if (column.name == "idRegystryActivity_ActivityMain") {
                    column.show();
                }
            });

        };
        idSbp.clearInvalid();
        idSbp.isValid();
    }
});