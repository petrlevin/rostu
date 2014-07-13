/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.Maximize
* @extends Ext.button.Button
* Компонент-кнопка для разворачивания грида на весь
* экран.
*/

Ext.define('App.components.Maximize', {
	extend: 'Ext.button.Button',

    /**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function (config) {
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Maximize grid component initialization');
        }
        //</debug>
        config = Ext.apply(config, {
			iconCls: 'x-maximize-tool'
        });
        Ext.apply(this, config);
        this.callParent([ config ]);

        this.on('click', this.onClickEvent, this)
    },

    setAvailability: function () {

        this.enable(); // всегда доступна
    },

    onClickEvent: function(sender, event, eOpts) {
    	var panel = this.parentForm.grid;

        if (panel.collapsed) { return; }

        this.panel = panel;

        panel.originalOwnerCt = panel.ownerCt;
        panel.originalPosition = panel.ownerCt.items.indexOf(panel);
        panel.originalSize = panel.getSize();

        if (!this.window) {
            var defaultConfig = {
                title: this.parentForm.fieldLabel || this.parentForm.title,
                id: (panel.getId() + '-MAX'),
                width: (Ext.getBody().getSize().width - 100),
                height: (Ext.getBody().getSize().height - 100),
                resizable: true,
                draggable: true,
                closable: true,
                closeAction: 'hide',
                hideBorders: true,
                plain: true,
                layout: 'fit',
                autoScroll: false,
                border: false,
                bodyBorder: false,
                frame: true,
                pinned: true,
                modal: true,
                bodyStyle: 'background-color: #ffffff;'
            };
            this.window = new Ext.Window(defaultConfig);
            this.window.on('hide', this.handleMinimize, this);
            this.window.on('move', this.correctWindowSize, this.window);
        }
        if (!panel.dummyComponent) {
            var dummyCompConfig = {
                title: panel.title,
                width: panel.getSize().width,
                height: panel.getSize().height,
                html: '&nbsp;'
            };
            panel.dummyComponent = new Ext.Panel(dummyCompConfig);
        }

        this.window.add(panel);

        panel.originalOwnerCt.insert(panel.originalPosition, panel.dummyComponent);
        panel.originalOwnerCt.doLayout();
        panel.dummyComponent.setSize(panel.originalSize);
        panel.dummyComponent.setVisible(true);
        panel.dummyComponent.getEl().mask();
        panel.toolWindow = this.window;
        this.window.show(this);

        this.setVisible(false);
    },

    correctWindowSize: function(sender, x, y) {

        var viewport = Ext.getCmp('main-viewport');
        if (viewport) {
            var size = this.getSize();
            var vsize = viewport.getSize();
            if (vsize.height < size.height) {
                size.height = vsize.height;
                this.setSize(size);
            }
            var pos = this.getPosition(false);
            this.suspendEvents(false);
            this.setPagePosition(pos[0] < 0 ? 0 : pos[0], pos[1] < 0 ? 0 : pos[1]);
            this.resumeEvents();
        }
    },

    handleMinimize: function (window) {
        var panel = this.panel;

        panel.dummyComponent.getEl().unmask();
        panel.dummyComponent.setVisible(false);
        panel.originalOwnerCt.insert(panel.originalPosition, panel);
        panel.setSize(panel.originalSize);
        panel.originalOwnerCt.doLayout();

        this.setVisible(true);
    }
});