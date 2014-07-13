String.prototype.endsWith = function(suffix) {

    return this.indexOf(suffix, this.length - suffix.length) !== -1;
};

Array.prototype.cascade = function(fn, scope, items) {

	for(var i = 0; i < this.length; i++) {
		fn.apply(scope, [this, this[i]]);

		if (Ext.isDefined(this[i][items]) &&
            !Ext.isEmpty(this[i][items])) {

			this[i][items].cascade.apply(this[i], arguments);
		}
	}
};

/*
* Добавление метода клиентской валидации
*/
Ext.apply(Ext.form.field.VTypes, {

    confirm: function (val, field) {
        if (field.initialField) {
            var pwd = Ext.getCmp(field.initialField);
            return (val == pwd.getValue());
        }
        return true;
    },

    confirmText: Locale.VTYPE_FIELDS_DO_NOT_MATCH
});
