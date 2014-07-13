Ext.define('App.components.mixins.ListDocument', function () {

    var me = null;

    var entityId = null;

    var executeOperation = function (operation, itemIds) {
        OperationsService.execGroup(entityId, itemIds, operation.id, onExecutedOperation, this);
    };

    var onExecutedOperation = function (data, response) {
        me.refresh();

        // <debug>
        if (response.type !== 'rpc') {
            if (Ext.isDefined(Ext.global.console)) {
                Ext.global.console.log('Произошла ошибка при изменении элементов');
            }
        }
        // </debug>

    };

    var getMultiEditBtn = function (text) {
        me = this;
        
        entityId = me.entity.id;
        
        return {
            text: text,
            iconCls: 'icon-status-change',
            hideOnClick: false,
            listeners: {
                activate: {
                    //todo: запрашивать новые элементы только при изменении выбора на гриде
                    fn: function () {
                        this.menu.removeAll();
                        this.menu.add({ disabled : true, iconCls: 'multiedit-operation-load', text: 'Загружаются доступные операции' });

                        var selected = me.grid.getSelectionModel().getSelection();
                        var itemIds = [];

                        Ext.each(selected, function (item) {
                            itemIds.push(item.data.id);
                        });

                        var operationMenu = this.menu;

                        DataService.getAvaliableAtomaricOperations(itemIds, entityId, function (response) {
                            operationMenu.removeAll();
                            
                            if (!response || !response.length)
                                operationMenu.add({ disabled: true, text: "Нет доступных операций" });
                            else {
                                Ext.each(response, function(item) {
                                    operationMenu.add({
                                        text: item.text,
                                        handler: function() {
                                            executeOperation(item, itemIds);
                                        }
                                    });
                                });
                            }
                        }); 
                    }
                }
            },
            menu: Ext.create('Ext.menu.Menu', {
                items: [],
                showSeparator: false
            })
        };
    };

    var getMenuItems = function () {
        var menuItems = [getMultiEditBtn.call(this, Locale.APP_BUTTON_MULTIEDITOPERATION)];
        return menuItems;
    };

    var getActionsButton = function (result) {
        return result.filter(function (item) {
            var id = item.itemId || item.id;
            return id.indexOf('-button-actions') > -1;
        })[0];
    };

    return {

        getActions: function (result) {
            //Справочники
            if (this.entity.identitytype == App.enums.EntityType.Document) {
                var actionsButton = getActionsButton(result);
                actionsButton.menu.add(getMenuItems.call(this));
            }

            return undefined; // сообщаем о том, что созданные пункты были вставлены в исходное меню
        }
    };
});