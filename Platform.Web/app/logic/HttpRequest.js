/**
 * см. http://examples.extjs.eu/ Form / Downloading files
 */
Ext.define('App.logic.HttpRequest', {
	
	frameName: 'iframe',
	formName: 'hidden_form',

    /**
     * @cfg
     * url, по котоорму будет сделан post-запрос
     */
	url: null,

    /**
     * @cfg {Object} 
     * Параметры, которые будут переданы при POST-запросе
     */
	params: null,
	
    /**
     * @cfg method {Object} 
     * Метод Http запроса (GET, POST)
     */

	constructor: function(config) {
		Ext.apply(this, config);
	},

	/**
	 * Скрытый фрейм. 
	 */
	getFrame: function () {
		var body = Ext.getBody();
		var iframe = Ext.get(this.frameName);
		
		if (!iframe) {
			iframe = body.createChild({
				tag: 'iframe',
				cls: 'x-hidden',
				id: this.frameName,
				name: this.frameName
			});
		}
		return iframe;
	},

	getForm: function () {
		this.getFrame();
		var body = Ext.getBody();
		var form = Ext.get(this.formName);
		
		if (form) {
			form.remove();
		}
		
		form = body.createChild({
			tag: 'form',
			cls: 'x-hidden',
			method: this.method || 'post',
			id: this.formName,
			action: this.url,
			target: this.target || this.frameName
		});

	    Ext.Object.each(this.params, function(name, value) {
	        form.createChild({
	            tag: 'input',
	            type: 'hidden',
	            name: name,
	            value: value
	        });
	    }, this);

		return form;
	},

	submit: function () {
		var form = this.getForm();
		form.dom.submit();
	}
});