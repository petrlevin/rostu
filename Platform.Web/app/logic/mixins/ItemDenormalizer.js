/**
* @class App.logic.mixins.ItemDenormalizer
* Адаптер денормализации. Добавляет значимые поля в форму элемента денормализованной ТЧ
*/
Ext.define('App.logic.mixins.ItemDenormalizer', {

	constructor: function () {

		// this.on('beforeshow', this.addFieldsInDenormalizedForm, this);
	},

	/**
	 * Если this - это форма элемента денормализованной ТЧ, то добавляет на нее поля
	 */
	addFieldsInDenormalizedForm: function () {
		var new_model, fieldsFactory,
			parent = this.getParent();

		if (parent && parent.entity && parent.entity.identitytype == 4 /* ТЧ */) {
			parent = parent.getParent();
			if (parent) {
				var tpField = parent.model.result.filter(function (field) {
					return field.identitylink == this.entity.id;
				}, this)[0];
				var settings = parent.denormalizedFields[tpField.name];
				if (settings && !Ext.isEmpty(settings.fields)) {
				    new_model = Ext.applyIf( {result: settings.fields}, this.model);
				    
				    fieldFactory = this.getFieldsConfig(new_model);
				    this.panel.add(fieldFactory.get());
				    // this.form = this.panel.getForm();
				}
			}
		}
	}
});