/*
* Обработчик клиентских событий отчета "Перечень целевых програм"
*/


Ext.define('App.events.ListProgram', {
    extend: 'App.events.CommonItem',

    events: [
		 { name: 'change', handler: 'onChangeByApproved', item: 'buildReportApprovedData' }
        , { name: 'change', handler: 'onChangeListTypeOutputProgram', item: 'idListTypeOutputProgram' }
    ],

    onChangeByApproved: function (sender, newvalue, oldvalue) {
        var fdt = this.getField('DateReport');
        if (newvalue) {
            fdt.allowBlank = false;
            fdt.enable();
            fdt.setValue(new Date());
        }
        else {
            fdt.allowBlank = true;
            fdt.setValue(null);
            fdt.disable();
        }
    }

    , onChangeListTypeOutputProgram: function (sender, newvalue, oldvalue) {
        var fdt = this.getField('idProgram');
        if (newvalue) {
            fdt.setValue(null);
        }

    }

});
