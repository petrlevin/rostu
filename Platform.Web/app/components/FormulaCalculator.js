Ext.define('App.components.FormulaCalculator', {
    extend: 'Ext.form.Panel',

    height: 250,
    width: 600,

    calculationFormulaEntityId: -1543503627, // id сущности "Формулы расчета"

    constructor: function() {

        this.bbar = [
            '->', {
                xtype: 'button',
                text: 'Вставить и закрыть',
                handler: this.onInsertClick,
                scope: this
            }
        ];

        this.callParent(arguments);

        this.formulaSelector = this.getFormulaSelector();
        this.formulaField = this.getFormulaField();
        this.indicators = this.getIndicatorsContainer();

        this.add([
            this.formulaSelector,
            this.formulaField,
            this.getResultField(),
            this.indicators
        ]);
    },

    // --- конфигураторы

    getFormulaSelector: function() {

        var initialModel = {
            name: 'valueSelector',
            caption: 'Формула рассчета',
            description: '',
            identityfieldtype: App.enums.EntityFieldType.Link,
            identitylink: this.calculationFormulaEntityId,
            allownull: true //,
            //defaultvalue
        };

        return Ext.create("App.components.fields.Link", {
            anchor: '100%',
            owner_id: this.id,
            //docid: this.docid,
            //entity: this.entity,
            //model: this.model,
            labelAlign: 'top',
            initialModel: initialModel,
            fieldLabel: initialModel.caption,
            name: initialModel.name,
            allowBlank: initialModel.allownull,
            listeners: {
                'change': this.onFormulaChange,
                scope: this
            }
        });
    },

    getFormulaField: function () {

        return Ext.create('Ext.form.field.Text', {
            anchor: '100%',
            readOnly: true
        });
    },

    getResultField: function () {

        this.resultField = Ext.create('Ext.form.field.Text', {
            name: '_result',
            flex: 1
        });
        return Ext.create('Ext.form.FieldContainer', {
            fieldLabel: 'Результат',
            labelAlign: 'top',
            layout: 'hbox',
            items: [this.resultField, {
                xtype: 'button',
                text: 'Расчитать',
                handler: this.recalculate,
                scope: this
            }]
        });
    },

    getIndicatorsContainer: function () {

        return Ext.create('Ext.form.FieldSet', {
            title: 'Показатели',
            collapsible: true,
            layout: 'vbox',
            labelWidth: 200
        });
    },

    // --- Обработчики событий

    onFormulaChange: function (formulaSelector) {

        var value = formulaSelector.fieldValue;
        if (Ext.isDefined(value.id))
            FormulaService.getFormulaIndicators(value.id, this.onIndicatorsLoaded, this);
        else {
            this.formulaField.reset();
            this.resultField.reset();
            this.indicators.removeAll();
        }

    },

    onIndicatorsLoaded: function (result) {

        var fields = [];
        this.formulaField.setValue(result.formulaText);
        Ext.each(result.indicators, function (indicator) {
            fields.push(Ext.create('Ext.form.field.Number', {
                fieldLabel: Ext.String.format('{0} ({1})', indicator.symbol, indicator.caption),
                name: indicator.symbol,
                value: indicator.defaultValue || 0,
                listeners: {
                    'change': this.recalculate,
                    scope: this
                }
            }));
        }, this);

        this.resultField.reset();
        this.indicators.removeAll();
        this.indicators.add(fields);
    },

    onInsertClick: function () {

        var result = this.resultField.getValue();

        if (!Ext.isNumeric(result)) {
            Ext.Msg.show({
                title: 'Ошибка при вычислении',
                msg: 'Не удалось получить числовой результат. Проверьте формулу и значения показателей.',
                buttons: Ext.Msg.OK
            });
        } else {
            this.numberField.setValue(result);
            this.up().close();
        }
    },

    // --- Действия

    recalculate: function () {

        var code = '';
        this.indicators.items.each(function (indicatorField) {
            code += Ext.String.format('var {0} = {1};\n', indicatorField.name, indicatorField.getValue());
        }, this);

        code += this.formulaField.getValue();

        var result = eval(code);

        if (result == 'Infinity')
            result = 'Бесконечность';

        this.resultField.setValue(result);
    }

});
