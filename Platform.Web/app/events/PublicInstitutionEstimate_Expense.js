/**
* @class App.events.PublicInstitutionEstimate_Expense
* Обработчик клиентских событий ТЧ "Расходы"
*/
Ext.define('App.events.PublicInstitutionEstimate_Expense', {
    extend: 'App.events.CommonTablePart',

    events: [
        { name: 'dataget', handler: 'onDataGet', item: null },
        { name: 'change', handler: 'onKOSGUChange', item: 'idKOSGU' },
        { name: 'change', handler: 'onOKATOChange', item: 'idOKATO' },
        { name: 'change', handler: 'onIndirectChange', item: 'isIndirectCosts' }
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

    onIndirectChange: function(sender, newValue) {
        this.getField('idindirectcostsdistributionmethod').setValue(null, '');
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
                    if (fieldName != 'ofg' && fieldName != 'pfg1' && fieldName != 'pfg2' &&
                            fieldName != 'additionalofg' && fieldName != 'additionalpfg1' && fieldName != 'additionalpfg2' &&
                                Ext.Array.contains(requiredFields, fieldName))
                        field.allowBlank = false;
                }
            });
        }
    },

    onKOSGUChange: function (sender, newValue) {
        if (sender.list && newValue) {
            var kosgu = sender.list.grid.getSelectionModel().getLastSelected().get('code'); 
            if (kosgu == '251')
                this.getField('IdOKATO').allowBlank = false;
        } else {
            this.getField('IdOKATO').allowBlank = true;
        }
    },
    onOKATOChange: function(sender, newValue) {
        if (newValue || this.getField('IdOKATO').getValue() ) {
            this.getField('isIndirectCosts').setValue(false);
            this.getField('isIndirectCosts').disable();
        } else {
            this.getField('isIndirectCosts').enable();
        }
    }
});