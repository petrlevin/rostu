/**
 * Copyright(c) 2011
 *
 * Licensed under the terms of the Open Source LGPL 3.0
 * http://www.gnu.org/licenses/lgpl.html
 * @author Greivin Britton, brittongr@gmail.com
 */
Ext.define('App.ux.NumericField', {

    extend: 'App.components.fields.Number',
        
    constructor: function (config) {
        var defaultConfig =
        {
            decimalPrecision: 2,
            alwaysDisplayDecimals: false,
            currencySymbol: '$',
            useThousandSeparator: true,
            style: 'text-align:right;'
        };

        App.ux.NumericField.superclass.constructor.call(this, Ext.apply(defaultConfig, config));

        this.updateNumberFormat();

        //this.onFocus = this.onFocus.createSequence(this.onFocus);
    },
    
    onBlur: function () {
        this.setValue( this.value );
    },

    currencySymbol: null,
    numberFormat: '',
    /**
     * I don't want to build the format every time that getFormattedValue is called
     * just once when the object is created and then every time that decimalPrecision or decimalSeparator is changed.
     */
    updateNumberFormat: function () {
        var defaultDecimalPrecision = '';
        var format = '';

        if (this.allowDecimals && this.decimalPrecision > 0)
            while (defaultDecimalPrecision.length < this.decimalPrecision)
                defaultDecimalPrecision += '0';

        //If useThousandSeparator = true then format must be '0,0' else '0'
        this.thousandSeparator = (this.decimalSeparator == '.' ? ',' : '.');
        this.numberFormat = (this.useThousandSeparator ? '0' + (this.thousandSeparator) : '') + '0';

        //Add the decimal precision to the format
        this.numberFormat += (this.allowDecimals && this.decimalPrecision > 0 ? this.decimalSeparator + defaultDecimalPrecision : '') + (this.decimalSeparator == '.' ? '' : '/i');
    },
    setDecimalPrecision: function (v) {
        this.decimalPrecision = v;
        this.updateNumberFormat();
    },
    setDecimalSeparator: function (v) {
        this.decimalSeparator = v;
        this.updateNumberFormat();
    },
    setValue: function (v) {
        App.ux.NumericField.superclass.setValue.call(this, v);

        this.setRawValue(this.getFormattedValue(this.getValue()));
    },
    getFormattedValue: function (v) {
        if (Ext.isEmpty(v))
            return '';
        else {
            var prefix = '';

            prefix = (v < 0 && !Ext.isEmpty(this.currencySymbol) ? '-' : '') + (!Ext.isEmpty(this.currencySymbol) ? this.currencySymbol + ' ' : '');
            v = v < 0 && !Ext.isEmpty(this.currencySymbol) ? v * -1 : v;

            //If decimals are always visible then use the format.
            if (this.alwaysDisplayDecimals){
                v = Ext.util.Format.number(v, prefix + this.numberFormat);
                if (this.useThousandSeparator && this.alternativeThouthandSeparator) {
                    if (this.decimalSeparator == '.')
                        v = v.replace(/\,/g, this.alternativeThouthandSeparator);
                    else
                        v = v.replace(/\./g, this.alternativeThouthandSeparator);
                }
                

                return v;
            }
            else {
                //Format the number manually
                v = String(v);

                if (this.useThousandSeparator) {
                    var ps = v.split('.');
                    ps[1] = ps[1] ? ps[1] : null;

                    var whole = ps[0];

                    var r = /(\d+)(\d{3})/;

                    var thousandSeparator = this.useThousandSeparator ? (this.decimalSeparator == '.' ? ',' : '.') : '';

                    while (r.test(whole))
                        whole = whole.replace(r, '$1' + thousandSeparator + '$2');

                    v = whole + (ps[1] ? this.decimalSeparator + ps[1] : '');
                }

                return prefix + v;
            }
        }
    },
    /**
     * Se sobreEscribe el parseValue para retornar el valor sin el simbolo de la moneda
     */
    parseValue: function (v) {
        //Replace the currency symbol
        return App.ux.NumericField.superclass.parseValue.call(this, this.removeFormat(v));
    },
    /**
     * Remove only the format added by this class to let the superclass validate with it's rules.
     * @param {Object} v
     */
    removeFormat: function (v) {
        if (Ext.isEmpty(v))
            return v;
        else {
            v = v.toString();
            v = v.replace(this.currencySymbol, '').replace(/ /g, '');

            if (this.decimalSeparator == '.')
                v = v.replace(/\,/g, '');
            else
                v = v.replace(/\./g, '');

            return v;
        }
    },
    /**
     * Remove the format before validating the the value.
     * @param {Object} v
     */
    getErrors: function (v) {
        return App.ux.NumericField.superclass.getErrors.call(this, this.removeFormat(v));
    },
    /**
     * Display the numeric value with the fixed decimal precision and without the format using the setRawValue, don't need to do a setValue because we don't want a double
     * formatting and process of the value because beforeBlur perform a getRawValue and then a setValue.
     */
    onFocus: function () {
        if (!this.allowDecimals || this.decimalSeparator == '.')
            this.setRawValue(this.getValue());
        else
            this.setRawValue(this.removeFormat(this.getRawValue()));
    },
    setCurrencySymbol: function (v) {
        this.currencySymbol = v;

        this.setRawValue(this.getFormattedValue(this.getValue()));
    }
});
