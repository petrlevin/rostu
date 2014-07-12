/**
* @file
*
* @brief General tests to check if ExtJS framework loaded, Jasmine framework used
* @details Copyright (c) 2006-2013 R.O.S.T.U.
*
* @author Pavel Dovgalenko (pavel.dovgalenko@gmail.com)
*
* @since $Id:$
*/

describe("Test", function() {
  it("ExtJS is defined in global scope", function() {
    expect(Ext).toBeDefined();
  });

  it("ExtJS version is defined", function() {
    expect(Ext.getVersion).toBeDefined();
    expect(typeof(Ext.getVersion)).toEqual('function');
  });
});

