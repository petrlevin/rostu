/**
* @class App.events.Role
* Обработчик клиентских событий сущности Role
*/
Ext.define('App.events.Role', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'change', handler: 'onChangeTypeKind', item: 'idRoleKind' },
        { name: 'change', handler: 'onChangeTypeKind', item: 'idRoleType' },
        { name: 'itemloaded', handler: 'onItemLoaded', item: null }
    ],

    LockRole: function(idkind, idtype) {
        var btype = (idtype == 0 && (App.Profile.userName == "admin" || App.Profile.userName == "bis")) || idtype > 0;

        var bkind = idkind == 1;

        var bfun = btype && bkind;
        var borg = btype && !bkind;

        if (bfun) {
            this.getField('tpFunctionalRights').up('panel').enable();
            this.getField('tpFunctionalRights').up('panel').show();
        } else {
            this.getField('tpFunctionalRights').up('panel').disable();
        }

        if (borg) {
            this.getField('tpRole_OrganizationRight').up('panel').enable();
            this.getField('tpRole_OrganizationRight').up('panel').show();
        } else {
            this.getField('tpRole_OrganizationRight').up('panel').disable();
        }
    },

    onChangeTypeKind: function(sender, newValue, oldValue) {
        this.LockRole(this.getField('idRoleKind').getValue(), this.getField('idRoleType').getValue());
    },

    onItemLoaded: function(sender) {
        this.getField('idRoleKind').disable();
        this.LockRole(this.getField('idRoleKind').getValue(), this.getField('idRoleType').getValue());
    }
});