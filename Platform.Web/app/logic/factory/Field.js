Ext.define('App.logic.factory.Field', {

    requires: [
        'App.components.mixins.FormField',
        'App.components.fields.TextArea',
        'App.components.fields.Text',
        'App.components.fields.Hidden',
        'App.components.fields.Number',
        'App.components.fields.Money',
        'App.components.fields.Date',
        'App.components.fields.DateTime',
        'App.components.fields.Link',
        'App.components.fields.EnumComboBox',
        'App.components.fields.GenericLink',
        'App.components.fields.TablePart',
        'App.components.fields.Multilink',
        'App.components.fields.Boolean',
        'Ext.tip.ToolTip',
        'Ext.ux.InputTextMask'
    ],

    constructor: function (config) {
        Ext.apply(this, config);
    },

    /**
     * true - в интерфейсе поле должно быть только для чтения
     */
    isReadOnly: function () {

        return this.readonly || (!Ext.isEmpty(this.idcalculatedfieldtype));
    },

    /**
	 * Определяет, является ли поле табличным.
	 * @param {Object} модель поля
	 */
    isTableField: function () {

        return [
            App.enums.EntityFieldType.Multilink,
            App.enums.EntityFieldType.Tablepart,
            App.enums.EntityFieldType.VirtualTablePart
        ].indexOf(this.identityfieldtype) >= 0;
    },

    /**
     * Является ли поле общей ссылкой. Наличие парного поля не проверяется.
     * @param {Object/App.logic.factory.Field} fieldCfg Необязательный параметр - конфигурация поля сущности. Если не задан, используется this.
     */
    isGenericLink: function (fieldCfg) {

        var f = fieldCfg || this;
        return [
            App.enums.EntityFieldType.ReferenceEntity,
            App.enums.EntityFieldType.ToolEntity,
            App.enums.EntityFieldType.TablepartEntity,
            App.enums.EntityFieldType.DocumentEntity
        ].indexOf(f.identityfieldtype) >= 0;
    },

    /**
     * Системное поле выбора сущности, имя которого заканчивается на 'Entity'
     * @param {Object/App.logic.factory.Field} fieldCfg Необязательный параметр - конфигурация поля сущности. Если не задан, используется this.
     */
    isEntitySelector: function (fieldCfg) {
        var f = fieldCfg || this;
        return f.identityfieldtype == App.enums.EntityFieldType.Link
            && f.issystem === true
            && f.name.endsWith('Entity');
    },

    isStatusField: function () {

        return (this.entity.entity.identitytype === App.enums.EntityType.Reference
            && this.identityfieldtype === App.enums.EntityFieldType.Link
            && this.identitylink === App.common.Ids.RefStatus);
    },

    /**
     * Является ли поле частью общей ссылки для выбора СУЩНОСТИ. Проверяется наличие парного поля.
     * @return {Object} 
     * false - поле не является частью общей ссылки для выбора СУЩНОСТИ.
     * null - нет парного поля. 
     * {Object} - парное поле для выбора значения.
     */
    getEntitySelectorPair: function () {

        if (!this.isEntitySelector())
            return false;

        var name = this.name;
        name = name.substr(0, name.length - 'Entity'.length);

        // ищем парное поле по имени name
        var pair = this.fields.filter(function (f) {
            return f.name == name && f.isGenericLink();
        }, this);

        if (Ext.isEmpty(pair))
            return null;
        else
            return pair[0];
    },

    /**
     * Является ли поле частью общей ссылки для выбора ЗНАЧЕНИЯ. Проверяется наличие парного поля.
     * @return {Object} 
     * false - поле не является частью общей ссылки для выбора ЗНАЧЕНИЯ.
     * null - если поле не является частью общей ссылки или нет парного поля. 
     * {Object} - парное поле для выбора сущности.
     */
    getValueSelectorPair: function () {
        if (!this.isGenericLink())
            return false;

        var name = this.name + 'Entity';
        var pair = this.fields.filter(function (f) {
            return f.name == name && f.isEntitySelector();
        }, this);

        if (Ext.isEmpty(pair))
            return this.createEntitySelectorPair();
        else
            return pair[0];
    },

    createEntitySelectorPair: function () {
        var me = this;

        var name = this.name + 'Entity';

        var pairModel = this.model.result.filter(function (f) {
            return f.name == name && me.isEntitySelector(f);
        }, this);

        if (Ext.isEmpty(pairModel)) {
            // <debug>
            Ext.Error.raise('General link field hasn\'t entity selector.');
            // </debug>
            return null;
        } else {
            var field =  Ext.create('App.logic.factory.Field', Ext.apply({
                form_id: me.form_id,
                owner_id: me.owner_id,
                entity: me.entity,  // объект класса EntityItem - сущность, которой принадлежит поле
                model: me.model,
                fields: me.fields
            }, pairModel[0]));

            this.fields.push(field);

            return field;
        }
            

    },

     /**
     * Получает объект поля формы
     * @param {Object} поле модели
     * @param {Object} конфиг-объект элемента формы (сериализованный объект класса FormLogic.FormItem). 
     * Если параметр не указан, то поле создается исключительно на основе поля сущности.
     */
    getField: function (defaultParams) {

        var defaults = Ext.apply(this.getFieldDefaultParameters(), defaultParams);

        if (this.isStatusField()) {

            return Ext.create("App.components.fields.Hidden", defaults);
        } else if (this.name.toLowerCase() === 'id' || this.ishidden) {

            return Ext.create("App.components.fields.Hidden", defaults);
        } else {
            switch (this.identityfieldtype) {
                case App.enums.EntityFieldType.DateTime:
                    return Ext.create("App.components.fields.DateTime", defaults);
                case App.enums.EntityFieldType.Date:
                    return Ext.create("App.components.fields.Date", defaults);
                case App.enums.EntityFieldType.Bool:
                    return Ext.create("App.components.fields.Boolean", defaults);
                case App.enums.EntityFieldType.String:
                    Ext.apply(defaults, { maxLength: this.size });
                    return Ext.create("App.components.fields.Text", defaults);
                case App.enums.EntityFieldType.Text:
                    return Ext.create("App.components.fields.TextArea", defaults);
                case App.enums.EntityFieldType.Numeric:
                case App.enums.EntityFieldType.Money:
                    Ext.apply(defaults, {
                        decimalPrecision: this.precision
                    });
                    var extremum = Ext.String.repeat('9', this.size - this.precision) + '.' + Ext.String.repeat('9', this.precision);
                    extremum = extremum.valueOf();
                    if (defaults.maxValue === undefined)
                        defaults.maxValue = extremum;
                    if (defaults.minValue === undefined)
                        defaults.minValue = -extremum;
                    return (this.identityfieldtype == App.enums.EntityFieldType.Numeric) ? Ext.create("App.components.fields.Number", defaults) : Ext.create("App.components.fields.Money", defaults);
                case App.enums.EntityFieldType.Int:
                case App.enums.EntityFieldType.BigInt:
                case App.enums.EntityFieldType.TinyInt:
                case App.enums.EntityFieldType.SmallInt:
                    defaults.decimalPrecision = 0;
                    return Ext.create("App.components.fields.Number", defaults);
                case App.enums.EntityFieldType.FileLink:
                    return Ext.create("App.components.fields.FileLink", defaults);
                case App.enums.EntityFieldType.Link:
                    if (Ext.isObject(this.getEntitySelectorPair())) {
                        // Получение отдельной части поля общей ссылки не предусмотрено. Вместо этого следует получить поле общей ссылки isGenericLink
                        return undefined;
                    }
                    if (this.identitylink && App.EntitiesMgr.getEntityById(this.identitylink).identitytype == App.enums.EntityType.Enum) {
                        return Ext.create("App.components.fields.EnumComboBox", defaults);
                    }
                    return Ext.create("App.components.fields.Link", defaults);
                case App.enums.EntityFieldType.Multilink:
                    return Ext.create("App.components.fields.Multilink", defaults);
                case App.enums.EntityFieldType.Tablepart:
                case App.enums.EntityFieldType.VirtualTablePart:
                    return Ext.create("App.components.fields.TablePart", defaults);
                case App.enums.EntityFieldType.ReferenceEntity:
                case App.enums.EntityFieldType.ToolEntity:
                case App.enums.EntityFieldType.TablepartEntity:
                case App.enums.EntityFieldType.DocumentEntity:
                    //defaults.hideLabel = true;
                    //return Ext.create("App.components.fields.Link", defaults);
                    return Ext.create('App.components.fields.GenericLink', {
                        entitySelector: Ext.apply(this.getValueSelectorPair().getFieldDefaultParameters(), defaultParams),
                        valueSelector: defaults
                    });
                default:
                    // <debug>
                    if (Ext.isDefined(Ext.global.console)) {
                        Ext.global.console.warn(Ext.String.format('Field type not found! Type id: {0}, field name : {1}',
                            this.name, this.identityfieldtype));
                    }
                    // </debug>
                    return Ext.create("App.components.fields.Text", defaults);
            }
        }
    },

    /**
     * Возвращает объект параметров, характерных для любого поля формы
     * @param {App.logic.factory.Field} модель поля
     */
    getFieldDefaultParameters: function () {

        var defaults = Ext.apply({}, {
            anchor: '100%',
            owner_id: this.form_id,
            docid: this.docid,
            entity: this.entity,
            model: this.model,
            fieldLabel: this.caption || this.name,
            labelAlign: this.identityfieldtype == App.enums.EntityFieldType.Bool ? 'left' : 'top',
            name: this.name,
            allowBlank: this.allownull,
            readOnly: this.isReadOnly(),
            initialModel: this,
            plugins: []
        });

        if (this.tooltip) {
            var tooltipText = this.tooltip;
            defaults = Ext.apply(defaults, {
                listeners: {
                    render: function (e) {
                        Ext.create('Ext.tip.ToolTip', {
                            target: e.labelEl,
                            html: tooltipText + ' &nbsp;'
                        });
                    }
                }
            });
        }

        if ( ( this.identityfieldtype == App.enums.EntityFieldType.DateTime || this.identityfieldtype == App.enums.EntityFieldType.Date ) && !this.regexpvalidator)
            this.regexpvalidator = "{inputMask:'99.99.9999'}";

        this.applyRegexValidatorDefaults(defaults);

        return defaults;
    },

    /**
	 * Определение свойств для поля, если у поля сущности зщадано свойство RegExpValidator
	 */
    applyRegexValidatorDefaults: function (defaults) {

        if (!this.regexpvalidator)
            return defaults;

        var json = Ext.JSON.decode(this.regexpvalidator, true);
        if (json) {
            if (json.inputMask) {
                defaults.plugins = defaults.plugins || [];
                defaults.plugins.push(Ext.create('Ext.ux.InputTextMask', json.inputMask));
            } else {
                Ext.apply(defaults, json);
            }
        } else {
            defaults.regex = value;
        }

        return defaults;
    }
});