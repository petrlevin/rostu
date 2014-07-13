/**
* @class App.events.AnalysisBAofDirectAndIndirectCost
* Обработчик клиентских событий отчета "Анализ объема бюджетных ассигнований в разрезе прямых и косвенных расходов"
*/
Ext.define('App.events.AnalysisBAofDirectAndIndirectCost', {
    extend: 'App.events.CommonItem',

    events: [
		{ name: 'change', handler: 'onChangeByApproved', item: 'byApproved' }
    ],

    onChangeByApproved: function (sender, newvalue, oldvalue) {
        var fdt = this.getField('DateReport');
        if (newvalue) {
            fdt.enable();
            fdt.setValue(new Date());
            fdt.allowBlank = false;
        }
        else {
            fdt.allowBlank = true;
            fdt.setValue(null);
            fdt.disable();
        }
    }
});