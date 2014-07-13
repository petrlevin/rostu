/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.logic.factory.Fields
* Фабрика полей формы. Создает необходимые поля в соответствии с типом {@link App.enums.EntityFieldType}
*/
Ext.define("App.logic.factory.Fields", {

	requires: [
	    'App.logic.factory.Field',
	    'Ext.button.Split',
	    'Ext.menu.Menu',
	    'Ext.Error'
	],

	statusButton: null,

	statusField: null,

	/**
	 * Имена созданных на форме полей. 
	 * Когда форма строится не на основе модели, а на основе произвольной конфигурации, то по завершению построения даный массив
	 * предоставляет имена созданных полей.
	 */
	formFields: [],

    fields: [],

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
	constructor: function (config) {

	    this.fields = [];

	    Ext.apply(this, config);
	    // <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Field factory constructor');
		}
		if (Ext.isEmpty(this.model) || Ext.isEmpty(this.model.result)) {

			Ext.Error.raise('FieldFactory does not have model configuration');
		}
		// </debug>

	    // Регистрируем все табличные части в кэше
		Ext.each(this.model.tableParts, function (tpart) {

			App.ModelMgr.registerInCache(
				Ext.String.format(
					'tp-{0}-{1}',
					this.model.entityId,
					tpart.id
				), tpart.model
			);
		}, this);

		// Конфигурация формы
		this.formItems = this.build(this.formCfg, this.model.result, true);
	},

	buildContainer : function(item) {
	    var controlName = item.controlName;
	    var controlAlias = item.controlAlias;

	    Ext.apply(item, item.properties);
        // items также удаляем, чтобы можно было нормально создать все компоненты штатно
	    Ext.destroyMembers(item, 'items', 'controlName', 'controlAlias');

	    if (controlAlias === 'panel') {
	        controlAlias = 'fieldcontainer';
	        controlName = 'Ext.form.FieldContainer';
	    }

	    // если имя компонента не задано, то возвращаем конфигурационных объект
	    if (!controlName) {
	        return controlAlias ? Ext.apply(item, {
	            xtype: controlAlias,
                layout: 'fit'
	        }) : item;
	    }
	    return Ext.create(controlName, item); 
	},

	buildField: function (fieldModel, defaultParams) {

	    var temp = Ext.create('App.logic.factory.Field', Ext.apply({
	        form_id: this.form_id,
	        docid: this.docid,
	        entity: this.entity,  // объект класса EntityItem - сущность, которой принадлежит поле
	        model: this.model,
	        fields: this.fields
	    }, fieldModel));
	    this.fields.push(temp);
	    return this.getField(temp, defaultParams);
	},

	prepare: function (item) {
	    var properties = {};
	    if (item.labelProperty) {
	        properties[item.labelProperty] = item.label;
	    }
	    item.properties = Ext.apply(properties, Ext.JSON.decode(item.properties, true));
	    Ext.applyIf(item.properties, Ext.JSON.decode(item.defaultProperties, true));
	},

	build: function (config, model, isMainForm) {
	    var temp, tempFields, fieldModel,
			result = [], tableParts = [];

		Ext.each(config || model, function(item, index, all) {
			if(Ext.isEmpty(config)) {
				// Построение только по модели
				// Создаем поле и пихаем в результирующий массив
			    temp = this.buildField(Ext.clone(item));

			    if (temp) {
			        if (temp.isTableField && temp.isTableField()) {
			            tableParts.push(temp);
			        } else {
			            result.push(temp);
			        }
			    }
			} else {
			    this.prepare(item);
			    // Построение по конфигурации
			    if (Ext.isEmpty(item.entityFieldName)) {
			        // Контейнер
			        temp = this.buildContainer(Ext.clone(item));
			        if (Ext.isDefined(item.items) &&
                        !Ext.isEmpty(item.items)) {

			            tempFields = this.build(item.items, model);
			            if (Ext.isFunction(temp.add)) {

			                temp.add(tempFields);
			            } else {
			                temp.items = tempFields;
			            }
			        }
			        result.push(temp);
			    } else {
			        // Создаем поле и пихаем в результирующий массив
			        fieldModel = Ext.Array.findBy(model, function (field) {
			            return field.name === item.entityFieldName;
			        });
			        if (fieldModel) {
			            if (this.isTableField(fieldModel) || fieldModel.identityfieldtype == App.enums.EntityFieldType.Link) {
			                item.properties.columnNames = [];
			                Ext.each(item.items, function (column) {
			                    item.properties.columnNames.push(column.entityFieldName.toLowerCase());
			                }, this);
			            }

			            temp = this.buildField(Ext.clone(fieldModel), item.properties);
			            result.push(temp);
			        }
			    }
			}
		}, this);

	    if (isMainForm) {
	        this.addStatusField(result, model);
	        this.addIdField(result, model);
	    }
	    
		return result.concat(tableParts);
	},

	setMenuHandler: function (handler, scope) {

		this.menuHandler = handler.bind(scope);
	},

	onClick: function (sender) {

		this.menuHandler(sender, this.statusField);
	},

	resetStatus: function () {

	    if (Ext.isDefined(this.oldValue)) {
	        this.statusButton.setText(
				App.StatusMgr.getStatusTextById(this.oldValue)
			);
	        delete this.oldValue;
	    }
	},

	/**
	 * Добавляет скрытое поле статуса и splitbutton для его переключения, если это еще не было сделано.
	 * Например, в кастомной форме поля статус нет, а в модели есть, следовательно надо добавить.
	 */
	addStatusField: function (formItems, model) {


	    var searchResult = this.fields.filter(function (field) {
	        return field.isStatusField();
	    }, this);

	    var statusField;
	        
	    if (searchResult.length == 0) {
	        //Поле нет на форме
	        searchResult = model.filter(function (field) {
	            return this.isStatusField(field);
	        }, this);

	        if (searchResult.length == 0)
	            return; // у сущности нет поля статус
	        else {
	            statusField = this.buildField(searchResult[0]);
	            formItems.push(statusField);
	        }
	    } else {
	        statusField = searchResult[0];
	    }
	    
	    statusField = formItems.filter(function (field) {
	        return field.name == statusField.name;
	    }, this)[0];

	    if (!statusField)
	        return;

	    this.statusField = statusField.name;
	    this.createStatusButton(statusField);

	    /*var searchResult = this.fields.filter(function (field) {
		    return field.isStatusField();
		}, this);

		if (searchResult.length == 0) {
		    // в кастомной форме нет поля статус
		    searchResult = model.filter(function (field) {
		        return this.isStatusField(field);
		    }, this);

		    if (searchResult.length == 0)
		        return; // нет поля статус
		    else
		        searchResult[0] = this.buildField(searchResult[0]);
		}

	    var statusModel = searchResult[0];
	    this.statusField = statusModel.name;

	    var statusField = statusModel.getField();
	    this.createStatusButton(statusField);
	    formItems.push(statusField);*/
	},

	isStatusField: function (field) {
	    return (this.entity.entity.identitytype === App.enums.EntityType.Reference
            && field.identityfieldtype === App.enums.EntityFieldType.Link
            && field.identitylink === App.common.Ids.RefStatus);
	},

    /**
     * Определяет, является ли поле табличным.
     * @param {Object} модель поля
     */
	isTableField: function (fieldModel) {

	    return [
            App.enums.EntityFieldType.Multilink,
            App.enums.EntityFieldType.Tablepart,
            App.enums.EntityFieldType.VirtualTablePart
	    ].indexOf(fieldModel.identityfieldtype) >= 0;
	},

	/**
	 * Добавляет поле id, если оно еще не было создано
	 */
	addIdField: function (formItems, model) {

		var idEf = this.fields.filter(function (field) {
			return field.name.toLowerCase() == 'id';
		}, this);

        //Если поле id уже добавлено -- ничего не делаем
		if (idEf.length > 0)
			return;

		var idFieldModel = model.filter(function (field) {
		    return field.name == 'id';
		}, this);

        //Не может быть
	    if (idFieldModel.length == 0)
	        return;

	    var idField = this.buildField(idFieldModel[0]);
	    formItems.push(idField);
    },

	/**
	 * Получает объект поля формы
	 * @param {Object} поле модели
	 * @param {Object} конфиг-объект элемента формы (сериализованный объект класса FormLogic.FormItem). 
	 * Если параметр не указан, то поле создается исключительно на основе поля сущности.
	 */
	getField: function (entityField, defaultParams) {

	    var field = entityField.getField(defaultParams);

	    this.formFields.push(entityField.name.toLowerCase());

	    return field;
	},

	createStatusButton: function (field) {

	    // Создать сплит, повесить обработчик обновления
	    // поля, чтобы обновить надпись на кнопке
	    this.statusButton = Ext.create('Ext.button.Split', {
	        text: 'Статус',
	        menu: Ext.create('Ext.menu.Menu', {
	            items: App.StatusMgr.getMenu(this.onClick, this)
	        })
	    });
        
	    field.on('change', function (sender, newValue, oldValue) {
	        this.oldValue = oldValue;
	        this.statusButton.setText(
                App.StatusMgr.getStatusTextById(newValue)
            );
	    }, this);
    },

	getStatusBtn: function () {

		return this.statusButton;
	},

	get: function () {

		return this.formItems;
	}
});