﻿/**
* @class App.events.PublicInstitutionEstimate_IndirectExpenses
* Обработчик клиентских событий ТЧ "Косвенные Расходы"
*/
Ext.define('App.events.PublicInstitutionEstimate_IndirectExpenses', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'dataget', handler: 'onDataGet', item: null }
    ],

	docService: null,
    
	constructor: function(config){
			this.docService = SborPublicInstitutionEstimateService;
			
			this.callParent([config]);
	},
    
    onDataGet: function (sender) {
        this.hideKbk(sender);
    },

    hideKbk: function () {
        var docId = this.getField('idOwner').getValue();

        if (docId)
            this.docService.get_Sbpblank(docId, this.onLoadSbpBlank, this);
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
                    field.fieldValue = { id: null, caption: '' };
                } else {
                    if (fieldName != 'ofg' && fieldName != 'pfg1' && fieldName != 'pfg2' && Ext.Array.contains(requiredFields, fieldName))
                        field.allowBlank = false;
                }
            });
        }
    }
});