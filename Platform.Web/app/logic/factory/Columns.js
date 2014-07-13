/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.logic.factory.Columns
* Фабрика колонок грида. Создает необходимые колонки в соответствии с типом {@link App.enums.EntityFieldType}
*/
Ext.define("App.logic.factory.Columns", {

    requires: [
        'App.components.columns.Text',
        'App.components.columns.Number',
        'App.components.columns.Money',
        'App.components.columns.Date',
        'App.components.columns.DateTime',
        'App.components.columns.Date',
        'App.components.columns.Link',
        'App.components.columns.Boolean',
        'App.util.Format',
        'Ext.tree.Column'
    ],
    
	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function (config) {
        // <debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Column factory constructor');
        }
        // </debug>
        Ext.apply(this, config);
        this.init();
    },

    init: function () {

        // <debug>
        if (Ext.isEmpty(this.model) || Ext.isEmpty(this.model.result)) {

            Ext.Error.raise('ColumnFactory does not have model configuration');
        }
        //</debug>
    },

    defaultFilter: function (config) {

        if (!Ext.isDefined(config.filter)) {
            config.filter = {
                xtype: 'extstring'
            }
        }
        return config;
    },

    get: function (hierarchy) {
        var caption,
            result = [],
            defaults = {};
        var isSumTitleFind = false;
        Ext.each(this.model.result, function (item, index) {
            var isSumTitle = (!isSumTitleFind && !item.ishidden) ? true : false;
            if (isSumTitle)
                isSumTitleFind = true;
                
            defaults = Ext.merge({
                dataIndex: item.name.toLowerCase(),
                filterable: true,
                name: item.name,
                text: item.caption || item.name,
                width: item.width || 100,
                hidden: item.name.toLowerCase() === 'id' || item.ishidden,
                tooltip: item.tooltip,
                renderer: function (value, meta, record) {
                    var column = meta.column;
                    var columnName = column.name;
                    if (columnName && record.raw[columnName.toLowerCase() + "_description"]) {
                        meta.tdAttr = 'data-qtip="' + Ext.String.htmlEncode(record.raw[columnName.toLowerCase() + "_description"]) + '&nbsp;"';
                    } else if (value) {
                        var max = 40;
                        if (value.length > max)
                            meta.tdAttr = 'data-qtip="' + Ext.String.htmlEncode(value) + '&nbsp;"';
                    }

                    if ( Ext.isFunction( column.defaultRenderer) )
                        return column.defaultRenderer(value, meta, record);

                    if (column.superclass && Ext.isFunction(column.superclass.renderer) ) {
                        return column.superclass.renderer(value, meta, record);
                    }

                    return value;
                }
            }, item.format ? { format: item.format } : {}
                , isSumTitle ? {
                summaryRenderer: function () {
                    return "&nbsp;&nbsp;&nbsp;&nbsp;Итого";
                }

            }: {
                summaryRenderer: function (value) {
                    return value;
                }
            });

            if (Ext.isDefined(hierarchy) && item.name.toLowerCase() === hierarchy.toLowerCase()) {
                if (this.isLink(item)) {
                    defaults.dataIndex = Ext.String.format('{0}_caption', item.name.toLowerCase());
                    defaults.identitylink = item.identitylink;
                } else {
                    defaults.dataIndex = item.name.toLowerCase();
                }
                defaults.xtype = 'treecolumn';
                result.push(this.defaultFilter(defaults));
            } else {
                switch (item.identityfieldtype) {
                    case App.enums.EntityFieldType.Date:
                        result.push(Ext.create('App.components.columns.Date', defaults));
                        break;
                	case App.enums.EntityFieldType.DateTime:
                        result.push(Ext.create('App.components.columns.DateTime', defaults));
                        break;
                    case App.enums.EntityFieldType.Bool:
                        result.push(Ext.create('App.components.columns.Boolean', defaults));
                        break;
                    case App.enums.EntityFieldType.String:
                    case App.enums.EntityFieldType.Text:
                    case App.enums.EntityFieldType.Multilink:
                        result.push(Ext.create('App.components.columns.Text', defaults));
                        break;
                    case App.enums.EntityFieldType.Numeric:
                        Ext.apply(defaults, { renderer: this.numericRenderer });
                        result.push(Ext.create('App.components.columns.Number',defaults));
                        break;
                    case App.enums.EntityFieldType.Money:
                        Ext.apply(defaults, { renderer: this.moneyRenderer });
                        result.push(Ext.create('App.components.columns.Money', defaults));
                        break;
                    case App.enums.EntityFieldType.Int:
                    case App.enums.EntityFieldType.BigInt:
                    case App.enums.EntityFieldType.TinyInt:
                    case App.enums.EntityFieldType.SmallInt:
                        Ext.apply(defaults, { renderer: this.numericRenderer, format: '0' });
                        result.push(Ext.create('App.components.columns.Number', defaults));
                        break;
                    case App.enums.EntityFieldType.Link:
                    case App.enums.EntityFieldType.FileLink:
                    case App.enums.EntityFieldType.ReferenceEntity:
                    case App.enums.EntityFieldType.ToolEntity:
                    case App.enums.EntityFieldType.TablepartEntity:
                    case App.enums.EntityFieldType.DocumentEntity:
                        defaults.identitylink = item.identitylink;
                        result.push(Ext.create('App.components.columns.Link', defaults));
                        break;
                    case App.enums.EntityFieldType.Tablepart:
                    case App.enums.EntityFieldType.VirtualTablePart:
                        break;
                    default:
                        result.push(Ext.create('App.components.columns.Text', defaults));
                }
            }
        }, this);
        this.columns = result;

        return this.columns;
    },
    
    numericRenderer: function (v) {
        if (Ext.isEmpty(v))
            return '';
        // Специальное поведение - возвращает ровно столько десятичных знаков, сколько есть в числе
        var value = v.toString();
        var decimalPartLength = value.indexOf('.') == -1 ? 0 : value.length - value.indexOf('.') - 1;
        var temp = Ext.util.Format.number(v, '0,000.' + Ext.String.repeat('0', decimalPartLength));
        return temp.replace(/\./g, ' ' /*thousandSeparator*/);
    },
    
    moneyRenderer: function (v) {
        if (Ext.isEmpty(v))
            return '';
        // Специальное поведение - возвращает ровно столько десятичных знаков, сколько есть в числе
        var temp = Ext.util.Format.number(v, '0,000.00');
        return temp.replace(/\./g, ' ' /*thousandSeparator*/);
    },

     /**
     * ToDo: этот метод не должен находиться в этом классе. Более подходящее место для него - Field.js, однако к объекту данного класса отсюда также нет доступа.
     */
    isLink: function (fieldCfg) {
        return [
	        App.enums.EntityFieldType.Link,
            App.enums.EntityFieldType.FileLink,
	        App.enums.EntityFieldType.ReferenceEntity,
	        App.enums.EntityFieldType.ToolEntity,
	        App.enums.EntityFieldType.TablepartEntity,
	        App.enums.EntityFieldType.DocumentEntity
        ].indexOf(fieldCfg.identityfieldtype) > -1;
    }
});