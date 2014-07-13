/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* author: Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*/
/**
* @class App.components.ClosableButton
* Кнопка с дополнительным значком закрытия
*/
Ext.define('App.components.ClosableButton', {
    extend: 'Ext.button.Button',
    alias: 'widget.closebutton',

    /**
     * Базовый класс для кнопки
     * @cfg {String}
     */
    baseCls: Ext.baseCSSPrefix + 'btn',

    /**
     * @cfg {String} closableCls
     * CSS класс, который будет добавляться к кнопке
     */
    closableCls: 'closable',

    /**
     * @cfg {Boolean} closable
     * Чтобы сделать кнопку "закрываемой"
     */
    closable: true,

    /**
     * @cfg {String} closeText
     * Текст подсказки на кнопке "Закрыть"
     */
    closeText: 'Закрыть',

    /**
     * Дочерний элемент кнопки, в котором будет элемент закрытия
     * @type {Array}
     */
    childEls: [
        'closeEl'
    ],

    getTemplateArgs: function() {
        var result = this.callParent();

        return Ext.apply(result, {
        	closable: this.closable,
        	closeText: this.closeText
        });
    },

    beforeRender: function() {
        
        this.callParent();
        
        this.addClsWithUI(this.baseCls);
        this.syncClosableUI();
    },

    // @private
    onClick: function(event, target, eOpts) {

    	if(target.id === this.closeEl.id) {
    		if(Ext.isFunction(this.closeHandler)) {
    			Ext.callback(this.closeHandler, this.scope || this);
    		}

	    	event.preventDefault();
    	} else {

	    	this.callParent(arguments);
    	}
    },

    onRender: function() {

        this.callParent(arguments);
		this.closeEl.addClsOnOver(this.baseCls + '-close-btn-over');
    },

    // inherit docs
    enable : function(silent) {

        this.callParent(arguments);

        this.removeClsWithUI(this.baseCls + '-disabled');

        return this;
    },

    // inherit docs
    disable : function(silent) {

        this.callParent(arguments);

        this.addClsWithUI(this.baseCls + '-disabled');

        return this;
    },

    /**
     * This method ensures that the closeBtn element exists or not based on 'closable'.
     * @private
     */
    syncClosableElements: function () {
        var closeEl = this.closeEl;

        if (this.closable) {
            if (!closeEl) {
                closeEl = this.closeEl = this.btnWrap.insertSibling({
                    tag: 'a',
                    cls: this.baseCls + '-close-btn',
                    href: '#',
                    title: this.closeText
                }, 'after');
            }
            closeEl.addClsOnOver(this.baseCls + '-close-btn-over');
        } else if (closeEl) {
            closeEl.remove();
            delete this.closeEl;
        }
    },

    /**
     * This method ensures that the UI classes are added or removed based on 'closable'.
     * @private
     */
    syncClosableUI: function () {
        var classes = [
	        	this.closableCls,
	        	this.closableCls + '-' + this.baseCls
        	];

        if (this.closable) {
            this.addClsWithUI(classes);
        } else {
            this.removeClsWithUI(classes);
        }
    }
});