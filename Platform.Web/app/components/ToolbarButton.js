Ext.define("App.components.ToolbarButton", {
    alias: 'widget.toolbarbutton',
    extend: 'Ext.container.Container',
    layout: 'hbox',
    requires: [
        'Ext.button.Button'
    ],
    items: [{
            xtype: 'button'
        },
        {
            xtype: 'button',
            iconCls: 'x-btn-close-btn',
            width: '19px',
            border:false
        }],
    isButton: true,
    constructor: function(config) {
        this.callParent([config]);
        this.width = this.width + 25;
        this.button = this.items.items[0];
        this.closebutton = this.items.items[1];
        this.button.on(
            {
                click: this.onClick,
                toggle: this.onToggle,
                scope: this
            }
        );
        this.closebutton.on(
            {
                click: this.onClose,
                scope: this
            }
        );
        //this.relayEvents(this.button, ['click', 'toggle']);
        Ext.apply(this.button, config);
        this.win = config.win;
        this.addEvents(
            'click',
            'toggle',
            'close'
        );

    },

    onClick: function(sender, e) {
        this.fireEvent('click', sender, e);
    },

    onToggle: function(sender, e) {
        this.fireEvent('toggle', sender, e);
    },

    onClose: function(sender, e) {

        this.fireEvent('close', this, e);
        this.win.close();
    },
    
    toggle: function(arg) {
        this.button.toggle(arg);
    },

    click: function() {
        this.toggle(true);
        this.onClick(this.button, {});
    },

    close: function () {
        this.onClose(this,{});
    }
})
