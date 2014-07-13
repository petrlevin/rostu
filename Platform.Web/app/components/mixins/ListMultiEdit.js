/*
* Список элементов, поддерживающих множественное редактирование
*/
Ext.define('App.components.mixins.ListMultiEdit', function () {

    var getMultiEditBtn = function (text) {

        var me = this;
        return {
            text: text,
            iconCls: 'icon-mass-edit',
            handler: function () {
                var selectedElements = [];
                me.grid.getSelectionModel().getSelection().forEach(function(item) {
                    Ext.Array.push(selectedElements, item.internalId);
                });

                if (!selectedElements.length) {
                    Ext.MessageBox.alert(Locale.APP_MESSAGE_FORM_WARNING, Locale.APP_MULTIEDIT_NOSELECTED);
                    return;
                }
                
                //Справочники
                if (me.entity.identitytype == 3) {

                    var statusFieldName = null;

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
                    
                    if (hasStatus(me.model.result)) {
                        var search = me.grid.getSelectionModel().getSelection().filter(function(item) {
                            return item.get(statusFieldName.toLowerCase()) != 1;
                        });

                        if (search.length)
                        {
                            Ext.MessageBox.alert(Locale.APP_MESSAGE_FORM_WARNING, Locale.APP_MULTIEDIT_SELECTEDNOTNEWREF);
                            return;
                        }
                    }
                }

                var dialog = Ext.create('App.components.MultiEditDialog', { componentId: me.id, selectedElements: selectedElements });

                var wind = {
                    height: 250,
                    width: 600,
                    layout: 'fit',
                    modal: true,
                    title: 'Множественное редактирование',
                    getForm: function() { return dialog; }
                };
                  
                App.WindowMgr.add(wind, {}, false);

                
            }
        };
    };

    
    var getMenuItems = function () {

        var menuItems = [getMultiEditBtn.call(this, Locale.APP_BUTTON_MULTIEDIT)];
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

            switch (this.entity.identitytype) {
                case 3: //Справочники
                case 4: //Табличная часть

                    var actionsButton = getActionsButton(result);
                    actionsButton.menu.add(getMenuItems.call(this));
                    
                    break;
            }

            return undefined; // сообщаем о том, что созданные пункты были вставлены в исходное меню
        }
    };
})