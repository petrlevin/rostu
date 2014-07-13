/**
* @class App.events.WordCommonReport
* Обработчик клиентских событий универсального отчета по шаблону word
*/
Ext.define('App.events.WordCommonReport', {
    extend: 'App.events.CommonItem',
    
    events: [
       { name: 'afterrender', handler: 'onAfterRender', item: null }
    ],
    

    //todo: убрать копипаст. реально нужно только параметризировать вызов reportLauncher'а
    onAfterRender: function () {

        var me = this;
        
        var bottomToolbar = this.getForm().panel.getDockedItems('toolbar[dock="bottom"]')[0];
        var reportBtn = this.getButton(bottomToolbar, 'report');

        reportBtn.handler = function () {

            var entityItem = me.getForm();

            var params = {
                _entityName: entityItem.entity.name
            };

            entityItem.form.getFields().each(function (field) {
                if (field.initialModel) {
                    var value;
                    if (field.getXTypes().indexOf('app_datetimefield') > -1) {
                        value = field.getValue();
                        if (value && value.toUTCString)
                            value = value.toUTCString();
                    } else if (field.getXTypes().indexOf('app_datefield') > -1) {
                        value = field.getValue();
                        if (value && value.toLocaleDateString)
                            value = value.toLocaleDateString();
                    } else {
                        value = field.getValue();
                        if (value && value.toString)
                            value = value.toString();
                    }
                    params[field.name] = value;
                }
            }, this);

            if (entityItem.saveElement() === false)
                App.ReportLauncher.launch(params, 'wordreport');
            else
                // ToDo: более правильным было бы использовать событие полного завершения загрузки элемента, т.е. вместе со всеми ТЧ
                entityItem.on('itemloaded', function () {
                    App.ReportLauncher.launch(params, 'wordreport', null, '_self');
                }, this, { single: true });
        };

    }
});