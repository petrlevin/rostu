/**
* @class App.events.SBP_Blank
* Обработчик клиентских событий сущности ТЧ "Бланки доведения и формирования" справочника "Субъекты бюджетного планирования"
*/
Ext.define('App.events.SBP_Blank', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'change', handler: 'onSource',    item: 'idBlankValueType_FinanceSource' },
	    { name: 'change', handler: 'onBlankType', item: 'idBlankType' }
    ],

    ctrlField: function (fn, isDisable) {
        var f = fn;
        if (typeof(fn) == "string") {
            f = this.getField(fn);
        }
        if (isDisable) {
            f.disable();
        } else {
            f.enable();
        }
    },
    
    onSource: function (sender, newValue, oldValue) {
        var f = this.getField('idBlankValueType_FinanceSource');
        if (f.readOnly) {
            this.ctrlField(f, true); // Доведение АУ/БУ
        }
    },

    onBlankType: function(sender, newValue, oldValue) {
        //if (sender.list) {
            var fkfo = this.getField('idBlankValueType_KFO');
            this.ctrlField(fkfo, (newValue == 3 || newValue == 5));     // Доведение АУ/БУ или Формирование АУ/БУ
            this.ctrlField('idBlankValueType_KOSGU', (newValue == 3));  // Доведение АУ/БУ
            if ((newValue == 3 || newValue == 5)) {
                fkfo.setValue(1, "Обязательное");
            }
        //}
    }

});