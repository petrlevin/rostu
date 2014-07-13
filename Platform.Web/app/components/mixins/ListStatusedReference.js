Ext.define('App.components.mixins.ListStatusedReference', function () {

    var statusFieldName = null;

    var changeStatus = function (list, statusId) {
        var selected = list.grid.getSelectionModel().getSelection();

        var itemIds = [];

        Ext.each(selected, function (item) {
            itemIds.push(item.data.id);
        });

        var statusValue = {};
        statusValue[statusFieldName] = statusId;

        var entityId = list.entity.id;

        var result = Ext.apply({}, statusValue);

        CommunicationDataService.updateElements(entityId, itemIds, result, function (data, response) {
            list.refresh();
            
            // <debug>
            if (response.type !== 'rpc') {
                if (Ext.isDefined(Ext.global.console)) {
                    Ext.global.console.log('Произошла ошибка при изменении элементов');
                }
                return;
            }
            // </debug>

        });

    };

    var getMultiEditBtn = function (text) {
        var me = this;

        var changeHandler = changeStatus;

        return {
            text: text,
            iconCls: 'icon-status-change',
            hideOnClick: false,
            menu: Ext.create('Ext.menu.Menu', {
                items: App.StatusMgr.getMenu(function (sender) {
                    changeHandler(me, sender.origin.id || 1);
                }, this),
                showSeparator: false
            })
        };
    };

    var getMenuItems = function () {

        var menuItems = [getMultiEditBtn.call(this, Locale.APP_BUTTON_MULTIEDITSTATUS)];
        return menuItems;
    };

    var getActionsButton = function (result) {
        return result.filter(function (item) {
            var id = item.itemId || item.id;
            return id.indexOf('-button-actions') > -1;
        })[0];
    };

    var hasStatus = function (result) {
        var ans = false;

        Ext.each(result, function (field) {
            if (isStatusField(field)) {
                ans = true;
                statusFieldName = field.name;
                return false;
            }
        }, this);

        return ans;
    };

    var isStatusField = function (field) {
        return (field.identityfieldtype === App.enums.EntityFieldType.Link
                    && field.identitylink === App.common.Ids.RefStatus);
    };

    return {

        getActions: function (result) {
            //Справочники
            if (this.entity.identitytype == 3) {
                if (hasStatus(this.model.result)) {
                    var actionsButton = getActionsButton(result);
                    actionsButton.menu.add(getMenuItems.call(this));
                }
            }

            return undefined; // сообщаем о том, что созданные пункты были вставлены в исходное меню
        }
    };
});