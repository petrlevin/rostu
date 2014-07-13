/**
* @class App.events.EntityField
* Простейший обработчик клиентских событий сущности EntityField
*/
Ext.define('App.events.EntityField', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'change', handler: 'onChangeType', item: 'idEntityFieldType' },
        { name: 'change', handler: 'onIsCaption', item: 'isCaption' },
        { name: 'change', handler: 'onExpression', item: 'Expression' },
        { name: 'change', handler: 'onChangeEntityLink', item: 'idEntityLink' }
    ],

    onChangeEntityLink: function(sender, newValue, oldValue) {
        if (!this.getField('Name').getValue() || !this.getField('Caption').getValue()) {
            var prefix = 'id';

            if (!sender.list)
                return;

            var selected = sender.list.grid.getSelectionModel().getLastSelected();
            var entityName = selected.get('name');
            var entityCaption = selected.get('caption');

            if (!this.getField('Name').getValue()) {
                if (this.getField('idEntityFieldType').getValue() == 9) //TablePart
                {
                    prefix = 'tp';
                    entityName = entityName.substr(entityName.indexOf('_') + 1) + 's';
                }

                this.getField('Name').setValue(prefix + entityName);
            }
            if (!this.getField('Caption').getValue()) {
                this.getField('Caption').setValue(entityCaption);
            }
        }
    },

    onExpression: function (sender, newValue, oldValue) {
        if (newValue != '') {
            this.getField('ReadOnly').setValue(1);
        }
    },
    
    onIsCaption: function(sender, newValue, oldValue) {
        if (newValue == 1) {
            this.getField('AllowNull').setValue(0);
        }
    },

    DisEnField: function(flag, name, empt) {
        if (flag) {
            this.getField(name).enable();
        } else {
            this.getField(name).disable();
            this.getField(name).setValue(empt);
        }
    },

    DisEnFieldLink: function (flag, name, empt) {
        if (flag) {
            this.getField(name).enable();
        } else {
            this.getField(name).disable();
            this.getField(name).setValue(empt, empt);
        }
    },

    onChangeType: function(sender, newValue, oldValue) {
        var bSize = true,
            bPrecision = true,
            bIdEntityLink = true,
            bIdOwnerField = true,
            bRegExpValidator = true,
            bidForeignKeyType = true,
            bSizeValue = 0,
            bPrecisionValue = 0;

        if (newValue == 1 || newValue == 6 || newValue == 17 || newValue == 24) { // Булево | Дата
            bSize = false;
            bPrecision = false;
            bIdEntityLink = false;
            bIdOwnerField = false;
            bidForeignKeyType = false;
        } else if (newValue == 2 || newValue == 3 || newValue == 4 || newValue == 15 || newValue == 16) { // NVarchar 1 - 4000 | Целое | Большое целое
            bPrecision = false;
            bIdEntityLink = false;
            bIdOwnerField = false;
            bidForeignKeyType = false;
        } else if (newValue == 5 || newValue == App.enums.EntityFieldType.Money) { // Вещественное 
            bIdEntityLink = false;
            bIdOwnerField = false;
            bidForeignKeyType = false;

            // Денежное
            if (newValue == App.enums.EntityFieldType.Money) {
                bSize = false;
                bSizeValue = 22;

                bPrecision = false;
                bPrecisionValue = 2;
            }
        } else if (newValue == 7 || newValue == 8 || newValue > 19) { // Ссылки и МТЧ
            bSize = false;
            bPrecision = false;
            bIdOwnerField = false;
            bRegExpValidator = false;
            if (newValue == App.enums.EntityFieldType.FileLink) {
                this.getField("idEntityLink").setValue(-1342177258, "Файлы");
            }
            else if (newValue > 19) {
                bIdEntityLink = false;
            }
            if (newValue == 8) { // мультилинк
                this.getField('idForeignKeyType').setValue(3, 'С обеспечением поддержки ссылочной целостности и каскадным удалением');
            } else if (newValue == 7 || newValue == App.enums.EntityFieldType.FileLink) //ссылка
            {
                if (App.EntitiesMgr.getEntityById(this.getField('idEntity').getValue()).identitytype == App.enums.EntityType.Report)
                    this.getField('idForeignKeyType').setValue(1, 'Без ссылочной целостности');
                else
                    this.getField('idForeignKeyType').setValue(2, 'С обеспечением поддержки ссылочной целостности');
            }
        } else if (newValue == 9) { // ТЧ
            bSize = false;
            bPrecision = false;
            //bIdEntityLink = false;
            bRegExpValidator = false;
            bidForeignKeyType = false;
        } 

        if (newValue == 7 || newValue == App.enums.EntityFieldType.FileLink) { //|| newValue == 8) {
            this.getField('idForeignKeyType').allowBlank = false;
        } else {
            this.getField('idForeignKeyType').allowBlank = true;
        }

        this.DisEnField(bSize, 'Size', bSizeValue);
        this.DisEnField(bPrecision, 'Precision', bPrecisionValue);
        this.DisEnField(bRegExpValidator, 'RegExpValidator', '');
        this.DisEnFieldLink(bIdEntityLink, 'idEntityLink', null);
        this.DisEnFieldLink(bIdOwnerField, 'idOwnerField', null);
        this.DisEnFieldLink(bidForeignKeyType, 'idForeignKeyType', null);
    }
});