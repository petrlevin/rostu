/**
* @class App.events.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense
* Обработчик клиентских событий ТЧ "Расходы учредителя АУБУ"
*/
Ext.define('App.events.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense', {
    extend: 'App.events.CommonTablePart',

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
        this.hideAdditionalNeeds();
    },

    hideAdditionalNeeds: function() {
        var document = this.getForm().getParent().getParent();

        if (document) {
            var hasAdditionalNeed = document.getField('HasAdditionalNeed').getValue();

            if (!hasAdditionalNeed) {
                this.hideFields(['additionalofg', 'additionalpfg1', 'additionalpfg2']);
            }
        }
    },

    hideKbk: function () {
        var docId = this.ownerEntity.getField('id').getValue();

        if (docId)
            this.docService.get_SbpblankAuBu(docId, this.onLoadSbpBlank, this);
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
                    if (fieldName != 'ofg' && fieldName != 'pfg1' && fieldName != 'pfg2' &&
                            fieldName != 'additionalofg' && fieldName != 'additionalpfg1' && fieldName != 'additionalpfg2' &&
                                Ext.Array.contains(requiredFields, fieldName))
                        field.allowBlank = false;
                }
            });
        }
    }
});