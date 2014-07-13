/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
* Andrew Shmelev (andrew.shmelev@gmail.com)
*
*/
/**
* @class App.components.mixins.FormField
*/
Ext.define("App.components.mixins.FormField", {

    isChanged: function () {
        var me = this,
            value = me.getValue();

        return value !== me.formValue && Ext.isEmpty(me.initialModel.idcalculatedfieldtype) && me.name != 'id';
    },

    set: function (Value, defaults) {
        var me = this;

        if (Value && Value.result) {
            var temp = Value.result[0][me.name] || Value.result[0][me.name.toLowerCase()];
            if (!defaults) {
                me.formValue = temp;
            }
            me.docid = Value.result[0].id;
            me.setValue(me.formValue || temp);
        };
        me.validate();
    },
    
    get: function () {

        var me = this,
            result = {};

        result[me.name] = me.getValue();
        return me.isChanged() || this.idOwnerFieldInDenormalizedTp() ? result : null;
    },
    
    /**
    * Является ли данное поле полем idOwner в денормализованной ТЧ ?
    * Примечание: Для денрмализованных ТЧ значение для поля idOwner при сохранении элемента должно всегда! отсылаться на сервер, 
    * а не только когда было изменено (т.е. только при создании элемента в ДТЧ).
    */
    idOwnerFieldInDenormalizedTp: function () {
        var p = this.getParent();
        if (!p || !p.getParent) return false;
        p = p.getParent();
        if (!p || !p.field_id) return false;
        var tp = Ext.getCmp(this.getParent().getParent().field_id);
        if (!tp.ownerfield) return false;

        return this.entity.model.isDenormalizedTablepart === true && this.name == tp.ownerfield.name;
    }
})
