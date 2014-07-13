/**
* @class App.events.ActivityOfSBP_ActivityResourceMaintenance
* Обработчик клиентских событий ТЧ "Ресурсное обеспечение мероприятий"
*/
Ext.define('App.events.ActivityOfSBP_ActivityResourceMaintenance', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'dataget', handler: 'onDataGet', item: null },
        { name: 'change', handler: 'onChangeBudget', item: 'IdBudget' },
        { name: 'change', handler: 'onChangeKosgu', item: 'IdKOSGU' }
    ],
    
    onDataGet: function () {
        this.hideBudget();
        this.hideKbk();
        this.hidePeriod();
        //var g = this.getField('idMaster').getValue();
    },
    
    hideBudget: function () {
        var id = this.getField('id').getValue();
        var idBudget = this.getField('IdBudget');
        if (id) {
            idBudget.disable();
        } else {
            idBudget.enable();
        }

    },
    onChangeBudget: function () {
        this.hideKbk();
        this.hidePeriod();
    },

    onChangeKosgu: function () {
        var idkosgu = this.getField('idKOSGU');
        var kosgu = idkosgu.getValue();
        if (kosgu) {
            DataService.getItem(idkosgu.initialModel.identitylink, kosgu, this.onLoadKosgu, this);
        }

    },

    onLoadKosgu: function (result, response) {
        var obj = result.result[0];
        var field = this.getField('idOKATO');
        field.allowBlank = !(obj.code == '251');
        field.clearInvalid();
        field.isValid();
    },

    hideKbk: function () {
        var docId = this.getField('idOwner').getValue();
        var idBudget = this.getField('idBudget').getValue();
        if (docId)
            SborCommonService.getSbpBlank_ActivityOfSBP(docId, idBudget, true, this.onLoadSbpBlank, this);
    },

    onLoadSbpBlank: function (result, response) {
        var hiddenFields = result.hidden;
        var requiredFields = result.required;

        var idBudget = this.getField('idBudget').getValue();

        if ((Ext.isArray(hiddenFields) && hiddenFields.length > 0) || (Ext.isArray(requiredFields) && requiredFields.length > 0)) {
            var form = this.getForm().form;

            Ext.each(form.getFields().items, function(field) {
                var fieldName = field.name.toLowerCase();

                if (Ext.Array.contains(hiddenFields, fieldName)) {
                    field.fieldValue = false;
                    field.allowBlank = true;
                    field.clearInvalid();
                    field.hide();
                } else {
                    if (idBudget) {
                        field.show();
                        if (Ext.Array.contains(requiredFields, fieldName)) {
                            field.allowBlank = false;
                            field.clearInvalid();
                            field.isValid();
                        }
                    }
                }
            });
        }

        this.onChangeKosgu();
    },

    hidePeriod: function () {

        var idBudget = this.getField('idBudget').getValue();
        SborCommonService.getPeriods_ActivityOfSBP(idBudget, this.onLoadPeriod, this);
    },

    onLoadPeriod: function (result, response) {
        var hiddenFields = result.hidden;
        var requiredFields = result.required;

        if ((Ext.isArray(hiddenFields) && hiddenFields.length > 0) || (Ext.isArray(requiredFields) && requiredFields.length > 0)) {
            var form = this.getForm().form;

            Ext.each(form.getFields().items, function (field) {
                var fieldName = field.name.toLowerCase();

                if (Ext.Array.contains(hiddenFields, fieldName)) {
                    field.hide();
                } else {
                    if (Ext.Array.contains(requiredFields, fieldName)) {
                        field.show();
                    }
                }
            });
        }
    }
  
});