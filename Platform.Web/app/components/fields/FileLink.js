/**
* @class App.components.fields.FileLink
* @extends App.components.fields.Link
* Ссылка на общие файлы
*/

Ext.define('App.components.fields.FileLink', {
    extend: 'App.components.fields.Link',
    
    requires: [
        'App.components.fields.Link'
    ],
    trigger2Cls: 'x-form-upload-trigger',
    trigger3Cls: 'x-form-download-trigger',
    trigger4Cls: 'x-form-clear-trigger',

    setReadOnly: function (state) {

        var action = function () {
            var visibledItemIndex = 2;
                
            this.triggerEl.each(function (trigger, composite, index) {
                if (index !== visibledItemIndex) {
                    if (state) {
                        trigger.hide();
                        trigger.el.parent().setWidth(0);
                    } else {
                        var width = this.triggerEl.item(visibledItemIndex).getWidth();
                        trigger.show();
                        trigger.el.parent().setWidth(width);
                    }
                }
            }, this);
        };

        if (this.rendered)
            action.call(this);
        else
            this.on('render', action, this, { single: true });

        this[state ? 'addCls' : 'removeCls'](this.readOnlyCls);
        //this.callParent(arguments);
    },

    /**
    *
    */
    onTrigger4Click: function () {
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Dictionary link field > trigger 4, clear');
        }
        //</debug>
        this.clear();
    },
    
    /**
    *
    */
    onTrigger3Click: function () {
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Dictionary link field > trigger 3, download');
        }
        //</debug>
        //this.clear();

        var params = {
            handlerType: 'FileLinkDownloader',
            fileLinkId: this.getValue()
        };

        var downloader = Ext.create('App.logic.HttpRequest', {
            method: 'GET',
            params: params,
            url: 'Services/DownloadFile.aspx'
        });
        downloader.submit();
    },
    
    /**
  *
  */
    onTrigger2Click: function () {
        var me = this;
        
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Dictionary link field > trigger 2, upload');
        }
        //</debug>

        Ext.create('App.components.FileUploadDialog', {
            successCallback: function (id, caption, description) {
                this.setValue(id, caption, description);
            },
            sender: me
        }).show();

        //alert("you want to upload file");
    },

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
	constructor: function (config) {

	    Ext.apply(this, config);
		this.callParent([ config ]);
	}

});
