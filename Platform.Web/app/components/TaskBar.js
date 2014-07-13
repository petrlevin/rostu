/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
* Andrew Shmelev (andrew.shmelev@gmail.com)
*
*/
/**
* @class App.components.TaskBar
* @extends Ext.toolbar.Toolbar
*/

Ext.define('App.components.TaskBar', {
	extend: 'Ext.toolbar.Toolbar',

	requires: [
		'App.components.ToolbarButton',
		'App.components.TrayClock',
		'App.components.StartMenu',
		'Ext.button.Button',
		'Ext.resizer.Splitter',
		'Ext.menu.Menu'
	],

	alias: 'widget.taskbar',

	cls: 'ux-taskbar',

	/**
		* @cfg {String} startBtnText
		* The text for the Start Button.
		*/
	startBtnText: 'Start',

	initComponent: function () {
		var me = this;

		me.startMenu = Ext.createWidget('startmenu', me.startMenuItems);

		me.quickStart = Ext.createWidget('toolbar', me.getQuickLaunch());

		me.windowBar = Ext.createWidget('toolbar', me.getWindowBarConfig());

		me.tray = Ext.createWidget('toolbar', me.getTrayConfig());

		me.items = [
			{
				xtype: 'button',
				cls: 'ux-start-button',
				iconCls: 'ux-start-button-icon',
				menu: me.startMenu,
				menuAlign: 'bl-tl',
				text: me.startBtnText
			},
			me.quickStart,
			{
				xtype: 'splitter', html: '&#160;',
				height: 14, width: 2, // TODO - there should be a CSS way here
				cls: 'x-toolbar-separator x-toolbar-separator-horizontal'
			},
			//'-',
			me.windowBar,
			'-',
			me.tray
		];

		me.callParent();
	},

	afterLayout: function () {
		var me = this;
		me.callParent();
		me.windowBar.el.on('contextmenu', me.onButtonContextMenu, me);
	},

	// Конфигурационные объекты

	/**
	* This method returns the configuration object for the Quick Start toolbar. A derived
	* class can override this method, call the base version to build the config and
	* then modify the returned object before returning it.
	*/
	getQuickLaunch: function () {
		var ret = {
			minWidth: 20,
			width: 60,
			items: [],
			enableOverflow: true
		};
        /*
		Ext.each(this.quickLaunchItems, function (item) {
			ret.items.push(Ext.apply(item, {
				tooltip: { text: item.text, align: 'bl-tl' },
				overflowText: item.text,
				text: undefined
			}));
		});
        */
		return ret;
	},

	/**
		* This method returns the configuration object for the Tray toolbar. A derived
		* class can override this method, call the base version to build the config and
		* then modify the returned object before returning it.
		*/
	getTrayConfig: function () {
		var ret = {
				width: 80,
				items: this.trayItems
		};
		delete this.trayItems;
		return ret;
	},

	getWindowBarConfig: function () {
		return {
			flex: 1,
			cls: 'ux-desktop-windowbar',
			items: ['&#160;'],

		    enableOverflow:true
			//layout: { overflowHandler: 'Scroller' },
			//plugins: Ext.create('Ext.ux.BoxReorderer', {})
		};
	},

	// Context Menu (не работает)

	getWindowBtnFromEl: function (el) {
		var c = this.windowBar.getChildByElement(el);
		return c || null;
	},

	onButtonContextMenu: function (e) {
		//var me = this, t = e.getTarget(), btn = me.getWindowBtnFromEl(t);
		//if (btn) {
		//	e.stopEvent();
		//	me.windowMenu.theWin = btn.win;
		//	me.windowMenu.showBy(t);
		//}
	},

	// Управление окнами рабочего стола

	onWindowBtnClick: function (btn, param) {

	    if (btn.currentTarget)
	        btn = Ext.getCmp(btn.currentTarget.id);
		//<debug>
	    	if (Ext.isDefined(Ext.global.console)) {
	    		Ext.global.console.log(btn);
	    	}
	    	//</debug>

	    var win = btn.win;
	    if (win.xtype == 'panel') {
	        this.windowBar.items.each(function(item) {
	            if (item.isButton) {
	                item.win.hide();
	            }
	        });
	    }

	    

	    if (win.xtype == 'window') {
	        if (win.minimized || win.hidden) {
	            win.show();
	        } else if (win.active) {
	            win.minimize();
	        } else {
	            win.toFront();
	        }
	    } else {
	        win.show();
	    }
	},

	addTaskButton: function (win) {
	    //clear - trigger.gif
	    //var tmpl = new Ext.Template("<div>Hello </div>");
	    //tmpl.compile();
	    var config = {
	        
	            iconCls: win.iconCls,
	            enableToggle: true,
	            toggleGroup: 'all',
	            reorderable: true,
	            width: 140,
	            xtype: 'toolbarbutton',
	            margins: '0 2 0 3',
	            text: Ext.util.Format.ellipsis(win.title, 20),
	            listeners: {
	                click: this.onWindowBtnClick,
	                scope: this
	            },
	            win: win,
	            closable:true
	            
		};

		var cmp = this.windowBar.add(config);
		//<debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log(cmp);
		}
		//</debug>
		cmp.toggle(true);
		return cmp;
	},

	removeTaskButton: function (btn) {
		var found, me = this;
		me.windowBar.items.each(function (item) {
			if (item === btn) {
					found = item;
			}
			return !found;
		});
		if (found) {
			me.windowBar.remove(found);
		}
		return found;
	},

	setActiveButton: function (btn) {
		if (btn) {
				btn.toggle(true);
		} else {
			this.windowBar.items.each(function (item) {
				if (item.isButton) {
					item.toggle(false);
				}
			});
		}
	}
}, function () {

    Ext.override(Ext.layout.container.boxOverflow.Menu, {
        onButtonToggle: function(btn, state) {
            // Keep the clone in sync with the original if necessary
            if ((btn.overflowClone)&&(btn.overflowClone.checked !== state)) {

                btn.overflowClone.setChecked(state);
            }
        }
    });
}
);
