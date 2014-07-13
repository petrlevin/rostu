/**
* @class App.events.PlanActivity_KBKOfFinancialProvision
* Обработчик клиентских событий ТЧ "КБК финансового обеспечения"
*/
Ext.define('App.events.PlanActivity_KBKOfFinancialProvision', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'dataget', handler: 'onDataGet', item: null }
    ],

    onDataGet: function (sender) {
        this.hideKbk(sender);
    },

    hideKbk: function () {
        var docId = this.getField('idOwner').getValue();
        if (docId)
            SborCommonService.getSbpBlank_PlanActivity(docId, null, this.onLoadSbpBlank, this);
    },

    onLoadSbpBlank: function (result, response) {
        var hiddenFields = result.hidden;
        var requiredFields = result.required;

        if ((Ext.isArray(hiddenFields) && hiddenFields.length > 0) || (Ext.isArray(requiredFields) && requiredFields.length > 0)) {
            var form = this.getForm().form;

            Ext.each(form.getFields().items, function (field) {
                var fieldName = field.name.toLowerCase();

                if (Ext.Array.contains(hiddenFields, fieldName)) {
                    field.hide();
                    if (field.name == "ismeanaubu") {
                        field.fieldValue = false;
                    } else {
                        field.fieldValue = { id: null, caption: '' };
                    }
                } else {
                    if ( Ext.Array.contains(requiredFields, fieldName) )
                        field.allowBlank = false;
                    else {
                        field.clearInvalid();
                        field.isValid();
                    }
                }
            });
        }
    }
});