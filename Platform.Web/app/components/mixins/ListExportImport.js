Ext.define('App.components.mixins.ListExportImport', function () {

    // формат : свойства
    var importProperties = {
        'Xml': {
            url: 'Services/ImportXml.ashx',
            format: 'Xml'
        },

        'Excel': {
            url: 'Services/Import.aspx',
            format: 'Excel'
        }
    };

    var exportProperties = {
        'Xml': {
            url: 'Services/ExportXml.ashx'
        },
        
        'Excel': {
            url: 'Services/Export.aspx'
        }
    };
    
    var exportHandlerType = {
        'Xml': 'ExportXml',

        'Excel': 'Export'
    };

    var getExportBtn = function (tplId, text, format, icon) {

        return {
            iconCls: icon,
            text: text,
            setAvailability: Ext.emptyFn,
            handler: exportButtonHandler,
            
            // параметры:
            handlerType: exportHandlerType[format],
            entitiesList: this,
            entityId: this.model.entityId,
            tplId: tplId,
            field_id: this.field_id,
            entityType: this.entity.identitytype,
            httpRequestProperties: { url: 'Services/DownloadFile.aspx' }
        };
    };

    var exportButtonHandler = function() {

        var tablePart = Ext.getCmp(this.field_id),
            params = {
                idEntity: this.entityId,
                idTemplate: this.tplId,
                visibleColumns: (this.entitiesList.getVisibleColumnsName() || []).join(','),
                searchStr: this.entitiesList.searchField.value,
                handlerType: this.handlerType
            };

        if (tablePart)
            Ext.apply(params, {
                idOwner: tablePart.docid,
                ownerFieldName: tablePart.ownerfield.name,
                fieldvalues: JSON.stringify(tablePart.getDependencies()),
                tpFiledId: tablePart.initialModel.id, // идентификатор табличного поля
                ownerItemId: this.entitiesList.getParent().getField('id').getValue()
            });

        var downloader = Ext.create('App.logic.HttpRequest', Ext.apply({
            params: params
        }, this.httpRequestProperties));
        
        downloader.submit();
    };
    
    var getImportBtn = function (tplId, text, format, icon) {
        return {
            iconCls: icon,
            text: text,
            handler: importButtonHandler,
            // параметры:
            entitiesList: this,
            entityId: this.model.entityId,
            tplId: tplId,
            field_id: this.field_id,
            entityType: this.entity.identitytype,
            httpRequestProperties: importProperties[format]
        };
    };
    
    var importButtonHandler = function () {

        var tablePart = Ext.getCmp(this.field_id),
            params = {
                idEntity: this.entityId,
                idTemplate: this.tplId,
                visibleColumns: (this.entitiesList.getVisibleColumnsName() || []).join(','),
                searchStr: this.entitiesList.searchField.value
            };

        if (tablePart)
            Ext.apply(params, {
                idOwner: tablePart.docid,
                ownerFieldName: tablePart.ownerfield.name,
                fieldvalues: JSON.stringify(tablePart.getDependencies()),
                tpFiledId: tablePart.initialModel.id, // идентификатор табличного поля
                ownerItemId: this.entitiesList.getParent().getField('id').getValue()
            });

        var dialog = Ext.create('App.components.ImportDialog', Ext.apply({
            params: params
        }, this.httpRequestProperties));
        dialog.show();
    };

    var iterator = function (btnGetter, captionPrefix, menuItems, format) {

        Ext.Object.each(this.model.importTemplates, function (tplId, tplCation) {
            menuItems.push(btnGetter.call(this, tplId, captionPrefix + tplCation, format));
        }, this);
    };


    var getMenuItems = function () {

    	var menuItems = [
    		getExportBtn.call(this, null, Locale.APP_BUTTON_EXPORT, 'Excel', 'icon-excel'),
			getExportBtn.call(this, null, Locale.APP_BUTTON_EXPORTXML, 'Xml', 'icon-export-xml'),
    	    getImportBtn.call(this, null, Locale.APP_BUTTON_IMPORTXML, 'Xml', 'icon-import-xml')
    	];
        iterator.call(this, getExportBtn, Locale.APP_BUTTON_EXPORT + ': ', menuItems, 'Excel');
        iterator.call(this, getImportBtn, Locale.APP_BUTTON_IMPORT + ': ', menuItems, 'Excel');
        return menuItems;
    };

    var getActionsButton = function(result) {
        return result.filter(function (item) {
            var id = item.itemId || item.id;
            return id.indexOf('-button-actions') > -1;
        })[0];
    };

    return {

        getActions: function (result) {
            var actionsButton = getActionsButton(result);
            actionsButton.menu.add(getMenuItems.call(this));
            return undefined; // сообщаем о том, что созданные пункты были вставлены в исходное меню
        }
    };
})