/**
 * Функционал ссылочных полей.
 */
Ext.define('App.components.mixins.LinkField', {


    /**
    * Value of the field from the form
    */
    formValue: {},

    /**
    * Real value that user selected on the form
    */
    fieldValue: {},
    

    clear: function () {

        this.setValue();
    },

    /**
     *
     */
    set: function (Value, defaults) {

        if (Value && Value.result) {
            var temp = Value.result[0];
            this.docid = temp.id;

            var value_id = temp[this.name.toLowerCase()];
            if (!Ext.isDefined(value_id))
                value_id = temp[this.name];

            if (Ext.isDefined(value_id)) {
                var value_caption = temp[Ext.String.format('{0}_caption', this.name.toLowerCase())] ||
                    temp[Ext.String.format('{0}_caption', this.name)];
                
                var value_description = temp[Ext.String.format('{0}_description', this.name.toLowerCase())];

                var tempValue = {
                    id: value_id,
                    caption: value_caption,
                    description: value_description
                };
                if (!defaults) {
                    this.formValue = Ext.apply({}, tempValue);
                }
                this.setValue(Ext.apply({}, tempValue));
            }
        }
        this.validate();
    },
    
    isChanged: function () {
        return this.fieldValue.id !== this.formValue.id;
    },

    /**
    *
    */
    get: function () {

        var result = {};

        if (this.isChanged() || this.idOwnerFieldInDenormalizedTp()) {
            result[this.name] = Ext.isDefined(this.fieldValue.id) ? this.fieldValue.id : null;
        }
        
        return result;
    }

});