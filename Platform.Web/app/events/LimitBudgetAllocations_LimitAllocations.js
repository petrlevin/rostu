/**
* @class App.events.LimitBudgetAllocations_LimitAllocations
* Обработчик клиентских событий ТЧ "Предельные объемы бюджетных ассигнований"
*/
Ext.define('App.events.LimitBudgetAllocations_LimitAllocations', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'itemloaded', handler: 'onItemLoaded', item: null },
        { name: 'operationbegin', handler: 'onOperationBegin', item: null },
        { name: 'dataget', handler: 'onDataget', item: null }
    ],

    onOperationBegin: function () {
        this.PermissionsInputAdditionalRequirements();
    },

    onDataget: function () {
        this.hideKbk();
    },

    onItemLoaded: function (sender) {
        this.hideKbk();
    },

    hideKbk: function () {
        var docId = this.getField('idOwner').getValue();

        if (docId) {
            SborLimitBudgetAllocationsService.get_SbpblankByDocument(docId, this.onLoadSbpBlank, this);
            SborLimitBudgetAllocationsService.getPermissionsInputAdditionalRequirementsForLBA(docId, function (result) { this.onPermissionsInputAdditionalRequirements(result); }, this);
        }
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
    },
    
    onPermissionsInputAdditionalRequirements: function (result) {
        var AdditionalNeedOFG = this.getField('AdditionalNeedOFG');
        var AdditionalNeedPFG1 = this.getField('AdditionalNeedPFG1');
        var AdditionalNeedPFG2 = this.getField('AdditionalNeedPFG2');

        if (result) {
            AdditionalNeedOFG.show();
            AdditionalNeedPFG1.show();
            AdditionalNeedPFG2.show();
        } else {
            AdditionalNeedOFG.hide();
            AdditionalNeedPFG1.hide();
            AdditionalNeedPFG2.hide();
        }
    }

});